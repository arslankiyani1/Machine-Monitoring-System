using MMS.Application.Ports.In.NoSql.MachineJob.Dto;


namespace MMS.Adapters.NoSQL.Repositories;

public class MachineJobRepository(MyCosmosDbContext dbContext) : IMachineJobRepository
{
    public async Task AddAsync(MachineJob entity)
    {
        await dbContext.MachineJobs.AddAsync(entity);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            dbContext.MachineJobs.Remove(entity);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<MachineJob?> GetByIdAsync(string id)
    {
        return await dbContext.MachineJobs.AsTracking()
            .Include(x => x.DowntimeEvents) 
            .Include(x => x.Quantities)
            .Include(x => x.Metrics)
            .Include(x => x.Schedule)
            .Include(x => x.Setup)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<MachineJob>> GetByMachineIdAsync(string machineId)
    {
        return await dbContext.MachineJobs
            .Where(job => job.MachineIds.Contains(machineId))
            .OrderByDescending(job => job.StartTime)
            .ToListAsync();
    }

    public async Task<MachineJob?> GetLatestJobByMachineIdAsync(Guid id)
    {
        return await dbContext.MachineJobs
             .Where(job => job.MachineIds.Contains(id.ToString()))
             .OrderBy(job => job.StartTime)
             .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(MachineJob entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        try
        {
            dbContext.MachineJobs.Update(entity);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("exception here",ex.Message);
        }
    }

    public async Task<List<MachineJob>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        return await dbContext.MachineJobs
            .Where(j => j.StartTime < end && j.EndTime > start)
            .ToListAsync();
    }

    public async Task<MachineJob?> GetActiveJobByMachineIdAsync(string machineId, DateTime currentTime)
    {
        return await dbContext.MachineJobs
        .Where(job =>
            job.MachineNames.Contains(machineId) &&
            job.Status == JobStatus.InProgress &&
            job.StartTime <= currentTime
        )
        .OrderBy(job => job.StartTime)
        .FirstOrDefaultAsync(); 
    }

    public async Task<IEnumerable<MachineJob>> GetJobsByMachineIdAndDateRangeAsync(
         Guid machineId, 
         DateTime start, 
         DateTime end)
    {
        var machineIdStr = machineId.ToString();

        return await dbContext.MachineJobs
            .AsNoTracking()
            .Where(job =>
                job.MachineIds.Contains(machineIdStr) &&           // ARRAY contains
                job.StartTime <= end &&                            // job starts before range ends
                job.EndTime >= start                               // job ends after range starts
            )
            .ToListAsync();
    }

    //public async Task<List<MachineJob>> GetJobsByMachineIdsAndDateRangeAsync(List<Guid> machineIds, DateTime start, DateTime end)
    //{
    //    var machineIdStrings = machineIds.Select(id => id.ToString()).ToList();
    //    return await dbContext.MachineJobs
    //        .AsNoTracking()
    //        .Where(x => x.MachineIds.Any(id => machineIdStrings.Contains(id))
    //                    && x.StartTime >= start
    //                    && x.EndTime <= end)
    //        .ToListAsync();
    //}

    public async Task<List<MachineJob>> GetJobsByMachineIdsAndDateRangeAsync(List<Guid> machineIds, DateTime start, DateTime end)
    {
        var machineIdStrings = machineIds.Select(id => id.ToString().ToLower()).ToList();

        return await dbContext.MachineJobs
        .AsNoTracking()
        .Where(job =>
            job.Status == JobStatus.InProgress &&                          // 👈 Only InProgress
            job.MachineIds.Any(id => machineIdStrings.Contains(id.ToLower())) &&
            job.StartTime < end &&
            job.EndTime > start
        )
        .ToListAsync();
    }


    public async Task<IEnumerable<MachineJob>> GetPagedAsync(
    IEnumerable<Guid> customerIds,
    PageParameters pageParameters,
    Guid? machineId)
    {
        var query = dbContext.MachineJobs.AsQueryable();

        // ✅ Customer filter
        if (customerIds != null && customerIds.Any())
            query = query.Where(job => customerIds.Contains(job.CustomerId));

        // ✅ Search filter (JobName)
        if (!string.IsNullOrWhiteSpace(pageParameters.Term))
        {
            var termLower = pageParameters.Term.Trim().ToLower();
            query = query.Where(j =>(j.JobName != null && j.JobName.ToLower().Contains(termLower)));
        }

        // ✅ Machine filter (handles List<string> correctly)
        if (machineId.HasValue)
        {
            var machineIdStr = machineId.Value.ToString();
            query = query.Where(j => j.MachineIds.Any(mid => mid == machineIdStr));
        }

        // ✅ Paging setup
        int skip = pageParameters.Skip ?? 0;
        int top = pageParameters.Top ?? 0;

        // If Top = 0, fetch all results
        if (top <= 0)
        {
            return await query
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        // ✅ Proper pagination
        return await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip(skip)
            .Take(top)
            .ToListAsync();
    }

    public async Task<JobStatusSummaryDto?> GetJobSummaryAsync(Guid customerId, PageParameters pageParameters)
    {
        try
        {
            var query = dbContext.MachineJobs
                .Where(j => j.CustomerId == customerId);

            if (!string.IsNullOrEmpty(pageParameters.Term))
            {
                string searchTerm = pageParameters.Term.ToLower();
                query = query.Where(j =>
                    j.OperatorName.ToLower().Contains(searchTerm) ||
                    j.MachineNames.ToArray().Contains(searchTerm));
            }

            var jobs = await query
                .Select(j => j.Status)
                .ToListAsync();

            var summary = new JobStatusSummaryDto
            {
                TotalJobs = jobs.Count,
                Scheduled = jobs.Count(s => s == JobStatus.Scheduled),
                InProgress = jobs.Count(s => s == JobStatus.InProgress),
                Interrupted = jobs.Count(s => s == JobStatus.Stopped),
                Completed = jobs.Count(s => s == JobStatus.Completed)
            };

            return summary;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public Task UpdateDownTimeAsync(string id, MachineJob activeJob)
    {
        throw new NotImplementedException();
    }

    public async Task<MachineJob?> GetCurrentJobAsync(string machineId, DateTime exactTime)
    {
        var activeJob = await dbContext.MachineJobs
         .Where(job => job.MachineIds.Contains(machineId) && job.Status == JobStatus.InProgress)
         .OrderByDescending(job => job.StartTime) // Order by the most recent job
         .FirstOrDefaultAsync(); // Get the most recent active job

        return activeJob;
    }
}