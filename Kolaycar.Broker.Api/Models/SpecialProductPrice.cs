namespace KolayCAR.Broker.API.Models
{
    public class SpecialProductPrice
    {
        public int Id { get; set; }
        public int AdditionalProductId { get; set; }
        public int VendorId { get; set; }
        public int LocationId { get; set; }
        public decimal? Price { get; set; }
        public int CurrencyId { get; set; }
    }
}
