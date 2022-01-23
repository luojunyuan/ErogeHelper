using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using ErogeHelper.Platform;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.ViewModel.HookConfig;

namespace ErogeHelper.View.MainGame.AssistiveTouchMenu;

public partial class MenuFunctionPage : Page
{
    private readonly Subject<MenuPageTag> _pageSubject = new();
    public IObservable<MenuPageTag> PageChanged => _pageSubject;

    public MenuFunctionPage()
    {
        InitializeComponent();
        ApplyTransistionInAnimation();
    }

    public void TransistIn(double moveDistance)
    {
        SetCurrentValue(VisibilityProperty, Visibility.Visible);

        GridPanel.Children.Cast<MenuItemControl>().FillBackground(false);

        var backTransform = AnimationTool.LeftOneTransform(moveDistance);
        Back.SetCurrentValue(RenderTransformProperty, backTransform);
        _backMoveAnimation.SetCurrentValue(DoubleAnimation.FromProperty, backTransform.X);

        _transitionInStoryboard.Begin();
    }

    public void TransistOut()
    {
        GridPanel.Children.Cast<MenuItemControl>().FillBackground(false);
        _transitionInStoryboard.SetCurrentValue(Timeline.AutoReverseProperty, true);
        _transitionInStoryboard.Begin();
        _transitionInStoryboard.Seek(TimeSpan.FromMilliseconds(AssistiveTouch.TouchTransformDuration));
    }

    private readonly Storyboard _transitionInStoryboard = new();
    private readonly DoubleAnimation _backMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;

    private void ApplyTransistionInAnimation()
    {
        var pageOpacityAnimation = AnimationTool.FadeInAnimation;
        Storyboard.SetTarget(pageOpacityAnimation, this);
        Storyboard.SetTargetProperty(pageOpacityAnimation, new PropertyPath(OpacityProperty));
        _transitionInStoryboard.Children.Add(pageOpacityAnimation);


        Storyboard.SetTarget(_backMoveAnimation, Back);
        Storyboard.SetTargetProperty(_backMoveAnimation, new PropertyPath(AnimationTool.XProperty));
        _transitionInStoryboard.Children.Add(_backMoveAnimation);

        _transitionInStoryboard.Completed += (_, _) =>
        {
            Back.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            GridPanel.Children.Cast<MenuItemControl>().FillBackground(true);

            if (_transitionInStoryboard.AutoReverse == true)
            {
                _transitionInStoryboard.SetCurrentValue(Timeline.AutoReverseProperty, false);
                SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            }
        };
    }

    private void BackOnClickEvent(object sender, EventArgs e) => _pageSubject.OnNext(MenuPageTag.FunctionBack);

    private void HookConfigOnClickEvent(object sender, EventArgs e) =>
        DI.ShowView<HookViewModel>();
}
