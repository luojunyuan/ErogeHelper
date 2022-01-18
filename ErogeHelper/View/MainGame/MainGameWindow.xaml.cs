using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ErogeHelper.Platform;
using ErogeHelper.Shared;
using ErogeHelper.ViewModel;
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
        var handle = WpfHelper.GetWpfWindowHandle(this);
        HwndTools.HideWindowInAltTab(handle);
        var keyboardDisposal = DisableWinArrawResizeShotcut(handle);

        ViewModel = DependencyResolver.GetService<MainGameViewModel>();

        this.Events().Loaded
            .Select(_ => handle)
            .InvokeCommand(this, x => x.ViewModel!.Loaded);

        this.WhenActivated(d =>
        {
            keyboardDisposal.DisposeWith(d);

            ViewModel.HideMainWindow
                .RegisterHandler(context => { Hide(); context.SetOutput(Unit.Default); }).DisposeWith(d);

            ViewModel.ShowMainWindow
                .RegisterHandler(context => { Show(); context.SetOutput(Unit.Default); }).DisposeWith(d);

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

            this.OneWayBind(ViewModel,
                vm => vm.ShowEdgeTouchMask,
                v => v.PreventFalseTouchMask.Visibility,
                value => value ? Visibility.Visible : Visibility.Collapsed).DisposeWith(d);
        });
    }

    private void InitializeDpi()
    {
        var dpiOfGameScreen = Screen.FromHandle(HwndTools.GetRealGameHandle()).ScaleFactor;
        State.UpdateDpi(dpiOfGameScreen);
        VisualTreeHelper.SetRootDpi(this, new(dpiOfGameScreen, dpiOfGameScreen));
    }

    private static IDisposable DisableWinArrawResizeShotcut(HWND handle)
    {
        var keyboard = WindowsInput.Capture.Global.Keyboard();
        var winLeftListener = new KeyChordEventSource(keyboard, new(KeyCode.LWin, KeyCode.Left)) { Enabled = true };
        var winUpListener = new KeyChordEventSource(keyboard, new(KeyCode.LWin, KeyCode.Up)) { Enabled = true };
        var winRightListener = new KeyChordEventSource(keyboard, new(KeyCode.LWin, KeyCode.Right)) { Enabled = true };
        var winDownListener = new KeyChordEventSource(keyboard, new(KeyCode.LWin, KeyCode.Down)) { Enabled = true };
        void winArrawDelegate(object? s, KeyChordEventArgs e)
        {
            if (User32.GetForegroundWindow() == handle)
            {
                e.Input.Next_Hook_Enabled = false;
            }
        }
        winLeftListener.Triggered += winArrawDelegate;
        winUpListener.Triggered += winArrawDelegate;
        winRightListener.Triggered += winArrawDelegate;
        winDownListener.Triggered += winArrawDelegate;

        return keyboard;
    }

    private void MainGameWindowOnDpiChanged(object sender, DpiChangedEventArgs e) =>
        State.UpdateDpi(e.NewDpi.DpiScaleX);

    // AssistiveTouch
    private void AssistiveTouchOnClick(object sender, RoutedEventArgs e)
    {
        Touch.Hide();
        TouchMenu.Show();
    }

    private void MainGameWindowOnDeactivated(object sender, EventArgs e)
    {
        TouchMenu.Hide();
        Touch.Show();
    }

    private void TouchMenuOnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is AssistiveTouchMenu)
        {
            MainGameWindowOnDeactivated(sender, e);
        }
    }
}
