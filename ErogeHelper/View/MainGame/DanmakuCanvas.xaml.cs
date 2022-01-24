using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using WPFDanmakuLib;

namespace ErogeHelper.View.MainGame;

public partial class DanmakuCanvas
{
    private readonly DanmakuStyle _danmakuStyle = new();
    private WPFDanmakuEngine? _damakuEngine;

    public DanmakuCanvas()
    {
        InitializeComponent();

        var engineBehavior = new EngineBehavior(DrawMode.WPF, CollisionPrevention.Enabled);
        _danmakuStyle.FontSize = 22;
        DanmakuContainer.Loaded += (s, _) =>
            _damakuEngine = new WPFDanmakuEngine(engineBehavior, _danmakuStyle, (Canvas)s);

        this.WhenActivated(d =>
        {
            this.WhenAnyObservable(x => x.ViewModel!.NewDanmakuTerm)
                .Subscribe(ToastDanmaku)
                .DisposeWith(d);
        });
    }

    private void ToastDanmaku(string text)
    {
        _danmakuStyle.PositionX = DanmakuContainer.ActualWidth;
        _danmakuStyle.OutlineEnabled = false;
        _danmakuStyle.ShadowEnabled = false;
        var duration = DanmakuContainer.ActualWidth / (120 + text.Length);
        _danmakuStyle.Duration = (int)duration;
        // override default danmaku style
        _damakuEngine?.DrawDanmaku(text, _danmakuStyle);
    }

    // Max danmaku length 100, tip when over it and disable button

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        ToastDanmaku(DanmakuContent.Text);
    }
}
