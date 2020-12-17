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

        public const bool ShowSourceText = true;

        public const bool ShowAppendText = true;

        public const bool PasteToDeepL = false;

        public const TextTemplateType TextTemplate = TextTemplateType.OutLineKanaTop;

        public const bool MecabEnable = true;

        public const bool KanaTop = true;
        public const bool KanaBottom = false;

        public const bool BaiduApiEnable = false;
        public const string BaiduApiAppid = "";
        public const string BaiduApiSecretKey = "";

        public const bool YeekitEnable = false;
    }
}
