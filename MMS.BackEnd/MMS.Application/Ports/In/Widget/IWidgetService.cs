namespace MMS.Application.Ports.In.Widget;

public interface IWidgetService
{
    Task<ApiResponse<IEnumerable<WidgetDto>>> GetListAsync(PageParameters pageParameters,
        WidgetType? widgetType,WidgetSourceType? SourceType);
    Task<ApiResponse<WidgetDto>> GetByIdAsync(Guid widgetId);
    Task<ApiResponse<WidgetDto>> AddAsync(AddWidgetDto request);
    Task<ApiResponse<WidgetDto>> UpdateAsync(Guid widgetId, UpdateWidgetDto request);
    Task<ApiResponse<string>> DeleteAsync(Guid widgetId);
}