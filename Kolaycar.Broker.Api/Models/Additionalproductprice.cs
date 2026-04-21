namespace KolayCAR.Broker.API.Models
{
    public partial class Additionalproductprice
    {
        public int Additionalproductvendorid { get; set; }
        public int Productid { get; set; }
        public int Vendorid { get; set; }
        public int Startday { get; set; }
        public int Endday { get; set; }
        public decimal? Price { get; set; }
        public int Currencyid { get; set; }
    }
}
