namespace MMS.Application.Services;

public class MachineMaintenanceTaskService(
    IMachineMaintenanceTaskRepository taskRepository,
    AutoMapperResult mapper,
    IUnitOfWork unitOfWork,
    IUserContextService userContextService,
    IBlobStorageService blobStorageService,
    IUserService userService,
    //IMachineJobRepository jobRepository,
    ICacheService cache) : IMachineMaintenanceTaskService
{
    private const string CachePrefix = "MachineMaintenanceTask:";

    public async Task<ApiResponse<MachineMaintenanceTaskDto>> AddAsync(AddMachineMaintenanceTaskDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var now = DateTime.UtcNow;

            if (request.ScheduledDate > request.DueDate )
            {
                return new ApiResponse<MachineMaintenanceTaskDto>(
                    StatusCodes.Status400BadRequest,
                    "Invalid dates: scheduled date cannot be greater than due date"
                );
            }
         
            if ( request.DueDate < now)
            {
                return new ApiResponse<MachineMaintenanceTaskDto>(
                    StatusCodes.Status400BadRequest,
                    "Invalid dates: due date cannot be in the past."
                );
            }
            var machine = await unitOfWork.MachineRepository.GetAsync(request.MachineId);
            if (machine.IsLeft)
            {
                return new ApiResponse<MachineMaintenanceTaskDto>(
                    StatusCodes.Status400BadRequest,
                    $"Machine with ID {request.MachineId} does not exist."
                );
            }

            var machineEntity = machine.IfRight();
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, machineEntity!.CustomerId);

            var taskEntity = mapper.Map<MachineMaintenanceTask>(request);
            taskEntity.CustomerId = machineEntity.CustomerId;
            taskEntity.MachineId = machineEntity.Id;

            var currentUserId = userContextService.UserId.ToString();
            if (!string.IsNullOrEmpty(currentUserId))
            {
                var userResult = await userService.GetUserByIdAsync(currentUserId.ToGuid());
                if (userResult.StatusCode == 200 && userResult.Data != null)
                {
                    var user = userResult.Data;
                    taskEntity.AssignedToUserId = user.UserId;
                    taskEntity.AssignedToUserName =
                        string.IsNullOrWhiteSpace(user.FirstName) && string.IsNullOrWhiteSpace(user.LastName)
                        ? user.Username
                        : $"{user.FirstName} {user.LastName}".Trim();
                }
            }

            if (request.Files is { Count: > 0 })
            {
                try
                {
                    var fileUris = await blobStorageService.UploadAllAsync(request.Files);
                    taskEntity.Attachments = fileUris.Data!.Select(uri => uri.ToString()).ToList();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new ApiResponse<MachineMaintenanceTaskDto>(
                        StatusCodes.Status400BadRequest,
                        $"File upload failed: {ex.Message}"
                    );
                }
            }

            var added = await taskRepository.AddAsync(taskEntity);
            await transaction.CommitAsync();

            return new ApiResponse<MachineMaintenanceTaskDto>(
                StatusCodes.Status201Created,
                "Machine maintenance task created successfully.",
                null
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<MachineMaintenanceTaskDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<string>> DeleteAsync(Guid taskId)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var result = await taskRepository.GetAsync(taskId);

            return await result.MatchAsync(
                async task =>
                {
                    var machineResult = await unitOfWork.MachineRepository.GetAsync(task.MachineId);
                    if (machineResult.IsLeft)
                    {
                        return new ApiResponse<string>(
                            StatusCodes.Status200OK,
                            $"Machine with ID {task.MachineId} not found."
                        );
                    }
                    var machine = machineResult.IfRight();
                    await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, machine!.CustomerId);


                    if (task.Attachments != null && task.Attachments.Any())
                    {
                        await blobStorageService.DeleteAllAsync(task.Attachments);
                    }

                    await taskRepository.DeleteAsync(task.Id);

                    await unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();
                  //  await cache.RemoveTrackedKeysAsync(CachePrefix);

                    return new ApiResponse<string>(
                        StatusCodes.Status200OK,
                        nameof(MachineMaintenanceTask) + ResponseMessages.Deleted,
                        null
                    );
                },
                error =>
                {
                    var status = error switch
                    {
                        EntitySoftDeleted => StatusCodes.Status410Gone,
                        EntityNotFound => StatusCodes.Status404NotFound,
                        _ => StatusCodes.Status400BadRequest
                    };

                    return Task.FromResult(new ApiResponse<string>(
                        status,
                        $"MachineMaintenanceTask with ID {taskId} not accessible: {error.GetType().Name}",
                        null
                    ));
                });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return new ApiResponse<string>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}",
                null
            );
        }
    }

    public async Task<ApiResponse<MachineMaintenanceTaskDto>> GetByIdAsync(Guid taskId)
    {
        try
        {
            var result = await taskRepository.GetAsync(taskId);

            return await result.MatchAsync(
                async task =>
                {
                    var machineResult = await unitOfWork.MachineRepository.GetAsync(task.MachineId);
                    if (machineResult.IsLeft)
                    {
                        return new ApiResponse<MachineMaintenanceTaskDto>(
                            StatusCodes.Status200OK,
                            $"Machine with ID {task.MachineId} not found."
                        );
                    }

                    var machine = machineResult.IfRight();
                    await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, machine!.CustomerId);

                    var dto = mapper.Map<MachineMaintenanceTaskDto>(task);

                    return new ApiResponse<MachineMaintenanceTaskDto>(
                        StatusCodes.Status200OK,
                        nameof(MachineMaintenanceTask) + ResponseMessages.Get,
                        dto
                    );
                },
                error =>
                {
                    var status = error switch
                    {
                        EntitySoftDeleted => StatusCodes.Status410Gone,
                        EntityNotFound => StatusCodes.Status404NotFound,
                        _ => StatusCodes.Status400BadRequest
                    };

                    return Task.FromResult(new ApiResponse<MachineMaintenanceTaskDto>(
                        status,
                        error.Message,
                        null
                    ));
                });
        }
        catch (Exception ex)
        {
            return new ApiResponse<MachineMaintenanceTaskDto>(
                StatusCodes.Status500InternalServerError,
                "An error occurred while retrieving the machine maintenance task: " + ex.Message,
                null
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<MachineMaintenanceTaskDto>>> GetListAsync(
    PageParameters pageParameters, Guid? machineId)
    {
        try
        {
            var term = pageParameters.Term?.Trim().ToLower();

            Expression<Func<MachineMaintenanceTask, bool>> searchExpr = t => true;
            if (!string.IsNullOrEmpty(term))
            {
                searchExpr = t => EF.Functions.Like(t.MaintenanceTaskName.ToLower(), $"%{term}%");
            }

            var taskFilters = new List<Expression<Func<MachineMaintenanceTask, bool>>>();

            if (machineId.HasValue && machineId.Value != Guid.Empty)
            {
                taskFilters.Add(t => t.MachineId == machineId.Value);
            }
      
            var tasks = await taskRepository.GetListAsync(
                pageParameters,
                searchExpr,
                taskFilters,
                q => q.OrderByDescending(t => t.Priority)
                      .ThenByDescending(t => t.CreatedAt)
            );

            return new ApiResponse<IEnumerable<MachineMaintenanceTaskDto>>(
                StatusCodes.Status200OK,
                nameof(MachineMaintenanceTask) + ResponseMessages.GetAll,
                mapper.Map<IEnumerable<MachineMaintenanceTaskDto>>(tasks)
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<MachineMaintenanceTaskDto>>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while retrieving the maintenance tasks list: {ex.Message}"
            );
        }
    }


    public async Task<ApiResponse<MachineMaintenanceTaskDto>> UpdateAsync(Guid taskId, UpdateMachineMaintenanceTaskDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var now = DateTime.UtcNow;
            if (request.ScheduledDate > request.DueDate)
            {
                return new ApiResponse<MachineMaintenanceTaskDto>(
                    StatusCodes.Status400BadRequest,
                    "Invalid dates: scheduled date cannot be greater than due date"
                );
            }

            if (request.DueDate < now)
            {
                return new ApiResponse<MachineMaintenanceTaskDto>(
                    StatusCodes.Status400BadRequest,
                    "Invalid dates: due date cannot be in the past."
                );
            }
        
            var result = await taskRepository.GetAsync(taskId);

            return await result.MatchAsync(
                async task =>
                {
                    var machineResult = await unitOfWork.MachineRepository.GetAsync(task.MachineId);
                    if (machineResult.IsLeft)
                    {
                        return new ApiResponse<MachineMaintenanceTaskDto>(
                            StatusCodes.Status200OK,
                            $"Machine with ID {task.MachineId} not found.",
                            null
                        );
                    }


                    var machine = machineResult.IfRight();
                    await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, machine!.CustomerId);

                    // Filter and validate existing attachments from request
                    // Remove invalid entries like "[]", "[[], url]", empty strings, etc.
                    var validExistingAttachments = request.Attachments?
                        .Where(a => !string.IsNullOrWhiteSpace(a) &&
                                   !a.Trim().StartsWith("[") &&
                                   !a.Trim().Equals("[]") &&
                                   Uri.IsWellFormedUriString(a, UriKind.Absolute))
                        .ToList() ?? new List<string>();

                    // LOGIC: Determine whether to process attachment deletions
                    // If request.Attachments is null (not sent) → Delete ALL files
                    // If request.Attachments is sent AND validExistingAttachments is empty → Delete ALL files
                    // If request.Attachments is sent AND validExistingAttachments has URLs → Keep only those, delete others
                    var filesToDelete = new List<string>();
                    var finalAttachments = new List<string>();

                    if (request.Attachments == null)
                    {
                        // Attachments field not provided - delete all existing files
                        filesToDelete = task.Attachments?
                            .Where(a => !string.IsNullOrWhiteSpace(a))
                            .ToList() ?? new List<string>();
                        finalAttachments = new List<string>();
                    }
                    else if (validExistingAttachments.Count == 0)
                    {
                        // Attachments provided but empty or all invalid - delete ALL files
                        filesToDelete = task.Attachments?
                            .Where(a => !string.IsNullOrWhiteSpace(a))
                            .ToList() ?? new List<string>();
                        finalAttachments = new List<string>();
                    }
                    else
                    {
                        // Attachments has valid URLs - keep only those, delete others
                        filesToDelete = task.Attachments?
                            .Where(a => !string.IsNullOrWhiteSpace(a))
                            .Except(validExistingAttachments)
                            .ToList() ?? new List<string>();

                        finalAttachments = new List<string>(validExistingAttachments);
                    }

                    // Delete files from blob storage if any
                    if (filesToDelete.Any())
                    {
                        try
                        {
                            await blobStorageService.DeleteAllAsync(filesToDelete);
                        }
                        catch
                        {
                            // Log error but continue - don't fail update if blob deletion fails
                        }
                    }

                    // Upload and add new files
                    if (request.Files != null && request.Files.Count > 0)
                    {
                        try
                        {
                            var fileUris = await blobStorageService.UploadAllAsync(request.Files);
                            if (fileUris?.Data != null)
                            {
                                finalAttachments.AddRange(fileUris.Data.Select(uri => uri.ToString()));
                            }
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            return new ApiResponse<MachineMaintenanceTaskDto>(
                                StatusCodes.Status400BadRequest,
                                $"File upload failed: {ex.Message}"
                            );
                        }
                    }

                    mapper.Map(request, task);
                    task.Attachments = finalAttachments;
                    var updated = await taskRepository.UpdateAsync(taskId, task);

                    await unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();
                    await cache.RemoveTrackedKeysAsync(CachePrefix);

                    var dto = mapper.GetResult<MachineMaintenanceTask, MachineMaintenanceTaskDto>(updated);
                    return new ApiResponse<MachineMaintenanceTaskDto>(
                        StatusCodes.Status200OK,
                        nameof(MachineMaintenanceTask) + ResponseMessages.Updated,
                        dto
                    );
                },
                error =>
                {
                    var status = error switch
                    {
                        EntitySoftDeleted => StatusCodes.Status410Gone,
                        EntityNotFound => StatusCodes.Status404NotFound,
                        _ => StatusCodes.Status400BadRequest
                    };

                    return Task.FromResult(new ApiResponse<MachineMaintenanceTaskDto>(
                        status,
                        $"MachineMaintenanceTask with ID {taskId} not accessible: {error.GetType().Name}",
                        null
                    ));
                });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<MachineMaintenanceTaskDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}",
                null
            );
        }
    }

    private async Task<List<Guid>> GetAllowedMachineIdsAsync()
    {
        try
        {
            Expression<Func<Machine, bool>> machineSearchExpr = m => true;
            var machineFilters = new List<Expression<Func<Machine, bool>>>();

            var customerIds = CustomerAccessHelper.GetAccessibleCustomerIds(userContextService);
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
}