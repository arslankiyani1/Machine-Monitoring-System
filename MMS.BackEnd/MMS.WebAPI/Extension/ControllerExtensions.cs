using System.Text.Json.Serialization;

namespace MMS.WebAPI.Extension;
public static class ControllerExtensions
{
    public static IServiceCollection AddConfiguredControllers(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<ExceptionFilter>();
        }).AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });


        return services;
    }
}