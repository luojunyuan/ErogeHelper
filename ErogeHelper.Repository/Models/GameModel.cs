using System;
using System.Collections.Generic;

namespace ErogeHelper.Repository.Models
{
    public class Game
    {
        public int Id { get; set; }

        public string Md5 { get; set; } = string.Empty;

        public string TextSettingJson { get; set; } = string.Empty;

        // 2019-07-26T00:00:00
        public DateTime UpdateTime { get; set; }
    }

    public class GameCache
    {
        public string Md5 { get; set; } = string.Empty;
        public bool UserHook { get; set; }
        public string HookCode { get; set; } = string.Empty;
        public string RegExp { get; set; } = string.Empty;
        public long ThreadContext { get; set; }
        public long SubThreadContext { get; set; }
    }
}