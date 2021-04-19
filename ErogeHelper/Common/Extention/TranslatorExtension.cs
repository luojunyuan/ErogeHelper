using ErogeHelper.Common.Enum;

namespace ErogeHelper.Common.Extention
{
    public static class TranslatorExtension
    {
        public static string I18N(this TranslatorName name)
        {
            return name switch
            {
                TranslatorName.BaiduApi => Language.Strings.Translator_BaiduApi,
                TranslatorName.BaiduWeb => Language.Strings.Translator_BaiduWeb,
                TranslatorName.Caiyun => Language.Strings.Translator_Caiyun,
                TranslatorName.GoogleCn => Language.Strings.Translator_GoogleCn,
                TranslatorName.NiuTrans => Language.Strings.Translator_NiuTrans,
                TranslatorName.TencentMt => Language.Strings.Translator_TencentMt,
                TranslatorName.Yeekit => Language.Strings.Translator_Yeekit,
                TranslatorName.Youdao => Language.Strings.Translator_Youdao,
                _ => name.ToString()
            };
        }
    }
}