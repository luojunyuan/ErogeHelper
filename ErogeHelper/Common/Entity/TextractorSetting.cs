using System.Collections.Generic;

namespace ErogeHelper.Common.Entity
{
    // Textractor minimum required information
    public class TextractorSetting
    {
        public bool IsUserHook { get; set; }
        public string Hookcode { get; set; } = string.Empty;
        public List<HookSetting> HookSettings { get; set; } = new();

        public class HookSetting
        {
            public ulong ThreadContext { get; set; }
            public ulong SubThreadContext { get; set; }
        }
    }
}
