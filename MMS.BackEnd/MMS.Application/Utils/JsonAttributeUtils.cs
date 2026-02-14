namespace MMS.Application.Utils;

public static class JsonAttributeUtils
{
    public static string? GetAttr(JsonElement attributes, string key) =>
        attributes.ValueKind != JsonValueKind.Undefined &&
        attributes.TryGetProperty(key, out var val) &&
        val.ValueKind == JsonValueKind.Array
            ? val[0].GetString()
            : null;

    public static List<string>? GetAttrList(JsonElement attributes, string key) =>
    attributes.ValueKind != JsonValueKind.Undefined &&
    attributes.TryGetProperty(key, out var val) &&
    val.ValueKind == JsonValueKind.Array
        ? val.EnumerateArray()
            .Select(v => v.GetString())
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => x!)
            .ToList()
        : null;


    public static Language GetLanguageAttr(JsonElement attributes, string key)
    {
        var raw = GetAttr(attributes, key);
        if (string.IsNullOrWhiteSpace(raw)) return Language.en;

        if (System.Enum.TryParse<Language>(raw, ignoreCase: true, out var parsed))
            return parsed;

        return Language.en;
    }
}
