using Config.Net;
using ErogeHelper.Common.Contracts;
using ErogeHelper.Common.Entities;
using ErogeHelper.Common.Enums;
using System.Text.Json;

namespace ErogeHelper.Model.Repositories.Interface
{
    public interface IEhConfigRepository
    {
        [Option(DefaultValue = "https://eh.nya.run")]
        public string EhServerBaseUrl { get; set; }

        // Text 関する
        [Option(DefaultValue = false)]
        public bool UseOuterTextWindow { get; set; }

        [Option(DefaultValue = 120)]
        public int MaxAcceptTextLength { get; set; }

        [Option(DefaultValue = 28.0)]
        public double FontSize { get; set; }

        [Option(DefaultValue = false)]
        public bool EnableMeCab { get; set; }

        // MISC
        [Option(DefaultValue = ConstantValues.DefaultAssistiveTouchPositionStr)]
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
        string ExternalSharedDrivePath { get; set; }

        // Text 模様
        [Option(DefaultValue = TextTemplateType.OutLineKanaTop)]
        public TextTemplateType TextTemplateConfig { get; set; }

        [Option(DefaultValue = KanaPositionType.Top)]
        public KanaPositionType KanaPosition { get; set; }

        [Option(DefaultValue = KanaRubyType.Hiragana)]
        public KanaRubyType KanaRuby { get; set; }

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

        // Translators
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
    }
}
