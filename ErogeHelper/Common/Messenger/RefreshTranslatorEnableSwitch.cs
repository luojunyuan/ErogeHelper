using ErogeHelper.Common.Enum;

namespace ErogeHelper.Common.Messenger
{
    public class RefreshTranslatorEnableSwitch
    {
        public RefreshTranslatorEnableSwitch(TranslatorName name)
        {
            Name = name;
        }
        public TranslatorName Name { get; }
    }
}