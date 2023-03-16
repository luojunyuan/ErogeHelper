using System.Reactive.Disposables;
using System.Reactive.Linq;
using ErogeHelper.Common.Definitions;
using ErogeHelper.Function;
using ErogeHelper.ViewModel.Preference;
using ModernWpf.Controls;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;

namespace ErogeHelper.View.Modern.Preference;

public partial class PreferenceWindow
{
    public PreferenceWindow()
    {
        InitializeComponent();
        
        ViewModel ??= DependencyResolver.GetService<PreferenceViewModel>();
        var generalViewModel = DependencyResolver.GetService<GeneralViewModel>();
        var aboutViewModel = new Lazy<AboutViewModel>(() => DependencyResolver.GetService<AboutViewModel>());

        this.WhenActivated(d =>
        {
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
                        case PreferencePageTag.General:
                            ViewModel!.Router.NavigateAndReset.Execute(generalViewModel);
                            break;
                        case PreferencePageTag.About:
                            ViewModel!.Router.NavigateAndReset.Execute(aboutViewModel.Value);
                            break;
                    }
                }).DisposeWith(d);

            NavigationView.SetCurrentValue(NavigationView.SelectedItemProperty, null);
            NavigationView.SetCurrentValue(NavigationView.SelectedItemProperty,
                NavigationView.MenuItems.OfType<NavigationViewItem>().First());

            ViewModel.DisposeWith(d);
        });
    }
}
