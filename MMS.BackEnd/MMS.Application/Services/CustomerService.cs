namespace MMS.Application.Services;

public class CustomerService(
    ICustomerRepository customerRepository,
    AutoMapperResult mapper,
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    IBlobStorageService blobStorageService,
    IUserContextService userContextService,
    IServiceProvider serviceProvider,
    // IMachineLogRepository machineLogRepository,
    // IMachineStatusSettingRepository _machineStatusSetting,
    //ICustomerDashboardSummaryRepository customerDashboardSummaryRepository,
    ICacheService cache) : ICustomerService
{
    private const string CustomerCachePath = "Customer";

    #region Add/Delete/GetById (Unchanged)

    public async Task<ApiResponse<CustomerDto>> AddAsync(AddCustomerDto addCustomerRequest)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            if (await unitOfWork.CustomerRepository.ExistsIgnoreCaseAsync(customer => customer.Email, addCustomerRequest.Email))
            {
                return new ApiResponse<CustomerDto>(
                    StatusCodes.Status409Conflict,
                    "Email already exists."
                );
            }

            if (await unitOfWork.CustomerRepository.ExistsPhoneAsync(
                addCustomerRequest.PhoneCountryCode,
                addCustomerRequest.PhoneNumber))
            {
                return new ApiResponse<CustomerDto>(
                    StatusCodes.Status409Conflict,
                    "An account with this phone number already exists."
                );
            }

            var customer = mapper.Map<Customer>(addCustomerRequest);

            if (!addCustomerRequest.Status.HasValue)
            {
                customer.Status = CustomerStatus.Active;
            }

            if (!string.IsNullOrWhiteSpace(addCustomerRequest.ImageBase64))
            {
                var uploadedUri = await blobStorageService.UploadBase64Async(addCustomerRequest.ImageBase64, BlobStorageConstants.CustomerFolder);
                customer.ImageUrls = uploadedUri.AbsoluteUri;
            }
            else
            {
                customer.ImageUrls = string.Empty;
            }

            await unitOfWork.CustomerRepository.AddAsync(customer);
            await unitOfWork.SaveChangesAsync();

            await cache.RemoveTrackedKeysAsync(CustomerCachePath);
            await transaction.CommitAsync();

            _ = emailService.SendAccountActivatedEmailAsync(customer.Email, customer.Name);

            return new ApiResponse<CustomerDto>(StatusCodes.Status201Created,
                nameof(Customer) + ResponseMessages.Added, mapper.GetResult<Customer, CustomerDto>(customer));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<CustomerDto>(
                StatusCodes.Status500InternalServerError,
                "An error occurred while adding the customer: " + ex.Message);
        }
    }

    public async Task<ApiResponse<CustomerDto>> DeleteAsync(Guid customerId)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, customerId);

            var existingCustomerResult = await unitOfWork.CustomerRepository.GetAsync(customerId);
            if (existingCustomerResult.IsLeft)
            {
                return new ApiResponse<CustomerDto>(
                    StatusCodes.Status404NotFound,
                    $"Customer with ID {customerId} not found."
                );
            }

            var rightCustomer = existingCustomerResult.IfRight();
            if (!string.IsNullOrWhiteSpace(rightCustomer?.ImageUrls))
            {
                try
                {
                    await blobStorageService.DeleteBase64Async(rightCustomer?.ImageUrls);
                }
                catch (Exception ex)
                {
                    return new ApiResponse<CustomerDto>(
                        StatusCodes.Status500InternalServerError,
                        $"Failed to delete customer's image from blob storage: {ex.Message}");
                }
            }

            await unitOfWork.CustomerRepository.DeleteAsync(customerId);

            await unitOfWork.SaveChangesAsync();
            await cache.RemoveTrackedKeysAsync(CustomerCachePath);
            await transaction.CommitAsync();

            return new ApiResponse<CustomerDto>(
                StatusCodes.Status200OK,
                nameof(Customer) + ResponseMessages.Deleted
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<CustomerDto>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<CustomerDto>(
                StatusCodes.Status500InternalServerError,
                "An error occurred while deleting the customer: " + ex.Message
            );
        }
    }

    public async Task<ApiResponse<CustomerDto>> GetByIdAsync(Guid customerId)
    {
        try
        {
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, customerId);

            // ✅ PERFORMANCE: Cache customer lookups (customers don't change frequently)
            var cacheKey = GetCustomerKey(customerId);
            var cached = await cache.GetAsync<CustomerDto>(cacheKey);
            if (cached != null)
            {
                return new ApiResponse<CustomerDto>(
                    StatusCodes.Status200OK,
                    nameof(Customer) + ResponseMessages.Get,
                    cached
                );
            }

            var result = await customerRepository.GetAsync(customerId);

            if (result.IsRight)
            {
                var customer = result.IfRight();
                var dto = mapper.Map<CustomerDto>(customer!);

                // ✅ PERFORMANCE: Cache for 10 minutes
                await cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10), CustomerCachePath);

                return new ApiResponse<CustomerDto>(
                    StatusCodes.Status200OK,
                    nameof(Customer) + ResponseMessages.Get,
                    dto
                );
            }

            var error = result.IfLeft();
            return new ApiResponse<CustomerDto>(
                StatusCodes.Status404NotFound,
                error!.Message
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<CustomerDto>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<CustomerDto>(
                StatusCodes.Status500InternalServerError,
                "An error occurred while retrieving the customer. " + ex.Message
            );
        }
    }

    #endregion

    #region Dashboard Methods (OPTIMIZED)

    /// <summary>
    /// ✅ OPTIMIZED: Main Dashboard - Uses parallel execution with scoped repositories + caching
    /// </summary>
    public async Task<ApiResponse<List<CustomerCardDto>>> GetCustomerDashboardAsync(
    PageParameters pageParameters, Guid? customerId)
    {
        try
        {
            // 1. Get accessible customer IDs
            List<Guid>? accessibleCustomerIds;
            if (customerId.HasValue)
            {
                await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, customerId.Value);
                accessibleCustomerIds = [customerId.Value];
            }
            else
            {
                accessibleCustomerIds = CustomerAccessHelper.GetAccessibleCustomerIds(userContextService);
            }

            string term = pageParameters.Term?.Trim() ?? string.Empty;
            int skip = pageParameters.Skip ?? 0;
            int top = pageParameters.Top ?? 10;

            // ✅ OPTIMIZATION: Use StringBuilder for cache key generation (faster than string concatenation)
            var cacheKeyBuilder = new System.Text.StringBuilder("customer:dashboard:");
            if (customerId.HasValue)
                cacheKeyBuilder.Append(customerId.Value);
            cacheKeyBuilder.Append(':').Append(term).Append(':').Append(skip).Append(':').Append(top).Append(':');

            if (accessibleCustomerIds != null && accessibleCustomerIds.Count > 0)
            {
                // ✅ OPTIMIZATION: Pre-allocate capacity for string join
                var idsArray = accessibleCustomerIds.Select(id => id.ToString()).ToArray();
                cacheKeyBuilder.Append(string.Join(",", idsArray));
            }
            else
            {
                cacheKeyBuilder.Append("all");
            }

            var cacheKey = cacheKeyBuilder.ToString();
            var cached = await cache.GetAsync<List<CustomerCardDto>>(cacheKey);
            if (cached != null)
            {
                return new ApiResponse<List<CustomerCardDto>>(
                    StatusCodes.Status200OK,
                    "Dashboard data fetched successfully (cached).",
                    cached
                );
            }

            // 2. Get paged customers with machines
            var pagedCustomers = await customerRepository
                .GetAllWithMachinesAsync(accessibleCustomerIds, term, skip, top);

            if (pagedCustomers == null || !pagedCustomers.Any())
            {
                return new ApiResponse<List<CustomerCardDto>>(
                    StatusCodes.Status200OK,
                    "No customers found.",
                    new List<CustomerCardDto>()
                );
            }

            // ✅ OPTIMIZATION: Collect machine IDs using HashSet for O(1) lookups and automatic deduplication
            var allMachineIdsSet = new HashSet<Guid>(pagedCustomers.Count * 2); // Pre-allocate capacity estimate
            foreach (var customer in pagedCustomers)
            {
                if (customer.Machine != null && customer.Machine.Count > 0)
                {
                    foreach (var machine in customer.Machine)
                    {
                        allMachineIdsSet.Add(machine.Id);
                    }
                }
            }

            var allMachineIds = allMachineIdsSet.ToList();

            if (!allMachineIds.Any())
            {
                var emptyResult = pagedCustomers.Select(c => new CustomerCardDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    TimeZone = c.TimeZone,
                    ImageUrl = c.ImageUrls,
                    TotalMachines = 0,
                    StatusSummary = CreateEmptyStatusSummary()
                }).ToList();

                return new ApiResponse<List<CustomerCardDto>>(
                    StatusCodes.Status200OK,
                    "Dashboard data fetched successfully.",
                    emptyResult
                );
            }

            // ✅ OPTIMIZATION: Direct async calls without Task.Run overhead + parallel execution
            using var logsScope = serviceProvider.CreateScope();
            using var settingsScope = serviceProvider.CreateScope();

            var logsRepo = logsScope.ServiceProvider.GetRequiredService<IMachineLogRepository>();
            var settingsRepo = settingsScope.ServiceProvider.GetRequiredService<IMachineStatusSettingRepository>();

            // ✅ OPTIMIZATION: Execute queries in parallel without Task.Run overhead
            var latestLogsTask = logsRepo.GetByMachineIdsLatestLogAsync(allMachineIds);
            var statusSettingsTask = settingsRepo.GetAllAsync(allMachineIds);

            await Task.WhenAll(latestLogsTask, statusSettingsTask);

            // ✅ OPTIMIZATION: Single-pass dictionary creation with duplicate handling
            var latestLogs = await latestLogsTask;
            var latestLogsByMachineId = new Dictionary<Guid, MachineLog>(latestLogs?.Count ?? 0);
            if (latestLogs != null)
            {
                foreach (var log in latestLogs)
                {
                    // ✅ OPTIMIZATION: Only add if not exists or if this log is more recent
                    if (!latestLogsByMachineId.TryGetValue(log.MachineId, out var existing) ||
                        log.LastUpdateTime > existing.LastUpdateTime)
                    {
                        latestLogsByMachineId[log.MachineId] = log;
                    }
                }
            }

            // ✅ OPTIMIZATION: Single-pass dictionary creation
            var statusSettings = await statusSettingsTask;
            var statusSettingsByMachineId = new Dictionary<Guid, MachineStatusSetting>();
            if (statusSettings != null)
            {
                foreach (var setting in statusSettings)
                {
                    // ✅ OPTIMIZATION: Only add first occurrence (avoid duplicates)
                    if (!statusSettingsByMachineId.ContainsKey(setting.MachineId))
                    {
                        statusSettingsByMachineId[setting.MachineId] = setting;
                    }
                }
            }

            // ✅ OPTIMIZATION: Pre-allocate list capacity and process in memory - NO more DB calls
            var result = new List<CustomerCardDto>(pagedCustomers.Count);
            foreach (var customer in pagedCustomers)
            {
                result.Add(BuildCustomerCard(customer, latestLogsByMachineId, statusSettingsByMachineId));
            }

            // ✅ PERFORMANCE: Cache result for 10 seconds (reduced from 30s to prevent stale "N/A" machine counts)
            await cache.SetAsync(cacheKey, result, TimeSpan.FromSeconds(10), CustomerCachePath);

            return new ApiResponse<List<CustomerCardDto>>(
                StatusCodes.Status200OK,
                "Dashboard data fetched successfully.",
                result
            );
        }
        catch (UnauthorizedAccessException)
        {
            return new ApiResponse<List<CustomerCardDto>>(
                StatusCodes.Status403Forbidden,
                "You do not have permission to access this resource."
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<CustomerCardDto>>(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred: {ex.Message}"
            );
        }
    }

    /// <summary>
    /// ✅ OPTIMIZED: Single Customer Summary - Uses batch fetching + caching
    /// </summary>
    public async Task<ApiResponse<CustomerCardDto>> GetCustomerSummaryAsync(Guid customerId)
    {
        try
        {
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, customerId);

            // ✅ PERFORMANCE: Cache customer summary (10 seconds - reduced from 30s to prevent stale data)
            var cacheKey = $"customer:summary:{customerId}";
            var cached = await cache.GetAsync<CustomerCardDto>(cacheKey);
            if (cached != null)
            {
                return new ApiResponse<CustomerCardDto>(
                    StatusCodes.Status200OK,
                    "Dashboard data fetched successfully (cached).",
                    cached
                );
            }

            // Get Customer with Machines
            var customers = await customerRepository.GetAllWithMachinesAsync(
                new List<Guid> { customerId }, string.Empty, 0, 1);

            var customer = customers.FirstOrDefault();

            if (customer == null)
            {
                return new ApiResponse<CustomerCardDto>(
                    StatusCodes.Status404NotFound,
                    "No customer found for the provided ID",
                    null
                );
            }

            var machineIds = customer.Machine?.Select(m => m.Id).ToList() ?? new List<Guid>();

            if (!machineIds.Any())
            {
                return new ApiResponse<CustomerCardDto>(
                    StatusCodes.Status200OK,
                    "Dashboard data fetched successfully",
                    new CustomerCardDto
                    {
                        Id = customer.Id,
                        Name = customer.Name,
                        TimeZone = customer.TimeZone,
                        ImageUrl = customer.ImageUrls,
                        TotalMachines = 0,
                        StatusSummary = CreateEmptyStatusSummary(),
                        Shifts = customer.Shifts?.Select(s => new ShiftDto
                        {
                            ShiftName = s.ShiftName,
                            Start = s.Start,
                            End = s.End
                        }).ToList() ?? new List<ShiftDto>()
                    }
                );
            }

            // ✅ PARALLEL with scoped repositories
            var latestLogsTask = Task.Run(async () =>
            {
                using var scope = serviceProvider.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IMachineLogRepository>();
                return await repo.GetByMachineIdsLatestLogAsync(machineIds);
            });

            var statusSettingsTask = Task.Run(async () =>
            {
                using var scope = serviceProvider.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IMachineStatusSettingRepository>();
                return await repo.GetAllAsync(machineIds);
            });

            await Task.WhenAll(latestLogsTask, statusSettingsTask);

            // ✅ FIXED: Handle duplicate MachineIds using GroupBy + null safety
            var latestLogs = await latestLogsTask;
            var latestLogsByMachineId = (latestLogs ?? new List<MachineLog>())
                .GroupBy(l => l.MachineId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(l => l.LastUpdateTime).First());

            var statusSettings = await statusSettingsTask;
            var statusSettingsList = statusSettings?.ToList() ?? new List<MachineStatusSetting>();
            var statusSettingsByMachineId = statusSettingsList
                .GroupBy(s => s.MachineId)
                .ToDictionary(g => g.Key, g => g.First());

            // Build result using shared logic
            var cardDto = BuildCustomerCard(customer, latestLogsByMachineId, statusSettingsByMachineId);

            // Add shifts (specific to this method)
            cardDto.Shifts = customer.Shifts?.Select(s => new ShiftDto
            {
                ShiftName = s.ShiftName,
                Start = s.Start,
                End = s.End
            }).ToList() ?? new List<ShiftDto>();

            // ✅ PERFORMANCE: Cache result for 10 seconds (reduced from 30s to prevent stale data)
            await cache.SetAsync(cacheKey, cardDto, TimeSpan.FromSeconds(10), CustomerCachePath);

            return new ApiResponse<CustomerCardDto>(
                StatusCodes.Status200OK,
                "Dashboard data fetched successfully",
                cardDto
            );
        }
        catch (UnauthorizedAccessException)
        {
            return new ApiResponse<CustomerCardDto>(
                StatusCodes.Status403Forbidden,
                "You do not have permission to access this resource.",
                null
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<CustomerCardDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while fetching dashboard data: {ex.Message}",
                null
            );
        }
    }

    /// <summary>
    /// ✅ OPTIMIZED: Customer Card Summary - Batch fetching
    /// </summary>
    public async Task<ApiResponse<CustomerCardDto>> GetCustomerCardSummaryAsync(Guid customerId)
    {
        try
        {
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, customerId);

            var emptyStatusCounts = System.Enum.GetValues<MachineStatus>()
                .ToDictionary(status => status.ToString(), _ => 0);

            var emptyDto = new CustomerCardDto
            {
                Id = customerId,
                Name = string.Empty,
                ImageUrl = string.Empty,
                TotalMachines = 0,
                StatusSummary = emptyStatusCounts
            };

            var machinesResult = await unitOfWork.CustomerRepository.GetMachinesByCustomerIdAsync(customerId);

            return await machinesResult.MatchAsync(
                async machines =>
                {
                    if (machines == null || !machines.Any())
                    {
                        return new ApiResponse<CustomerCardDto>(
                            StatusCodes.Status200OK,
                            "No machines assigned to this customer.",
                            emptyDto
                        );
                    }

                    var machineIds = machines.Select(m => m.Id).ToList();

                    // ✅ BATCH fetch instead of loop
                    List<MachineLog> latestLogs;
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var repo = scope.ServiceProvider.GetRequiredService<IMachineLogRepository>();
                        latestLogs = await repo.GetByMachineIdsLatestLogAsync(machineIds);
                    }

                    // ✅ Get status settings for proper status determination
                    List<MachineStatusSetting> statusSettings;
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var repo = scope.ServiceProvider.GetRequiredService<IMachineStatusSettingRepository>();
                        var statusSettingsEnumerable = await repo.GetAllAsync(machineIds);
                        statusSettings = statusSettingsEnumerable?.ToList() ?? new List<MachineStatusSetting>();
                    }

                    var statusSettingsByMachineId = statusSettings?
                        .GroupBy(s => s.MachineId)
                        .ToDictionary(g => g.Key, g => g.First()) ?? new Dictionary<Guid, MachineStatusSetting>();

                    // ✅ Use same logic as BuildCustomerCard for consistency
                    var statusCounts = CreateEmptyStatusSummary();
                    int machinesWithLogs = 0;

                    foreach (var machineId in machineIds)
                    {
                        var latestLog = latestLogs.FirstOrDefault(l => l.MachineId == machineId);
                        if (latestLog != null)
                        {
                            machinesWithLogs++;
                            statusSettingsByMachineId.TryGetValue(machineId, out var statusSetting);

                            var status = DetermineStatus(latestLog, statusSetting);
                            statusCounts[status]++;

                            // ✅ NEW LOGIC: If machine has any connected status (Error, Warning, DownTime, or other),
                            // it means machine is connected to server, so also increment Online count
                            // Note: If status is already "Online", it's already counted, so we don't double-count
                            if (status == "Error" || status == "Warning" || status == "DownTime" || status == "other")
                            {
                                statusCounts["Online"]++;
                            }
                        }
                    }

                    statusCounts["N/A"] = machineIds.Count - machinesWithLogs;

                    var dto = new CustomerCardDto
                    {
                        Id = customerId,
                        Name = string.Empty,
                        ImageUrl = string.Empty,
                        TotalMachines = machineIds.Count,
                        StatusSummary = statusCounts
                    };

                    return new ApiResponse<CustomerCardDto>(
                        StatusCodes.Status200OK,
                        "Dashboard data fetched successfully.",
                        dto
                    );
                },
                error => Task.FromResult(new ApiResponse<CustomerCardDto>(
                    StatusCodes.Status200OK,
                    "No machines assigned to this customer.",
                    emptyDto
                ))
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<CustomerCardDto>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<CustomerCardDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while fetching dashboard data: {ex.Message}"
            );
        }
    }

    /// <summary>
    /// ✅ OPTIMIZED: Dashboard Details - Batch fetching for all customers + caching
    /// </summary>
    public async Task<ApiResponse<List<CustomerCardDto>>> GetCustomerDashboardDetailsAsync(PageParameters pageParameters)
    {
        try
        {
            var customerIds = CustomerAccessHelper.GetAccessibleCustomerIds(userContextService);
            string term = pageParameters.Term ?? string.Empty;
            int skip = pageParameters.Skip ?? 0;
            int top = pageParameters.Top ?? 10;

            // ✅ PERFORMANCE: Cache dashboard details (30 seconds)
            // ✅ FIX: Handle null case for SystemAdmin (null means all customers)
            var customerIdsForCache = customerIds != null && customerIds.Any()
                ? string.Join(",", customerIds)
                : "all";
            var cacheKey = $"customer:dashboard:details:{term}:{skip}:{top}:{customerIdsForCache}";
            var cached = await cache.GetAsync<List<CustomerCardDto>>(cacheKey);
            if (cached != null)
            {
                return new ApiResponse<List<CustomerCardDto>>(
                    StatusCodes.Status200OK,
                    "Dashboard data fetched successfully (cached).",
                    cached
                );
            }

            var pagedCustomers = await customerRepository.GetAllWithMachinesAsync(
                customerIds, term, skip, top);

            if (!pagedCustomers.Any())
            {
                return new ApiResponse<List<CustomerCardDto>>(
                    StatusCodes.Status200OK,
                    "No customers found.",
                    new List<CustomerCardDto>()
                );
            }

            // ✅ Collect all machine IDs across all customers
            var allMachineIds = pagedCustomers
                .Where(c => c.Machine != null)
                .SelectMany(c => c.Machine)
                .Select(m => m.Id)
                .Distinct()
                .ToList();

            // ✅ SINGLE batch fetch for all logs
            List<MachineLog> allLatestLogs;
            using (var scope = serviceProvider.CreateScope())
            {
                var logRepo = scope.ServiceProvider.GetRequiredService<IMachineLogRepository>();
                allLatestLogs = await logRepo.GetByMachineIdsLatestLogAsync(allMachineIds);
            }

            var logsByMachineId = allLatestLogs.ToDictionary(l => l.MachineId, l => l);

            // ✅ Process in memory
            var result = pagedCustomers.Select(customer =>
            {
                var machineIds = customer.Machine?.Select(m => m.Id).ToList() ?? new List<Guid>();
                var customerLogs = machineIds
                    .Where(id => logsByMachineId.ContainsKey(id))
                    .Select(id => logsByMachineId[id])
                    .ToList();

                var statusCounts = System.Enum.GetValues<MachineStatus>()
                    .ToDictionary(
                        status => status.ToString(),
                        status => customerLogs.Count(log =>
                            System.Enum.TryParse<MachineStatus>(log.Status, out var parsedStatus) &&
                            parsedStatus == status)
                    );

                return new CustomerCardDto
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    ImageUrl = customer.ImageUrls,
                    TotalMachines = machineIds.Count,
                    StatusSummary = statusCounts
                };
            }).ToList();

            // ✅ PERFORMANCE: Cache result for 10 seconds (reduced from 30s to prevent stale "N/A" machine counts)
            await cache.SetAsync(cacheKey, result, TimeSpan.FromSeconds(10), CustomerCachePath);

            return new ApiResponse<List<CustomerCardDto>>(
                StatusCodes.Status200OK,
                "Dashboard data fetched successfully",
                result
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<List<CustomerCardDto>>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<CustomerCardDto>>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while fetching dashboard data: {ex.Message}",
                null
            );
        }
    }

    /// <summary>
    /// ✅ OPTIMIZED: Machines by Customer - Parallel with scoped repositories + caching
    /// </summary>
    public async Task<ApiResponse<List<MachineJobDto>>> GetMachinesByCustomerIdAsync(
    Guid customerId,
    PageParameters pageParameters,
    DateTime from,
    DateTime to)
    {
        try
        {
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, customerId);
            var start = from;
            var end = to;

            // ✅ PERFORMANCE: Generate cache key based on all parameters
            var cacheKeyBuilder = new System.Text.StringBuilder("machines:customer:");
            cacheKeyBuilder.Append(customerId).Append(':');
            cacheKeyBuilder.Append(pageParameters.Skip ?? 0).Append(':');
            cacheKeyBuilder.Append(pageParameters.Top ?? 0).Append(':');
            cacheKeyBuilder.Append(pageParameters.Term ?? string.Empty).Append(':');
            cacheKeyBuilder.Append(start.ToString("yyyy-MM-ddTHH:mm:ss")).Append(':');
            cacheKeyBuilder.Append(end.ToString("yyyy-MM-ddTHH:mm:ss"));
            var cacheKey = cacheKeyBuilder.ToString();

            // ✅ PERFORMANCE: Check cache first (5 seconds TTL for real-time data)
            var cached = await cache.GetAsync<List<MachineJobDto>>(cacheKey);
            if (cached != null)
            {
                return new ApiResponse<List<MachineJobDto>>(
                    StatusCodes.Status200OK,
                    $"{nameof(MachineJobDto)} {ResponseMessages.Get} (cached)",
                    cached
                );
            }

            var result = await customerRepository.GetMachinesByCustomerIdAsync(customerId, pageParameters);

            return await result.MatchAsync(
                async machines =>
                {
                    if (machines == null || !machines.Any())
                    {
                        return new ApiResponse<List<MachineJobDto>>(
                            StatusCodes.Status200OK,
                            $"{nameof(MachineJobDto)} {ResponseMessages.Get}",
                            new List<MachineJobDto>()
                        );
                    }

                    var machineIds = machines.Select(m => m.Id).ToList();

                    // ✅ OPTIMIZATION: Direct async calls with scoped repositories (no Task.Run overhead)
                    using var jobsScope = serviceProvider.CreateScope();
                    using var logsScope = serviceProvider.CreateScope();
                    using var latestLogsScope = serviceProvider.CreateScope();

                    var jobsRepo = jobsScope.ServiceProvider.GetRequiredService<IMachineJobRepository>();
                    var logsRepo = logsScope.ServiceProvider.GetRequiredService<IMachineLogRepository>();
                    var latestLogsRepo = latestLogsScope.ServiceProvider.GetRequiredService<IMachineLogRepository>();

                    // ✅ OPTIMIZATION: Execute queries in parallel without Task.Run overhead
                    var allJobsTask = jobsRepo.GetJobsByMachineIdsAndDateRangeAsync(machineIds, start, end);
                    var allLogsTask = logsRepo.GetByMachineIdsForCustomerDashboardAsync(machineIds, start, end);
                    var latestLogsTask = latestLogsRepo.GetByMachineIdsLatestLogAsync(machineIds);

                    await Task.WhenAll(allJobsTask, allLogsTask, latestLogsTask);

                    var allJobs = await allJobsTask;
                    var allLogs = await allLogsTask;
                    var latestMachineLogs = await latestLogsTask;

                    // ✅ FIXED: Create lookups with duplicate handling
                    var jobsByMachineId = allJobs?
                        .SelectMany(j => j.MachineIds.Select(id => new { MachineId = Guid.Parse(id), Job = j }))
                        .GroupBy(x => x.MachineId)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.Job).Distinct().ToList())
                        ?? new Dictionary<Guid, List<MachineJob>>();

                    // ✅ Create JobId to JobName lookup dictionary
                    var jobNameLookup = allJobs?
                        .Where(j => !string.IsNullOrEmpty(j.Id) && !string.IsNullOrEmpty(j.JobName))
                        .ToDictionary(j => j.Id, j => j.JobName)
                        ?? new Dictionary<string, string>();

                    var logsByMachineId = allLogs?
                        .GroupBy(l => l.MachineId)
                        .ToDictionary(g => g.Key, g => g.ToList())
                        ?? new Dictionary<Guid, List<MachineLog>>();

                    // ✅ FIXED: Handle duplicate MachineIds for latest logs
                    var latestLogByMachineId = latestMachineLogs?
                        .GroupBy(l => l.MachineId)
                        .ToDictionary(g => g.Key, g => g.OrderByDescending(l => l.LastUpdateTime).First())
                        ?? new Dictionary<Guid, MachineLog>();

                    // ✅ PERFORMANCE: Collect all unique operator IDs first for batch user lookup
                    // ✅ OPTIMIZATION: Use LINQ to flatten and collect operator IDs efficiently
                    var operatorIds = new HashSet<Guid>();
                    var machineJobMap = new Dictionary<Guid, MachineJob>();

                    // ✅ OPTIMIZATION: Single pass through all jobs instead of nested loops
                    foreach (var machine in machines)
                    {
                        if (!jobsByMachineId.TryGetValue(machine.Id, out var machineJobs) || machineJobs.Count == 0)
                            continue;

                        // ✅ OPTIMIZATION: Find first job with operator in single LINQ operation
                        var jobWithOperator = machineJobs.FirstOrDefault(j => j.OperatorId != Guid.Empty);
                        if (jobWithOperator != null)
                        {
                            if (operatorIds.Add(jobWithOperator.OperatorId))
                            {
                                machineJobMap[machine.Id] = jobWithOperator;
                            }
                        }
                    }

                    // ✅ OPTIMIZATION: Batch fetch all users in parallel (with caching) - improved efficiency
                    var userLookup = new Dictionary<Guid, (string Name, string ImageUrl)>();
                    if (operatorIds.Any())
                    {
                        // ✅ OPTIMIZATION: Pre-allocate list capacity and batch cache lookups
                        var userTasks = new List<Task<(Guid Id, UserModel? User)>>(operatorIds.Count);

                        foreach (var operatorId in operatorIds)
                        {
                            var task = Task.Run(async () =>
                            {
                                // ✅ OPTIMIZATION: Create scope inside Task.Run for proper scoped service handling
                                using var userScope = serviceProvider.CreateScope();
                                var userService = userScope.ServiceProvider.GetRequiredService<IUserService>();
                                var cacheService = userScope.ServiceProvider.GetRequiredService<ICacheService>();

                                // ✅ PERFORMANCE: Cache user lookups
                                var userCacheKey = $"User:{operatorId}";
                                var cachedUser = await cacheService.GetAsync<UserModel>(userCacheKey);

                                if (cachedUser != null)
                                {
                                    return (operatorId, cachedUser);
                                }

                                var userResponse = await userService.GetUserByIdAsync(operatorId);
                                if (userResponse.StatusCode == StatusCodes.Status200OK && userResponse.Data != null)
                                {
                                    // Cache for 5 minutes
                                    await cacheService.SetAsync(userCacheKey, userResponse.Data, TimeSpan.FromMinutes(5));
                                    return (operatorId, userResponse.Data);
                                }
                                return (operatorId, (UserModel?)null);
                            });
                            userTasks.Add(task);
                        }

                        var userResults = await Task.WhenAll(userTasks);
                        foreach (var (id, user) in userResults)
                        {
                            if (user != null)
                            {
                                userLookup[id] = ($"{user.FirstName} {user.LastName}", user.ProfileImage ?? string.Empty);
                            }
                        }
                    }

                    // ✅ Process machines - NO async calls in loop
                    var machineDtos = new List<MachineJobDto>();

                    foreach (var machine in machines)
                    {
                        var machineJobs = jobsByMachineId.GetValueOrDefault(machine.Id) ?? new List<MachineJob>();
                        var machineLogs = logsByMachineId.GetValueOrDefault(machine.Id) ?? new List<MachineLog>();
                        var latestLog = latestLogByMachineId.GetValueOrDefault(machine.Id);

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
                                    UserName = l.UserName ?? "Unattended",
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
                            var allLogsToProcess = deduplicatedClosedLogs
                                .Concat(openLogsByStatus.Values)
                                .OrderBy(l => l.Start ?? l.LastUpdateTime)
                                .ToList();

                            // ✅ OPTIMIZATION: Single pass to create activity segments
                            activitySegments = allLogsToProcess.Select(log => new ActivitySegment
                            {
                                Status = log.Status,
                                Start = log.Start ?? DateTime.MinValue,
                                End = log.End,
                                JobId = log.JobId ?? string.Empty,
                                JobName = !string.IsNullOrEmpty(log.JobId) && jobNameLookup.TryGetValue(log.JobId, out var jobName) 
                                    ? jobName 
                                    : string.Empty,
                                Color = log.Color,
                                UserName = log.UserName,
                                CurrentProgram = log.CurrentProgram ?? string.Empty,
                                MainProgram = log.MainProgram ?? string.Empty,
                                IsRunning = log.End == null
                            }).ToList();
                        }

                        // ✅ OPTIMIZATION: Calculate sum using LINQ Sum instead of loop
                        float sumPlannedSec = machineJobs
                            .Where(j => j.Schedule != null)
                            .Select(j =>
                            {
                                var duration = j.Schedule!.PlannedEnd - j.Schedule.PlannedStart;
                                return (float)duration.TotalSeconds;
                            })
                            .Where(sec => sec > 0)
                            .Sum();

                        string userName = "Unattended";
                        Guid operatorId = Guid.Empty;
                        string operatorProfileImageUrl = string.Empty;

                        // ✅ PERFORMANCE: Use pre-fetched user data
                        if (machineJobMap.TryGetValue(machine.Id, out var jobWithUser) &&
                            jobWithUser.OperatorId != Guid.Empty &&
                            userLookup.TryGetValue(jobWithUser.OperatorId, out var userInfo))
                        {
                            userName = userInfo.Name;
                            operatorProfileImageUrl = userInfo.ImageUrl;
                            operatorId = jobWithUser.OperatorId;
                        }

                        var (availability, performance, quality, oee) = OeeCalculator.CalculateOeeMetrics(machineJobs, sumPlannedSec);

                        machineDtos.Add(new MachineJobDto
                        {
                            MachineId = machine.Id,
                            MachineModel = machine.MachineModel ?? string.Empty,
                            ImageUrl = machine.ImageUrl ?? string.Empty,
                            MachineName = machine.MachineName ?? string.Empty,
                            SerialNumber = machine.SerialNumber ?? string.Empty,
                            Availability = availability,
                            UserName = userName,
                            OperatorImageUrl = operatorProfileImageUrl,
                            UserId = operatorId,
                            Performance = performance,
                            Quality = quality,
                            lastLogStatus = latestLog?.Status,
                            lastLogColor = latestLog?.Color,
                            lastUpdatedTime = latestLog?.LastUpdateTime,
                            Oee = oee,
                            Activity = activitySegments
                        });
                    }

                    // ✅ PERFORMANCE: Cache result for 5 seconds (short TTL for real-time data)
                    await cache.SetAsync(cacheKey, machineDtos, TimeSpan.FromSeconds(5), CustomerCachePath);

                    return new ApiResponse<List<MachineJobDto>>(
                        StatusCodes.Status200OK,
                        $"{nameof(MachineJobDto)} {ResponseMessages.Get}",
                        machineDtos
                    );
                },
                error => Task.FromResult(new ApiResponse<List<MachineJobDto>>(
                    StatusCodes.Status404NotFound,
                    error.Message
                ))
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<List<MachineJobDto>>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<MachineJobDto>>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while retrieving machines: {ex.Message}"
            );
        }
    }

    #endregion

    #region Other Methods (GetListAsync, UpdateAsync)

    public async Task<ApiResponse<IEnumerable<CustomerDto>>> GetListAsync(PageParameters pageParameters, CustomerStatus? customerStatus)
    {
        try
        {
            // ✅ PERFORMANCE: Check cache first before querying
            string cacheKey = $"customer:list:{customerStatus}:{pageParameters.Term}:{pageParameters.Top}:{pageParameters.Skip}:{userContextService.UserId}";
            var cached = await cache.GetAsync<List<Customer>>(cacheKey);
            if (cached != null)
            {
                return new ApiResponse<IEnumerable<CustomerDto>>(
                    StatusCodes.Status200OK,
                    $"{nameof(Customer)} {ResponseMessages.GetAll}",
                    mapper.Map<IEnumerable<CustomerDto>>(cached)
                );
            }

            var term = pageParameters.Term?.Trim().ToLower() ?? string.Empty;
            Expression<Func<Customer, bool>> searchExpr = c =>
                   string.IsNullOrEmpty(term)
                   || EF.Functions.Like(c.Name.Trim().ToLower(), $"%{term}%")
                   || EF.Functions.Like(c.Country.Trim().ToLower(), $"%{term}%");

            var customerIds = CustomerAccessHelper.GetAccessibleCustomerIds(userContextService);

            var filterExpressions = new List<Expression<Func<Customer, bool>>>();
            if (customerStatus != null)
            {
                filterExpressions.Add(c => c.Status == customerStatus);
            }
            if (customerIds != null && customerIds.Any())
            {
                filterExpressions.Add(c => customerIds.Contains(c.Id));
            }

            var customers = await customerRepository.GetListAsync(
                pageParameters,
                searchExpr,
                filterExpressions,
                q => q.OrderBy(c => c.Name));

            // ✅ PERFORMANCE: Cache for 5 minutes (list changes more frequently than individual)
            await cache.SetAsync(cacheKey, customers.ToList(), TimeSpan.FromMinutes(5), CustomerCachePath);
            return new ApiResponse<IEnumerable<CustomerDto>>(
                StatusCodes.Status200OK,
                $"{nameof(Customer)} {ResponseMessages.GetAll}",
                mapper.Map<IEnumerable<CustomerDto>>(customers)
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<IEnumerable<CustomerDto>>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<CustomerDto>>(
                StatusCodes.Status500InternalServerError,
                "An error occurred while retrieving the customer list." + ex.Message
            );
        }
    }

    public async Task<ApiResponse<CustomerDto>> UpdateAsync(Guid customerId, UpdateCustomerDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, customerId);
            var existingCustomerResult = await unitOfWork.CustomerRepository.GetAsync(customerId);

            if (existingCustomerResult.IsLeft)
            {
                return new ApiResponse<CustomerDto>(
                    StatusCodes.Status400BadRequest,
                    $"Customer with ID {customerId} not found."
                );
            }
            var rightCustomer = existingCustomerResult.IfRight();

            if (await unitOfWork.CustomerRepository.ExistsPhoneAsync(
               request.PhoneCountryCode,
               request.PhoneNumber, request.Id))
            {
                return new ApiResponse<CustomerDto>(
                    StatusCodes.Status409Conflict,
                    "An account with this phone number already exists."
                );
            }

            if (!string.Equals(rightCustomer?.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                var isEmailTaken = await unitOfWork.CustomerRepository.ExistsEmail(c =>
                    c.Email!.ToLower() == request.Email!.ToLower() && c.Id != customerId);

                if (isEmailTaken)
                {
                    return new ApiResponse<CustomerDto>(
                        StatusCodes.Status409Conflict,
                        $"The email '{request.Email}' is already used by another customer."
                    );
                }
            }
            if (!string.IsNullOrWhiteSpace(request.ImageBase64))
            {
                string uploadedUrl;
                string? oldImageUrl = rightCustomer?.ImageUrls;

                // ✅ FIX: Upload new blob first, then delete old one only after upload succeeds
                // This ensures that if upload fails, the old blob is still available
                var newUri = await blobStorageService.UploadBase64Async(request.ImageBase64!, BlobStorageConstants.CustomerFolder);
                uploadedUrl = newUri.AbsoluteUri;

                // Only delete old blob after successful upload
                if (!string.IsNullOrEmpty(oldImageUrl))
                {
                    try
                    {
                        await blobStorageService.DeleteBase64Async(oldImageUrl);
                    }
                    catch (Exception ex)
                    {
                        // Log but don't fail - old blob might not exist or already deleted
                        Console.WriteLine($"Warning: Could not delete old blob: {ex.Message}");
                    }
                }

                rightCustomer!.ImageUrls = uploadedUrl;
            }

            mapper.Map(request, rightCustomer);
            rightCustomer!.Shifts = request.Shifts;

            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            var cacheKey = GetCustomerKey(customerId);

            await cache.RemoveTrackedKeysAsync(cacheKey);
            await cache.RemoveTrackedKeysAsync(CustomerCachePath);

            return new ApiResponse<CustomerDto>(
                StatusCodes.Status200OK,
                nameof(Customer) + ResponseMessages.Updated,
                mapper.GetResult<Customer, CustomerDto>(rightCustomer!)
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<CustomerDto>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<CustomerDto>(StatusCodes.Status500InternalServerError, "An error occurred while updating the customer: " + ex.Message);
        }
    }

    #endregion

    #region Private Helper Methods

    private static Dictionary<string, int> CreateEmptyStatusSummary()
    {
        return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Online"] = 0,
            ["Offline"] = 0,
            ["Warning"] = 0,
            ["Error"] = 0,
            ["DownTime"] = 0,
            ["other"] = 0,
            ["N/A"] = 0
        };
    }

    // ✅ OPTIMIZATION: Single-pass processing with pre-allocated dictionary
    private static CustomerCardDto BuildCustomerCard(
        Customer customer,
        Dictionary<Guid, MachineLog> latestLogsByMachineId,
        Dictionary<Guid, MachineStatusSetting> statusSettingsByMachineId)
    {
        var statusSummary = CreateEmptyStatusSummary();
        int totalMachines = customer.Machine?.Count ?? 0;

        if (customer.Machine != null && customer.Machine.Count > 0)
        {
            // ✅ OPTIMIZATION: Pre-allocate capacity and process in single pass
            int machinesWithLogs = 0;

            foreach (var machine in customer.Machine)
            {
                if (latestLogsByMachineId.TryGetValue(machine.Id, out var latestLog))
                {
                    machinesWithLogs++;

                    // ✅ OPTIMIZATION: Use TryGetValue instead of GetValueOrDefault for better performance
                    statusSettingsByMachineId.TryGetValue(machine.Id, out var statusSetting);

                    var status = DetermineStatus(latestLog, statusSetting);
                    statusSummary[status]++;

                    // ✅ NEW LOGIC: If machine has any connected status (Error, Warning, DownTime, or other),
                    // it means machine is connected to server, so also increment Online count
                    // Note: If status is already "Online", it's already counted, so we don't double-count
                    if (status == "Error" || status == "Warning" || status == "DownTime" || status == "other")
                    {
                        statusSummary["Online"]++;
                    }
                }
            }

            statusSummary["N/A"] = totalMachines - machinesWithLogs;
        }
        else
        {
            statusSummary["N/A"] = totalMachines;
        }

        return new CustomerCardDto
        {
            Id = customer.Id,
            Name = customer.Name,
            TimeZone = customer.TimeZone,
            ImageUrl = customer.ImageUrls,
            TotalMachines = totalMachines,
            StatusSummary = statusSummary
        };
    }

    private static string DetermineStatus(MachineLog latestLog, MachineStatusSetting? statusSetting)
    {
        MachineStatus? machineStatus = null;
        if (!string.IsNullOrEmpty(latestLog.Status) &&
            System.Enum.TryParse(latestLog.Status, true, out MachineStatus parsedStatus))
        {
            machineStatus = parsedStatus;
        }

        // ✅ Treat explicit Offline status as Offline even if Type == "other"
        if (machineStatus == MachineStatus.Offline) return "Offline";

        // ✅ Check for "other" type logs (unmatched downtime reasons)
        if (latestLog.Type?.Equals("other", StringComparison.OrdinalIgnoreCase) == true)
            return "other";

        if (latestLog.Type?.Equals("DownTimelog", StringComparison.OrdinalIgnoreCase) == true)
            return "DownTime";

        if (machineStatus == MachineStatus.Error) return "Error";
        if (machineStatus == MachineStatus.Warning) return "Warning";

        if (statusSetting?.Inputs != null && latestLog.Inputs != null)
        {
            var logSignal = latestLog.Inputs.FirstOrDefault()?.Signals;
            if (!string.IsNullOrEmpty(logSignal))
            {
                var hasMatch = statusSetting.Inputs.Any(i =>
                    i.Signals?.Equals(logSignal, StringComparison.OrdinalIgnoreCase) == true);

                if (hasMatch) return "Online";
            }
        }

        return "Offline";
    }

    private string GetCustomerKey(Guid id) => $"Customer:{id}";

    #endregion
}