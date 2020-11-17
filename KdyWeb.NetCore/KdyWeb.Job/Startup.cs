using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using AutoMapper;
using Kdy.StandardJob;
using KdyWeb.BaseInterface;
using KdyWeb.BaseInterface.BaseModel;
using KdyWeb.BaseInterface.Extensions;
using KdyWeb.BaseInterface.Repository;
using KdyWeb.Dto;
using KdyWeb.EntityFramework;
using KdyWeb.Job.JobService;
using KdyWeb.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KdyWeb.Job
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
            services.TryAdd(ServiceDescriptor.Scoped(typeof(IKdyRepository<>), typeof(CommonRepository<>)));
            services.TryAdd(ServiceDescriptor.Scoped(typeof(IKdyRepository<,>), typeof(CommonRepository<,>)));

            #region �Զ�ע��ɰ�Job

            //���ص�ǰ��Ŀ����
            var assemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "Kdy*.dll").Select(Assembly.LoadFrom).ToArray();
            //���г�����������
            var allTypes = new List<System.Type>();
            foreach (var itemAssemblies in assemblies)
            {
                allTypes.AddRange(itemAssemblies.GetTypes());
            }

            //���õĽӿ�
            var baseType = typeof(IKdyJobFlag);
            //������Ҫ�õ��ķ��������ӿ�
            var useType = allTypes.Where(a => baseType.IsAssignableFrom(a) && a.IsAbstract == false).ToList();
            foreach (var item in useType)
            {
                //�÷��������ӿ�
                var currentInterface = item.GetInterfaces().FirstOrDefault(a => a.Name.EndsWith(item.Name));
                if (currentInterface == null)
                {
                    continue;
                }

                //ÿ�����󣬶���ȡһ���µ�ʵ����ͬһ�������ȡ��λ�õ���ͬ��ʵ��
                services.AddScoped(currentInterface, item);
            }
            #endregion

            //AutoMapperע��
            //https://www.codementor.io/zedotech/how-to-using-automapper-on-asp-net-core-3-0-via-dependencyinjection-zq497lzsq
            //services.AddAutoMapper(typeof(KdyMapperInit));
            var dtoAssembly = typeof(KdyMapperInit).Assembly;
            var entityAssembly = typeof(BaseEntity<>).Assembly;
            services.AddAutoMapper(dtoAssembly, entityAssembly);

            services.AddControllers(opt =>
                {
                    opt.Filters.Add<ModelStateValidFilter>();
                })
                .AddNewtonsoftJson(option =>
                {
                    option.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                });

            //ע��Hangfire
            services.InitHangFireServer(Configuration);

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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            //todo:!!!! �����ע��˳�� �÷ŵ�Routing��
            app.UseKdyLog();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

          
            app.InitDashboard();

            //ȫ��DI����
            KdyBaseServiceProvider.ServiceProvide = app.ApplicationServices;
            KdyBaseServiceProvider.HttpContextAccessor = app.ApplicationServices.GetService<IHttpContextAccessor>();
            app.InitExceptionLess(Configuration);
        }
    }
}
