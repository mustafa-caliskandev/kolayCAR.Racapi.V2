namespace KolayCAR.Broker.Domain.Models
{
    public class SpecialProductPrice
    {
        public int? Id { get; set; }
        public int? AdditionalProductId { get; set; }
        public int VendorId { get; set; }
        public string LocalProductName { get; set; }
        public string ApiProductName { get; set; }
        public string ApiProductCode { get; set; }
        public string LocationName { get; set; }
        public int LocationId { get; set; }
        public decimal? Price { get; set; }
        public int CurrencyId { get; set; }
        public ExtraRentalTypes? RentalTypes { get; set; }
    }
}
