namespace ErogeHelper.Common.Entities;

public class TextractorSetting
{
    /// <summary>
    /// Engine name
    /// </summary>
    public string HookName { get; set; } = string.Empty;
    public bool IsUserHook { get; set; }
    public string HookCode { get; set; } = string.Empty;
    public List<HookSetting> HookSettings { get; set; } = new();

    public class HookSetting
    {
        public TextThread ThreadType { get; set; }
        public long ThreadContext { get; set; }
        public long SubThreadContext { get; set; }
    }

    public enum TextThread
    {
        CharacterName,
        Text
    }
}
