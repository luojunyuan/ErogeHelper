namespace ErogeHelper.ViewModel.Entity.NotifyItem
{
    public class AppendTextItem
    {
        public AppendTextItem(string message, string extraInfo = "")
        {
            Message = message;
            ExtraInfo = extraInfo;
        }

        public string Message { get; }
        public string ExtraInfo { get; }
    }
}