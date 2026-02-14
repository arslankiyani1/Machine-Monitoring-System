namespace MMS.Adapters.RabbitMq.SignalR.RabbitMq.Contracts;

public class OrderCreated
{
    public int OrderId { get; set; }
    public int Message { get; set; }
}