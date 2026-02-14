using Stripe;

namespace MMS.Application.Services;

public class SubscriptionService(
    ISubscriptionRepository subscriptionRepository, 
    IUnitOfWork unitOfWork,
    ICustomerSubscriptionRepository _customerSubscriptionRepository,
    IInvoiceRepository _invoiceRepository,
    StripeClient _stripeClient,
    AutoMapperResult autoMapper) : ISubscriptionService
{
    public async Task<ApiResponse<SubscriptionDto>> AddAsync(SubscriptionAddDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var subscription = autoMapper.Map<MMS.Application.Models.SQL.Subscription>(request);
            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ApiResponse<SubscriptionDto>(
                StatusCodes.Status201Created,
                nameof(MMS.Application.Models.SQL.Subscription) + ResponseMessages.Added,
                autoMapper.GetResult<MMS.Application.Models.SQL.Subscription, SubscriptionDto>(await subscriptionRepository.AddAsync(subscription))
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<SubscriptionDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<string>> DeleteAsync(Guid subscriptionId)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await subscriptionRepository.GetAsync(subscriptionId);

            return await result.MatchAsync(
                async subscription =>
                {
                    await subscriptionRepository.DeleteAsync(subscriptionId);
                    await unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ApiResponse<string>(
                        StatusCodes.Status200OK,
                        nameof(MMS.Application.Models.SQL.Subscription) + ResponseMessages.Deleted
                    );
                },
                async error =>
                {
                    return new ApiResponse<string>(
                        StatusCodes.Status404NotFound,
                        $"Subscription with ID {subscriptionId} not found."
                    );
                }
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

    public async Task<ApiResponse<SubscriptionDto>> GetByIdAsync(Guid subscriptionId)
    {
        try
        {
            var result = await subscriptionRepository.GetAsync(subscriptionId);

            return await result.MatchAsync(
                subscription =>
                {
                    var dto = autoMapper.Map<SubscriptionDto>(subscription);
                    return Task.FromResult(new ApiResponse<SubscriptionDto>(
                        StatusCodes.Status200OK,
                        nameof(MMS.Application.Models.SQL.Subscription) + ResponseMessages.Get,
                        dto
                    ));
                },
                error =>
                {
                    var statusCode = error switch
                    {
                        EntityNotFound => StatusCodes.Status404NotFound,
                        EntitySoftDeleted => StatusCodes.Status410Gone,
                        _ => StatusCodes.Status400BadRequest
                    };

                    return Task.FromResult(new ApiResponse<SubscriptionDto>(
                        statusCode,
                        $"Subscription with ID {subscriptionId} not accessible: {error.GetType().Name}"
                    ));
                }
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<SubscriptionDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while retrieving the subscription: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<SubscriptionDto>>> GetListAsync(
        PageParameters pageParameters, 
        SubscriptionStatus? status)
    {
        try
        {
            Expression<Func<MMS.Application.Models.SQL.Subscription, bool>> searchExpr = _ => true;

            var subscriptionFilters = new List<Expression<Func<MMS.Application.Models.SQL.Subscription, bool>>>
            {
                s => !status.HasValue || s.Status == status.Value
            };
            
            var subscriptions = await subscriptionRepository.GetListAsync(
                pageParameters,
                searchExpr,
                subscriptionFilters,
                q => q.OrderBy(s => s.Status)
            );

            return new ApiResponse<IEnumerable<SubscriptionDto>>(
                StatusCodes.Status200OK,
                nameof(MMS.Application.Models.SQL.Subscription) + ResponseMessages.GetAll,
                autoMapper.Map<IEnumerable<SubscriptionDto>>(subscriptions)
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<SubscriptionDto>>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<string>> ProcessSubscriptionPaymentAsync(SubscriptionPaymentDto dto)
    {
        try
        {
            if (dto.Amount <= 0)
            {
                return new ApiResponse<string>(
                    StatusCodes.Status400BadRequest,
                    "Amount must be greater than zero."
                );
            }
            var existingActiveSubResult = await _customerSubscriptionRepository.ExistingActiveSubAsync(dto.CustomerId);

            if (existingActiveSubResult.IsRight)
            {
                var existingActiveSub = existingActiveSubResult.Match(
                    right => right,
                    left => null!
                );

                existingActiveSub.IsActive = false;

                var updateOldSub = await _customerSubscriptionRepository.UpdateAsync(existingActiveSub);

                if (updateOldSub.IsLeft)
                {
                    var errorMessage = updateOldSub.Match(
                        right => string.Empty,
                        left => left.Message
                    );

                    return new ApiResponse<string>(
                        StatusCodes.Status500InternalServerError,
                        $"Failed to deactivate previous subscription: {errorMessage}"
                    );
                }
            }

            var paymentIntentService = new PaymentIntentService(_stripeClient);
            var paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
            {
                Amount = (long)((dto.Amount + dto.Tax) * 100),
                Currency = "usd",
                PaymentMethodTypes = new List<string> { "card" },
                Description = $"Subscription Payment for Customer {dto.CustomerId}"
            });

            var subscriptionResult = await _customerSubscriptionRepository.AddAsync(new CustomerSubscription
            {
                Id = Guid.NewGuid(),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = true,
                RenewalType = dto.RenewalType,
                Status = dto.SubscriptionStatus,
                CustomerId = dto.CustomerId,
                SubscriptionId = dto.SubscriptionId,
                InvoiceId = Guid.Empty
            });

            return await subscriptionResult.MatchAsync(
                async customerSubscription =>
                {
                    var invoiceResult = await _invoiceRepository.AddAsync(new MMS.Application.Models.SQL.Invoice
                    {
                        Invoicenumber = dto.InvoiceNumber,
                        Payment = dto.PaymentDate,
                        Amout = dto.Amount,
                        Tax = dto.Tax,
                        Status = dto.InvoiceStatus,
                        Paymentmethod = dto.PaymentMethod,
                        PaymentGatewayTrxId = paymentIntent.Id,
                        CustomerSubscriptionId = customerSubscription.Id,
                        CustomerId = dto.CustomerId,
                        BillingAdressId = dto.BillingAddressId
                    });

                    return await invoiceResult.MatchAsync(
                        async invoice =>
                        {
                            customerSubscription.InvoiceId = invoice.Id;
                            var updateResult = await _customerSubscriptionRepository.UpdateAsync(customerSubscription);

                            return updateResult.Match(
                                _ => new ApiResponse<string>(StatusCodes.Status200OK, "Successfully added subscription", paymentIntent.ClientSecret),
                                err => new ApiResponse<string>(500, $"Failed to update subscription: {err.Message}")
                            );
                        },
                        invoiceError => Task.FromResult(new ApiResponse<string>(500, $"Invoice creation failed: {invoiceError.Message}"))
                    );
                },
                subError => Task.FromResult(new ApiResponse<string>(500, $"Subscription creation failed: {subError.Message}"))
            );
        }
        catch (StripeException stripeEx)
        {
            return new ApiResponse<string>(
                StatusCodes.Status400BadRequest,
                $"Stripe error: {stripeEx.Message}"
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<SubscriptionDto>> UpdateAsync(Guid subscriptionId, SubscriptionUpdateDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await subscriptionRepository.GetAsync(subscriptionId);

            return await result.MatchAsync(
                async subscription =>
                {
                    autoMapper.Map(request, subscription);

                    await unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ApiResponse<SubscriptionDto>(
                        StatusCodes.Status200OK,
                        nameof(MMS.Application.Models.SQL.Subscription) + ResponseMessages.Updated,
                        autoMapper.GetResult<MMS.Application.Models.SQL.Subscription, SubscriptionDto>(subscription)
                    );
                },
                async error =>
                {
                    return new ApiResponse<SubscriptionDto>(
                        StatusCodes.Status404NotFound,
                        $"Subscription with ID {subscriptionId} not found."
                    );
                }
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<SubscriptionDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }
}