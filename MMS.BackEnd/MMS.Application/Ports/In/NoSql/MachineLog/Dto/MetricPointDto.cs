namespace MMS.Application.Ports.In.NoSql.MachineLog.Dto;

public class MetricPointDto
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
}

public class MetricResponseDto
{
    public string Metric { get; set; } = default!;
    public string Unit { get; set; } = default!;
    public List<MetricPointDto> Points { get; set; } = new List<MetricPointDto>();
}

public class MetricRequestDto
{
    public MetricType Metric { get; set; }
    public DateTime From { get; set; } = default!;
    public DateTime To { get; set; } = default!;
    public TimeRange Range { get; set; }
    //public AggregationInterval Interval { get; set; }
}
