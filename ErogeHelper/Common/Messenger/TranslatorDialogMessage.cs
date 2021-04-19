using ErogeHelper.Common.Enum;

namespace ErogeHelper.Common.Messenger
{
    public class TranslatorDialogMessage
    {
        public TranslatorDialogMessage(TranslatorName translatorName)
        {
            Name = translatorName;
        }

        public TranslatorName Name { get; }
    }
}