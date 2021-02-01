using ErogeHelper.Common.Selector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Model
{
    internal class DefaultValuesStore
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

        public const bool BaiduApiEnable = false;
        public const string BaiduApiAppid = "";
        public const string BaiduApiSecretKey = "";

        public const bool YeekitEnable = false;
    }
}
