namespace MMS.Application.Common;

public static class TimeRangeHelper
{
    public static (DateTime Start, DateTime End) GetRange(TimeRange range)
    {
        var now = DateTime.UtcNow;                   // current time in UTC
        var todayMidnight = now.Date;                // today at 00:00:00
        var endOfToday = todayMidnight.AddDays(1).AddTicks(-1); // today at 23:59:59.9999999

        return range switch
        {
            // ✅ Daily: 00:00 → 23:59:59
            TimeRange.Daily => (todayMidnight, endOfToday),

            // ✅ Weekly: Last 7 days including full today
            TimeRange.Weekly => (todayMidnight.AddDays(-6), endOfToday),

            // ✅ Monthly: Last 30 days including full today
            TimeRange.Monthly => (todayMidnight.AddMonths(-1).AddDays(1), endOfToday),

            _ => (todayMidnight, endOfToday)
        };
    }
}
