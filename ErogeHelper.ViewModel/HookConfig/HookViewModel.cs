using System.Collections.ObjectModel;
using System.Drawing;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using DynamicData;
using DynamicData.Binding;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.Shared.Entities;
using ErogeHelper.Shared.Languages;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.HookConfig;

public class HookViewModel : ReactiveObject, IDisposable
{
    public static Action<bool> EnableClipboardCallback { get; set; } = null!;

    public HCodeViewModel HCodeViewModel { get; }
    public RCodeViewModel RCodeViewModel { get; }
    public TextRegExpViewModel TextCleanViewModel { get; }

    public Interaction<Unit, Unit> ReiPatcherInteraction { get; } = new();

    public HookViewModel(
        ITextractorService? textractorService = null,
        IGameInfoRepository? gameInfoRepository = null,
        IGameDataService? gameDataService = null,
        HCodeViewModel? hcodeViewModel = null,
        RCodeViewModel? rcodeViewModel = null,
        TextRegExpViewModel? textCleanViewModel = null)
    {
        textractorService ??= DependencyResolver.GetService<ITextractorService>();
        gameInfoRepository ??= DependencyResolver.GetService<IGameInfoRepository>();
        gameDataService ??= DependencyResolver.GetService<IGameDataService>();
        HCodeViewModel = hcodeViewModel ?? DependencyResolver.GetService<HCodeViewModel>();
        RCodeViewModel = rcodeViewModel ?? DependencyResolver.GetService<RCodeViewModel>();
        TextCleanViewModel = textCleanViewModel ?? DependencyResolver.GetService<TextRegExpViewModel>();

        CurrentInUseHookName = textractorService.Setting.HookCode == string.Empty ?
            Strings.Common_None : textractorService.Setting.HookName;
        ConsoleInfo = string.Join('\n', textractorService.GetConsoleOutputInfo());
        ClipboardStatus = gameInfoRepository.GameInfo.UseClipboard;
        var hookThreads = new SourceCache<HookThreadParam, long>(p => p.Handle);
        var hookThreadItemsList = new SourceCache<HookThreadItemViewModel, long>(vm => vm.Handle);

        this.WhenAnyValue(x => x.CurrentInUseHookName)
            .Select(hookname => hookname == Strings.Common_None ? Color.Red : Color.Green)
            .ToPropertyEx(this, x => x.CurrentInUseHookColor);

        textractorService.Data
            .Where(hp => hp.Handle == 0)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(hp => ConsoleInfo += "\n" + hp.Text)
            .DisposeWith(_disposables);

        var gameDir = Path.GetDirectoryName(gameDataService.GamePath)!;

        var isUnityGame = File.Exists(Path.Combine(gameDir, "UnityPlayer.dll"));
        Observable.Return(isUnityGame)
            .ToPropertyEx(this, x => x.ShowReiPatcherTipDialog);
        ReiPatcherTipDialog = ReactiveCommand.CreateFromObservable(() => ReiPatcherInteraction.Handle(Unit.Default));

        Observable.Return(isUnityGame)
            .ToPropertyEx(this, x => x.ShowUnityGameTip);

        Refresh = ReactiveCommand.Create(() =>
        {
            hookThreads.Clear();
            hookThreadItemsList.Clear();
        });

        var canUseFunctions = new BehaviorSubject<bool>(textractorService.Injected);
        canUseFunctions.DisposeWith(_disposables);
        // TODO: Write a TextractorConfig.txt file for Unity game { string:Substring (int,int) }
        ReInject = SetupReInject(textractorService, gameDataService, canUseFunctions);

        OpenHCodeDialog =
            ReactiveCommand.CreateFromObservable(() => HCodeViewModel.Show.Handle(Unit.Default), canUseFunctions);
        OpenHCodeDialog
            .Where(code => code != string.Empty)
            // TODO: Check gamename and code suffix (and .log?) (GameProcess.Name ?)
            //.Select(code => var gameFileName = Path.GetFileName(gameDataService.GamePath))
            .Subscribe(textractorService.InsertHook);

        OpenRCodeDialog =
            ReactiveCommand.CreateFromObservable(() => RCodeViewModel.Show.Handle(Unit.Default), canUseFunctions);
        OpenRCodeDialog
            .Where(text => text != string.Empty)
            .Subscribe(textractorService.SearchRCode);

        this.WhenAnyValue(x => x.ClipboardStatus)
            .Skip(1)
            .Subscribe(v =>
            {
                EnableClipboardCallback(v);
                gameInfoRepository.UpdateUseClipboard(v);
            });

        textractorService.Data
            .Where(hp => hp.Handle != 0)
            .Select(hp => new HookThreadParam(
                hp.Handle, hp.Text, hp.Address, hp.Name, hp.HookCode, hp.Ctx, hp.Ctx2))
            .Subscribe(hookThreads.AddOrUpdate)
            .DisposeWith(_disposables);

        hookThreads.Connect()
            .DistinctValues(v => new HookEngineLabel(v.Address, v.EngineName))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _hookEngineNames)
            .Where(_ => HookEngineNames.Count != 0)
            .Subscribe(_ => SelectedHookEngine ??= _hookEngineNames.First());

        var canRemoveHook = Utils.IsArm ? Observable.Return(false) :
            this.WhenAnyValue(
                x => x.SelectedHookEngine,
                v => v != null && !v.Value.EngineName.Equals("Clipboard", StringComparison.Ordinal));
        RemoveHook = ReactiveCommand.Create(() =>
        {
            var threadAddress = SelectedHookEngine!.Value.Address;
            textractorService.RemoveHook(threadAddress);
            var targetThreads = hookThreads.Items.Where(p => p.Address == threadAddress);
            hookThreads.Remove(targetThreads);
            hookThreadItemsList.Clear();
        }, canRemoveHook);

        #region Hook Thread Items
        hookThreadItemsList
            .Connect()
            .Bind(out _hookThreadItems)
            .Subscribe();

        var itemIndex = 1;
        var resetSelectedAction = true;
        var changeHookThreadsAction = new Subject<Unit>();
        this.WhenAnyValue(x => x.SelectedHookEngine)
            .WhereNotNull()
            .Do(_ =>
            {
                changeHookThreadsAction.OnNext(Unit.Default);
                hookThreadItemsList.Clear();
                itemIndex = 1;
                resetSelectedAction = true;
            })
            .ObserveOn(TaskPoolScheduler.Default.DisableOptimizations(typeof(ISchedulerLongRunning)))
            .SelectMany(_ => hookThreads.Items.ToObservable()
                .Where(v => v.Address == SelectedHookEngine!.Value.Address)
                .Select(buf =>
                {
                    if (resetSelectedAction)
                    {
                        resetSelectedAction = false;
                        return Observable.Return(buf);
                    }
                    return Observable.Return(buf).Delay(TimeSpan.FromMilliseconds(100));
                })
                .Concat()
                .TakeUntil(changeHookThreadsAction))
            .Select(v => new HookThreadItemViewModel()
            {
                Index = itemIndex++,
                Handle = v.Handle,
                TotalText = v.LatestText,
                HookCode = v.HookCode,
                EngineName = v.EngineName,
                Context = v.ThreadContext,
                SubContext = v.SubThreadContext
            })
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(hookThreadItemsList.AddOrUpdate);

        textractorService.Data
            // NOTE: This is a lossy back pressure for display
            .Throttle(TimeSpan.FromMilliseconds(ConstantValue.UIMinimumResponseTime))
            .Where(v => SelectedHookEngine is not null &&
                v.Address == SelectedHookEngine!.Value.Address &&
                // different handle
                hookThreadItemsList.Items.All(m => m.Handle != v.Handle))
            .Select(v => new HookThreadItemViewModel()
            {
                Index = itemIndex++,
                Handle = v.Handle,
                TotalText = v.Text,
                HookCode = v.HookCode,
                EngineName = v.Name,
                Context = v.Ctx,
                SubContext = v.Ctx2
            })
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(hookThreadItemsList.AddOrUpdate)
            .DisposeWith(_disposables);
        #endregion Hook Thread Items

        var canDoNextStep = _hookThreadItems
            .ToObservableChangeSet()
            .AutoRefresh(m => m.IsTextThread)
            .ToCollection()
            .Select(vms => vms.Any(m => m.IsTextThread));

        OpenTextSplitDialog = ReactiveCommand.CreateFromObservable(() =>
        {
            var selectedItems = _hookThreadItems.Where(vm => vm.IsTextThread);
            var firstItemText = selectedItems.First().TotalText;
            var index = firstItemText.LastIndexOf('\n');
            if (index == -1) index = 0;
            return TextCleanViewModel.Show.Handle((selectedItems.Select(vm => vm.Handle), firstItemText[index..]));
        }, canDoNextStep);
        OpenTextSplitDialog
            .Where(output => output.CanSubmit)
            .Subscribe(output => CurrentInUseHookName = SubmitSetting(textractorService, gameInfoRepository,
                hookThreadItemsList.Items.ToList(), output.RegExp, output.CurrentText));
    }

    [Reactive]
    public string CurrentInUseHookName { get; set; }

    [ObservableAsProperty]
    public Color CurrentInUseHookColor { get; }

    [Reactive]
    public string ConsoleInfo { get; set; }

    public ReactiveCommand<Unit, Unit> ReInject { get; }

    [ObservableAsProperty]
    public bool ShowUnityGameTip { get; }

    [ObservableAsProperty]
    public bool ShowReiPatcherTipDialog { get; }

    public ReactiveCommand<Unit, Unit> ReiPatcherTipDialog { get; }

    public ReactiveCommand<Unit, Unit> Refresh { get; }

    public ReactiveCommand<Unit, string> OpenHCodeDialog { get; }

    public ReactiveCommand<Unit, string> OpenRCodeDialog { get; }

    [Reactive]
    public bool ClipboardStatus { get; set; }

    private readonly ReadOnlyObservableCollection<HookEngineLabel> _hookEngineNames;
    public ReadOnlyObservableCollection<HookEngineLabel> HookEngineNames => _hookEngineNames;

    [Reactive]
    public HookEngineLabel? SelectedHookEngine { get; set; }

    public ReactiveCommand<Unit, Unit> RemoveHook { get; }

    private readonly ReadOnlyObservableCollection<HookThreadItemViewModel> _hookThreadItems;
    public ReadOnlyObservableCollection<HookThreadItemViewModel> HookThreadItems => _hookThreadItems;

    public ReactiveCommand<Unit, (bool CanSubmit, string RegExp, string CurrentText)> OpenTextSplitDialog { get; }

    /// <returns>HookName</returns>
    private static string SubmitSetting(
        ITextractorService textractorService,
        IGameInfoRepository gameInfoRepository,
        IReadOnlyCollection<HookThreadItemViewModel> hookThreadItemViewModels,
        string regexp, string currentText)
    {
        // Build textractor setting
        var textractorSetting = new TextractorSetting()
        {
            IsUserHook = false, // TODO: UserHook
            HookCode = hookThreadItemViewModels.First().HookCode,
            HookName = hookThreadItemViewModels.First().EngineName,
            HookSettings = hookThreadItemViewModels
                .Where(vm => vm.IsCharacterThread || vm.IsTextThread)
                .Select(vm => new TextractorSetting.HookSetting
                {
                    ThreadType = SelectThreadType(vm),
                    ThreadContext = vm.Context,
                    SubThreadContext = vm.SubContext
                }).ToList()
        };

        // Update textractor setting
        textractorService.SetSetting(textractorSetting);

        // Update to local database
        var setting = JsonSerializer.Serialize(textractorSetting);
        gameInfoRepository.UpdateTextractorSetting(setting);

        // And RegExp
        gameInfoRepository.UpdateRegExp(regexp);

        // Refresh current text in TextWindow
        MessageBus.Current.SendMessage<HookVMToTextVM>(new() { CurrentText = currentText });

        // Toast
        Interactions.ContentDialog
            .Handle(Strings.HookPage_SubmitDialogMessage)
            .Subscribe();

        return textractorSetting.HookName;
    }

    private ReactiveCommand<Unit, Unit> SetupReInject(
        ITextractorService textractorService,
        IGameDataService gameDataService,
        BehaviorSubject<bool> canUseFunctions)
    {
        if (Utils.IsArm)
        {
            var injectOnlyOnce = new BehaviorSubject<bool>(true);
            return ReactiveCommand.Create(() =>
            {
                textractorService.InjectProcesses(gameDataService);

                injectOnlyOnce.OnNext(false);
                injectOnlyOnce.OnCompleted();
                canUseFunctions.OnNext(true);
                canUseFunctions.OnCompleted();
            }, injectOnlyOnce);
        }
        else
        {
            return ReactiveCommand.Create(() =>
            {
                ConsoleInfo = string.Empty;
                Refresh.Execute().Subscribe();
                textractorService.ReAttachProcesses();

                if (!canUseFunctions.IsDisposed)
                {
                    canUseFunctions.OnNext(true);
                    canUseFunctions.OnCompleted();
                    canUseFunctions.Dispose();
                }
            });
        }
    }

    private static TextractorSetting.TextThread SelectThreadType(HookThreadItemViewModel vm)
    {
        if (vm.IsTextThread)
        {
            return TextractorSetting.TextThread.Text;
        }
        else if (vm.IsCharacterThread)
        {
            return TextractorSetting.TextThread.CharacterName;
        }

        throw new InvalidOperationException();
    }

    private readonly CompositeDisposable _disposables = new();
    public void Dispose() => _disposables.Dispose();

    // ReSharper disable once NotAccessedPositionalProperty.Global
    public readonly record struct HookEngineLabel(long Address, string EngineName);

    /// <param name="Handle">Unique key</param>
    /// <param name="Address">Represent a group of hooks, uniformity as EngineName and HookCode</param>
    public readonly record struct HookThreadParam(
        long Handle,
        string LatestText,
        long Address,
        string EngineName,
        string HookCode,
        long ThreadContext,
        long SubThreadContext);
}
