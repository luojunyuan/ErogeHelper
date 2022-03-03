using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Media.Animation;
using ErogeHelper.Platform.MISC;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.View.Keyboard;
using ErogeHelper.ViewModel.MainGame.AssistiveTouchMenu;
using ModernWpf.Controls;
using ReactiveUI;
using Vanara.PInvoke;
using WindowsInput.Events;

namespace ErogeHelper.View.MainGame.AssistiveTouchMenu;

public partial class MenuGamePage : IViewFor<MenuGameViewModel>
{
    #region ViewModel DependencyProperty
    /// <summary>Identifies the <see cref="ViewModel"/> dependency property.</summary>
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(MenuGameViewModel),
        typeof(MenuGamePage));

    public MenuGameViewModel? ViewModel
    {
        get => (MenuGameViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => SetValue(ViewModelProperty, (MenuGameViewModel?)value);
    }
    #endregion

    private readonly Subject<MenuPageTag> _pageSubject = new();
    public IObservable<MenuPageTag> PageChanged => _pageSubject;

    public MenuGamePage()
    {
        InitializeComponent();
        ApplyTransitionInAnimation();

        if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            return;

        ViewModel = DependencyResolver.GetService<MenuGameViewModel>();
        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel,
                vm => vm.SwitchFullScreenIcon,
                v => v.FullScreenSwitcher.Symbol,
                iconEnum => iconEnum switch
                {
                    SymbolName.BackToWindow => Symbol.BackToWindow,
                    SymbolName.FullScreen => Symbol.FullScreen,
                    _ => throw new InvalidCastException(),
                }).DisposeWith(d);
            this.OneWayBind(ViewModel,
                vm => vm.SwitchFullScreenText,
                v => v.FullScreenSwitcher.Text).DisposeWith(d);

            this.BindCommand(ViewModel,
                vm => vm.CloseGame,
                v => v.CloseGame,
                nameof(CloseGame.ClickEvent)).DisposeWith(d);

            this.Bind(ViewModel,
               vm => vm.LoseFocusEnable,
               v => v.LoseFocus.IsOn).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.TouchToMouseEnable,
                v => v.TouchToMouse.IsOn).DisposeWith(d);
        });
    }

    public void TransitIn(double moveDistance)
    {
        SetCurrentValue(VisibilityProperty, Visibility.Visible);

        GridPanel.Children.Cast<IMenuItemBackground>().Fill(false);

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

    public void TransitOut()
    {
        GridPanel.Children.Cast<IMenuItemBackground>().Fill(false);
        _transitionInStoryboard.SetCurrentValue(Timeline.AutoReverseProperty, true);
        _transitionInStoryboard.Begin();
        _transitionInStoryboard.Seek(TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration));
    }

    private readonly Storyboard _transitionInStoryboard = new();
    private readonly DoubleAnimation _fullscreenMoveXAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _fullscreenMoveYAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _backMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _closeGameMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _loseFocusMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _touchToMouseMoveXAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _touchToMouseMoveYAnimation = AnimationTool.TransformMoveToZeroAnimation;

    private void ApplyTransitionInAnimation()
    {
        var pageOpacityAnimation = AnimationTool.FadeInAnimation;
        Storyboard.SetTarget(pageOpacityAnimation, this);
        Storyboard.SetTargetProperty(pageOpacityAnimation, new PropertyPath(OpacityProperty)); // TODO: OpacityProperty.Name ?
        pageOpacityAnimation.Freeze();
        _transitionInStoryboard.Children.Add(pageOpacityAnimation);


        Storyboard.SetTarget(_fullscreenMoveXAnimation, FullScreenSwitcher);
        Storyboard.SetTargetProperty(_fullscreenMoveXAnimation, new PropertyPath(AnimationTool.XProperty));
        _transitionInStoryboard.Children.Add(_fullscreenMoveXAnimation);
        Storyboard.SetTarget(_fullscreenMoveYAnimation, FullScreenSwitcher);
        Storyboard.SetTargetProperty(_fullscreenMoveYAnimation, new PropertyPath(AnimationTool.YProperty));
        _transitionInStoryboard.Children.Add(_fullscreenMoveYAnimation);

        Storyboard.SetTarget(_backMoveAnimation, Back);
        Storyboard.SetTargetProperty(_backMoveAnimation, new PropertyPath(AnimationTool.XProperty));
        _transitionInStoryboard.Children.Add(_backMoveAnimation);

        Storyboard.SetTarget(_closeGameMoveAnimation, CloseGame);
        Storyboard.SetTargetProperty(_closeGameMoveAnimation, new PropertyPath(AnimationTool.XProperty));
        _transitionInStoryboard.Children.Add(_closeGameMoveAnimation);

        Storyboard.SetTarget(_loseFocusMoveAnimation, LoseFocus);
        Storyboard.SetTargetProperty(_loseFocusMoveAnimation, new PropertyPath(AnimationTool.YProperty));
        _transitionInStoryboard.Children.Add(_loseFocusMoveAnimation);

        Storyboard.SetTarget(_touchToMouseMoveXAnimation, TouchToMouse);
        Storyboard.SetTargetProperty(_touchToMouseMoveXAnimation, new PropertyPath(AnimationTool.XProperty));
        _transitionInStoryboard.Children.Add(_touchToMouseMoveXAnimation);
        Storyboard.SetTarget(_touchToMouseMoveYAnimation, TouchToMouse);
        Storyboard.SetTargetProperty(_touchToMouseMoveYAnimation, new PropertyPath(AnimationTool.YProperty));
        _transitionInStoryboard.Children.Add(_touchToMouseMoveYAnimation);

        _transitionInStoryboard.Completed += (_, _) =>
        {
            FullScreenSwitcher.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            Back.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            CloseGame.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            LoseFocus.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            TouchToMouse.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            GridPanel.Children.Cast<IMenuItemBackground>().Fill(true);

            if (_transitionInStoryboard.AutoReverse)
            {
                _transitionInStoryboard.SetCurrentValue(Timeline.AutoReverseProperty, false);
                SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            }
        };
    }

    private async void FullScreenSwitcherOnClickEvent(object sender, EventArgs e)
    {
        ViewModel!.SwitchFullScreen.Execute().Subscribe();
        await WindowsInput.Simulate.Events()
            .Hold(KeyCode.Alt)
            .Hold(KeyCode.Enter)
            .Wait(ConstantValue.UIMinimumResponseTime)
            .Release(KeyCode.Enter)
            .Release(KeyCode.Alt)
            .Invoke().ConfigureAwait(false);
    }

    private void BackOnClickEvent(object sender, EventArgs e) => _pageSubject.OnNext(MenuPageTag.GameBack);

    private void VirtualKeyboardOnClickEvent(object sender, EventArgs e)
    {
        foreach(var window in Application.Current.Windows)
        {
            if (window is VirtualKeyboardWindow keyboardWindow)
            {
                User32.BringWindowToTop(WpfHelper.GetWpfWindowHandle(keyboardWindow));
                return;
            }
        }

        new VirtualKeyboardWindow().Show();
    }
}
