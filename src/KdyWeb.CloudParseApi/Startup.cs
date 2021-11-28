using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using AutoMapper;
using KdyWeb.BaseInterface;
using KdyWeb.BaseInterface.BaseModel;
using KdyWeb.BaseInterface.Extensions;
using KdyWeb.BaseInterface.Filter;
using KdyWeb.BaseInterface.Repository;
using KdyWeb.Dto;
using KdyWeb.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;

namespace KdyWeb.CloudParseApi
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
            //�ر�ModelState�Զ�У��
            services.Configure<ApiBehaviorOptions>(option =>
            {
                option.SuppressModelStateInvalidFilter = true;
            });
            services.KdyRegister();

            //ע��ͨ�÷��Ͳִ�
            services.TryAdd(ServiceDescriptor.Transient(typeof(IKdyRepository<>), typeof(CommonRepository<>)));
            services.TryAdd(ServiceDescriptor.Transient(typeof(IKdyRepository<,>), typeof(CommonRepository<,>)));

            //AutoMapperע��
            //https://www.codementor.io/zedotech/how-to-using-automapper-on-asp-net-core-3-0-via-dependencyinjection-zq497lzsq
            //services.AddAutoMapper(typeof(KdyMapperInit));
            var dtoAssembly = typeof(KdyMapperInit).Assembly;
            var entityAssembly = typeof(BaseEntity<>).Assembly;
            services.AddAutoMapper(dtoAssembly, entityAssembly);

            services.AddControllers(opt =>
            {
                opt.Filters.Add<ModelStateValidFilter>();
            }).AddNewtonsoftJson(option =>
            {
                option.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            });

            //ע��HttpClient
            services.AddHttpClient(KdyBaseConst.HttpClientName)
                .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
                {
                    //ȡ���Զ���ת
                    AllowAutoRedirect = false,
                });

            //��ʼ�����������
            services.InitIdGenerate(Configuration)
                .UseRedisCache(Configuration)
                .AddMemoryCache();

            //Swagger
            services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "���̽���Api",
                    Version = "v1"
                });

                var xmlPath = AppDomain.CurrentDomain.BaseDirectory;
                var filePath = Directory.GetFiles(xmlPath, "KdyWeb.*.xml");
                foreach (var item in filePath)
                {
                    option.IncludeXmlComments(item, true);
                }

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthorization();

            app.UseKdyLog();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //ȫ��DI����
            KdyBaseServiceProvider.ServiceProvide = app.ApplicationServices;
            KdyBaseServiceProvider.HttpContextAccessor = app.ApplicationServices.GetService<IHttpContextAccessor>();

            if (env.IsDevelopment())
            {
                //swagger
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                    // c.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
                });
            }
        }
    }
}
