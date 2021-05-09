using System;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using ErogeHelper.Common.Constraint;
using ErogeHelper.Common.Extension;
using ErogeHelper.Common.Function;
using ErogeHelper.ViewModel.Window;
using ReactiveUI;
using Splat;

namespace ErogeHelper.View.Window
{
    public partial class MainGameWindow : IEnableLogger
    {
        public MainGameWindow(MainGameViewModel? gameViewModel = null)
        {
            InitializeComponent();

            ViewModel = gameViewModel ?? DependencyInject.GetService<MainGameViewModel>();            

            // https://github.com/reactiveui/ReactiveUI/issues/2395
            this.Log().RxUiWarningTipOnce("Fine exceptions FileNotFoundException " +
                    "reactiveUI is scanning for Drawing, XamForms, Winforms, etc");
            this.WhenActivated(disposableRegistration =>
            {
                this.OneWayBind(ViewModel,
                    vm => vm.ConstString,
                    v => v.ATextBlock.Text).DisposeWith(disposableRegistration);
                this.BindCommand(ViewModel,
                    vm => vm.ButtonCommand,
                    v => v.AButton).DisposeWith(disposableRegistration);
                this.Bind(ViewModel,
                    vm => vm.Text,
                    v => v.BindTextBox.Text).DisposeWith(disposableRegistration);
            });
        }
    }
}
