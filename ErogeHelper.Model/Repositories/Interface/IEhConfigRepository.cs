using System.ComponentModel;
using Config.Net;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.Shared.Enums;

namespace ErogeHelper.Model.Repositories.Interface;

public interface IEHConfigRepository : INotifyPropertyChanged
{
    [Option(DefaultValue = "https://eh.nya.run")]
    public string ServerBaseUrl { get; set; }

    // TextWindow 関する
    [Option(DefaultValue = false)]
    public bool TextWindowBlur { get; set; }

    [Option(DefaultValue = 1.0)] // Between 0.2 - 2
    public double TextWindowWidthScale { get; set; }

    [Option(DefaultValue = 800.0)]
    public double TextWindowWidth { get; set; }

    [Option(DefaultValue = 0.7)]
    public double TextWindowOpacity { get; set; }

    [Option(DefaultValue = false)]
    public bool HideTextWindow { get; set; }

    // Text 関する
    [Option(DefaultValue = 120)]
    public int MaxAcceptTextLength { get; set; }

    [Option(DefaultValue = 36.0)]
    public double FontSize { get; set; }

    [Option(DefaultValue = false)]
    public bool EnableMeCab { get; set; }

    // MISC
    [Option(DefaultValue = true)]
    public bool InjectProcessByDefault { get; set; }

    [Option(DefaultValue = 600.0)]
    public double PreferenceWindowHeight { get; set; }

    [Option(DefaultValue = 600.0)]
    public double PreferenceWindowWidth { get; set; }


    [Option(DefaultValue = ConstantValue.DefaultAssistiveTouchPositionStr)]
    public string AssistiveTouchPosition { get; set; }

    [Option(DefaultValue = false)]
    public bool UseBigAssistiveTouchSize { get; set; }

    [Option(DefaultValue = false)]
    public bool MonitorClipboard { get; set; }

    [Option(DefaultValue = false)]
    public bool PasteToDeepL { get; set; }

    [Option(DefaultValue = false)]
    public bool UseDanmaku { get; set; }

    [Option(DefaultValue = "")]
    public string ExternalSharedDrivePath { get; set; }

    [Option(DefaultValue = false)]
    public bool DPICompatibilityByApplication { get; set; }

    [Option(DefaultValue = false)]
    public bool UseEdgeTouchMask { get; set; }

    [Option(DefaultValue = false)]
    public bool UseTouchToolBox { get; set; }

    [Option(DefaultValue = false)]
    public bool UpdatePreviewVersion { get; set; }

    // Text 模様
    [Option(DefaultValue = TextTemplate.OutLineKanaTop)]
    public TextTemplate TextTemplateConfig { get; set; }

    [Option(DefaultValue = KanaPosition.Top)]
    public KanaPosition KanaPosition { get; set; }

    [Option(DefaultValue = KanaRuby.Hiragana)]
    public KanaRuby KanaRuby { get; set; }

    // Dictionaries
    [Option(DefaultValue = false)]
    public bool MojiDictEnable { get; set; }

    // Account: erogehelper@github.com
    // Password: ******
    [Option(DefaultValue = "r:02625329e868e96b5eb65bca9dead47e")]
    public string MojiSessionToken { get; set; }

    [Option(DefaultValue = false)]
    public bool JishoDictEnable { get; set; }


    public TransLanguage SrcTransLanguage { get; set; }

    public TransLanguage TargetTransLanguage { get; set; }

    #region Translators

    [Option(Alias = "Translators.BaiduApiEnable", DefaultValue = false)]
    public bool BaiduApiEnable { get; set; }

    [Option(Alias = "Translators.BaiduApiAppid", DefaultValue = "")]
    public string BaiduApiAppid { get; set; }

    [Option(Alias = "Translators.BaiduApiSecretKey", DefaultValue = "")]
    public string BaiduApiSecretKey { get; set; }

    [Option(Alias = "Translators.YeekitEnable", DefaultValue = false)]
    public bool YeekitEnable { get; set; }

    [Option(Alias = "Translators.BaiduWebEnable", DefaultValue = false)]
    public bool BaiduWebEnable { get; set; }

    [Option(Alias = "Translators.CaiyunEnable", DefaultValue = false)]
    public bool CaiyunEnable { get; set; }

    [Option(Alias = "Translators.CaiyunToken", DefaultValue = "3975l6lr5pcbvidl6jl2")]
    public string CaiyunToken { get; set; }

    [Option(Alias = "Translators.AlapiEnable", DefaultValue = false)]
    public bool AlapiEnable { get; set; }

    [Option(Alias = "Translators.YoudaoEnable", DefaultValue = false)]
    public bool YoudaoEnable { get; set; }

    [Option(Alias = "Translators.NiuTransEnable", DefaultValue = false)]
    public bool NiuTransEnable { get; set; }

    [Option(Alias = "Translators.NiuTransApiKey", DefaultValue = "")]
    public string NiuTransApiKey { get; set; }

    [Option(Alias = "Translators.GoogleCnEnable", DefaultValue = false)]
    public bool GoogleCnEnable { get; set; }

    [Option(Alias = "Translators.TencentMtEnable", DefaultValue = false)]
    public bool TencentMtEnable { get; set; }

    [Option(Alias = "Translators.TencentMtSecretId", DefaultValue = "")]
    public string TencentMtSecretId { get; set; }

    [Option(Alias = "Translators.TencentMtSecretKey", DefaultValue = "")]
    public string TencentMtSecretKey { get; set; }

    [Option(Alias = "Translators.CloudEnable", DefaultValue = false)]
    public bool CloudEnable { get; set; }

    #endregion
}

