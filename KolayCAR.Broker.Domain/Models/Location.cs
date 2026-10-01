using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KolayCAR.Broker.Domain.Models
{
    public class Location
    {
        public int LocationId { get; set; }
        public string LocationCode { get; set; }
        public int CountryId { get; set; }
        public int CityId { get; set; }
        public string LocationName { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string IataCode { get; set; }
        public bool IsPickup { get; set; }
        public bool IsAirport { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public Coordinate Coordinate { get; set; }
        public string MailAddress { get; set; }
        public bool? IsOffice { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<LocationOfficeWorkingHour> OfficeWorkingHours { get; set; }
        [NotMapped]
        public string DistrictCode { get; set; }
        public string CityCode { get; set; }
        public string CityName { get; set; }
    }

    public class Coordinate
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}
