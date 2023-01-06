using BO.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace ServiceBusProcessor
{
    public static class Program
    {
        /// <summary>
        /// config values
        /// </summary>
        public static async Task Main()
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                      .AddJsonFile("appsettings.json.user", optional: true, reloadOnChange: true)
                  .AddEnvironmentVariables();

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Trace);
            string nlogConfigFile = "nlog.config";
            builder.Logging.AddNLog(nlogConfigFile);


            builder.Services.Configure<AzureTestHarnessOptions>(builder.Configuration.GetSection(AzureTestHarnessOptions.AzureTestHarness));
            builder.Services.AddLogging();
            builder.Host.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
                b.AddServiceBus();
            });

            WebApplication app = builder.Build();

            await app.RunAsync().ConfigureAwait(false);
        }
    }
}
