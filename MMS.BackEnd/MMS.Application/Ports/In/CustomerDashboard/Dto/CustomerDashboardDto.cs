namespace MMS.Application.Ports.In.CustomerDashboard.Dto;

public record CustomerDashboardDto(
    Guid Id,
    string Name,
    int? RefreshInterval,
    bool IsDefault,
    DashboardTheme Theme,
    DashboardStatus Status,
    Dictionary<string, object> Layout,
    Guid CustomerId
);
