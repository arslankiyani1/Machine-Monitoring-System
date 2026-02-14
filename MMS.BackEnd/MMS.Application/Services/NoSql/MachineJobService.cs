using MMS.Application.Ports.In.NoSql.MachineJob.Dto;

namespace MMS.Application.Services.NoSql;

public class MachineJobService(
    IMachineJobRepository repository,
    IUserContextService userContextService,
    AutoMapperResult autoMapperResult,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMachineLogRepository machineLogRepository, 
    IServiceProvider serviceProvider,
    IBlobStorageService blobStorageService) : IMachineJobService
{
    private const string AllJobsCacheKey = "machine_jobs_all";

    public async Task<ApiResponse<Ports.In.NoSql.MachineJob.Dto.MachineJobDto>> CreateAsync(MachineJobAddDto dto)
    {
        try
        {
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, dto.CustomerId);

            var customer = await unitOfWork.CustomerRepository.GetMachinesByCustomerIdAsync(dto.CustomerId);
            if (customer is null)
            {
                return new ApiResponse<Ports.In.NoSql.MachineJob.Dto.MachineJobDto>(
                    StatusCodes.Status404NotFound,
                    $"Customer with ID {dto.CustomerId} does not exist."
                );
            }

            var machines = await unitOfWork.MachineRepository.GetByIdsAsync(dto.MachineIds);
            if (machines is null || !machines.Any())
            {
                return new ApiResponse<Ports.In.NoSql.MachineJob.Dto.MachineJobDto>(
                    StatusCodes.Status404NotFound,
                    $"One or more machine IDs do not exist."
                );
            }

            using var scope = serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            var operatorResponse = await userService.GetUserByIdAsync(dto.OperatorId);
            if (operatorResponse.StatusCode != StatusCodes.Status200OK || operatorResponse.Data == null)
            {
                return new ApiResponse<Ports.In.NoSql.MachineJob.Dto.MachineJobDto>(
                    StatusCodes.Status400BadRequest,
                    $"Invalid OperatorId: {dto.OperatorId}. The operator does not exist in the system."
                );
            }

            dto = dto with
            {
                OperatorName = $"{operatorResponse.Data.FirstName} {operatorResponse.Data.LastName}"
            };

            var entity = autoMapperResult.Map<MachineJob>(dto);
            entity.Id = Guid.NewGuid().ToString();
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            await repository.AddAsync(entity);

            var result = autoMapperResult.Map<Ports.In.NoSql.MachineJob.Dto.MachineJobDto>(entity);

            return new ApiResponse<Ports.In.NoSql.MachineJob.Dto.MachineJobDto>(
                StatusCodes.Status201Created,
                "Machine job created successfully.",
                result
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<Ports.In.NoSql.MachineJob.Dto.MachineJobDto>(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            return new ApiResponse<Ports.In.NoSql.MachineJob.Dto.MachineJobDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<string>> DeleteAsync(string id)
    {
        int deletedCount = 0;
        try
        {
            var existing = await repository.GetByIdAsync(id);
            if (existing is null)
            {
                return new ApiResponse<string>(
                    StatusCodes.Status200OK,
                    $"Machine job with ID {id} not found."
                );
            }

            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, existing.CustomerId);

            if (existing.Attachments != null && existing.Attachments.Any())
            {
                var validAttachments = existing.Attachments
                    .Where(x => !string.IsNullOrWhiteSpace(x) && x != "string")
                    .ToList();

                if (validAttachments.Any())
                {
                    deletedCount = await blobStorageService.DeleteAllAsync(validAttachments);
                }
            }

            await repository.DeleteAsync(id);

            await cache.RemoveAsync(AllJobsCacheKey);
            await cache.RemoveAsync($"machine_job_{id}");

            return new ApiResponse<string>(
                StatusCodes.Status200OK,
                $"Machine job deleted successfully. {deletedCount} blob(s) removed.",
                ""
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<string>(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<MachineJob>>> GetAllAsync(PageParameters pageParameters,
        Guid? machineId,
        Guid? customerId = null)
    {
        try
        {
            var validCustomerIds = CustomerAccessHelper.GetAccessibleCustomerIds(userContextService);

            var queryCustomerIds = customerId.HasValue ? new List<Guid> { customerId.Value } : validCustomerIds;
            var result = await repository.GetPagedAsync(queryCustomerIds!, pageParameters, machineId);

            if (result == null || !result.Any())
            {
                return new ApiResponse<IEnumerable<MachineJob>>(
                    StatusCodes.Status200OK,
                    "No machine jobs found.",
                    new List<MachineJob>()
                );
            }

            return new ApiResponse<IEnumerable<MachineJob>>(
                StatusCodes.Status200OK,
                "Fetched from DB",
                result
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<IEnumerable<MachineJob>>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<MachineJob>>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<MachineJob?>> GetByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new ApiResponse<MachineJob?>(
                    StatusCodes.Status400BadRequest,
                    "Invalid request. Job ID cannot be null or empty."
                );
            }

            var job = await repository.GetByIdAsync(id);

            if (job is null)
            {
                return new ApiResponse<MachineJob?>(
                    StatusCodes.Status404NotFound,
                    $"No Machine Job found with ID: {id}."
                );
            }
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, job.CustomerId);

            return new ApiResponse<MachineJob?>(
                StatusCodes.Status200OK,
                "Machine job retrieved successfully.",
                job
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<MachineJob?>(
                StatusCodes.Status403Forbidden,
                $"Access denied: {ex.Message}"
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<MachineJob?>(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred while retrieving the machine job. Details: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<JobStatusSummaryDto>> GetJobSummaryAsync(PageParameters pageParameters, Guid customerId)
    {
        try
        {
            if (customerId == Guid.Empty)
            {
                return new ApiResponse<JobStatusSummaryDto>(
                    StatusCodes.Status400BadRequest,
                    "Invalid request. Customer ID cannot be empty."
                );
            }
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, customerId);

            var summary = await repository.GetJobSummaryAsync(customerId, pageParameters);

            if (summary == null)
            {
                return new ApiResponse<JobStatusSummaryDto>(
                    StatusCodes.Status404NotFound,
                    $"No machine jobs found for customer ID: {customerId}."
                );
            }

            return new ApiResponse<JobStatusSummaryDto>(
                StatusCodes.Status200OK,
                "Job summary retrieved successfully.",
                summary
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<JobStatusSummaryDto>(
                StatusCodes.Status403Forbidden,
                $"Access denied: {ex.Message}"
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<JobStatusSummaryDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<Ports.In.NoSql.MachineJob.Dto.MachineJobDto>> UpdateAsync(MachineJobUpdateDto dto)
    {
        try
        {
            // 1️⃣ Fetch existing job
            var existing = await repository.GetByIdAsync(dto.Id);
            if (existing is null)
            {
                return new ApiResponse<Ports.In.NoSql.MachineJob.Dto.MachineJobDto>(
                    StatusCodes.Status204NoContent,
                    $"Machine job with ID {dto.Id} not found."
                );
            }

            // 2️⃣ Validate customer access
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, dto.CustomerId);

            // 3️⃣ Handle attachments safely
            var oldAttachments = existing.Attachments ?? new List<string>();
            var newAttachments = dto.Attachments ?? new List<string>();

            var toDelete = oldAttachments.Except(newAttachments).ToList();
            var toAdd = newAttachments.Except(oldAttachments).ToList();

            if (toDelete.Any())
            {
                var validUrls = toDelete
                    .Where(x => !string.IsNullOrEmpty(x) && Uri.IsWellFormedUriString(x, UriKind.Absolute))
                    .ToList();

                if (validUrls.Count != toDelete.Count)
                {
                    return new ApiResponse<Ports.In.NoSql.MachineJob.Dto.MachineJobDto>(
                        StatusCodes.Status400BadRequest,
                        "Some blob URLs are invalid."
                    );
                }

                await blobStorageService.DeleteAllAsync(validUrls);
            }

            var updatedAttachments = oldAttachments
                .Except(toDelete)
                .Union(toAdd)
                .Distinct()
                .ToList();

            // 4️⃣ Use AutoMapper to update entity
            autoMapperResult.Map(dto, existing);

            // Override fields AFTER mapping (dto should NOT overwrite these)
            existing.Attachments = updatedAttachments;
            existing.UpdatedAt = DateTime.UtcNow;

            // 5️⃣ Handle downtime events separately
            existing.DowntimeEvents ??= new List<DowntimeEvent>();
            existing.DowntimeEvents.Clear();

            if (dto.DowntimeEvents != null && dto.DowntimeEvents.Any())
            {
                existing.DowntimeEvents = dto.DowntimeEvents.Select(evt => new DowntimeEvent
                {
                    Reason = evt.Reason,
                    StartTime = evt.StartTime,
                    EndTime = evt.EndTime,
                    Duration = evt.Duration
                }).ToList();
            }

            // 6️⃣ Save changes
            await repository.UpdateAsync(existing);

            // 7️⃣ Map to DTO and return full object
            var resultDto = autoMapperResult.Map<Ports.In.NoSql.MachineJob.Dto.MachineJobDto>(existing);

            return new ApiResponse<Ports.In.NoSql.MachineJob.Dto.MachineJobDto>(
                StatusCodes.Status200OK,
                "Machine job updated successfully.",
                resultDto
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<Ports.In.NoSql.MachineJob.Dto.MachineJobDto>(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return new ApiResponse<Ports.In.NoSql.MachineJob.Dto.MachineJobDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<JobDetailsStats>> GetJobDetailsStatsAsync(string jobId)
    {
        try
        {
            // ========== VALIDATION ==========
            if (string.IsNullOrWhiteSpace(jobId))
            {
                return new ApiResponse<JobDetailsStats>(
                    StatusCodes.Status400BadRequest,
                    "Invalid request. Job ID cannot be null or empty."
                );
            }

            // ========== GET JOB ==========
            var job = await repository.GetByIdAsync(jobId);
            if (job == null)
            {
                return new ApiResponse<JobDetailsStats>(
                    StatusCodes.Status404NotFound,
                    $"No Machine Job found with ID: {jobId}."
                );
            }

            // ========== VALIDATE ACCESS ==========
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, job.CustomerId);

            // ========== VALIDATE SCHEDULE ==========
            if (job.Schedule == null)
            {
                return new ApiResponse<JobDetailsStats>(
                    StatusCodes.Status400BadRequest,
                    "Invalid job schedule. Schedule cannot be null."
                );
            }

            double plannedSec = (job.Schedule.PlannedEnd - job.Schedule.PlannedStart).TotalSeconds;
            if (plannedSec <= 0)
            {
                return new ApiResponse<JobDetailsStats>(
                    StatusCodes.Status400BadRequest,
                    "Invalid job schedule. Planned time must be positive."
                );
            }

            // ========== FETCH LOGS BY JOB ID ONLY ==========
            // ✅ SIMPLIFIED: Only get logs that have this JobId
            var jobLogs = await machineLogRepository.GetByJobIdAsync(jobId);

            // ========== CALCULATE METRICS ==========
            var metrics = MachineLogMetricsCalculator.CalculateJobMetricsFromLogs(jobLogs, job);

            // ========== CALCULATE STATUS PERCENTAGES ==========
            var utilizationResult = MachineLogService.MachineLogStatusCalculator.CalculateStatusPercentages(
                jobLogs,
                job.Schedule.PlannedStart,
                job.Schedule.PlannedEnd
            );

            // ========== BUILD RESPONSE ==========
            var downtimeResponse = new DowntimeApiResponseDto
            {
                TotalDowntime = metrics.TotalDowntimeSeconds,
                TotalDowntimePercent = metrics.TotalDowntimePercent,
                DowntimeMetrics = metrics.DowntimeBreakdown
            };

            var stats = new JobDetailsStats(
                metrics.Oee,
                metrics.Performance,
                metrics.Availability,
                metrics.Quality,
                downtimeResponse,
                metrics.Utilization,
                utilizationResult.StatusPercent
            );

            return new ApiResponse<JobDetailsStats>(
                StatusCodes.Status200OK,
                "Job details stats retrieved successfully.",
                stats
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<JobDetailsStats>(
                StatusCodes.Status403Forbidden,
                $"Access denied: {ex.Message}"
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<JobDetailsStats>(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred while retrieving job stats. Details: {ex.Message}"
            );
        }
    }
}
