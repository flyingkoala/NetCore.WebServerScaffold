using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using WebServer.Filter;

namespace WebServer
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

            //注册Swagger生成器，定义一个和多个Swagger 文档
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", GetSwaggerInfo());
                // 为 Swagger JSON and UI设置xml文档注释路径
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);//获取应用程序所在目录（绝对，不受工作目录影响）
                var xmlPath = Path.Combine(basePath, "webserver.xml");
                c.IncludeXmlComments(xmlPath);
                //控制器标签添加注释
                c.DocumentFilter<ApplyTagDescriptions>();
                //添加accesstoken验证
                c.OperationFilter<AssignOperationVendorExtensions>();

            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //启用中间件服务生成Swagger作为JSON终结点
            app.UseSwagger();
            //启用中间件服务对swagger-ui，指定Swagger JSON终结点
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebServer API V1");
                c.RoutePrefix = string.Empty;
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        /// <summary>
        /// swagger配置
        /// </summary>
        /// <returns></returns>
        private Info GetSwaggerInfo()
        {
            var info = new Info()
            {
                Version = "v1.0.0",
                Title = "WebServer API",
                Contact = new Contact
                {
                    Name = "Weikang",
                    Email = "kangwei19@live.com",
                    Url = "https://github.com/flyingkoala/NetCore.WebServerScaffold"
                },
                Description = "A simple example ASP.NET Core WebServer"
            };
            return info;
        }
    }
}
