using System.Drawing;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Shared.Languages;
using Splat;
using UpdateChecker;
using UpdateChecker.VersionComparers;

namespace ErogeHelper.Model.Repositories;

public class UpdateService : IUpdateService, IEnableLogger
{
    public IObservable<(string tip, Color versionColor, bool canUpdate)>
        CheckUpdate(string version, bool usePreviewVersion)
    {
        var updateChecker = new GitHubReleasesUpdateChecker(
            "erogehelper",
            "erogehelper",
            usePreviewVersion,
            version,
            tag => tag.Replace(@"v", string.Empty), // Tag to version string
            new DefaultVersionComparer() // Version comparer
        );

        return Observable.Create<(string tip, Color versionColor, bool canUpdate)>(async observable =>
        {
            try
            {
                var findNewVersion = await updateChecker.CheckAsync(default).ConfigureAwait(false);

                if (findNewVersion)
                {
                    var latestVersion = updateChecker.LatestVersion;

                    observable.OnNext((string.Format(Strings.About_NewVersion, latestVersion), Color.Orange, true));
                }
                else
                {
                    var latestVersion = updateChecker.LatestVersion ?? "9.9.9.9";
                    if (usePreviewVersion)
                        observable.OnNext((Strings.About_PreviewLatestVersion, Color.Purple, false));
                    else if (new Version(version) > new Version(latestVersion))
                    {
                        observable.OnNext((Strings.About_PreviewVersion, Color.Purple, false));
                    }
                    else
                    {
                        observable.OnNext((Strings.About_LatestVersion, Color.Green, false));
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                this.Log().Debug(ex.Message);
                observable.OnNext((Strings.About_CheckingFailed, Color.Red, false));
            }
            catch (Exception ex)
            {
                // Network exception or cannot find any correct tag.
                this.Log().Error(ex);
                observable.OnNext(("Check Failed", Color.Red, false));
            }

            observable.OnCompleted();

            return Disposable.Empty;
        });
    }
}
