namespace MMS.Application.Ports.In.MachineMaintenenceTask.Dto;

public record AddMachineMaintenanceTaskDto(
  string MaintenanceTaskName,                
    string? Reason,                            
    string? Notes,                             
    List<IFormFile>? Files,                    
    MaintenanceTaskCategory Category,          
    PriorityLevel Priority,                   
    DateTime StartTime,                      
    DateTime EndTime,                       
    DateTime ScheduledDate,                  
    DateTime DueDate,                        
    Guid MachineId                      
);