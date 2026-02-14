namespace MMS.Application.Common;

public static class CollectionExtensions
{
    /// <summary>
    /// Returns the last element of a sequence, or default(T) if the sequence is empty.
    /// </summary>
    public static T? LastOrDefault<T>(this IEnumerable<T> source)
    {
        if (source == null) return default;

        // If it's a list, use index for efficiency
        if (source is IList<T> list)
        {
            return list.Count > 0 ? list[list.Count - 1] : default;
        }

        // Otherwise, enumerate the sequence
        T? result = default;
        foreach (var item in source)
        {
            result = item;
        }
        return result;
    }
}