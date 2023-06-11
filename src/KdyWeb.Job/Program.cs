using System.Runtime.InteropServices;
using KdyWeb.BaseInterface.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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
                    string consulUrl = context.Configuration.GetValue<string>(ConsulConfigCenterExt.ConsulConfigUrl),
                        clientName = context.Configuration.GetValue<string>(ConsulConfigCenterExt.ConfigClientName),
                        consulToken = context.Configuration.GetValue<string>(ConsulConfigCenterExt.ConsulToken);
                    if (string.IsNullOrEmpty(clientName) == false)
                    {
                        clientName = "." + clientName;
                    }

                    config.InitConfigCenter(context, consulUrl,
                        consulToken,
                        $"{env.ApplicationName}/appsettings.{env.EnvironmentName}{clientName}.json");
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureExceptionLessLogging();
    }
}
