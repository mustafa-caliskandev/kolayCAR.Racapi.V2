using System.Text.Json.Serialization;

namespace KolayCAR.Broker.API.Models
{
    public partial class Countrylang
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public int Id { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public int Lang { get; set; }
        [JsonPropertyName("CountryId")]
        public int Countryid { get; set; }
        //[JsonPropertyName("CountryCode")]
        // public string CountryCode { get; set; }
        [JsonPropertyName("CountryName")]
        public string Countryname { get; set; }
    }

}
