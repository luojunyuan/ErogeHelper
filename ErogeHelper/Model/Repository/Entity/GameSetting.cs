using System.Text.Json.Serialization;

namespace ErogeHelper.Model.Repository.Entity
{
    public class GameSetting
    {
        [JsonPropertyName("Id")]
        public int GameId { get; set; }

        [JsonPropertyName("TextSettingJson")]
        public string GameSettingJson { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"GameId={GameId} GameSettingJson={GameSettingJson}";
        }
    }
}