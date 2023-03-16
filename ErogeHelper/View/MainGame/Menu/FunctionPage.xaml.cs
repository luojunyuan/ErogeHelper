using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ErogeHelper.Common.Definitions;
using ErogeHelper.Function.WpfComponent;
using ErogeHelper.Function.WpfExtend;
using ErogeHelper.ViewModel.MainGame;
using ReactiveUI;
using Splat;

namespace ErogeHelper.View.MainGame.Menu;

public partial class FunctionPage : IViewFor<AssistiveTouchViewModel>, IEnableLogger, ITouchMenuPage
{
    private readonly Subject<TouchMenuPageTag> _pageSubject = new();
    public IObservable<TouchMenuPageTag> PageChanged => _pageSubject;

    #region ViewModel DependencyProperty
    /// <summary>Identifies the <see cref="ViewModel"/> dependency property.</summary>
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(AssistiveTouchViewModel),
        typeof(FunctionPage));

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
    public FunctionPage()
    {
        InitializeComponent();
        InitializeAnimation();

        ViewModel = ((MainGameWindow)Application.Current.MainWindow).Touch.ViewModel;

        this.WhenActivated(_ =>
        {
            this.Bind(ViewModel,
               vm => vm.AttachEnable,
               v => v.Attach.IsOn);
        });
    }

    public void Show(double moveDistance)
    {
        SetCurrentValue(VisibilityProperty, Visibility.Visible);

        XamlResource.SetAssistiveTouchItemBackground(Brushes.Transparent);

        var backTransform = AnimationTool.LeftOneTransform(moveDistance);
        Back.SetCurrentValue(RenderTransformProperty, backTransform);
        _backMoveAnimation.SetCurrentValue(DoubleAnimation.FromProperty, backTransform.X);

        _transitionInStoryboard.Begin();
    }

    public void Close()
    {
        XamlResource.SetAssistiveTouchItemBackground(Brushes.Transparent);
        _transitionInStoryboard.SetCurrentValue(Timeline.AutoReverseProperty, true);
        _transitionInStoryboard.Begin();
        _transitionInStoryboard.Seek(AssistiveMenu.MenuTransistDuration);
    }

    private void BackOnClick(object sender, EventArgs e) => _pageSubject.OnNext(TouchMenuPageTag.FunctionBack);

    private readonly Storyboard _transitionInStoryboard = new();
    private readonly DoubleAnimation _backMoveAnimation = AnimationTool.TransformMoveToZeroAnimation;

    private void InitializeAnimation()
    {
        AnimationTool.BindingAnimation(_transitionInStoryboard, AnimationTool.FadeInAnimation, this, new(OpacityProperty), true);

        AnimationTool.BindingAnimation(_transitionInStoryboard, _backMoveAnimation, Back, AnimationTool.XProperty);

        _transitionInStoryboard.Completed += (_, _) =>
        {
            XamlResource.SetAssistiveTouchItemBackground(XamlResource.AssistiveTouchBackground);

            if (!_transitionInStoryboard.AutoReverse)
            {
                Back.SetCurrentValue(RenderTransformProperty, AnimationTool.ZeroTransform);
            }
            else
            {
                _transitionInStoryboard.SetCurrentValue(Timeline.AutoReverseProperty, false);
                SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
                TouchMenuItem.ClickLocked = false;
            }
        };
    }

    private void HookConfigOnClick(object sender, EventArgs e)
    {
        //WpfHelper.ShowWindow<Modern.HookConfig.HookWindow>();

    }
}
