namespace KolayCAR.Broker.API.Models
{
    public partial class Currency
    {
        public int Currencyid { get; set; }
        public string Currencyname { get; set; }
        public string Currencyisocode { get; set; }
        public string Symbol { get; set; }
        public bool? Active { get; set; }
        public bool InternationalActive { get; set; }
    }
}
