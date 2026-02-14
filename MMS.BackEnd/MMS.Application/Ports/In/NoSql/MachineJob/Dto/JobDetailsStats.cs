namespace MMS.Application.Ports.In.NoSql.MachineJob.Dto;

public record JobDetailsStats(
    float Oee,
    float Performance,
    float Availability,
    float Quality,
    DowntimeApiResponseDto Downtime,
    float Utilization,
    Dictionary<string, double> StatusPercent
);