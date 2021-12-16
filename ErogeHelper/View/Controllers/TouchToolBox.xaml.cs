using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ErogeHelper.Shared.Contracts;
using ReactiveUI;
using WindowsInput.Events;

namespace ErogeHelper.View.Controllers;

public partial class TouchToolBox
{
    public TouchToolBox()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            this.Bind(ViewModel,
               vm => vm.TouchToolBoxVisible,
               v => v.TouchToolBoxView.Visibility,
               value => value ? Visibility.Visible : Visibility.Collapsed,
               visibility => visibility == Visibility.Visible).DisposeWith(d);
        });

        _enterHoder = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(ConstantValue.PressEnterKeyIntervalTime)
        };
        _enterHoder.Tick += async (_, _) =>
            await WindowsInput.Simulate.Events()
                .Click(KeyCode.Enter)
                .Invoke().ConfigureAwait(false);
    }

    private void ControlButton_Click(object sender, RoutedEventArgs e)
    {
        TheButtonBox.Visibility = TheButtonBox.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        ControlButton.Content = ControlButton.Content.ToString() == "<" ? '>' : '<';
    }

    private async void Esc(object sender, RoutedEventArgs e) =>
        await WindowsInput.Simulate.Events()
            .Click(KeyCode.Escape)
            .Invoke().ConfigureAwait(false);

    private async void Ctrl(object sender, MouseButtonEventArgs e) =>
        await WindowsInput.Simulate.Events()
            .Hold(KeyCode.Control)
            .Invoke().ConfigureAwait(false);

    private async void CtrlRelease(object sender, MouseButtonEventArgs e) =>
        await WindowsInput.Simulate.Events()
            .Release(KeyCode.Control)
            .Invoke().ConfigureAwait(false);

    private readonly DispatcherTimer _enterHoder;

    private bool _enterIsHolded = false;

    private async void Enter(object sender, MouseButtonEventArgs e)
    {
        _enterIsHolded = true;
        await WindowsInput.Simulate.Events()
            .Click(KeyCode.Enter)
            .Wait(ConstantValue.PressFirstKeyLagTime)
            .Invoke().ConfigureAwait(false);
        if (_enterIsHolded)
        {
            _enterHoder.Start();
        }
    }

    private void EnterRelease(object sender, MouseButtonEventArgs e)
    {
        _enterHoder.Stop();
        _enterIsHolded = false;
    }

    private async void Space(object sender, RoutedEventArgs e) =>
        await WindowsInput.Simulate.Events()
            .Click(KeyCode.Space)
            .Invoke().ConfigureAwait(false);

    private async void PageUp(object sender, RoutedEventArgs e) =>
        await WindowsInput.Simulate.Events()
            .Click(KeyCode.PageUp)
            .Invoke().ConfigureAwait(false);

    private async void PageDown(object sender, RoutedEventArgs e) =>
        await WindowsInput.Simulate.Events()
            .Click(KeyCode.PageDown)
            .Invoke().ConfigureAwait(false);

    private async void UpArrow(object sender, RoutedEventArgs e) =>
        await WindowsInput.Simulate.Events()
            .Click(KeyCode.Up)
            .Invoke().ConfigureAwait(false);

    private async void LeftArrow(object sender, RoutedEventArgs e) =>
        await WindowsInput.Simulate.Events()
            .Click(KeyCode.Left)
            .Invoke().ConfigureAwait(false);

    private async void DownArrow(object sender, RoutedEventArgs e) =>
        await WindowsInput.Simulate.Events()
            .Click(KeyCode.Down)
            .Invoke().ConfigureAwait(false);

    private async void RightArrow(object sender, RoutedEventArgs e) =>
        await WindowsInput.Simulate.Events()
            .Click(KeyCode.Down)
            .Invoke().ConfigureAwait(false);

    private async void ScrollUpOnClick(object sender, RoutedEventArgs e)
    {
        ScrollUp.Visibility = Visibility.Collapsed;
        ScrollDown.Visibility = Visibility.Collapsed;
        // Certain focus by mouse position
        await WindowsInput.Simulate.Events()
            .Wait(ConstantValue.UIMinimumResponseTime)
            .Scroll(ButtonCode.VScroll, ButtonScrollDirection.Up)
            .Wait(ConstantValue.UIMinimumResponseTime)
            .Invoke().ConfigureAwait(true);
        ScrollUp.Visibility = Visibility.Visible;
        ScrollDown.Visibility = Visibility.Visible;
    }

    private async void ScrollDownOnClick(object sender, RoutedEventArgs e)
    {
        ScrollDown.Visibility = Visibility.Collapsed;
        await WindowsInput.Simulate.Events()
            .Wait(ConstantValue.UIMinimumResponseTime)
            .Scroll(ButtonCode.VScroll, ButtonScrollDirection.Down)
            .Wait(ConstantValue.UIMinimumResponseTime)
            .Invoke().ConfigureAwait(true);
        ScrollDown.Visibility = Visibility.Visible;
    }
}
