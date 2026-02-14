namespace MMS.Application.Services.NoSql;

public class MachineMonitoringService(
    IMachineLogRepository repository,
    IOperationalDataRepository operationalDataRepository,
    IMachineStatusSettingRepository _statusSettingRepo,
    IMachineSettingRepository _machineSettingRepository,
    IMachineService machineService,
    IMachineJobRepository machineJobRepository,
    IUnitOfWork unitOfWork,
    IUserService userService,
    IServiceScopeFactory serviceScopeFactory,
    ICustomerDashboardSummaryService customerDashboardSummaryService,
    IRabbitMQProducer _producer,
    ICacheService cacheService,
    IMachineSensorRepository _machineSensorRepository,
    IHttpContextAccessor _httpContextAccessor,
    IDistributedLockService distributedLockService
    ) : IMachineMonitoringService
{

    // ✅ COLOR CACHE - Static so it persists across requests
    private static readonly ConcurrentDictionary<string, string> _colorCache = new();
    private static bool _colorCacheInitialized = false;
    private static readonly SemaphoreSlim _cacheInitLock = new(1, 1);

    private static readonly string[] AvailableColors =
    {
        "#F59E0B", "#EF4444", "#EC4899", "#8B5CF6", "#6366F1", "#3B82F6",
        "#06B6D4", "#14B8A6", "#10B981", "#22C55E", "#84CC16", "#F97316",
        "#E11D48", "#7C3AED", "#2563EB", "#0891B2"
    };

    #region Color Cache Methods

    /// <summary>
    /// Gets or assigns a permanent color for a Machine + Reason combination.
    /// Initializes cache from DB on first call.
    /// </summary>
    private async Task<string> GetOrAssignDowntimeColorAsync(Guid machineId, string downtimeReason)
    {
        // ✅ Initialize cache from database on first call
        if (!_colorCacheInitialized)
        {
            await InitializeColorCacheFromDatabaseAsync();
        }

        var cacheKey = $"{machineId}_{downtimeReason.ToLowerInvariant().Trim()}";

        // ✅ Return cached color if exists (PERMANENT)
        if (_colorCache.TryGetValue(cacheKey, out var cachedColor))
        {
            return cachedColor;
        }

        // ✅ Assign new unique color for this machine
        var machinePrefix = $"{machineId}_";
        var usedColors = _colorCache
            .Where(kvp => kvp.Key.StartsWith(machinePrefix))
            .Select(kvp => kvp.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newColor = AvailableColors.FirstOrDefault(c => !usedColors.Contains(c))
                       ?? AvailableColors[usedColors.Count % AvailableColors.Length];

        // Store in cache permanently
        _colorCache.TryAdd(cacheKey, newColor);

        return newColor;
    }

    private async Task InitializeColorCacheFromDatabaseAsync()
    {
        // Use semaphore to ensure only one thread initializes
        await _cacheInitLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_colorCacheInitialized) return;

            try
            {
                // Get all existing downtime logs with colors from database
                var existingLogs = await repository.GetDistinctDowntimeColorsAsync();

                foreach (var log in existingLogs)
                {
                    if (string.IsNullOrWhiteSpace(log.Status) || string.IsNullOrWhiteSpace(log.Color))
                        continue;

                    var cacheKey = $"{log.MachineId}_{log.Status.ToLowerInvariant().Trim()}";
                    _colorCache.TryAdd(cacheKey, log.Color);
                }

                Console.WriteLine($"🎨 Color cache initialized with {_colorCache.Count} mappings from database");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error initializing color cache: {ex.Message}");
            }

            _colorCacheInitialized = true;
        }
        finally
        {
            _cacheInitLock.Release();
        }
    }

    #endregion

    #region Helper Methods

    private string GetCurrentSource()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Items.TryGetValue("RequestSource", out var source) == true)
        {
            return source?.ToString() ?? "System";
        }
        return "System";
    }

    private async Task<MachineLogSignalRDto> CreateAndPublishLogDtoAsync(
        MachineLog log,
        MachineDto machine,
        UserModel? user,
        string? jobId)
    {
        try
        {
            // ✅ PERFORMANCE: Cache status summary (calculated in background, cached for 30 seconds)
            var summaryCacheKey = $"StatusSummary:{machine.CustomerId}";
            var statusSummary = await cacheService.GetAsync<Dictionary<string, int>>(summaryCacheKey);

            if (statusSummary == null)
            {
                statusSummary = await CalculateStatusSummary(machine.CustomerId ?? Guid.Empty);
                // Cache for 30 seconds (summary changes frequently but not instantly)
                await cacheService.SetAsync(summaryCacheKey, statusSummary, TimeSpan.FromSeconds(30));
            }

            var dto = new MachineLogSignalRDto(
                Id: log.Id,
                MachineId: machine.Id,
                CustomerId: machine.CustomerId ?? Guid.Empty,
                Name: machine.MachineName,
                UserId: user?.UserId ?? Guid.Empty,
                Color: log.Color,
                Status: log.Status,
                UserName: user != null ? $"{user.FirstName} {user.LastName}" : log.UserName ?? "N/A",
                JobId: jobId ?? string.Empty,
                StatusSummary: statusSummary
            );

            // ✅ PERFORMANCE: Parallelize independent operations
            var dashboardTask = customerDashboardSummaryService.UpsertCustomerDashboardSummaryAsync(dto);
            var publishTask = Task.Run(() => _producer.PublishMachineLogAsync(dto));

            await Task.WhenAll(dashboardTask, publishTask);

            return dto;
        }
        catch
        {
            throw; // Re-throw to let caller handle it
        }
    }

    private static Measurement? ExtractMeasurement(string type, OperationalDataDto opDataDto)
    {
        return type.ToLower() switch
        {
            "feedrate" => opDataDto.FeedRate,
            "spindlespeed" => opDataDto.SpindleSpeed,
            "temperature" => opDataDto.Temperature,
            "vibration" => opDataDto.Vibration,
            "powerconsumption" => opDataDto.PowerConsumption,
            "torque" => opDataDto.Torque,
            "coolantlevel" => opDataDto.CoolantLevel,
            "airpressure" => opDataDto.AirPressure,
            "spindlestatus" => new Measurement { Value = opDataDto.SpindleStatus ? 1 : 0, Unit = "status" },
            "cycletime" when opDataDto.CycleTime.HasValue => new Measurement
            {
                Value = (float)opDataDto.CycleTime.Value.TotalSeconds,
                Unit = "seconds"
            },
            _ => null
        };
    }

    private static Dictionary<string, object> ConvertOperationalDataToDictionary(OperationalData opData)
    {
        var dictionary = new Dictionary<string, object>();
        if (opData?.Measurement != null)
            dictionary[opData.Type] = opData.Measurement.Value;
        return dictionary;
    }

    #endregion

    #region Alert Context Builders

    private static AlertContext BuildMachineStatusAlertContext(
        MachineLog log,
        string machineName,
        string priority)
    {
        // ✅ OPTIMIZATION: Use StringBuilder for multiple string concatenations
        var operationalDataBuilder = new System.Text.StringBuilder($"Status: {log.Status}");

        if (log.Inputs?.Count > 0)
        {
            var inputsStr = string.Join(", ", log.Inputs.Select(i => $"{i.InputKey}: {i.Signals}"));
            operationalDataBuilder.Append($", Inputs: [{inputsStr}]");
        }

        if (!string.IsNullOrWhiteSpace(log.JobId))
            operationalDataBuilder.Append($", JobId: {log.JobId}");

        if (!string.IsNullOrWhiteSpace(log.UserName) && log.UserName != "N/A")
            operationalDataBuilder.Append($", Operator: {log.UserName}");

        return new AlertContext
        {
            MachineId = log.MachineId,
            CustomerId = log.CustomerId,
            MachineName = machineName,
            Status = log.Status,
            OperationalData = operationalDataBuilder.ToString(),
            Priority = priority,
            TriggeredAt = log.Start,
            Source = log.Source,
            JobId = log.JobId,
            UserName = log.UserName
        };
    }

    private static AlertContext BuildSensorAlertContext(
        MachineSensorLog log,
        string machineName,
        Guid customerId,
        string priority)
    {
        var operationalData = string.Join(", ",
            log.SensorReading?.Select(r => $"{r.Key}: {r.Value} {r.Unit}") ?? Enumerable.Empty<string>());

        return new AlertContext
        {
            MachineId = log.MachineId,
            CustomerId = customerId,
            MachineName = machineName,
            Status = log.SensorStatus.ToString(),
            OperationalData = operationalData,
            Priority = priority,
            TriggeredAt = log.DateTime
        };
    }

    private static AlertContext BuildOperationalDataAlertContext(
        OperationalData opData,
        string machineName,
        string priority)
    {
        return new AlertContext
        {
            MachineId = opData.MachineId,
            CustomerId = opData.CustomerId,
            MachineName = machineName,
            Status = "Operational Alert",
            OperationalData = $"{opData.Type}: {opData.Measurement?.Value} {opData.Measurement?.Unit}",
            Priority = priority,
            TriggeredAt = opData.Timestamp,
            Source = opData.Source
        };
    }

    #endregion

    #region Main Public Methods

    public async Task<ApiResponse<MachineLogSignalRDto>> ProcessMonitoringAsync(MachineMonitoring input)
    {
        var exactTime = DateTime.UtcNow;

        Console.WriteLine($"🔍 DEBUG: Looking up machine by name: '{input.MachineName}'");

        // ✅ PERFORMANCE: Machine lookup is now cached in MachineService
        var machineResponse = await machineService.GetByMachineName(input.MachineName!);
        if (machineResponse.StatusCode != StatusCodes.Status200OK || machineResponse.Data == null)
        {
            Console.WriteLine($"❌ DEBUG: Machine not found for name: '{input.MachineName}'");
            return new ApiResponse<MachineLogSignalRDto>(StatusCodes.Status400BadRequest, "Machine not found.");
        }

        var machine = machineResponse.Data;
        Console.WriteLine($"✅ DEBUG: Found machine - Name: '{machine.MachineName}', ID: '{machine.Id}', CustomerId: '{machine.CustomerId}'");

        // ✅ PERFORMANCE: Route to appropriate handler (locks handled inside)
        if (input.Type == "DownTime" && !string.IsNullOrWhiteSpace(input.Reason))
            return await HandleManualDowntimeAsync(machine, input, exactTime);

        return await HandleSignalBasedMonitoringAsync(machine, input, exactTime);
    }

    public async Task<ApiResponse<string>> ProcessSensorLogAsync(MachinemonitoringDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.SensorName))
                return new ApiResponse<string>(StatusCodes.Status400BadRequest, "SensorName is required.");

            if (string.IsNullOrWhiteSpace(dto.MachineStatus))
                return new ApiResponse<string>(StatusCodes.Status400BadRequest, "MachineStatus is required.");

            var sensor = await _machineSensorRepository.GetByNameAsync(dto.SensorName);
            if (sensor == null)
                return new ApiResponse<string>(StatusCodes.Status404NotFound, $"Sensor not found for name '{dto.SensorName}'");

            if (!sensor.MachineId.HasValue)
                return new ApiResponse<string>(StatusCodes.Status400BadRequest, $"Sensor '{dto.SensorName}' is not associated with any machine.");

            if (!System.Enum.TryParse<SensorStatus>(dto.MachineStatus, true, out var sensorStatus))
            {
                var validStatuses = string.Join(", ", System.Enum.GetNames(typeof(SensorStatus)));
                return new ApiResponse<string>(StatusCodes.Status400BadRequest, $"Invalid MachineStatus '{dto.MachineStatus}'. Valid values: {validStatuses}");
            }

            var sensorReadings = dto.SensorReading.Select(r => new SensorReading
            {
                Key = r.Key,
                Value = r.Value,
                Unit = r.Unit
            }).ToList();

            var newLog = new MachineSensorLog
            {
                Id = Guid.NewGuid(),
                SensorName = sensor.Name,
                SensorId = sensor.Id,
                MachineId = sensor.MachineId.Value,
                SensorStatus = sensorStatus,
                DateTime = DateTime.UtcNow,
                SensorReading = sensorReadings
            };

            await unitOfWork.MachineSensorLogRepository.AddAsync(newLog);

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var scopedAlertService = scope.ServiceProvider.GetRequiredService<IAlertNotificationService>();
                    var scopedAlertRuleRepo = scope.ServiceProvider.GetRequiredService<IAlertRuleRepository>();
                    var scopedMachineService = scope.ServiceProvider.GetRequiredService<IMachineService>();

                    await CheckAndTriggerSensorAlertsAsync(newLog, scopedAlertService, scopedAlertRuleRepo, scopedMachineService);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error in CheckAndTriggerSensorAlertsAsync for Sensor: {dto.SensorName}. Exception: {ex.Message}");
                }
            });

            return new ApiResponse<string>(StatusCodes.Status200OK, $"Sensor log processed successfully for sensor '{dto.SensorName}'.");
        }
        catch (Exception)
        {
            return new ApiResponse<string>(StatusCodes.Status500InternalServerError, "An error occurred while processing the sensor log.");
        }
    }

    public async Task<ApiResponse<string>> ProcessOperationalDataAsync(CreateOperationalData dto)
    {
        try
        {
            if (dto?.OperationalData == null || !dto.OperationalData.Any())
                return new ApiResponse<string>(StatusCodes.Status400BadRequest, "No operational data provided.");

            if (string.IsNullOrWhiteSpace(dto.MachineName))
                return new ApiResponse<string>(StatusCodes.Status400BadRequest, "MachineName is required.");

            if (string.IsNullOrWhiteSpace(dto.Type))
                return new ApiResponse<string>(StatusCodes.Status400BadRequest, "Type is required.");

            var machineResult = await machineService.GetByMachineName(dto.MachineName);
            if (machineResult.StatusCode != StatusCodes.Status200OK || machineResult.Data == null)
                return new ApiResponse<string>(StatusCodes.Status404NotFound, $"Machine not found for name '{dto.MachineName}'");

            var machine = machineResult.Data;
            var customerId = machine.CustomerId ?? Guid.Empty;

            if (customerId == Guid.Empty)
                return new ApiResponse<string>(StatusCodes.Status400BadRequest, $"Machine '{dto.MachineName}' does not have an associated customer.");

            var currentSource = GetCurrentSource();
            var now = DateTime.UtcNow;
            var operationalDataList = new List<OperationalData>();

            foreach (var opDataDto in dto.OperationalData)
            {
                var measurement = ExtractMeasurement(dto.Type, opDataDto);

                if (measurement == null)
                {
                    return new ApiResponse<string>(StatusCodes.Status400BadRequest,
                        $"Invalid Type '{dto.Type}'. Supported types: feedrate, spindlespeed, temperature, vibration, powerconsumption, torque, coolantlevel, airpressure, spindlestatus, cycletime");
                }

                operationalDataList.Add(new OperationalData
                {
                    Id = Guid.NewGuid().ToString(),
                    MachineId = machine.Id,
                    CustomerId = customerId,
                    Timestamp = now,
                    Type = dto.Type.ToLower(),
                    Source = currentSource,
                    Measurement = new Measurement { Value = measurement.Value, Unit = measurement.Unit }
                });
            }

            if (!operationalDataList.Any())
                return new ApiResponse<string>(StatusCodes.Status400BadRequest, $"No measurement found for type '{dto.Type}' in the provided operational data.");

            await operationalDataRepository.AddRangeAsync(operationalDataList);

            string machineName = machine.MachineName ?? dto.MachineName;

            _ = Task.Run(async () =>
            {
                foreach (var opData in operationalDataList)
                {
                    try
                    {
                        using var scope = serviceScopeFactory.CreateScope();
                        var scopedAlertService = scope.ServiceProvider.GetRequiredService<IAlertNotificationService>();
                        var scopedAlertRuleRepo = scope.ServiceProvider.GetRequiredService<IAlertRuleRepository>();

                        await CheckAndTriggerOperationalAlertsAsync(opData, machineName, scopedAlertService, scopedAlertRuleRepo);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Alert evaluation error for operational data: {ex.Message}");
                    }
                }
            });

            return new ApiResponse<string>(StatusCodes.Status200OK, $"Operational data processed successfully. Saved {operationalDataList.Count} {dto.Type} measurement(s).");
        }
        catch (Exception)
        {
            return new ApiResponse<string>(StatusCodes.Status500InternalServerError, "An error occurred while processing operational data.");
        }
    }

    // ✅ FIX: Added distributed lock and proper duplicate prevention for offline logs
    // ✅ CRITICAL FIX: Use same lock key as HandleSignalBasedMonitoringAsync to prevent race conditions
    public async Task MarkMachineOfflineAsync(Guid machineId)
    {
        await cacheService.CancelMachineHeartbeatAsync(machineId.ToString());

        var machineResponse = await machineService.GetByIdAsync(machineId);
        if (machineResponse?.Data == null)
        {
            return;
        }

        var machine = machineResponse.Data;
        var exactTime = DateTime.UtcNow;

        // ✅ CRITICAL FIX: Use SAME lock key as HandleSignalBasedMonitoringAsync to prevent concurrent execution
        // This ensures offline and machine log processing cannot overlap
        var lockKey = $"machine:monitor:{machine.Id}";
        var lockExpiry = TimeSpan.FromSeconds(10);
        var lockWaitTime = TimeSpan.FromSeconds(1);

        using var distributedLock = await distributedLockService.AcquireLockAsync(
            lockKey,
            lockExpiry,
            lockWaitTime);

        if (distributedLock == null)
        {
            // Another process is already handling this machine offline, skip
            return;
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            // ✅ FIX: Get ALL open logs (not just last one) to handle duplicates
            var openLogs = await repository.GetAllOpenLogsByMachineIdForUpdateAsync(machine.Id);

            // ✅ FIX: Check if offline log already exists (prevent duplicates)
            var existingOfflineLog = openLogs
                .FirstOrDefault(l => l.Status.Equals(MachineStatus.Offline.ToString(), StringComparison.OrdinalIgnoreCase));

            if (existingOfflineLog != null)
            {
                // ✅ FIX: Update LastUpdateTime to show activity (heartbeat)
                existingOfflineLog.LastUpdateTime = exactTime;
                await repository.UpdateAsync(existingOfflineLog);
                await transaction.CommitAsync();
                await cacheService.SetMachineHeartbeatAsync(machine.Id.ToString());
                await cacheService.RemoveTrackedKeysAsync("Customer");
                return;
            }

            // ✅ CRITICAL FIX: Check if machine has active non-offline logs (machine is online) - if so, don't create offline log
            // This prevents creating offline log when machine is actually sending logs
            // Check openLogs first (more efficient, already loaded)
            var activeNonOfflineLog = openLogs
                .FirstOrDefault(l => !l.Status.Equals(MachineStatus.Offline.ToString(), StringComparison.OrdinalIgnoreCase));

            if (activeNonOfflineLog != null)
            {
                // ✅ FIX: Check if the active log is recent (within heartbeat TTL)
                // If heartbeat expired, it means no signals for 3 minutes. If log is also >3 minutes old, machine is offline
                var lastActivityTime = activeNonOfflineLog.LastUpdateTime;
                var timeSinceLastUpdate = exactTime - lastActivityTime;
                var heartbeatTtl = TimeSpan.FromMinutes(3); // Match RedisCacheService defaultHeartbeatTtl
                var gracePeriod = TimeSpan.FromSeconds(30); // Grace period for processing/network delays (30 seconds)

                // If log was updated recently (within heartbeat TTL - grace period), machine is still active
                // If log is stale (>= heartbeat TTL - grace period), machine should be marked offline
                if (timeSinceLastUpdate < (heartbeatTtl - gracePeriod))
                {
                    // Machine has recent active logs (within last ~2.5 minutes) - machine is still online
                    // Reset heartbeat to prevent immediate re-expiration
                    await transaction.CommitAsync();
                    await cacheService.SetMachineHeartbeatAsync(machine.Id.ToString());
                    await cacheService.RemoveTrackedKeysAsync("Customer");
                    return;
                }
                // If log is old (>= 2.5 minutes) and heartbeat expired, continue to create offline log
                // This means the machine is truly offline, even though there's an old active log
            }

            // ✅ FIX: Close ALL open logs atomically before creating offline log
            var logsToClose = openLogs.Where(l => l.End == null).ToList();
            if (logsToClose.Count > 0)
            {
                var currentSource = GetCurrentSource();
                await repository.CloseMultipleLogsAsync(logsToClose, exactTime, currentSource);
            }

            // ✅ FIX: Final check - verify no offline log was created by another request
            var finalCheckLog = await repository.GetLastOpenLogByMachineId(machine.Id);
            if (finalCheckLog != null && finalCheckLog.Status.Equals(MachineStatus.Offline.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                await transaction.CommitAsync();
                await cacheService.SetMachineHeartbeatAsync(machine.Id.ToString());
                await cacheService.RemoveTrackedKeysAsync("Customer");
                return;
            }

            // ✅ FIX: Get user from last closed log if available
            UserModel? user = null;
            if (logsToClose.Count > 0)
            {
                var lastClosedLog = logsToClose.OrderByDescending(l => l.LastUpdateTime).FirstOrDefault();
                if (lastClosedLog != null && lastClosedLog.UserId != Guid.Empty)
                {
                    var userResult = await userService.GetUserByIdAsync(lastClosedLog.UserId);
                    if (userResult?.StatusCode == 200)
                        user = userResult.Data;
                }
            }

            var newLog = new MachineLog
            {
                Id = Guid.NewGuid().ToString(),
                MachineId = machine.Id,
                CustomerId = machine.CustomerId ?? Guid.Empty,
                Start = exactTime,
                LastUpdateTime = exactTime,
                Status = MachineStatus.Offline.ToString(),
                Color = "#000000",
                JobId = null!,
                MainProgram = null!,
                End = null,
                UserId = user?.UserId ?? Guid.Empty,
                UserName = user != null ? $"{user.FirstName} {user.LastName}" : "N/A"
            };

            await repository.AddMachineLogAsync(newLog);
            await repository.SaveChangesAsync();
            await transaction.CommitAsync();

            await cacheService.SetMachineHeartbeatAsync(machine.Id.ToString());
            await cacheService.RemoveTrackedKeysAsync("Customer");

            // ✅ PERFORMANCE: Fire-and-forget for SignalR/RabbitMQ (not critical for response)
            _ = Task.Run(async () =>
            {
                try
                {
                    await CreateAndPublishLogDtoAsync(newLog, machine, user, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in CreateAndPublishLogDtoAsync for offline log: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"❌ Error in MarkMachineOfflineAsync for MachineId: {machine.Id}. Exception: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Private Monitoring Handlers

    private async Task<ApiResponse<MachineLogSignalRDto>> HandleManualDowntimeAsync(
        MachineDto machine,
        MachineMonitoring input,
        DateTime exactTime)
    {
        // ✅ PERFORMANCE: Reduced lock wait time from 5s to 1s (most operations are fast)
        var lockKey = $"machine:monitor:{machine.Id}";
        var lockExpiry = TimeSpan.FromSeconds(10);
        var lockWaitTime = TimeSpan.FromSeconds(1); // ✅ OPTIMIZED: Reduced from 5s to 1s

        using var distributedLock = await distributedLockService.AcquireLockAsync(
            lockKey,
            lockExpiry,
            lockWaitTime);

        if (distributedLock == null)
        {
            return new ApiResponse<MachineLogSignalRDto>(
                StatusCodes.Status409Conflict,
                "Another operation is currently processing this machine. Please try again.");
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            // ✅ PERFORMANCE: Cache machine settings (they change infrequently)
            var settingCacheKey = $"MachineSetting:{machine.Id}";
            var setting = await cacheService.GetAsync<MachineSetting>(settingCacheKey);

            if (setting == null)
            {
                setting = await _machineSettingRepository.GetByMachineIdAsync(machine.Id);
                if (setting != null)
                {
                    // Cache for 10 minutes
                    await cacheService.SetAsync(settingCacheKey, setting, TimeSpan.FromMinutes(10));
                }
            }

            var normalizedInputReason = input.Reason?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedInputReason))
            {
                await transaction.RollbackAsync();
                return new ApiResponse<MachineLogSignalRDto>(StatusCodes.Status400BadRequest, "Downtime reason cannot be empty.");
            }

            var matchingReason = setting.DownTimeReasons
                .FirstOrDefault(r => r.Equals(normalizedInputReason, StringComparison.OrdinalIgnoreCase));

            // ✅ NEW FUNCTIONALITY: If reason doesn't match settings, still save log but with type "other"
            bool isUnmatchedReason = matchingReason == null;
            string logType = isUnmatchedReason ? "other" : "DownTimelog";
            string statusToUse = isUnmatchedReason ? normalizedInputReason : matchingReason;

            var currentSource = GetCurrentSource();

            // ✅ Get fresh open logs INSIDE lock and transaction
            var openLogs = await repository.GetAllOpenLogsByMachineIdForUpdateAsync(machine.Id);

            // ✅ CRITICAL FIX: Check if machine is currently offline - if so, close offline log first
            // This prevents overlapping offline and downtime logs
            var offlineLog = openLogs
                .FirstOrDefault(l => l.Status.Equals(MachineStatus.Offline.ToString(), StringComparison.OrdinalIgnoreCase));

            if (offlineLog != null)
            {
                // Machine is coming back online with downtime reason - close offline log first
                await repository.CloseMultipleLogsAsync(new List<MachineLog> { offlineLog }, exactTime, currentSource);
                await cacheService.RemoveTrackedKeysAsync("Customer");
                // Remove from openLogs list to avoid double-closing
                openLogs = openLogs.Where(l => l.Id != offlineLog.Id).ToList();
            }

            // ✅ Check for same status (race condition prevention)
            var existingLogWithSameStatus = openLogs
                .FirstOrDefault(l => l.Status.Equals(statusToUse, StringComparison.OrdinalIgnoreCase));

            if (existingLogWithSameStatus != null)
            {
                // Update LastUpdateTime to show activity (heartbeat)
                existingLogWithSameStatus.LastUpdateTime = exactTime;
                await repository.UpdateAsync(existingLogWithSameStatus);

                await transaction.CommitAsync();
                //await cacheService.SetMachineHeartbeatAsync(machine.Id.ToString());

                // ✅ FIX: Invalidate customer dashboard cache on log updates
                await cacheService.RemoveTrackedKeysAsync("Customer");

                return new ApiResponse<MachineLogSignalRDto>(StatusCodes.Status200OK,
                    $"Same status '{statusToUse}' detected — previous log is continuing.");
            }

            // ✅ Close ALL remaining open logs atomically
            var logsToClose = openLogs.Where(l => l.End == null).ToList();
            if (logsToClose.Any())
            {
                await repository.CloseMultipleLogsAsync(logsToClose, exactTime, currentSource);
                // ✅ FIX: Invalidate customer dashboard cache when logs are closed
                await cacheService.RemoveTrackedKeysAsync("Customer");
            }

            // ✅ Final check: Verify no new log was created by another request (shouldn't happen with lock, but double-check)
            var recentLog = await repository.GetLastOpenLogByMachineId(machine.Id);
            if (recentLog != null && recentLog.Status.Equals(statusToUse, StringComparison.OrdinalIgnoreCase))
            {
                await transaction.CommitAsync();
                //await cacheService.SetMachineHeartbeatAsync(machine.Id.ToString());

                // ✅ FIX: Invalidate customer dashboard cache on log updates
                await cacheService.RemoveTrackedKeysAsync("Customer");

                return new ApiResponse<MachineLogSignalRDto>(StatusCodes.Status200OK,
                    $"Same status '{statusToUse}' detected — previous log is continuing.");
            }

            // ✅ PERFORMANCE: Get active job (optimized)
            var activeJob = await machineJobRepository.GetActiveJobByMachineIdAsync(input.MachineName!.ToString(), exactTime);

            // ✅ PERFORMANCE: Cache user lookups (users don't change frequently)
            UserModel? user = null;
            if (activeJob != null)
            {
                var userCacheKey = $"User:{activeJob.OperatorId}";
                user = await cacheService.GetAsync<UserModel>(userCacheKey);

                if (user == null)
                {
                    var userResult = await userService.GetUserByIdAsync(activeJob.OperatorId);
                    if (userResult.StatusCode == 200)
                    {
                        user = userResult.Data;
                        // Cache user for 5 minutes
                        if (user != null)
                        {
                            await cacheService.SetAsync(userCacheKey, user, TimeSpan.FromMinutes(5));
                        }
                    }
                }
            }

            // ✅ Get color for matched reasons, use smart defaults for unmatched
            string logColor;
            if (isUnmatchedReason)
            {
                // ✅ NEW LOGIC: Smart color assignment for unmatched reasons
                var normalizedReason = normalizedInputReason.ToLowerInvariant();

                // Yellow color: Program Complete, Waiting for Operator Input, Idle - Stop, Idle - Machine Warning, Op. Stop
                if (normalizedReason.Contains("program complete") ||
                    normalizedReason.Equals("program complete", StringComparison.OrdinalIgnoreCase) ||
                    normalizedReason.Contains("waiting for operator input") ||
                    normalizedReason.Contains("waiting for operator") ||
                    normalizedReason.Equals("waiting for operator input", StringComparison.OrdinalIgnoreCase) ||
                    normalizedReason.Contains("idle - stop") ||
                    normalizedReason.Contains("idle stop") ||
                    normalizedReason.Equals("idle - stop", StringComparison.OrdinalIgnoreCase) ||
                    normalizedReason.Equals("idle stop", StringComparison.OrdinalIgnoreCase) ||
                    normalizedReason.Contains("idle - machine warning") ||
                    normalizedReason.Contains("idle machine warning") ||
                    normalizedReason.Contains("idle - warning") ||
                    normalizedReason.Equals("idle - machine warning", StringComparison.OrdinalIgnoreCase) ||
                    normalizedReason.Equals("idle machine warning", StringComparison.OrdinalIgnoreCase) ||
                    normalizedReason.Equals("idle - warning", StringComparison.OrdinalIgnoreCase) ||
                    normalizedReason.Contains("op. stop") ||
                    normalizedReason.Contains("op stop") ||
                    normalizedReason.Equals("op. stop", StringComparison.OrdinalIgnoreCase) ||
                    normalizedReason.Equals("op stop", StringComparison.OrdinalIgnoreCase))
                {
                    logColor = "#EAB308"; // Yellow color
                }
                // Orange color: Idle - Alarm, Machine Alarm
                else if (normalizedReason.Contains("idle - alarm") ||
                         normalizedReason.Contains("idle alarm") ||
                         normalizedReason.Contains("idle - machine alarm") ||
                         normalizedReason.Equals("idle - alarm", StringComparison.OrdinalIgnoreCase) ||
                         normalizedReason.Equals("idle alarm", StringComparison.OrdinalIgnoreCase) ||
                         normalizedReason.Equals("idle - machine alarm", StringComparison.OrdinalIgnoreCase) ||
                         normalizedReason.Contains("machine alarm") ||
                         (normalizedReason.Contains("alarm") && !normalizedReason.Contains("idle")))
                {
                    logColor = "#F97316"; // Orange color
                }
                // Red color: Setup-In Cycle, Setup-Uncategorized, un-categorized
                else if (normalizedReason.Contains("setup-in cycle") ||
                         normalizedReason.Contains("setup in cycle") ||
                         normalizedReason.Equals("setup-in cycle", StringComparison.OrdinalIgnoreCase) ||
                         normalizedReason.Equals("setup in cycle", StringComparison.OrdinalIgnoreCase) ||
                         normalizedReason.Contains("setup-uncategorized") ||
                         normalizedReason.Contains("setup uncategorized") ||
                         normalizedReason.Contains("setup-un-categorized") ||
                         normalizedReason.Contains("setup un-categorized") ||
                         normalizedReason.Equals("setup-uncategorized", StringComparison.OrdinalIgnoreCase) ||
                         normalizedReason.Equals("setup uncategorized", StringComparison.OrdinalIgnoreCase) ||
                         normalizedReason.Equals("setup-un-categorized", StringComparison.OrdinalIgnoreCase) ||
                         normalizedReason.Contains("un-categorized") ||
                         normalizedReason.Contains("un categorized") ||
                         normalizedReason.Contains("Idle-Uncategorized") ||
                         normalizedReason.Equals("un-categorized", StringComparison.OrdinalIgnoreCase))               
                {
                    logColor = "#EF4444"; // Red color
                }
                // Black color: Offline
                else if (normalizedReason.Contains("offline") ||
                         normalizedReason.Equals("offline", StringComparison.OrdinalIgnoreCase))
                {
                    logColor = "#000000"; // Black color
                }
                // Green color: cycle/running states
                else if (normalizedReason.Contains("cycle") ||
                    normalizedReason.Contains("running") ||
                    normalizedReason.Contains("in cycle") ||
                    normalizedReason.Contains("operating") ||
                    normalizedReason.Contains("production"))
                {
                    logColor = "#22C55E"; // Green color
                }
                else
                {
                    logColor = "#808080"; // Default gray color for other unmatched reasons
                }
            }
            else
            {
                logColor = await GetOrAssignDowntimeColorAsync(machine.Id, matchingReason!);
            }

            var downtimeLog = new MachineLog
            {
                Id = Guid.NewGuid().ToString(),
                Type = logType, // "other" for unmatched reasons, "DownTimelog" for matched
                MachineId = machine.Id,
                CustomerId = machine.CustomerId ?? Guid.Empty,
                JobId = activeJob?.Id!,
                Status = statusToUse!, // Use input reason for unmatched, matched reason for matched
                Color = logColor,
                Start = exactTime,
                End = null,
                Source = currentSource,
                LastUpdateTime = exactTime,
                Inputs = new List<MachineInput>(),
                MainProgram = activeJob?.MainProgram!
            };

            if (user != null)
            {
                downtimeLog.UserId = user.UserId;
                downtimeLog.UserName = $"{user.FirstName} {user.LastName}";
            }

            await repository.AddMachineLogAsync(downtimeLog);
            await repository.SaveChangesAsync();

            await transaction.CommitAsync();
            //await cacheService.SetMachineHeartbeatAsync(machine.Id.ToString());

            // ✅ FIX: Invalidate customer dashboard cache when machine log is created/updated
            // This ensures "N/A" machine counts are accurate
            await cacheService.RemoveTrackedKeysAsync("Customer");

            // Invalidate status summary cache to ensure fresh calculation
            var summaryCacheKey = $"StatusSummary:{machine.CustomerId}";
            await cacheService.RemoveAsync(summaryCacheKey);

            var logTypeMessage = isUnmatchedReason
                ? $"✅ Manual log created with unmatched reason (type: 'other'). Starting background alert processing for MachineId: {machine.Id}, Status: {statusToUse}"
                : $"✅ Manual downtime log created successfully. Starting background alert processing for MachineId: {machine.Id}, Status: {statusToUse}";
            Console.WriteLine(logTypeMessage);

            // ✅ PERFORMANCE: Fire-and-forget heavy operations (dashboard summary, SignalR, RabbitMQ, alerts)
            // Don't wait for these - they're not critical for the response
            _ = Task.Run(async () =>
            {
                try
                {
                    var dto = await CreateAndPublishLogDtoAsync(downtimeLog, machine, user, activeJob?.Id.ToString());

                    // Create a NEW scope that won't be disposed with the request
                    using var alertScope = serviceScopeFactory.CreateScope();

                    var scopedAlertService = alertScope.ServiceProvider.GetRequiredService<IAlertNotificationService>();
                    var scopedAlertRuleRepo = alertScope.ServiceProvider.GetRequiredService<IAlertRuleRepository>();

                    await CheckAndTriggerMachineStatusAlertsAsync(downtimeLog, machine.MachineName, scopedAlertService, scopedAlertRuleRepo);
                }
                catch (Exception ex)
                {
                    // Log but don't fail the request
                    Console.WriteLine($"❌ Error in background processing for manual downtime - MachineId: {machine.Id}. Exception: {ex.Message}");
                    Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                }
            });

            // Calculate fresh status summary for the response (cache was invalidated above)
            var statusSummary = await CalculateStatusSummary(machine.CustomerId ?? Guid.Empty);
            // Cache for 30 seconds
            await cacheService.SetAsync(summaryCacheKey, statusSummary, TimeSpan.FromSeconds(30));

            // Return DTO with status summary
            var quickDto = new MachineLogSignalRDto(
                Id: downtimeLog.Id,
                MachineId: machine.Id,
                CustomerId: machine.CustomerId ?? Guid.Empty,
                Name: machine.MachineName,
                UserId: user?.UserId ?? Guid.Empty,
                Color: downtimeLog.Color,
                Status: downtimeLog.Status,
                UserName: user != null ? $"{user.FirstName} {user.LastName}" : "N/A",
                JobId: activeJob?.Id ?? string.Empty,
                StatusSummary: statusSummary
            );

            var responseMessage = isUnmatchedReason
                ? $"Manual log created with unmatched reason (type: 'other'): {statusToUse}. Note: This reason is not configured in machine settings."
                : $"Manual downtime log created with reason: {statusToUse}";

            return new ApiResponse<MachineLogSignalRDto>(StatusCodes.Status200OK,
                responseMessage, quickDto);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<MachineLogSignalRDto>(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    private async Task<ApiResponse<MachineLogSignalRDto>> HandleSignalBasedMonitoringAsync(
        MachineDto machine,
        MachineMonitoring input,
        DateTime exactTime)
    {
        var statusSettingCacheKey = $"MachineStatusSetting:{machine.Id}";
        var setting = await cacheService.GetAsync<MachineStatusSetting>(statusSettingCacheKey);

        if (setting == null)
        {
            setting = await _statusSettingRepo.GetByMachineIdAsync(machine.Id);
            if (setting != null)
            {
                // Cache for 10 minutes
                await cacheService.SetAsync(statusSettingCacheKey, setting, TimeSpan.FromMinutes(10));
            }
        }

        if (setting?.Inputs == null)
            return new ApiResponse<MachineLogSignalRDto>(StatusCodes.Status400BadRequest, "No status settings found.");

        var match = setting.Inputs.FirstOrDefault(i =>
            i.Signals?.Equals(input.Signals, StringComparison.OrdinalIgnoreCase) == true);

        if (match == null)
            return new ApiResponse<MachineLogSignalRDto>(StatusCodes.Status400BadRequest, $"No matching status for signal: {input.Signals}");

        var currentStatus = string.IsNullOrWhiteSpace(match.Status)
            ? MachineStatus.UnCategorized.ToString()
            : match.Status;

        var lockKey = $"machine:monitor:{machine.Id}";
        var lockExpiry = TimeSpan.FromSeconds(10);
        var lockWaitTime = TimeSpan.FromSeconds(1); // ✅ OPTIMIZED: Reduced from 5s to 1s

        using var distributedLock = await distributedLockService.AcquireLockAsync(
            lockKey,
            lockExpiry,
            lockWaitTime);

        if (distributedLock == null)
        {
            return new ApiResponse<MachineLogSignalRDto>(
                StatusCodes.Status409Conflict,
                "Another operation is currently processing this machine. Please try again.");
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var currentSource = GetCurrentSource();

            var openLogs = await repository.GetAllOpenLogsByMachineIdForUpdateAsync(machine.Id);

            // ✅ CRITICAL FIX: Check if machine is currently offline - if so, close offline log first
            // This prevents overlapping offline and machine status logs
            var offlineLog = openLogs
                .FirstOrDefault(l => l.Status.Equals(MachineStatus.Offline.ToString(), StringComparison.OrdinalIgnoreCase));

            if (offlineLog != null)
            {
                // Machine is coming back online - close offline log first
                await repository.CloseMultipleLogsAsync(new List<MachineLog> { offlineLog }, exactTime, currentSource);
                await cacheService.RemoveTrackedKeysAsync("Customer");
                // Remove from openLogs list to avoid double-closing
                openLogs = openLogs.Where(l => l.Id != offlineLog.Id).ToList();
            }

            // ✅ MANUAL DOWNTIME PRIORITY: If a manual downtime log is active, ignore signals
            var openDowntimeLog = openLogs.FirstOrDefault(l =>
                !string.IsNullOrWhiteSpace(l.Type) &&
                (l.Type.Equals(MMS.Application.Utils.MachineLogMetricsCalculator.LOG_TYPE_DOWNTIME, StringComparison.OrdinalIgnoreCase) ||
                 l.Type.Equals("other", StringComparison.OrdinalIgnoreCase)));

            if (openDowntimeLog != null)
            {
                openDowntimeLog.LastUpdateTime = exactTime;
                await repository.UpdateAsync(openDowntimeLog);

                await transaction.CommitAsync();
                //await cacheService.SetMachineHeartbeatAsync(machine.Id.ToString());
                await cacheService.RemoveTrackedKeysAsync("Customer");

                return new ApiResponse<MachineLogSignalRDto>(StatusCodes.Status200OK,
                    "Manual downtime is active — signal update ignored.");
            }

            var existingLogWithSameStatus = openLogs
                .FirstOrDefault(l => l.Status.Equals(currentStatus, StringComparison.OrdinalIgnoreCase));

            if (existingLogWithSameStatus != null)
            {
                // Update LastUpdateTime to show activity
                existingLogWithSameStatus.LastUpdateTime = exactTime;
                await repository.UpdateAsync(existingLogWithSameStatus);

                await transaction.CommitAsync();
                //await cacheService.SetMachineHeartbeatAsync(machine.Id.ToString());

                // ✅ FIX: Invalidate customer dashboard cache on log updates
                await cacheService.RemoveTrackedKeysAsync("Customer");

                return new ApiResponse<MachineLogSignalRDto>(StatusCodes.Status200OK,
                    "Same status detected — previous log is continuing.");
            }

            // ✅ Close ALL remaining open logs atomically
            var logsToClose = openLogs.Where(l => l.End == null).ToList();
            if (logsToClose.Any())
            {
                await repository.CloseMultipleLogsAsync(logsToClose, exactTime, currentSource);
                // ✅ FIX: Invalidate customer dashboard cache when logs are closed
                await cacheService.RemoveTrackedKeysAsync("Customer");
            }

            // ✅ Final check after closing
            var recentLog = await repository.GetLastOpenLogByMachineId(machine.Id);
            if (recentLog != null && recentLog.Status.Equals(currentStatus, StringComparison.OrdinalIgnoreCase))
            {
                await transaction.CommitAsync();
                //await cacheService.SetMachineHeartbeatAsync(machine.Id.ToString());
                return new ApiResponse<MachineLogSignalRDto>(StatusCodes.Status200OK,
                    "Same status detected — previous log is continuing.");
            }

            // ✅ PERFORMANCE: Get active job (optimized)
            var activeJob = await machineJobRepository.GetActiveJobByMachineIdAsync(input.MachineName!.ToString(), exactTime);

            // ✅ PERFORMANCE: Cache user lookups (users don't change frequently)
            UserModel? user = null;
            if (activeJob != null)
            {
                var userCacheKey = $"User:{activeJob.OperatorId}";
                user = await cacheService.GetAsync<UserModel>(userCacheKey);

                if (user == null)
                {
                    var userResult = await userService.GetUserByIdAsync(activeJob.OperatorId);
                    if (userResult.StatusCode == 200)
                    {
                        user = userResult.Data;
                        // Cache user for 5 minutes
                        if (user != null)
                        {
                            await cacheService.SetAsync(userCacheKey, user, TimeSpan.FromMinutes(5));
                        }
                    }
                }
            }

            var logColor = string.IsNullOrWhiteSpace(match.Color) ? "#3B82F6" : match.Color;

            var newLog = new MachineLog
            {
                Id = Guid.NewGuid().ToString(),
                MachineId = machine.Id,
                CustomerId = machine.CustomerId ?? Guid.Empty,
                JobId = activeJob?.Id!,
                Status = currentStatus,
                Color = logColor,
                Start = exactTime,
                End = null,
                Source = currentSource,
                LastUpdateTime = exactTime,
                Inputs = new List<MachineInput> { new() { InputKey = match.InputKey, Signals = match.Signals } },
                MainProgram = activeJob?.MainProgram!
            };

            if (user != null)
            {
                newLog.UserId = user.UserId;
                newLog.UserName = $"{user.FirstName} {user.LastName}";
            }

            await repository.AddMachineLogAsync(newLog);
            await repository.SaveChangesAsync();

            await transaction.CommitAsync();
            //await cacheService.SetMachineHeartbeatAsync(machine.Id.ToString());

            // ✅ FIX: Invalidate customer dashboard cache when machine log is created/updated
            // This ensures "N/A" machine counts are accurate
            await cacheService.RemoveTrackedKeysAsync("Customer");

            // Invalidate status summary cache to ensure fresh calculation
            var summaryCacheKey = $"StatusSummary:{machine.CustomerId}";
            await cacheService.RemoveAsync(summaryCacheKey);

            // ✅ PERFORMANCE: Fire-and-forget heavy operations (dashboard summary, SignalR, RabbitMQ, alerts)
            // Don't wait for these - they're not critical for the response
            _ = Task.Run(async () =>
            {
                try
                {
                    var dto = await CreateAndPublishLogDtoAsync(newLog, machine, user, activeJob?.Id.ToString());

                    // Background alert check
                    // Create a NEW scope that won't be disposed with the request
                    using var alertScope = serviceScopeFactory.CreateScope();
                    var scopedAlertService = alertScope.ServiceProvider.GetRequiredService<IAlertNotificationService>();
                    var scopedAlertRuleRepo = alertScope.ServiceProvider.GetRequiredService<IAlertRuleRepository>();

                    await CheckAndTriggerMachineStatusAlertsAsync(newLog, machine.MachineName, scopedAlertService, scopedAlertRuleRepo);
                }
                catch (Exception ex)
                {
                    // Log but don't fail the request
                    Console.WriteLine($"Error in background processing: {ex.Message}");
                }
            });

            // Calculate fresh status summary for the response (cache was invalidated above)
            var statusSummary = await CalculateStatusSummary(machine.CustomerId ?? Guid.Empty);
            // Cache for 30 seconds
            await cacheService.SetAsync(summaryCacheKey, statusSummary, TimeSpan.FromSeconds(30));

            // Return DTO with status summary
            var quickDto = new MachineLogSignalRDto(
                Id: newLog.Id,
                MachineId: machine.Id,
                CustomerId: machine.CustomerId ?? Guid.Empty,
                Name: machine.MachineName,
                UserId: user?.UserId ?? Guid.Empty,
                Color: newLog.Color,
                Status: newLog.Status,
                UserName: user != null ? $"{user.FirstName} {user.LastName}" : "N/A",
                JobId: activeJob?.Id ?? string.Empty,
                StatusSummary: statusSummary
            );

            return new ApiResponse<MachineLogSignalRDto>(StatusCodes.Status200OK, "Machine log created.", quickDto);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<MachineLogSignalRDto>(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    #endregion

    #region Alert Checking Methods

    private async Task CheckAndTriggerMachineStatusAlertsAsync(
        MachineLog newLog,
        string machineName,
        IAlertNotificationService alertService,
        IAlertRuleRepository alertRuleRepo,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🚀 CheckAndTriggerMachineStatusAlertsAsync CALLED - MachineId: {newLog?.MachineId}, Status: {newLog?.Status}, MachineName: {machineName}");

        if (newLog == null || newLog.MachineId == Guid.Empty)
        {
            Console.WriteLine($"⚠️ CheckAndTriggerMachineStatusAlertsAsync: Invalid log or MachineId. Log is null: {newLog == null}, MachineId: {newLog?.MachineId}");
            return;
        }

        try
        {
            Console.WriteLine($"🔍 Checking alerts for MachineId: {newLog.MachineId}, Status: {newLog.Status}, MachineName: {machineName}");

            var rules = await alertRuleRepo.GetAlertScopeByMachineIdAsync(newLog.MachineId, cancellationToken);
            if (rules == null || !rules.Any())
            {
                var allRules = await alertRuleRepo.GetAllAsync();
                foreach (var rule in allRules.Where(r => r.Enabled))
                {
                    Console.WriteLine($"🔍 DEBUG: Rule '{rule.RuleName}' - MachineId: {rule.MachineId}, AlertScope: {rule.AlertScope}");
                }
                return;
            }

            // ✅ OPTIMIZATION: Filter and process in parallel where possible
            var machineAlertRules = rules
                .Where(r => r.AlertScope == AlertScope.Machine && r.Enabled)
                .ToList();

            if (machineAlertRules.Count == 0)
            {
                Console.WriteLine($"⚠️ No enabled Machine-scope alert rules found for MachineId: {newLog.MachineId}");
                return;
            }

            // ✅ OPTIMIZATION: Process rules in parallel (fire-and-forget for non-critical updates)
            var matchingRules = machineAlertRules
                .Where(rule => EvaluateMachineStatusCondition(rule, newLog.Status))
                .ToList();

            if (matchingRules.Count == 0)
            {
                foreach (var rule in machineAlertRules)
                {
                    Console.WriteLine($"🔍 Rule '{rule.RuleName}' - Conditions: {rule.Conditions?.Count ?? 0}");
                    if (rule.Conditions != null)
                    {
                        foreach (var condition in rule.Conditions)
                        {
                            Console.WriteLine($"   - Reasons: [{string.Join(", ", condition.Reasons ?? new List<string>())}]");
                        }
                    }
                }
                return;
            }

            var alertTasks = matchingRules.Select(async rule =>
            {
                try
                {
                    rule.LastTriggered = DateTime.UtcNow;
                    await alertRuleRepo.UpdateAsync(rule, cancellationToken);

                    var context = BuildMachineStatusAlertContext(newLog, machineName, rule.Priority.ToString());
                    await alertService.SendAlertAsync(context, rule, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error processing rule '{rule.RuleName}' for MachineId: {newLog.MachineId}. Exception: {ex.Message}");
                }
            });

            // ✅ OPTIMIZATION: Wait for all alerts to complete
            await Task.WhenAll(alertTasks);
            Console.WriteLine($"✅ Completed processing all alerts for MachineId: {newLog.MachineId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in CheckAndTriggerMachineStatusAlertsAsync for MachineId: {newLog.MachineId}. Exception: {ex.Message}");
        }
    }

    private static bool EvaluateMachineStatusCondition(AlertRule rule, string currentStatus)
    {
        if (rule?.Conditions == null || !rule.Conditions.Any() || string.IsNullOrWhiteSpace(currentStatus))
        {
            Console.WriteLine($"⚠️ EvaluateMachineStatusCondition: Invalid rule or status. Rule: {rule?.RuleName}, Status: {currentStatus}");
            return false;
        }

        Console.WriteLine($"🔍 Evaluating rule '{rule.RuleName}' against status '{currentStatus}'. Conditions count: {rule.Conditions.Count}");

        foreach (var condition in rule.Conditions)
        {
            if (condition.Reasons == null || !condition.Reasons.Any())
            {
                Console.WriteLine($"⚠️ Condition has no reasons defined. Rule: {rule.RuleName}");
                continue;
            }

            var matchingReason = condition.Reasons.FirstOrDefault(reason =>
                !string.IsNullOrWhiteSpace(reason) &&
                reason.Equals(currentStatus, StringComparison.OrdinalIgnoreCase));

            if (matchingReason != null)
            {
                Console.WriteLine($"✅ Rule '{rule.RuleName}' matches! Status '{currentStatus}' found in reasons: [{string.Join(", ", condition.Reasons)}]");
                return true;
            }
            else
            {
                Console.WriteLine($"❌ Rule '{rule.RuleName}' does not match. Current status: '{currentStatus}', Rule reasons: [{string.Join(", ", condition.Reasons)}]");
            }
        }

        Console.WriteLine($"❌ No conditions matched for rule '{rule.RuleName}' with status '{currentStatus}'");
        return false;
    }

    private async Task CheckAndTriggerSensorAlertsAsync(
        MachineSensorLog newLog,
        IAlertNotificationService alertService,
        IAlertRuleRepository alertRuleRepo,
        IMachineService scopedMachineService,
        CancellationToken cancellationToken = default)
    {
        if (newLog == null || newLog.MachineId == Guid.Empty)
            return;

        var rules = await alertRuleRepo.GetAlertScopeByMachineIdAsync(newLog.MachineId, cancellationToken);
        if (rules == null || !rules.Any())
            return;

        Dictionary<string, object>? operationalData;
        try
        {
            operationalData = newLog.SensorReading?
                .ToDictionary(r => r.Key.ToString(), r => (object)r.Value)
                ?? new Dictionary<string, object>();
        }
        catch (Exception)
        {
            return;
        }

        Guid customerId = Guid.Empty;
        string machineName = newLog.MachineId.ToString();
        try
        {
            var machineResponse = await scopedMachineService.GetByIdAsync(newLog.MachineId, cancellationToken);
            if (machineResponse.StatusCode == StatusCodes.Status200OK && machineResponse.Data != null)
            {
                customerId = machineResponse.Data.CustomerId ?? Guid.Empty;
                machineName = machineResponse.Data.MachineName ?? machineName;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error fetching machine details for MachineId: {newLog.MachineId}. Exception: {ex.Message}");
        }

        var enabledRules = rules.Where(r => r.Enabled).ToList();
        if (enabledRules.Count == 0)
            return;

        var alertTasks = enabledRules.Select(async rule =>
        {
            try
            {
                var context = BuildSensorAlertContext(newLog, machineName, customerId, rule.Priority.ToString());
                await alertService.EvaluateAndTriggerAlertAsync(rule, operationalData, context, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error processing sensor alert rule '{rule.RuleName}'. Exception: {ex.Message}");
            }
        });

        await Task.WhenAll(alertTasks);
    }

    private async Task CheckAndTriggerOperationalAlertsAsync(
        OperationalData opData,
        string machineName,
        IAlertNotificationService alertService,
        IAlertRuleRepository alertRuleRepo,
        CancellationToken cancellationToken = default)
    {
        if (opData == null || opData.MachineId == Guid.Empty)
            return;

        try
        {
            var rules = await alertRuleRepo.GetEnabledByMachineIdAsync(opData.MachineId, cancellationToken);
            if (rules == null || !rules.Any())
                return;

            var operationalData = ConvertOperationalDataToDictionary(opData);

            // ✅ OPTIMIZATION: Process rules in parallel
            var alertTasks = rules.Select(async rule =>
            {
                try
                {
                    var context = BuildOperationalDataAlertContext(opData, machineName, rule.Priority.ToString());
                    await alertService.EvaluateAndTriggerAlertAsync(rule, operationalData, context, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error processing rule for MachineId: {opData.MachineId}, Rule: {rule.RuleName}. Exception: {ex.Message}");
                }
            });

            await Task.WhenAll(alertTasks);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: Failed to fetch alert rules for MachineId: {opData.MachineId}. Exception: {ex.Message}");
        }
    }

    #endregion

    #region Status Summary

    private async Task<Dictionary<string, int>> CalculateStatusSummary(Guid customerId)
    {
        var groupedStatusSummary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Online"] = 0,
            ["Offline"] = 0,
            ["Warning"] = 0,
            ["Error"] = 0,
            ["DownTime"] = 0,
            ["other"] = 0,
            ["N/A"] = 0
        };

        if (customerId == Guid.Empty)
            return groupedStatusSummary;

        // ✅ FIX: Use scoped repository to avoid DbContext conflicts with parallel operations
        List<Machine> machinesList;
        using (var scope = serviceScopeFactory.CreateScope())
        {
            var customerRepo = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
            var machinesResult = await customerRepo.GetMachinesByCustomerIdAsync(customerId);

            if (machinesResult == null)
                return groupedStatusSummary;

            var machines = await machinesResult.MatchAsync(
                async m => m,
                async error => (List<Machine>)null
            );

            if (machines == null || !machines.Any())
                return groupedStatusSummary;

            // ✅ FIX: Materialize the machines list while still in scope to ensure DbContext operation completes
            machinesList = machines.ToList();
        }

        // ✅ Now that the DbContext is disposed, we can safely start parallel operations
        var logTasks = machinesList.Select(async machine =>
        {
            using var scope = serviceScopeFactory.CreateScope();
            var machineLogRepo = scope.ServiceProvider.GetRequiredService<IMachineLogRepository>();
            var statusSettingRepo = scope.ServiceProvider.GetRequiredService<IMachineStatusSettingRepository>();

            // ✅ FIX: Execute database calls sequentially within each task to avoid DbContext conflicts
            var latestLog = await machineLogRepo.GetLastestLogMachineIdAsync(machine.Id);
            var statusSetting = await statusSettingRepo.GetByMachineIdAsync(machine.Id);

            return new { machine.Id, latestLog, statusSetting };
        });

        var machineLogs = await Task.WhenAll(logTasks);

        // ✅ Track machines with logs to calculate N/A count
        int machinesWithLogs = 0;

        foreach (var entry in machineLogs)
        {
            var log = entry.latestLog;
            var statusSetting = entry.statusSetting;

            if (log == null)
                continue;

            machinesWithLogs++; // Count machines that have logs

            bool isConnectedStatus = false; // Track if machine is connected (has any status except Offline)
            bool alreadyCountedAsOnline = false; // Track if Online was already incremented

            MachineStatus? machineStatus = null;
            if (!string.IsNullOrEmpty(log.Status) &&
                System.Enum.TryParse(log.Status, true, out MachineStatus parsedStatus))
            {
                machineStatus = parsedStatus;
            }

            // ✅ Treat explicit Offline status as Offline even if Type == "other"
            if (machineStatus == MachineStatus.Offline)
            {
                groupedStatusSummary["Offline"]++;
                // Offline machines are not connected, so don't increment Online
            }
            // ✅ Check for "other" type logs (unmatched downtime reasons)
            else if (log.Type?.Equals("other", StringComparison.OrdinalIgnoreCase) == true)
            {
                groupedStatusSummary["other"]++;
                isConnectedStatus = true;
            }
            else if (log.Type?.Equals("DownTimelog", StringComparison.OrdinalIgnoreCase) == true)
            {
                groupedStatusSummary["DownTime"]++;
                isConnectedStatus = true;
            }
            else
            {
                if (machineStatus == MachineStatus.Error)
                {
                    groupedStatusSummary["Error"]++;
                    isConnectedStatus = true;
                }
                else if (machineStatus == MachineStatus.Warning)
                {
                    groupedStatusSummary["Warning"]++;
                    isConnectedStatus = true;
                }
                else if (statusSetting?.Inputs != null && log.Inputs != null)
                {
                    var logSignal = log.Inputs.FirstOrDefault()?.Signals;

                    if (!string.IsNullOrEmpty(logSignal))
                    {
                        var matchedInput = statusSetting.Inputs.FirstOrDefault(i =>
                            i.Signals?.Equals(logSignal, StringComparison.OrdinalIgnoreCase) == true);

                        if (matchedInput != null)
                        {
                            groupedStatusSummary["Online"]++;
                            isConnectedStatus = true;
                            alreadyCountedAsOnline = true; // Already counted as Online, don't double-count
                        }
                        else
                        {
                            groupedStatusSummary["Offline"]++;
                        }
                    }
                    else
                    {
                        groupedStatusSummary["Offline"]++;
                    }
                }
                else
                {
                    groupedStatusSummary["Offline"]++;
                }
            }

            // ✅ NEW LOGIC: If machine has any connected status (Error, Warning, DownTime, or other),
            // it means machine is connected to server, so also increment Online count
            // Note: If already counted as Online, don't double-count
            if (isConnectedStatus && !alreadyCountedAsOnline)
            {
                groupedStatusSummary["Online"]++;
            }
        }

        // ✅ Calculate N/A: machines without any logs
        groupedStatusSummary["N/A"] = machinesList.Count - machinesWithLogs;

        return groupedStatusSummary;
    }

    #endregion
}