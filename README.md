# **RabbitMQ** 使用示例 (持续更新)
## **RabbitMQ** 是什么
  > 参考博客园晓晨Master大佬的介绍 https://www.cnblogs.com/stulzq/p/7551819.html >

  RabbitMQ 是一种实现了 AMQP，即Advanced Message Queuing Protocol，高级消息队列协议 的高性能消息队列中间件,服务器端用Erlang语言编写，支持多种客户端，如：Python、Ruby、.NET、Java、JMS、C、PHP、ActionScript、XMPP、STOMP等，支持AJAX。用于在分布式系统中存储转发消息,RabbitMQ提供了可靠的消息机制、跟踪机制和灵活的消息路由，支持消息集群和分布式部署。适用于排队算法、秒杀活动、消息分发、异步处理、数据同步、处理耗时任务、CQRS等应用场景。
## 如何在 **NETCore** 中使用 **RabbitMQ**
1. 安装.NETCore开发环境（本文使用的是.NETCore 3.1）
2. 安装并配置RabbitMQ
3. Nuget中安装RabbitMQ.Client（本文使用的是5.1.2）

### 创建生产者

```csharp

            // 创建连接
            var conn = connectionFactory.CreateConnection();

            // 创建通道
            var channel = conn.CreateModel();

            // 声明一个队列
            channel.QueueDeclare("myQueue",false,false,false);

            // 定义发送内容
            string input = JsonConvert.SerializeObject(data);

            var sendBytes = Encoding.UTF8.GetBytes(input);

            channel.BasicPublish("", "myQueue", null, sendBytes);
            channel.Close();
            conn.Close();
```

### 创建消费者

```csharp
            ConnectionFactory connectionFactory = new ConnectionFactory() 
            {
                UserName = "guest",
                Password = "guest",
                HostName = "localhost"
            };

            _conn = connectionFactory.CreateConnection();

            _chanel = _conn.CreateModel();

            // 事件基本消费者
            var consumer = new EventingBasicConsumer(_chanel);

            consumer.Received +=  (ch, args) =>
            {
                var msg = Encoding.UTF8.GetString(args.Body);
                Console.WriteLine($"收到消息:{msg}");
                
                // 确认消息已被消费
                _chanel.BasicAck(args.DeliveryTag,false);

            };

            // 启动消费者 ，启用手动应答
            _chanel.BasicConsume("myQueue", false, consumer);

```

### 使用交换机（Exchange)绑定 队列（Queue）
