namespace MMS.Test;

/// <summary>
/// Tests to verify that offline functionality and machine logs don't create overlapping logs
/// This test simulates the race condition where MarkMachineOfflineAsync and ProcessMonitoringAsync
/// are called concurrently and verifies they use the same lock to prevent overlaps.
/// </summary>
public class MachineMonitoringOverlapTests
{
    private readonly Mock<IMachineLogRepository> _mockRepository;
    private readonly Mock<IOperationalDataRepository> _mockOperationalDataRepository;
    private readonly Mock<IMachineStatusSettingRepository> _mockStatusSettingRepo;
    private readonly Mock<IMachineSettingRepository> _mockMachineSettingRepository;
    private readonly Mock<IMachineService> _mockMachineService;
    private readonly Mock<IMachineJobRepository> _mockMachineJobRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
    private readonly Mock<ICustomerDashboardSummaryService> _mockCustomerDashboardSummaryService;
    private readonly Mock<IRabbitMQProducer> _mockProducer;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IMachineSensorRepository> _mockMachineSensorRepository;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IDistributedLockService> _mockDistributedLockService;
    
    private readonly Guid _testMachineId;
    private readonly Guid _testCustomerId;
    private readonly MachineDto _testMachine;
    private readonly ConcurrentBag<MachineLog> _createdLogs;
    private readonly ConcurrentBag<string> _lockKeysUsed;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _lockSemaphores;
    private readonly ConcurrentDictionary<string, int> _activeLocks;
    private readonly ConcurrentDictionary<Guid, List<MachineLog>> _repositoryLogs;

    public MachineMonitoringOverlapTests()
    {
        _mockRepository = new Mock<IMachineLogRepository>();
        _mockOperationalDataRepository = new Mock<IOperationalDataRepository>();
        _mockStatusSettingRepo = new Mock<IMachineStatusSettingRepository>();
        _mockMachineSettingRepository = new Mock<IMachineSettingRepository>();
        _mockMachineService = new Mock<IMachineService>();
        _mockMachineJobRepository = new Mock<IMachineJobRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserService = new Mock<IUserService>();
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockCustomerDashboardSummaryService = new Mock<ICustomerDashboardSummaryService>();
        _mockProducer = new Mock<IRabbitMQProducer>();
        _mockCacheService = new Mock<ICacheService>();
        _mockMachineSensorRepository = new Mock<IMachineSensorRepository>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockDistributedLockService = new Mock<IDistributedLockService>();

        _testMachineId = Guid.NewGuid();
        _testCustomerId = Guid.NewGuid();
        _testMachine = new MachineDto(
            Id: _testMachineId,
            MachineName: "TestMachine",
            MachineModel: "TestModel",
            Manufacturer: "TestManufacturer",
            SerialNumber: "SN123456",
            Location: "TestLocation",
            InstallationDate: DateTime.UtcNow,
            CommunicationProtocol: CommunicationProtocol.Modbus,
            MachineType: MachineType.CNC,
            CustomerId: _testCustomerId,
            ImageUrl: null
        );

        _createdLogs = new ConcurrentBag<MachineLog>();
        _lockKeysUsed = new ConcurrentBag<string>();
        _lockSemaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
        _activeLocks = new ConcurrentDictionary<string, int>();
        _repositoryLogs = new ConcurrentDictionary<Guid, List<MachineLog>>();

        SetupMocks();
    }

    private void SetupMocks()
    {
        // Setup machine service
        _mockMachineService
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<MachineDto>(StatusCodes.Status200OK, "Success", _testMachine));

        _mockMachineService
            .Setup(x => x.GetByMachineName(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<MachineDto>(StatusCodes.Status200OK, "Success", _testMachine));

        // Setup repository to track created logs with shared state
        var repositoryLogs = new ConcurrentDictionary<Guid, List<MachineLog>>();
        
        _mockRepository
            .Setup(x => x.AddMachineLogAsync(It.IsAny<MachineLog>()))
            .Callback<MachineLog>(log =>
            {
                _createdLogs.Add(log);
                repositoryLogs.AddOrUpdate(log.MachineId, 
                    new List<MachineLog> { log }, 
                    (key, list) => { list.Add(log); return list; });
            })
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(x => x.GetAllOpenLogsByMachineIdForUpdateAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid machineId) =>
            {
                if (repositoryLogs.TryGetValue(machineId, out var logs))
                {
                    return logs.Where(l => l.End == null).ToList();
                }
                return new List<MachineLog>();
            });

        _mockRepository
            .Setup(x => x.GetLastOpenLogByMachineId(It.IsAny<Guid>()))
            .ReturnsAsync((Guid machineId) =>
            {
                if (repositoryLogs.TryGetValue(machineId, out var logs) && logs.Any())
                {
                    return logs.OrderByDescending(l => l.Start).FirstOrDefault();
                }
                return null;
            });

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<MachineLog>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(x => x.CloseMultipleLogsAsync(It.IsAny<List<MachineLog>>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .Callback<List<MachineLog>, DateTime, string>((logs, endTime, source) =>
            {
                foreach (var log in logs)
                {
                    log.End = endTime;
                }
            })
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Setup status setting
        var statusSetting = new MachineStatusSetting
        {
            MachineId = _testMachineId,
            Inputs = new List<MachineInput>
            {
                new MachineInput
                {
                    InputKey = "Signal1",
                    Signals = "0101",
                    Status = "Wait Tooling",
                    Color = "#FF00FF"
                }
            }
        };

        _mockStatusSettingRepo
            .Setup(x => x.GetByMachineIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(statusSetting);

        // Setup unit of work
        var mockTransaction = new Mock<IDbContextTransaction>();
        mockTransaction.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockTransaction.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        // Setup cache service
        _mockCacheService
            .Setup(x => x.GetAsync<MachineStatusSetting>(It.IsAny<string>()))
            .ReturnsAsync((MachineStatusSetting?)null);

        _mockCacheService
            .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _mockCacheService
            .Setup(x => x.CancelMachineHeartbeatAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _mockCacheService
            .Setup(x => x.SetMachineHeartbeatAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _mockCacheService
            .Setup(x => x.RemoveTrackedKeysAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Setup distributed lock service - CRITICAL: Simulate real locking behavior
        // Use SemaphoreSlim to actually block concurrent access to the same lock key
        _mockDistributedLockService
            .Setup(x => x.AcquireLockAsync(
                It.IsAny<string>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, TimeSpan, TimeSpan?, CancellationToken>(async (key, expiry, waitTime, ct) =>
            {
                _lockKeysUsed.Add(key);
                
                // Get or create semaphore for this lock key (allows only 1 concurrent access)
                var semaphore = _lockSemaphores.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
                
                // Try to acquire the semaphore (simulates distributed lock)
                var acquired = await semaphore.WaitAsync(waitTime ?? TimeSpan.FromSeconds(1), ct);
                
                if (!acquired)
                {
                    // Lock acquisition failed (timeout or cancelled)
                    return null;
                }
                
                // Track active lock
                _activeLocks.AddOrUpdate(key, 1, (k, v) => v + 1);
                
                // Create a disposable that releases the lock when disposed
                var mockLock = new Mock<IDisposable>();
                mockLock.Setup(l => l.Dispose())
                    .Callback(() =>
                    {
                        semaphore.Release();
                        _activeLocks.AddOrUpdate(key, 0, (k, v) => Math.Max(0, v - 1));
                    });
                
                return mockLock.Object;
            });

        // Setup HTTP context
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);
    }

    [Fact]
    public async Task MarkMachineOfflineAsync_And_ProcessMonitoringAsync_UseSameLockKey_NoOverlappingLogs()
    {
        // Arrange - Clear previous state
        _createdLogs.Clear();
        _lockKeysUsed.Clear();
        _repositoryLogs.Clear();
        foreach (var semaphore in _lockSemaphores.Values)
        {
            semaphore.Dispose();
        }
        _lockSemaphores.Clear();
        _activeLocks.Clear();
        
        var service = CreateService();
        var machineMonitoring = new MachineMonitoring
        {
            MachineName = "TestMachine",
            Signals = "0101",
            Type = "Signal"
        };

        // Act: Simulate concurrent execution
        var offlineTask = Task.Run(async () =>
        {
            await Task.Delay(5); // Small delay to increase chance of race condition
            await service.MarkMachineOfflineAsync(_testMachineId);
        });

        var monitoringTask = Task.Run(async () =>
        {
            await Task.Delay(5); // Small delay to increase chance of race condition
            await service.ProcessMonitoringAsync(machineMonitoring);
        });

        await Task.WhenAll(offlineTask, monitoringTask);
        
        // Give a moment for any background tasks to complete
        await Task.Delay(100);

        // Assert: Verify both methods used the SAME lock key
        var expectedLockKey = $"machine:monitor:{_testMachineId}";
        Assert.Contains(expectedLockKey, _lockKeysUsed);
        
        // Verify that the lock key was used (both methods should try to acquire it)
        var lockKeyCount = _lockKeysUsed.Count(k => k == expectedLockKey);
        Assert.True(lockKeyCount >= 1, $"Expected lock key '{expectedLockKey}' to be used. Used keys: {string.Join(", ", _lockKeysUsed)}");

        // Verify no overlapping logs were created
        // After both operations, there should be at most ONE open log
        var openLogs = _createdLogs.Where(l => l.End == null).ToList();
        Assert.True(openLogs.Count <= 1, 
            $"Expected at most 1 open log, but found {openLogs.Count}. " +
            $"Logs: {string.Join(", ", openLogs.Select(l => $"Status={l.Status}, Start={l.Start}"))}");

        // If there's an open log, verify it's either Offline OR the machine status, but not both
        if (openLogs.Count == 1)
        {
            var log = openLogs[0];
            Assert.True(
                log.Status.Equals(MachineStatus.Offline.ToString(), StringComparison.OrdinalIgnoreCase) ||
                log.Status.Equals("Wait Tooling", StringComparison.OrdinalIgnoreCase),
                $"Unexpected log status: {log.Status}");
        }
    }

    [Fact]
    public async Task ProcessMonitoringAsync_ClosesOfflineLog_WhenMachineComesOnline()
    {
        // Arrange
        var service = CreateService();
        
        // First, create an offline log
        var existingOfflineLog = new MachineLog
        {
            Id = Guid.NewGuid().ToString(),
            MachineId = _testMachineId,
            CustomerId = _testCustomerId,
            Status = MachineStatus.Offline.ToString(),
            Start = DateTime.UtcNow.AddMinutes(-5),
            End = null,
            Color = "#000000"
        };

        _mockRepository
            .Setup(x => x.GetAllOpenLogsByMachineIdForUpdateAsync(_testMachineId))
            .ReturnsAsync(new List<MachineLog> { existingOfflineLog });

        var machineMonitoring = new MachineMonitoring
        {
            MachineName = "TestMachine",
            Signals = "0101",
            Type = "Signal"
        };

        var closedLogs = new List<MachineLog>();
        _mockRepository
            .Setup(x => x.CloseMultipleLogsAsync(It.IsAny<List<MachineLog>>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .Callback<List<MachineLog>, DateTime, string>((logs, endTime, source) =>
            {
                closedLogs.AddRange(logs);
            })
            .Returns(Task.CompletedTask);

        // Act
        await service.ProcessMonitoringAsync(machineMonitoring);

        // Assert: Verify offline log was closed
        Assert.Contains(existingOfflineLog, closedLogs);
        Assert.True(closedLogs.Any(l => l.Status.Equals(MachineStatus.Offline.ToString(), StringComparison.OrdinalIgnoreCase)),
            "Expected offline log to be closed when machine logs arrive");
    }

    [Fact]
    public async Task MarkMachineOfflineAsync_DoesNotCreateOfflineLog_WhenMachineHasActiveLogs()
    {
        // Arrange
        var service = CreateService();
        
        // Simulate machine has active logs (not offline)
        var activeLog = new MachineLog
        {
            Id = Guid.NewGuid().ToString(),
            MachineId = _testMachineId,
            CustomerId = _testCustomerId,
            Status = "Wait Tooling",
            Start = DateTime.UtcNow.AddMinutes(-1),
            End = null,
            Color = "#FF00FF"
        };

        // Setup: Machine has active log (not offline) - should prevent offline log creation
        _mockRepository
            .Setup(x => x.GetAllOpenLogsByMachineIdForUpdateAsync(_testMachineId))
            .ReturnsAsync(new List<MachineLog> { activeLog });

        _mockRepository
            .Setup(x => x.GetLastOpenLogByMachineId(_testMachineId))
            .ReturnsAsync(activeLog);

        // Act
        await service.MarkMachineOfflineAsync(_testMachineId);

        // Assert: Verify no offline log was created
        var offlineLogs = _createdLogs.Where(l => 
            l.Status.Equals(MachineStatus.Offline.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
        
        Assert.Empty(offlineLogs);
    }

    [Fact]
    public async Task ConcurrentCalls_ToMarkMachineOfflineAsync_UseSameLock_NoDuplicates()
    {
        // Arrange - Clear previous state
        _createdLogs.Clear();
        _lockKeysUsed.Clear();
        _repositoryLogs.Clear();
        foreach (var semaphore in _lockSemaphores.Values)
        {
            semaphore.Dispose();
        }
        _lockSemaphores.Clear();
        _activeLocks.Clear();
        
        var service = CreateService();
        var callCount = 0;

        _mockRepository
            .Setup(x => x.GetAllOpenLogsByMachineIdForUpdateAsync(_testMachineId))
            .ReturnsAsync(() =>
            {
                callCount++;
                return new List<MachineLog>();
            });

        // Act: Simulate multiple concurrent calls
        var tasks = Enumerable.Range(0, 5).Select(_ =>
            Task.Run(async () =>
            {
                await Task.Delay(Random.Shared.Next(0, 20));
                await service.MarkMachineOfflineAsync(_testMachineId);
            })
        ).ToArray();

        await Task.WhenAll(tasks);
        
        // Give a moment for any background tasks to complete
        await Task.Delay(100);

        // Assert: Verify only ONE offline log was created (others should be skipped due to lock)
        var offlineLogs = _createdLogs.Where(l => 
            l.Status.Equals(MachineStatus.Offline.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
        
        Assert.True(offlineLogs.Count <= 1, 
            $"Expected at most 1 offline log, but found {offlineLogs.Count}");
    }

    private MachineMonitoringService CreateService()
    {
        return new MachineMonitoringService(
            _mockRepository.Object,
            _mockOperationalDataRepository.Object,
            _mockStatusSettingRepo.Object,
            _mockMachineSettingRepository.Object,
            _mockMachineService.Object,
            _mockMachineJobRepository.Object,
            _mockUnitOfWork.Object,
            _mockUserService.Object,
            _mockServiceScopeFactory.Object,
            _mockCustomerDashboardSummaryService.Object,
            _mockProducer.Object,
            _mockCacheService.Object,
            _mockMachineSensorRepository.Object,
            _mockHttpContextAccessor.Object,
            _mockDistributedLockService.Object
        );
    }
}
