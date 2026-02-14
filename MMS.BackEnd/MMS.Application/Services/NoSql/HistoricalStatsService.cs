using MMS.Application.Ports.Out.Persistence.Interfaces.NoSql;
using static MMS.Application.Services.NoSql.MachineLogService;

namespace MMS.Application.Services.NoSql;

public class HistoricalStatsService(
    IHistoricalStatsRepository repository,
    IUserContextService userContextService,
    IMachineJobRepository machineJobRepository,
    IUnitOfWork unitOfWork,
    IMachineLogRepository machineLogRepository,
    AutoMapperResult mapper,
    IServiceProvider serviceProvider) : IHistoricalStatsService
{
    public async Task<ApiResponse<IEnumerable<HistoricalStats>>> GetAllByMachineIdAsync(Guid machineId)
    {
        try
        {
            var machineResult = await unitOfWork.MachineRepository.GetAsync(machineId);
            if (machineResult.IsLeft)
                return new ApiResponse<IEnumerable<HistoricalStats>>(StatusCodes.Status404NotFound, $"Machine with ID {machineId} not found.");

            var machine = machineResult.IfRight();
            await CustomerAccessHelper.ValidateCustomerAccessAllowMMSBridgeAsync(userContextService, machine.CustomerId);

            var stats = await repository.GetAllByMachineIdAsync(machineId);

            return new ApiResponse<IEnumerable<HistoricalStats>>(StatusCodes.Status200OK, "Historical stats fetched.", stats);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<IEnumerable<HistoricalStats>>(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<HistoricalStats>>(StatusCodes.Status500InternalServerError, $"Error fetching stats: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<HistoricalStats>>> GetAllAsync(PageParameters pageParameters)
    {
        try
        {
            var items = await repository.GetPagedAsync(pageParameters);

            if (items == null || !items.Any())
            {
                return new ApiResponse<IEnumerable<HistoricalStats>>(
                    StatusCodes.Status404NotFound,
                    "No historical stats found."
                );
            }

            return new ApiResponse<IEnumerable<HistoricalStats>>(
                StatusCodes.Status200OK,
                "Historical stats retrieved successfully.",
                items
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<IEnumerable<HistoricalStats>>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<HistoricalStats>>(
                StatusCodes.Status500InternalServerError,
                $"Error fetching stats: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<HistoricalStats>> GetByIdAsync(string id)
    {
        try
        {
            var stat = await repository.GetByIdAsync(id);
            if (stat == null)
                return new ApiResponse<HistoricalStats>(StatusCodes.Status404NotFound, $"Historical stat with ID {id} not found.");

            return new ApiResponse<HistoricalStats>(StatusCodes.Status200OK, "Fetched historical stat.", stat);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<HistoricalStats>(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            return new ApiResponse<HistoricalStats>(StatusCodes.Status500InternalServerError, $"Error fetching stat: {ex.Message}");
        }
    }

    public async Task<ApiResponse<HistoricalStats>> CreateAsync(HistoricalStats dto)
    {
        try
        {
            var entity = mapper.Map<HistoricalStats>(dto);
            entity.Id = Guid.NewGuid().ToString();

            await repository.AddAsync(entity);

            return new ApiResponse<HistoricalStats>(StatusCodes.Status201Created, "Created", entity);
        }
        catch (Exception ex)
        {
            return new ApiResponse<HistoricalStats>(StatusCodes.Status500InternalServerError, $"Error creating stat: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> UpdateAsync(HistoricalStats dto)
    {
        try
        {
            var existing = await repository.GetByIdAsync(dto.Id);
            if (existing == null)
                return new ApiResponse<string>(StatusCodes.Status404NotFound, $"Stat with ID {dto.Id} not found.");

            existing = dto;

            await repository.UpdateAsync(existing);

            return new ApiResponse<string>(StatusCodes.Status204NoContent, "Updated");
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>(StatusCodes.Status500InternalServerError, $"Error updating stat: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> DeleteAsync(string id)
    {
        try
        {
            var existing = await repository.GetByIdAsync(id);
            if (existing == null)
                return new ApiResponse<string>(StatusCodes.Status404NotFound, $"Stat with ID {id} not found.");

            await repository.DeleteAsync(id);

            return new ApiResponse<string>(StatusCodes.Status204NoContent, "Deleted");
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>(StatusCodes.Status500InternalServerError, $"Error deleting stat: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<HistoricalStats>>> CreateHistoricalRecordForDayAsync(DateTime date)
    {
        try
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var jobs = await machineJobRepository.GetByDateRangeAsync(startOfDay, endOfDay);

            if (!jobs.Any())
                return new ApiResponse<IEnumerable<HistoricalStats>>(
                    StatusCodes.Status404NotFound,
                    "No jobs found for the day.");

            var machineJobs = jobs
                .SelectMany(job => job.MachineIds.Select(id => new { MachineId = Guid.Parse(id), Job = job }))
                .GroupBy(x => x.MachineId, x => x.Job)
                .ToDictionary(g => g.Key, g => g.ToList());

            var statsList = new List<HistoricalStats>();
            
            // ✅ OPTIMIZATION: Process machines in parallel batches (default batch size: 10)
            const int batchSize = 10;
            var machineIds = machineJobs.Keys.ToList();
            
            for (int i = 0; i < machineIds.Count; i += batchSize)
            {
                var batch = machineIds.Skip(i).Take(batchSize);
                
                // Process batch in parallel
                var batchTasks = batch.Select(async machineId =>
                {
                    // ✅ OPTIMIZATION: Each parallel task uses its own scope to avoid DbContext conflicts
                    using var scope = serviceProvider.CreateScope();
                    var scopedUnitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var scopedMachineLogRepository = scope.ServiceProvider.GetRequiredService<IMachineLogRepository>();
                    var scopedRepository = scope.ServiceProvider.GetRequiredService<IHistoricalStatsRepository>();
                    
                    var machineJobsList = machineJobs[machineId];
                    
                    var machineResult = await scopedUnitOfWork.MachineRepository.GetAsync(machineId);
                    if (machineResult.IsLeft) return null;

                    var machine = machineResult.IfRight();
                    var customerId = machine!.CustomerId;

                    var allLogs = await scopedMachineLogRepository
                        .GetLogsByMachineIdAndDateRangeAsync(machineId, startOfDay, endOfDay);

                    // ✅ OPTIMIZATION: Update logs in-place (more efficient than creating new list)
                    var logsList = allLogs?.ToList() ?? new List<MachineLog>();
                    foreach (var log in logsList)
                    {
                        if (log.End == null)
                            log.End = endOfDay;
                    }

                    var downtimeLogs = await scopedMachineLogRepository
                        .GetDowntimeLogsByMachineIdAsync(machineId, startOfDay, endOfDay, null);

                    var downtimeLogsList = downtimeLogs?.ToList() ?? new List<MachineLog>();

                    var utilizationPercent = CalculateUtilizationFromLogs(logsList, startOfDay, endOfDay);

                    var (totalDowntimeSec, historicalDowntimeEvents) =
                        CalculateDowntimeFromLogs(downtimeLogsList, startOfDay, endOfDay);

                    var stat = new HistoricalStats
                    {
                        MachineId = machineId,
                        CustomerId = customerId,
                        GeneratedDate = startOfDay,
                        JobIds = machineJobsList.Select(j => j.Id).Distinct().ToList(),
                        JobMetrics = new List<JobMetric>(),
                        HistoricalDowntimeEvents = historicalDowntimeEvents
                    };

                    double total_planned = 0;
                    double total_operating = 0;
                    double total_good = 0;
                    double total_bad = 0;
                    double total_completed = 0;
                    double total_ideal_time = 0;
                    double total_required = 0;

                    var jobMetrics = new List<JobMetric>();

                    foreach (var job in machineJobsList)
                    {
                        double planned_sec =
                            (job.Schedule.PlannedEnd - job.Schedule.PlannedStart).TotalSeconds;

                        if (planned_sec <= 0) continue;

                        double good = job.Quantities.Good;
                        double bad = job.Quantities.Bad;
                        double completed = job.Quantities.Completed;
                        double required = job.Quantities.Required;

                        double operating_sec = (utilizationPercent / 100) * planned_sec;

                        double avail_ratio = operating_sec / planned_sec;

                        double perf_ratio =
                            (operating_sec > 0 && job.Metrics.TargetCycleTime > 0)
                                ? (job.Metrics.TargetCycleTime * good) / operating_sec
                                : 0;

                        perf_ratio = Math.Clamp(perf_ratio, 0, 1);

                        double qual_ratio =
                            (good + bad > 0) ? good / (good + bad) : 0;

                        qual_ratio = Math.Clamp(qual_ratio, 0, 1);

                        double oee = avail_ratio * perf_ratio * qual_ratio * 100;

                        jobMetrics.Add(new JobMetric
                        {
                            Id = job.Id,
                            JobName = job.JobName,
                            StartTime = job.StartTime,
                            EndTime = job.EndTime,
                            Status = job.Status,
                            ProgramNo = job.ProgramNo,
                            OperatorName = job.OperatorName,
                            Availability = avail_ratio * 100,
                            Performance = perf_ratio * 100,
                            Quality = qual_ratio * 100,
                            OEE = oee,
                            QtyCompleted = (int)Math.Round(completed),
                            QtyGood = (int)Math.Round(good),
                            QtyBad = (int)Math.Round(bad),
                            PlannedTime = planned_sec,
                            RequiredQty = (int)Math.Round(required)
                        });

                        total_planned += planned_sec;
                        total_operating += operating_sec;
                        total_good += good;
                        total_bad += bad;
                        total_completed += completed;
                        total_ideal_time += job.Metrics.TargetCycleTime * good;
                        total_required += required;
                    }

                    if (total_planned <= 0) return null;

                    double overall_avail = total_operating / total_planned;
                    double overall_perf = total_operating > 0 ? total_ideal_time / total_operating : 0;
                    overall_perf = Math.Clamp(overall_perf, 0, 1);

                    double overall_qual =
                        (total_good + total_bad > 0) ? total_good / (total_good + total_bad) : 0;

                    overall_qual = Math.Clamp(overall_qual, 0, 1);

                    double overall_oee = overall_avail * overall_perf * overall_qual * 100;

                    // ✅ DOWNTIME IN PERCENT
                    double downtimePercent =
                        total_planned > 0 ? (totalDowntimeSec / total_planned) * 100 : 0;

                    stat.DownTime = Math.Round(downtimePercent, 2);
                    stat.Availability = Math.Round(overall_avail * 100, 2);
                    stat.Performance = Math.Round(overall_perf * 100, 2);
                    stat.Quality = Math.Round(overall_qual * 100, 2);
                    stat.OEE = Math.Round(overall_oee, 2);
                    stat.QtyCompleted = (int)Math.Round(total_completed);
                    stat.QtyGood = (int)Math.Round(total_good);
                    stat.QtyBad = (int)Math.Round(total_bad);
                    stat.PlannedTime = total_planned;
                    stat.RequiredQty = (int)Math.Round(total_required);
                    stat.Utilization = utilizationPercent;
                    stat.JobMetrics = jobMetrics;

                    var existing = await scopedRepository.GetByMachineAndDateAsync(machineId, startOfDay);

                    if (existing != null)
                    {
                        existing.DownTime = stat.DownTime;
                        existing.Availability = stat.Availability;
                        existing.Performance = stat.Performance;
                        existing.Quality = stat.Quality;
                        existing.OEE = stat.OEE;
                        existing.QtyCompleted = stat.QtyCompleted;
                        existing.QtyGood = stat.QtyGood;
                        existing.QtyBad = stat.QtyBad;
                        existing.PlannedTime = stat.PlannedTime;
                        existing.RequiredQty = stat.RequiredQty;
                        existing.Utilization = stat.Utilization;

                        existing.JobMetrics.Clear();
                        existing.JobMetrics.AddRange(jobMetrics);

                        existing.HistoricalDowntimeEvents.Clear();
                        existing.HistoricalDowntimeEvents.AddRange(historicalDowntimeEvents);

                        await scopedRepository.UpdateAsync(existing);
                        stat = existing;
                    }
                    else
                    {
                        stat.Id = $"{machineId}_{startOfDay:yyyyMMdd}";
                        await scopedRepository.AddAsync(stat);
                    }

                    return stat;
                });

                var batchResults = await Task.WhenAll(batchTasks);
                statsList.AddRange(batchResults.Where(s => s != null)!);
            }

            return new ApiResponse<IEnumerable<HistoricalStats>>(
                StatusCodes.Status200OK,
                "Historical stats calculated and stored.",
                statsList);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<IEnumerable<HistoricalStats>>(
                StatusCodes.Status403Forbidden,
                ex.Message);
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<HistoricalStats>>(
                StatusCodes.Status500InternalServerError,
                $"Error calculating stats: {ex.Message}");
        }
    }

    private double CalculateUtilizationFromLogs(List<MachineLog> logs, DateTime startOfDay, DateTime endOfDay)
    {
        if (logs == null || !logs.Any())
            return 0;

        // Use existing MachineLogStatusCalculator
        var breakdownDto = MachineLogStatusCalculator.CalculateStatusPercentages(logs, startOfDay, endOfDay);

        if (breakdownDto.StatusPercent == null || !breakdownDto.StatusPercent.Any())
            return 0;

        // Return total of all status percentages (or you can modify based on your needs)
        return Math.Round(breakdownDto.StatusPercent.Values.Sum(), 2);
    }

    private (double TotalDowntimeSec, List<HistoricalDowntimeEvent> Events) CalculateDowntimeFromLogs(
         List<MachineLog> downtimeLogs,
         DateTime startOfDay,
         DateTime endOfDay)
    {
        if (downtimeLogs == null || !downtimeLogs.Any())
            return (0, new List<HistoricalDowntimeEvent>());

        // Use existing DowntimeCalculator
        var downtimeEvents = DowntimeCalculator.CalculateDowntimeEvents(downtimeLogs, startOfDay, endOfDay);

        if (!downtimeEvents.Any())
            return (0, new List<HistoricalDowntimeEvent>());

        var response = DowntimeCalculator.CalculateDowntimeResponse(downtimeEvents);

        // Convert to HistoricalDowntimeEvent format
        var historicalEvents = response.DowntimeMetrics
            .Select(dm => new HistoricalDowntimeEvent
            {
                Reason = dm.Reason,
                TotalDuration = dm.Duration
            })
            .ToList();

        return (response.TotalDowntime, historicalEvents);
    }
}