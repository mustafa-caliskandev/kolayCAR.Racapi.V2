using Newtonsoft.Json;

namespace KolayCAR.Broker.Domain.Models.Requests.Eren
{
    public class ErenCancelRequest
    {
        [JsonProperty("booking_number")]
        public string BookingNumber { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }
    }
}
