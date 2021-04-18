using System.Collections.Generic;

namespace ErogeHelper.Common.Entity
{
    public class TextractorSetting
    {
        public bool IsUserHook { get; set; }
        public string Hookcode { get; set; } = string.Empty;
        public IEnumerable<HookSetting> HookSettings { get; set; } = new List<HookSetting>();
        public List<ulong> UselessAddresses { get; set; } = new();

        public class HookSetting
        {
            public long ThreadContext { get; set; }
            public long SubThreadContext { get; set; }
        }
    }
}