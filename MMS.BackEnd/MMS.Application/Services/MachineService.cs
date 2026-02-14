namespace MMS.Application.Services;

public class MachineService(
    IMachineRepository machineRepository,
    AutoMapperResult mapper,
    IUnitOfWork unitOfWork,
    IMachineStatusSettingRepository machineStatusSetting,
    IBlobStorageService blobStorageService,
    IUserContextService userContextService,
    IMachineJobRepository _machineJobRepository,
    IServiceProvider serviceProvider,
    IHistoricalStatsRepository _historicalRepository,
    IMachineMaintenanceTaskRepository machineMaintenanceTask,
    ICacheService cache) : IMachineService
{
    private const string CachePrefix = "Machine:";
    private string GetMachineKey(Guid id) => $"Machine:{id}/{userContextService.UserId}";

    public async Task<ApiResponse<IEnumerable<MachineDto>>> GetListAsync(
     PageParameters pageParameters,
     CommunicationProtocol? protocol,
     MachineType? type,
     Guid customerId)
    {
        try
        {
            // ✅ PERFORMANCE: Build cache key from all parameters
            string? term = pageParameters.Term?.Trim();
            var accessibleIds = customerId == Guid.Empty 
                ? CustomerAccessHelper.GetAccessibleCustomerIds(userContextService)
                : null;
            var customerIdsForCache = accessibleIds != null && accessibleIds.Any() 
                ? string.Join(",", accessibleIds) 
                : customerId != Guid.Empty ? customerId.ToString() : "all";
            
            var cacheKey = $"machine:list:{protocol}:{type}:{customerIdsForCache}:{term}:{pageParameters.Top}:{pageParameters.Skip}:{userContextService.UserId}";
            
            // ✅ PERFORMANCE: Check cache first
            var cached = await cache.GetAsync<List<MachineDto>>(cacheKey);
            if (cached != null)
            {
                return new ApiResponse<IEnumerable<MachineDto>>(
                    StatusCodes.Status200OK,
                    nameof(Machine) + ResponseMessages.GetAll + " (cached)",
                    cached
                );
            }

            bool hasTerm = !string.IsNullOrEmpty(term);

            Expression<Func<Machine, bool>>? searchExpr = hasTerm
                ? m =>
                    EF.Functions.ILike(m.MachineName!, $"%{term}%") ||
                    EF.Functions.ILike(m.Manufacturer!, $"%{term}%") ||
                    EF.Functions.ILike(m.SerialNumber!, $"%{term}%")
                : null;

            var filters = new List<Expression<Func<Machine, bool>>>();

            if (protocol.HasValue)
                filters.Add(m => m.CommunicationProtocol == protocol);

            if (type.HasValue)
                filters.Add(m => m.MachineType == type);

            if (customerId == Guid.Empty)
            {
                if (accessibleIds?.Any() == true)
                    filters.Add(m => accessibleIds.Contains(m.CustomerId));
            }
            else
            {
                await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, customerId);
                filters.Add(m => m.CustomerId == customerId);
            }

            var machines = await machineRepository.GetListAsync(
                pageParameters,
                searchExpr!,
                filters,
                q => q.OrderBy(m => m.MachineName)
                      .ThenBy(m => m.Manufacturer)
                      .ThenBy(m => m.SerialNumber)
            );

            var machineDtos = mapper.Map<IEnumerable<MachineDto>>(machines);
            var machineDtosList = machineDtos.ToList();
            
            // ✅ PERFORMANCE: Cache for 5 minutes
            await cache.SetAsync(cacheKey, machineDtosList, TimeSpan.FromMinutes(5), CachePrefix);
            
            return new ApiResponse<IEnumerable<MachineDto>>(
                StatusCodes.Status200OK,
                nameof(Machine) + ResponseMessages.GetAll,
                machineDtosList
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<MachineDto>>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }


    public async Task<ApiResponse<MachineDto>> GetByIdAsync(
    Guid machineId,
    CancellationToken cancellationToken = default)
    {
        try
        {
            if (machineId == Guid.Empty)
            {
                return new ApiResponse<MachineDto>(
                    StatusCodes.Status400BadRequest,
                    "Invalid Machine Id."
                );
            }

            // ✅ PERFORMANCE: Cache machine lookups by ID
            var cacheKey = GetMachineKey(machineId);
            var cached = await cache.GetAsync<MachineDto>(cacheKey);
            if (cached != null)
            {
                return new ApiResponse<MachineDto>(
                    StatusCodes.Status200OK,
                    nameof(Machine) + ResponseMessages.Get + " (cached)",
                    cached
                );
            }

            var result = await machineRepository.GetAsync(machineId, cancellationToken);

            return result.Match(
                right =>
                {
                    var dto = mapper.Map<MachineDto>(right);
                    // ✅ PERFORMANCE: Cache for 10 minutes
                    _ = cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10), CachePrefix);
                    
                    return new ApiResponse<MachineDto>(
                        StatusCodes.Status200OK,
                        nameof(Machine) + ResponseMessages.Get,
                        dto
                    );
                },
                left => new ApiResponse<MachineDto>(
                    StatusCodes.Status404NotFound,
                    left.Message
                )
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<MachineDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<MachineDto>> GetBySerialNumber(string serialNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            // ✅ PERFORMANCE: Cache machine lookups by serial number
            var cacheKey = $"{CachePrefix}BySerial:{serialNumber}";
            var cached = await cache.GetAsync<MachineDto>(cacheKey);
            if (cached != null)
            {
                return new ApiResponse<MachineDto>(
                    StatusCodes.Status200OK,
                    nameof(Machine) + ResponseMessages.Get + " (cached)",
                    cached
                );
            }

            var result = await machineRepository.GetBySerialNumberAsync(serialNumber);

            if (result.IsRight)
            {
                var machine = result.IfRight();
                await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, machine!.CustomerId);

                var machineDto = mapper.Map<MachineDto>(machine!);
                
                // ✅ PERFORMANCE: Cache for 5 minutes
                await cache.SetAsync(cacheKey, machineDto, TimeSpan.FromMinutes(5), CachePrefix);

                return new ApiResponse<MachineDto>(
                    StatusCodes.Status200OK,
                    nameof(Machine) + ResponseMessages.Get,
                    machineDto
                );
            }

            var error = result.IfLeft();
            return new ApiResponse<MachineDto>(
                StatusCodes.Status200OK,
                error!.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<MachineDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<MachineDto>> GetByMachineName(string machineName, CancellationToken cancellationToken = default)
    {
        try
        {
            // ✅ PERFORMANCE: Cache machine lookups by name (machines don't change frequently)
            var cacheKey = $"{CachePrefix}ByName:{machineName}";
            var cached = await cache.GetAsync<MachineDto>(cacheKey);
            if (cached != null)
            {
                return new ApiResponse<MachineDto>(
                    StatusCodes.Status200OK,
                    nameof(Machine) + ResponseMessages.Get,
                    cached
                );
            }

            var result = await machineRepository.GetByMachineNameAsync(machineName);

            if (result.IsRight)
            {
                var machine = result.IfRight();
                var machineDto = mapper.Map<MachineDto>(machine!);
                
                // ✅ PERFORMANCE: Cache for 5 minutes (machines rarely change)
                await cache.SetAsync(cacheKey, machineDto, TimeSpan.FromMinutes(5));
                
                return new ApiResponse<MachineDto>(
                    StatusCodes.Status200OK,
                    nameof(Machine) + ResponseMessages.Get,
                    machineDto
                );
            }

            var error = result.IfLeft();
            return new ApiResponse<MachineDto>(
                StatusCodes.Status404NotFound,
                error!.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<MachineDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<MachineDto>> AddAsync(AddMachineDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        string? imageUrl = null;

        try
        {
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, request.CustomerId);

            if (!await unitOfWork.CustomerRepository.ExistsAsync((Guid)request.CustomerId!))
            {
                return new ApiResponse<MachineDto>(
                    StatusCodes.Status400BadRequest,
                    $"Customer with ID {request.CustomerId} does not exist."
                );
            }

            if (await unitOfWork.MachineRepository.ExistsAsync(m => m.SerialNumber!.ToLower() == request.SerialNumber!.ToLower()))
            {
                return new ApiResponse<MachineDto>(
                    StatusCodes.Status409Conflict,
                    $"A machine with Serial Number '{request.SerialNumber}' already exists."
                );
            }

            if (!String.IsNullOrEmpty(request.ImageBase64))
            {
                try
                {
                    imageUrl = (await blobStorageService.UploadBase64Async(request.ImageBase64,BlobStorageConstants.MachinesFolder)).AbsoluteUri;
                }
                catch (Exception ex)
                {
                    return new ApiResponse<MachineDto>(
                        StatusCodes.Status400BadRequest,
                        $"Image upload failed: {ex.Message}"
                    );
                }
            }

            var machine = mapper.Map<Machine>(request);
            machine.ImageUrl = imageUrl;

            var addedMachine = await machineRepository.AddAsync(machine);

            var defaultStatusSetting = new MachineStatusSetting
            {
                Id = Guid.NewGuid().ToString(),
                MachineId = machine.Id,
                Inputs = new List<MachineInput>
                {
                    new() { InputKey = "input_one", Signals = "1,0,0,0", Color = "#22C55E", Status = MachineStatus.InCycle.ToString() },
                    new() { InputKey = "input_two", Signals = "0,1,0,0", Color = "#F59E0B", Status = MachineStatus.MachineAlarm.ToString() },
                    new() { InputKey = "input_three", Signals = "0,0,1,0", Color = "#3B82F6", Status = MachineStatus.OperationStop.ToString() },
                    new() { InputKey = "input_four", Signals = "0,0,0,1", Color = "#EF4444", Status = MachineStatus.PalletChange.ToString() }
                }
            };
            await machineStatusSetting.AddAsync(defaultStatusSetting);

            var defaultMachineSetting = new MachineSetting
            {
                MachineId = machine.Id,
                CycleStartInterlock = false,
                GuestLock = false,
                ReverseCSlockLogic = false,
                AutomaticPartsCounter = false,
                MaxFeedrate = 2500,
                MaxSpindleSpeed = 1200,
                StopTimelimit = null,
                PlannedProdusctionTime = null,
                MinElapsedCycleTime = null,
                Status = MachineSettingsStatus.Active,
                DownTimeReasons = new List<string>()
            };
            await unitOfWork.MachineSettingRepository.AddAsync(defaultMachineSetting);

            await transaction.CommitAsync();
            return new ApiResponse<MachineDto>(
                StatusCodes.Status201Created,
                nameof(Machine) + ResponseMessages.Added,
                mapper.GetResult<Machine, MachineDto>(addedMachine)
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<MachineDto>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<MachineDto>(
                StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<MachineDto>> UpdateAsync(Guid machineId, UpdateMachineDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, request.CustomerId);

            var result = await machineRepository.GetAsync(machineId);
            if (result.IsLeft)
            {
                return new ApiResponse<MachineDto>(
                    StatusCodes.Status400BadRequest,
                    $"Machine with ID {machineId} not found."
                );
            }

            var rightMachine = result.IfRight();

            if (!await unitOfWork.CustomerRepository.ExistsAsync(request.CustomerId))
            {
                return new ApiResponse<MachineDto>(
                    StatusCodes.Status400BadRequest,
                    $"Customer with ID {request.CustomerId} does not exist."
                );
            }


            if (!string.Equals(rightMachine!.SerialNumber, request.SerialNumber, StringComparison.OrdinalIgnoreCase))
            {
                var isSerialInUse = await machineRepository.ExistsAsync(m =>
                    m.SerialNumber == request.SerialNumber && m.Id != machineId);

                if (isSerialInUse)
                {
                    return new ApiResponse<MachineDto>(
                        StatusCodes.Status409Conflict,
                        $"Serial number '{request.SerialNumber}' is already used by another machine."
                    );
                }
            }

            if (!String.IsNullOrWhiteSpace(request.ImageBase64))
            {
                string uploadedUrl;

                if (!String.IsNullOrEmpty(rightMachine.ImageUrl))
                {
                    // ✅ FIX: Delete old blob first, then upload new one to get a new URL
                    // This ensures that when a different image is uploaded, it gets a new URL
                    try
                    {
                        await blobStorageService.DeleteBase64Async(rightMachine.ImageUrl);
                    }
                    catch (Exception ex)
                    {
                        // Log but don't fail - old blob might not exist or already deleted
                        Console.WriteLine($"Warning: Could not delete old blob: {ex.Message}");
                    }
                }

                // ✅ Always upload as new blob to get a new URL
                var newUri = await blobStorageService.UploadBase64Async(request.ImageBase64!, BlobStorageConstants.CustomerFolder);
                uploadedUrl = newUri.AbsoluteUri;

                rightMachine.ImageUrl = uploadedUrl;
            }

            mapper.Map(request, rightMachine);

            await unitOfWork.SaveChangesAsync();
            await cache.RemoveTrackedKeysAsync(CachePrefix);
            await transaction.CommitAsync();

            return new ApiResponse<MachineDto>(
                StatusCodes.Status200OK,
                nameof(Machine) + ResponseMessages.Updated,
                mapper.GetResult<Machine, MachineDto>(rightMachine));
        }
        catch (UnauthorizedAccessException ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<MachineDto>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<MachineDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<string>> DeleteAsync(Guid machineId)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var result = await machineRepository.GetAsync(machineId);
            if (result.IsLeft)
            {
                return new ApiResponse<string>(
                    StatusCodes.Status200OK,
                    $"Machine with ID {machineId} not found."
                );
            }

            var rightMachine = result.IfRight();

            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, rightMachine!.CustomerId);

            if (!string.IsNullOrWhiteSpace(rightMachine.ImageUrl))
            {
                await blobStorageService.DeleteBase64Async(rightMachine.ImageUrl);
            }
            await machineMaintenanceTask.DeleteByMachineIdAsync(machineId);
            await machineRepository.DeleteAsync(machineId);
            await cache.RemoveTrackedKeysAsync(CachePrefix);
            await transaction.CommitAsync();

            return new ApiResponse<string>(
                StatusCodes.Status200OK,
                nameof(Machine) + ResponseMessages.Deleted
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ApiResponse<IEnumerable<MachineJobSummaryDto>>> GetSummaryByIdAsync(
     Guid id,
     TimeRange range,
     DateTime? from = null,
     DateTime? to = null)
    {
        try
        {
            IEnumerable<MachineJobSummaryDto> emptyList = new List<MachineJobSummaryDto>();

            var (start, end) = from.HasValue && to.HasValue
                ? (from.Value, to.Value)
                : TimeRangeHelper.GetRange(range);

            // ✅ PERFORMANCE: Cache key includes all parameters
            var cacheKey = $"machine:summary:{id}:{range}:{start:yyyy-MM-dd}:{end:yyyy-MM-dd}";
            var cached = await cache.GetAsync<List<MachineJobSummaryDto>>(cacheKey);
            if (cached != null)
            {
                return new ApiResponse<IEnumerable<MachineJobSummaryDto>>(
                    StatusCodes.Status200OK,
                    "Machine summary fetched successfully (cached).",
                    cached
                );
            }

            var machineResult = await machineRepository.GetAsync(id);
            if (machineResult.IsLeft)
                return new ApiResponse<IEnumerable<MachineJobSummaryDto>>(StatusCodes.Status204NoContent, $"Machine with ID {id} not found.");

            var machine = machineResult.IfRight();
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, machine!.CustomerId);

            bool isCustomRange = from.HasValue && to.HasValue;
            bool includesToday = end.Date >= DateTime.UtcNow.Date;

            bool useDailyLogic = range == TimeRange.Daily || (isCustomRange && includesToday);

            List<MachineJobSummaryDto> summaries;

            if (!useDailyLogic)
            {
                var historicalUtilization = await _historicalRepository.GetByMachineIdAndDateRangeAsync(id, start, end);

                if (historicalUtilization == null || !historicalUtilization.Any())
                    return new ApiResponse<IEnumerable<MachineJobSummaryDto>>(StatusCodes.Status204NoContent, "No data found.", emptyList);

                summaries = MachineJobSummaryHelper.GetMachineJobSummaries(historicalUtilization).ToList();
            }
            else
            {
                var jobs = await _machineJobRepository.GetJobsByMachineIdAndDateRangeAsync(id, start, end);

                if (jobs == null || !jobs.Any())
                    return new ApiResponse<IEnumerable<MachineJobSummaryDto>>(StatusCodes.Status200OK, "No machine jobs found.", emptyList);

                summaries = MachineJobSummaryHelper.GetMachineJobSummariesFromJobList(jobs!, start, end).ToList();
            }

            // ✅ PERFORMANCE: Cache for 30 seconds (summary data changes frequently)
            await cache.SetAsync(cacheKey, summaries, TimeSpan.FromSeconds(30), CachePrefix);

            return new ApiResponse<IEnumerable<MachineJobSummaryDto>>(
                StatusCodes.Status200OK,
                "Machine summary fetched successfully.",
                summaries);
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<MachineJobSummaryDto>>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<MachineJobDto>> GetMachineDetailsByIdAsync(Guid machineId)
    {
        try
        {
            // ✅ PERFORMANCE: Cache machine details (30 seconds - data changes frequently)
            var cacheKey = $"machine:details:{machineId}";
            var cached = await cache.GetAsync<MachineJobDto>(cacheKey);
            if (cached != null)
            {
                return new ApiResponse<MachineJobDto>(
                    StatusCodes.Status200OK,
                    $"{nameof(MachineJobDto)} Record retrieved successfully (cached).",
                    cached
                );
            }

            using var scope = serviceProvider.CreateScope();
            using var scope2 = serviceProvider.CreateScope();

            var machineRepo = scope.ServiceProvider.GetRequiredService<IMachineRepository>();
            var machineJobRepo = scope.ServiceProvider.GetRequiredService<IMachineJobRepository>();
            var machineLogRepo = scope2.ServiceProvider.GetRequiredService<IMachineLogRepository>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

            //Validate machine existence
            var machineResult = await machineRepo.GetAsync(machineId);
            if (machineResult.IsLeft)
            {
                return new ApiResponse<MachineJobDto>(
                    StatusCodes.Status404NotFound,
                    $"Machine with id: {machineId} does not exist"
                );
            }

            var machine = machineResult.IfRight();

            // Validate customer access
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, machine!.CustomerId);

            // ✅ PERFORMANCE: Execute all database queries in parallel
            var machineJobsTask = machineJobRepo.GetByMachineIdAsync(machineId.ToString());
            var machineLogsTask = machineLogRepo.GetByMachineIdAsync(machineId);

            await Task.WhenAll(machineJobsTask, machineLogsTask);

            var machineJobs = await machineJobsTask;
            var machineLogsEnumerable = await machineLogsTask;
            
            // ✅ FIX: Convert to list for processing
            var machineLogs = machineLogsEnumerable?.ToList() ?? new List<MachineLog>();

            // ✅ OPTIMIZED: Filter duplicate activity segments efficiently using GroupBy for both closed and open logs
            var activitySegments = new List<ActivitySegment>();

            if (machineLogs.Count > 0)
            {
                // ✅ OPTIMIZATION: Separate closed and open logs for efficient processing
                var closedLogs = machineLogs.Where(l => l.End != null).ToList();
                var openLogs = machineLogs.Where(l => l.End == null).ToList();

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

                // ✅ OPTIMIZATION: For open logs, group by status and take most recent (already sorted by time)
                var openLogsByStatus = openLogs
                    .GroupBy(l => l.Status ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(l => l.Start ?? l.LastUpdateTime).First());

                // ✅ OPTIMIZATION: Combine deduplicated closed logs with deduplicated open logs, then sort once
                var allLogsToProcess = deduplicatedClosedLogs
                    .Concat(openLogsByStatus.Values)
                    .OrderBy(l => l.Start ?? l.LastUpdateTime)
                    .ToList();

                // ✅ OPTIMIZATION: Single pass to create activity segments
                activitySegments = allLogsToProcess.Select(log => new ActivitySegment
                {
                    Status = log.Status ?? "Unknown",
                    Start = log.Start ?? DateTime.MinValue,
                    End = log.End,
                    JobId = log.JobId ?? string.Empty,
                    Color = log.Color ?? "#CCCCCC",
                    UserName = log.UserName ?? "N/A",
                    CurrentProgram = log.CurrentProgram ?? string.Empty,
                    MainProgram = log.MainProgram ?? string.Empty,
                    IsRunning = log.End == null
                }).ToList();
            }

            float sumWeightedAvail = 0f, sumWeightedPerf = 0f, sumWeightedQual = 0f, sumPlannedSec = 0f;
            string userName = "N/A";
            Guid userIdToFetch = Guid.Empty;

            // ✅ OPTIMIZATION: Use LINQ aggregation instead of foreach loop
            if (machineJobs?.Count > 0)
            {
                var jobMetrics = machineJobs
                    .Where(j => j.Schedule != null)
                    .Select(job =>
                    {
                        var plannedDuration = job.Schedule!.PlannedEnd - job.Schedule.PlannedStart;
                        float plannedSec = (float)plannedDuration.TotalSeconds;

                        if (plannedSec <= 0)
                            return (PlannedSec: 0f, DowntimeSec: 0f, Job: job);

                        float downtimeSec = 0f;
                        if (job.DowntimeEvents?.Count > 0)
                        {
                            var plannedStart = job.Schedule.PlannedStart;
                            var plannedEnd = job.Schedule.PlannedEnd;

                            downtimeSec = job.DowntimeEvents.Sum(d =>
                            {
                                var start = d.StartTime < plannedStart ? plannedStart : d.StartTime;
                                var end = d.EndTime > plannedEnd ? plannedEnd : d.EndTime;
                                return (end > start) ? (float)(end - start).TotalSeconds : 0f;
                            });
                        }

                        return (PlannedSec: plannedSec, DowntimeSec: downtimeSec, Job: job);
                    })
                    .Where(m => m.PlannedSec > 0)
                    .ToList();

                foreach (var (plannedSec, downtimeSec, job) in jobMetrics)
                {
                    float availabilityRatio = Math.Clamp((plannedSec - downtimeSec) / plannedSec, 0f, 1f);
                    float RuntimeSeconds = Math.Max(plannedSec - downtimeSec, 0);

                    float performanceRatio = 0f;
                    if (job.Metrics?.TargetCycleTime > 0 && RuntimeSeconds > 0)
                    {
                        var goodCount = job.Quantities?.Good ?? 0;
                        performanceRatio = Math.Clamp(
                            (job.Metrics.TargetCycleTime * goodCount) / RuntimeSeconds,
                            0f, 1f
                        );
                    }

                    float qualityRatio = 0f;
                    var good = job.Quantities?.Good ?? 0;
                    var bad = job.Quantities?.Bad ?? 0;
                    var total = good + bad;
                    if (total > 0)
                    {
                        qualityRatio = (float)good / total;
                    }

                    sumWeightedAvail += availabilityRatio * plannedSec;
                    sumWeightedPerf += performanceRatio * plannedSec;
                    sumWeightedQual += qualityRatio * plannedSec;
                    sumPlannedSec += plannedSec;

                    if (userIdToFetch == Guid.Empty && job.OperatorId != Guid.Empty)
                    {
                        userIdToFetch = job.OperatorId;
                    }
                }

                // ✅ PERFORMANCE: Cache user lookups
                if (userIdToFetch != Guid.Empty)
                {
                    var userCacheKey = $"User:{userIdToFetch}";
                    var cachedUser = await cacheService.GetAsync<UserModel>(userCacheKey);
                    
                    if (cachedUser != null)
                    {
                        userName = $"{cachedUser.FirstName} {cachedUser.LastName}";
                    }
                    else
                    {
                        var response = await userService.GetUserByIdAsync(userIdToFetch);
                        if (response.StatusCode == StatusCodes.Status200OK && response.Data != null)
                        {
                            userName = $"{response.Data.FirstName} {response.Data.LastName}";
                            // Cache user for 5 minutes
                            await cacheService.SetAsync(userCacheKey, response.Data, TimeSpan.FromMinutes(5));
                        }
                    }
                }
            }

            float divisor = sumPlannedSec > 0f ? sumPlannedSec : 1f; 
            float overallAvailRatio = sumPlannedSec > 0f ? sumWeightedAvail / divisor : 0f;
            float overallPerfRatio = sumPlannedSec > 0f ? sumWeightedPerf / divisor : 0f;
            float overallQualRatio = sumPlannedSec > 0f ? sumWeightedQual / divisor : 0f;
            float overallOeeRatio = overallAvailRatio * overallPerfRatio * overallQualRatio;

            
            var dto = new MachineJobDto
            {
                MachineId =  machine.Id,
                MachineModel =machine.MachineModel,
                UserName = userName,
                ImageUrl =  machine.ImageUrl??string.Empty,
                Availability = (float)Math.Round(overallAvailRatio * 100, 2),
                Performance = (float)Math.Round(overallPerfRatio * 100, 2),
                Quality = (float)Math.Round(overallQualRatio * 100, 2),
                Oee = (float)Math.Round(overallOeeRatio * 100, 2),
                Activity = activitySegments
            };

            // ✅ PERFORMANCE: Cache result for 30 seconds
            await cache.SetAsync(cacheKey, dto, TimeSpan.FromSeconds(30), CachePrefix);

            return new ApiResponse<MachineJobDto>(
                StatusCodes.Status200OK,
                $"{nameof(MachineJobDto)} Record retrieved successfully.",
                dto
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<MachineJobDto>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<MachineJobDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while retrieving machine details: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<string>> CreateDefaultSettingsForMachineAsync(Guid machineId)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var result = await machineRepository.GetAsync(machineId);
            if (result.IsLeft)
            {
                return new ApiResponse<string>(
                    StatusCodes.Status404NotFound,
                    $"Machine with ID {machineId} not found."
                );
            }

            var machine = result.IfRight();

            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, machine!.CustomerId);

            var existingMachineSetting = await unitOfWork.MachineSettingRepository.GetByMachineIdAsync(machineId);
            if (existingMachineSetting != null)
            {
                return new ApiResponse<string>(
                    StatusCodes.Status409Conflict,
                    $"Default machine settings already exist for machine {machine.MachineName}."
                );
            }

            var defaultMachineSetting = new MachineSetting
            {
                MachineId = machine.Id,
                CycleStartInterlock = false,
                GuestLock = false,
                ReverseCSlockLogic = false,
                AutomaticPartsCounter = false,
                MaxFeedrate = 2500,
                MaxSpindleSpeed = 1200,
                StopTimelimit = null,
                PlannedProdusctionTime = null,
                MinElapsedCycleTime = null,
                Status = MachineSettingsStatus.Active,
                DownTimeReasons = new List<string>()
            };
            await unitOfWork.MachineSettingRepository.AddAsync(defaultMachineSetting);

            await transaction.CommitAsync();

            return new ApiResponse<string>(
                StatusCodes.Status201Created,
                $"Default settings created successfully for machine {machine.MachineName}."
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<string>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<string>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }
}




