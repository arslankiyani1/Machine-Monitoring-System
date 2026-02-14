using MMS.Application.Ports.In.NoSql.Alert.Dto;

namespace MMS.Application.Ports.In.NoSql.Alert;

public interface IAlertRuleService
{
    Task<ApiResponse<IEnumerable<AlertRuleDto>>> GetAllAsync(PageParameters paarameters, Guid? machineId);
    Task<ApiResponse<AlertRuleDto>> GetByIdAsync(string id);
    Task<ApiResponse<AlertRuleDto>> CreateAsync(AddAlertRuleDto entity);
    Task<ApiResponse<AlertRuleDto>> UpdateAsync(UpdateAlertRuleDto entity);
    Task<ApiResponse<AlertRuleDto>> DeleteAsync(string id);
}
