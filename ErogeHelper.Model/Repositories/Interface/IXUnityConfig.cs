using Config.Net;

namespace ErogeHelper.Model.Repositories.Interface;

public interface IXUnityConfig
{
    [Option(Alias = "Service.Endpoint")]
    string Endpoint { get; set; }

    [Option(Alias = "Custom.Url")]
    string Url { get; set; }

    [Option(Alias = "Behaviour.CopyToClipboard")]
    bool CopyToClipboard { get; set; }

    [Option(Alias = "Behaviour.ClipboardDebounceTime")]
    double ClipboardDebounceTime { get; set; }

    [Option(Alias = "Behaviour.FallbackFontTextMeshPro")]
    string FallbackFontTextMeshPro { get; set; }
}
