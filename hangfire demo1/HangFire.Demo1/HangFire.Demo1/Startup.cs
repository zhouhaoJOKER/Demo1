using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Hangfire;
using Hangfire.SqlServer;
using HangFire.Demo1.Models.commom;
using HangFire.Demo1.Filters;
using Microsoft.OpenApi.Models;
using HangFire.Demo1.DBHelpers;

namespace HangFire.Demo1
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(this.configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                UsePageLocksOnDequeue = true,
                DisableGlobalLocks = true
            }));
            services.Configure<DefaultConnections>(configuration.GetSection("DbConnections"));
            services.Configure<RedisConnections>(configuration.GetSection("RedisConnections"));
            services.AddScoped<ITestDbManager,TestDbManager>();
            services.AddScoped<IOfficalDbManager, OfficalDbManager>();
            services.AddScoped<RedisHelper>();
            services.AddSingleton<ConfigUtil>();
            //添加actionfilterattribute添加注入 是为了方便注入了Ilogger ，因为我们的[TestApiActionFilterAttribute]构造函数中 需要logger
            services.AddScoped(typeof(TestApiActionFilterAttribute));
            services.AddDistributedMemoryCache();
            services.AddMemoryCache();
            services.AddHangfireServer();
            services.AddSingleton<DBHelper>();
            services.AddControllers(options=> {
                //添加actionfilterattribute 过滤器 全局注册
                options.Filters.Add(typeof(TestGlobalActionFilterAttribute));
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();
            app.UseHangfireDashboard();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = "";
            });

            app.UseRouting();
            
            app.UseEndpoints(endpoints =>
            {
                //这个可以不需要
                //endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name:"default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
