namespace MMS.Application.Services.NoSql;

public class MachineStatusSettingService(
    IMachineStatusSettingRepository repository,
    IUserContextService userContextService,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IMachineStatusSettingService
{
    private const string CachePrefix = "MachineStatusSetting";

    public async Task<ApiResponse<IEnumerable<MachineStatusSetting>>> GetAllAsync()
    {
        try
        {
            var cacheKey = $"{CachePrefix}:All";
            var cached = await cacheService.GetAsync<IEnumerable<MachineStatusSetting>>(cacheKey);
            if (cached is not null)
            {
                return new ApiResponse<IEnumerable<MachineStatusSetting>>(
                    StatusCodes.Status200OK,
                    "Data fetched from cache.",
                    cached
                );
            }

            var allowedMachineIds = await GetAllowedMachineIdsAsync();
            var items = await repository.GetAllAsync(allowedMachineIds);

            await cacheService.SetAsync(cacheKey, items);

            return new ApiResponse<IEnumerable<MachineStatusSetting>>(
                StatusCodes.Status200OK,
                $"{nameof(MachineStatusSetting)} {ResponseMessages.GetAll}",
                items
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<IEnumerable<MachineStatusSetting>>(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<MachineStatusSetting>>(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<MachineStatusSetting>> GetByIdAsync(string id)
    {
        try
        {
            var cacheKey = $"{CachePrefix}:{id}";
            var cached = await cacheService.GetAsync<MachineStatusSetting>(cacheKey);
            if (cached is not null)
            {
                return new ApiResponse<MachineStatusSetting>(StatusCodes.Status200OK, "Fetched from cache.", cached);
            }

            var item = await repository.GetByIdAsync(id);
            if (item == null)
                return new ApiResponse<MachineStatusSetting>(StatusCodes.Status204NoContent, $"Entity with ID {id} not found.");

            var machineResult = await unitOfWork.MachineRepository.GetAsync(item.MachineId);
            if (machineResult.IsLeft)
                return new ApiResponse<MachineStatusSetting>(StatusCodes.Status404NotFound, $"Machine with ID {item.MachineId} not found.");

            var machine = machineResult.IfRight();
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, machine!.CustomerId);

            await cacheService.SetAsync(cacheKey, item);

            return new ApiResponse<MachineStatusSetting>(StatusCodes.Status200OK, $"{nameof(MachineStatusSetting)} {ResponseMessages.Get}", item);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<MachineStatusSetting>(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            return new ApiResponse<MachineStatusSetting>(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<MachineStatusSetting>> GetByMachineIdAsync(Guid machineId)
    {
        try
        {
            var cacheKey = $"{CachePrefix}:Machine:{machineId}";
            var cached = await cacheService.GetAsync<MachineStatusSetting>(cacheKey);
            if (cached is not null)
                return new ApiResponse<MachineStatusSetting>(StatusCodes.Status200OK, "Fetched from cache.", cached);

            var machineResult = await unitOfWork.MachineRepository.GetAsync(machineId);
            if (machineResult.IsLeft)
                return new ApiResponse<MachineStatusSetting>(StatusCodes.Status404NotFound, $"Machine with ID {machineId} not found.");

            var machine = machineResult.IfRight();
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, machine!.CustomerId);

            var item = await repository.GetByMachineIdAsync(machineId);
            if (item == null)
                return new ApiResponse<MachineStatusSetting>(StatusCodes.Status204NoContent, $"Entity with machine ID {machineId} not found.");

            await cacheService.SetAsync(cacheKey, item);

            return new ApiResponse<MachineStatusSetting>(StatusCodes.Status200OK, $"{nameof(MachineStatusSetting)} {ResponseMessages.Get}", item);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<MachineStatusSetting>(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            return new ApiResponse<MachineStatusSetting>(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        }
    }


    public async Task<ApiResponse<MachineStatusSetting>> CreateAsync(MachineStatusSetting entity)
    {
        try
        {
            if (!IsValid(entity))
                return new ApiResponse<MachineStatusSetting>(StatusCodes.Status400BadRequest, "Invalid input data.");

            var machineResult = await unitOfWork.MachineRepository.GetAsync(entity.MachineId);
            if (machineResult.IsLeft)
                return new ApiResponse<MachineStatusSetting>(StatusCodes.Status404NotFound, $"Machine with ID {entity.MachineId} does not exist.");

            var machine = machineResult.IfRight();
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, machine!.CustomerId);

            var existing = await repository.GetByMachineIdAsync(entity.MachineId);
            if (existing != null)
                return new ApiResponse<MachineStatusSetting>(StatusCodes.Status409Conflict, "Entity already exists.");

            entity.Id = Guid.NewGuid().ToString();
            await repository.AddAsync(entity);
            return new ApiResponse<MachineStatusSetting>(StatusCodes.Status201Created, "Created successfully.", entity);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<MachineStatusSetting>(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            return new ApiResponse<MachineStatusSetting>(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> UpdateAsync(string id, MachineStatusSetting updatedEntity)
    {
        try
        {

            var existing = await repository.GetByIdAsync(id);
            if (existing == null)
                return new ApiResponse<string>(StatusCodes.Status204NoContent, $"Entity with ID {id} not found.");

            if (!IsValid(updatedEntity))
                return new ApiResponse<string>(StatusCodes.Status400BadRequest, "Invalid input data.");

            var machineResult = await unitOfWork.MachineRepository.GetAsync(updatedEntity.MachineId);
            if (machineResult.IsLeft)
                return new ApiResponse<string>(StatusCodes.Status404NotFound, "Machine not found.");

            var machine = machineResult.IfRight();
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, machine!.CustomerId);

            existing.Inputs?.Clear();
            foreach (var input in updatedEntity.Inputs ?? [])
            {
                existing.Inputs?.Add(new MachineInput
                {
                    InputKey = input.InputKey,
                    Signals = input.Signals,
                    Color = input.Color,
                    Status = input.Status
                });
            }
            var cacheKey = $"{CachePrefix}:Machine:{machine.Id}";

            await cacheService.RemoveAsync(cacheKey);

            await repository.SaveChangesAsync();

            await cacheService.SetAsync(cacheKey, existing);


            return new ApiResponse<string>(StatusCodes.Status200OK, "Updated successfully.");
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

    public async Task<ApiResponse<string>> DeleteAsync(string id)
    {
        try
        {
            var existing = await repository.GetByIdAsync(id);
            if (existing == null)
                return new ApiResponse<string>(StatusCodes.Status204NoContent, $"Entity with ID {id} not found.");

            var machineResult = await unitOfWork.MachineRepository.GetAsync(existing.MachineId);
            if (machineResult.IsLeft)
                return new ApiResponse<string>(StatusCodes.Status404NotFound, "Machine not found.");

            var machine = machineResult.IfRight();
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, machine!.CustomerId);

            await repository.DeleteAsync(id);

            await cacheService.RemoveAsync($"{CachePrefix}:{id}");
            await cacheService.RemoveAsync($"{CachePrefix}:Machine:{existing.MachineId}");
            await cacheService.RemoveAsync($"{CachePrefix}:All");

            return new ApiResponse<string>(StatusCodes.Status200OK, "Deleted successfully.");
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

    private static bool IsValid(MachineStatusSetting setting)
    {
        if (setting.Inputs == null) return false;

        var signals = new HashSet<string>();

        foreach (var input in setting.Inputs)
        {
            if (string.IsNullOrWhiteSpace(input.Signals))
                return false;

            var parts = input.Signals.Split(',');
            if (parts.Length != 4 || !parts.All(p => p == "0" || p == "1"))
                return false;

            if (!signals.Add(input.Signals.Trim()))
                return false;
        }

        return true;
    }

    private async Task<List<Guid>> GetAllowedMachineIdsAsync()
    {
        try
        {
            var customerIds = CustomerAccessHelper.GetAccessibleCustomerIds(userContextService);
            var filters = new List<Expression<Func<Machine, bool>>>();

            if (customerIds?.Any() == true)
                filters.Add(m => customerIds.Contains(m.CustomerId));

            var machines = await unitOfWork.MachineRepository.GetListAsync(
                null,
                m => true,
                filters,
                q => q.OrderBy(m => m.Id)
            );

            return machines.Select(m => m.Id).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to fetch allowed machine IDs: {ex.Message}", ex);
        }
    }

    public async Task<ApiResponse<IEnumerable<string>>> GetAllStatusesAsync()
    {
        try
        {
            var statuses = await repository.GetAllStatusesAsync();

            return new ApiResponse<IEnumerable<string>>(
                StatusCodes.Status200OK,
                "All machine statuses fetched successfully.",
                statuses
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<string>>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }
}
