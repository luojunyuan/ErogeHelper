using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ErogeHelper.Share;
using ErogeHelper.Share.Contracts;
using ErogeHelper.ViewModel.Windows;
using ModernWpf.Controls;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;

namespace ErogeHelper.View.Windows
{
    public partial class PreferenceWindow
    {
        // XXX: Using static to avoid memory leak?
        //private static readonly PreferenceNavigationView NavigationView = new();

        public PreferenceWindow()
        {
            InitializeComponent();

            ViewModel ??= DependencyResolver.GetService<PreferenceViewModel>();
            Height = ViewModel.Height;
            Width = ViewModel.Width;

            this.Events().Closed
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
                                ViewModel!.Router.NavigateAndReset.Execute(ViewModel.GeneralViewModel);
                                break;
                            case PageTag.About:
                                ViewModel!.Router.NavigateAndReset.Execute(ViewModel.AboutViewModel);
                                break;
                            default:
                                break;
                        }
                    }).DisposeWith(d);

                NavigationView.SelectedItem = null;
                NavigationView.SelectedItem = NavigationView.MenuItems.OfType<NavigationViewItem>().First();
            });
        }
    }
}
