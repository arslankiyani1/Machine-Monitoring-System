using MMS.Application.Utils;

namespace MMS.Application.Ports.In.CustomerReportSetting.Dto;

public class GenerateReportResponseDto
{
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public List<ReportData>? ReportData { get; set; }
}

