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
using ErogeHelper.ViewModel.MainGame;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.TextDisplay;

public class TextViewModel : ReactiveObject, IEnableLogger, IDisposable
{
    public static HWND TextWindowHandle { get; set; } = IntPtr.Zero;

    private readonly IEHConfigRepository _ehConfigRepository;
    private readonly IMeCabService _mecabService;

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
        _mecabService = mecabService ?? DependencyResolver.GetService<IMeCabService>();
        gameInfoRepository ??= DependencyResolver.GetService<IGameInfoRepository>();

        var mainGameViewModel = DependencyResolver.GetService<MainGameViewModel>();

        // The "Zen" of ctor. if ViewModel is getting too big, split it up.

        WindowScale = _ehConfigRepository.TextWindowWidthScale;
        _fontSize = _ehConfigRepository.FontSize;
        WindowOpacity = _ehConfigRepository.TextWindowOpacity;
        EnableBlurBackground = _ehConfigRepository.TextWindowBlur;
        _furiganaItemViewModel = new ObservableCollection<FuriganaItemViewModel>();
        _appendTextViewModel = new ObservableCollection<AppendTextItemViewModel>()
        {
            new() { Text = Strings.TextWindow_DragAreaTip, FontSize = _fontSize },
            new() { Text = Strings.TextWindow_WaitingForText, FontSize = _fontSize }
        };
        if (textractorService.Setting.HookCode == string.Empty)
        {
            _appendTextViewModel.Add(new() { Text = Strings.TextWindow_SetHookTip, FontSize = _fontSize });
        }

        WindowWidth = CaculateWindowWindth(mainGameViewModel.Width, _ehConfigRepository.TextWindowWidthScale);
        mainGameViewModel.WhenAnyValue(x => x.Width)
            .Subscribe(width => WindowWidth = CaculateWindowWindth(width, _ehConfigRepository.TextWindowWidthScale))
            .DisposeWith(_disposables);
        this.WhenAnyValue(x => x.WindowScale)
            .Subscribe(scale => WindowWidth = CaculateWindowWindth(mainGameViewModel.Width, scale));

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

        // TODO: Relocate position when enter or exit full-screen 应该好弄
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



        _ehConfigRepository.WhenAnyValue(x => x.EnableMeCab, enable => !enable)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToPropertyEx(this, x => x.ShowFunctionNotEnableTip)
            .DisposeWith(_disposables);

        _ehConfigRepository.WhenAnyValue(x => x.EnableMeCab)
            .Skip(1)
            .Select(enable =>
                (enable && _currentText != string.Empty) ? GenerateFuriganaViewModels(_currentText) : null)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(vms => 
            { 
                if (vms == null) _furiganaItemViewModel.Clear(); 
                else UpdateOrAddFuriganaItems(vms); 
            }).DisposeWith(_disposables);

        _ehConfigRepository.WhenAnyValue(x => x.KanaPosition)
            .Skip(1)
            .Where(_ => _currentText != string.Empty)
            .Subscribe(_ =>
            {
                _furiganaItemViewModel.Clear();
                UpdateOrAddFuriganaItems(GenerateFuriganaViewModels(_currentText));
            }).DisposeWith(_disposables);

        _ehConfigRepository.WhenAnyValue(x => x.KanaRuby)
            .Skip(1)
            .Where(_ => _currentText != string.Empty)
            .Subscribe(_ => UpdateOrAddFuriganaItems(GenerateFuriganaViewModels(_currentText)))
            .DisposeWith(_disposables);

        // New text stream
        var textMessage = MessageBus.Current.Listen<HookVMToTextVM>()
            .Select(msg => _currentText = msg.CurrentText);

        var sharedData = textractorService.SelectedData
            .Select(hp => hp.Text)
            .Merge(textMessage)
            .Do(text => _currentText = text)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Do(_ => _appendTextViewModel.Clear())
            .ObserveOn(RxApp.TaskpoolScheduler)
            // TODO: Regexp clean
            .Publish();

        sharedData.Connect().DisposeWith(_disposables);
        sharedData
            .Where(_ => _ehConfigRepository.EnableMeCab)
            .Select(text => GenerateFuriganaViewModels(text))
            .Do(_ => User32.BringWindowToTop(TextWindowHandle))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(UpdateOrAddFuriganaItems).DisposeWith(_disposables);

        WindowWidthChanged = ReactiveCommand.Create<Unit, double>(_ => WindowScale);
        WindowWidthChanged
            .Throttle(TimeSpan.FromMilliseconds(ConstantValue.UserConfigOperationDelayTime))
            .Subscribe(v => _ehConfigRepository.TextWindowWidthScale = v)
            .DisposeWith(_disposables);
        WindowOpacityChanged = ReactiveCommand.Create<Unit, double>(_ => WindowOpacity);
        WindowOpacityChanged
            .Throttle(TimeSpan.FromMilliseconds(ConstantValue.UserConfigOperationDelayTime))
            .Subscribe(v => _ehConfigRepository.TextWindowOpacity = v)
            .DisposeWith(_disposables);

        Pronounce = ReactiveCommand.Create(() => { });

        var canDecrease = this.WhenAnyValue(x => x.FontSize)
            .Select(size => size > 10);
        FontDecrease = ReactiveCommand.Create(() => FontSize -= 2, canDecrease);
        FontIncrease = ReactiveCommand.Create(() => FontSize += 2);

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

    [ObservableAsProperty]
    public bool ShowFunctionNotEnableTip { get; }

    #region Command Panel

    [Reactive]
    public double WindowScale { get; set; }

    public ReactiveCommand<Unit, double> WindowWidthChanged { get; }

    public ReactiveCommand<Unit, double> WindowOpacityChanged { get; }

    public ReactiveCommand<Unit, Unit> Pronounce { get; }

    public ReactiveCommand<Unit, double> FontDecrease { get; }
    
    public ReactiveCommand<Unit, double> FontIncrease { get; }

    private double _fontSize;
    private double FontSize
    {
        get => _fontSize;
        set
        {
            foreach (var i in _furiganaItemViewModel)
            {
                i.FontSize = value;
            }
            foreach (var i in _appendTextViewModel)
            {
                i.FontSize = value;
            }
            _ehConfigRepository.FontSize = value;
            this.RaiseAndSetIfChanged(ref _fontSize, value);
        }
    }


    [Reactive]
    public bool EnableBlurBackground { get; set; }

    #endregion Command Panel

    private string _currentText = string.Empty;

    private static double CaculateWindowWindth(double gameWidth, double userScale) => gameWidth * userScale;

    // Note: Sometimes it can't be positioned correctly at second screen, need to try several times
    private void MoveToGameCenter(IGameWindowHooker gameWindowHooker, double windowWidth, double dpi, double offset = 0)
    {
        var gamePos = gameWindowHooker.InvokeUpdatePosition();
        Left = (gamePos.Left + (gamePos.Width - windowWidth * dpi) / 2) / dpi;
        Top = (gamePos.Top + gamePos.Height / 2) / dpi + offset;
    }

    private List<FuriganaItemViewModel> GenerateFuriganaViewModels(string text) =>
        _mecabService.GenerateMeCabWords(text)
            .Select(item => new FuriganaItemViewModel()
            {
                Text = item.Word,
                Kana = item.Kana,
                FontSize = _fontSize,
                BackgroundColor = item.PartOfSpeech.ToColor(),
            }).ToList();

    private void UpdateOrAddFuriganaItems(List<FuriganaItemViewModel> vms)
    {
        if (_furiganaItemViewModel.Count < vms.Count)
        {
            var i = 0;
            for (; i < _furiganaItemViewModel.Count; i++)
            {
                UpdateFuriganaItemViewModelAt(i, vms[i]);
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
                UpdateFuriganaItemViewModelAt(i, vms[i]);
            }

            for (; i < _furiganaItemViewModel.Count; i++)
            {
                _furiganaItemViewModel[i].Kana = string.Empty;
                _furiganaItemViewModel[i].Text = string.Empty;
            }
        }
    }

    private void UpdateFuriganaItemViewModelAt(int i, FuriganaItemViewModel vm)
    {
        _furiganaItemViewModel[i].Kana = vm.Kana;
        _furiganaItemViewModel[i].Text = vm.Text;
        _furiganaItemViewModel[i].FontSize = vm.FontSize;
        _furiganaItemViewModel[i].BackgroundColor = vm.BackgroundColor;
    }

    private readonly CompositeDisposable _disposables = new();
    public void Dispose() => _disposables.Dispose();
}
