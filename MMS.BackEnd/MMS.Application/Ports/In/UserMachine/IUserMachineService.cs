namespace MMS.Application.Ports.In.UserMachine;

public interface IUserMachineService
{
    Task<ApiResponse<IEnumerable<UserMachineDto>>> GetListAsync(PageParameters pageParameters);
    Task<ApiResponse<UserMachineDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<UserMachineDto>> AddAsync(AddUserMachineDto request);
    Task<ApiResponse<UserMachineDto>> UpdateAsync(Guid id, UpdateUserMachineDto request);
    Task<ApiResponse<string>> DeleteAsync(Guid id);
}