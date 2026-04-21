namespace KolayCAR.Broker.API.Models
{
    public partial class Country
    {
        public int Countryid { get; set; }
        public bool? Defaultcountry { get; set; }
        public string Countrycode2 { get; set; }
        public string Countrycode3 { get; set; }
        public int? Numbercode { get; set; }
        public int? Phonecode { get; set; }
    }
}
