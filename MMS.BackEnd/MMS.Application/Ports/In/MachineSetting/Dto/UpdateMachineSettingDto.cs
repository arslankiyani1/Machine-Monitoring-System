namespace MMS.Application.Ports.In.MachineSetting.Dto;

public record UpdateMachineSettingDto(
    Guid Id, 
    bool CycleStartInterlock,
    bool GuestLock,
    bool ReverseCSlockLogic,
    bool AutomaticPartsCounter,
    decimal? MaxFeedrate,
    int? MaxSpindleSpeed,
    TimeOnly? StopTimelimit,
    TimeOnly? PlannedProdusctionTime,
    TimeOnly? MinElapsedCycleTime,
    MachineSettingsStatus Status,
    List<string> DownTimeReasons,
    Guid MachineId
);

