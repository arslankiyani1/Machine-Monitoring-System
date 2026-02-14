namespace MMS.Application.Ports.In.Widget.Dto;

public record AddWidgetDto(
    WidgetType WidgetType,
    WidgetSourceType SourceType,
    Dictionary<string, object> Config,
    Guid DashboardId
);
