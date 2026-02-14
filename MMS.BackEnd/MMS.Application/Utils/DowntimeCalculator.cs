namespace MMS.Application.Utils;

public static class DowntimeCalculator
{
    private static readonly Dictionary<DowntimeReason, string> DowntimeColors = new Dictionary<DowntimeReason, string>
    {
        { DowntimeReason.ToolChange, "#00AAFF" }, // Blue for ToolChange
        { DowntimeReason.Maintenance, "#FFAA00" }, // Orange for Maintenance
        //{ DowntimeReason.Error, "#FF0000" },      // Red for Error
        //{ DowntimeReason.Offline, "#808080" }     // Gray for Offline
    };

    public static List<HistoricalDowntimeEvent> CalculateHistoricalDowntimeEvents(
    List<MachineJob> jobs, DateTime startOfDay, DateTime endOfDay)
    {
        // ✅ OPTIMIZATION: Use LINQ SelectMany to flatten and filter in single pass
        if (jobs == null || jobs.Count == 0)
            return new List<HistoricalDowntimeEvent>();

        return jobs
            .Where(j => j.DowntimeEvents != null && j.DowntimeEvents.Count > 0)
            .SelectMany(j => j.DowntimeEvents!)
            .Select(eventItem =>
            {
                var start = eventItem.StartTime < startOfDay ? startOfDay : eventItem.StartTime;
                var end = eventItem.EndTime > endOfDay ? endOfDay : eventItem.EndTime;
                return (Start: start, End: end);
            })
            .Where(interval => interval.End > interval.Start)
            .Select(interval => new HistoricalDowntimeEvent
            {
                TotalDuration = (float)(interval.End - interval.Start).TotalSeconds
            })
            .OrderBy(e => e.Reason)
            .ToList();
    }

    public static double CalculateJobDowntimeSeconds(MachineJob job)
    {
        if (job?.DowntimeEvents == null || !job.DowntimeEvents.Any())
            return 0;

        return job.DowntimeEvents.Sum(d =>
        {
            var start = d.StartTime < job.Schedule.PlannedStart ? job.Schedule.PlannedStart : d.StartTime;
            var end = d.EndTime > job.Schedule.PlannedEnd ? job.Schedule.PlannedEnd : d.EndTime;
            return (end > start) ? (end - start).TotalSeconds : 0;
        });
    }


    public static DowntimeApiResponseDto CalculateDowntimeResponse(List<DowntimeResponseDto> downtimeMetrics)
    {
        // ✅ OPTIMIZATION: Calculate total once and update percentages in single pass
        double totalDowntime = downtimeMetrics.Sum(m => m.Duration);
        
        if (totalDowntime > 0)
        {
            var percentageMultiplier = 100.0 / totalDowntime;
            foreach (var item in downtimeMetrics)
            {
                item.Percentage = (float)Math.Round(item.Duration * percentageMultiplier, 2);
            }
        }
        else
        {
            // ✅ OPTIMIZATION: Avoid division by zero, set all to 0
            foreach (var item in downtimeMetrics)
            {
                item.Percentage = 0;
            }
        }

        return new DowntimeApiResponseDto
        {
            TotalDowntime = totalDowntime,
            DowntimeMetrics = downtimeMetrics
        };
    }

    public static List<DowntimeResponseDto> CalculateDowntimeEvents(
     List<MachineLog> logs,
     DateTime start,
     DateTime end)
    {
        if (logs == null || !logs.Any())
            return new List<DowntimeResponseDto>();

        // Group logs by JobId and Status (reason)
        // ✅ FILTER: Only include logs that have BOTH Start AND End values
        var groupedByJobAndStatus = logs
            .Where(log => log.Start.HasValue && log.End.HasValue)  // ← Key change: exclude End = null
            .GroupBy(log => new {
                JobId = log.JobId ?? "N/A",
                Status = log.Status
            });

        var downtimeEvents = new List<DowntimeResponseDto>();
        double totalDuration = 0;

        foreach (var group in groupedByJobAndStatus)
        {
            double duration = 0;
            string? colorFromDb = null;

            foreach (var log in group)
            {
                // Get color from the first log that has a color value
                if (string.IsNullOrEmpty(colorFromDb) && !string.IsNullOrEmpty(log.Color))
                {
                    colorFromDb = log.Color;
                }

                // Clamp start time to range
                DateTime logStartTime = log.Start!.Value > start
                    ? log.Start.Value
                    : start;

                // Clamp end time to range (End is guaranteed to have value now)
                DateTime logEndTime = log.End!.Value < end
                    ? log.End.Value
                    : end;

                // Calculate duration in seconds
                var logDuration = (logEndTime - logStartTime).TotalSeconds;
                if (logDuration > 0)
                    duration += logDuration;
            }

            if (duration > 0)
            {
                downtimeEvents.Add(new DowntimeResponseDto
                {
                    JobId = group.Key.JobId,
                    Reason = group.Key.Status,
                    Color = colorFromDb ?? "#808080",
                    Duration = (float)duration,
                    Percentage = 0
                });

                totalDuration += duration;
            }
        }

        // ✅ OPTIMIZATION: Calculate percentages in single pass with pre-computed multiplier
        if (totalDuration > 0)
        {
            var percentageMultiplier = 100.0 / totalDuration;
            foreach (var evt in downtimeEvents)
            {
                evt.Percentage = (float)Math.Round(evt.Duration * percentageMultiplier, 2);
            }
        }

        return downtimeEvents.OrderByDescending(e => e.Duration).ToList();
    }
}