using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MMS.WebAPI.Extension;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum) return;

        var enumNames = Enum.GetNames(context.Type);
        schema.Enum.Clear();
        foreach (var name in enumNames)
        {
            schema.Enum.Add(new OpenApiString(name));
        }

        schema.Type = "string";
        schema.Format = null;
    }
}
