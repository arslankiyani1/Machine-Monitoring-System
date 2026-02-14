namespace MMS.Application.Ports.In.Invoice;

public interface IInvoiceService
{
    Task<ApiResponse<IEnumerable<InvoiceDto>>> GetListAsync(PageParameters pageParameters,
        Guid? customerId = null,
        Guid? subscriptionId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? status = null);

    Task<ApiResponse<InvoiceDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<InvoiceDto>> AddAsync(AddInvoiceDto request);
    Task<ApiResponse<InvoiceDto>> UpdateAsync(Guid id, UpdateInvoiceDto request);
    Task<ApiResponse<string>> DeleteAsync(Guid id);
}
