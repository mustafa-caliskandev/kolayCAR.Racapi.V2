namespace KolayCAR.Broker.API.Models
{
    public partial class Agencyvendorprofitmarkup
    {
        public int Id { get; set; }
        public int Agencyid { get; set; }
        public int Vendorid { get; set; }
        public decimal? Profitmarkup { get; set; }
        public decimal? Profitmarkupadditionalproducts { get; set; }
        public decimal? Profitmarkuponewayfee { get; set; }
        public bool? Active { get; set; }
    }
}
