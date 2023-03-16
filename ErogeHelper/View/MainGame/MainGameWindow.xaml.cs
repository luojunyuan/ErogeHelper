using System.Reactive;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ErogeHelper.Common.Extensions;
using ErogeHelper.Function;
using ErogeHelper.Function.NativeHelper;
using ErogeHelper.Function.WpfExtend;
using ErogeHelper.ViewModel.MainGame;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using Splat;
using Vanara.PInvoke;
using WindowsInput.Events;
using WindowsInput.Events.Sources;
using WpfScreenHelper;

namespace ErogeHelper.View.MainGame;

public partial class MainGameWindow : IEnableLogger
{
    public MainGameWindow()
    {
        InitializeComponent();
        InitializeDpi();
        var handle = State.MainGameWindowHandle = WpfHelper.GetWpfWindowHandle(this);
        HwndTools.HideWindowInAltTab(handle);

        ViewModel = new MainGameViewModel();

        this.Events().Loaded.ToUnit().InvokeCommand(this, x => x.ViewModel!.Loaded);

        this.WhenActivated(d =>
        {
            Disposable.Create(() => this.Log().Debug("MainWindow UnActivated")).DisposeWith(d);
            DisableWinArrowResizeShotcut(handle).DisposeWith(d);

            ViewModel.HideMainWindow
                .RegisterHandler(context => { Hide(); context.SetOutput(Unit.Default); })
                .DisposeWith(d);

            ViewModel.ShowMainWindow
                .RegisterHandler(context => { Show(); context.SetOutput(Unit.Default); })
                .DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.Height,
                v => v.Height).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.Width,
                v => v.Width).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.Left,
                v => v.Left).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.Top,
                v => v.Top).DisposeWith(d);
        });

        ViewModel.AddEdgeMask
            .RegisterHandler(context =>
            {
                if (context.Input)
                {
                    ((System.Windows.Controls.Grid)Content).Children.Add(EdgeMask);
                    EdgeMask.BeginAnimation(OpacityProperty, FadeInAnimation);
                }
                else ((System.Windows.Controls.Grid)Content).Children.Remove(EdgeMask);

                context.SetOutput(Unit.Default);
            });
    }

    private readonly DoubleAnimation FadeInAnimation = new()
    {
        To = 1.0,
        Duration = TimeSpan.FromSeconds(1.5),
        FillBehavior = FillBehavior.Stop,
        AutoReverse = true
    };
    private readonly System.Windows.Controls.Border EdgeMask = new() 
    {
        BorderBrush = Brushes.Red, 
        BorderThickness = new(5), 
        Opacity = 0.002
    };

    private void InitializeDpi()
    {
        var dpiOfGameScreen = Screen.FromHandle(State.GameRealWindowHandle).ScaleFactor;
        State.UpdateDpi(dpiOfGameScreen);
        VisualTreeHelper.SetRootDpi(this, new(dpiOfGameScreen, dpiOfGameScreen));
    }

    private void MainGameWindowOnDpiChanged(object sender, DpiChangedEventArgs e) =>
        State.UpdateDpi(e.NewDpi.DpiScaleX);

    private static IDisposable DisableWinArrowResizeShotcut(nint handle)
    {
        // Thread name MessagePumpingObject
        var keyboard = WindowsInput.Capture.Global.KeyboardAsync();
        var winLeftListener = new KeyChordEventSource(keyboard, new(KeyCode.LWin, KeyCode.Left)) { Enabled = true };
        var winUpListener = new KeyChordEventSource(keyboard, new(KeyCode.LWin, KeyCode.Up)) { Enabled = true };
        var winRightListener = new KeyChordEventSource(keyboard, new(KeyCode.LWin, KeyCode.Right)) { Enabled = true };
        var winDownListener = new KeyChordEventSource(keyboard, new(KeyCode.LWin, KeyCode.Down)) { Enabled = true };
        var altF4Listener = new KeyChordEventSource(keyboard, new(KeyCode.Alt, KeyCode.F4)) { Enabled = true };
        void WinArrowDelegate(object? s, KeyChordEventArgs e)
        {
            if (User32.GetForegroundWindow() == handle)
            {
                e.Input.Next_Hook_Enabled = false;
            }
        }
        winLeftListener.Triggered += WinArrowDelegate;
        winUpListener.Triggered += WinArrowDelegate;
        winRightListener.Triggered += WinArrowDelegate;
        winDownListener.Triggered += WinArrowDelegate;
        altF4Listener.Triggered += WinArrowDelegate;

        return keyboard;
    }

    #region Disable Touch White Point 

    protected override void OnPreviewTouchDown(TouchEventArgs e)
    {
        base.OnPreviewTouchDown(e);
        Cursor = Cursors.None;
    }

    protected override void OnPreviewTouchMove(TouchEventArgs e)
    {
        base.OnPreviewTouchMove(e);
        Cursor = Cursors.None;
    }

    protected override void OnGotMouseCapture(MouseEventArgs e)
    {
        base.OnGotMouseCapture(e);
        Cursor = Cursors.Arrow;
    }

    protected override void OnPreviewMouseMove(MouseEventArgs e)
    {
        base.OnPreviewMouseMove(e);

        if (e.StylusDevice == null)
            Cursor = Cursors.Arrow;
    }

    #endregion
}
