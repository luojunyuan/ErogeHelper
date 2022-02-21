using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.ViewModel.Preference;
using ModernWpf.Controls;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;

namespace ErogeHelper.View.Preference;

public partial class PreferenceWindow
{
    public PreferenceWindow()
    {
        InitializeComponent(); // 97ms

        ViewModel ??= DependencyResolver.GetService<PreferenceViewModel>(); // 28ms
        var generalViewModel = DependencyResolver.GetService<GeneralViewModel>(); // 6ms
        var mecabViewModel = DependencyResolver.GetService<MeCabViewModel>(); // 25ms
        var ttsViewModel = DependencyResolver.GetService<TTSViewModel>(); // 25ms
        //var danmakuViewModel = DependencyResolver.GetService<DanmakuViewModel>();
        var transViewModel = DependencyResolver.GetService<TransViewModel>(); // 40ms
        var aboutViewModel = DependencyResolver.GetService<AboutViewModel>(); // 40ms

        Height = ViewModel.Height;
        Width = ViewModel.Width;

        var closedEvent = this.Events().Closed
            .Select(_ => Unit.Default)
            .InvokeCommand(this, x => x.ViewModel!.Closed); // 16ms

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
                            ViewModel!.Router.NavigateAndReset.Execute(generalViewModel); // 9ms
                            break;
                        case PageTag.MeCab:
                            ViewModel!.Router.NavigateAndReset.Execute(mecabViewModel);
                            break;
                        case PageTag.TTS:
                            ViewModel!.Router.NavigateAndReset.Execute(ttsViewModel);
                            break;
                        //case PageTag.Danmaku:
                        //    ViewModel!.Router.NavigateAndReset.Execute(danmakuViewModel);
                        //    break;
                        case PageTag.Trans:
                            ViewModel!.Router.NavigateAndReset.Execute(transViewModel);
                            break;
                        case PageTag.About:
                            ViewModel!.Router.NavigateAndReset.Execute(aboutViewModel);
                            break;
                    }
                }).DisposeWith(d);

            NavigationView.SetCurrentValue(NavigationView.SelectedItemProperty, null);
            NavigationView.SetCurrentValue(NavigationView.SelectedItemProperty,
                NavigationView.MenuItems.OfType<NavigationViewItem>().First());

            closedEvent.DisposeWith(d);
            ViewModel.DisposeWith(d);
        });
    }
}
