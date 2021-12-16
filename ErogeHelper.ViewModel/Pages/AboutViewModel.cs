using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using AutoUpdaterDotNET;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.Shared.Languages;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.Pages;

public class AboutViewModel : ReactiveObject, IRoutableViewModel, IDisposable
{
    public IScreen HostScreen => throw new NotImplementedException();

    public string UrlPathSegment => PageTag.About;

    public AboutViewModel(
        IEHConfigRepository? ehConfigRepository = null,
        IUpdateService? updateService = null)
    {
        ehConfigRepository ??= DependencyResolver.GetService<IEHConfigRepository>();
        updateService ??= DependencyResolver.GetService<IUpdateService>();

        VersionBrushColor = Color.Gray;
        UpdateStatusTip = Strings.About_CheckingVersion;
        ShowUpdateButton = false;

        this.WhenAnyValue(x => x.ShowUpdateButton)
            .ToPropertyEx(this, x => x.CanJumpRelease);

        var currentPreviewFlag = AcceptedPreviewVersion = ehConfigRepository.UpdatePreviewVersion;
        this.WhenAnyValue(x => x.AcceptedPreviewVersion)
            .Skip(1)
            .Subscribe(v => ehConfigRepository.UpdatePreviewVersion = v);

        CheckUpdate = ReactiveCommand.CreateFromObservable(() =>
        {
            VersionBrushColor = Color.Gray;
            UpdateStatusTip = Strings.About_CheckingVersion;
            ShowUpdateButton = false;

            currentPreviewFlag = AcceptedPreviewVersion;
            return updateService.CheckUpdate(AppVersion, currentPreviewFlag);
        });
        CheckUpdate
            .Subscribe(package => (UpdateStatusTip, VersionBrushColor, ShowUpdateButton) = package)
            .DisposeWith(_disposables);

        Update = ReactiveCommand.Create(() => DoUpdate(currentPreviewFlag));

        Observable
            .FromEvent<AutoUpdater.CheckForUpdateEventHandler, UpdateInfoEventArgs>(
                e => AutoUpdater.CheckForUpdateEvent += e,
                e => AutoUpdater.CheckForUpdateEvent -= e)
            .Where(updateInfo => updateInfo.Error is null)
            .SelectMany(updateInfo =>
            {
                if (!updateInfo.IsUpdateAvailable)
                {
                    return Interactions.MessageBoxConfirm.Handle("Update not availble now. Please try later, or you can update from release page.")
                        .Where(_ => false)
                        .Select(_ => updateInfo);
                }
                var updateTip = currentPreviewFlag
                    ? string.Format(Strings.About_Update_Tip, updateInfo.CurrentVersion)
                      + Strings.About_Update_PreviewWarning
                    : string.Format(Strings.About_Update_Tip, updateInfo.CurrentVersion);
                // PREFERENCE: better surface with update log info
                return Interactions.MessageBoxConfirm.Handle(updateTip)
                .Where(continueUpdate => continueUpdate)
                .Select(_ => updateInfo);

            })
            .Select(AutoUpdater.DownloadUpdate)
            .Where(downloadFinished => downloadFinished)
            .Subscribe(_ =>
            {
                Interactions.TerminateApp.Handle(Unit.Default).Subscribe();

                if (Utils.IsFileInUse(
                        Path.Combine(AppContext.BaseDirectory, "ErogeHelper.ShellMenuHandler.dll")))
                {
                    Process.GetProcessesByName("explorer").ToList()
                        .ForEach(p => p.Kill());
                    Process.Start("explorer");
                }
            })
            .DisposeWith(_disposables);
    }

    public string AppVersion { get; set; } = string.Empty;

    [Reactive]
    public Color VersionBrushColor { get; set; }

    [Reactive]
    public string UpdateStatusTip { get; set; }

    [Reactive]
    public bool ShowUpdateButton { get; set; }

    [ObservableAsProperty]
    public bool CanJumpRelease { get; } = default;

    [Reactive]
    public bool AcceptedPreviewVersion { get; set; }

    public ReactiveCommand<Unit, (string tip, Color versionColor, bool canUpdate)> CheckUpdate { get; }

    public ReactiveCommand<Unit, Unit> Update { get; }

    private static void DoUpdate(bool previewVersion)
    {
        AutoUpdater.Proxy = WebRequest.DefaultWebProxy;
        AutoUpdater.RunUpdateAsAdmin = false;
        AutoUpdater.InstallationPath = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.FullName;
        var architecture = RuntimeInformation.ProcessArchitecture;

        if (!previewVersion)
        {
            if (architecture == Architecture.X86)
            {
                AutoUpdater.Start(x86_32);
            }
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
            {
                AutoUpdater.Start(x86_32_preview);
            }
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

    private readonly CompositeDisposable _disposables = new();
    public void Dispose() => _disposables.Dispose();

    private const string UpdateInfoPrefix = "https://cdn.jsdelivr.net/gh/luojunyuan/FreeJsdelivrUpdateInfo/";
    private const string x86_64 = UpdateInfoPrefix + "x86_64.xml";
    private const string x86_32 = UpdateInfoPrefix + "x86_32.xml";
    private const string arm64 = UpdateInfoPrefix + "arm64.xml";
    private const string x86_64_preview = UpdateInfoPrefix + "x86_64_preview.xml";
    private const string x86_32_preview = UpdateInfoPrefix + "x86_32_preview.xml";
    private const string arm64_preview = UpdateInfoPrefix + "arm64_preview.xml";
}
