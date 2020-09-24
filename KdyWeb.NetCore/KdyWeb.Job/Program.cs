using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        public static void Main(string[] args)
        {
            //1��Windows����
            //Microsoft.Extensions.Hosting.WindowsServices ���ڸ��������worker services�İ�
            //2��Linux����
            // Microsoft.Extensions.Hosting.Systemd NuGet��
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                Directory.SetCurrentDirectory(pathToContentRoot);
            }

            var builder = CreateHostBuilder(
                args.Where(arg => arg != "--console").ToArray());
            var host = builder.Build();
            if (isService)
            {
                //�Է���ʽ
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
            }
            else
            {
                host.Run();
            }
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
                .ConfigureServices((context, service) =>
                {
                    //��̨���еķ���
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
