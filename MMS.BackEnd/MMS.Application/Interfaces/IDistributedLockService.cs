namespace MMS.Application.Interfaces;

public interface IDistributedLockService
{
    Task<IDisposable?> AcquireLockAsync(string lockKey, TimeSpan expiry, TimeSpan? waitTime = null, CancellationToken cancellationToken = default);
    Task<(bool Success, IDisposable? LockHandle)> TryAcquireLockAsync(string lockKey, TimeSpan expiry);
}
