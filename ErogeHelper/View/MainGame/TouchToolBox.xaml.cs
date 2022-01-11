﻿using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ErogeHelper.Shared.Contracts;
using WindowsInput.Events;

namespace ErogeHelper.View.MainGame;

public partial class TouchToolBox
{
    //<!--  TODO: Use a window instead  -->
    //<!--<maingame:TouchToolBox x:Name="TouchToolBox" Margin="5,50,0,0" Visibility="Collapsed" />-->
    //this.WhenAnyValue(x => x.Menu.LoseFocusEnable, x => x.Menu.TouchBoxEnable, (a, b) => a && b)
    public TouchToolBox()
    {
        InitializeComponent();

        _enterHolder = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(ConstantValue.PressEnterKeyIntervalTime)
        };
        _enterHolder.Tick += async (_, _) =>
            await WindowsInput.Simulate.Events()
                .Click(KeyCode.Enter)
                .Invoke().ConfigureAwait(false);
    }

    private void ControlButton_Click(object sender, RoutedEventArgs e)
    {
        TheButtonBox.SetCurrentValue(VisibilityProperty, TheButtonBox.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible);
        ControlButton.SetCurrentValue(ContentProperty, ControlButton.Content.ToString() == "<" ? '>' : '<');
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

    private readonly DispatcherTimer _enterHolder;

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
            _enterHolder.Start();
        }
    }

    private void EnterRelease(object sender, MouseButtonEventArgs e)
    {
        _enterHolder.Stop();
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
        ScrollUp.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
        ScrollDown.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
        // Certain focus by mouse position
        await WindowsInput.Simulate.Events()
            .Wait(ConstantValue.UIMinimumResponseTime)
            .Scroll(ButtonCode.VScroll, ButtonScrollDirection.Up)
            .Wait(ConstantValue.UIMinimumResponseTime)
            .Invoke().ConfigureAwait(true);
        ScrollUp.SetCurrentValue(VisibilityProperty, Visibility.Visible);
        ScrollDown.SetCurrentValue(VisibilityProperty, Visibility.Visible);
    }

    private async void ScrollDownOnClick(object sender, RoutedEventArgs e)
    {
        ScrollDown.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
        await WindowsInput.Simulate.Events()
            .Wait(ConstantValue.UIMinimumResponseTime)
            .Scroll(ButtonCode.VScroll, ButtonScrollDirection.Down)
            .Wait(ConstantValue.UIMinimumResponseTime)
            .Invoke().ConfigureAwait(true);
        ScrollDown.SetCurrentValue(VisibilityProperty, Visibility.Visible);
    }
}