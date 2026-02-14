


namespace MMS.Application.Ports.Out.Persistence.Interfaces.NoSql;

public interface IOperationalDataRepository
{
    //Task AddAsync(OperationalData operationalData);
    Task AddRangeAsync(List<OperationalData> operationalDataList);
    //Task<IEnumerable<OperationalData>> GetByMachineIdAndTimeRangeAsync(Guid machineId, DateTime from, DateTime to);
    Task<IEnumerable<OperationalData>> GetByMachineIdTypeAndTimeRangeAsync(Guid machineId, string type, DateTime from, DateTime to);
}
