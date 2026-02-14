namespace MMS.Application.Services;

public class MachineSensorService(
    AutoMapperResult _mapper,
    IUnitOfWork unitOfWork,
    IServiceProvider serviceProvider,
    IMachineSensorRepository _machineSensorRepository) : IMachineSensorService
{
    public async Task<ApiResponse<MachineSensorDto>> AddAsync(AddMachineSensorDto request)
    {
        try
        {
            // Validate machine exists if provided
            if (request.MachineId.HasValue)
            {
                var machineExists = await unitOfWork.MachineRepository.ExistsAsyncs(request.MachineId.Value);
                if (!machineExists)
                {
                    return new ApiResponse<MachineSensorDto>(
                        StatusCodes.Status404NotFound,
                        $"Machine with Id {request.MachineId} does not exist."
                    );
                }
            }

            var entity = _mapper.Map<MachineSensor>(request);
            var result = await _machineSensorRepository.AddAsync(entity);

            return result.Match(
                sensor => new ApiResponse<MachineSensorDto>(
                    StatusCodes.Status201Created,
                    $"{nameof(MachineSensor)} {ResponseMessages.Added}",
                    _mapper.Map<MachineSensorDto>(sensor)
                ),
                error => new ApiResponse<MachineSensorDto>(
                    StatusCodes.Status400BadRequest,
                    error.Message
                )
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<MachineSensorDto>(
                StatusCodes.Status500InternalServerError,
                $"Error while adding MachineSensor: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<string>> DeleteAsync(Guid sensorId)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var result = await _machineSensorRepository.GetAsync(sensorId);

            if (result.IsLeft)
            {
                await transaction.RollbackAsync();
                return new ApiResponse<string>(
                    StatusCodes.Status404NotFound,
                    $"MachineSensor with ID '{sensorId}' does not exist or has already been deleted."
                );
            }

            await _machineSensorRepository.DeleteAsync(sensorId);
            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ApiResponse<string>(
                StatusCodes.Status200OK,
                $"MachineSensor with ID '{sensorId}' has been successfully deleted."
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<string>(
                StatusCodes.Status500InternalServerError,
                $"Error occurred while deleting MachineSensor: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<MachineSensorDto>> GetByIdAsync(Guid sensorId)
    {
        try
        {
            var result = await _machineSensorRepository.GetAsync(sensorId);

            return result.Match(
                sensor => new ApiResponse<MachineSensorDto>(
                    StatusCodes.Status200OK,
                    $"{nameof(MachineSensor)} {ResponseMessages.Get}",
                    _mapper.Map<MachineSensorDto>(sensor!)
                ),
                _ => new ApiResponse<MachineSensorDto>(
                    StatusCodes.Status404NotFound,
                    $"MachineSensor with ID {sensorId} not found."
                )
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<MachineSensorDto>(
                StatusCodes.Status500InternalServerError,
                $"Unexpected error: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<MachineSensorDto>>> GetListAsync(PageParameters pageParameters)
    {
        try
        {
            var term = pageParameters.Term?.Trim().ToLower() ?? string.Empty;

            Expression<Func<MachineSensor, bool>> searchExpr = s =>
                string.IsNullOrEmpty(term) || s.Name.ToLower().Contains(term);

            var sensors = await _machineSensorRepository.GetListAsync(
                pageParameters,
                searchExpr,
                null,
                q => q.OrderBy(s => s.Name)
            );

            return new ApiResponse<IEnumerable<MachineSensorDto>>(
                StatusCodes.Status200OK,
                $"{nameof(MachineSensor)}{ResponseMessages.GetAll}",
                _mapper.Map<IEnumerable<MachineSensorDto>>(sensors)
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<MachineSensorDto>>(
                StatusCodes.Status500InternalServerError,
                $"Error while retrieving MachineSensors: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<MachineSensorMachinesensorlogDto>>> GetSensorsByCustomerIdAsync(
        Guid customerId,
        PageParameters pageParameters)
    {
        try
        {
            // 1. Get sensors for customer
            List<MachineSensor> sensors;
            using (var scope = serviceProvider.CreateScope())
            {
                var sensorRepo = scope.ServiceProvider.GetRequiredService<IMachineSensorRepository>();
                sensors = await sensorRepo.GetByCustomerIdAsync(customerId);
            }

            // 2. Apply search filter in memory
            if (!string.IsNullOrWhiteSpace(pageParameters.Term))
            {
                var searchTerm = pageParameters.Term.Trim().ToLower();
                sensors = sensors
                    .Where(s => !string.IsNullOrEmpty(s.Name) && s.Name.ToLower().Contains(searchTerm))
                    .ToList();
            }

            var totalCount = sensors.Count;

            // 3. Apply paging
            var pagedSensors = sensors
                .Skip(pageParameters.Skip ?? 0)
                .Take(pageParameters.Top ?? 10)
                .ToList();

            if (!pagedSensors.Any())
            {
                return new ApiResponse<IEnumerable<MachineSensorMachinesensorlogDto>>(
                    StatusCodes.Status200OK,
                    $"Sensors retrieved successfully. Showing 0 of {totalCount} records.",
                    Enumerable.Empty<MachineSensorMachinesensorlogDto>()
                );
            }

            // 4. ✅ BATCH: Get all sensor IDs for batch fetch
            var sensorIds = pagedSensors.Select(s => s.Id).ToList();

            // 5. ✅ BATCH: Single query to get latest logs for ALL sensors
            Dictionary<Guid, MachineSensorLog> latestLogsBySensorId;
            using (var scope = serviceProvider.CreateScope())
            {
                var logRepo = scope.ServiceProvider.GetRequiredService<IMachineSensorLogRepository>();
                var logs = await logRepo.GetLatestBySensorIdsAsync(sensorIds);

                latestLogsBySensorId = logs
                    .GroupBy(l => l.SensorId)
                    .ToDictionary(g => g.Key, g => g.First());
            }

            // 6. ✅ Map in memory - NO more DB calls
            var resultList = pagedSensors.Select(sensor =>
            {
                latestLogsBySensorId.TryGetValue(sensor.Id, out var latestLog);

                return new MachineSensorMachinesensorlogDto
                {
                    Id = sensor.Id,
                    Name = sensor.Name ?? "N/A",
                    ImageUrl = sensor.ImageUrl,
                    MachineId = sensor.MachineId,
                    Interface = sensor.Interface,
                    ModbusIp = sensor.ModbusIp,
                    SensorType = sensor.SensorType,
                    SerialNumber = sensor.SerialNumber,
                    Status = latestLog?.SensorStatus.ToString() ?? "N/A",
                    LastUpdated = latestLog?.DateTime
                };
            }).ToList();

            return new ApiResponse<IEnumerable<MachineSensorMachinesensorlogDto>>(
                StatusCodes.Status200OK,
                $"Sensors retrieved successfully. Showing {resultList.Count} of {totalCount} records.",
                resultList
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<MachineSensorMachinesensorlogDto>>(
                StatusCodes.Status500InternalServerError,
                $"Error retrieving sensors: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<MachineSensorDto>> UpdateAsync(UpdateMachineSensorDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var result = await _machineSensorRepository.GetAsync(request.Id);

            if (result.IsLeft)
            {
                await transaction.RollbackAsync();
                return new ApiResponse<MachineSensorDto>(
                    StatusCodes.Status404NotFound,
                    $"MachineSensor with ID {request.Id} not found."
                );
            }

            var existing = result.IfRight();

            if (existing == null)
            {
                await transaction.RollbackAsync();
                return new ApiResponse<MachineSensorDto>(
                    StatusCodes.Status404NotFound,
                    $"MachineSensor with ID {request.Id} not found."
                );
            }

            // Validate machine exists if provided
            if (request.MachineId.HasValue)
            {
                var machineExists = await unitOfWork.MachineRepository.ExistsAsyncs(request.MachineId.Value);
                if (!machineExists)
                {
                    await transaction.RollbackAsync();
                    return new ApiResponse<MachineSensorDto>(
                        StatusCodes.Status400BadRequest,
                        $"Invalid MachineId: {request.MachineId.Value}. Machine does not exist."
                    );
                }
            }

            // Map updates
            try
            {
                _mapper.Map(request, existing);
            }
            catch (Exception mapEx)
            {
                await transaction.RollbackAsync();
                return new ApiResponse<MachineSensorDto>(
                    StatusCodes.Status400BadRequest,
                    $"Mapping error: {mapEx.Message}"
                );
            }

            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ApiResponse<MachineSensorDto>(
                StatusCodes.Status200OK,
                $"{nameof(MachineSensor)} {ResponseMessages.Updated}",
                _mapper.Map<MachineSensorDto>(existing)
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<MachineSensorDto>(
                StatusCodes.Status500InternalServerError,
                $"Error while updating MachineSensor: {ex.Message}"
            );
        }
    }
}