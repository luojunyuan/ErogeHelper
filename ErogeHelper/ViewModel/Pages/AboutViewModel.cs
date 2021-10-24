using AutoUpdaterDotNET;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Model.Repositories.Interface;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows;
using UpdateChecker;
using UpdateChecker.VersionComparers;

namespace ErogeHelper.ViewModel.Pages
{
    public class AboutViewModel : ReactiveObject, IRoutableViewModel, IEnableLogger, IDisposable
    {
        public IScreen HostScreen { get; }

        public string? UrlPathSegment => PageTags.About;

        public AboutViewModel(
            IScreen? hostScreen = null,
            IEhConfigRepository? ehConfigRepository = null)
        {
            ehConfigRepository ??= DependencyResolver.GetService<IEhConfigRepository>();

            HostScreen = hostScreen!;

            CheckUpdateCommand = ReactiveCommand.CreateFromTask(CheckUpdateAsync);

            AcceptPreviewVersion = ehConfigRepository.UpdatePreviewVersion;
            this.WhenAnyValue(x => x.AcceptPreviewVersion)
                .Skip(1)
                .Throttle(TimeSpan.FromMilliseconds(ConstantValues.UserOperationDelay))
                .DistinctUntilChanged()
                .Subscribe(v => ehConfigRepository.UpdatePreviewVersion = v);

            UpdateCommand = ReactiveCommand.Create(() =>
            {
                AutoUpdater.Proxy = WebRequest.DefaultWebProxy;
                AutoUpdater.RunUpdateAsAdmin = false;
                var architecture = typeof(string).Assembly.GetName().ProcessorArchitecture;
                if (!_currentButtonIsForPreview)
                {
                    if (architecture == System.Reflection.ProcessorArchitecture.X86)
                    {
                        AutoUpdater.Start(x86_32);
                    }
                    else if (architecture == System.Reflection.ProcessorArchitecture.Amd64)
                    {
                        AutoUpdater.Start(x86_64);
                    }
                    else if (architecture == System.Reflection.ProcessorArchitecture.Arm)
                    {
                        AutoUpdater.Start(arm64);
                    }
                }
                else
                {
                    if (architecture == System.Reflection.ProcessorArchitecture.X86)
                    {
                        AutoUpdater.Start(x86_32_preview);
                    }
                    else if (architecture == System.Reflection.ProcessorArchitecture.Amd64)
                    {
                        AutoUpdater.Start(x86_64_preview);
                    }
                    else if (architecture == System.Reflection.ProcessorArchitecture.Arm)
                    {
                        AutoUpdater.Start(arm64_preview);
                    }
                }
            });

            _updateObservable = Observable
                .FromEvent<AutoUpdater.CheckForUpdateEventHandler, UpdateInfoEventArgs>(
                    e => AutoUpdater.CheckForUpdateEvent += e,
                    e => AutoUpdater.CheckForUpdateEvent -= e)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(updateInfo => updateInfo.Error is null)
                .Subscribe(updateInfo =>
                {
                    if (updateInfo.IsUpdateAvailable)
                    {
                        var updateTip = $@"There is new version {updateInfo.CurrentVersion} available. You are using version {updateInfo.InstalledVersion}. Do you want to update the application now? (if it said Locate Emulator is in using, Please close the game, the click try again)";
                        if (_currentButtonIsForPreview)
                        {
                            updateTip += "\n NOTE: You about updating to dev version. This version may not stable only suit for advance users";
                        }
                        var dialogResult = ModernWpf.MessageBox.Show(
                            updateTip, 
                            @"Update Available",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        if (dialogResult.Equals(MessageBoxResult.Yes) || dialogResult.Equals(MessageBoxResult.OK))
                        {
                            try
                            {
                                if (AutoUpdater.DownloadUpdate(updateInfo))
                                {
                                    // New version ready
                                    if (Environment.GetCommandLineArgs().Any(arg => arg is "/le" or "-le"))
                                    {
                                        ModernWpf.MessageBox.Show("Please save and close your game to finish the update", "Eroge Helper");
                                    }
                                    else
                                    {
                                        _terminateAppSubj.OnNext(Unit.Default);
                                        _terminateAppSubj.OnCompleted();
                                    }

                                    if (Utils.IsFileInUse(
                                        Path.Combine(AppContext.BaseDirectory, "ErogeHelper.ShellMenuHandler.dll")))
                                    {
                                        Process.GetProcessesByName("explorer").ToList()
                                            .ForEach(p => p.Kill());
                                        Process.Start("explorer");
                                    }
                                }
                            }
                            catch (Exception exception)
                            {
                                ModernWpf.MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                            }
                        }
                    }
                    else
                    {
                        ModernWpf.MessageBox.Show(@"No update available: There is no newly update file available please try again later.");
                    }
                });
        }

        private readonly Subject<Unit> _terminateAppSubj = new();
        public IObservable<Unit> TerminateAppSubj => _terminateAppSubj;

        [Reactive]
        public bool ShowUpdateButton { get; set; }

        [Reactive]
        public System.Windows.Media.Brush VersionBrushColor { get; set; } = System.Windows.Media.Brushes.Gray;

        [Reactive]
        public string UpdateStatusTip { get; set; } = "Check update ...";

        [Reactive]
        public bool CanJumpRelease { get; set; }

        [Reactive]
        public bool AcceptPreviewVersion { get; set; }

        public ReactiveCommand<Unit, Unit> CheckUpdateCommand { get; }

        public ReactiveCommand<Unit, Unit> UpdateCommand { get; }

        private bool _currentButtonIsForPreview;

        private async Task CheckUpdateAsync()
        {
            ShowUpdateButton = false;
            VersionBrushColor = System.Windows.Media.Brushes.Gray;
            UpdateStatusTip = "Check update ...";
            _currentButtonIsForPreview = AcceptPreviewVersion;

            var version = EhContext.AppVersion;
            var updateChecker = new GitHubReleasesUpdateChecker(
                @"luojunyuan",
                @"Eroge-Helper",
                _currentButtonIsForPreview,
                version,
                tag => tag.Replace(@"v", string.Empty), // Tag to version string
                new DefaultVersionComparer() // Version comparer
            );
            try
            {
                var responseOk = await updateChecker.CheckAsync(default).ConfigureAwait(true);
                if (responseOk)
                {
                    var latestVersion = updateChecker.LatestVersion;

                    UpdateStatusTip = $"Found new version {latestVersion}";
                    VersionBrushColor = System.Windows.Media.Brushes.Orange;
                    CanJumpRelease = true;
                    ShowUpdateButton = true;
                }
                else
                {
                    ShowUpdateButton = false;
                    var latestVersion = updateChecker.LatestVersion ?? "9.9.9.9";
                    if (_currentButtonIsForPreview)
                    {
                        UpdateStatusTip = "Latest preview version";
                        VersionBrushColor = System.Windows.Media.Brushes.Purple;
                    }
                    else if (new Version(version) > new Version(latestVersion))
                    {
                        UpdateStatusTip = "preview version";
                        VersionBrushColor = System.Windows.Media.Brushes.Purple;
                    }
                    else
                    {
                        UpdateStatusTip = "Latest version";
                        VersionBrushColor = System.Windows.Media.Brushes.Green;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                UpdateStatusTip = "No Internet";
                VersionBrushColor = System.Windows.Media.Brushes.Red;
                this.Log().Debug(ex.Message);
            }
            catch (Exception ex)
            {
                // Network exception or cannot find any correct tag.
                UpdateStatusTip = "Check Failed";
                VersionBrushColor = System.Windows.Media.Brushes.Red;
                this.Log().Error(ex);
            }
        }

        private readonly IDisposable _updateObservable;

        public void Dispose()
        {
            _updateObservable.Dispose();
        }

        private const string UpdateInfoPrefix = "https://cdn.jsdelivr.net/gh/luojunyuan/FreeJsdelivrUpdateInfo/";
        private const string x86_64 = UpdateInfoPrefix + "x86_64.xml";
        private const string x86_32 = UpdateInfoPrefix + "x86_32.xml";
        private const string arm64 = UpdateInfoPrefix + "arm64.xml";
        private const string x86_64_preview = UpdateInfoPrefix + "x86_64_preview.xml";
        private const string x86_32_preview = UpdateInfoPrefix + "x86_32_preview.xml";
        private const string arm64_preview = UpdateInfoPrefix + "arm64_preview.xml";
    }
}
