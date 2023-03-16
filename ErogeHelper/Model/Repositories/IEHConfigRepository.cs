using System.ComponentModel;
using Config.Net;

namespace ErogeHelper.Model.Repositories;

public interface IEHConfigRepository : INotifyPropertyChanged
{
    public const string AssistiveTouchDefaultPosition = "{\"Corner\":0,\"Scale\":0.5}";

    [Option(DefaultValue = false)]
    public bool AssistiveTouchBig { get; set; }

    [Option(DefaultValue = AssistiveTouchDefaultPosition)]
    public string AssistiveTouchPosition { get; set; }

    // Function options

    [Option(DefaultValue = false)]
    public bool DPICompatibilityByApplication { get; set; }

    [Option(DefaultValue = false)]
    public bool StartupInjectProcess { get; set; }

    [Option(DefaultValue = false)]
    public bool UseEdgeTouchMask { get; set; }

    [Option(DefaultValue = false)]
    public bool ShowTextWindow { get; set; }


    [Option(DefaultValue = "0 0 400 50")]
    public string MagSourceInputString { get; set; }

    [Option(DefaultValue = false)]
    public bool MagSmoothing { get; set; }
}
