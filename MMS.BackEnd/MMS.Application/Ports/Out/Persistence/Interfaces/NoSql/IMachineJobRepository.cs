using MMS.Application.Ports.In.NoSql.MachineJob.Dto;

namespace MMS.Application.Ports.Out.Persistence.Interfaces.NoSql;

public interface IMachineJobRepository
{
    //Task<IEnumerable<MachineJob>> GetAllAsync(IEnumerable<Guid>? customerIds);
    Task<MachineJob?> GetByIdAsync(string id);
    Task AddAsync(MachineJob entity);
    Task UpdateAsync(MachineJob entity);
    Task DeleteAsync(string id);
    Task<List<MachineJob>> GetByMachineIdAsync(string machineId);
    Task<MachineJob?> GetLatestJobByMachineIdAsync(Guid id);
    Task<List<MachineJob>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<IEnumerable<MachineJob>> GetJobsByMachineIdAndDateRangeAsync(Guid machineId, DateTime start, DateTime end);
    Task<MachineJob?> GetActiveJobByMachineIdAsync(string machineId, DateTime currentTime);
    Task<List<MachineJob>> GetJobsByMachineIdsAndDateRangeAsync(List<Guid> machineIds, DateTime start, DateTime end);
    Task<IEnumerable<MachineJob>> GetPagedAsync(IEnumerable<Guid> customerIds, PageParameters pageParameters, Guid? machineId);
    Task<JobStatusSummaryDto?> GetJobSummaryAsync(Guid customerId,PageParameters pageParameters);
    Task UpdateDownTimeAsync(string id, MachineJob activeJob);
    Task<MachineJob?> GetCurrentJobAsync(string machineId, DateTime exactTime);
}