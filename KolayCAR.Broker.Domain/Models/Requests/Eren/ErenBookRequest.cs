using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Requests.Eren
{
    public class ErenBookRequest
    {
        [JsonProperty("booking_reference")]
        public string BookingReference { get; set; }

        [JsonProperty("search_request_id")]
        public string SearchRequestId { get; set; }

        [JsonProperty("quote_id")]
        public string QuoteId { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("flight_number")]
        public string FlightNumber { get; set; }

        [JsonProperty("driver_age")]
        public int DriverAge { get; set; }

        [JsonProperty("id_number")]
        public string IdNumber { get; set; }

        [JsonProperty("passport_number")]
        public string PassportNumber { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("extras")]
        public List<ErenBookExtra> Extras { get; set; }
    }

    public class ErenBookExtra
    {
        [JsonProperty("service_id")]
        public int ServiceId { get; set; }
    }
}
