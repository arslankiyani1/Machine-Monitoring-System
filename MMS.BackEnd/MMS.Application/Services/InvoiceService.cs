namespace MMS.Application.Services;

public class InvoiceService(AutoMapperResult _mapper,
    IUnitOfWork unitOfWork, IInvoiceRepository _invoiceRepository,
    ICustomerRepository _customerRepository) : IInvoiceService
{
    public async Task<ApiResponse<InvoiceDto>> AddAsync(AddInvoiceDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var customerExists = await _customerRepository.ExistsAsync(request.CustomerId);
            if (!customerExists)
            {
                return new ApiResponse<InvoiceDto>(
                    StatusCodes.Status200OK,
                    $"Customer with ID '{request.CustomerId}' not found.");
            }
            var invoice = _mapper.Map<Invoice>(request);
            invoice.Id = Guid.NewGuid();

            var addedInvoice = await _invoiceRepository.AddAsync(invoice);
            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ApiResponse<InvoiceDto>(
                StatusCodes.Status201Created,
                nameof(Invoice) + ResponseMessages.Added,
                _mapper.GetResult<Invoice, InvoiceDto>(addedInvoice)
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<InvoiceDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while adding the invoice: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<string>> DeleteAsync(Guid id)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var result = await _invoiceRepository.GetAsync(id);

            return await result.MatchAsync(
                async invoice =>
                {
                    await _invoiceRepository.DeleteAsync(id);
                    await unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ApiResponse<string>(
                        StatusCodes.Status200OK,
                        nameof(Invoice) + ResponseMessages.Deleted
                    );
                },
                async error =>
                {
                    await transaction.RollbackAsync();
                    return new ApiResponse<string>(
                        StatusCodes.Status404NotFound,
                        $"Invoice with ID {id} not found."
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

    public async Task<ApiResponse<InvoiceDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var result = await _invoiceRepository.GetAsync(id);

            return await result.MatchAsync(
                async right =>
                {
                    return new ApiResponse<InvoiceDto>(
                        StatusCodes.Status200OK,
                        $"{nameof(Invoice)} {ResponseMessages.Get}",
                        _mapper.Map<InvoiceDto>(right)
                    );
                },
                async left =>
                    new ApiResponse<InvoiceDto>(
                        StatusCodes.Status404NotFound,
                        left.Message
                    )
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<InvoiceDto>(
                StatusCodes.Status500InternalServerError,
                $"Unexpected error: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<InvoiceDto>>> GetListAsync(
        PageParameters pageParameters,
        Guid? customerId = null,
        Guid? subscriptionId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? status = null)
    {
        try
        {
            Expression<Func<Invoice, bool>> searchExpr = _ => true;

            var filters = new List<Expression<Func<Invoice, bool>>>();

            if (customerId.HasValue)
                filters.Add(i => i.CustomerId == customerId);

            if (subscriptionId.HasValue)
                filters.Add(i => i.CustomerSubscriptionId == subscriptionId);

            if (startDate.HasValue)
                filters.Add(i => i.Payment >= startDate);

            if (endDate.HasValue)
                filters.Add(i => i.Payment <= endDate);

            if (!string.IsNullOrWhiteSpace(status))
                filters.Add(i => i.Status == status);

            var invoices = await _invoiceRepository.GetListAsync(
                pageParameters,
                searchExpr,
                filters,
                q => q.OrderBy(i => i.Payment)
            );

            return new ApiResponse<IEnumerable<InvoiceDto>>(
                StatusCodes.Status200OK,
                nameof(Invoice) + ResponseMessages.GetAll,
                _mapper.Map<IEnumerable<InvoiceDto>>(invoices)
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<InvoiceDto>>(
                StatusCodes.Status500InternalServerError,
                $"Error while retrieving invoices: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<InvoiceDto>> UpdateAsync(Guid id, UpdateInvoiceDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var result = await _invoiceRepository.GetAsync(id);

            return await result.MatchAsync(
                async existing =>
                {
                    _mapper.Map(request, existing);

                    await unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ApiResponse<InvoiceDto>(
                        StatusCodes.Status200OK,
                        nameof(Invoice) + ResponseMessages.Updated,
                        _mapper.GetResult<Invoice, InvoiceDto>(existing)
                    );
                },
                async error =>
                    new ApiResponse<InvoiceDto>(
                        StatusCodes.Status404NotFound,
                        $"Invoice with ID {id} not found."
                    )
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<InvoiceDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }
}
