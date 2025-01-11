using h.Primitives.Games;
using Microsoft.Extensions.DependencyInjection;

namespace h.Contracts;

public static class DependencyInjection
{
    public static IServiceCollection AddShared(this IServiceCollection services)
    {
        return services;
    }
}
