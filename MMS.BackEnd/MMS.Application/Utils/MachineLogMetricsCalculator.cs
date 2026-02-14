namespace MMS.Application.Utils;

public static class MachineLogMetricsCalculator
{
    public const string LOG_TYPE_MACHINE = "MachineLog";
    public const string LOG_TYPE_DOWNTIME = "DownTimelog";

    public static JobMetricsFromLogs CalculateJobMetricsFromLogs(
        IEnumerable<MachineLog>? machineLogs, MachineJob job)
    {
        if (job?.Schedule == null) return JobMetricsFromLogs.Empty();

        var plannedStart = job.Schedule.PlannedStart;
        var plannedEnd = job.Schedule.PlannedEnd;
        var plannedSeconds = (plannedEnd - plannedStart).TotalSeconds;

        if (plannedSeconds <= 0) return JobMetricsFromLogs.Empty();

        var logsList = machineLogs?.ToList() ?? new List<MachineLog>();
        if (!logsList.Any())
        {
            return new JobMetricsFromLogs
            {
                PlannedSeconds = plannedSeconds,
                Availability = 100f,
                Performance = CalculatePerformanceOnly(job, plannedSeconds),
                Quality = CalculateQualityOnly(job)
            };
        }

        var jobLogs = FilterLogsByTimeOverlap(logsList, plannedStart, plannedEnd);

        // Filter by Type
        var machineStatusLogs = machineLogs
            .Where(l => string.IsNullOrEmpty(l.Type) ||
                       l.Type.Equals(LOG_TYPE_MACHINE, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Downtime separately filtered:
        var downtimeLogs = jobLogs
            .Where(l => l.Type?.Equals(LOG_TYPE_DOWNTIME, StringComparison.OrdinalIgnoreCase) == true)
            .ToList();

        // Calculate running time with overlap handling
        var runningSeconds = CalculateRunningTimeWithoutOverlap(machineStatusLogs, plannedStart, plannedEnd);

        // Calculate downtime
        var (totalDowntimeSeconds, downtimeBreakdown) = CalculateDowntimeWithBreakdown(
            downtimeLogs, plannedStart, plannedEnd);

        // Cap utilization at 100%
        var utilization = plannedSeconds > 0
            ? Math.Min(Math.Round((runningSeconds / plannedSeconds) * 100, 2), 100)
            : 0;

        var operatingSeconds = Math.Max(plannedSeconds - totalDowntimeSeconds, 0);
        var availabilityRatio = plannedSeconds > 0 ? Math.Clamp(operatingSeconds / plannedSeconds, 0, 1) : 0;

        var performanceRatio = 0.0;
        if (operatingSeconds > 0 && job.Metrics?.TargetCycleTime > 0)
        {
            var goodCount = job.Quantities?.Good ?? 0;
            performanceRatio = Math.Clamp((job.Metrics.TargetCycleTime * goodCount) / operatingSeconds, 0, 1);
        }

        var good = job.Quantities?.Good ?? 0;
        var bad = job.Quantities?.Bad ?? 0;
        var total = good + bad;
        var qualityRatio = total > 0 ? Math.Clamp((double)good / total, 0, 1) : 0;

        var oee = availabilityRatio * performanceRatio * qualityRatio * 100;

        var totalDowntimePercent = plannedSeconds > 0
            ? (float)Math.Round((totalDowntimeSeconds / plannedSeconds) * 100, 2)
            : 0;

        return new JobMetricsFromLogs
        {
            TotalDowntimeSeconds = totalDowntimeSeconds,
            TotalDowntimePercent = totalDowntimePercent,
            TotalRunningSeconds = runningSeconds,
            PlannedSeconds = plannedSeconds,
            OperatingSeconds = operatingSeconds,
            Utilization = (float)utilization,
            Availability = (float)Math.Round(availabilityRatio * 100, 2),
            Performance = (float)Math.Round(performanceRatio * 100, 2),
            Quality = (float)Math.Round(qualityRatio * 100, 2),
            Oee = (float)Math.Round(oee, 2),
            DowntimeBreakdown = downtimeBreakdown
        };
    }

    public static double CalculateDowntimeSecondsFromLogs(
        IEnumerable<MachineLog>? machineLogs, DateTime plannedStart, DateTime plannedEnd)
    {
        if (machineLogs == null || !machineLogs.Any()) return 0;

        // ✅ SIMPLIFIED: Only filter by time, JobId already filtered
        return machineLogs
            .Where(l => l.Type?.Equals(LOG_TYPE_DOWNTIME, StringComparison.OrdinalIgnoreCase) == true)
            .Sum(log => CalculateLogDuration(log, plannedStart, plannedEnd));
    }

    public static double CalculateUtilizationFromLogs(
        IEnumerable<MachineLog>? machineLogs, DateTime periodStart, DateTime periodEnd)
    {
        if (machineLogs == null || !machineLogs.Any()) return 0;

        var totalPeriodSeconds = (periodEnd - periodStart).TotalSeconds;
        if (totalPeriodSeconds <= 0) return 0;

        // ✅ SIMPLIFIED: Only filter by type and time, JobId already filtered
        var machineStatusLogs = machineLogs
            .Where(l => string.IsNullOrEmpty(l.Type) ||
                       l.Type.Equals(LOG_TYPE_MACHINE, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var runningSeconds = CalculateRunningTimeWithoutOverlap(machineStatusLogs, periodStart, periodEnd);

        return Math.Min(Math.Round((runningSeconds / totalPeriodSeconds) * 100, 2), 100);
    }

    #region Private Helpers

    private static float CalculatePerformanceOnly(MachineJob job, double plannedSeconds)
    {
        if (plannedSeconds <= 0 || job.Metrics?.TargetCycleTime <= 0) return 0;
        var goodCount = job.Quantities?.Good ?? 0;
        return (float)Math.Round(Math.Clamp((job.Metrics!.TargetCycleTime * goodCount) / plannedSeconds, 0, 1) * 100, 2);
    }

    private static float CalculateQualityOnly(MachineJob job)
    {
        var good = job.Quantities?.Good ?? 0;
        var bad = job.Quantities?.Bad ?? 0;
        var total = good + bad;
        return total <= 0 ? 0 : (float)Math.Round(((double)good / total) * 100, 2);
    }

    // ✅ NEW: Simple time overlap filter only (no JobId check)
    private static List<MachineLog> FilterLogsByTimeOverlap(List<MachineLog> logs, DateTime plannedStart, DateTime plannedEnd)
    {
        return logs.Where(log =>
        {
            if (!log.Start.HasValue)
                return false;

            var logEnd = log.End ?? DateTime.UtcNow;
            return log.Start.Value < plannedEnd && logEnd > plannedStart;
        }).ToList();
    }

    private static double CalculateRunningTimeWithoutOverlap(List<MachineLog> machineStatusLogs, DateTime start, DateTime end)
    {
        if (!machineStatusLogs.Any()) return 0;

        var intervals = machineStatusLogs
            .Where(log => log.Start.HasValue)
            .Select(log =>
            {
                var logStart = log.Start!.Value < start ? start : log.Start.Value;
                var logEnd = (log.End ?? DateTime.UtcNow) > end ? end : (log.End ?? DateTime.UtcNow);
                return (Start: logStart, End: logEnd);
            })
            .Where(i => i.End > i.Start)
            .OrderBy(i => i.Start)
            .ToList();

        if (!intervals.Any()) return 0;

        var mergedIntervals = new List<(DateTime Start, DateTime End)>();
        var current = intervals[0];

        for (int i = 1; i < intervals.Count; i++)
        {
            var next = intervals[i];

            if (next.Start <= current.End)
            {
                current = (current.Start, next.End > current.End ? next.End : current.End);
            }
            else
            {
                mergedIntervals.Add(current);
                current = next;
            }
        }
        mergedIntervals.Add(current);

        return mergedIntervals.Sum(i => (i.End - i.Start).TotalSeconds);
    }

    private static (double, List<DowntimeResponseDto>) CalculateDowntimeWithBreakdown(
        List<MachineLog> downtimeLogs, DateTime start, DateTime end)
    {
        var breakdown = new Dictionary<string, (double Duration, string Color)>(StringComparer.OrdinalIgnoreCase);

        foreach (var log in downtimeLogs)
        {
            var duration = CalculateLogDuration(log, start, end);
            if (duration <= 0) continue;

            var reason = FormatReasonName(log.Status ?? "Unknown");
            var color = log.Color ?? "#808080";

            AddBreakdown(breakdown, reason, duration, color);
        }

        var total = breakdown.Values.Sum(v => v.Duration);

        var list = breakdown.Select(kvp => new DowntimeResponseDto
        {
            Reason = kvp.Key,
            Duration = (float)kvp.Value.Duration,
            Color = kvp.Value.Color,
            Percentage = total > 0 ? (float)Math.Round((kvp.Value.Duration / total) * 100, 2) : 0
        }).OrderByDescending(d => d.Duration).ToList();

        return (total, list);
    }

    private static void AddBreakdown(Dictionary<string, (double, string)> dict, string reason, double duration, string color)
    {
        if (dict.TryGetValue(reason, out var existing))
            dict[reason] = (existing.Item1 + duration, existing.Item2);
        else
            dict[reason] = (duration, color);
    }

    private static double CalculateLogDuration(MachineLog log, DateTime start, DateTime end)
    {
        if (!log.Start.HasValue) return 0;
        var logStart = log.Start.Value < start ? start : log.Start.Value;
        var logEnd = (log.End ?? DateTime.UtcNow) > end ? end : (log.End ?? DateTime.UtcNow);
        return logEnd <= logStart ? 0 : (logEnd - logStart).TotalSeconds;
    }

    private static string FormatReasonName(string reason)
    {
        if (string.IsNullOrEmpty(reason)) return "Unknown";
        return string.Join(" ", reason.Split('_').Select(w =>
            char.ToUpper(w[0]) + (w.Length > 1 ? w[1..].ToLower() : "")));
    }

    #endregion
}