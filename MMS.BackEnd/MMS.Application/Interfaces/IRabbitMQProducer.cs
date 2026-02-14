namespace MMS.Application.Interfaces;

public interface IRabbitMQProducer
{
    void PublishMachineLogAsync(MachineLogSignalRDto log);
}
