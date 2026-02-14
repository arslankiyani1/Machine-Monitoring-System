using MMS.Application.Interfaces;

namespace MMS.Adapter.MQTT.Publisher;

// kind of postman thing (publisher to MQTT broker) like we do using postman  its a programatically publisher
// we need this in future implementation
public class MqttPublisherService : IMqttPublisherService
{
    private readonly string _hubHostName;
    private readonly string _deviceId;
    private readonly string _deviceKey;
    private readonly IMqttClient _mqttClient;
    private string _sasToken = string.Empty;
    private static readonly SemaphoreSlim _reconnectGate = new(1, 1);

    public MqttPublisherService(IConfiguration config)
    {
        _hubHostName = config["MqttSettings:HubHostName"]!;
        _deviceId = config["MqttSettings:DeviceId"]!;
        _deviceKey = config["MqttSettings:DeviceKey"]!;

        _mqttClient = new MqttFactory().CreateMqttClient();
        _mqttClient.DisconnectedAsync += OnDisconnected;
    }

    public async Task ConnectAsync() => await ReconnectWithBackoffAsync();

    public async Task PublishOneInteractive(int machineId, string machineName, string signal)
    {
        await EnsureConnectedAsync();

        var payload = new
        {
            _machineId = machineId,
            _machineName = machineName,
            _signal = signal
        };

        string jsonPayload = JsonSerializer.Serialize(payload);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic($"devices/{_deviceId}/messages/events/")
            .WithPayload(jsonPayload)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _mqttClient.PublishAsync(message);
        Console.WriteLine($"Message published: {jsonPayload}");
    }

    private async Task ReconnectWithBackoffAsync()
    {
        if (!await _reconnectGate.WaitAsync(0))
            return;

        try
        {
            var delay = TimeSpan.FromSeconds(2);
            var maxDelay = TimeSpan.FromSeconds(30);

            while (!_mqttClient.IsConnected)
            {
                try
                {
                    EnsureFreshSasToken();

                    var options = new MqttClientOptionsBuilder()
                        .WithTcpServer(_hubHostName, 8883)
                        .WithClientId(_deviceId)
                        .WithCredentials($"{_hubHostName}/{_deviceId}/?api-version=2021-04-12", _sasToken)
                        .WithTlsOptions(o => o.UseTls())
                        .WithCleanSession()
                        .Build();

                    Console.WriteLine("Connecting to Azure IoT Hub...");
                    var result = await _mqttClient.ConnectAsync(options);

                    if (result.ResultCode == MqttClientConnectResultCode.Success)
                    {
                        Console.WriteLine("Connected to IoT Hub!\n");
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"Connection failed: {result.ResultCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Connection failed: {ex.Message}");

                    if (ex.Message.Contains("NotAuthorized", StringComparison.OrdinalIgnoreCase) ||
                        ex.Message.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase))
                    {
                        _sasToken = Extensions.GenerateSasToken(_hubHostName, _deviceId, _deviceKey, 24 * 3600);
                        Console.WriteLine("SAS token refreshed due to NotAuthorized/Unauthorized");
                    }
                }

                await Task.Delay(delay);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, maxDelay.TotalSeconds));
            }
        }
        finally
        {
            _reconnectGate.Release();
        }
    }

    private async Task OnDisconnected(MqttClientDisconnectedEventArgs e)
    {
        Console.WriteLine($"Disconnected: {e.Reason}.");

        if (e.Reason == MqttClientDisconnectReason.NotAuthorized ||
            e.Reason == MqttClientDisconnectReason.UnspecifiedError)
        {
            _sasToken = Extensions.GenerateSasToken(_hubHostName, _deviceId, _deviceKey, 24 * 3600);
            Console.WriteLine("Refreshed SAS token after disconnect");
        }
        await ReconnectWithBackoffAsync();
    }

    private async Task EnsureConnectedAsync()
    {
        if (_mqttClient.IsConnected) return;

        await ReconnectWithBackoffAsync();
        if (!_mqttClient.IsConnected)
            throw new InvalidOperationException("MQTT client is not connected.");
    }

    private void EnsureFreshSasToken()
    {
        if (string.IsNullOrWhiteSpace(_sasToken) || !Extensions.IsSasTokenValid(_sasToken))
        {
            _sasToken = Extensions.GenerateSasToken(_hubHostName, _deviceId, _deviceKey, 24 * 3600);
            Console.WriteLine("Generated new SAS token");
        }
    }
}