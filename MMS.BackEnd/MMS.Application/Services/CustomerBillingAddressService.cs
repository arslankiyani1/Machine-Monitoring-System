namespace MMS.Application.Services;

public class CustomerBillingAddressService(AutoMapperResult _mapper,
    ICustomerBillingAddressRepository _repository,
    IUnitOfWork unitOfWork) : ICustomerBillingAddressService
{
    public async Task<ApiResponse<CustomerBillingAddressDto>> AddAsync(AddCustomerBillingAddressDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var entity = _mapper.Map<CustomerBillingAddress>(request);
            entity.Id = Guid.NewGuid();

            var result = await _repository.AddAsync(entity);
            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ApiResponse<CustomerBillingAddressDto>(
                StatusCodes.Status201Created,
                nameof(CustomerBillingAddress) + ResponseMessages.Added,
                _mapper.GetResult<CustomerBillingAddress, CustomerBillingAddressDto>(result)
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<CustomerBillingAddressDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<string>> DeleteAsync(Guid id)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await _repository.GetAsync(id);

            return await result.MatchAsync(
                async existing =>
                {
                    await _repository.DeleteAsync(id);
                    await unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ApiResponse<string>(
                        StatusCodes.Status200OK,
                        nameof(CustomerBillingAddress) + ResponseMessages.Deleted
                    );
                },
                async error =>
                {
                    await transaction.RollbackAsync();
                    return new ApiResponse<string>(
                        StatusCodes.Status404NotFound,
                        $"Billing Address with ID {id} not found."
                    );
                });
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

    public async Task<ApiResponse<CustomerBillingAddressDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var result = await _repository.GetAsync(id);
            return await result.MatchAsync(
                async entity => new ApiResponse<CustomerBillingAddressDto>(
                    StatusCodes.Status200OK,
                    $"{nameof(CustomerBillingAddress)} {ResponseMessages.Get}",
                    _mapper.Map<CustomerBillingAddressDto>(entity)
                ),
                async error => new ApiResponse<CustomerBillingAddressDto>(
                    StatusCodes.Status404NotFound,
                    error.Message
                ));
        }
        catch (Exception ex)
        {
            return new ApiResponse<CustomerBillingAddressDto>(
                StatusCodes.Status500InternalServerError,
                $"Unexpected error: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<CustomerBillingAddressDto>>> GetListAsync(Guid? customerId = null)
    {
        try
        {
            Expression<Func<CustomerBillingAddress, bool>> filter = _ => true;

            var filters = new List<Expression<Func<CustomerBillingAddress, bool>>>();
            if (customerId != null)
                filters.Add(x => x.CustomerId == customerId.Value);

            var result = await _repository.GetListAsync(
                null!,
                filter,
                filters,
                q => q.OrderByDescending(x => x.CreatedAt)
            );

            return new ApiResponse<IEnumerable<CustomerBillingAddressDto>>(
                StatusCodes.Status200OK,
                nameof(CustomerBillingAddress) + ResponseMessages.GetAll,
                _mapper.Map<IEnumerable<CustomerBillingAddressDto>>(result)
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<CustomerBillingAddressDto>>(
                StatusCodes.Status500InternalServerError,
                $"Error while retrieving billing addresses: {ex.Message}"
            );
        }
    }

    public Task<ApiResponse<IEnumerable<CustomerBillingAddressDto>>> GetListAsync(Guid customerId)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiResponse<CustomerBillingAddressDto>> UpdateAsync(Guid billingAddressId, UpdateCustomerBillingAddressDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var result = await _repository.GetAsync(billingAddressId);
            return await result.MatchAsync(
                async existing =>
                {
                    _mapper.Map(request, existing);
                    await unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ApiResponse<CustomerBillingAddressDto>(
                        StatusCodes.Status200OK,
                        nameof(CustomerBillingAddress) + ResponseMessages.Updated,
                        _mapper.GetResult<CustomerBillingAddress, CustomerBillingAddressDto>(existing)
                    );
                },
                async error =>
                {
                    await transaction.RollbackAsync();
                    return new ApiResponse<CustomerBillingAddressDto>(
                        StatusCodes.Status404NotFound,
                        $"Billing Address with ID {billingAddressId} not found."
                    );
                });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<CustomerBillingAddressDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }
}