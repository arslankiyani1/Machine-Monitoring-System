using MMS.Application.Ports.In.Support.Dto;

namespace MMS.Application.Ports.In.Support;

public interface ISupportService
{
    Task<ApiResponse<IEnumerable<SupportTicketDto>>> GetListAsync(Guid customerId, PageParameters pageParameters);
    Task<ApiResponse<SupportTicketDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<SupportTicketDto>> AddAsync(AddSupportTicketDto request);
    Task<ApiResponse<SupportTicketDto>> MarkAsResolvedAsync(Guid supportId);
    Task<ApiResponse<SupportTicketDto>> UpdateAsync(Guid id, UpdateSupportTicketDto request);
    Task<ApiResponse<string>> DeleteAsync(Guid id);
}
