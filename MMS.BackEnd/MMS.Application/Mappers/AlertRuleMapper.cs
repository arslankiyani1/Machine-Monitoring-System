using MMS.Application.Ports.In.NoSql.Alert.Dto;

namespace MMS.Application.Mappers;

public class AlertRuleMapper : Profile
{
    public AlertRuleMapper()
    {
        // Main mappings
        CreateMap<AddAlertRuleDto, AlertRule>().ReverseMap();
        CreateMap<UpdateAlertRuleDto, AlertRule>().ReverseMap();
        CreateMap<AlertRule, AlertRuleDto>().ReverseMap();

        // Nested object mappings
        CreateMap<ConditionDto, Condition>().ReverseMap();
        CreateMap<AlertActionDto, AlertAction>().ReverseMap();
    }
}
