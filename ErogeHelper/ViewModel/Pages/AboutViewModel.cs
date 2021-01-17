using Caliburn.Micro;
using ErogeHelper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using UpdateChecker;
using UpdateChecker.VersionComparers;

namespace ErogeHelper.ViewModel.Pages
{
    class AboutViewModel : PropertyChangedBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(AboutViewModel));
        private string _checkUpdateStatus = "Check update ...";
        private Brush _brushColor = Brushes.Gray;
        private bool _canJumpRelease = false;

        public AboutViewModel()
        {
            Task.Run(CheckUpdateAsync);
        }

        public Brush BrushColor { get => _brushColor; set { _brushColor = value; NotifyOfPropertyChange(() => BrushColor); } }
        public string CheckUpdateStatus { get => _checkUpdateStatus; set { _checkUpdateStatus = value; NotifyOfPropertyChange(() => CheckUpdateStatus); } }
        public bool CanJumpRelease { get => _canJumpRelease; set { _canJumpRelease = value; NotifyOfPropertyChange(() => CanJumpRelease); } }
        private async Task CheckUpdateAsync()
        {
            var version = DataRepository.AppVersion;
            var updateChecker = new GitHubReleasesUpdateChecker(
                    @"luojunyuan", // Owner
                    @"Eroge-Helper", // Repo
                    false,  // Is pre-release
                    version, // Current app version string
                    tag => tag.Replace(@"v", string.Empty), // Tag to version string
                    new DefaultVersionComparer() // Version comparer
            );
            try
            {
                var res = await updateChecker.CheckAsync(default);
                //var res = await updaterChecker.CheckAsync(new HttpClient(), new CancellationToken());
                if (res)
                {
                    // Update Found
                    var latestVersion = updateChecker.LatestVersion;
                    var latestVersionUrl = updateChecker.LatestVersionUrl;
                    var assetsUrl = updateChecker.LatestRelease?.assets?.Select(asset => asset.browser_download_url);
                    log.Info($"Found new {latestVersion}");

                    CheckUpdateStatus = $"Found new version {latestVersion}";
                    BrushColor = Brushes.Orange;
                    CanJumpRelease = true;
                    // ToolTip: Click me jump to release page
                }
                else
                {
                    // No newer version was found
                    log.Info("Current version is latest");

                    CheckUpdateStatus = "Latest version";
                    BrushColor = Brushes.Green;
                }
            }
            catch (Exception ex)
            {
                // Network exception or cannot find any correct tag.
                log.Error(ex);
            }
        }
    }
}
