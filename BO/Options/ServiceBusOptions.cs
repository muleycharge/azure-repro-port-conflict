namespace BO.Options
{
    public class ServiceBusOptions
    {
        public const string ServiceBus = "ServiceBus";

        public string? ConnectionString { get; set; }

        public string? Test1Topic1Name { get; set; }

        public string? Test1Topic2Name { get; set; }

        public string? Test1SubscriptionName { get; set; }

        public string? Test2TopicName { get; set; }

        public string? Test2Subscription1Name { get; set; }

        public string? Test2Subscription2Name { get; set; }
    }
}
