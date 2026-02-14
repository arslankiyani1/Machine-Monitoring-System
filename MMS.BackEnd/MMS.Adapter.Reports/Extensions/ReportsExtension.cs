using Microsoft.Extensions.DependencyInjection;
using MMS.Application.Ports.In.CustomerReportSetting;

namespace MMS.Adapter.Reports.Extensions;

public static class ReportsExtension
{
    public static IServiceCollection AddReportGenerator(this IServiceCollection services)
    {
        services.AddScoped<IReportGenerateService, ReportGenerateService>();
        return services;
    }
}


