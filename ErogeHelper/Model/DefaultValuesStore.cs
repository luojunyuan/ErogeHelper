using ErogeHelper.Common.Selector;
using ErogeHelper.Model.Translator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Model
{
    internal static class DefaultValuesStore
    {
        public const double FontSize = 28;

        public const bool EnableMecab = false;

        public const bool ShowAppendText = true;

        public const bool PasteToDeepL = false;

        public const TextTemplateType TextTemplate = TextTemplateType.OutLineKanaTop;

        public const bool KanaDefault = false;
        public const bool KanaTop = true;
        public const bool KanaBottom = false;

        public const bool Romaji = false;
        public const bool Hiragana = true;
        public const bool Katakana = false;

        // Account: erogehelper@github.com
        // Password: erogehelper
        public const string MojiSessionToken = "r:02625329e868e96b5eb65bca9dead47e";
        public const bool MojiDictEnable = false;
        public const bool JishoDictEnable = false;

        public static Languages TransSrcLanguage = Languages.日本語;
        public static Languages TransTargetLanguage = Languages.English;

        public const bool AlapiEnable = false;

        public const bool BaiduApiEnable = false;
        public const string BaiduApiAppid = "";
        public const string BaiduApiSecretKey = "";

        public const bool YeekitEnable = false;

        public const bool BaiduWebEnable = false;

        public const bool CaiyunEnable = false;
        public const string CaiyunDefaultToken = "3975l6lr5pcbvidl6jl2";
    }
}
