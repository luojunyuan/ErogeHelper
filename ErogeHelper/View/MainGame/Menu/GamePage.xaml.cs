using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ErogeHelper.Common.Definitions;
using ErogeHelper.Common.Languages;
using ErogeHelper.Function;
using ErogeHelper.Function.NativeHelper;
using ErogeHelper.Function.WpfComponent;
using ErogeHelper.Function.WpfExtend;
using ErogeHelper.View.Keyboard;
using ErogeHelper.ViewModel.MainGame;
using ReactiveUI;
using Vanara.PInvoke;
using WindowsInput.Events;

namespace ErogeHelper.View.MainGame.Menu;

public partial class GamePage : IViewFor<AssistiveTouchViewModel>, ITouchMenuPage
{
    private readonly Subject<TouchMenuPageTag> _pageSubject = new();
    public IObservable<TouchMenuPageTag> PageChanged => _pageSubject;

    #region ViewModel DependencyProperty
    /// <summary>Identifies the <see cref="ViewModel"/> dependency property.</summary>
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(AssistiveTouchViewModel),
        typeof(GamePage));

    public AssistiveTouchViewModel? ViewModel
    {
        get => (AssistiveTouchViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => SetValue(ViewModelProperty, (AssistiveTouchViewModel?)value);
    }
    #endregion
    public GamePage()
    {
        InitializeComponent();
        InitializeAnimation();

        State.GameFullscreenChanged
            .Select(isFullscreen => isFullscreen ? 
                (Symbol.BackToWindow, Strings.AssistiveTouch_Window) : 
                (Symbol.FullScreen, Strings.AssistiveTouch_FullScreen))
            .Subscribe(p => (FullScreenSwitcher.Symbol, FullScreenSwitcher.Text) = p);

        ViewModel = ((MainGameWindow)Application.Current.MainWindow).Touch.ViewModel;

        this.WhenActivated(_ =>
        {
            this.Bind(ViewModel,
               vm => vm.LoseFocusEnable,
               v => v.LoseFocus.IsOn);

            this.Bind(ViewModel,
                vm => vm.TouchToMouseEnable,
                v => v.TouchToMouse.IsOn);
        });
    }

    public void Show(double moveDistance)
    {
        SetCurrentValue(VisibilityProperty, Visibility.Visible);

        XamlResource.SetAssistiveTouchItemBackground(Brushes.Transparent);

        var fullscreenTransform = AnimationTool.RightOneTopOneTransform(moveDistance);
        var backTransform = AnimationTool.RightOneTransform(moveDistance);
        var closeGameTransform = AnimationTool.RightTwoTransform(moveDistance);
        var loseFocusTransform = AnimationTool.BottomOneTransform(moveDistance);
        var touchToMouseTransform = AnimationTool.RightOneBottomOneTransform(moveDistance);

        FullScreenSwitcher.SetCurrentValue(RenderTransformProperty, fullscreenTransform);
        Back.SetCurrentValue(RenderTransformProperty, backTransform);
        CloseGame.SetCurrentValue(RenderTransformProperty, closeGameTransform);
        LoseFocus.SetCurrentValue(RenderTransformProperty, loseFocusTransform);
        TouchToMouse.SetCurrentValue(RenderTransformProperty, touchToMouseTransform);

        _fullscreenMoveXAnimation.SetCurrentValue(DoubleAnimation.FromProperty, fullscreenTransform.X);
        _fullscreenMoveYAnimation.SetCurrentValue(DoubleAnimation.FromProperty, fullscreenTransform.Y);
        _backMoveAnimation.SetCurrentValue(DoubleAnimation.FromProperty, backTransform.X);
        _closeGameMoveAnimation.SetCurrentValue(DoubleAnimation.FromProperty, closeGameTransform.X);
        _loseFocusMoveAnimation.SetCurrentValue(DoubleAnimation.FromProperty, loseFocusTransform.Y);
        _touchToMouseMoveXAnimation.SetCurrentValue(DoubleAnimation.FromProperty, touchToMouseTransform.X);
        _touchToMouseMoveYAnimation.SetCurrentValue(DoubleAnimation.FromProperty, touchToMouseTransform.Y);

        _transitionInStoryboard.Begin();
    }

    public void Close()
    {
        XamlResource.SetAssistiveTouchItemBackground(Brushes.Transparent);
        _transitionInStoryboard.SetCurrentValue(Timeline.AutoReverseProperty, true);
        _transitionInStoryboard.Begin();
        _transitionInStoryboard.Seek(AssistiveMenu.MenuTransistDuration);
    }

    private readonly Storyboard _transitionInStoryboard = new();
    private readonly DoubleAnimation _fullscreenMoveXAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _fullscreenMoveYAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _backMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _closeGameMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _loseFocusMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _touchToMouseMoveXAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _touchToMouseMoveYAnimation = AnimationTool.TransformMoveToZeroAnimation;

    private void InitializeAnimation()
    {
        AnimationTool.BindingAnimation(_transitionInStoryboard, AnimationTool.FadeInAnimation, this, new(OpacityProperty), true);

        AnimationTool.BindingAnimation(_transitionInStoryboard, _fullscreenMoveXAnimation, FullScreenSwitcher, AnimationTool.XProperty);
        AnimationTool.BindingAnimation(_transitionInStoryboard, _fullscreenMoveYAnimation, FullScreenSwitcher, AnimationTool.YProperty);
        AnimationTool.BindingAnimation(_transitionInStoryboard, _backMoveAnimation, Back, AnimationTool.XProperty);
        AnimationTool.BindingAnimation(_transitionInStoryboard, _closeGameMoveAnimation, CloseGame, AnimationTool.XProperty);
        AnimationTool.BindingAnimation(_transitionInStoryboard, _loseFocusMoveAnimation, LoseFocus, AnimationTool.YProperty);
        AnimationTool.BindingAnimation(_transitionInStoryboard, _touchToMouseMoveXAnimation, TouchToMouse, AnimationTool.XProperty);
        AnimationTool.BindingAnimation(_transitionInStoryboard, _touchToMouseMoveYAnimation, TouchToMouse, AnimationTool.YProperty);

        _transitionInStoryboard.Completed += (_, _) =>
        {
            XamlResource.SetAssistiveTouchItemBackground(XamlResource.AssistiveTouchBackground);

            if (!_transitionInStoryboard.AutoReverse)
            {
                FullScreenSwitcher.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
                Back.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
                CloseGame.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
                LoseFocus.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
                TouchToMouse.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            }
            else
            {
                _transitionInStoryboard.SetCurrentValue(Timeline.AutoReverseProperty, false);
                SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
                TouchMenuItem.ClickLocked = false;
            }
        };
    }

    private async void FullScreenSwitcherOnClickEvent(object sender, EventArgs e)
    {
        if (!State.IsFullscreen) HwndTools.WindowLostFocus(State.MainGameWindowHandle, true);
        User32.BringWindowToTop(State.MainProcess.MainWindowHandle);
        await WindowsInput.Simulate.Events()
            .Hold(KeyCode.Alt)
            .Hold(KeyCode.Enter)
            .Wait(EHContext.UIMinimumResponseTime)
            .Release(KeyCode.Enter)
            .Release(KeyCode.Alt)
            .Invoke().ConfigureAwait(false);
    }

    private void BackOnClick(object sender, EventArgs e) => _pageSubject.OnNext(TouchMenuPageTag.GameBack);

    private void VirtualKeyboardOnClickEvent(object sender, EventArgs e)
    {
        foreach (var window in Application.Current.Windows)
        {
            if (window is VirtualKeyboardWindow keyboardWindow)
            {
                User32.BringWindowToTop(keyboardWindow.Handle);
                return;
            }
        }

        new VirtualKeyboardWindow().Show();
    }

    private void CloseGameOnClick(object sender, EventArgs e)
    {
        User32.PostMessage(
            State.MainProcess.MainWindowHandle,
            (uint)User32.WindowMessage.WM_SYSCOMMAND,
            (IntPtr)User32.SysCommand.SC_CLOSE);
        User32.BringWindowToTop(State.MainProcess.MainWindowHandle);
    }
}
