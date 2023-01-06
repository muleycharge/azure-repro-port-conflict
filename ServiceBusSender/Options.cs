using CommandLine;

namespace ServiceBusSender
{
    public class Options
    {
        [Option('m', "message-count", Default = 1, HelpText = "Number of test messages to add to the queue.")]
        public int MessageCount { get; set; }

        [Option('b', "service-bus-conn", HelpText = "Service Bus connection string", Required = true)]
        public string? ServiceBusConnection { get; set; }

        [Option("test1-topic-name", HelpText = "Where test messages are sent", Required = true)]
        public string? Test1Topic1Name { get; set; }

        [Option('i', "insights-key", HelpText = "Application Insights instrumentation key", Required = true)]
        public string? ApplicationInsightsKey { get; set; }
    }
}
