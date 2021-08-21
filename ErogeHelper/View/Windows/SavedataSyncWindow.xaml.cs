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
    public partial class SavedataSyncWindow
    {
        public SavedataSyncWindow(SavedataSyncViewModel? savedataSyncViewModel = null)
        {
            InitializeComponent();

            ViewModel = savedataSyncViewModel ?? DependencyInject.GetService<SavedataSyncViewModel>();

            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel,
                    vm => vm.KeyAction,
                    v => v.KeyButton)
                    .DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.SelectCloudPath,
                    v => v.ChooseUNCPath)
                    .DisposeWith(d);
                this.BindCommand(ViewModel,
                    vm => vm.SelectSaveDataPath,
                    v => v.ChooseSaveDataPath)
                    .DisposeWith(d);
                //this.BindCommand(ViewModel,
                //    vm => vm.CloudSwitch,
                //    v => v.CloudSaveCheckBox.IsOn)
                //    .DisposeWith(d);

                //this.OneWayBind(ViewModel,
                //    vm => vm.CloudSwitchCanBeOpen,
                //    v => v.CloudSaveCheckBox.IsEnabled)
                //    .DisposeWith(d);
                this.OneWayBind(ViewModel,
                    vm => vm.CloudPath,
                    v => v.UNCPath.Text)
                    .DisposeWith(d);
                this.OneWayBind(ViewModel,
                    vm => vm.SaveDataPath,
                    v => v.SaveDataPath.Text)
                    .DisposeWith(d);
            });
        }
    }
}
