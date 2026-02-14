namespace MMS.Adapters.AzureStorage;

public class BlobStorageOptions
{
    public string ConnectionString { get; set; } = null!;
    public string ContainerName { get; set; } = null!;
}