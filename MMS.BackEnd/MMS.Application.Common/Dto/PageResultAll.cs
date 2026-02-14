namespace MMS.Application.Common.Dto;

public class PageResultAll<TItem, TCount> : IBaseDto
{
    private PageResultAll(IEnumerable<TItem> withFilter, TCount withOutFilter)
    {
        Meta = new MetaObject(withFilter.Count(), Convert.ToInt32(withOutFilter));
        Items = withFilter;
    }

    [JsonPropertyName("_meta")]
    public MetaObject Meta { [UsedImplicitly] get; }

    [JsonPropertyName("items")]
    public IEnumerable<TItem> Items { [UsedImplicitly] get; }
    public static PageResultAll<TItem, TCount> Create(IEnumerable<TItem> items, TCount withOutFilter)
        => new PageResultAll<TItem, TCount>(items, withOutFilter);
}