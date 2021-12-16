using System.Collections.ObjectModel;

namespace ErogeHelper.Shared.Extensions;

public static class EnumerableEx
{
    public static ReadOnlyObservableCollection<TSource> ToReadOnlyObservableCollectio<TSource>
        (this IEnumerable<TSource> source) => new(new ObservableCollection<TSource>(source));
}
