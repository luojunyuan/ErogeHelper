namespace ErogeHelper.Shared.Contracts;

public static class PageTag
{
    public const string General = "general";
    public const string About = "about";
    public const string MeCab = "mecab";
    public const string Danmaku = "danmaku";
    public const string Trans = "trans";
    public const string TTS = "tts";
}

public enum MenuPageTag
{
    None,
    Game,
    GameBack,
    Device,
    DeviceBack,
    Function,
    FunctionBack
}
