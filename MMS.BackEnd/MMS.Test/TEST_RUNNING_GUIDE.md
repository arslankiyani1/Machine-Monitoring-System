# How to Run MachineMonitoringOverlapTests Manually

This guide explains how to manually run the tests that verify the fix for the overlapping logs bug.

## Prerequisites

1. **.NET SDK 8.0** or later installed
2. **PowerShell** or **Command Prompt** access
3. **Project dependencies** restored (NuGet packages)

## Method 1: Using Command Line (Recommended)

### Step 1: Navigate to the Project Root
```powershell
cd "c:\Users\SST\Desktop\workspace\code\new github Code"
```

### Step 2: Run All Overlap Tests
```powershell
dotnet test MMS.BackEnd/MMS.Test/MMS.UnitTests.csproj --filter "FullyQualifiedName~MachineMonitoringOverlapTests"
```

### Step 3: Run a Specific Test
```powershell
# Run only the concurrent execution test
dotnet test MMS.BackEnd/MMS.Test/MMS.UnitTests.csproj --filter "FullyQualifiedName~MarkMachineOfflineAsync_And_ProcessMonitoringAsync_UseSameLockKey_NoOverlappingLogs"

# Run only the offline log closing test
dotnet test MMS.BackEnd/MMS.Test/MMS.UnitTests.csproj --filter "FullyQualifiedName~ProcessMonitoringAsync_ClosesOfflineLog_WhenMachineComesOnline"
```

### Step 4: Run with Detailed Output
```powershell
dotnet test MMS.BackEnd/MMS.Test/MMS.UnitTests.csproj --filter "FullyQualifiedName~MachineMonitoringOverlapTests" --verbosity normal
```

### Step 5: Run with Code Coverage
```powershell
dotnet test MMS.BackEnd/MMS.Test/MMS.UnitTests.csproj --filter "FullyQualifiedName~MachineMonitoringOverlapTests" --collect:"XPlat Code Coverage"
```

## Method 2: Using Visual Studio

1. **Open the Solution** in Visual Studio
2. **Open Test Explorer** (Test → Test Explorer or `Ctrl+E, T`)
3. **Build the Solution** (`Ctrl+Shift+B`)
4. **Find the Tests**:
   - Search for "MachineMonitoringOverlapTests" in Test Explorer
   - You should see 4 test methods:
     - `MarkMachineOfflineAsync_And_ProcessMonitoringAsync_UseSameLockKey_NoOverlappingLogs`
     - `ProcessMonitoringAsync_ClosesOfflineLog_WhenMachineComesOnline`
     - `MarkMachineOfflineAsync_DoesNotCreateOfflineLog_WhenMachineHasActiveLogs`
     - `ConcurrentCalls_ToMarkMachineOfflineAsync_UseSameLock_NoDuplicates`
5. **Run Tests**:
   - Right-click on the test class or individual test → **Run Selected Tests**
   - Or click the **Run All** button

## Method 3: Using Visual Studio Code

1. **Open the Workspace** in VS Code
2. **Install .NET Test Explorer Extension** (if not already installed)
3. **Open Test Explorer** (View → Testing or `Ctrl+Shift+T`)
4. **Run Tests** by clicking the play button next to each test

## Method 4: Using Rider (JetBrains)

1. **Open the Solution** in Rider
2. **Open Unit Tests Window** (View → Tool Windows → Unit Tests)
3. **Find MachineMonitoringOverlapTests** in the test tree
4. **Run Tests** by right-clicking and selecting "Run"

## Test Output Interpretation

### Successful Test Run
```
Passed!  - Failed:     0, Passed:     4, Skipped:     0, Total:     4
```

### Failed Test Run
```
Failed!  - Failed:     2, Passed:     2, Skipped:     0, Total:     4
```

### Understanding Test Results

1. **MarkMachineOfflineAsync_And_ProcessMonitoringAsync_UseSameLockKey_NoOverlappingLogs**
   - **Purpose**: Verifies both methods use the same lock key and don't create overlapping logs
   - **Expected**: At most 1 open log after concurrent execution
   - **What it tests**: Lock key consistency and race condition prevention

2. **ProcessMonitoringAsync_ClosesOfflineLog_WhenMachineComesOnline**
   - **Purpose**: Verifies offline logs are closed when machine logs arrive
   - **Expected**: Offline log is closed when ProcessMonitoringAsync is called
   - **What it tests**: Proper state transition from offline to online

3. **MarkMachineOfflineAsync_DoesNotCreateOfflineLog_WhenMachineHasActiveLogs**
   - **Purpose**: Verifies offline logs aren't created when machine is active
   - **Expected**: No offline log created when machine has active logs
   - **What it tests**: Guard condition in MarkMachineOfflineAsync

4. **ConcurrentCalls_ToMarkMachineOfflineAsync_UseSameLock_NoDuplicates**
   - **Purpose**: Verifies multiple concurrent calls don't create duplicate offline logs
   - **Expected**: Only 1 offline log created despite multiple concurrent calls
   - **What it tests**: Lock effectiveness for duplicate prevention

## Troubleshooting

### Build Errors
If you get build errors:
```powershell
# Clean and rebuild
dotnet clean MMS.BackEnd/MMS.Test/MMS.UnitTests.csproj
dotnet build MMS.BackEnd/MMS.Test/MMS.UnitTests.csproj
```

### Missing Dependencies
If tests fail due to missing dependencies:
```powershell
# Restore packages
dotnet restore MMS.BackEnd/MMS.Test/MMS.UnitTests.csproj
```

### Test Discovery Issues
If tests aren't discovered:
```powershell
# Rebuild the test project
dotnet build MMS.BackEnd/MMS.Test/MMS.UnitTests.csproj --no-incremental
```

## Quick Reference Commands

```powershell
# Build only
dotnet build MMS.BackEnd/MMS.Test/MMS.UnitTests.csproj

# Run all tests in the project
dotnet test MMS.BackEnd/MMS.Test/MMS.UnitTests.csproj

# Run with detailed logging
dotnet test MMS.BackEnd/MMS.Test/MMS.UnitTests.csproj --logger "console;verbosity=detailed"

# Run and generate test results file
dotnet test MMS.BackEnd/MMS.Test/MMS.UnitTests.csproj --filter "FullyQualifiedName~MachineMonitoringOverlapTests" --logger "trx;LogFileName=test-results.trx"
```

## Notes

- The tests use **Moq** for mocking dependencies
- Tests simulate **concurrent execution** using `Task.Run` and `Task.WhenAll`
- The distributed lock service is mocked, so actual locking behavior may differ in production
- Some tests may need refinement to properly simulate distributed lock behavior

## What the Tests Verify

✅ **Lock Key Consistency**: Both `MarkMachineOfflineAsync` and `HandleSignalBasedMonitoringAsync` use the same lock key (`machine:monitor:{machineId}`)

✅ **No Overlapping Logs**: When both methods run concurrently, only one log should be created

✅ **Proper State Transitions**: Offline logs are closed when machine comes online

✅ **Guard Conditions**: Offline logs aren't created when machine is already active
