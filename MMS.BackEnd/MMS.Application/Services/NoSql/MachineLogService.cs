using MMS.Application.Ports.Out.Persistence.Interfaces.NoSql;

namespace MMS.Application.Services.NoSql;

public class MachineLogService(
    IMachineLogRepository repository,
    IUserContextService userContextService,
    IUnitOfWork unitOfWork,
    AutoMapperResult _mapper,
    IOperationalDataRepository operationalDataRepository,
    ICacheService cacheService) : IMachineLogService
{
    private const string AllLogsCacheKey = "machine_logs_all";
    private static string GetByIdCacheKey(string id) => $"machine_log:{id}";

    public async Task<ApiResponse<IEnumerable<MachineLog>>> GetAllAsync(PageParameters pageParameters)
    {
        try
        {
            var allowedMachineIds = await GetAllowedMachineIdsAsync();
            var logs = await repository.GetPagedAsync(allowedMachineIds, pageParameters);
            if (logs == null || !logs.Any())
            {
                return new ApiResponse<IEnumerable<MachineLog>>(
                    StatusCodes.Status404NotFound,
                    "No machine logs found."
                );
            }
            
            return new ApiResponse<IEnumerable<MachineLog>>(
                StatusCodes.Status200OK,
                "Machine logs fetched successfully.",
                logs
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<MachineLog>>(
                StatusCodes.Status500InternalServerError,
                $"Error fetching machine logs: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<MachineLog>> GetByIdAsync(string id)
    {
        try
        {
            var log = await repository.GetByIdAsync(id);
            if (log == null)
                return new ApiResponse<MachineLog>(StatusCodes.Status200OK, 
                    $"Machine log with ID {id} not found.");

            var machineResult = await unitOfWork.MachineRepository.GetAsync(log.MachineId);
            if (machineResult.IsLeft)
                return new ApiResponse<MachineLog>(StatusCodes.Status404NotFound, $"Machine with ID {log.MachineId} not found.");

            var machine = machineResult.IfRight();
            await CustomerAccessHelper.ValidateCustomerAccessAllowMMSBridgeAsync(userContextService, machine!.CustomerId);
            return new ApiResponse<MachineLog>(StatusCodes.Status200OK, "Fetched machine log.", log);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<MachineLog>(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            return new ApiResponse<MachineLog>(StatusCodes.Status500InternalServerError, $"Error fetching log: {ex.Message}");
        }
    }

    public async Task<ApiResponse<MachineLog>> CreateAsync(MachineLog dto)
    {
        try
        {
            if (dto.End?.ToString() == "null")
            {
                return new ApiResponse<MachineLog>(
                    StatusCodes.Status400BadRequest,
                    "Invalid 'end' field: must be a valid DateTime or null."
                );
            }

            var entity = _mapper.Map<MachineLog>(dto);
            entity.Id = Guid.NewGuid().ToString();
            entity.UserId = userContextService.UserId??Guid.Empty;
            await repository.AddAsync(entity);
            return new ApiResponse<MachineLog>(StatusCodes.Status201Created, "Created", entity);
        }
        catch (Exception ex)
        {
            return new ApiResponse<MachineLog>(StatusCodes.Status500InternalServerError, $"Error creating log: {ex.Message}");
        }
    }

    public async Task<ApiResponse<MachineLog>> UpdateAsync(MachineLog dto)
    {
        try
        {
            var existing = await repository.GetByIdAsync(dto.Id);
            if (existing == null)
                return new ApiResponse<MachineLog>(StatusCodes.Status204NoContent, $"Log with ID {dto.Id} not found.");

            _mapper.Map(dto, existing);
            existing.UserId = userContextService.UserId ?? Guid.Empty;
            await repository.UpdateAsync(existing);
            return new ApiResponse<MachineLog>(StatusCodes.Status204NoContent, "Updated");
        }
        catch (Exception ex)
        {
            return new ApiResponse<MachineLog>(StatusCodes.Status500InternalServerError, $"Error updating log: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> DeleteAsync(string id)
    {
        try
        {
            var existing = await repository.GetByIdAsync(id);
            if (existing == null)
                return new ApiResponse<string>(StatusCodes.Status404NotFound, $"Log with ID {id} not found.");

            await repository.DeleteAsync(id);
            await cacheService.RemoveAsync(GetByIdCacheKey(id));
            await cacheService.RemoveAsync(AllLogsCacheKey);

            return new ApiResponse<string>(StatusCodes.Status204NoContent, "Deleted");
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>(StatusCodes.Status500InternalServerError, $"Error deleting log: {ex.Message}");
        }
    }

    public async Task<ApiResponse<UtilizationResponseDto>> GetMachineUtilizationAsync(
    Guid machineId,
    DateTime? from = null,
    DateTime? to = null)
    {
        // Validation
        if (!from.HasValue || !to.HasValue)
        {
            return new ApiResponse<UtilizationResponseDto>(
                400, "Both 'from' and 'to' dates are required.");
        }

        var start = from.Value;
        var end = to.Value;

        if (start > end)
        {
            return new ApiResponse<UtilizationResponseDto>(
                400, "'From' must be <= 'To'.");
        }

        // Single query - only MachineLog type with completed logs
        var logs = await repository.GetLogsByMachineIdAndDateRangeAsync(machineId, start, end);

        if (logs == null || logs.Count == 0)
        {
            return new ApiResponse<UtilizationResponseDto>(
                200,
                "No logs found.",
                new UtilizationResponseDto
                {
                    StatusPercent = new Dictionary<string, double>(),
                    TotalUtilization = 0
                });
        }

        // Calculate utilization
        var result = MachineLogStatusCalculator.CalculateStatusPercentages(logs, start, end);

        return new ApiResponse<UtilizationResponseDto>(200, "Utilization data.", result);
    }
    
    public static class MachineLogStatusCalculator
    {
        public static UtilizationResponseDto CalculateStatusPercentages(
        List<MachineLog> logs,
        DateTime rangeStart,
        DateTime rangeEnd)
        {
            var result = new UtilizationResponseDto
            {
                StatusPercent = new Dictionary<string, double>(),
                StatusColors = new Dictionary<string, string>(),
                TotalUtilization = 0
            };

            // ✅ Include both MachineLog and "other" type logs, process them together
            var machineStatusLogs = logs?
                .Where(l => string.IsNullOrEmpty(l.Type) ||
                           l.Type.Equals(MachineLogMetricsCalculator.LOG_TYPE_MACHINE,
                                        StringComparison.OrdinalIgnoreCase) ||
                           l.Type.Equals("other", StringComparison.OrdinalIgnoreCase))
                .ToList() ?? new List<MachineLog>();

            if (machineStatusLogs.Count == 0)
            {
                result.StatusPercent["Uncategorized"] = 100;
                result.StatusColors["Uncategorized"] = GetDefaultColorForStatus("Uncategorized");
                result.TotalUtilization = 0;
                return result;
            }

            double totalRangeSeconds = (rangeEnd - rangeStart).TotalSeconds;
            if (totalRangeSeconds <= 0)
            {
                result.StatusPercent["Uncategorized"] = 100;
                result.StatusColors["Uncategorized"] = GetDefaultColorForStatus("Uncategorized");
                result.TotalUtilization = 0;
                return result;
            }

            // ✅ Collect all time points and their statuses, and track colors
            var events = new List<(DateTime Time, string Status, bool IsStart)>();
            var statusColorMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var log in machineStatusLogs)
            {
                if (!log.Start.HasValue || !log.End.HasValue)
                    continue;

                var logStart = log.Start.Value;
                var logEnd = log.End.Value;

                var overlapStart = logStart > rangeStart ? logStart : rangeStart;
                var overlapEnd = logEnd < rangeEnd ? logEnd : rangeEnd;

                if (overlapEnd <= overlapStart)
                    continue;

                var status = string.IsNullOrWhiteSpace(log.Status)
                    ? "Uncategorized"
                    : log.Status.Trim();

                // Track color for this status (use first non-null color found)
                if (!string.IsNullOrWhiteSpace(log.Color) && !statusColorMap.ContainsKey(status))
                {
                    statusColorMap[status] = log.Color;
                }

                events.Add((overlapStart, status, true));
                events.Add((overlapEnd, status, false));
            }

            // ✅ Sort events by time
            events = events.OrderBy(e => e.Time).ThenBy(e => e.IsStart ? 0 : 1).ToList();

            // ✅ Process events and track active statuses
            var statusSeconds = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            var activeStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            DateTime? lastTime = rangeStart;

            foreach (var ev in events)
            {
                if (lastTime.HasValue && activeStatuses.Count > 0)
                {
                    double seconds = (ev.Time - lastTime.Value).TotalSeconds;
                    // Use the "dominant" status (you can choose: first, last, or priority-based)
                    var currentStatus = activeStatuses.First();

                    if (statusSeconds.ContainsKey(currentStatus))
                        statusSeconds[currentStatus] += seconds;
                    else
                        statusSeconds[currentStatus] = seconds;
                }

                if (ev.IsStart)
                    activeStatuses.Add(ev.Status);
                else
                    activeStatuses.Remove(ev.Status);

                lastTime = ev.Time;
            }

            // Convert seconds to percentages
            foreach (var kvp in statusSeconds)
            {
                result.StatusPercent[kvp.Key] = Math.Round(
                    (kvp.Value / totalRangeSeconds) * 100, 2);
            }

            // Calculate Uncategorized time
            double accountedPercent = result.StatusPercent.Values.Sum();
            if (accountedPercent < 100)
            {
                double uncategorizedPercent = Math.Round(100 - accountedPercent, 2);

                if (result.StatusPercent.ContainsKey("Uncategorized"))
                    result.StatusPercent["Uncategorized"] = Math.Round(
                        result.StatusPercent["Uncategorized"] + uncategorizedPercent, 2);
                else
                    result.StatusPercent["Uncategorized"] = uncategorizedPercent;
            }

            // ✅ Normalize all percentages to ensure 2 decimal places (fix floating-point precision issues)
            var normalizedStatusPercent = new Dictionary<string, double>();
            foreach (var kvp in result.StatusPercent)
            {
                normalizedStatusPercent[kvp.Key] = Math.Round(kvp.Value, 2);
            }
            result.StatusPercent = normalizedStatusPercent;

            // ✅ Assign colors to each status (use from logs or default based on status name)
            foreach (var status in result.StatusPercent.Keys)
            {
                if (!result.StatusColors.ContainsKey(status))
                {
                    // Use color from logs if available, otherwise assign default color
                    if (statusColorMap.ContainsKey(status))
                    {
                        result.StatusColors[status] = statusColorMap[status];
                    }
                    else
                    {
                        result.StatusColors[status] = GetDefaultColorForStatus(status);
                    }
                }
            }

            // ✅ Calculate TotalUtilization (exclude Offline and Uncategorized)
            result.TotalUtilization = Math.Round(
                result.StatusPercent
                    .Where(kvp => !kvp.Key.Equals("Offline", StringComparison.OrdinalIgnoreCase) &&
                                  !kvp.Key.Equals("Uncategorized", StringComparison.OrdinalIgnoreCase))
                    .Sum(kvp => kvp.Value),
                2);

            return result;
        }

        private static string GetDefaultColorForStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return "#808080"; // Gray for empty status

            var normalizedStatus = status.ToLowerInvariant().Trim();

            // In Cycle / Running states = Green
            if (normalizedStatus.Contains("cycle") ||
                normalizedStatus.Contains("running") ||
                normalizedStatus.Contains("in cycle") ||
                normalizedStatus.Contains("operating") ||
                normalizedStatus.Contains("production"))
            {
                return "#22C55E"; // Green
            }

            // Op. Stop = Yellow
            if (normalizedStatus.Contains("op. stop") ||
                normalizedStatus.Contains("op stop") ||
                normalizedStatus.Equals("op. stop"))
            {
                return "#EAB308"; // Yellow
            }

            // Machine Alarm = Orange
            if (normalizedStatus.Contains("machine alarm") ||
                normalizedStatus.Contains("alarm"))
            {
                return "#F97316"; // Orange
            }

            // Idle / Uncategorized = Red
            if (normalizedStatus.Contains("idle") ||
                normalizedStatus.Contains("un-categorized") ||
                normalizedStatus.Contains("uncategorized") ||
                normalizedStatus.Contains("un categorized"))
            {
                return "#EF4444"; // Red
            }

            // Offline = Black
            if (normalizedStatus.Contains("offline"))
            {
                return "#000000"; // Black
            }

            // Default gray for unknown statuses
            return "#808080"; // Gray
        }
    }

    public async Task<ApiResponse<DowntimeApiResponseDto>> GetMachineDowntimeAsync(
    Guid machineId,
    Guid? jobId = null,
    DateTime? from = null,
    DateTime? to = null)
    {
        // Validation
        if (!from.HasValue || !to.HasValue)
        {
            return new ApiResponse<DowntimeApiResponseDto>(
                400, "Both 'from' and 'to' dates are required.");
        }

        var start = from.Value;
        var end = to.Value;

        if (start > end)
        {
            return new ApiResponse<DowntimeApiResponseDto>(
                400, "'From' must be <= 'To'.");
        }

        // Single query
        var logs = await repository.GetDowntimeLogsByMachineIdAsync(machineId, start, end, jobId);

        if (logs == null || logs.Count == 0)
        {
            return new ApiResponse<DowntimeApiResponseDto>(
                200,
                "No downtime logs found.",
                new DowntimeApiResponseDto
                {
                    TotalDowntime = 0,
                    TotalDowntimePercent = 0,
                    DowntimeMetrics = new List<DowntimeResponseDto>()
                });
        }

        // Calculate downtime
        var downtimeEvents = DowntimeCalculator.CalculateDowntimeEvents(logs, start, end);

        if (downtimeEvents.Count == 0)
        {
            return new ApiResponse<DowntimeApiResponseDto>(
                200,
                "No downtime events found.",
                new DowntimeApiResponseDto
                {
                    TotalDowntime = 0,
                    TotalDowntimePercent = 0,
                    DowntimeMetrics = new List<DowntimeResponseDto>()
                });
        }

        var response = DowntimeCalculator.CalculateDowntimeResponse(downtimeEvents);

        // Calculate overall percentage
        var totalRangeSeconds = (end - start).TotalSeconds;
        response.TotalDowntimePercent = totalRangeSeconds > 0
            ? Math.Round((response.TotalDowntime / totalRangeSeconds) * 100, 2)
            : 0;

        return new ApiResponse<DowntimeApiResponseDto>(200, "Downtime data.", response);
    }

    private async Task<List<Guid>> GetAllowedMachineIdsAsync()
    {

        try
        {
            Expression<Func<Machine, bool>> machineSearchExpr = m => true;
            var machineFilters = new List<Expression<Func<Machine, bool>>>();

            var customerIds = CustomerAccessHelper.GetAccessibleCustomerAllowMMSBridgeIds(userContextService);
            if (customerIds != null && customerIds.Any())
            {
                machineFilters.Add(m => customerIds.Contains(m.CustomerId));
            }

            var machines = await unitOfWork.MachineRepository.GetListAsync(
                null,
                machineSearchExpr,
                machineFilters,
                q => q.OrderBy(m => m.Id)
            );

            return machines.Select(m => m.Id).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to fetch allowed machine IDs: {ex.Message}", ex);

        }
    }

    public async Task<ApiResponse<IEnumerable<ActivitySegment>>> GetMachineTimelineAsync(
    Guid machineId,
    DateTime? from = null,
    DateTime? to = null)
    {
        try
        {
            var logs = await repository.GetMachineLogsAsync(machineId, from, to,true);
            IEnumerable<ActivitySegment> activities = new List<ActivitySegment>();

            if (logs.Count == 0)
            {
                return new ApiResponse<IEnumerable<ActivitySegment>>(
                    StatusCodes.Status200OK,
                    "No data found for the specified time range.",
                    activities);
            }

            // ✅ OPTIMIZED: Filter duplicate activity segments efficiently using GroupBy for both closed and open logs
            // ✅ OPTIMIZATION: Separate closed and open logs for efficient processing
            var closedLogs = logs.Where(l => l.End != null).ToList();
            var openLogs = logs.Where(l => l.End == null).ToList();

            // ✅ FIX: Filter duplicate closed logs by grouping on key fields (start, end, status, jobId)
            var deduplicatedClosedLogs = closedLogs
                .GroupBy(l => new
                {
                    Start = l.Start ?? DateTime.MinValue,
                    End = l.End,
                    Status = l.Status ?? "Unknown",
                    JobId = l.JobId ?? string.Empty,
                    UserName = l.UserName ?? "N/A",
                    MainProgram = l.MainProgram ?? string.Empty,
                    CurrentProgram = l.CurrentProgram ?? string.Empty
                })
                .Select(g => g.First())
                .ToList();

            // ✅ OPTIMIZATION: For open logs, group by status and take most recent
            var openLogsByStatus = openLogs
                .GroupBy(l => l.Status ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.OrderByDescending(l => l.Start ?? l.LastUpdateTime).First());

            // ✅ OPTIMIZATION: Combine deduplicated closed logs with deduplicated open logs, then sort once
            activities = deduplicatedClosedLogs
                .Concat(openLogsByStatus.Values)
                .OrderBy(l => l.Start ?? l.LastUpdateTime)
                .Select(log => new ActivitySegment
                {
                    Status = log.Status,
                    Start = log.Start ?? DateTime.MinValue,
                    End = log.End,
                    IsRunning = log.End == null,
                    JobId = log.JobId ?? string.Empty,
                    Color = log.Color ?? "#CCCCCC",
                    UserName = log.UserName,
                    CurrentProgram = log.CurrentProgram ?? string.Empty,
                    MainProgram = log.MainProgram ?? string.Empty
                })
                .ToList();

            return new ApiResponse<IEnumerable<ActivitySegment>>(
                StatusCodes.Status200OK,
                "Logs data fetched.",
                activities);
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<ActivitySegment>>(
                StatusCodes.Status500InternalServerError,
                $"Error retrieving metrics: {ex.Message}",
                null
            );
        }
    }

    public async Task<ApiResponse<MetricResponseDto>> GetMetricsAsync(Guid machineId, MetricRequestDto request)
    {
        try
        {
            if (request.From != default && request.To != default && request.From > request.To)
            {
                return new ApiResponse<MetricResponseDto>(StatusCodes.Status400BadRequest,
                    "From date must be before or equal to To date.", null);
            }

            var (from, to) = (request.From != default && request.To != default)
                ? (request.From, request.To)
                : TimeRangeHelper.GetRange(request.Range);

            bool includeEndNull = request.From == default && request.To == default && request.Range == TimeRange.Daily;

            // Map MetricType to the type string stored in database
            var metricTypeString = request.Metric switch
            {
                MetricType.SpindleSpeed => "spindlespeed",
                MetricType.FeedRate => "feedrate",
                MetricType.SpindleStatus => "spindlestatus",
                MetricType.Temperature => "temperature",
                MetricType.Vibration => "vibration",
                MetricType.PowerConsumption => "powerconsumption",
                MetricType.Torque => "torque",
                MetricType.CoolantLevel => "coolantlevel",
                MetricType.AirPressure => "airpressure",
                MetricType.CycleTime => "cycletime",
                _ => request.Metric.ToString().ToLower()
            };

            // Query OperationalData filtered by machine, time range, and type
            var operationalDataList = await operationalDataRepository
                .GetByMachineIdTypeAndTimeRangeAsync(machineId, metricTypeString, from, to);

            // Convert to list if needed
            var operationalData = operationalDataList.ToList();

            if (!operationalData.Any())
            {
                return new ApiResponse<MetricResponseDto>(StatusCodes.Status200OK,
                    "No metrics data found for the specified time range.",
                    new MetricResponseDto
                    {
                        Metric = request.Metric.ToString(),
                        Unit = GetDefaultUnit(request.Metric),
                        Points = new List<MetricPointDto>()
                    });
            }

            // Get first data for unit reference
            OperationalData? firstData = operationalData.FirstOrDefault();
            var points = new List<MetricPointDto>(operationalData.Count);

            // Process each operational data record
            foreach (var opData in operationalData)
            {
                if (opData?.Measurement == null)
                    continue;

                points.Add(new MetricPointDto
                {
                    Timestamp = opData.Timestamp,
                    Value = opData.Measurement.Value
                });
            }

            // Get unit from first measurement or use default
            var unit = firstData?.Measurement?.Unit ?? GetDefaultUnit(request.Metric);

            var response = new MetricResponseDto
            {
                Metric = request.Metric.ToString(),
                Unit = unit,
                Points = points.OrderBy(p => p.Timestamp).ToList()
            };

            return new ApiResponse<MetricResponseDto>(StatusCodes.Status200OK, "Metrics data fetched.", response);
        }
        catch (Exception ex)
        {
            return new ApiResponse<MetricResponseDto>(StatusCodes.Status500InternalServerError,
                $"Error retrieving metrics: {ex.Message}", null);
        }
    }

    // Helper method to get default unit for each metric type
    private string GetDefaultUnit(MetricType metricType)
    {
        return metricType switch
        {
            MetricType.SpindleSpeed => "RPM",
            MetricType.FeedRate => "mm/min",
            MetricType.SpindleStatus => "status",
            MetricType.Temperature => "°C",
            MetricType.Vibration => "mm/s",
            MetricType.PowerConsumption => "kW",
            MetricType.Torque => "Nm",
            MetricType.CoolantLevel => "%",
            MetricType.AirPressure => "bar",
            MetricType.CycleTime => "seconds",
            _ => string.Empty
        };
    }
}