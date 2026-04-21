using Newtonsoft.Json;

namespace KolayCAR.Broker.Domain.Models.Responses.Eren
{
    public class ErenTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("access_expires_at")]
        public string AccessExpiresAt { get; set; }
    }
}
