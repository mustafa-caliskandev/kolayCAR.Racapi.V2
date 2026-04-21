using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;

namespace KolayCAR.Broker.API.Mappers
{
    public static class CouponCodeMapper
    {
        public static CouponCode Map(this Coupon coupon, CurrencyTypes currencyType = CurrencyTypes.TRY) =>
            coupon != null ? new CouponCode
            {
                Id = coupon.Id,
                AdminId = coupon.AdminId,
                AgencyId = coupon.AgencyId,
                MemberId = coupon.MemberId,
                Active = coupon.Active,
                Code = coupon.Code,
                Name = coupon.Name,
                Description = coupon.Description,
                MultipleUsage = coupon.MultipleUsage,
                UsageCount = coupon.UsageCount,
                CouponCreationType = (CouponCreationTypes)coupon.CreationType,
                CouponCreationDate = coupon.CreationDate,
                CouponDiscountType = (CouponDiscountTypes)coupon.DiscountType,
                CouponDiscountValue = coupon.DiscountValue.ToFloatNullSafe(),
                CouponCurrencyType = (coupon.CurrencyId == null ? currencyType : (CurrencyTypes)coupon.CurrencyId - 1 ),
                StartDate = coupon.StartDate,
                EndDate = coupon.EndDate,
                CouponStartDate = coupon.CouponStartDate,
                CouponEndDate = coupon.CouponEndDate,
                ShowPrice = coupon.ShowPrice
            }
            : null;

        public static Coupon Map(this CouponCode coupon) =>
            coupon != null ? new Coupon
            {
                Id = coupon.Id,
                AdminId = coupon.AdminId,
                AgencyId = coupon.AgencyId,
                MemberId = coupon.MemberId,
                Active = coupon.Active,
                Code = coupon.Code,
                Name = coupon.Name,
                Description = coupon.Description,
                MultipleUsage = coupon.MultipleUsage,
                UsageCount = coupon.UsageCount,
                CreationType = (int)coupon.CouponCreationType,
                CreationDate = coupon.CouponCreationDate,
                DiscountType = (int)coupon.CouponDiscountType,
                DiscountValue = coupon.CouponDiscountValue.ToDecimalNullSafe(),
                CurrencyId = (int)(coupon.CouponCurrencyType + 1),
                StartDate = coupon.StartDate,
                EndDate = coupon.EndDate,
                ShowPrice = coupon.ShowPrice
            }
            : null;
    }
}
