using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using AutoUpdaterDotNET;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.Shared.Languages;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.Preference;

public class AboutViewModel : ReactiveObject, IRoutableViewModel, IActivatableViewModel
{
    public IScreen HostScreen => throw new NotImplementedException();

    public string UrlPathSegment => PageTag.About;

    public ViewModelActivator Activator => new();

    public AboutViewModel(IUpdateService? updateService = null)
    {
        updateService ??= DependencyResolver.GetService<IUpdateService>();

        var disposables = new CompositeDisposable();

        var currentPreviewFlag = false;

        var updateVMSubj = new Subject<(string tip, Color versionColor, bool canUpdate)>();
        updateVMSubj.Select(pack => pack.tip).ToPropertyEx(this, x => x.UpdateStatusTip);
        updateVMSubj.Select(pack => pack.versionColor).ToPropertyEx(this, x => x.VersionBrushColor);
        updateVMSubj.Select(pack => pack.canUpdate).ToPropertyEx(this, x => x.ShowUpdateButton);

        this.WhenAnyValue(x => x.ShowUpdateButton)
            .ToPropertyEx(this, x => x.CanJumpRelease);

        CheckUpdate = ReactiveCommand.CreateFromObservable<bool, (string, Color, bool)>(isAcceptedPreview =>
        {
            updateVMSubj.OnNext((Strings.About_CheckingVersion, Color.Gray, false));
            currentPreviewFlag = isAcceptedPreview;
            return updateService.CheckUpdate(AppVersion, isAcceptedPreview);
        });
        CheckUpdate.Subscribe(pack => updateVMSubj.OnNext(pack)).DisposeWith(disposables);

        Update = ReactiveCommand.Create(() => DoUpdate(currentPreviewFlag));

        Observable
            .FromEvent<AutoUpdater.CheckForUpdateEventHandler, UpdateInfoEventArgs>(
                e => AutoUpdater.CheckForUpdateEvent += e,
                e => AutoUpdater.CheckForUpdateEvent -= e)
            .Where(updateInfo => updateInfo.Error is null)
            // XXX: Memory leak here
            .Where(updateInfo =>
            {
                if (!updateInfo.IsUpdateAvailable)
                {
                    return Interactions.MessageBoxConfirm
                        .Handle("Update not availble now. Please try later, or you can update from release page.")
                        .Wait();
                }
                var updateTip = currentPreviewFlag
                    ? string.Format(Strings.About_Update_Tip, updateInfo.CurrentVersion)
                      + Strings.About_Update_PreviewWarning
                    : string.Format(Strings.About_Update_Tip, updateInfo.CurrentVersion);
                // PREFERENCE: better surface with update log info
                return Interactions.MessageBoxConfirm
                    .Handle(updateTip)
                    .Wait();
            })
            .Select(AutoUpdater.DownloadUpdate)
            .Where(downloadFinished => downloadFinished)
            .Subscribe(_ =>
            {
                // UNDONE: May memory leak
                AppExit.Handle(Unit.Default).Subscribe();

                if (Utils.IsFileInUse(
                        Path.Combine(AppContext.BaseDirectory, "ErogeHelper.ShellMenuHandler.dll")))
                {
                    Process.GetProcessesByName("explorer").ToList()
                        .ForEach(p => p.Kill());
                    Process.Start("explorer");
                }
            })
            .DisposeWith(disposables);

        this.WhenActivated(d => d(disposables));
    }

    public string AppVersion { get; set; } = string.Empty;

    [ObservableAsProperty]
    public Color VersionBrushColor { get; set; }

    [ObservableAsProperty]
    public string? UpdateStatusTip { get; set; }

    [ObservableAsProperty]
    public bool ShowUpdateButton { get; set; }

    [ObservableAsProperty]
    public bool CanJumpRelease { get; }

    public ReactiveCommand<bool, (string, Color, bool)> CheckUpdate { get; }

    public ReactiveCommand<Unit, Unit> Update { get; }

    public Interaction<Unit, Unit> AppExit { get; set; } = new();

    private static void DoUpdate(bool previewVersion)
    {
        AutoUpdater.Proxy = WebRequest.DefaultWebProxy;
        AutoUpdater.RunUpdateAsAdmin = false;
        AutoUpdater.InstallationPath = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.FullName;
        var architecture = RuntimeInformation.ProcessArchitecture;

        if (!previewVersion)
        {
            if (architecture == Architecture.X86)
                AutoUpdater.Start(x86_32);
            else if (architecture == Architecture.X64)
            {
                AutoUpdater.Start(x86_64);
            }
            else if (architecture == Architecture.Arm64)
            {
                AutoUpdater.Start(arm64);
            }
        }
        else
        {
            if (architecture == Architecture.X86)
                AutoUpdater.Start(x86_32_preview);
            else if (architecture == Architecture.X64)
            {
                AutoUpdater.Start(x86_64_preview);
            }
            else if (architecture == Architecture.Arm64)
            {
                AutoUpdater.Start(arm64_preview);
            }
        }
    }

    private const string UpdateInfoPrefix = "https://cdn.jsdelivr.net/gh/luojunyuan/FreeJsdelivrUpdateInfo/";
    private const string x86_64 = UpdateInfoPrefix + "x86_64.xml";
    private const string x86_32 = UpdateInfoPrefix + "x86_32.xml";
    private const string arm64 = UpdateInfoPrefix + "arm64.xml";
    private const string x86_64_preview = UpdateInfoPrefix + "x86_64_preview.xml";
    private const string x86_32_preview = UpdateInfoPrefix + "x86_32_preview.xml";
    private const string arm64_preview = UpdateInfoPrefix + "arm64_preview.xml";
}
