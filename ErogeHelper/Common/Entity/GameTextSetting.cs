namespace ErogeHelper.Common.Entity
{
    public class GameTextSetting
    {
        // ?: Do I need MD5 in this class?
        public string Md5 { get; set; } = string.Empty;
        public bool UserHook { get; set; }
        public string Hookcode { get; set; } = string.Empty;
        public string RegExp { get; set; } = string.Empty;
        public long ThreadContext { get; set; }
        public long SubThreadContext { get; set; }
    }
}