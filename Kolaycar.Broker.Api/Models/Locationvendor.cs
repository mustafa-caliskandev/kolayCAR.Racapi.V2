namespace KolayCAR.Broker.API.Models
{
    public partial class Locationvendor
    {
        public int Id { get; set; }
        public int? Vendorid { get; set; }
        public int Locallocationid { get; set; }
        public int? Locationid { get; set; }
        public bool? Active { get; set; }
        public bool? Ispickup { get; set; }
        public string Apilocationname { get; set; }
        public bool? Isoffice { get; set; }
        public string Locationcode { get; set; }
        public string DistrictCode { get; set; }
        public string CityCode { get; set; }
        public bool? FlightCardMandatory { get; set; }
        public int? EarliestResTime { get; set; }
        public string? RateCode { get; set; }
    }
}
