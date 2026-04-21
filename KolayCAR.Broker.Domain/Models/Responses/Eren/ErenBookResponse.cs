using Newtonsoft.Json;

namespace KolayCAR.Broker.Domain.Models.Responses.Eren
{
    public class ErenBookResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("booking_reference")]
        public string BookingReference { get; set; }

        [JsonProperty("booking_number")]
        public string BookingNumber { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
