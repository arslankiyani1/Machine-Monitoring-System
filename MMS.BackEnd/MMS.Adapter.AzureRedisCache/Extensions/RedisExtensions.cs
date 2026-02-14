namespace MMS.Adapter.AzureRedisCache.Extensions;

/// <summary>
/// Extension methods for Redis adapter dependency injection (Dependency Inversion Principle)
/// </summary>
public static class RedisExtensions
{
    /// <summary>
    /// Registers all Redis cache services following SOLID principles
    /// </summary>
    public static IServiceCollection AddRedisCacheServices(this IServiceCollection services)
    {
        // Register adapters (abstractions)
        services.AddSingleton<Abstractions.IRedisDatabaseAdapter>(sp =>
        {
            var redis = sp.GetRequiredService<IConnectionMultiplexer>();
            return new Implementations.RedisDatabaseAdapter(redis.GetDatabase());
        });

        services.AddSingleton<Abstractions.IRedisServerAdapter>(sp =>
        {
            var redis = sp.GetRequiredService<IConnectionMultiplexer>();
            return new Implementations.RedisServerAdapter(redis);
        });

        // Register serializer
        services.AddSingleton<Abstractions.ICacheSerializer, Implementations.JsonCacheSerializer>();

        // Register specialized services (Single Responsibility Principle)
        services.AddScoped<Implementations.RedisCacheOperations>();
        services.AddScoped<Abstractions.IKeyTracker, Implementations.RedisKeyTracker>();
        services.AddScoped<Abstractions.IMachineHeartbeatService, Implementations.RedisMachineHeartbeatService>();

        // Register main cache service (composed of specialized services)
        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }

    /// <summary>
    /// Registers distributed lock service
    /// </summary>
    public static IServiceCollection AddDistributedLockService(this IServiceCollection services)
    {
        // Ensure RedisDatabaseAdapter is registered
        if (!services.Any(s => s.ServiceType == typeof(Abstractions.IRedisDatabaseAdapter)))
        {
            services.AddSingleton<Abstractions.IRedisDatabaseAdapter>(sp =>
            {
                var redis = sp.GetRequiredService<IConnectionMultiplexer>();
                return new Implementations.RedisDatabaseAdapter(redis.GetDatabase());
            });
        }

        services.AddScoped<IDistributedLockService, DistributedLockService>();
        return services;
    }
}


