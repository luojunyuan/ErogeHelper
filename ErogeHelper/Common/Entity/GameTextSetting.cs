namespace ErogeHelper.Common.Entity
{
    public class GameTextSetting
    {
        public bool IsUserHook { get; set; }
        public string Hookcode { get; set; } = string.Empty;
        public string RegExp { get; set; } = string.Empty;
        public long ThreadContext { get; set; }
        public long SubThreadContext { get; set; }
    }
}