namespace MMS.Application.Interfaces;

public interface IConnectionMappingService
{
    void Add(Guid machineId, string connectionId);
    void Remove(string connectionId);
    string? Get(Guid machineId);
}