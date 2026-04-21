using Newtonsoft.Json;

namespace KolayCAR.Broker.Domain.Models.Requests.Eren
{
    public class ErenTokenRequest
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
