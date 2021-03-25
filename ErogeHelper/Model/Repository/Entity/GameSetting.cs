using System.Text.Json.Serialization;

namespace ErogeHelper.Model.Repository.Entity
{
    public class GameSetting
    {
        [JsonPropertyName("id")]
        public int GameId { get; set; }

        [JsonPropertyName("textSettingJson")]
        public string GameSettingJson { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"GameId={GameId} GameSettingJson={GameSettingJson}";
        }
    }
}