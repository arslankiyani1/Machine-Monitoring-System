namespace MMS.Application.Ports.In.NoSql.MachineSensorLog.Dto;

public class MachineSensorLogsDto
{
    public Guid SensorId { get; set; }
    public SensorStatus Status { get; set; }
    public DateTime DateTime { get; set; }

    public Dictionary<string, string> Parameters { get; set; } = new();
}

public class SensorTrendDataDto
{
    public ParameterType Parameter { get; set; } = default!;
    public string Unit { get; set; } = default!;
    public List<SensorTrendPoint> DataPoints { get; set; } = new();
}

public class SensorTrendPoint
{
    public DateTime Date { get; set; }
    public float Value { get; set; }
}