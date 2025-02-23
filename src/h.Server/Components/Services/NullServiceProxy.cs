using System.Reflection;

namespace h.Server.Components.Services;

public class NullServiceProxy : DispatchProxy
{
    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        // If the method returns void, simply do nothing.
        if (targetMethod.ReturnType == typeof(void))
            return null;

        // For methods with a return type, return the default value.
        return GetDefaultValue(targetMethod.ReturnType);
    }

    private object GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}

public static class NullServiceProxyServiceCollectionExtensions
{
    public static IServiceCollection AddNullService<T>(this IServiceCollection services)
        where T : class
    {
        services.AddScoped<T>(sp => DispatchProxy.Create<T, NullServiceProxy>());
        return services;
    }
}

