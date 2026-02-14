namespace MMS.Application.Enum;

public enum DowntimeReason
{
    [JsonPropertyName("Maintenance")]
    Maintenance,              // Routine maintenance
    [JsonPropertyName("PM")]
    PM,                       // Preventive maintenance
    [JsonPropertyName("SetupChangeover")]
    SetupChangeover,          // Job or product changeover
    [JsonPropertyName("SetUp")]
    SetUp,                    // Machine setup/preparation before running
    [JsonPropertyName("Calibration")]
    Calibration,              // Machine or sensor calibration
    [JsonPropertyName("Inspection")]
    Inspection,               // Quality inspection during production
    [JsonPropertyName("ToolChange")]
    ToolChange,               // Replacing or adjusting tool
    [JsonPropertyName("Tooling")]
    Tooling,                  // Tool preparation or tooling change
    [JsonPropertyName("Cleaning")]
    Cleaning,                 // Routine cleaning
    [JsonPropertyName("ClearChips")]
    ClearChips,               // Clearing chips/swarf from machine
    [JsonPropertyName("UnplannedRepair")]
    UnplannedRepair,          // Emergency repair needed
    [JsonPropertyName("HydraulicFailure")]
    HydraulicFailure,         // Hydraulic system malfunction
    [JsonPropertyName("CoolantFailure")]
    CoolantFailure,           // Coolant or lubrication issue
    [JsonPropertyName("SpindleFailure")]
    SpindleFailure,           // Spindle malfunction
    [JsonPropertyName("SensorFault")]
    SensorFault,              // Faulty sensor input
    [JsonPropertyName("PowerFailure")]
    PowerFailure,             // Power outage or trip
    [JsonPropertyName("NetworkFailure")]
    NetworkFailure,           // IoT or PLC connectivity issue
    [JsonPropertyName("LoadUnload")]
    LoadUnload,               // Loading or unloading parts
    [JsonPropertyName("Break")]
    Break,                    // Operator short break
    [JsonPropertyName("Lunch")]
    Lunch,                    // Lunch or long break
    [JsonPropertyName("NoOperator")]
    NoOperator,               // Operator not present
    [JsonPropertyName("WaitMaterials")]
    WaitMaterials,            // No material available
    [JsonPropertyName("NoMaterial")]
    NoMaterial,               // Same as WaitMaterials (alternative name)
    [JsonPropertyName("WaitTooling")]
    WaitTooling,              // Tool not ready
    [JsonPropertyName("WaitProgram")]
    WaitProgram,              // Waiting for CNC program
    [JsonPropertyName("WaitInspection")]
    WaitInspection,           // Waiting for quality approval
    [JsonPropertyName("WaitMaintenance")]
    WaitMaintenance,          // Waiting for maintenance person
    [JsonPropertyName("ShiftChange")]
    ShiftChange,              // Operator/shift change time
    [JsonPropertyName("LowUtilization")]
    LowUtilization,           // Below expected production rate
    [JsonPropertyName("EmergencyStop")]
    EmergencyStop,            // Emergency stop triggered
    [JsonPropertyName("SafetyInterlock")]
    SafetyInterlock,          // Safety door or interlock open
    [JsonPropertyName("EnvironmentalCondition")]
    EnvironmentalCondition,   // Temperature/humidity condition
    [JsonPropertyName("PlannedShutdown")]
    PlannedShutdown,          // Planned holiday or shutdown
    [JsonPropertyName("PowerSaveMode")]
    PowerSaveMode,            // Sleep/standby mode

    [JsonPropertyName("Other")]
    Other                     // Unspecified reason
}