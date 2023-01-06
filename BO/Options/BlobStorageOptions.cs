namespace BO.Options
{
    public class BlobStorageOptions
    {
        public const string BlobStorage = "BlobStorage";

        public string? ConnectionString { get; set; }

        public string? TestContainerName { get; set; }
    }
}
