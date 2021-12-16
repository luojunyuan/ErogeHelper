using ErogeHelper.Shared;
using ErogeHelper.View.Items;
using ErogeHelper.ViewModel.Items;
using ReactiveUI;
using Splat;

namespace ErogeHelper.Functions;

internal class ItemViewLocator : IViewLocator, IEnableLogger
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
            return Activator.CreateInstance(typeof(FuriganaItem)) as IViewFor;
        }
        else
        {
            return _defaultViewLocator.ResolveView(viewModel, contract);
        }
    }
}
