using Newtonsoft.Json;

namespace KolayCAR.Broker.Domain.Models.Requests.Eren
{
    public class ErenSearchRequest
    {
        [JsonProperty("pickup_id")]
        public int PickupId { get; set; }

        [JsonProperty("dropoff_id")]
        public int DropoffId { get; set; }

        [JsonProperty("pickup_date")]
        public string PickupDate { get; set; }

        [JsonProperty("dropoff_date")]
        public string DropoffDate { get; set; }

        [JsonProperty("currency_code")]
        public string CurrencyCode { get; set; }
    }
}
