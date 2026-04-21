namespace KolayCAR.Broker.API.Models
{
    public partial class Vendorextras
    {
        public int Id { get; set; }
        public int Extraid { get; set; }
        public int Vendorid { get; set; }
        public int? Apiextraid { get; set; }
        public int Rentaltype { get; set; }
        public bool Quantityincreasable { get; set; }
        public decimal Price { get; set; }
        public string Extracode { get; set; }
        public int? Currencyid { get; set; }
    }
}
