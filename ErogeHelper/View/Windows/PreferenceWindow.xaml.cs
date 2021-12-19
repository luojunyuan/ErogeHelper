using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.ViewModel.Pages;
using ErogeHelper.ViewModel.Windows;
using ModernWpf.Controls;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;

namespace ErogeHelper.View.Windows;

public partial class PreferenceWindow
{
    public PreferenceWindow()
    {
        InitializeComponent();

        ViewModel ??= DependencyResolver.GetService<PreferenceViewModel>();
        var generalViewModel = DependencyResolver.GetService<GeneralViewModel>();
        var mecabViewModel = DependencyResolver.GetService<MeCabViewModel>();
        var aboutViewModel = DependencyResolver.GetService<AboutViewModel>();

        Height = ViewModel.Height;
        Width = ViewModel.Width;

        var closedEvent = this.Events().Closed
            .Select(_ => Unit.Default)
            .InvokeCommand(this, x => x.ViewModel!.Closed);

        this.WhenActivated(d =>
        {
            this.Bind(ViewModel,
                vm => vm.Height,
                v => v.Height).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.Width,
                v => v.Width).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.PageHeader,
                v => v.HeaderBlock.Text).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.Router,
                v => v.RoutedViewHost.Router).DisposeWith(d);

            NavigationView.Events().SelectionChanged
                .Subscribe(parameter =>
                {
                    if (parameter.args.SelectedItem is not NavigationViewItem { Tag: string tag })
                    {
                        return;
                    }

                    switch (tag)
                    {
                        case PageTag.General:
                            ViewModel!.Router.NavigateAndReset.Execute(generalViewModel);
                            break;
                        case PageTag.MeCab:
                            ViewModel!.Router.NavigateAndReset.Execute(mecabViewModel);
                            break;
                        case PageTag.About:
                            ViewModel!.Router.NavigateAndReset.Execute(aboutViewModel);
                            break;
                        default:
                            break;
                    }
                }).DisposeWith(d);

            NavigationView.SelectedItem = null;
            NavigationView.SelectedItem = NavigationView.MenuItems.OfType<NavigationViewItem>().First();

            closedEvent.DisposeWith(d);
            ViewModel.DisposeWith(d);
        });
    }
}
