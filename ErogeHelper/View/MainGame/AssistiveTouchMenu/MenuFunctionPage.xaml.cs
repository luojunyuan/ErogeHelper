using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Media.Animation;
using ErogeHelper.Platform;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.ViewModel.CloudSave;
using ErogeHelper.ViewModel.HookConfig;

namespace ErogeHelper.View.MainGame.AssistiveTouchMenu;

public partial class MenuFunctionPage
{
    private readonly Subject<MenuPageTag> _pageSubject = new();
    public IObservable<MenuPageTag> PageChanged => _pageSubject;

    public MenuFunctionPage()
    {
        InitializeComponent();
        ApplyTransitionInAnimation();
    }

    public void TransitIn(double moveDistance)
    {
        SetCurrentValue(VisibilityProperty, Visibility.Visible);

        GridPanel.Children.Cast<IMenuItemBackground>().Fill(false);

        var ttsTransform = AnimationTool.LeftOneTopOneTransform(moveDistance);
        var cloudSaveTransform = AnimationTool.LeftTwoTransform(moveDistance);
        var backTransform = AnimationTool.LeftOneTransform(moveDistance);
        TTS.SetCurrentValue(RenderTransformProperty, ttsTransform);
        CloudSave.SetCurrentValue(RenderTransformProperty, cloudSaveTransform);
        Back.SetCurrentValue(RenderTransformProperty, backTransform);
        _ttsXMoveAnimation.SetCurrentValue(DoubleAnimation.FromProperty, ttsTransform.X);
        _ttsYMoveAnimation.SetCurrentValue(DoubleAnimation.FromProperty, ttsTransform.Y);
        _cloudSaveMoveAnimation.SetCurrentValue(DoubleAnimation.FromProperty, cloudSaveTransform.X);
        _backMoveAnimation.SetCurrentValue(DoubleAnimation.FromProperty, backTransform.X);

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
    private readonly DoubleAnimation _ttsXMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _ttsYMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _cloudSaveMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;
    private readonly DoubleAnimation _backMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;

    private void ApplyTransitionInAnimation()
    {
        var pageOpacityAnimation = AnimationTool.FadeInAnimation;
        Storyboard.SetTarget(pageOpacityAnimation, this);
        Storyboard.SetTargetProperty(pageOpacityAnimation, new PropertyPath(OpacityProperty));
        pageOpacityAnimation.Freeze();
        _transitionInStoryboard.Children.Add(pageOpacityAnimation);


        Storyboard.SetTarget(_ttsXMoveAnimation, TTS);
        Storyboard.SetTargetProperty(_ttsXMoveAnimation, new PropertyPath(AnimationTool.XProperty));
        _transitionInStoryboard.Children.Add(_ttsXMoveAnimation);
        Storyboard.SetTarget(_ttsYMoveAnimation, TTS);
        Storyboard.SetTargetProperty(_ttsYMoveAnimation, new PropertyPath(AnimationTool.YProperty));
        _transitionInStoryboard.Children.Add(_ttsYMoveAnimation);
        Storyboard.SetTarget(_cloudSaveMoveAnimation, CloudSave);
        Storyboard.SetTargetProperty(_cloudSaveMoveAnimation, new PropertyPath(AnimationTool.XProperty));
        _transitionInStoryboard.Children.Add(_cloudSaveMoveAnimation);
        Storyboard.SetTarget(_backMoveAnimation, Back);
        Storyboard.SetTargetProperty(_backMoveAnimation, new PropertyPath(AnimationTool.XProperty));
        _transitionInStoryboard.Children.Add(_backMoveAnimation);

        _transitionInStoryboard.Completed += (_, _) =>
        {
            TTS.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            CloudSave.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            Back.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            GridPanel.Children.Cast<IMenuItemBackground>().Fill(true);

            if (_transitionInStoryboard.AutoReverse)
            {
                _transitionInStoryboard.SetCurrentValue(Timeline.AutoReverseProperty, false);
                SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            }
        };
    }

    private void BackOnClickEvent(object sender, EventArgs e) => _pageSubject.OnNext(MenuPageTag.FunctionBack);

    private void HookConfigOnClickEvent(object sender, EventArgs e) => DI.ShowView<HookViewModel>();

    private void CloudSaveOnClickEvent(object sender, EventArgs e) => DI.ShowView<CloudSaveViewModel>();

    private void TTSOnClickEvent(object sender, EventArgs e)
    {

    }
}
