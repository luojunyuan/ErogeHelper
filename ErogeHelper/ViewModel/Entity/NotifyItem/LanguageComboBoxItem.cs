using ErogeHelper.Common.Enum;

namespace ErogeHelper.ViewModel.Entity.NotifyItem
{
    public class LanguageComboBoxItem
    {
        public LanguageComboBoxItem(TransLanguage language)
        {
            LangEnum = language;
            Language = language.ToString();
        }

        public string Language { get; set; }

        public TransLanguage LangEnum { get; set; }
    }
}