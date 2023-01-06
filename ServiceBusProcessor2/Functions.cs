using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ServiceBusProcessor
{
    public class Functions
    {
        private readonly ILogger<Functions> _logger;

        public Functions(ILogger<Functions> logger)
        {
            _logger = logger;
        }

        public async Task Test1Receive2([ServiceBusTrigger("%AzureTestHarness:ServiceBus:Test1Topic1Name%", "%AzureTestHarness:ServiceBus:Test1Subscription2Name%", Connection = "AzureTestHarness:ServiceBus:ConnectionString")] Guid messageId,
            ILogger logger)
        {
            try
            {
                // Simulate process task
                await Task.Delay(300);
            }
            catch (Exception e)
            {
                logger.LogError($"Failed to process message {e.Message}");
                _logger.LogError(e, "Failed to process message ID {messageId}", messageId);
            }

            logger.LogInformation($"Completed processing message ID {messageId}");
        }
    }
}
