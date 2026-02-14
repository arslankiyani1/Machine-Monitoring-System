namespace MMS.Adapter.MQTT.Listener;

public class MqttBackgroundListener : BackgroundService
{
    private readonly EventProcessorClient _processor;
    private readonly IServiceProvider _services;
    private readonly ILogger<MqttBackgroundListener> _logger;

    public MqttBackgroundListener(
        IConfiguration config,
        IServiceProvider services,
        ILogger<MqttBackgroundListener> logger)
    {
        _services = services;
        _logger = logger;

        var blobStorageConnectionString = config["AzureIoTListener:BlobStorageConnectionString"]!;
        var blobContainerName = config["AzureIoTListener:BlobContainerName"]!;
        var eventHubConnectionString = config["AzureIoTListener:EventHubConnectionString"]!;
        var eventHubName = config["AzureIoTListener:EventHubName"]!;
        var consumerGroup = config["AzureIoTListener:ConsumerGroup"]!;

        var blobContainerClient = new BlobContainerClient(blobStorageConnectionString, blobContainerName);

        // Handle container already exists error gracefully
        try
        {
            blobContainerClient.CreateIfNotExists();
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 409)
        {
            // Container already exists - this is fine, just continue
            logger.LogInformation("Blob container '{ContainerName}' already exists.", blobContainerName);
        }

        _processor = new EventProcessorClient(blobContainerClient, consumerGroup, eventHubConnectionString, eventHubName);
        _processor.ProcessEventAsync += ProcessEventHandler;   // instance method
        _processor.ProcessErrorAsync += ProcessErrorHandler;   // instance method
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IoT Hub Background Listener started...");
        await _processor.StartProcessingAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
            await Task.Delay(1000, stoppingToken);

        _logger.LogInformation("Stopping processor...");
        await _processor.StopProcessingAsync();
    }

    private async Task ProcessEventHandler(ProcessEventArgs eventArgs)
    {
        try
        {
            if (eventArgs.Data is null) return;

            string message = Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray());
            var dto = JsonSerializer.Deserialize<MachineMonitoring>(message);
            _logger.LogInformation("Received message: {Message}", message);

            using var scope = _services.CreateScope();
            var monitoring = scope.ServiceProvider.GetRequiredService<IMachineMonitoringService>();

            if (dto is { })
            {
                await monitoring.ProcessMonitoringAsync(dto);
                await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event in partition {PartitionId}", eventArgs.Partition.PartitionId);
        }
    }

    private Task ProcessErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Error in partition {PartitionId}", args.PartitionId);
        return Task.CompletedTask;
    }
}