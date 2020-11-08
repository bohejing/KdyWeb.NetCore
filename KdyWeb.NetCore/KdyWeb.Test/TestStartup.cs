using System.Net.Http;
using AutoMapper;
using KdyWeb.BaseInterface;
using KdyWeb.BaseInterface.BaseModel;
using KdyWeb.BaseInterface.Extensions;
using KdyWeb.BaseInterface.Repository;
using KdyWeb.Dto;
using KdyWeb.EntityFramework;
using KdyWeb.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace KdyWeb.Test
{
    public class TestStartup
    {
        public TestStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //����Զ���α���
            services.AddControllersWithViews(options =>
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

            //services.AddDbContextPool<KdyContext>(options =>
            //{
            //    var connectionStr = Configuration.GetConnectionString("WeChatDb");
            //    options.UseSqlServer(connectionStr);
            //});
            //todo: ����ע��˹�ϵ ����ִ�DbContext�ſ���ʹ��
            //services.AddScoped<DbContext, KdyContext>();
            //services.AddScoped<IRwContextFactory, RwContextFactory>();
            //services.AddScoped<IUnitOfWork, UnitOfWork>();
            //services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.KdyRegister();

            //ע��ͨ�÷��Ͳִ�
            services.TryAdd(ServiceDescriptor.Scoped(typeof(IKdyRepository<>), typeof(CommonRepository<>)));
            services.TryAdd(ServiceDescriptor.Scoped(typeof(IKdyRepository<,>), typeof(CommonRepository<,>)));

            //AutoMapperע��
            //https://www.codementor.io/zedotech/how-to-using-automapper-on-asp-net-core-3-0-via-dependencyinjection-zq497lzsq
            //services.AddAutoMapper(typeof(KdyMapperInit));
            var dtoAssembly = typeof(KdyMapperInit).Assembly;
            var entityAssembly = typeof(BaseEntity<>).Assembly;
            services.AddAutoMapper(dtoAssembly, entityAssembly);

            //ע��HttpClient
            services.AddHttpClient(KdyBaseConst.HttpClientName)
                .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
                {
                    //ȡ���Զ���ת
                    AllowAutoRedirect = false,
                    //���Զ�����cookie
                    // UseCookies = false
                });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.HttpOnly = HttpOnlyPolicy.Always;
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
            });

            //��ʼ�����������
            services.InitHangFire(Configuration)
                .InitIdGenerate(Configuration)
                .UseRedisCache(Configuration)
                .AddMemoryCache();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }
    }
}
