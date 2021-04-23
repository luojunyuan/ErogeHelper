using System.Text.Json.Serialization;

namespace ErogeHelper.Model.Entity.Response
{
    public class GameSettingResponse
    {
        [JsonPropertyName("Id")]
        public int GameId { get; set; }

        [JsonPropertyName("TextSettingJson")]
        public string GameSettingJson { get; set; } = string.Empty;

        public string RegExp { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"GameId={GameId} TextractorSettingJson={GameSettingJson} RegExp={RegExp}";
        }
    }
}