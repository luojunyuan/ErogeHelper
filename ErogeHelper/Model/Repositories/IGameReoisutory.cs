using System.ComponentModel;
using Config.Net;

namespace ErogeHelper.Model.Repositories;

public interface IGameInfoRepository : INotifyPropertyChanged
{
    [Option(DefaultValue = "")]
    public string Md5 { get; set; }

    [Option(DefaultValue = "{}")]
    public string TextractorSettingJson { get; set; }

    [Option(DefaultValue = "")]
    public string RegExp { get; set; }

    [Option(DefaultValue = false)]
    public bool IsLoseFocus { get; set; }

    [Option(DefaultValue = false)]
    public bool IsEnableTouchToMouse { get; set; }

    [Option(DefaultValue = false)]
    public bool UseClipboard { get; set; }

    [Option(DefaultValue = "1/1/0001 12:00:00 AM")]
    public string CommentLastSyncTime { get; set; }
}
