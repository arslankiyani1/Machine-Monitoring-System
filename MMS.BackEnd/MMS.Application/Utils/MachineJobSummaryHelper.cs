namespace MMS.Application.Utils;
public static class MachineJobSummaryHelper
{
    public static List<MachineJobSummaryDto> GetMachineJobSummaries(List<HistoricalStats> historicalRecords)
    {
        var summaries = new List<MachineJobSummaryDto>();

        if (historicalRecords == null || !historicalRecords.Any())
        {
            return summaries;
        }


        // Calculate and add average summary from all historical records first
        var count = historicalRecords.Count;
        var sumGood = (float)historicalRecords.Sum(h => h.QtyGood);
        var sumBad = (float)historicalRecords.Sum(h => h.QtyBad);
        var avgOee = (float)historicalRecords.Average(h => h.OEE);
        var avgPerformance = (float)historicalRecords.Average(h => h.Performance);
        var avgAvailability = (float)historicalRecords.Average(h => h.Availability);
        var avgQuality = (float)historicalRecords.Average(h => h.Quality);

        var emptyId = Guid.NewGuid();

        var avgDto = new MachineJobSummaryDto
        {
            Id = emptyId.ToString(),
            Status = JobStatus.None,
            JobName = "All",
            OperatorName = string.Empty,
            ProgramNo = "All",
            Good = (int)Math.Round(sumGood),
            Bad = (int)Math.Round(sumBad),
            Oee = avgOee,
            Performance = avgPerformance,
            Availability = avgAvailability,
            Quality = avgQuality,
            StartTime = default(DateTime),
            EndTime = null
        };
        summaries.Add(avgDto);

        // Sort historical records in descending order by GeneratedDate
        var orderedRecords = historicalRecords
            .OrderByDescending(h => h.GeneratedDate)
            .ToList();
        // Add individual job summaries from JobMetrics
        foreach (var historicalRecord in orderedRecords)
        {
            foreach (var jobMetric in historicalRecord.JobMetrics ?? new List<JobMetric>())
            {
                var dto = new MachineJobSummaryDto
                {
                    Id = jobMetric.Id,
                    Status = jobMetric.Status,
                    JobName = jobMetric.JobName,
                    OperatorName = jobMetric.OperatorName,
                    ProgramNo = jobMetric.ProgramNo,
                    Good = jobMetric.QtyGood,
                    Bad = jobMetric.QtyBad,
                    Oee = (float)jobMetric.OEE,
                    Performance = (float)jobMetric.Performance,
                    Availability = (float)jobMetric.Availability,
                    Quality = (float)jobMetric.Quality,
                    StartTime = jobMetric.StartTime,
                    EndTime = jobMetric.EndTime
                };
                summaries.Add(dto);
            }
        }

        return summaries;
    }

    public static List<MachineJobSummaryDto> GetMachineJobSummariesFromJobList(IEnumerable<MachineJob> machineJobsList, DateTime startOfDay, DateTime endOfDay)
    {
        var summaries = new List<MachineJobSummaryDto>();

        if (machineJobsList == null || !machineJobsList.Any())
        {
            return summaries;
        }

        // Initialize accumulators for averages
        double totalPlanned = 0;
        double totalDowntime = 0;
        double totalOperating = 0;
        double totalGood = 0;
        double totalBad = 0;
        double totalOee = 0;
        double totalAvailability = 0;
        double totalPerformance = 0;
        double totalQuality = 0;
        int count = 0;

        // Calculate individual job summaries
        var jobMetrics = new List<JobMetric>();
        foreach (var job in machineJobsList)
        {
            double n = job.MachineIds.Count;
            double planned_sec = (job.Schedule.PlannedEnd - job.Schedule.PlannedStart).TotalSeconds;
            if (planned_sec <= 0) continue;

            double downtime_sec = DowntimeCalculator.CalculateJobDowntimeSeconds(job);
            double operating_sec = Math.Max(planned_sec - downtime_sec, 0);

            double good_apport = job.Quantities.Good ;
            double bad_apport = job.Quantities.Bad;
            double completed_apport = job.Quantities.Completed;
            double required_apport = job.Quantities.Required;

            double avail_ratio = planned_sec > 0 ? operating_sec / planned_sec : 0;
            double perf_ratio = (operating_sec > 0 && job.Metrics.TargetCycleTime > 0)
                ? (job.Metrics.TargetCycleTime * good_apport) / operating_sec
                : 0;
            perf_ratio = Math.Clamp(perf_ratio, 0, 1);

            double qual_ratio = (good_apport + bad_apport > 0) ? good_apport / (good_apport + bad_apport) : 0;
            qual_ratio = Math.Clamp(qual_ratio, 0, 1);

            double oee = avail_ratio * perf_ratio * qual_ratio * 100;

            double utilization = UtilizationCalculator.CalculateUtilization(new List<MachineJob> { job }, startOfDay, endOfDay);

            var jm = new JobMetric
            {
                Id = job.Id,
                Status = job.Status,
                JobName = job.JobName,
                ProgramNo = job.ProgramNo ?? string.Empty,
                StartTime = job.StartTime,
                EndTime = job.EndTime,
                Availability = avail_ratio * 100,
                Performance = perf_ratio * 100,
                Quality = qual_ratio * 100,
                OEE = oee,
                QtyCompleted = (int)Math.Round(completed_apport),
                QtyGood = (int)Math.Round(good_apport),
                QtyBad = (int)Math.Round(bad_apport),
                PlannedTime = planned_sec,
                RequiredQty = (int)Math.Round(required_apport),
            };

            // Create DTO for individual job
            var dto = new MachineJobSummaryDto
            {
                Id = jm.Id,
                Status = jm.Status,
                JobName = jm.JobName,
                OperatorName = jm.OperatorName ?? string.Empty,
                ProgramNo = jm.ProgramNo ?? string.Empty,
                Good = jm.QtyGood,
                Bad = jm.QtyBad,
                Oee = (float)jm.OEE,
                Performance = (float)jm.Performance,
                Availability = (float)jm.Availability,
                Quality = (float)jm.Quality,
                StartTime = jm.StartTime,
                EndTime = jm.EndTime
            };
            summaries.Add(dto);

            // Update accumulators for averages
            totalPlanned += planned_sec;
            totalDowntime += downtime_sec;
            totalOperating += operating_sec;
            totalGood += good_apport;
            totalBad += bad_apport;
            totalOee += oee;
            totalAvailability += avail_ratio * 100;
            totalPerformance += perf_ratio * 100;
            totalQuality += qual_ratio * 100;
            count++;
        }

        // Calculate and add average summary at the start
        var avgDto = new MachineJobSummaryDto
        {
            Id = Guid.NewGuid().ToString(),
            Status = JobStatus.None,
            JobName = "All",
            OperatorName = string.Empty,
            ProgramNo = "All",
            Good = (int)totalGood,
            Bad = (int)totalBad,
            Oee = count > 0 ? (float)(totalOee / count) : 0,
            Performance = count > 0 ? (float)(totalPerformance / count) : 0,
            Availability = count > 0 ? (float)(totalAvailability / count) : 0,
            Quality = count > 0 ? (float)(totalQuality / count) : 0,
            StartTime = default(DateTime),
            EndTime = null
        };
        summaries.Insert(0, avgDto);

        return summaries;
    }
}