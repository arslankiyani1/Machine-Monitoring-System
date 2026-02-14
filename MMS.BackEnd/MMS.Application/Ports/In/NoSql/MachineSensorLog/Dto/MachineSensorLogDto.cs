namespace MMS.Application.Ports.In.NoSql.MachineSensorLog.Dto;

// ✅ Response DTO (Read)
public record MachineSensorLogDto(
    Guid Id,
    Guid MachineId,
    Guid SensorId,
    SensorStatus SensorStatus,
    List<SensorReadingDto> SensorReading,
    DateTime DateTime
);

// ✅ Add DTO (Create)
public record AddMachineSensorLogDto(
    Guid MachineId,
    Guid SensorId,
    SensorStatus SensorStatus,
    List<SensorReadingDto> SensorReading,
    DateTime DateTime
);

// ✅ Update DTO
public record UpdateMachineSensorLogDto(
    Guid Id,
    Guid MachineId,
    Guid SensorId,
    SensorStatus SensorStatus,
    List<SensorReadingDto> SensorReading,
    DateTime DateTime
);

// ✅ Nested DTO for SensorReading
public record SensorReadingDto(
    ParameterType Key,
    float Value,
    string Unit
);