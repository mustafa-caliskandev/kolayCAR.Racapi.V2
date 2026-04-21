using System.Text.Json.Serialization;

namespace KolayCAR.Broker.API.Models
{
    public partial class City
    {
        [JsonPropertyName("CountryId")]
        public int Countryid { get; set; }
        [JsonPropertyName("CityId")]
        public int Cityid { get; set; }
        [JsonPropertyName("CityName")]
        public string Cityname { get; set; }
        [JsonIgnore]
        public int Parent { get; set; }
    }
}
