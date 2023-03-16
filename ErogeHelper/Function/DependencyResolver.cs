using Splat;

namespace ErogeHelper.Function;

public static class DependencyResolver
{
    public static T GetService<T>() => Locator.Current.GetService<T>() ??
                                       throw new InvalidOperationException(
                                           $"No service for type {typeof(T)} has been registered.");
}