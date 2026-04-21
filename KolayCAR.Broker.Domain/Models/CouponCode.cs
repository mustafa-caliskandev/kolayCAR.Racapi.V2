using System;

namespace KolayCAR.Broker.Domain.Models
{
    public class CouponCode
    {
        public int Id { get; set; }
        public int? AdminId { get; set; }
        public int? AgencyId { get; set; }
        public int? MemberId { get; set; }
        public bool? Active { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? MultipleUsage { get; set; }
        public int? UsageCount { get; set; }
        public CouponCreationTypes CouponCreationType { get; set; }
        public DateTime CouponCreationDate { get; set; }
        public CouponDiscountTypes CouponDiscountType { get; set; }
        public float CouponDiscountValue { get; set; }
        public CurrencyTypes CouponCurrencyType { get; set; }
        public int Score { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CouponStartDate { get; set; }
        public DateTime? CouponEndDate { get; set; }
        public bool? ShowPrice { get; set; }
    }

    public enum CouponCreationTypes
    {
        ByAdmin,
        ByUser
    }

    public enum CouponDiscountTypes
    {
        ByPercent,
        ByPrice
    }

    public enum CouponUsageResultTypes
    {
        None,
        GeneralError,
        GreaterThanPaymentAmount,
        GreaterThanTotalAmount
    }

    public class CheckCouponIsUsableResult
    {
        public bool Usable { get; set; }
        public CouponUsageResultTypes CouponUsageResultType { get; set; }
    }

    public class ApplyCouponCodeResponse
    {
        public Vehicle Vehicle { get; set; }
        public CouponUsageResultTypes CouponUsageResultType { get; set; }
    }
}
