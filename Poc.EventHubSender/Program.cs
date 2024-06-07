using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Poc.EventHubSender
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder().AddUserSecrets<Program>();
            var secretsConfig = builder.Build();

            string eventHubConnectionString = secretsConfig["EventHubConnectionString"];
            var sender = new EventHubProducerClient(eventHubConnectionString);
            while (true)
            {
                var eventBatch = await sender.CreateBatchAsync();
                eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())));
                await sender.SendAsync(eventBatch);
                await Task.Delay(5);
                Console.WriteLine($"Event sent.");
            }
        }
    }
}
