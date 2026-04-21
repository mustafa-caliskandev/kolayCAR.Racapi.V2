namespace KolayCAR.Broker.API.Models.Dtos
{
    public class SearchLocationDto
    {
        public int LanguageId { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public bool IsAirport { get; set; }
        public bool IsPopular { get; set; }
        public bool IsPickup { get; set; }
        public int LocationOrder { get; set; }
    }
}
