using Azure.Messaging.ServiceBus;
using BO.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;

namespace BLL
{
    public class Sender
    {
        private readonly ILogger<Sender> _logger;
        protected readonly ServiceBusClient _client;
        protected Lazy<ServiceBusSender> _senderTest1Topic1;



        public Sender(ILogger<Sender> logger, IOptions<AzureTestHarnessOptions> options)
        {
            _logger = logger;
            _logger.LogInformation("Initializing {className}", nameof(Sender));
            try
            {
                _client = new ServiceBusClient(options.Value.ServiceBus?.ConnectionString ?? throw new InvalidOperationException("ConnectionString is required"));
                _senderTest1Topic1 = new Lazy<ServiceBusSender>(() => _client.CreateSender(options.Value.ServiceBus.Test1Topic1Name));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize {className} with options {@options}", nameof(Sender), options.Value);
                throw;
            }
        }

        public Sender(string busConnection, string test1TopicName)
        {
            _logger = Mock.Of<ILogger<Sender>>();
            _client = new ServiceBusClient(busConnection);
            _senderTest1Topic1 = new Lazy<ServiceBusSender>(() => _client.CreateSender(test1TopicName));

        }

        public Task SendTest1Topic1Sub1(string messageId)
        {
            return SendTest1Topic1(messageId, 1);
        }

        public Task SendTest1Topic1Sub2(string messageId)
        {
            return SendTest1Topic1(messageId, 2);
        }
        private async Task SendTest1Topic1(string messageId, int sub)
        {
            try
            {
                ServiceBusMessage message = new ServiceBusMessage(JsonConvert.SerializeObject(messageId));
                message.MessageId = messageId.ToString();
                message.ContentType = "application/json";
                message.ApplicationProperties.Add("sub", sub);
                await _senderTest1Topic1.Value.SendMessageAsync(message).ConfigureAwait(false);
                _logger.LogInformation("Topic 1, Subscription {sub}: Created message with message id {messageId}", sub, messageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in {className}.{methodName} sending message ID {messageId} to subscription {sub}. Message Sent: {messageSent}", nameof(Sender), nameof(SendTest1Topic1), messageId, sub, messageId);
                throw;
            }
        }
    }

}
