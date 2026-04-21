using Newtonsoft.Json;

namespace KolayCAR.Broker.Domain.Models.Requests.RentGo
{
    public class RentGoSearchRequest
    {
        [JsonProperty("type")]
        public int Type { get; set; } = 1;

        [JsonProperty("pickupDate")]
        public long PickupDate { get; set; }

        [JsonProperty("pickupOfficeId")]
        public string PickupOfficeId { get; set; }

        [JsonProperty("dropoffDate")]
        public long DropoffDate { get; set; }

        [JsonProperty("dropoffOfficeId")]
        public string DropoffOfficeId { get; set; }

        [JsonProperty("campaignId")]
        public string CampaignId { get; set; }

        [JsonProperty("lang")]
        public string Lang { get; set; }
    }
}
