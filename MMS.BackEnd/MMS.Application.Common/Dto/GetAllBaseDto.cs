namespace MMS.Application.Common.Dto;

public class GetAllBaseDto : IBaseDto
{
    public PageParameters PageParameters { get; set; } = default!;
}