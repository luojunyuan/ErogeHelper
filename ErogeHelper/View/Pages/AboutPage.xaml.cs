using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using Color = System.Drawing.Color;

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

            AppVersion.Text = App.EHVersion;

            this.WhenActivated(d =>
            {
                this.WhenAnyValue(x => x.AppVersion.Text)
                    .BindTo(this, x => x.ViewModel!.AppVersion);

                ViewModel!.CheckUpdate.Execute().Subscribe().DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.VersionBrushColor,
                    v => v.VersionBorder.BorderBrush,
                    DrawingColorToBrush,
                    BrushToDrawingColor).DisposeWith(d);
                this.Bind(ViewModel,
                    vm => vm.CanJumpRelease,
                    v => v.VersionBorder.IsEnabled).DisposeWith(d);
                this.Bind(ViewModel,
                    vm => vm.VersionBrushColor,
                    v => v.VersionForeground.Foreground,
                    DrawingColorToBrush,
                    BrushToDrawingColor).DisposeWith(d);
                this.Bind(ViewModel,
                    vm => vm.UpdateStatusTip,
                    v => v.UpdateStatusTip.Text).DisposeWith(d);
                this.Bind(ViewModel,
                    vm => vm.AcceptedPreviewVersion,
                    v => v.PreviewCheckBox.IsChecked).DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.ShowUpdateButton,
                    v => v.UpdateButton.Visibility,
                    value => value ? Visibility.Visible : Visibility.Collapsed,
                    visibility => visibility == Visibility.Visible).DisposeWith(d);
                this.BindCommand(ViewModel,
                    vm => vm.Update,
                    v => v.UpdateButton).DisposeWith(d);
            });
        }

        private Brush DrawingColorToBrush(Color color)
        {
            var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
            if (brush.CanFreeze) 
            {
                brush.Freeze();
            }

            return brush;
        }

        private Color BrushToDrawingColor(Brush brush)
        {
            var color = ((SolidColorBrush)brush).Color;
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
