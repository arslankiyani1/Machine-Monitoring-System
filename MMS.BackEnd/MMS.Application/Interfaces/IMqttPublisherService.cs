namespace MMS.Application.Interfaces;

public interface IMqttPublisherService
{
    Task ConnectAsync();
    Task PublishOneInteractive(int machineId, string machineName, string signal);
}
