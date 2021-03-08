using ErogeHelper.Model.Service.Interface;
using ErogeHelper.Repository.Data;
using ErogeHelper.Repository.Models;
using System.Linq;
using System.Text.Json;

namespace ErogeHelper.Model.Service
{
    class GameSettingService : IGameSettingService
    {
        public TextSetting? GetInfoByMD5(string md5)
        {
            using var db = new EHDbContext();

            var game = db.Games.Where(g => g.Md5.Equals(md5)).FirstOrDefault();
            if (game is not null && !game.TextSettingJson.Equals(string.Empty))
                return JsonSerializer.Deserialize<TextSetting>(game.TextSettingJson);

            var localGameInfo = db.GameCaches.SingleOrDefault(g => g.Md5.Equals(md5));
            if (localGameInfo is not null)
            {
                return new TextSetting
                {
                    UserHook = localGameInfo.UserHook,
                    HookCode = localGameInfo.HookCode,
                    RegExp = localGameInfo.RegExp,
                    ThreadContext = localGameInfo.ThreadContext,
                    SubThreadContext = localGameInfo.SubThreadContext,
                };
            }

            return null;
        }
    }

    public class TextSetting
    {
        public bool UserHook { get; set; }
        public string HookCode { get; set; } = string.Empty;
        public string RegExp { get; set; } = string.Empty;
        public long ThreadContext { get; set; }
        public long SubThreadContext { get; set; }
    }
}
