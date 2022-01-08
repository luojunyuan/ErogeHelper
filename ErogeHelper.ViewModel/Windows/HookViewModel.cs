using System.Collections.ObjectModel;
using System.Drawing;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
using DynamicData;
using DynamicData.Binding;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Entities;
using ErogeHelper.Shared.Languages;
using ErogeHelper.ViewModel.Dialogs;
using ErogeHelper.ViewModel.Items;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace ErogeHelper.ViewModel.Windows;

public class HookViewModel : ReactiveObject, IEnableLogger, IDisposable
{
    public HCodeViewModel HCodeViewModel { get; }

    public HookViewModel(
        ITextractorService? textractorService = null,
        IGameInfoRepository? gameInfoRepository = null,
        HCodeViewModel? hcodeViewModel = null)
    {
        textractorService ??= DependencyResolver.GetService<ITextractorService>();
        gameInfoRepository ??= DependencyResolver.GetService<IGameInfoRepository>();
        HCodeViewModel = hcodeViewModel ?? DependencyResolver.GetService<HCodeViewModel>();

        CurrentInUseHookName = textractorService.Setting.HookCode == string.Empty ?
            Strings.Common_None : textractorService.Setting.HookName;
        ConsoleInfo = string.Join('\n', textractorService.GetConsoleOutputInfo());
        var hookThreads = new SourceCache<HookThreadParam, long>(p => p.Handle);

        this.WhenAnyValue(x => x.CurrentInUseHookName)
            .Select(hookname => hookname == Strings.Common_None ? Color.Red : Color.Green)
            .ToPropertyEx(this, x => x.CurrentInUseHookColor);

        textractorService.Data
            .Where(hp => hp.Handle == 0)
            .Subscribe(hp => ConsoleInfo += "\n" + hp.Text)
            .DisposeWith(_disposables);

        var canReInject = Utils.IsArm ? Observable.Return(false) : Observable.Return(true);
        ReInject = ReactiveCommand.Create(textractorService.ReAttachProcesses, canReInject);

        OpenHCodeDialog = ReactiveCommand.CreateFromObservable(() => HCodeViewModel.Show.Handle(Unit.Default));
        // TODO: Already insert tip, try move game text and check Combobox
        OpenHCodeDialog
            .Where(code => code != string.Empty)
            .Subscribe(textractorService.InsertHook);

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
            .Subscribe(v => SelectedHookEngine ??= _hookEngineNames.First());

        var canRemoveHook = Utils.IsArm ? Observable.Return(false) :
            this.WhenAnyValue<HookViewModel, bool, HookEngineLabel?>(
                x => x.SelectedHookEngine,
                v => v != null);
        RemoveHook = ReactiveCommand.Create(
            () => textractorService.RemoveHook(SelectedHookEngine!.Value.Address), canRemoveHook);

        #region Hook Thread Items
        var hookThreadItemsList = new SourceCache<HookThreadItemViewModel, long>(vm => vm.Handle);
        hookThreadItemsList
            .Connect()
            .Bind(out _hookThreadItems)
            .Subscribe();

        this.WhenAnyValue(x => x.SelectedHookEngine)
            .WhereNotNull()
            // Use Do() may be bad approach
            .Do(_ => hookThreadItemsList.Clear())
            .SelectMany(_ => hookThreads.Items.ToObservable())
            .Where(v => v.Address == SelectedHookEngine!.Value.Address)
            .Select(v => new HookThreadItemViewModel()
            {
                Index = hookThreadItemsList.Count + 1,
                Handle = v.Handle,
                TotalText = v.LatestText,
                HookCode = v.HookCode,
                EngineName = v.EngineName,
                Context = v.ThreadContext,
                SubContext = v.SubThreadContext
            })
            .Subscribe(hookThreadItemsList.AddOrUpdate);

        textractorService.Data
            .Where(v => SelectedHookEngine is not null &&
                v.Address == SelectedHookEngine!.Value.Address &&
                !hookThreadItemsList.Items.Any(m => m.Handle == v.Handle))
            .Select(v => new HookThreadItemViewModel()
            {
                Index = hookThreadItemsList.Count + 1,
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

        var canSubmit = _hookThreadItems
            .ToObservableChangeSet()
            .AutoRefresh(m => m.IsTextThread)
            .ToCollection()
            .Select(vms => vms.Any(m => m.IsTextThread) == true);

        Submit = ReactiveCommand.Create(() => CurrentInUseHookName =
            SubmitSetting(textractorService, gameInfoRepository, hookThreadItemsList.Items), canSubmit);
    }

    [Reactive]
    public string CurrentInUseHookName { get; set; }

    [ObservableAsProperty]
    public Color CurrentInUseHookColor { get; }

    [Reactive]
    public string ConsoleInfo { get; set; }

    public ReactiveCommand<Unit, Unit> ReInject { get; }

    public ReactiveCommand<Unit, string> OpenHCodeDialog { get; }

    // No memory leak, but would like to exist for a while
    private readonly ReadOnlyObservableCollection<HookEngineLabel> _hookEngineNames;
    public ReadOnlyObservableCollection<HookEngineLabel> HookEngineNames => _hookEngineNames;

    [Reactive]
    public HookEngineLabel? SelectedHookEngine { get; set; }

    public ReactiveCommand<Unit, Unit> RemoveHook { get; }

    private readonly ReadOnlyObservableCollection<HookThreadItemViewModel> _hookThreadItems;
    public ReadOnlyObservableCollection<HookThreadItemViewModel> HookThreadItems => _hookThreadItems;

    public ReactiveCommand<Unit, string> Submit { get; }

    /// <returns>HookName</returns>
    private static string SubmitSetting(
        ITextractorService textractorService,
        IGameInfoRepository gameInfoRepository,
        IEnumerable<HookThreadItemViewModel> hookThreadItemViewModels)
    {
        // Remove useless hooks except selected one
        // _textractorService.RemoveUselessHooks();

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

        // Post to server except Search code

        // Update to local database
        var setting = JsonSerializer.Serialize(textractorSetting);
        gameInfoRepository.UpdateTextractorSetting(setting);
        // And RegExp

        // Refresh current text in TextWindow

        // Toast
        Interactions.ContentDialog
            .Handle(Strings.HookPage_SubmitDialogMessage)
            .Subscribe();

        return textractorSetting.HookName;
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
