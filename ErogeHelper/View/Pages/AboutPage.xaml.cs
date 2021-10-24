using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;

namespace ErogeHelper.View.Pages
{
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage
    {
        public AboutPage()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                ViewModel!.CheckUpdateCommand.Execute();

                this.WhenAnyObservable(x => x.ViewModel!.TerminateAppSubj)
                   .Subscribe(_ =>
                   {
                       Application.Current.Windows
                           .Cast<Window>().ToList()
                           .ForEach(w => w.Close());
                       App.Terminate();
                   }).DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.VersionBrushColor,
                    v => v.VersionBorder.BorderBrush).DisposeWith(d);
                this.Bind(ViewModel,
                    vm => vm.CanJumpRelease,
                    v => v.VersionBorder.IsEnabled).DisposeWith(d);
                this.Bind(ViewModel,
                    vm => vm.VersionBrushColor,
                    v => v.VersionForground.Foreground).DisposeWith(d);
                this.Bind(ViewModel,
                    vm => vm.UpdateStatusTip,
                    v => v.UpdateStatusTip.Text).DisposeWith(d);
                this.Bind(ViewModel,
                    vm => vm.AcceptPreviewVersion,
                    v => v.PreviewCheckBox.IsChecked).DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.ShowUpdateButton,
                    v => v.UpdateButton.Visibility,
                    value => value ? Visibility.Visible : Visibility.Collapsed,
                    visibility => visibility == Visibility.Visible).DisposeWith(d);
                this.BindCommand(ViewModel,
                    vm => vm.UpdateCommand,
                    v => v.UpdateButton).DisposeWith(d);
            });
        }
    }
}
