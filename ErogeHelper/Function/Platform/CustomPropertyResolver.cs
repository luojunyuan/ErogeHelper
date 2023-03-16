using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using ReactiveUI;

namespace ErogeHelper.Function.Platform;

// https://stackoverflow.com/questions/30352447/using-reactiveuis-bindto-to-update-a-xaml-property-generates-a-warning/#31464255
public class CustomPropertyResolver : ICreatesObservableForProperty
{
    public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged = false)
    {
        if (!typeof(FrameworkElement).IsAssignableFrom(type))
            return 0;

        var fi = type.GetTypeInfo()
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .FirstOrDefault(x => x.Name == propertyName);

        return fi is not null ? 2 /* POCO affinity+1 */ : 0;
    }

    public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
        object sender,
        System.Linq.Expressions.Expression expression,
        string propertyName,
        bool beforeChanged = false,
        bool suppressWarnings = false)
    {
        var foo = (FrameworkElement)sender;

        return Observable.Return(
            new ObservedChange<object, object>(sender, expression, null!),
            new DispatcherScheduler(foo.Dispatcher)).Concat(Observable.Never<IObservedChange<object, object>>());
    }
}
