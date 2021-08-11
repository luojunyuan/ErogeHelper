using ErogeHelper.Common;
using ErogeHelper.ViewModel.Windows;
using ReactiveUI;
using Splat;
using System;
using System.Reactive.Disposables;
using System.Windows;

namespace ErogeHelper.View.Windows
{
    /// <summary>
    /// SelectProcessWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SelectProcessWindow : IEnableLogger
    {
        public SelectProcessWindow(SelectProcessViewModel? selectProcessViewModel = null)
        {
            InitializeComponent();

            ViewModel = selectProcessViewModel ?? DependencyInject.GetService<SelectProcessViewModel>();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel,
                    vm => vm.ProcessComboBoxItems,
                    v => v.ProcessComboBox.ItemsSource)
                    .DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.SelectedProcessItem,
                    v => v.ProcessComboBox.SelectedItem)
                    .DisposeWith(d); ;

                this.BindCommand(ViewModel,
                    vm => vm.FilterProcess,
                    v => v.ProcessComboBox,
                    nameof(ProcessComboBox.DropDownOpened))
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                    vm => vm.ShowTipSymbol,
                    v => v.TipSymbol.Visibility)
                    .DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.Inject,
                    v => v.InjectButton)
                    .DisposeWith(d);

                this.Events().Closing
                    .Subscribe(_ => App.Terminate())
                    .DisposeWith(d);

                ViewModel.CloseWindow
                    .Subscribe(_ => Close())
                    .DisposeWith(d);

                ViewModel.HideWindow
                    .Subscribe(_ => Hide())
                    .DisposeWith(d);
            });
        }
    }
}
