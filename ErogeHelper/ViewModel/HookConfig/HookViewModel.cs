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
using ErogeHelper.Common.Definitions;
using ErogeHelper.Common.Entities;
using ErogeHelper.Common.Languages;
using ErogeHelper.Function;
using ErogeHelper.Model.Repositories;
using ErogeHelper.Model.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.HookConfig;

public class HookViewModel : ReactiveObject, IDisposable
{
    public HCodeViewModel HCodeViewModel { get; }
    public RCodeViewModel RCodeViewModel { get; }
    public TextRegExpViewModel TextCleanViewModel { get; }

    public HookViewModel(
        ITextractorService? textractorService = null,
        IGameInfoRepository? gameInfoRepository = null,
        HCodeViewModel? hcodeViewModel = null,
        RCodeViewModel? rcodeViewModel = null,
        TextRegExpViewModel? textCleanViewModel = null)
    {
        textractorService ??= DependencyResolver.GetService<ITextractorService>();
        gameInfoRepository ??= DependencyResolver.GetService<IGameInfoRepository>();
        HCodeViewModel = hcodeViewModel ?? DependencyResolver.GetService<HCodeViewModel>();
        RCodeViewModel = rcodeViewModel ?? DependencyResolver.GetService<RCodeViewModel>();
        TextCleanViewModel = textCleanViewModel ?? DependencyResolver.GetService<TextRegExpViewModel>();

        CurrentInUseHookName = textractorService.Setting.HookCode == string.Empty ?
            Strings.Common_None : textractorService.Setting.HookName;
        ConsoleInfo = textractorService.GetConsoleOutputInfo();
        ShowUnityGameTip = State.IsUnityGame;
        ShowRemoveHook = textractorService.FullSupport;
        ClipboardStatus = gameInfoRepository.UseClipboard;
        var hookThreads = new SourceCache<HookThreadParam, long>(p => p.Handle);
        var hookThreadItemsList = new SourceCache<HookThreadItemViewModel, long>(vm => vm.Handle);

        this.WhenAnyValue(x => x.CurrentInUseHookName)
            .Select(hookname => hookname == Strings.Common_None ? Color.Red : Color.Green)
            .ToPropertyEx(this, x => x.CurrentInUseHookColor);

        textractorService.Data
            .Where(hp => hp.Handle == 0)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(hp => ConsoleInfo += ConsoleInfo != string.Empty ? "\n" + hp.Text : hp.Text)
            .DisposeWith(_disposables);

        var canUseFunctions = new BehaviorSubject<bool>(textractorService.Injected);
        canUseFunctions.DisposeWith(_disposables);

        ReInject = ReactiveCommand.CreateFromTask(async () =>
        {
            if (textractorService.Injected)
            {
                canUseFunctions.OnNext(false);
                ConsoleInfo = string.Empty;
                Refresh?.Execute().Subscribe();
                await textractorService.ReAttachProcesses().ConfigureAwait(true);
                canUseFunctions.OnNext(true);
                return;
            }

            textractorService.InjectProcesses();
            await Task.Delay(500).ConfigureAwait(true);
            canUseFunctions.OnNext(true);
        });

        // FIXME: SE引擎不会出现，SelectedHookEngine切换后才正常, 再检查下打开窗口后是否正常出现
        Refresh = ReactiveCommand.Create(() =>
        {
            hookThreads.Clear();
            hookThreadItemsList.Clear();
        }, canUseFunctions);

        OpenHCodeDialog =
            ReactiveCommand.CreateFromObservable(() => HCodeViewModel.Show.Handle(Unit.Default), canUseFunctions);
        OpenHCodeDialog
            .Where(code => code != string.Empty)
            // TODO: Check gamename and code suffix (and .log?) (GameProcess.Name ?)
            // TODO: if there is : and suffix with exe, the executable name must be file name. How about .log?
            .Do(_ => Refresh.Execute().Subscribe())
            .Subscribe(textractorService.InsertHook);

        var canOpenRCodeDialog = canUseFunctions
            .CombineLatest(Observable.Return(textractorService.FullSupport))
            .Select(p => p.First && p.Second);
        OpenRCodeDialog =
            ReactiveCommand.CreateFromObservable(() => RCodeViewModel.Show.Handle(Unit.Default), canOpenRCodeDialog);
        OpenRCodeDialog
            .Where(text => text != string.Empty)
            .Subscribe(textractorService.SearchRCode);

        this.WhenAnyValue(x => x.ClipboardStatus)
            .Skip(1)
            .Subscribe(v => gameInfoRepository.UseClipboard = v);

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

        var canRemoveHook = Observable.Return(false);
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
            .Throttle(TimeSpan.FromMilliseconds(EHContext.UIMinimumResponseTime))
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

    public bool ShowUnityGameTip { get; }

    public ReactiveCommand<Unit, Unit> Refresh { get; }

    public ReactiveCommand<Unit, string> OpenHCodeDialog { get; }

    public ReactiveCommand<Unit, string> OpenRCodeDialog { get; }

    [Reactive]
    public bool ClipboardStatus { get; set; }

    private readonly ReadOnlyObservableCollection<HookEngineLabel> _hookEngineNames;
    public ReadOnlyObservableCollection<HookEngineLabel> HookEngineNames => _hookEngineNames;

    [Reactive]
    public HookEngineLabel? SelectedHookEngine { get; set; }

    public bool ShowRemoveHook { get; }
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
            IsUserHook = hookThreadItemViewModels.First().EngineName.Contains("UserHook"),
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
        if (textractorSetting.IsUserHook && textractorSetting.HookName != "UserHook1")
        {
            textractorSetting.HookName = "UserHook1";
        }
        gameInfoRepository.TextractorSettingJson = JsonSerializer.Serialize(textractorSetting);

        // And RegExp
        gameInfoRepository.RegExp = regexp;

        // Refresh current text in TextWindow
        MessageBus.Current.SendMessage<HookVMToTextVM>(new() { CurrentText = currentText });

        // Toast
        Interactions.ContentDialog
            .Handle(Strings.HookPage_SubmitDialogMessage)
            .Subscribe();

        return hookThreadItemViewModels.First().EngineName;
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
