namespace MMS.Adapters.NoSQL.Repositories;

public class OperationalDataRepository
    (MyCosmosDbContext dbContext) : IOperationalDataRepository
{
    public async Task AddRangeAsync(List<OperationalData> operationalDataList)
    {
        await dbContext.AddRangeAsync(operationalDataList);
        await dbContext.SaveChangesAsync();
    }

    //public async Task<IEnumerable<OperationalData>> GetByMachineIdAndTimeRangeAsync(Guid machineId, DateTime from, DateTime to)
    //{
    //    return await dbContext.OperationalData
    //   .Where(od => od.MachineId == machineId
    //             && od.Timestamp >= from
    //             && od.Timestamp <= to)
    //   .OrderBy(od => od.Timestamp)
    //   .ToListAsync();
    //}

    public async Task<IEnumerable<OperationalData>> GetByMachineIdTypeAndTimeRangeAsync(
    Guid machineId,
    string type,
    DateTime from,
    DateTime to)
    {
        return await dbContext.OperationalData
            .Where(od => od.MachineId == machineId
                      && od.Type.ToLower() == type.ToLower()
                      && od.Timestamp >= from
                      && od.Timestamp <= to)
            .OrderBy(od => od.Timestamp)
            .ToListAsync();
    }
}
