using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using Services;

namespace MyRabbitMQConsumer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddHostedService<MyRabbitMQConsumerHostedService>();
            //services.AddHostedService<MyDirectConsumerHostedService>();
           
            //services.AddDbContext<MyDbContext>(builder =>
            //{
            //    builder.UseMySql("server=localhost;uid=root;pwd=root;database=shop;");
            //});

            services.AddDbContextPool<MyDbContext>(builder=> 
            {
                builder.UseMySql("server=localhost;uid=root;pwd=root;database=shop;");
            });

            services.AddScoped<ISaleService, SaleService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IOrderService, OrderService>();


            var lockFactory = RedLockFactory.Create(new List<RedLockEndPoint>()
            {
                new DnsEndPoint("127.0.0.1",6379)
            });
            services.AddSingleton(typeof(IDistributedLockFactory), lockFactory);

            services.AddHostedService<MyOrderHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
