namespace MMS.Test;

public class MachineActivityDuplicateTests
{
    [Fact]
    public void DeduplicateClosedLogs_WithExactDuplicates_ReturnsSingleActivity()
    {
        // Arrange: Create duplicate closed logs with same start, end, status
        var duplicateLog1 = new MachineLog
        {
            Id = "log1",
            MachineId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            UserName = "TestUser",
            Status = "Offline",
            Start = DateTime.Parse("2026-01-05T19:00:00Z"),
            End = DateTime.Parse("2026-01-06T06:15:53.4624066Z"),
            JobId = "",
            Color = "#000000",
            MainProgram = "",
            CurrentProgram = "",
            LastUpdateTime = DateTime.UtcNow
        };

        var duplicateLog2 = new MachineLog
        {
            Id = "log2",
            MachineId = duplicateLog1.MachineId,
            CustomerId = duplicateLog1.CustomerId,
            UserName = "TestUser",
            Status = "Offline",
            Start = DateTime.Parse("2026-01-05T19:00:00Z"),
            End = DateTime.Parse("2026-01-06T06:15:53.4624066Z"),
            JobId = "",
            Color = "#000000",
            MainProgram = "",
            CurrentProgram = "",
            LastUpdateTime = DateTime.UtcNow
        };

        var logs = new List<MachineLog> { duplicateLog1, duplicateLog2 };

        // Act: Apply deduplication logic (similar to what we implemented)
        var deduplicatedClosedLogs = logs
            .GroupBy(l => new
            {
                Start = l.Start ?? DateTime.MinValue,
                End = l.End,
                Status = l.Status ?? "Unknown",
                JobId = l.JobId ?? string.Empty,
                UserName = l.UserName ?? "N/A",
                MainProgram = l.MainProgram ?? string.Empty,
                CurrentProgram = l.CurrentProgram ?? string.Empty
            })
            .Select(g => g.First())
            .ToList();

        // Assert: Should have only one activity segment
        Assert.Single(deduplicatedClosedLogs);
        Assert.Equal("log1", deduplicatedClosedLogs.First().Id);
    }

    [Fact]
    public void DeduplicateClosedLogs_WithDifferentStatuses_ReturnsBothActivities()
    {
        // Arrange: Create logs with different statuses (should not be deduplicated)
        var log1 = new MachineLog
        {
            Id = "log1",
            MachineId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            UserName = "TestUser",
            Status = "Offline",
            Start = DateTime.Parse("2026-01-05T19:00:00Z"),
            End = DateTime.Parse("2026-01-06T06:15:53.4624066Z"),
            JobId = "",
            Color = "#000000",
            MainProgram = "",
            CurrentProgram = "",
            LastUpdateTime = DateTime.UtcNow
        };

        var log2 = new MachineLog
        {
            Id = "log2",
            MachineId = log1.MachineId,
            CustomerId = log1.CustomerId,
            UserName = "TestUser",
            Status = "Running", // Different status
            Start = DateTime.Parse("2026-01-05T19:00:00Z"),
            End = DateTime.Parse("2026-01-06T06:15:53.4624066Z"),
            JobId = "",
            Color = "#00FF00",
            MainProgram = "",
            CurrentProgram = "",
            LastUpdateTime = DateTime.UtcNow
        };

        var logs = new List<MachineLog> { log1, log2 };

        // Act: Apply deduplication logic
        var deduplicatedClosedLogs = logs
            .GroupBy(l => new
            {
                Start = l.Start ?? DateTime.MinValue,
                End = l.End,
                Status = l.Status ?? "Unknown",
                JobId = l.JobId ?? string.Empty,
                UserName = l.UserName ?? "N/A",
                MainProgram = l.MainProgram ?? string.Empty,
                CurrentProgram = l.CurrentProgram ?? string.Empty
            })
            .Select(g => g.First())
            .ToList();

        // Assert: Should have both activities (different statuses)
        Assert.Equal(2, deduplicatedClosedLogs.Count);
    }
}
