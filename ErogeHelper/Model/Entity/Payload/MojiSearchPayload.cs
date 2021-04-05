using System.Text.Json.Serialization;

namespace ErogeHelper.Model.Entity.Payload
{
    public class MojiSearchPayload
    {
        [JsonPropertyName("langEnv")]
        public string LangEnv { get; set; } = string.Empty;

        [JsonPropertyName("needWords")]
        public string NeedWords { get; set; } = string.Empty;

        [JsonPropertyName("searchText")]
        public string SearchText { get; set; } = string.Empty;

        [JsonPropertyName("_ApplicationId")]
        public string ApplicationId { get; set; } = string.Empty;

        [JsonPropertyName("_ClientVersion")]
        public string ClientVersion { get; set; } = string.Empty;

        [JsonPropertyName("_InstallationId")]
        public string InstallationId { get; set; } = string.Empty;

        [JsonPropertyName("_SessionToken")]
        public string SessionToken { get; set; } = string.Empty;
    }
}
