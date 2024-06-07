using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Poc.PartitionDelta.Isolated
{
    public class EventHubPartitionDelta
    {
        private readonly ILogger<EventHubPartitionDelta> log;
        private readonly EventHubConsumerClient consumer;

        public EventHubPartitionDelta(ILogger<EventHubPartitionDelta> logger, EventHubConsumerClient consumerClient)
        {
            log = logger;
            //⚠️ IMPORTANT:
            //Don't instantiate this for every function invocation. This will lead to SNAT Port Exhaustion.
            //Instead, use the singleton instance provided by the DI container as demonstrated here.
            //See Program.cs for configuration of the client.
            consumer = consumerClient;
        }

        [Function(nameof(EventHubPartitionDelta))]
        public async Task RunAsync([EventHubTrigger("events", Connection = "EventHubConnectionString")] EventData[] events, JsonElement partitionContext)
        {
            log.LogInformation($"Events Received: {events.Length}");
            //partitionContext.ReadLastEnqueuedEventProperties() does not return values as the consumer is not set up to track the last enqueued event properties.
            var partitionId = partitionContext.GetProperty("PartitionId").GetString();
            var partitionInfo = await consumer.GetPartitionPropertiesAsync(partitionId);
            //log.LogInformation($"LastEnqueuedSequenceNumber: {partitionContext.ReadLastEnqueuedEventProperties().SequenceNumber}");
            foreach (var e in events)
            {
                log.LogInformation($"PartitionId: {partitionId}, SequenceNumber: {e.SequenceNumber}, Delta: {partitionInfo.LastEnqueuedSequenceNumber - e.SequenceNumber}");
                await Task.Delay(500);
            }
        }
    }
}
