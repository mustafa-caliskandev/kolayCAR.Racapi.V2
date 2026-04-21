namespace KolayCAR.Broker.Domain.Models
{
    public class UsingCouponCode
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public CouponUsageResultTypes CouponUsageResultType { get; set; }
        public int CouponId { get; set; }
        public string CouponCode { get; set; }
        public float DiscountValue { get; set; }
        public float DiscountAmount { get; set; }
        public CouponDiscountTypes CouponDiscountType { get; set; }
        public float TotalPriceBeforeDiscount { get; set; }
        public float TotalPricePayNowBeforeDiscount { get; set; }
    }
}
