using System;
using KdyWeb.BaseInterface;
using KdyWeb.BaseInterface.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace KdyWeb.NetCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                 .ConfigureAppConfiguration((hostingContext, config) =>
                 {
                     //��������
                     var env = hostingContext.HostingEnvironment;
                     hostingContext.Configuration = config.Build();
                     var consulUrl = hostingContext.Configuration.GetValue<string>(ConsulConfigCenterExt.ConsulConfigUrl);
                     var consulToken = hostingContext.Configuration.GetValue<string>(ConsulConfigCenterExt.ConsulToken);

                     config.InitConfigCenter(hostingContext, consulUrl,
                         consulToken,
                         $"{env.ApplicationName}/appsettings.{env.EnvironmentName}.json");
                 })
                 .ConfigureWebHostDefaults(webBuilder =>
                 {
                     webBuilder.UseStartup<Startup>();
                 }).ConfigureExceptionLessLogging();
        }

    }
}
