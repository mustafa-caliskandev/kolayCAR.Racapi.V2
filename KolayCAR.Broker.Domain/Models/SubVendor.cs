namespace KolayCAR.Broker.Domain.Models
{
    public class SubVendor
    {
        public int Id { get; set; }
        public int SubVendorId { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public VendorTypes VendorType { get; set; }
        public bool Active { get; set; }
        public float? ProfitMarkup { get; set; }
        public float? ProfitMarkupAdditionalProducts { get; set; }
        public float? ProfitMarkupOneWayFee { get; set; }
        public float? APIProfitMarkup { get; set; }
        public float? APIProfitMarkupAdditionalProducts { get; set; }
        public float? APIProfitMarkupOneWayFee { get; set; }
        public string Logo { get; set; }
        public int CurrencyId { get; set; }
    }
}
