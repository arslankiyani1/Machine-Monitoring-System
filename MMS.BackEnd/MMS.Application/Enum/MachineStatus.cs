namespace MMS.Application.Enum;

public enum MachineStatus
{
    InCycle = 0,
    OperationStop = 1,
    MachineAlarm = 2,
    PalletChange = 3,
    Offline = 4,
    Warning = 5,
    Error = 6,
    UnCategorized = 7
}

public enum SensorStatus
{
    Online,
    Offline,
    NotRunning
}
