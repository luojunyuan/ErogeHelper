﻿using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.ViewModel.Pages;
using ErogeHelper.ViewModel.Windows;
using ModernWpf.Controls;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Disposables;

namespace ErogeHelper.View.Windows
{
    /// <summary>
    /// PreferenceWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PreferenceWindow
    {
        public PreferenceWindow(
            PreferenceViewModel? gameViewModel = null,
            CloudSavedataViewModel? cloudSavedataViewModel = null,
            AboutViewModel? aboutViewModel = null)
        {
            InitializeComponent();

            ViewModel = gameViewModel ?? DependencyInject.GetService<PreferenceViewModel>();
            cloudSavedataViewModel ??= DependencyInject.GetService<CloudSavedataViewModel>();
            aboutViewModel ??= DependencyInject.GetService<AboutViewModel>();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel,
                    vm => vm.PageHeader,
                    v => v.HeaderBlock.Text)
                    .DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.Router,
                    v => v.RoutedViewHost.Router)
                    .DisposeWith(d);

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
                                ViewModel.Router.NavigateAndReset.Execute(cloudSavedataViewModel);
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