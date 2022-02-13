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
using ErogeHelper.Shared.Languages;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.TextDisplay;

public class TextViewModel : ReactiveObject, IEnableLogger, IDisposable
{
    public static HWND TextWindowHandle { get; set; } = IntPtr.Zero;

    private readonly IEHConfigRepository _ehConfigRepository;

    public TextViewModel(
        ITextractorService? textractorService = null,
        IEHConfigRepository? ehConfigRepository = null,
        IGameDataService? gameDataService = null,
        IGameWindowHooker? gameWindowHooker = null,
        IMeCabService? mecabService = null,
        IGameInfoRepository? gameInfoRepository = null)
    {
        textractorService ??= DependencyResolver.GetService<ITextractorService>();
        _ehConfigRepository = ehConfigRepository ?? DependencyResolver.GetService<IEHConfigRepository>();
        gameDataService ??= DependencyResolver.GetService<IGameDataService>();
        gameWindowHooker ??= DependencyResolver.GetService<IGameWindowHooker>();
        mecabService ??= DependencyResolver.GetService<IMeCabService>();
        gameInfoRepository ??= DependencyResolver.GetService<IGameInfoRepository>();

        // The "Zen" of ctor. if ViewModel is getting too big, split it up.

        _fontSize = _ehConfigRepository.FontSize;
        WindowWidth = _ehConfigRepository.TextWindowWidth;
        WindowOpacity = _ehConfigRepository.TextWindowOpacity;
        EnableBlurBackground = _ehConfigRepository.TextWindowBlur;
        _furiganaItemViewModel = new ObservableCollection<FuriganaItemViewModel>();
        _appendTextViewModel = new ObservableCollection<AppendTextItemViewModel>() 
        { 
            new() { Text = Strings.TextWindow_DragAreaTip, FontSize = _fontSize },
            new() { Text = Strings.TextWindow_WaitingForText, FontSize = _fontSize }
        };
        if (textractorService.Setting == null)
        {
            _appendTextViewModel.Add(new() { Text = Strings.TextWindow_SetHookTip, FontSize = _fontSize });
        }
        if (!_ehConfigRepository.EnableMeCab)
        {
            _appendTextViewModel.Add(new() { Text = Strings.TextWindow_EnableFunctionTip, FontSize = _fontSize });
        }

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
            }).DisposeWith(_disposables);
            
        Loaded = ReactiveCommand.Create(() =>
        {
            HwndTools.WindowBlur(TextWindowHandle, _ehConfigRepository.TextWindowBlur);
            HwndTools.WindowLostFocus(TextWindowHandle, gameInfoRepository.GameInfo.IsLoseFocus);
            MoveToGameCenter(gameWindowHooker, WindowWidth, State.Dpi);
        }).DisposeWith(_disposables);

        gameWindowHooker.GamePosChanged
            .Subscribe(pos =>
            {
                Left += pos.HorizontalChange / State.Dpi;
                Top += pos.VerticalChange / State.Dpi;
            }).DisposeWith(_disposables);

        // TODO: Relocate position when enter or exit full-screen
        gameDataService.GameFullscreenChanged
            .Where(isFullscreen => isFullscreen)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ =>
            {
                var gamePos = gameWindowHooker.InvokeUpdatePosition();
                User32.BringWindowToTop(TextWindowHandle);
            }).DisposeWith(_disposables);
        State.DpiChanged
            .Subscribe(x => MoveToGameCenter(gameWindowHooker, WindowWidth, x))
            .DisposeWith(_disposables);



        var sharedData = textractorService.SelectedData
            .ObserveOn(RxApp.MainThreadScheduler)
            .Do(_ => _appendTextViewModel.Clear())
            .ObserveOn(RxApp.TaskpoolScheduler)
            // TODO: Regexp clean
            .Publish();

        sharedData.Connect().DisposeWith(_disposables);
        sharedData
            .Where(_ => _ehConfigRepository.EnableMeCab)
            .Select(hp => mecabService
                .GenerateMeCabWords(hp.Text).Select(item => new FuriganaItemViewModel()
                {
                    Text = item.Word,
                    Kana = item.Kana,
                    FontSize = _fontSize,
                    BackgroundColor = item.PartOfSpeech.ToColor(),
                }).ToList())
            .Do(_ => User32.BringWindowToTop(TextWindowHandle))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(UpdateOrAddFuriganaItems).DisposeWith(_disposables);


        WindowWidthChanged = ReactiveCommand.Create<double, double>(width => WindowWidth = width);
        WindowWidthChanged
            .Throttle(TimeSpan.FromMilliseconds(ConstantValue.UserConfigOperationDelayTime))
            .Subscribe(v => _ehConfigRepository.TextWindowWidth = v)
            .DisposeWith(_disposables);
        WindowOpacityChanged = ReactiveCommand.Create<double, double>(opacity => WindowOpacity = opacity);
        WindowOpacityChanged
            .Throttle(TimeSpan.FromMilliseconds(ConstantValue.UserConfigOperationDelayTime))
            .Subscribe(v => _ehConfigRepository.TextWindowOpacity = v)
            .DisposeWith(_disposables);

        Pronounce = ReactiveCommand.Create(() => { });

        var canDecrease = this.WhenAnyValue(x => x.FontSize)
            .Select(size => size > 10);
        FontDecrease = ReactiveCommand.Create(() =>
        {
            FontModify(-2);
            FontSize -= 2;
        }, canDecrease);
        FontIncrease = ReactiveCommand.Create(() =>
        {
            FontModify(2);
            FontSize += 2;
        });

        this.WhenAnyValue(x => x.EnableBlurBackground)
            .Skip(1)
            .Subscribe(x =>
            {
                HwndTools.WindowBlur(TextWindowHandle, x);
                _ehConfigRepository.TextWindowBlur = x;
            });
    }

    public ReactiveCommand<Unit, Unit> Loaded { get; }

    private readonly Subject<Unit> _showSubj = new();
    public IObservable<Unit> Show => _showSubj;

    private readonly Subject<Unit> _hideSubj = new();
    public IObservable<Unit> Hide => _hideSubj;

    [Reactive]
    public double Left { get; set; }

    [Reactive]
    public double Top { get; set; }

    [Reactive]
    public double WindowWidth { get; set; }

    [Reactive]
    public double WindowOpacity { get; set; }

    private readonly ObservableCollection<FuriganaItemViewModel> _furiganaItemViewModel;
    public ObservableCollection<FuriganaItemViewModel> TextControlViewModel => _furiganaItemViewModel;

    private readonly ObservableCollection<AppendTextItemViewModel> _appendTextViewModel;
    public ObservableCollection<AppendTextItemViewModel> AppendTextControlViewModel => _appendTextViewModel;

    #region Command Panel

    public ReactiveCommand<double, double> WindowWidthChanged { get; }

    public ReactiveCommand<double, double> WindowOpacityChanged { get; }

    public ReactiveCommand<Unit, Unit> Pronounce { get; }

    public ReactiveCommand<Unit, Unit> FontDecrease { get; }
    
    public ReactiveCommand<Unit, Unit> FontIncrease { get; }

    private double _fontSize;
    private double FontSize
    {
        get => _fontSize;
        set
        {
            _ehConfigRepository.FontSize = value;
            this.RaiseAndSetIfChanged(ref _fontSize, value);
        }
    }

    private void FontModify(double quantity)
    {
        foreach (var i in _furiganaItemViewModel)
        {
            i.FontSize += quantity;
        }
        foreach (var i in _appendTextViewModel)
        {
            // TODO: FIXME: Not work
            i.FontSize += quantity;
        }
    }


    [Reactive]
    public bool EnableBlurBackground { get; set; }

    #endregion Command Panel

    // Note: Sometimes it can't be positioned correctly at second screen, need to try several times
    private void MoveToGameCenter(IGameWindowHooker gameWindowHooker, double windowWidth, double dpi)
    {
        var gamePos = gameWindowHooker.InvokeUpdatePosition();
        Left = (gamePos.Left + (gamePos.Width - windowWidth * dpi) / 2) / dpi;
        Top = (gamePos.Top + gamePos.Height / 2) / dpi;
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
