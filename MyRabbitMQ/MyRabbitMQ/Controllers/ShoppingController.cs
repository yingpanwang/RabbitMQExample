using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataProvider.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MyRabbitMQ.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RedLockNet;
using Services;
using StackExchange.Redis;

namespace MyRabbitMQ.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingController : ControllerBase
    {
        static readonly string SalePrefix = "SALES_";
        readonly IDistributedLockFactory _lockFactory;
        readonly IProductService _productService;
        readonly IOrderService _orderService;
         IDatabase _redisDb;
        readonly ISaleService _saleService;
        static bool IsInit = false;
       
        public ShoppingController(
            IDistributedLockFactory lockFactory,
            IProductService service,
            IOrderService orderService,
            ISaleService saleService)
        {
            _lockFactory = lockFactory;
            _productService = service;
            _orderService = orderService;
            _saleService = saleService;
            _redisDb = ConnectionMultiplexer.Connect("127.0.0.1:6379").GetDatabase(1);
        }

        [HttpGet("buy")]
        public async Task<IActionResult> Buy(int pId,CancellationToken cancellationToken)
        {
            cancellationToken.Register(()=> 
            {
                Console.WriteLine("取消了购买!");
            });

            /**
             * 获取redis锁
             * 
             * resource 资源
             * expiryTime 过期时间
             * waitTime 获取锁等待时间
             * retryTime 重试间隔时间
             * **/
            using (var redLock = await _lockFactory.CreateLockAsync(pId.ToString(), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(50), cancellationToken))
            {
                if (redLock.IsAcquired)
                {
                    bool buyResult = await _productService.Buy(pId);
                    return new JsonResult(new BuyResult { Success = buyResult, Msg = buyResult ? "购买成功!" : "购买失败！" });
                }
                else
                {
                    return new JsonResult(new BuyResult { Success = false, Msg = "系统繁忙!" });
                }
            }
        }

        [HttpGet("buyasync")]
        public async Task<IActionResult> BuyAsync(int pId,int sId, CancellationToken cancellationToken)
        {
            // 初始化抢购信息
                if (!IsInit)
                {
                    var sales = (await _saleService.GetSales()).ToList();

                    for (int i = 0; i < sales.Count(); i++)
                    {
                        _redisDb.StringSet($"{SalePrefix}{sales[i].PId}", sales[i].SaleCount);
                    }
                    IsInit = true;
                }
            
            string userId = Guid.NewGuid().ToString("N");
            int stock = Convert.ToInt32(_redisDb.StringGet($"{SalePrefix}{pId}"));
            if (stock <= 0) 
            {
                return CreateResult(false,"已售完！");
            }

            DataProvider.Entities.Order order = await _orderService.FindUserSaleOrderAsync(userId, pId,sId);
            if (order != null) 
            {
                return CreateResult(false, "请勿重复抢购!");
            }

            RabbitMQConnectionService rabbitMQConnectionService = new RabbitMQConnectionService();
            order = new DataProvider.Entities.Order()
            {
                CreationTime = DateTime.Now,
                PId = pId,
                UserId = userId,
                SId = sId
            };

            using (var conn = rabbitMQConnectionService.GetConnection()) 
            {
                // 创建通道
                using var channel = conn.CreateModel();
                string exchangeName = "order";
                string routeKey = "sendOrder";

                ///
                //channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, false, false);
                ////声明一个队列
                //channel.QueueDeclare("saleQueue", false, false, false);

                //channel.QueueBind("saleQueue", exchangeName, routeKey, null);

                // 定义发送内容
                string input = JsonConvert.SerializeObject(order);

                var sendBytes = Encoding.UTF8.GetBytes(input);

                channel.BasicPublish(exchangeName, routeKey, null, sendBytes);
                
            }

            return CreateResult(true, "排队中...");
        }
        

        private IActionResult CreateResult(bool isSuccessed,string msg = "") 
        {
            if (isSuccessed)
            {
                return new JsonResult(new BuyResult { Success = isSuccessed, Msg = msg });
            }
            else 
            {
                return new JsonResult(new BuyResult { Success = isSuccessed, Msg = msg });
            }
        }
    }

    public class BuyResult
    {
        public bool Success { get; set; }
        public string Msg { get; set; }
    }
}