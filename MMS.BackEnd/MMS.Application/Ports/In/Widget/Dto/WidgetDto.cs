
namespace MMS.Application.Ports.In.Widget.Dto;

public record WidgetDto(
    Guid Id,
    WidgetType WidgetType,
    WidgetSourceType SourceType,
    Dictionary<string, object> Config,
    Guid? DashboardId
);
