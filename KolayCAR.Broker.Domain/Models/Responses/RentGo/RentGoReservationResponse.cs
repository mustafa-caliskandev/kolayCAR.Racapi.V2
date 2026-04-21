using Newtonsoft.Json;

namespace KolayCAR.Broker.Domain.Models.Responses.RentGo
{
    public class RentGoReservationResponse
    {
        [JsonProperty("resType")]
        public int ResType { get; set; }

        [JsonProperty("listId")]
        public string ListId { get; set; }

        [JsonProperty("total")]
        public decimal Total { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("pnr")]
        public string Pnr { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }
    }
}
