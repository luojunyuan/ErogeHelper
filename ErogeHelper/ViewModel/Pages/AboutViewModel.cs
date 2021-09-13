using ErogeHelper.Common;
using ErogeHelper.Common.Contracts;
using ErogeHelper.ViewModel.Routing;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UpdateChecker;
using UpdateChecker.VersionComparers;

namespace ErogeHelper.ViewModel.Pages
{
    public class AboutViewModel : ReactiveObject, IRoutableViewModel, IEnableLogger
    {
        private string _checkUpdateStatus = "Check update ...";
        private Brush _brushColor = Brushes.Gray;
        private bool _canJumpRelease;

        public IScreen HostScreen { get; }
        public AboutViewModel(IPreferenceScreen? hostScreen = null)
        {
            HostScreen = hostScreen ?? DependencyInject.GetService<IPreferenceScreen>();

            CheckUpdateAsync().ConfigureAwait(false);
        }
        public Brush BrushColor
        { get => _brushColor; set => this.RaiseAndSetIfChanged(ref _brushColor, value); }
        public string CheckUpdateStatus
        { get => _checkUpdateStatus; set => this.RaiseAndSetIfChanged(ref _checkUpdateStatus, value); }
        public bool CanJumpRelease
        { get => _canJumpRelease; set => this.RaiseAndSetIfChanged(ref _canJumpRelease, value); }

        public string? UrlPathSegment => PageTags.About;


        private async Task CheckUpdateAsync()
        {
            var version = EhContext.AppVersion;
            var updateChecker = new GitHubReleasesUpdateChecker(
                    @"luojunyuan", // Owner
                    @"ErogeHelper", // Repo
                    false,  // Is pre-release
                    version ?? "?.?.?.?", // Current app version string
                    tag => tag.Replace(@"v", string.Empty), // Tag to version string
                    new DefaultVersionComparer() // Version comparer
            );
            try
            {
                var responseOk = await updateChecker.CheckAsync(default);
                if (responseOk)
                {
                    // Update Found
                    var latestVersion = updateChecker.LatestVersion;
                    //var latestVersionUrl = updateChecker.LatestVersionUrl;
                    //var assetsUrl = updateChecker.LatestRelease?.assets?.Select(asset => asset.browser_download_url);
                    this.Log().Debug($"Found new {latestVersion}");

                    CheckUpdateStatus = $"Found new version {latestVersion}";
                    BrushColor = Brushes.Orange;
                    CanJumpRelease = true;
                    // ToolTip: Click me jump to release page
                }
                else
                {
                    CheckUpdateStatus = "Latest version";
                    BrushColor = Brushes.Green;
                }
            }
            catch (HttpRequestException ex)
            {
                CheckUpdateStatus = "Check Failed";
                BrushColor = Brushes.Red;
                this.Log().Debug(ex.Message);
            }
            catch (TaskCanceledException ex)
            {
                this.Log().Debug(ex.Message);
            }
            catch (Exception ex)
            {
                // Network exception or cannot find any correct tag.
                this.Log().Error(ex);
            }
        }
    }
}
