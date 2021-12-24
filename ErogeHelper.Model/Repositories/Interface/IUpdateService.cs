using System.Drawing;

namespace ErogeHelper.Model.Repositories.Interface;

public interface IUpdateService
{
    IObservable<(string tip, Color versionColor, bool canUpdate)> CheckUpdate(string version, bool previewVersion);
}
