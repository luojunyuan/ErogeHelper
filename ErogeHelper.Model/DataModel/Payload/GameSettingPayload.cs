namespace ErogeHelper.Model.DataModel.Payload;

public record GameSettingPayload
{
    public GameSettingPayload(string username, string password, string md5)
    {
        Username = username;
        Password = password;
        Md5 = md5;
        //foreach (var (type, value) in names)
        //{
        //    Names.Add(new GameNamePair(type, value));
        //}
    }

    public string Username { get; }

    public string Password { get; }

    public string Md5 { get; }

    //public List<GameNamePair> Names { get; } = new();

    public class GameNamePair
    {
        public GameNamePair(string type, string value)
        {
            Type = type;
            Value = value;
        }
        public string Type { get; }
        public string Value { get; }
    }

}
