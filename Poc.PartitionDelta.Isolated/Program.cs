using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Poc.PartitionDelta.Isolated
{
    internal class Program
    {
        static void Main(string[] args)
        {
            FunctionsDebugger.Enable();

            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();

                    //⚠️ IMPORTANT:
                    //Register the EventHubConsumerClient as a singleton and reuse it across function invocations.
                    services.AddAzureClients(builder =>
                    {
                        builder.AddEventHubConsumerClient("$Default", Environment.GetEnvironmentVariable("EventHubConnectionString"));
                    });
                })
                .Build();

            host.Run();
        }
    }
}
