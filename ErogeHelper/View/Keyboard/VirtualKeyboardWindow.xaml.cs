using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Input;
using ErogeHelper.Function;
using ErogeHelper.Function.NativeHelper;
using ErogeHelper.Function.WpfExtend;
using ReactiveUI;
using Vanara.PInvoke;
using WindowsInput.Events;

namespace ErogeHelper.View.Keyboard;

public partial class VirtualKeyboardWindow
{
    public nint Handle { get; private set; }

    public VirtualKeyboardWindow()
    {
        InitializeComponent();
        Handle = WpfHelper.GetWpfWindowHandle(this);
        HwndTools.HideWindowInAltTab(Handle);
        HwndTools.WindowLostFocus(Handle, true);
        var disposable = new CompositeDisposable();
        // View -> Model
        DependencyResolver.GetService<Model.Services.IGameWindowHooker>().BringKeyboardWindowTopDataFlow
            .Subscribe(_ => User32.BringWindowToTop(Handle))
            .DisposeWith(disposable);

        // PerMonitorV2 issue: May disapear at second screen
        var mainWindow = Application.Current.MainWindow;
        mainWindow.WhenAnyValue(x => x.Left)
            .BindTo(this, x => x.Left)
            .DisposeWith(disposable);
        mainWindow.WhenAnyValue(x => x.Top)
            .BindTo(this, x => x.Top)
            .DisposeWith(disposable);
        mainWindow.WhenAnyValue(x => x.Width)
            .BindTo(this, x => x.Width)
            .DisposeWith(disposable);
        mainWindow.WhenAnyValue(x => x.Height)
            .BindTo(this, x => x.Height)
            .DisposeWith(disposable);
        Closed += (_, _) => disposable.Dispose();
    }

    private void PanelControlButtonOnClick(object sender, RoutedEventArgs e)
    {
        TheButtonBox.SetCurrentValue(VisibilityProperty,
            TheButtonBox.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible);
        PanelControlButton.SetCurrentValue(ContentProperty, PanelControlButton.Content.ToString() == "<" ? '>' : '<');
    }

    private const int UserTimerMinimum = 0x0000000A;
    private static EventBuilder PressKeyWithDelay(KeyCode key) =>
        WindowsInput.Simulate.Events()
            .Hold(key)
            .Wait(UserTimerMinimum)
            .Release(key);

    private async void Esc(object sender, RoutedEventArgs e) =>
        await PressKeyWithDelay(KeyCode.Escape).Invoke().ConfigureAwait(false);

    private async void Ctrl(object sender, MouseButtonEventArgs e) =>
        await WindowsInput.Simulate.Events().Hold(KeyCode.Control).Invoke().ConfigureAwait(false);

    private async void CtrlRelease(object sender, MouseButtonEventArgs e) =>
        await WindowsInput.Simulate.Events().Release(KeyCode.Control).Invoke().ConfigureAwait(false);

    private async void Enter(object sender, RoutedEventArgs e) =>
        await PressKeyWithDelay(KeyCode.Enter).Invoke().ConfigureAwait(false);

    private async void Space(object sender, RoutedEventArgs e) =>
        await PressKeyWithDelay(KeyCode.Space).Invoke().ConfigureAwait(false);

    private async void PageUp(object sender, RoutedEventArgs e) =>
        await PressKeyWithDelay(KeyCode.PageUp).Invoke().ConfigureAwait(false);

    private async void PageDown(object sender, RoutedEventArgs e) =>
        await PressKeyWithDelay(KeyCode.PageDown).Invoke().ConfigureAwait(false);

    private async void UpArrow(object sender, RoutedEventArgs e) =>
        await PressKeyWithDelay(KeyCode.Up).Invoke().ConfigureAwait(false);

    private async void LeftArrow(object sender, RoutedEventArgs e) =>
        await PressKeyWithDelay(KeyCode.Left).Invoke().ConfigureAwait(false);

    private async void DownArrow(object sender, RoutedEventArgs e) =>
        await PressKeyWithDelay(KeyCode.Down).Invoke().ConfigureAwait(false);

    private async void RightArrow(object sender, RoutedEventArgs e) =>
        await PressKeyWithDelay(KeyCode.Right).Invoke().ConfigureAwait(false);

    private async void ScrollUpOnClick(object sender, RoutedEventArgs e)
    {
        ScrollUp.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
        ScrollDown.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
        // Certain focus by mouse position
        await WindowsInput.Simulate.Events()
            .Wait(EHContext.UIMinimumResponseTime)
            .Scroll(ButtonCode.VScroll, ButtonScrollDirection.Up)
            .Wait(EHContext.UIMinimumResponseTime)
            .Invoke().ConfigureAwait(true);
        ScrollUp.SetCurrentValue(VisibilityProperty, Visibility.Visible);
        ScrollDown.SetCurrentValue(VisibilityProperty, Visibility.Visible);
    }

    private async void ScrollDownOnClick(object sender, RoutedEventArgs e)
    {
        ScrollDown.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
        await WindowsInput.Simulate.Events()
            .Wait(EHContext.UIMinimumResponseTime)
            .Scroll(ButtonCode.VScroll, ButtonScrollDirection.Down)
            .Wait(EHContext.UIMinimumResponseTime)
            .Invoke().ConfigureAwait(true);
        ScrollDown.SetCurrentValue(VisibilityProperty, Visibility.Visible);
    }

    private void CloseKeyboardButtonOnClick(object sender, RoutedEventArgs e) => Close();
}
