namespace MMS.Application.Utils;

public static class UtilizationCalculator
{
    public static double CalculateUtilization(IEnumerable<MachineJob> jobs, DateTime startOfDay, DateTime endOfDay)
    {
        if (jobs == null)
            return 0;

        var completedJobs = jobs.Where(j => j.Status == JobStatus.Completed);

        if (!completedJobs.Any())
            return 0;

        var totalTimeAvailable = (endOfDay - startOfDay).TotalSeconds;

        var runTime = completedJobs.Sum(j =>
        {
            var start = j.StartTime < startOfDay ? startOfDay : j.StartTime;
            var end = j.EndTime > endOfDay ? endOfDay : j.EndTime;
            return (end > start) ? (end - start).TotalSeconds : 0;
        });

        return (runTime / totalTimeAvailable) * 100;
    }
}
