using ErogeHelper.Shared;
using ErogeHelper.View.TextDisplay;
using ErogeHelper.ViewModel.TextDisplay;
using ReactiveUI;
using Splat;

namespace ErogeHelper.Platform.RxUI;

internal class ItemViewLocator : IViewLocator
{
    private readonly IViewLocator _defaultViewLocator;

    public ItemViewLocator()
    {
        _defaultViewLocator = DependencyResolver.GetService<IViewLocator>();
    }

    public IViewFor? ResolveView<T>(T? viewModel, string? contract = null)
    {
        ArgumentNullException.ThrowIfNull(viewModel, nameof(viewModel));

        if (viewModel is FuriganaItemViewModel)
        {
            this.Log().Debug($"Resolved service type '{typeof(FuriganaItemViewModel)}'");
            return Activator.CreateInstance(typeof(FuriganaItem), viewModel) as IViewFor;
        }
        else
        {
            return _defaultViewLocator.ResolveView(viewModel, contract);
        }
    }
}
