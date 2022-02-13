using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.Shared.Enums;
using ErogeHelper.Shared.Extensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.TextDisplay;

public class TextViewModel : ReactiveObject, IEnableLogger, IDisposable
{
    public static HWND TextWindowHandle { get; set; } = IntPtr.Zero;

    /// <summary>
    /// 好像没啥用了
    /// </summary>
    private string _currentText = "TODO: 如果设置了钩子就是Waiting for text，如果没有就是 暂未设置Hook，请先设置钩子，并打开原文或一个翻译器";

    public TextViewModel(
        ITextractorService? textractorService = null,
        IEHConfigRepository? ehConfigRepository = null,
        IGameDataService? gameDataService = null,
        IGameWindowHooker? gameWindowHooker = null,
        IMeCabService? mecabService = null,
        IGameInfoRepository? gameInfoRepository = null)
    {
        textractorService ??= DependencyResolver.GetService<ITextractorService>();
        ehConfigRepository ??= DependencyResolver.GetService<IEHConfigRepository>();
        gameDataService ??= DependencyResolver.GetService<IGameDataService>();
        gameWindowHooker ??= DependencyResolver.GetService<IGameWindowHooker>();
        mecabService ??= DependencyResolver.GetService<IMeCabService>();
        gameInfoRepository ??= DependencyResolver.GetService<IGameInfoRepository>();

        WindowWidth = ehConfigRepository.TextWindowWidth;
        WindowOpacity = ehConfigRepository.TextWindowOpacity;
        EnableBlurBackground = ehConfigRepository.TextWindowBlur;
        _furiganaItemViewModel = new ObservableCollection<FuriganaItemViewModel>();

        WindowStatusChangeSubscription(ehConfigRepository, gameWindowHooker).DisposeWith(_disposables);

        Loaded = ReactiveCommand.Create(() =>
        {
            HwndTools.WindowBlur(TextWindowHandle, ehConfigRepository.TextWindowBlur);
            HwndTools.WindowLostFocus(TextWindowHandle, gameInfoRepository.GameInfo.IsLoseFocus);
            MoveToGameCenter(gameWindowHooker, WindowWidth, State.Dpi);
        }).DisposeWith(_disposables);

        gameWindowHooker.GamePosChanged
            .Subscribe(pos =>
            {
                Left += pos.HorizontalChange / State.Dpi;
                Top += pos.VerticalChange / State.Dpi;
            }).DisposeWith(_disposables);

        textractorService.SelectedData
            .Where(_ => ehConfigRepository.EnableMeCab)
            .Select(hp => _currentText = hp.Text)
            .Select(sentence => mecabService
                .GenerateMeCabWords(sentence).Select(item => new FuriganaItemViewModel()
                {
                    Text = item.Word,
                    Kana = item.Kana,
                    FontSize = ehConfigRepository.FontSize,
                    BackgroundColor = item.PartOfSpeech.ToColor(),
                }).ToList())
            .Do(_ => User32.BringWindowToTop(TextWindowHandle))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(UpdateOrAddFuriganaItems).DisposeWith(_disposables);

        // Relocate position
        // TODO: Massive work Relocating TextWindow position when dpi changed (or move between monitors)
        gameDataService.GameFullscreenChanged
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(isFullscreen =>
            {
                var gamePos = gameWindowHooker.InvokeUpdatePosition();
                // Use gamePos and (Left, Top) WindowWidth to prepare where textwindow should be
                // this.Log().Debug($"{Left} {Top} {WindowWidth}");
                // this.Log().Debug(gamePos);
                if (isFullscreen)
                {
                    User32.BringWindowToTop(TextWindowHandle);
                }
                else
                {

                }
            }).DisposeWith(_disposables);
        State.DpiChanged
            .Subscribe(x => MoveToGameCenter(gameWindowHooker, WindowWidth, x))
            .DisposeWith(_disposables);

        WindowWidthChanged = ReactiveCommand.Create<double, double>(width => WindowWidth = width);
        WindowWidthChanged
            .Throttle(TimeSpan.FromMilliseconds(ConstantValue.UserConfigOperationDelayTime))
            .Subscribe(v => ehConfigRepository.TextWindowWidth = v)
            .DisposeWith(_disposables);
        WindowOpacityChanged = ReactiveCommand.Create<double, double>(opacity => WindowOpacity = opacity);
        WindowOpacityChanged
            .Throttle(TimeSpan.FromMilliseconds(ConstantValue.UserConfigOperationDelayTime))
            .Subscribe(v => ehConfigRepository.TextWindowOpacity = v)
            .DisposeWith(_disposables);

        Pronounce = ReactiveCommand.Create(() => { });
        this.WhenAnyValue(x => x.EnableBlurBackground)
            .Skip(1)
            .Subscribe(x =>
            {
                HwndTools.WindowBlur(TextWindowHandle, x);
                ehConfigRepository.TextWindowBlur = x;
            });
    }

    [Reactive]
    public double Left { get; set; }

    [Reactive]
    public double Top { get; set; }

    [Reactive]
    public double WindowWidth { get; set; }

    [Reactive]
    public double WindowOpacity { get; set; }

    public ReactiveCommand<Unit, Unit> Loaded { get; }

    private readonly ObservableCollection<FuriganaItemViewModel> _furiganaItemViewModel;
    public ObservableCollection<FuriganaItemViewModel> TextControlViewModel => _furiganaItemViewModel;

    #region Command Panel

    public ReactiveCommand<double, double> WindowWidthChanged { get; }

    public ReactiveCommand<double, double> WindowOpacityChanged { get; }

    public ReactiveCommand<Unit, Unit> Pronounce { get; }

    [Reactive]
    public bool EnableBlurBackground { get; set; }

    #endregion Command Panel

    private void MoveToGameCenter(IGameWindowHooker gameWindowHooker, double windowWidth, double dpi)
    {
        var gamePos = gameWindowHooker.InvokeUpdatePosition();
        Top = (gamePos.Top + gamePos.Height / 2) / dpi;
        Left = (gamePos.Left + (gamePos.Width - windowWidth * dpi) / 2) / dpi;
    }

    private readonly Subject<Unit> _showSubj = new();
    public IObservable<Unit> Show => _showSubj;

    private readonly Subject<Unit> _hideSubj = new();
    public IObservable<Unit> Hide => _hideSubj;

    private readonly Subject<Unit> _closeSubj = new();
    public IObservable<Unit> Close => _closeSubj;

    private IDisposable WindowStatusChangeSubscription(
        IEHConfigRepository ehConfigRepository,
        IGameWindowHooker gameWindowHooker)
    {
        var disposables = new CompositeDisposable();
        ehConfigRepository.WhenAnyValue(x => x.HideTextWindow)
            .Skip(1)
            .Subscribe(close =>
            {
                if (close)
                {
                    _closeSubj.OnNext(Unit.Default);
                    TextWindowHandle = IntPtr.Zero;
                }
                else
                {
                    _showSubj.OnNext(Unit.Default);
                }
            }).DisposeWith(disposables);

        gameWindowHooker.WhenViewOperated
          .ObserveOn(RxApp.MainThreadScheduler)
          .Subscribe(operation =>
          {
              switch (operation)
              {
                  case ViewOperation.Show:
                      _showSubj.OnNext(Unit.Default);
                      break;
                  case ViewOperation.Hide:
                      _hideSubj.OnNext(Unit.Default);
                      break;
              }
          }).DisposeWith(disposables);

        return disposables;
    }

    private void UpdateOrAddFuriganaItems(List<FuriganaItemViewModel> vms)
    {
        if (_furiganaItemViewModel.Count < vms.Count)
        {
            var i = 0;
            for (; i < _furiganaItemViewModel.Count; i++)
            {
                _furiganaItemViewModel[i].Kana = vms[i].Kana;
                _furiganaItemViewModel[i].Text = vms[i].Text;
                _furiganaItemViewModel[i].FontSize = vms[i].FontSize;
                _furiganaItemViewModel[i].BackgroundColor = vms[i].BackgroundColor;
            }
            for (; i < vms.Count; i++)
            {
                _furiganaItemViewModel.Add(vms[i]);
            }
        }
        else
        {
            var i = 0;
            for (; i < vms.Count; i++)
            {
                _furiganaItemViewModel[i].Kana = vms[i].Kana;
                _furiganaItemViewModel[i].Text = vms[i].Text;
                _furiganaItemViewModel[i].FontSize = vms[i].FontSize;
                _furiganaItemViewModel[i].BackgroundColor = vms[i].BackgroundColor;
            }
            for (; i < _furiganaItemViewModel.Count; i++)
            {
                _furiganaItemViewModel[i].Kana = string.Empty;
                _furiganaItemViewModel[i].Text = string.Empty;
            }
        }
    }

    private readonly CompositeDisposable _disposables = new();
    public void Dispose() => _disposables.Dispose();
}
