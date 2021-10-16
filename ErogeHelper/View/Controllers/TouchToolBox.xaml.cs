using ErogeHelper.ViewModel.Controllers;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace ErogeHelper.View.Controllers
{
    /// <summary>
    /// TouchToolBox.xaml 的交互逻辑
    /// </summary>
    public partial class TouchToolBox
    {
        public TouchToolBox()
        {
            InitializeComponent();

            ViewModel = DependencyResolver.GetService<TouchToolBoxViewModel>();

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel,
                   vm => vm.TouchToolBoxVisible,
                   v => v.TouchToolBoxView.Visibility,
                   value => value ? Visibility.Visible : Visibility.Collapsed,
                   visibility => visibility == Visibility.Visible).DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.Esc,
                    v => v.Esc).DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.Ctrl,
                    v => v.Ctrl,
                    nameof(PreviewMouseDown)).DisposeWith(d);
                this.BindCommand(ViewModel,
                    vm => vm.CtrlRelease,
                    v => v.Ctrl,
                    nameof(PreviewMouseUp)).DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.Enter,
                    v => v.Enter,
                    nameof(PreviewMouseDown)).DisposeWith(d);
                this.BindCommand(ViewModel,
                    vm => vm.EnterRelease,
                    v => v.Enter,
                    nameof(PreviewMouseUp)).DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.Space,
                    v => v.Space).DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.PageUp,
                    v => v.PageUp).DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.PageDown,
                    v => v.PageDown).DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.UpArrow,
                    v => v.UpArrow).DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.LeftArrow,
                    v => v.LeftArrow).DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.DownArrow,
                    v => v.DownArrow).DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.RightArrow,
                    v => v.RightArrow).DisposeWith(d);
            });
        }

        private void ControlButton_Click(object sender, RoutedEventArgs e)
        {
            TheButtonBox.Visibility = TheButtonBox.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            ControlButton.Content = ControlButton.Content.ToString() == "<" ? '>' : '<';
        }
    }
}
