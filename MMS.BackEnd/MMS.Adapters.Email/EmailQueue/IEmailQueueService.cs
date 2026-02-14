namespace MMS.Adapters.Email.EmailQueue;

public interface IEmailQueueService
{
    void QueueEmail(Func<Task> emailTask);
    Task<Func<Task>> DequeueEmailAsync(CancellationToken cancellationToken);
}