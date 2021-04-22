namespace ErogeHelper.Model.Entity.Table
{
    public class UserTermTable
    {
        public UserTermTable(string from, string to)
        {
            From = from;
            To = to;
        }

        public string From { get; }
        public string To { get; }
    }
}