using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace KolayCAR.Broker.API.Models
{
    public class ReservationDetailSelectedExtra
    {
        [Required]
        [JsonProperty("extraId")]
        public int Id { get; set; }

        [Required]
        [JsonProperty("extraRentalType")]
        public int ExtraRentalType { get; set; }

        [Required]
        [JsonProperty("rentalDuration")]
        public int RentalDuration { get; set; }

        [Required]
        [JsonProperty("extraName")]
        public string Name { get; set; }

        [DataType(DataType.Currency)]
        [JsonProperty("price")]
        public float Price { get; set; }
    }
}
