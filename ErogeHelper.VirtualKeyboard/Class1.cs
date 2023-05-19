using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using WindowsInput.Events;
using System.Drawing.Printing;

namespace ErogeHelper.VirtualKeyboard
{
    class Class1
    {
      
        // Key type
        // button repeat hold disappear

        // THIS IS NEEDED ?
        //class Model
        //{
        //    KeyCode KeyCode { get; }
        //    string Content { get; }
        //    Quadrant Quadrant { get; }
        //    Margins Margins { get; }
        //}
        //enum Quadrant
        //{
        //    First = 1,
        //    Second,
        //    Third,
        //    Fourth,
        //}
        //private void PanelControlButtonOnClick(object sender, RoutedEventArgs e)
        //{
        //    TheButtonBox.SetCurrentValue(VisibilityProperty,
        //        TheButtonBox.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible);
        //    PanelControlButton.SetCurrentValue(ContentProperty, PanelControlButton.Content.ToString() == "<" ? '>' : '<');
        //}

        private const int UserTimerMinimum = 0x0000000A;
        private const int UIMinimumResponseTime = 50;

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

        //private async void ScrollUpOnClick(object sender, RoutedEventArgs e)
        //{
        //    ScrollUp.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
        //    ScrollDown.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
        //    // Certain focus by mouse position
        //    await WindowsInput.Simulate.Events()
        //        .Wait(UIMinimumResponseTime)
        //        .Scroll(ButtonCode.VScroll, ButtonScrollDirection.Up)
        //        .Wait(UIMinimumResponseTime)
        //        .Invoke().ConfigureAwait(true);
        //    ScrollUp.SetCurrentValue(VisibilityProperty, Visibility.Visible);
        //    ScrollDown.SetCurrentValue(VisibilityProperty, Visibility.Visible);
        //}

        //private async void ScrollDownOnClick(object sender, RoutedEventArgs e)
        //{
        //    ScrollDown.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
        //    await WindowsInput.Simulate.Events()
        //        .Wait(UIMinimumResponseTime)
        //        .Scroll(ButtonCode.VScroll, ButtonScrollDirection.Down)
        //        .Wait(UIMinimumResponseTime)
        //        .Invoke().ConfigureAwait(true);
        //    ScrollDown.SetCurrentValue(VisibilityProperty, Visibility.Visible);
        //}

    }
}
