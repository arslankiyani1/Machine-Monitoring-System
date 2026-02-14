namespace MMS.Adapters.RabbitMq.SignalR;

public class ConnectionMappingService : IConnectionMappingService
{
    private readonly ConcurrentDictionary<Guid, string> _machineToConnection = new();
    private readonly ConcurrentDictionary<string, Guid> _connectionToMachine = new();

    public void Add(Guid machineId, string connectionId)
    {
        _machineToConnection[machineId] = connectionId;
        _connectionToMachine[connectionId] = machineId;
    }

    public void Remove(string connectionId)
    {
        if (_connectionToMachine.TryRemove(connectionId, out var machineId))
        {
            _machineToConnection.TryRemove(machineId, out _);
        }
    }

    public string? Get(Guid machineId)
    {
        return _machineToConnection.TryGetValue(machineId, out var connId) ? connId : null;
    }
}
