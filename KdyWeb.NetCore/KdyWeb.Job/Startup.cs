using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kdy.StandardJob;
using Kdy.StandardJob.JobService;
using KdyWeb.BaseInterface;
using KdyWeb.BaseInterface.Extensions;
using KdyWeb.BaseInterface.InterfaceFlag;
using KdyWeb.BaseInterface.KdyLog;
using KdyWeb.Service.Job;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            //���ص�ǰ��Ŀ����
            var assemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "Kdy*.dll").Select(Assembly.LoadFrom).ToArray();

            //���г�����������
            var allTypes = new List<System.Type>();
            foreach (var itemAssemblies in assemblies)
            {
                allTypes.AddRange(itemAssemblies.GetTypes());
            }

            #region �Զ�ע��Scoped
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

            //ע��ExceptionLess��־
            services.AddHttpContextAccessor()
                .AddSingleton<IKdyLog, KdyLogForExceptionLess>();
            services.AddControllers();
            services.InitHangFireServer(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.InitDashboard();
            app.InitExceptionLess(Configuration);

        }
    }
}
