using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ErogeHelper.View.Controllers;

public partial class AssistiveTouchTransformAnimation : UserControl
{
    private const int AnimationDurationTime = 200;

    private readonly Storyboard _transformStoryboard;
    private readonly DoubleAnimation _heightAnimation;
    private readonly DoubleAnimation _widthAnimation;
    private readonly ThicknessAnimation _marginAnimation;
    private readonly DoubleAnimation _whitePointOpacityAnimation;
    private readonly ThicknessAnimation _whitePointMoveAnimation;

    public AssistiveTouchTransformAnimation()
    {
        InitializeComponent();

        _transformStoryboard = new();
        _transformStoryboard.SetValue(Timeline.DesiredFrameRateProperty, 60);
        _heightAnimation = new DoubleAnimation
        {
            Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDurationTime))
        };
        Storyboard.SetTarget(_heightAnimation, AnimatiedBorder);
        Storyboard.SetTargetProperty(_heightAnimation, new PropertyPath(HeightProperty));
        _widthAnimation = new DoubleAnimation
        {
            Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDurationTime))
        };
        Storyboard.SetTarget(_widthAnimation, AnimatiedBorder);
        Storyboard.SetTargetProperty(_widthAnimation, new PropertyPath(WidthProperty));
       
        _marginAnimation = new ThicknessAnimation
        {
            Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDurationTime))
        };
        Storyboard.SetTarget(_marginAnimation, AnimatiedBorder);
        Storyboard.SetTargetProperty(_marginAnimation, new PropertyPath(MarginProperty));
       
        _whitePointOpacityAnimation = new DoubleAnimation
        {
            Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDurationTime))
        };
        Storyboard.SetTarget(_whitePointOpacityAnimation, WhitePoint);
        Storyboard.SetTargetProperty(_whitePointOpacityAnimation, new PropertyPath(OpacityProperty));
        
        _whitePointMoveAnimation = new ThicknessAnimation
        {
            Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDurationTime))
        };
        Storyboard.SetTarget(_whitePointMoveAnimation, WhitePoint);
        Storyboard.SetTargetProperty(_whitePointMoveAnimation, new PropertyPath(MarginProperty));

        _transformStoryboard.Children.Add(_heightAnimation);
        _transformStoryboard.Children.Add(_widthAnimation);
        _transformStoryboard.Children.Add(_marginAnimation);
        _transformStoryboard.Children.Add(_whitePointOpacityAnimation); 
        _transformStoryboard.Children.Add(_whitePointMoveAnimation);

        //var oneOpacityAnimation = new DoubleAnimation
        //{
        //    To = big ? 1 : 0,
        //    Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDurationTime))
        //};
        //Storyboard.SetTarget(oneOpacityAnimation, IconOne);
        //Storyboard.SetTargetProperty(oneOpacityAnimation, new PropertyPath(OpacityProperty));
        //sb.Children.Add(oneOpacityAnimation);
        //var oneMoveAnimation = new ThicknessAnimation
        //{
        //    To = new Thickness(0, 0, 0, value),
        //    Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDurationTime))
        //};
        //Storyboard.SetTarget(oneMoveAnimation, IconOne);
        //Storyboard.SetTargetProperty(oneMoveAnimation, new PropertyPath(MarginProperty));
        //sb.Children.Add(oneMoveAnimation);
    }

    public void BeginAnimation(Thickness menuMargin)
    {
        _heightAnimation.To = 300;
        _widthAnimation.To = 300;
        _marginAnimation.To = menuMargin;
        _whitePointOpacityAnimation.To = 0;
        _whitePointMoveAnimation.To = menuMargin;

        _transformStoryboard.Begin();
    }
}
