using MMS.Application.Ports.In.NoSql.MachineSensorLog.Dto;

namespace MMS.Application.Services.NoSql;

public class MachineSensorLogService(
    IMachineSensorLogRepository repository,
    IUserContextService userContextService,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    AutoMapperResult mapper ) : IMachineSensorLogService
{
    private const string AllCacheKey = "MachineSensorData:All";
    private static string GetByIdCacheKey(Guid id) => $"MachineSensorData:{id}";

    public async Task<ApiResponse<MachineSensorLogDto>> CreateAsync(AddMachineSensorLogDto dto)
    {
        try
        {
            var machineResult = await unitOfWork.MachineRepository.GetAsync(dto.MachineId);
            if (machineResult.IsLeft)
            {
                return new ApiResponse<MachineSensorLogDto>(
                    StatusCodes.Status404NotFound,
                    $"Machine with ID {dto.MachineId} does not exist."
                );
            }
            
            var entity = mapper.Map<MachineSensorLog>(dto);
            entity.Id = Guid.NewGuid();

            await repository.AddAsync(entity);
            await cacheService.RemoveAsync(AllCacheKey);

            var resultDto = mapper.Map<MachineSensorLogDto>(entity);

            return new ApiResponse<MachineSensorLogDto>(
                StatusCodes.Status201Created,
                "Machine sensor data created successfully.",
                resultDto
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<MachineSensorLogDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<Guid>> DeleteAsync(Guid id)
    {
        try
        {
            var existing = await repository.GetByIdAsync(id);
            if (existing == null)
                return new ApiResponse<Guid>(StatusCodes.Status404NotFound, $"Entity with id {id} not found.");

            await repository.DeleteAsync(id);
            await cacheService.RemoveAsync(GetByIdCacheKey(id));
            await cacheService.RemoveAsync(AllCacheKey);

            return new ApiResponse<Guid>(
                StatusCodes.Status200OK,
                $"Entity with id {id} deleted successfully.",
                Guid.Empty
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<MachineSensorLogDto>>> GetAllAsync(PageParameters pageParameters)
    {
        try
        {
            var allowedMachineIds = await GetAllowedMachineIdsAsync();
            var sensorData = await repository.GetAllAsync(allowedMachineIds, pageParameters);

            var dtoList = mapper.Map<IEnumerable<MachineSensorLogDto>>(sensorData);

            return new ApiResponse<IEnumerable<MachineSensorLogDto>>(
                StatusCodes.Status200OK,
                "Fetched machine sensor data successfully.",
                dtoList
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<MachineSensorLogDto>>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while fetching data: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<MachineSensorLogDto>> GetByIdAsync(Guid id)
    {
        var cacheKey = GetByIdCacheKey(id);
        try
        {
            var cached = await cacheService.GetAsync<MachineSensorLogDto>(cacheKey);
            if (cached != null)
            {
                return new ApiResponse<MachineSensorLogDto>(
                    StatusCodes.Status200OK,
                    $"Fetched machine sensor data with id {id} from cache.",
                    cached
                );
            }

            var entity = await repository.GetByIdAsync(id);
            if (entity == null)
                return new ApiResponse<MachineSensorLogDto>(StatusCodes.Status404NotFound, $"Entity with id {id} not found.");

            var machineResult = await unitOfWork.MachineRepository.GetAsync(entity.MachineId);
            if (machineResult.IsLeft)
            {
                return new ApiResponse<MachineSensorLogDto>(
                    StatusCodes.Status404NotFound,
                    $"Machine with ID {entity.MachineId} does not exist."
                );
            }

            var machine = machineResult.IfRight();
            await CustomerAccessHelper.ValidateCustomerAccessAllowMMSBridgeAsync(userContextService, machine!.CustomerId);

            var dto = mapper.Map<MachineSensorLogDto>(entity);
            await cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));

            return new ApiResponse<MachineSensorLogDto>(
                StatusCodes.Status200OK,
                $"Fetched machine sensor log with id {id}.",
                dto
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<MachineSensorLogDto>(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            return new ApiResponse<MachineSensorLogDto>(
                StatusCodes.Status500InternalServerError,
                $"Error: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<MachineSensorLogsDto>>> GetLatestSensorDataAsync(
    Guid sensorId,
    DateTime? from,
    DateTime? to)
    {
        DateTime start, end;

        if (!from.HasValue && !to.HasValue)
        {
            return new ApiResponse<IEnumerable<MachineSensorLogsDto>>(
                StatusCodes.Status400BadRequest,
                "At least one of 'from' or 'to' parameters must be provided."
            );
        }

        end = to ?? DateTime.UtcNow;
        start = from ?? end.AddDays(-7);

        var rangeDescription = $"{start:yyyy-MM-dd HH:mm} to {end:yyyy-MM-dd HH:mm}";

        var sensorExists = await unitOfWork.MachineSensorRepository.ExistsAsync(sensorId);
        if (!sensorExists)
        {
            return new ApiResponse<IEnumerable<MachineSensorLogsDto>>(
                StatusCodes.Status404NotFound,
                $"Sensor not found with ID: {sensorId}"
            );
        }

        var logs = await repository.GetBySensorIdAndTimeRangeAsync(sensorId, start, end);

        if (logs == null || !logs.Any())
        {
            return new ApiResponse<IEnumerable<MachineSensorLogsDto>>(
                StatusCodes.Status200OK,
                $"No sensor data found for Sensor ID: {sensorId} in {rangeDescription} range."
            );
        }

        var allReadings = logs
            .SelectMany(log => log.SensorReading)
            .GroupBy(r => new { r.Key, r.Unit })
            .Select(g => new
            {
                Key = g.Key.Key,
                Unit = g.Key.Unit,
                AverageValue = g.Average(x => x.Value)
            })
            .ToList();

        var dto = new MachineSensorLogsDto
        {
            SensorId = sensorId,
            Status = logs.Last().SensorStatus,
            DateTime = DateTime.UtcNow,
            Parameters = allReadings.ToDictionary(
                r => r.Key.ToString().Replace("_", " "),
                r => $"{Math.Round(r.AverageValue, 2)} {r.Unit}"
            )
        };

        return new ApiResponse<IEnumerable<MachineSensorLogsDto>>(
            StatusCodes.Status200OK,
            $"Sensor data fetched successfully for {rangeDescription} range.",
            new List<MachineSensorLogsDto> { dto }
        );
    }

    public async Task<ApiResponse<IEnumerable<SensorTrendDataDto>>>     GetSensorTrendAsync(
    Guid sensorId,
    DateTime? from,
    DateTime? to)
    {
        if (!from.HasValue && !to.HasValue)
        {
            return new ApiResponse<IEnumerable<SensorTrendDataDto>>(
                StatusCodes.Status400BadRequest,
                "At least one of 'from' or 'to' parameters must be provided."
            );
        }

        var end = to ?? DateTime.UtcNow;
        var start = from ?? end.AddDays(-7);
        var rangeDescription = $"{start:yyyy-MM-dd HH:mm} to {end:yyyy-MM-dd HH:mm}";

        var sensorExists = await unitOfWork.MachineSensorRepository.ExistsAsync(sensorId);
        if (!sensorExists)
        {
            return new ApiResponse<IEnumerable<SensorTrendDataDto>>(
                StatusCodes.Status404NotFound,
                $"Sensor not found with ID: {sensorId}"
            );
        }

        var logs = await repository.GetBySensorIdAndTimeRangeAsync(sensorId, start, end);

        if (!logs.Any())
        {
            return new ApiResponse<IEnumerable<SensorTrendDataDto>>(
                StatusCodes.Status204NoContent,
                $"No sensor logs found for Sensor {sensorId} in {rangeDescription} range."
            );
        }

        var groupedData = logs
            .SelectMany(l => l.SensorReading.Select(r => new
            {
                Parameter = r.Key,
                r.Value,
                r.Unit,
                l.DateTime
            }))
            .GroupBy(x => new { x.Parameter, x.Unit })
            .Select(g => new SensorTrendDataDto
            {
                Parameter = g.Key.Parameter,
                Unit = g.Key.Unit ?? "",
                DataPoints = g
                    .OrderBy(x => x.DateTime)
                    .Select(x => new SensorTrendPoint
                    {
                        Date = x.DateTime,
                        Value = x.Value
                    })
                    .ToList()
            })
            .ToList();

        return new ApiResponse<IEnumerable<SensorTrendDataDto>>(
            StatusCodes.Status200OK,
            $"Trend data fetched successfully for {rangeDescription}.",
            groupedData
        );
    }

    public async Task<ApiResponse<MachineSensorLogDto>> UpdateAsync(UpdateMachineSensorLogDto dto)
    {
        try
        {
            var existing = await repository.GetByIdAsync(dto.Id);
            if (existing == null)
                return new ApiResponse<MachineSensorLogDto>(StatusCodes.Status404NotFound, $"Entity with id {dto.Id} not found.");

            var machineResult = await unitOfWork.MachineRepository.GetAsync(dto.MachineId);
            if (machineResult.IsLeft)
            {
                return new ApiResponse<MachineSensorLogDto>(
                    StatusCodes.Status404NotFound,
                    $"Machine with ID {dto.MachineId} does not exist."
                );
            }

            mapper.Map(dto, existing); // update entity
            await repository.UpdateAsync(existing);

            await cacheService.RemoveAsync(GetByIdCacheKey(dto.Id));
            await cacheService.RemoveAsync(AllCacheKey);

            var resultDto = mapper.Map<MachineSensorLogDto>(existing);

            return new ApiResponse<MachineSensorLogDto>(
                StatusCodes.Status200OK,
                "Machine sensor data updated successfully.",
                resultDto
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<MachineSensorLogDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    private async Task<List<Guid>> GetAllowedMachineIdsAsync()
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
}
