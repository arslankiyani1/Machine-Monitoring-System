namespace MMS.Application.Services;

public class CustomerReportSettingService(
    ICustomerReportSettingRepository reportRepository,
    AutoMapperResult mapper,
    IUserContextService userContextService,
    IHistoricalStatsRepository historicalStatsRepository,
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    IReportGenerateService reportGenerateService,
    IBlobStorageService blobStorageService,
    ICacheService cache) : ICustomerReportSettingService
{
    private const string CachePrefix = "CustomerReportSetting:";
    private string GetCustomerReportKey(Guid id) => $"CustomerReportSetting:{id}/{userContextService.UserId}";

    public async Task<ApiResponse<GenerateReportResponseDto>> AddAsync(AddCustomerReportSettingDto request)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, request.CustomerId);

            if (request.MachineIds == null || request.MachineIds.Count == 0)
            {
                return new ApiResponse<GenerateReportResponseDto>(
                    StatusCodes.Status400BadRequest,
                    "Machine IDs are required to generate a report.",
                    default!
                );
            }

            if (request.ReportType == null || request.ReportType.Count == 0)
            {
                return new ApiResponse<GenerateReportResponseDto>(
                    StatusCodes.Status400BadRequest,
                    "Report types are required to generate a report.",
                    default!
                );
            }

            transaction = await unitOfWork.BeginTransactionAsync();

            var reportSetting = mapper.Map<CustomerReportSetting>(request);
            if (reportSetting != null)
            {
                reportSetting.Id = Guid.NewGuid();
                await reportRepository.AddAsync(reportSetting);
            }

            if (!request.IsCustomReport) // when false Scheduled report generation
            {
                await unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
                return new ApiResponse<GenerateReportResponseDto>(
                    StatusCodes.Status201Created,
                    $"{nameof(CustomerReportSetting)} scheduled successfully",
                    default!
                );
            }

            if (request.IsCustomReport)
            {
                if (!request.ReportPeriodStartDate.HasValue)
                {
                    throw new FluentValidation.ValidationException("Report period start date is required.");
                }

                if (!request.ReportPeriodEndDate.HasValue)
                {
                    throw new FluentValidation.ValidationException("Report period end date is required.");
                }

                if (request.ReportPeriodStartDate > request.ReportPeriodEndDate)
                {
                    throw new FluentValidation.ValidationException(
                        "Start date must be before or equal to end date."
                    );
                }

                if (request.ReportPeriodEndDate < request.ReportPeriodStartDate)
                {
                    throw new FluentValidation.ValidationException(
                        "End date must be after or equal to start date."
                    );
                }
            }

            var machineData = new Dictionary<Guid, List<HistoricalStats>>();
            var machineDetails = new Dictionary<Guid, MachineDto>();
            var machineErrors = new List<string>();

            foreach (var machineId in request.MachineIds!)
            {
                var stats = await historicalStatsRepository.GetByMachineIdAndDateRangeAsync(
                    machineId, request.ReportPeriodStartDate, request.ReportPeriodEndDate);
                machineData.Add(machineId, stats);

                try
                {
                    var machineResult = await unitOfWork.MachineRepository.GetAsync(machineId);
                    if (machineResult.IsLeft)
                    {
                        machineErrors.Add($"Machine with ID {machineId} not found.");
                        continue;
                    }
                    machineDetails.Add(machineId, mapper.Map<MachineDto>(machineResult.IfRight()));
                }
                catch (Exception ex)
                {
                    machineErrors.Add($"Error fetching machine {machineId}: {ex.Message}");
                }
            }

            if (machineDetails.Count != request.MachineIds.Count)
            {
                await transaction.RollbackAsync();
                return new ApiResponse<GenerateReportResponseDto>(
                    StatusCodes.Status400BadRequest,
                    $"Invalid machine details. Expected {request.MachineIds.Count} machines, but only {machineDetails.Count} were found. Errors: {string.Join("; ", machineErrors)}",
                    null
                );
            }

            var reportDatas = new Dictionary<Guid, ReportData>();
            var reportDataList = new List<ReportData>();

            foreach (var kv in machineData)
            {
                var reportData = CalculateReportData(request.ReportType!, machineDetails[kv.Key], kv.Value);
                reportDatas.Add(kv.Key, reportData);
                reportDataList.Add(reportData);
            }

            var fileBytes = await reportGenerateService.GenerateAsync(
                request.Format.ToString(), reportDatas, request.ReportType!, request.ReportName);

            // Consolidated file format mapping
            var (fileExtension, contentType) = GetFileFormatInfo(request.Format);
            var fileName = $"{request.ReportName}_{DateTime.UtcNow:yyyyMMddHHmmss}.{fileExtension}";
            var blobLink = await blobStorageService.UploadReportAsync(fileBytes, fileName, BlobStorageConstants.ReportsFolder);
            var emailSent = await emailService.SendReportAsync(request.Email, request.ReportName, fileBytes, request.Format.ToString());

            var customerReport = new CustomerReport
            {
                Id = Guid.NewGuid(),
                CustomerReportSettingId = reportSetting!.Id,
                CustomerId = request.CustomerId,
                ReportName = request.ReportName,
                BlobLink = blobLink.ToString(),
                Format = request.Format,
                IsSent = emailSent,
                GeneratedDate = DateTime.UtcNow
            };

            await unitOfWork.CustomerReportRepository.AddAsync(customerReport);
            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ApiResponse<GenerateReportResponseDto>(
                StatusCodes.Status201Created,
                nameof(CustomerReportSetting) + ResponseMessages.Added,
                new GenerateReportResponseDto
                {
                    FileBytes = fileBytes,
                    FileName = fileName,
                    ContentType = contentType,
                    ReportData = reportDataList
                }
            );
        }
        catch (Exception ex)
        {
            if (transaction != null)
                await transaction.RollbackAsync();

            return new ApiResponse<GenerateReportResponseDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while adding the report: {ex.Message}",
                null);
        }
        finally
        {
            transaction?.Dispose();
        }
    }

    private static (string Extension, string ContentType) GetFileFormatInfo(ReportFormat format)
    {
        return format.ToString().ToLower() switch
        {
            "pdf" => ("pdf", "application/pdf"),
            "csv" => ("csv", "text/csv"),
            "excel" => ("xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
            _ => ("pdf", "application/pdf")
        };
    }

    public async Task<ApiResponse<string>> DeleteAsync(Guid id)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var result = await reportRepository.GetAsync(id);

            return await result.MatchAsync(
                async report =>
                {
                    await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, report.CustomerId);

                    await reportRepository.DeleteAsync(report.Id);
                    await unitOfWork.SaveChangesAsync();

                    await cache.RemoveTrackedKeysAsync(CachePrefix);
                    await transaction.CommitAsync();
                    return new ApiResponse<string>(StatusCodes.Status200OK, nameof(CustomerReportSetting) + ResponseMessages.Deleted, null);
                },
                error =>
                {
                    var status = error switch
                    {
                        EntitySoftDeleted => StatusCodes.Status410Gone,
                        EntityNotFound => StatusCodes.Status404NotFound,
                        _ => StatusCodes.Status400BadRequest
                    };

                    return Task.FromResult(new ApiResponse<string>(status, $"CustomerReportSetting with ID {id} not accessible: {error.GetType().Name}", null));
                });
        }
        catch (UnauthorizedAccessException ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<string>(StatusCodes.Status403Forbidden, ex.Message, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<string>(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}", null);
        }
    }

    public async Task<ApiResponse<CustomerReportSettingDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var cacheKey = GetCustomerReportKey(id);
            var cached = await cache.GetAsync<CustomerReportSettingDto>(cacheKey);
            if (cached is not null)
            {
                return new ApiResponse<CustomerReportSettingDto>(
                    StatusCodes.Status200OK,
                    nameof(MMS.Application.Models.SQL.Customer) + ResponseMessages.Get,
                    cached
                );
            }

            var result = await reportRepository.GetAsync(id);

            return await result.MatchAsync(
                async report =>
                {
                    await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, report.CustomerId);
                    return new ApiResponse<CustomerReportSettingDto>
                    (StatusCodes.Status200OK, nameof
                    (CustomerReportSetting) + ResponseMessages.Get,
                    mapper.Map<CustomerReportSettingDto>(report));
                },
                error =>
                {
                    var status = error switch
                    {
                        EntitySoftDeleted => StatusCodes.Status410Gone,
                        EntityNotFound => StatusCodes.Status404NotFound,
                        _ => StatusCodes.Status400BadRequest
                    };

                    return Task.FromResult(new ApiResponse<CustomerReportSettingDto>(status, error.Message, null));
                });
        }
        catch (Exception ex)
        {
            return new ApiResponse<CustomerReportSettingDto>(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}", null);
        }
    }

    public async Task<ApiResponse<IEnumerable<CustomerReportSettingDto>>> GetListAsync(
        PageParameters pageParameters,
        ReportType? reportType,
        bool? isActive,
        ReportFrequency? reportFrequency,
        bool? isCustomReport,
        Guid? customerId)
    {
        try
        {
            Expression<Func<CustomerReportSetting, bool>> searchExpr = x =>
                string.IsNullOrEmpty(pageParameters.Term) ||
                x.ReportName.ToLower().Contains(pageParameters.Term.ToLower());

            var reportFilters = new List<Expression<Func<CustomerReportSetting, bool>>>();

            if (reportFrequency.HasValue)
                reportFilters.Add(r => r.Frequency == reportFrequency.Value);

            if (isActive.HasValue)
                reportFilters.Add(r => r.IsActive == isActive.Value);

            if (isCustomReport.HasValue)
                reportFilters.Add(r => r.IsCustomReport == isCustomReport.Value);

            // Add customerId filter if provided
            if (customerId.HasValue)
            {
                reportFilters.Add(r => r.CustomerId == customerId.Value);
            }
            else
            {
                // Only apply access restrictions if customerId is not explicitly provided
                var accessibleCustomerIds = CustomerAccessHelper.GetAccessibleCustomerIds(userContextService);

                if (accessibleCustomerIds != null && accessibleCustomerIds.Any())
                    reportFilters.Add(r => accessibleCustomerIds.Contains(r.CustomerId));
            }

            var reports = await reportRepository.GetReportSettingRepositoryAsync(
                pageParameters,
                searchExpr,
                reportFilters,
                q => q.OrderByDescending(r => r.CreatedAt));

            if (reportType.HasValue)
            {
                reports = reports.Where(r => r.ReportType != null && r.ReportType.Any(rt => rt == reportType.Value)).ToList();
            }

            foreach (var item in reports)
            {
                if ( string.IsNullOrEmpty(item.CustomerReports?.BlobLink))
                {
                    item.IsActive = false;
                }
                else
                {
                    item.IsActive = true;
                }
            }

            var mapped = mapper.Map<IEnumerable<CustomerReportSettingDto>>(reports);

            return new ApiResponse<IEnumerable<CustomerReportSettingDto>>(
                StatusCodes.Status200OK,
                nameof(CustomerReportSetting) + ResponseMessages.GetAll,
                mapped);
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<CustomerReportSettingDto>>(
                StatusCodes.Status500InternalServerError,
                "An error occurred while retrieving the customer reports: " + ex.Message);
        }
    }

    public async Task<ApiResponse<CustomerReportSettingDto>> UpdateAsync(Guid id, UpdateCustomerReportSettingDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, request.CustomerId);

            var result = await reportRepository.GetAsync(id);

            if (result.IsLeft)
            {
                var error = result.IfLeft();
                var status = error switch
                {
                    EntitySoftDeleted => StatusCodes.Status410Gone,
                    EntityNotFound => StatusCodes.Status404NotFound,
                    _ => StatusCodes.Status400BadRequest
                };

                return new ApiResponse<CustomerReportSettingDto>(status, $"CustomerReportSetting with ID {id} not accessible: {error!.GetType().Name}", null);
            }

            var report = result.IfRight();

            mapper.Map(request, report);
            await unitOfWork.SaveChangesAsync();

            await cache.RemoveTrackedKeysAsync(CachePrefix);
            await transaction.CommitAsync();
            var dto = mapper.GetResult<CustomerReportSetting, CustomerReportSettingDto>(report);
            return new ApiResponse<CustomerReportSettingDto>(StatusCodes.Status200OK, nameof(CustomerReportSetting) + ResponseMessages.Updated, dto);
        }

        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<CustomerReportSettingDto>(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}", null);
        }
    }

    private ReportData CalculateReportData(IEnumerable<ReportType> reportTypes, MachineDto machineDto, List<HistoricalStats> stats)
    {
        if (!stats.Any())
        {
            return new ReportData
            {
                MachineDetails = new Dictionary<string, string>
                {
                    { "SerialNumber", machineDto.SerialNumber },
                    { "MachineName", machineDto.MachineName },
                    { "MachineModel", machineDto.MachineModel }
                },
                Summary = new Dictionary<string, double>(),
                DailyRows = new List<Dictionary<string, object>>()
            };
        }

        // Calculate summary using helper method
        var summary = new Dictionary<string, double>();
        foreach (var typeEnum in reportTypes)
        {
            var type = typeEnum.ToString();
            summary[type] = CalculateMetricValue(typeEnum, stats);
        }

        // Group by date and create daily rows
        var grouped = stats.GroupBy(s => s.GeneratedDate.Date).OrderBy(g => g.Key);
        var dailyRows = new List<Dictionary<string, object>>();
        foreach (var group in grouped)
        {
            var row = new Dictionary<string, object>
            {
                ["Date"] = group.Key
            };
            foreach (var typeEnum in reportTypes)
            {
                var type = typeEnum.ToString();
                row[type] = CalculateMetricValue(typeEnum, group);
            }
            dailyRows.Add(row);
        }

        return new ReportData
        {
            MachineDetails = new Dictionary<string, string>
            {
                { "SerialNumber", machineDto.SerialNumber },
                { "MachineName", machineDto.MachineName },
                { "MachineModel", machineDto.MachineModel }
            },
            Summary = summary,
            DailyRows = dailyRows
        };
    }
    private static double CalculateMetricValue(ReportType type, IEnumerable<HistoricalStats> stats)
    {
        return type.ToString() switch
        {
            "OEE" => stats.Average(s => s.OEE),
            "Availability" => stats.Average(s => s.Availability),
            "Performance" => stats.Average(s => s.Performance),
            "Quality" => stats.Average(s => s.Quality),
            "Utilization" => stats.Average(s => s.Utilization),
            "Downtime" => stats.Sum(s => s.DownTime),
            "RequiredQty" => stats.Sum(s => s.RequiredQty),
            "CompletedQty" => stats.Sum(s => s.QtyCompleted),
            "GoodQty" => stats.Sum(s => s.QtyGood),
            "BadQty" => stats.Sum(s => s.QtyBad),
            _ => 0
        };
    }
}