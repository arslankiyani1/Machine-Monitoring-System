namespace MMS.Application.Common.Dto;

[DebuggerDisplay("Skip: {" + nameof(Skip) + "}, Top: {" + nameof(Top) + "}, Term: {" + nameof(Term) + "}")]
public class PageParameters : IBaseDto
{
    private const int MaxPageSize = 100;
    private int _top = 10;

    [JsonPropertyName("skip")]
    public int? Skip { get; set; } = 0;

    [JsonPropertyName("top")]
    public int? Top
    {
        get => _top;
        set => _top = (value.HasValue && value.Value > MaxPageSize)
            ? MaxPageSize
            : (value ?? 10);
    }

    [JsonPropertyName("term")]
    public string? Term { get; set; }
}
