namespace KolayCAR.Broker.Domain.Models
{
    public class PriceOptions
    {
        public bool CommissionFreePeymentTypeActive { get; set; }
        public bool FreeDailyPriceActive { get; set; }
        public bool FreeExtraPriceActive { get; set; }
        public bool FreeOneWayFeeActive { get; set; }
        public VendorPriceCalculationTypes VendorPriceCalculationType { get; set; }
    }
    public enum VendorPriceCalculationTypes
    {
        PurchasePrice,
        SalePrice
    }
}
