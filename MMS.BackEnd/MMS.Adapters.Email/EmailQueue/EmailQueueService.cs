namespace MMS.Adapters.Email.EmailQueue;

[ExcludeFromCodeCoverage]
public class EmailQueueService(Channel<Func<Task>> emailQueue)
    : IEmailQueueService
{
    private readonly Channel<Func<Task>> _emailQueue = emailQueue ?? throw new ArgumentNullException(nameof(emailQueue));

    public void QueueEmail(Func<Task> emailTask)
    {
        ArgumentNullException.ThrowIfNull(emailTask);
        _emailQueue.Writer.TryWrite(emailTask);
    }

    public async Task<Func<Task>> DequeueEmailAsync(CancellationToken cancellationToken)
    {
        var emailTask = await _emailQueue.Reader.ReadAsync(cancellationToken);
        return emailTask;
    }
}
