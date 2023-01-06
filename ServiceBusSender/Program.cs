using BLL;
using CommandLine;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ServiceBusSender
{
    internal class Program
    {
        private static CancellationTokenSource cancelToken = new CancellationTokenSource();

        private const int serviceBusBatchSize = 50;

        static async Task Main(string[] args)
        {
            Console.CancelKeyPress += (object? sender, ConsoleCancelEventArgs e) =>
            {
                e.Cancel = true;
                cancelToken.Cancel();
            };

            (await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(RunTest).ConfigureAwait(false))
                .WithNotParsed(errors =>
                {
                    var jsonString = JsonConvert.SerializeObject(
                        args, Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });
                    Console.WriteLine(jsonString);
                    foreach (Error error in errors)
                    {
                        Console.WriteLine(error);
                    }
                });
        }

        private static async Task RunTest(Options options)
        {
            TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
            configuration.InstrumentationKey = options.ApplicationInsightsKey;
            TelemetryClient telemetryClient = new TelemetryClient(configuration);

            try
            {
                Console.WriteLine($"Sending {options.MessageCount} messages");
                Sender sender = new Sender(options.ServiceBusConnection!, options.Test1Topic1Name!);
                Task[] tasks = new Task[options.MessageCount * 2];
                for (int i = 0; i < options.MessageCount; i++)
                {
                    Guid messageId = Guid.NewGuid();
                    string message1 = $"1_{messageId}";
                    string message2 = $"2_{messageId}";
                    tasks[i * 2] = sender.SendTest1Topic1Sub1(message1)
                    .ContinueWith(t =>
                    {
                        Console.WriteLine($"Sent Message ID {message1} to subscription 1");
                        return t;
                    });
                    tasks[1 + i * 2] = sender.SendTest1Topic1Sub1(message2)
                    .ContinueWith(t =>
                    {
                        Console.WriteLine($"Sent Message ID {message2} to subscription 2");
                        return t;
                    });
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.StackTrace?.ToString());
                telemetryClient.TrackException(ex, properties: new Dictionary<string, string>() { { "Message", "Failed to send batch" } });

                throw;
            }
        }
    }
}
