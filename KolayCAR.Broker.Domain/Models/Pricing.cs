namespace KolayCAR.Broker.Domain.Models
{
    public class Pricing
    {
        public float PaidAmount { get; set; }
        public float SpecialDailyPrice { get; set; } = 0;
        public float SpecialOneWayFee { get; set; } = 0;
        public float ExtraAmount { get; set; }
        public bool? IsCommissionFreePrice { get; set; } = false;
        public float PaidAmountAfterUsingCouponCode { get; set; }
        public float InstallmentCommissionAmount { get; set; }
    }
}
