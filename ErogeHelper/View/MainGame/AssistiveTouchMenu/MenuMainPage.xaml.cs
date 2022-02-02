using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using ErogeHelper.Platform;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.ViewModel.Preference;

namespace ErogeHelper.View.MainGame.AssistiveTouchMenu;

public partial class MenuMainPage : Page
{
    private readonly DoubleAnimation _fadeOutAnimation = AnimationTool.FadeOutAnimation;
    private readonly DoubleAnimation _fadeInAnimation = AnimationTool.FadeInAnimation;

    private readonly Subject<MenuPageTag> _pageSubject = new();
    public IObservable<MenuPageTag> PageChanged => _pageSubject;

    public MenuMainPage()
    {
        InitializeComponent();
        _fadeInAnimation.Completed += (_, _) => GridPanel.Children.Cast<IMenuItemBackround>().Fill(true);
        _fadeOutAnimation.Completed += (_, _) =>
        {
            SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            GridPanel.Children.Cast<IMenuItemBackround>().Fill(true);
        };
    }

    public void FadeOut()
    {
        GridPanel.Children.Cast<IMenuItemBackround>().Fill(false);
        BeginAnimation(OpacityProperty, _fadeOutAnimation);
    }

    public void FadeIn()
    {
        GridPanel.Children.Cast<IMenuItemBackround>().Fill(false);
        SetCurrentValue(VisibilityProperty, Visibility.Visible);
        BeginAnimation(OpacityProperty, _fadeInAnimation);
    }

    private void PreferenceOnClickEvent(object sender, EventArgs e) => DI.ShowView<PreferenceViewModel>();

    private void GameOnClickEvent(object sender, EventArgs e) => _pageSubject.OnNext(MenuPageTag.Game);

    private void DeviceOnClickEvent(object sender, EventArgs e) => _pageSubject.OnNext(MenuPageTag.Device);

    private void FunctionOnClickEvent(object sender, EventArgs e) => _pageSubject.OnNext(MenuPageTag.Function);
}
