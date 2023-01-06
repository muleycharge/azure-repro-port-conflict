namespace BO.Options
{
    public class AzureTestHarnessOptions
    {
        public const string AzureTestHarness = "AzureTestHarness";

        public string? ApplicationInsightsKey { get; set; }

        public BlobStorageOptions? BlobStorage { get; set; }

        public ServiceBusOptions? ServiceBus { get; set; }
    }
}
