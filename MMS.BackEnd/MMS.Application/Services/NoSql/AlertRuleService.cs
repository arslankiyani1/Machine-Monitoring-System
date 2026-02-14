namespace MMS.Application.Services.NoSql;

public class AlertRuleService(
    IAlertRuleRepository _repository, 
    AutoMapperResult _mapper,
    IUnitOfWork _unitOfWork) : IAlertRuleService
{
    public async Task<ApiResponse<AlertRuleDto>> CreateAsync(AddAlertRuleDto dto)
    {
        try
        {
            var customerExists = await _unitOfWork.CustomerRepository.ExistsAsync(dto.CustomerId);
            if (!customerExists)
            {
                return new ApiResponse<AlertRuleDto>(
                    StatusCodes.Status400BadRequest,
                    $"Customer with ID '{dto.CustomerId}' does not exist."
                );
            }

            if (dto.AlertScope == AlertScope.Machine)
            {
                var machineExists = await _unitOfWork.MachineRepository.ExistsAsyncs(dto.MachineId);
                if (!machineExists)
                {
                    return new ApiResponse<AlertRuleDto>(
                        StatusCodes.Status400BadRequest,
                        $"Machine with ID '{dto.MachineId}' does not exist."
                    );
                }
            }
         
            if(dto.AlertScope == AlertScope.Sensor)
            {
                var sensorExists = await _unitOfWork.MachineSensorRepository.ExistsAsync(dto.SensorId);
                if (!sensorExists)
                {
                    return new ApiResponse<AlertRuleDto>(
                        StatusCodes.Status400BadRequest,
                        $"Sensor with ID '{dto.SensorId}' does not exist."
                    );
                }
            }
         

            var entity = _mapper.Map<AlertRule>(dto);
            entity.Id = Guid.NewGuid().ToString();
            entity.CreatedAt = DateTime.UtcNow;

            var result = await _repository.AddAsync(entity);

            return new ApiResponse<AlertRuleDto>(
                StatusCodes.Status201Created,
                $"{nameof(AlertRule)} {ResponseMessages.Added}",
                _mapper.Map<AlertRuleDto>(result)
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<AlertRuleDto>(
                StatusCodes.Status500InternalServerError,
                $"Error creating {nameof(AlertRule)}: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<AlertRuleDto>> DeleteAsync(string id)
    {
        try
        {
            var existingItem = await _repository.GetByIdAsync(id);
            if (existingItem == null)
            {
                return new ApiResponse<AlertRuleDto>(
                    StatusCodes.Status404NotFound,
                    $"The {nameof(AlertRule)} with ID {id} was not found or has already been deleted."
                );
            }

            var dto = _mapper.Map<AlertRuleDto>(existingItem);

            var deleted = await _repository.DeleteAsync(id);
            if (!deleted)
            {
                return new ApiResponse<AlertRuleDto>(
                    StatusCodes.Status204NoContent,
                    $"{nameof(AlertRule)} {ResponseMessages.NotFound}"
                );
            }

            return new ApiResponse<AlertRuleDto>(
                StatusCodes.Status200OK,
                $"{nameof(AlertRule)} {ResponseMessages.Deleted}",
                dto
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<AlertRuleDto>(
                StatusCodes.Status500InternalServerError,
                $"Error deleting {nameof(AlertRule)}: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<AlertRuleDto>>> GetAllAsync(PageParameters pageParameters, Guid? machineId)
    {
        try
        {
            var pagedResult = await _repository.GetPagedAsync(pageParameters,machineId);

            var mapped = _mapper.Map<IEnumerable<AlertRuleDto>>(pagedResult);

            return new ApiResponse<IEnumerable<AlertRuleDto>>(
                StatusCodes.Status200OK,
                $"{nameof(AlertRule)} {ResponseMessages.GetAll}",
                mapped
            ); 
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<AlertRuleDto>>(
                StatusCodes.Status500InternalServerError,
                $"Error retrieving {nameof(AlertRule)} list: {ex.InnerException?.Message ?? ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<AlertRuleDto>> GetByIdAsync(string id)
    {
        try
        {
            var result = await _repository.GetByIdAsync(id);

            if (result is null)
            {
                return new ApiResponse<AlertRuleDto>(
                    StatusCodes.Status404NotFound,
                    $"{nameof(AlertRule)} {ResponseMessages.NotFound}"
                );
            }

            return new ApiResponse<AlertRuleDto>(
                StatusCodes.Status200OK,
                $"{nameof(AlertRule)} {ResponseMessages.Get}",
                _mapper.Map<AlertRuleDto>(result)
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<AlertRuleDto>(
                StatusCodes.Status500InternalServerError,
                $"Error retrieving {nameof(AlertRule)}: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<AlertRuleDto>> UpdateAsync(UpdateAlertRuleDto dto)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(dto.Id.ToString());

            if (existing is null)
            {
                return new ApiResponse<AlertRuleDto>(
                    StatusCodes.Status404NotFound,
                    $"{nameof(AlertRule)} {ResponseMessages.NotFound}"
                );
            }
            if (dto.AlertScope == AlertScope.Machine)
            {
                var machineExists = await _unitOfWork.MachineRepository.ExistsAsyncs(dto.MachineId);
                if (!machineExists)
                {
                    return new ApiResponse<AlertRuleDto>(
                        StatusCodes.Status400BadRequest,
                        $"Machine with ID '{dto.MachineId}' does not exist."
                    );
                }
            }
            if (dto.AlertScope == AlertScope.Sensor)
            {
            var sensorExists = await _unitOfWork.MachineSensorRepository.ExistsAsync(dto.SensorId);
            if (!sensorExists)
            {
                return new ApiResponse<AlertRuleDto>(
                    StatusCodes.Status400BadRequest,
                    $"Sensor with ID '{dto.SensorId}' does not exist."
                );
            }
            }
            _mapper.Map(dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            var updated = await _repository.UpdateAsync(existing);

            return new ApiResponse<AlertRuleDto>(
                StatusCodes.Status200OK,
                $"{nameof(AlertRule)} {ResponseMessages.Updated}",
                _mapper.Map<AlertRuleDto>(updated!)
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<AlertRuleDto>(
                StatusCodes.Status500InternalServerError,
                $"Error updating {nameof(AlertRule)}: {ex.Message}"
            );
        }
    }
}
