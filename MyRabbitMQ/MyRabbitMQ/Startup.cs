using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyRabbitMQ.Services;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using Services;

namespace MyRabbitMQ
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddHostedService<MyRabbitMQConsumerHostService>();
            
            services.AddDbContext<MyDbContext>(builder=> 
            {
                builder.UseMySql("server=localhost;uid=root;pwd=root;database=shop;");
            });
            services.AddScoped<ISaleService, SaleService>();
            services.AddScoped<IProductService,ProductService>();
            services.AddScoped<IOrderService, OrderService>();


            var lockFactory = RedLockFactory.Create(new List<RedLockEndPoint>()
            {
                new DnsEndPoint("127.0.0.1",6379)
            });
            services.AddSingleton(typeof(IDistributedLockFactory),lockFactory);

            services.AddSingleton<RabbitMQConnectionService>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(name:"Default",
                    pattern:"api/{Controller}/{action}");
            });
        }
    }
}
