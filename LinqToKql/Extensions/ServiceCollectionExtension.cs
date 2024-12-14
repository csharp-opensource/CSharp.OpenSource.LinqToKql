using CSharp.OpenSource.LinqToKql.Provider;
using Microsoft.Extensions.DependencyInjection;

namespace CSharp.OpenSource.LinqToKql.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddKustoDbContext<TI, T, S>(this IServiceCollection services, Func<IServiceProvider, S>? getExecutor = null)
        where TI : class
        where T : class, TI, IKustoDbContext
        where S : class, ILinqToKqlProviderExecutor
    {
        getExecutor ??= sp => sp.GetRequiredService<S>();
        services.AddSingleton<IKustoDbContextExecutor<T>>(sp => new KustoDbContextExecutor<T>(getExecutor(sp)));
        services.AddSingleton<TI, T>();
        if (typeof(T) != typeof(TI))
        {
            services.AddSingleton<T, T>();
        }
        return services;
    }

    public static IServiceCollection AddKustoDbContext<T, S>(this IServiceCollection services, Func<IServiceProvider, S>? getExecutor = null)
        where T : class, IKustoDbContext
        where S : class, ILinqToKqlProviderExecutor
    {
        return services.AddKustoDbContext<T, T, S>(getExecutor);
    }
}
