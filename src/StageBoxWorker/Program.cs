using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace StageBoxWorker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var asService = !(Debugger.IsAttached || args.Contains("-- console"));

            var builder = new HostBuilder().ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<ExchangeRateWorker>();
            });
            builder.UseEnvironment(asService ? EnvironmentName.Production : EnvironmentName.Development);

            if (asService)
                await builder.RunAsServiceAsync();
            else
                await builder.RunConsoleAsync();
        }
    }
}
