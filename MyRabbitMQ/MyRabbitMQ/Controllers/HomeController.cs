using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace MyRabbitMQ.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() 
        {
            return new JsonResult(new { Message = "Get Info Successed" });
        }

        [HttpPost("msg")]
        public IActionResult Post([FromBody]MQMsg data)
        {
            // 创建连接工厂
            ConnectionFactory connectionFactory = new ConnectionFactory() 
            {
                UserName = "guest",
                Password = "guest",
                HostName = "localhost"
            };

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
            return new JsonResult(new { Message = "Post Info Successed" });
        }

        [HttpPost("directmsg")]
        public IActionResult PostDirectMsg([FromBody]MQMsg data)
        {
            // 创建连接工厂
            ConnectionFactory connectionFactory = new ConnectionFactory()
            {
                UserName = "guest",
                Password = "guest",
                HostName = "localhost"
            };

            // 创建连接
            var conn = connectionFactory.CreateConnection();

            // 创建通道
            var channel = conn.CreateModel();
            string exchangeName = "myDirectExchange";
            string routeKey = "test";

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, false, false);
            // 声明一个队列
            channel.QueueDeclare("myQueue", false, false, false);
            channel.QueueBind("", exchangeName, routeKey, null);

            // 定义发送内容
            string input = JsonConvert.SerializeObject(data);

            var sendBytes = Encoding.UTF8.GetBytes(input);

            channel.BasicPublish(exchangeName, routeKey, null, sendBytes);
            channel.Close();
            conn.Close();
            return new JsonResult(new { Message = "Post Info Successed" });
        }

    }

    public class MQMsg 
    {
        public string Name { get; set; }
        public string Desc { get; set; }
    }
}