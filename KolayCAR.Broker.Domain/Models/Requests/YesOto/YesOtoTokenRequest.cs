using System.Text.Json.Serialization;

namespace KolayCAR.Broker.Domain.Models.Requests.YesOto
{
    public class YesOtoTokenRequest
    {
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }

        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; }

        [JsonPropertyName("grant_type")]
        public string GrantType { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }
}
