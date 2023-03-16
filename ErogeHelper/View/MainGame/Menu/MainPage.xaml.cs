using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using ErogeHelper.Common.Definitions;
using ErogeHelper.Common.Languages;
using ErogeHelper.Function.WpfComponent;
using ErogeHelper.Function.WpfExtend;

namespace ErogeHelper.View.MainGame.Menu;

public partial class MainPage : Page, ITouchMenuPage
{
    private readonly DoubleAnimation _fadeOutAnimation = AnimationTool.FadeOutAnimation;
    private readonly DoubleAnimation _fadeInAnimation = AnimationTool.FadeInAnimation;

    private readonly Subject<TouchMenuPageTag> _pageSubject = new();
    public IObservable<TouchMenuPageTag> PageChanged => _pageSubject;

    public MainPage()
    {
        InitializeComponent();
        _fadeOutAnimation.Completed += (_, _) =>
        {
            Visibility = Visibility.Hidden;
            TouchMenuItem.ClickLocked = false;
        };
        _fadeOutAnimation.Freeze();
        _fadeInAnimation.Freeze();
    }

    public void Close() => BeginAnimation(OpacityProperty, _fadeOutAnimation);

    public void Show(double _)
    {
        Visibility = Visibility.Visible;
        BeginAnimation(OpacityProperty, _fadeInAnimation);
    }

    private void PreferenceOnClickEvent(object sender, EventArgs e) => 
        WpfHelper.ShowWindow<Modern.Preference.PreferenceWindow>();

    private void GameOnClick(object sender, EventArgs e) => _pageSubject.OnNext(TouchMenuPageTag.Game);

    private void DeviceOnClick(object sender, EventArgs e) => _pageSubject.OnNext(TouchMenuPageTag.Device);

    private void FunctionOnClick(object sender, EventArgs e) => _pageSubject.OnNext(TouchMenuPageTag.Function);

}
