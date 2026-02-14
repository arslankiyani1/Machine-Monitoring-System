namespace MMS.Application.Utils;

public class OeeCalculator
{
    public static (float Availability, float Performance, float Quality, float Oee) CalculateOeeMetrics(
        IEnumerable<MachineJob> machineJobs, float sumPlannedSec)
    {
        float sumWeightedAvail = 0f, sumWeightedPerf = 0f, sumWeightedQual = 0f;

        if (machineJobs?.Any() == true)
        {
            foreach (var job in machineJobs)
            {
                var plannedDuration = job.Schedule.PlannedEnd - job.Schedule.PlannedStart;
                float plannedSec = (float)plannedDuration.TotalSeconds;
                if (plannedSec <= 0) continue;

                float downtimeSec = job.DowntimeEvents?.Sum(d =>
                {
                    var start = d.StartTime < job.Schedule.PlannedStart ? job.Schedule.PlannedStart : d.StartTime;
                    var end = d.EndTime > job.Schedule.PlannedEnd ? job.Schedule.PlannedEnd : d.EndTime;
                    return (end > start) ? (float)(end - start).TotalSeconds : 0f;
                }) ?? 0f;

                float RuntimeSeconds = Math.Max(plannedSec - downtimeSec, 0);

                float availabilityRatio = Math.Clamp((plannedSec - downtimeSec) / plannedSec, 0f, 1f);

                float performanceRatio = 0f;
                if (job.Metrics?.TargetCycleTime > 0 && RuntimeSeconds > 0)
                {
                    var goodCount = job.Quantities?.Good ?? 0;
                    performanceRatio = Math.Clamp(
                        (job.Metrics.TargetCycleTime * goodCount) / RuntimeSeconds,
                        0f, 1f
                    );
                }

                float qualityRatio = 0f;
                var good = job.Quantities?.Good ?? 0;
                var bad = job.Quantities?.Bad ?? 0;
                var total = good + bad;
                if (total > 0)
                {
                    qualityRatio = (float)good / total;
                }

                sumWeightedAvail += availabilityRatio * plannedSec;
                sumWeightedPerf += performanceRatio * plannedSec;
                sumWeightedQual += qualityRatio * plannedSec;
            }
        }

        float divisor = sumPlannedSec > 0 ? sumPlannedSec : 1;
        float overallAvailRatio = sumPlannedSec > 0f ? sumWeightedAvail / divisor : 0f;
        float overallPerfRatio = sumPlannedSec > 0f ? sumWeightedPerf / divisor : 0f;
        float overallQualRatio = sumPlannedSec > 0f ? sumWeightedQual / divisor : 0f;
        float overallOeeRatio = overallAvailRatio * overallPerfRatio * overallQualRatio;

        return (
            Availability: (float)Math.Round(overallAvailRatio * 100, 2),
            Performance: (float)Math.Round(overallPerfRatio * 100, 2),
            Quality: (float)Math.Round(overallQualRatio * 100, 2),
            Oee: (float)Math.Round(overallOeeRatio * 100, 2)
        );
    }
}
