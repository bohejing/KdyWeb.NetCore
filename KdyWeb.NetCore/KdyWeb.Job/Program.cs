using System.Runtime.InteropServices;
using Exceptionless;
using KdyWeb.BaseInterface.Extensions;
using KdyWeb.Job.JobService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KdyWeb.Job
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //https://docs.microsoft.com/zh-cn/aspnet/core/host-and-deploy/windows-service?view=aspnetcore-3.1&tabs=visual-studio
            //1��Windows����
            //Microsoft.Extensions.Hosting.WindowsServices ���ڸ��������worker services�İ�
            //2��Linux����
            // Microsoft.Extensions.Hosting.Systemd NuGet��

            var builder = CreateHostBuilder(args);
            var isWin = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (isWin == false)
            {
                //Linux
                builder.UseSystemd();
            }
            else
            {
                //Windows
                builder.UseWindowsService();
            }

            var host = builder.Build();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    //��������
                    var env = context.HostingEnvironment;
                    context.Configuration = config.Build();
                    string consulUrl = context.Configuration[ConsulConfigCenterExt.ConsulConfigUrl];
                    config.InitConfigCenter(context, consulUrl,
                        $"{env.ApplicationName}/appsettings.{env.EnvironmentName}.json");
                })
                //.ConfigureServices((context, service) =>
                //{
                //})
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).
                ConfigureLogging(config =>
                {
                    config.AddExceptionless();
                });
    }
}
