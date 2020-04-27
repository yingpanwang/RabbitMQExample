using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RedLockNet;
using Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyRabbitMQConsumer
{
    public class MyOrderHostedService : IHostedService
    {
        private IConnection _conn;
        private IModel _chanel;
        private IDatabase _redisDb;

        static readonly string SalePrefix = "SALES_";
        readonly IProductService _productService;
        readonly IOrderService _orderService;
        readonly ISaleService _saleService;
        readonly IDistributedLockFactory _lockFactory;

        public MyOrderHostedService(IServiceProvider serviceProvider)
        {
            var scop = serviceProvider.CreateScope();
            _productService = scop.ServiceProvider.GetService<IProductService>();
            _orderService = scop.ServiceProvider.GetService<IOrderService>();
            _saleService = scop.ServiceProvider.GetService<ISaleService>();
            _lockFactory = scop.ServiceProvider.GetService<IDistributedLockFactory>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            InitRedisDb();

            ConnectionFactory connectionFactory = new ConnectionFactory()
            {
                UserName = "guest",
                Password = "guest",
                HostName = "localhost"
            };

            _conn = connectionFactory.CreateConnection();

            _chanel = _conn.CreateModel();
            string exchangeName = "order";
            string routeKey = "sendOrder";
            _chanel.ExchangeDeclare(exchangeName, ExchangeType.Direct, false, false);
            //声明一个队列
            _chanel.QueueDeclare("saleQueue", false, false, false);

            _chanel.QueueBind("saleQueue", exchangeName, routeKey, null);

            // 事件基本消费者
            var consumer = new EventingBasicConsumer(_chanel);

            consumer.Received += async (ch, args) =>
            {
                var msg = Encoding.UTF8.GetString(args.Body);
                var order = JsonConvert.DeserializeObject<DataProvider.Entities.Order>(msg);

                using (var redLock = await _lockFactory.CreateLockAsync(order.PId.ToString(), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(50), cancellationToken))
                {
                    if (redLock.IsAcquired)
                    {
                        bool createOrderSuccessed = await _orderService.CreateOrderAsync(order);
                        bool reduceSaleSuccessed = await _saleService.ReduceSaleAsync(order.SId.Value);
                        if (createOrderSuccessed && reduceSaleSuccessed)
                        {
                            long redisStock = _redisDb.StringDecrement($"{SalePrefix}{order.PId}");
                            Console.WriteLine("redis剩余库存:" + redisStock);
                        }
                        else
                        {
                            var sale = await _saleService.GetSale(order.SId.Value);
                            if (!sale.IsFinished)
                            {
                                long redisStock = _redisDb.StringIncrement($"{SalePrefix}{order.PId}");
                                Console.WriteLine("创建订单失败\n");
                                Console.WriteLine("返还redis库存:" + redisStock);
                            }
                            
                        }
                    }
                    else
                    {
                        
                    }
                }
                
                // 确认消息已被消费
                _chanel.BasicAck(args.DeliveryTag, false);

            };

            // 启动消费者 ，启用手动应答
            _chanel.BasicConsume("saleQueue", false, consumer);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void InitRedisDb()
        {
            if (_redisDb == null)
            {
                StackExchange.Redis.ConnectionMultiplexer
                    cm = ConnectionMultiplexer.Connect("127.0.0.1:6379");

                _redisDb = cm.GetDatabase(1);
            }
        }
    }
}
