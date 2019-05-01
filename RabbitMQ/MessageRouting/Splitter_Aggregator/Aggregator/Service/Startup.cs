using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service.Configuration;
using Service.DAO;
using Service.Services.Base;

namespace Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<DelayOptions>(Configuration);
            services.AddOptions();
            services.AddTransient<IRabbitMQBase, RabbitMQBase>();
            services.AddSingleton<IOrderItemsDao, OrderItemsDao>();
            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, Services.Receiver.Receiver>();
            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, Services.Sender.Sender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
