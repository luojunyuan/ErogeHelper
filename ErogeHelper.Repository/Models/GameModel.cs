using System;
using System.Collections.Generic;

namespace ErogeHelper.Repository.Models
{
    public class Game
    {
        public int Id { get; set; }

        public string Md5 { get; set; } = string.Empty;

        public List<GameName> Names { get; set; } = new();

        public int TextSettingId { get; set; }

        public TextSetting TextSetting { get; set; } = new();

        public int CreatorId { get; set; }

        public User Creator { get; set; } = new();

        public DateTime CreationTime { get; set; }

        public DateTime ModifiedTime { get; set; }
    }

    public class GameName
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class TextSetting
    {
        public int Id { get; set; }
        public bool UserHook { get; set; }
        public string HookCode { get; set; } = string.Empty;
        public string RegExp { get; set; } = string.Empty;
        public long ThreadContext { get; set; }
        public long SubThreadContext { get; set; }
    }
}