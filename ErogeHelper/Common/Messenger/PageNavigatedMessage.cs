using ErogeHelper.Common.Enum;

namespace ErogeHelper.Common.Messenger
{
    public class PageNavigatedMessage
    {
        public PageNavigatedMessage(PageName pageName)
        {
            Page = pageName;
        }

        public PageName Page { get; }
    }
}