using System.Text.Json.Serialization;

namespace ErogeHelper.Model.DataModel.Response;

public record GameSettingResponse
{
    [JsonPropertyName("Id")]
    public int GameId { get; set; }
}
