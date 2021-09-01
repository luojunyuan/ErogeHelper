using ErogeHelper.Common;
using ErogeHelper.ViewModel.Windows;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ErogeHelper.View.Windows
{
    /// <summary>
    /// SavedataSyncWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PreferenceWindow
    {
        public PreferenceWindow(PreferenceViewModel? preferenceViewModel = null)
        {
            InitializeComponent();

            ViewModel = preferenceViewModel ?? DependencyInject.GetService<PreferenceViewModel>();

            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel,
                    vm => vm.OpenCloudEditDialog,
                    v => v.CloudEditButton)
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                    vm => vm.CloudSwitchCanBeSet,
                    v => v.CloudSaveSwitch.IsEnabled)
                    .DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.CloudSwitchIsOn,
                    v => v.CloudSaveSwitch.IsOn)
                    .DisposeWith(d);
            });
        }
    }
}
