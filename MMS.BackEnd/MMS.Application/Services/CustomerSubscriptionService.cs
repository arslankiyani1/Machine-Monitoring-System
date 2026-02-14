namespace MMS.Application.Services;

public class CustomerSubscriptionService(
    AutoMapperResult mapper,
    ICustomerSubscriptionRepository customerSubscriptionRepository,
    IUserContextService userContextService,
    IUnitOfWork unitOfWork) : ICustomerSubscriptionService
{
    public async Task<ApiResponse<CustomerSubscriptionDto>> AddAsync(AddCustomerSubscriptionDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, request.CustomerId);

            if (!await unitOfWork.CustomerRepository.ExistsAsync(request.CustomerId))
            {
                return new ApiResponse<CustomerSubscriptionDto>(
                    StatusCodes.Status200OK,
                    $"Customer with ID {request.CustomerId} does not exist."
                );
            }
            else if (await unitOfWork.CustomerSubscriptionRepository.ExistsByCustomerIdAsync(request.CustomerId))
            {
                return new ApiResponse<CustomerSubscriptionDto>(
                    StatusCodes.Status409Conflict,
                    $"A Customer subscription already exists for the customer with ID: {request.CustomerId}."
                );
            }

            var customerSubscription = mapper.Map<CustomerSubscription>(request);
            var addedSubscription = await customerSubscriptionRepository.AddAsync(customerSubscription);
            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ApiResponse<CustomerSubscriptionDto>(
                StatusCodes.Status201Created,
                nameof(CustomerSubscription) + ResponseMessages.Added,
                mapper.GetResult<CustomerSubscription, CustomerSubscriptionDto>(addedSubscription)
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<CustomerSubscriptionDto>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<CustomerSubscriptionDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while adding the subscription: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<string>> DeleteAsync(Guid subscriptionId)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var result = await customerSubscriptionRepository.GetAsync(subscriptionId);

            return await result.MatchAsync(
                async subscription =>
                {
                    await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, subscription.CustomerId);

                    await customerSubscriptionRepository.DeleteAsync(subscriptionId);
                    await unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ApiResponse<string>(
                        StatusCodes.Status200OK,
                        nameof(CustomerSubscription) + ResponseMessages.Deleted
                    );
                },
                async error =>
                {
                    await transaction.RollbackAsync();
                    return new ApiResponse<string>(
                        StatusCodes.Status404NotFound,
                        $"CustomerSubscription with ID {subscriptionId} not found."
                    );
                }
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<string>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
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

    public async Task<ApiResponse<IEnumerable<CustomerSubscriptionDto>>> GetByCustomerIdAsync(Guid customerId)
    {
        try
        {
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, customerId);
            var subscriptions = await customerSubscriptionRepository.GetByCustomerIdAsync(customerId);

            if (subscriptions == null || !subscriptions.Any())
            {
                return new ApiResponse<IEnumerable<CustomerSubscriptionDto>>(
                    StatusCodes.Status404NotFound,
                    $"No subscriptions found for customer with ID {customerId}."
                );
            }
            var subscriptionDtos = mapper.Map<IEnumerable<CustomerSubscriptionDto>>(subscriptions);

            return new ApiResponse<IEnumerable<CustomerSubscriptionDto>>(
                StatusCodes.Status200OK,
                $"Subscriptions for customer ID {customerId} retrieved successfully.",
                subscriptionDtos
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<IEnumerable<CustomerSubscriptionDto>>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<CustomerSubscriptionDto>>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while retrieving subscriptions: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<CustomerSubscriptionDto>> GetByIdAsync(Guid subscriptionId)
    {
        try
        {
            var result = await customerSubscriptionRepository.GetAsync(subscriptionId);

            return await result.MatchAsync(
                async right =>
                {
                    await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, right.CustomerId);

                    return new ApiResponse<CustomerSubscriptionDto>(
                   StatusCodes.Status200OK,
                   $"{nameof(CustomerSubscription)} {ResponseMessages.Get}",
                   mapper.Map<CustomerSubscriptionDto>(right)
               );
                },
               
                async left =>
                    new ApiResponse<CustomerSubscriptionDto>(
                        StatusCodes.Status404NotFound,
                        left.Message
                    )
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<CustomerSubscriptionDto>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<CustomerSubscriptionDto>(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<CustomerSubscriptionDto>>> GetListAsync(
        PageParameters pageParameters, 
        Guid CustomerId, 
        CustomerSubscriptionStatus? status,
        DateTime? start, DateTime? end)
    {
        try
        {
            Expression<Func<CustomerSubscription, bool>> searchExpr = _ => true;

            var filters = new List<Expression<Func<CustomerSubscription, bool>>>();


            if (CustomerId == Guid.Empty || CustomerId == default)
            {
                var customerIds = CustomerAccessHelper.GetAccessibleCustomerIds(userContextService);
                if (customerIds != null && customerIds.Any())
                {
                    filters.Add(s => customerIds.Contains(s.CustomerId));
                }
            }
            else
            {
                await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, CustomerId);
                filters.Add(s => s.CustomerId == CustomerId);
            }

            if (start != default)
                filters.Add(s => s.StartDate >= start);

            if (end != default)
                filters.Add(s => s.EndDate <= end);

            var customerSubscriptionsList = await customerSubscriptionRepository.GetListAsync(
                pageParameters,
                searchExpr,
                filters,
                q => q.OrderBy(s => s.StartDate)
            );

            return new ApiResponse<IEnumerable<CustomerSubscriptionDto>>(
                StatusCodes.Status200OK,
                nameof(CustomerSubscription) + ResponseMessages.GetAll,
                mapper.Map<IEnumerable<CustomerSubscriptionDto>>(customerSubscriptionsList)
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<IEnumerable<CustomerSubscriptionDto>> (
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<CustomerSubscriptionDto>>(
                StatusCodes.Status500InternalServerError,
                "An error occurred while retrieving the customer subscriptions list." + ex.Message
            );
        }
    }

    public async Task<ApiResponse<CustomerSubscriptionDto>> UpdateAsync(UpdateCustomerSubscriptionDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await customerSubscriptionRepository.GetAsync(request.SubscriptionId);

            return await result.MatchAsync(
                async existing =>
                {
                    if (request.CustomerId != existing.CustomerId)
                    {
                        if (!await unitOfWork.CustomerRepository.ExistsAsync(request.CustomerId))
                        {
                            return new ApiResponse<CustomerSubscriptionDto>(
                                StatusCodes.Status200OK,
                                $"Customer with ID {request.CustomerId} not found."
                            );
                        }

                        var isCustomerAssigned = await customerSubscriptionRepository
                            .AnyAsync(x => x.CustomerId == request.CustomerId && x.Id != request.Id);

                        if (isCustomerAssigned)
                        {
                            return new ApiResponse<CustomerSubscriptionDto>(
                                StatusCodes.Status409Conflict,
                                $"Customer with ID {request.CustomerId} is already assigned to another subscription."
                            );
                        }
                    }

                    mapper.Map(request, existing);
                    await unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ApiResponse<CustomerSubscriptionDto>(
                        StatusCodes.Status200OK,
                        nameof(CustomerSubscription) + ResponseMessages.Updated,
                        mapper.GetResult<CustomerSubscription, CustomerSubscriptionDto>(existing)
                    );
                },
                async error =>
                {
                    return new ApiResponse<CustomerSubscriptionDto>(
                        StatusCodes.Status404NotFound,
                        $"CustomerSubscription with ID {request.SubscriptionId} not found."
                    );
                }
            );
        }

        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<CustomerSubscriptionDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }
}