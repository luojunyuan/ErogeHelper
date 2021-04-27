using System;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using ErogeHelper.ViewModel.Window;
using ReactiveUI;
using Splat;

namespace ErogeHelper.View.Window
{
    public partial class MainGameWindow : IEnableLogger
    {
        public MainGameWindow()
        {
            InitializeComponent();

            // ViewModel = Locator.Current.GetService<MainGameViewModel>();            
            ViewModel = new MainGameViewModel();

            // https://github.com/reactiveui/ReactiveUI/issues/2395
            this.Log().Debug(
                "Fine exceptions FileNotFoundException reactiveUI is scanning for Drawing, XamForms, Winforms, etc");
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
