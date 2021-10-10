using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.ViewModel.Pages;
using ErogeHelper.ViewModel.Windows;
using ModernWpf.Controls;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace ErogeHelper.View.Windows
{
    /// <summary>
    /// PreferenceWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PreferenceWindow
    {
        public PreferenceWindow(
            PreferenceViewModel? preferenceViewModel = null,
            GeneralViewModel? generalViewModel = null,
            CloudSavedataViewModel? cloudSavedataViewModel = null,
            AboutViewModel? aboutViewModel = null)
        {
            InitializeComponent();

            ViewModel = preferenceViewModel ?? DependencyResolver.GetService<PreferenceViewModel>();
            generalViewModel ??= new GeneralViewModel(hostScreen: ViewModel);
            cloudSavedataViewModel ??= new CloudSavedataViewModel(hostScreen: ViewModel);
            aboutViewModel ??= new AboutViewModel(hostScreen: ViewModel);

            Width = ViewModel!.Width;
            Height = ViewModel!.Height;

            this.Events().Closed
                .Select(_ => Unit.Default)
                .InvokeCommand(this, x => x.ViewModel!.ClosedCommand);

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

                this.Bind(ViewModel,
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
                            case PageTags.General:
                                ViewModel.Router.NavigateAndReset.Execute(generalViewModel);
                                break;
                            case PageTags.About:
                                ViewModel.Router.NavigateAndReset.Execute(aboutViewModel);
                                break;
                            default:
                                break;
                        }
                    }).DisposeWith(d);

                NavigationView.SelectedItem = NavigationView.MenuItems.OfType<NavigationViewItem>().First();
            });
        }
    }
}
