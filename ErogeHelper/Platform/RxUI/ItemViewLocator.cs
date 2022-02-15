using ErogeHelper.Shared;
using ErogeHelper.View.TextDisplay;
using ErogeHelper.ViewModel.TextDisplay;
using ReactiveUI;
using Splat;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Shared.Enums;

namespace ErogeHelper.Platform.RxUI;

internal class ItemViewLocator : IViewLocator
{
    private readonly IViewLocator _defaultViewLocator;
    private readonly IEHConfigRepository _ehConfigRepository;

    public ItemViewLocator(IEHConfigRepository ehConfigRepository)
    {
        _ehConfigRepository = ehConfigRepository;
        _defaultViewLocator = DependencyResolver.GetService<IViewLocator>();
    }

    public IViewFor? ResolveView<T>(T? viewModel, string? contract = null)
    {
        ArgumentNullException.ThrowIfNull(viewModel, nameof(viewModel));

        if (viewModel is FuriganaItemViewModel)
        {
            this.Log().Debug($"Resolved service type '{typeof(FuriganaItemViewModel)}'");
            return _ehConfigRepository.KanaPosition switch
            {
                KanaPosition.Top => Activator.CreateInstance(typeof(FuriganaTop), viewModel) as IViewFor,
                KanaPosition.None => Activator.CreateInstance(typeof(FuriganaNone), viewModel) as IViewFor,
                KanaPosition.Bottom => Activator.CreateInstance(typeof(FuriganaBottom), viewModel) as IViewFor,
                _ => throw new NotImplementedException()
            };
        }
        else
        {
            return _defaultViewLocator.ResolveView(viewModel, contract);
        }
    }
}
