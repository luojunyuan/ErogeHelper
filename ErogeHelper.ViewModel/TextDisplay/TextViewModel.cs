using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
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

public partial class TextViewModel : ReactiveObject, IEnableLogger, IDisposable
{
    private readonly IEHConfigRepository _ehConfigRepository;
    private readonly IMeCabService _mecabService;
    private readonly ITextractorService _textractorService;
    private readonly IGameDataService _gameDataService;
    private readonly IGameWindowHooker _gameWindowHooker;
    private readonly IGameInfoRepository _gameInfoRepository;

    public TextViewModel(
        ITextractorService? textractorService = null,
        IEHConfigRepository? ehConfigRepository = null,
        IGameDataService? gameDataService = null,
        IGameWindowHooker? gameWindowHooker = null,
        IMeCabService? mecabService = null,
        IGameInfoRepository? gameInfoRepository = null)
    {
        _textractorService = textractorService ?? DependencyResolver.GetService<ITextractorService>();
        _ehConfigRepository = ehConfigRepository ?? DependencyResolver.GetService<IEHConfigRepository>();
        _gameDataService = gameDataService ?? DependencyResolver.GetService<IGameDataService>();
        _gameWindowHooker = gameWindowHooker ?? DependencyResolver.GetService<IGameWindowHooker>();
        _mecabService = mecabService ?? DependencyResolver.GetService<IMeCabService>();
        _gameInfoRepository = gameInfoRepository ?? DependencyResolver.GetService<IGameInfoRepository>();

        // The "Zen" of ctor. if ViewModel is getting too big, split it up.

        Loaded = ReactiveCommand.Create(() =>
        {
            HwndTools.WindowBlur(TextWindowHandle, _ehConfigRepository.TextWindowBlur);
            HwndTools.WindowLostFocus(TextWindowHandle, _gameInfoRepository.GameInfo.IsLoseFocus);
            MoveToGameCenter(_gameWindowHooker, WindowWidth, State.Dpi, true);
        }).DisposeWith(_disposables);

        InitializeWindowRectBindings();

        (_furiganaItemViewModel, _appendTextViewModel)
            = InitializeTextBindings();

        (WindowWidthChanged, WindowOpacityChanged, Pronounce, FontDecrease, FontIncrease)
            = InitializeCommandPanelBingdings();
    }

    public void Dispose() => _disposables.Dispose();

    private void InitializeWindowRectBindings()
    {
        WindowWidth = _ehConfigRepository.TextWindowWidth;
        WindowOpacity = _ehConfigRepository.TextWindowOpacity;
        EnableBlurBackground = _ehConfigRepository.TextWindowBlur;

        _gameWindowHooker.WhenViewOperated
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

        _gameWindowHooker.GamePosChanged
            .Subscribe(pos =>
            {
                Left += pos.HorizontalChange / State.Dpi;
                Top += pos.VerticalChange / State.Dpi;
            }).DisposeWith(_disposables);

        // TODO: Relocate position when enter or exit full-screen
        _gameDataService.GameFullscreenChanged
            .Where(isFullscreen => isFullscreen)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ =>
            {
                var gamePos = _gameWindowHooker.InvokeUpdatePosition();
                User32.BringWindowToTop(TextWindowHandle);
            }).DisposeWith(_disposables);

        State.DpiChanged
            .Subscribe(dpi => MoveToGameCenter(_gameWindowHooker, WindowWidth, dpi, true))
            .DisposeWith(_disposables);
    }

    private (ObservableCollection<FuriganaItemViewModel>, ObservableCollection<AppendTextItemViewModel>) 
        InitializeTextBindings()
    {
        var furiganaItemViewModel = new ObservableCollection<FuriganaItemViewModel>();
        var appendTextViewModel = new ObservableCollection<AppendTextItemViewModel>()
        {
            new() { Text = Strings.TextWindow_DragAreaTip, FontSize = FontSize },
            new() { Text = Strings.TextWindow_WaitingForText, FontSize = FontSize }
        };
        if (_textractorService.Setting.HookCode == string.Empty)
        {
            appendTextViewModel.Add(new() { Text = Strings.TextWindow_SetHookTip, FontSize = FontSize });
        }
        FontSize = _ehConfigRepository.FontSize;
        this.WhenAnyValue(x => x.FontSize)
            .Subscribe(v =>
            {
                foreach (var i in furiganaItemViewModel)
                {
                    i.FontSize = v;
                }
                foreach (var i in appendTextViewModel)
                {
                    i.FontSize = v;
                }
                _ehConfigRepository.FontSize = v;
            });

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
                if (vms == null) furiganaItemViewModel.Clear();
                else UpdateOrAddFuriganaItems(vms);
            }).DisposeWith(_disposables);

        _ehConfigRepository.WhenAnyValue(x => x.KanaPosition)
            .Skip(1)
            .Where(_ => _currentText != string.Empty)
            .Subscribe(_ =>
            {
                furiganaItemViewModel.Clear();
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

        var sharedData = _textractorService.SelectedData
            .Select(hp => hp.Text)
            .Merge(textMessage)
            .Do(text => _currentText = text)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Do(_ => appendTextViewModel.Clear())
            .ObserveOn(RxApp.TaskpoolScheduler)
            .Select(text => _gameInfoRepository.GameInfo.RegExp == string.Empty ? text :
                Regex.Replace(text, _gameInfoRepository.GameInfo.RegExp, string.Empty, RegexOptions.Compiled))
            .Publish();

        sharedData.Connect().DisposeWith(_disposables);
        sharedData
            .Where(_ => _ehConfigRepository.EnableMeCab)
            .Select(text => GenerateFuriganaViewModels(text))
            .Do(_ => User32.BringWindowToTop(TextWindowHandle))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(UpdateOrAddFuriganaItems).DisposeWith(_disposables);

        return (furiganaItemViewModel, appendTextViewModel);
    }

    private (ReactiveCommand<Unit, double>, ReactiveCommand<Unit, double>, ReactiveCommand<Unit, Unit>,
        ReactiveCommand<Unit, double>, ReactiveCommand<Unit, double>) 
        InitializeCommandPanelBingdings()
    {
        var windowWidthChanged = ReactiveCommand.Create<Unit, double>(_ => WindowWidth);
        windowWidthChanged
            .Throttle(TimeSpan.FromMilliseconds(ConstantValue.UserConfigOperationDelayTime))
            .Subscribe(v => _ehConfigRepository.TextWindowWidth = v)
            .DisposeWith(_disposables);
        var windowOpacityChanged = ReactiveCommand.Create<Unit, double>(_ => WindowOpacity);
        windowOpacityChanged
            .Throttle(TimeSpan.FromMilliseconds(ConstantValue.UserConfigOperationDelayTime))
            .Subscribe(v => _ehConfigRepository.TextWindowOpacity = v)
            .DisposeWith(_disposables);

        var pronounce = ReactiveCommand.Create(() => { });

        var canDecrease = this.WhenAnyValue(x => x.FontSize)
            .Select(size => size > 10);
        var fontDecrease = ReactiveCommand.Create(() => FontSize -= 2, canDecrease);
        var fontIncrease = ReactiveCommand.Create(() => FontSize += 2);

        this.WhenAnyValue(x => x.EnableBlurBackground)
            .Skip(1)
            .Subscribe(x =>
            {
                HwndTools.WindowBlur(TextWindowHandle, x);
                _ehConfigRepository.TextWindowBlur = x;
            });

        return (windowWidthChanged, windowOpacityChanged, pronounce, fontDecrease, fontIncrease);
    }

    // Note: Sometimes it can't be positioned correctly at second screen, need to try several times
    private void MoveToGameCenter(IGameWindowHooker gameWindowHooker,
        double windowWidth, double dpi, bool moveTop = false, double topOffset = 0)
    {
        var gamePos = gameWindowHooker.InvokeUpdatePosition();
        Left = (gamePos.Left + (gamePos.Width - windowWidth * dpi) / 2) / dpi;
        Top = (moveTop ? gamePos.Top
                       : (gamePos.Top + gamePos.Height / 2)) / dpi
            + topOffset;
    }

    private List<FuriganaItemViewModel> GenerateFuriganaViewModels(string text) =>
        _mecabService.GenerateMeCabWords(text)
            .Select(item => new FuriganaItemViewModel()
            {
                Text = item.Word,
                Kana = item.Kana,
                FontSize = FontSize,
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
}
