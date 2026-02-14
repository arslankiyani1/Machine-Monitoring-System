namespace MMS.Application.Ports.In.CustomerReportSetting;

public interface IReportGenerateService
{
    Task<byte[]> GenerateAsync(string format, Dictionary<Guid, ReportData> reportDatas, IEnumerable<ReportType> reportTypes, string reportName);
}