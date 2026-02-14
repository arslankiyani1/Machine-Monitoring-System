namespace MMS.Application.Common.Dto;

[DebuggerDisplay("TotalCount: {" + nameof(TotalCount) + "}")]
public class MetaObject : IBaseDto
{
    [JsonPropertyName("_totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("_totalAllRecords")]
    public int TotalAllRecords { get; set; }

    public MetaObject(int totalCount)
    {
        TotalCount = totalCount;
    }

    public MetaObject(int totalCount, int count)
    {
        TotalCount = totalCount;
        TotalAllRecords = count;
    }
}