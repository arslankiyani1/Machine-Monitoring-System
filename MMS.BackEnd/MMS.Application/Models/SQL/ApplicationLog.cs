namespace MMS.Application.Models.SQL;

public class ApplicationLog : Trackable
{
    public string? ApiRequest { get; set; } = default!;
    public string? ApiResponse { get; set; } = default!;
    public string? Exception { get; set; } = default!;
    public string? Level { get; set; }
    public string? Message { get; set; }
    public string? Url { get; set; }
}