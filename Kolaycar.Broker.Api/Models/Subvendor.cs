namespace KolayCAR.Broker.API.Models
{
    public partial class Subvendor
    {
        public int Id { get; set; }
        public int Subvendorid { get; set; }
        public int Vendorid { get; set; }
        public string Vendorname { get; set; }
        public int Vendortype { get; set; }
        public bool? Active { get; set; }
        public decimal? Profitmarkup { get; set; }
        public decimal? Profitmarkupadditionalproducts { get; set; }
        public decimal? Profitmarkuponewayfee { get; set; }
        public decimal? Apiprofitmarkup { get; set; }
        public decimal? Apiprofitmarkupadditionalproducts { get; set; }
        public decimal? Apiprofitmarkuponewayfee { get; set; }
        public string Logo { get; set; }
        public int? Currencyid { get; set; }
    }
}
