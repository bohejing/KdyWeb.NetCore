using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Exceptionless;
using KdyWeb.BaseInterface;
using KdyWeb.BaseInterface.Extensions;
using KdyWeb.Job.JobService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KdyWeb.Job
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //https://docs.microsoft.com/zh-cn/aspnet/core/host-and-deploy/windows-service?view=aspnetcore-3.1&tabs=visual-studio
            //1、Windows部署
            //Microsoft.Extensions.Hosting.WindowsServices 用于给程序添加worker services的包
            //2、Linux部署
            // Microsoft.Extensions.Hosting.Systemd NuGet包

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

            await builder.RunConsoleAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    //环境变量
                    var env = context.HostingEnvironment;
                    context.Configuration = config.Build();
                    string consulUrl = context.Configuration[ConsulConfigCenterExt.ConsulConfigUrl];
                    config.InitConfigCenter(context, consulUrl,
                        $"{env.ApplicationName}/appsettings.{env.EnvironmentName}.json");
                })
                .ConfigureServices((context, service) =>
                {
                    //后台运行的服务
                    service.AddHostedService<TestJobService>();
                })
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
