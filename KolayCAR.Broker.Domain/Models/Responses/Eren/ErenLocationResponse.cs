using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Responses.Eren
{
    public class ErenLocationListResponse
    {
        [JsonProperty("locations")]
        public List<ErenLocationResponse> Locations { get; set; }
    }

    public class ErenLocationResponse
    {
        [JsonProperty("location_id")]
        public int LocationId { get; set; }

        [JsonProperty("location_name")]
        public string LocationName { get; set; }

        [JsonProperty("latitude")]
        public string Latitude { get; set; }

        [JsonProperty("longitude")]
        public string Longitude { get; set; }

        [JsonProperty("iata_code")]
        public string IataCode { get; set; }

        [JsonProperty("country")]
        public ErenCountry Country { get; set; }

        [JsonProperty("city")]
        public ErenCity City { get; set; }

        [JsonProperty("office")]
        public ErenOffice Office { get; set; }
    }

    public class ErenCountry
    {
        [JsonProperty("country_id")]
        public int CountryId { get; set; }

        [JsonProperty("country_name")]
        public string CountryName { get; set; }
    }

    public class ErenCity
    {
        [JsonProperty("city_id")]
        public int CityId { get; set; }

        [JsonProperty("city_name")]
        public string CityName { get; set; }
    }

    public class ErenOffice
    {
        [JsonProperty("office_id")]
        public int OfficeId { get; set; }

        [JsonProperty("office_name")]
        public string OfficeName { get; set; }

        [JsonProperty("office_address")]
        public string OfficeAddress { get; set; }

        [JsonProperty("zip_code")]
        public string ZipCode { get; set; }

        [JsonProperty("office_email")]
        public string OfficeEmail { get; set; }

        [JsonProperty("office_phone")]
        public string OfficePhone { get; set; }
    }
}
