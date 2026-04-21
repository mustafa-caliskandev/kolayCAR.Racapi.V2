namespace KolayCAR.Broker.Domain.Models
{
    public class LocationVendor
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
        public bool SpecialPackrequirement { get; set; }
        public string DistrictCode { get; set; }
        public string CityCode { get; set; }
        public string? RateCode { get; set; }
    }
}
