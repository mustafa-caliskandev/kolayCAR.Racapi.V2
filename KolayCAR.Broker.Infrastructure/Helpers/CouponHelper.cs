using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using System;

namespace KolayCAR.Broker.Infrastructure.Helpers
{
    public class CouponHelper
    {
        public static CheckCouponIsUsableResult CheckCouponIsUsable(
            CouponCode couponCode,
            float? amount = null,
            CurrencyTypes? currencyType = null,
            DateTime? pickupDate = null,
            DateTime? dropDate = null,
            float? paidAmount = null,
            bool highAmountDiscountActive = false)
        {
            var result = new CheckCouponIsUsableResult
            {
                Usable = true,
                CouponUsageResultType = CouponUsageResultTypes.None
            };

            if (couponCode != null && (couponCode.Active ?? true))
            {
                if (couponCode.AdminId != null || couponCode.AgencyId != null || couponCode.MemberId != null)
                {
                    /*
                     * StartDate => Kuponun geçerli olduğu tarih başlangıcı
                     * EndDate => Kuponun geçerli olduğu tarih bitişi
                     * CouponStartDate => Araç alış tarihi
                     * CouponEndDate => Araç dönüş tarihi
                     */
                    if (couponCode.CouponStartDate != null && pickupDate != null && pickupDate < couponCode.CouponStartDate)
                        result.Usable = false;
                    else if (couponCode.CouponEndDate != null && dropDate != null && pickupDate >= couponCode.CouponEndDate)
                        result.Usable = false;
                    else if (couponCode.StartDate != null && DateTime.Now < couponCode.StartDate)
                        result.Usable = false;
                    else if (couponCode.EndDate != null && DateTime.Now >= couponCode.EndDate)
                        result.Usable = false;
                    else if (string.IsNullOrEmpty(couponCode.Code.TrimNullSafe()))
                        result.Usable = false;
                    else if (!(couponCode.MultipleUsage ?? true) && couponCode.UsageCount > 0)
                        result.Usable = false;
                    else if (couponCode.CouponDiscountType == CouponDiscountTypes.ByPercent && (couponCode.CouponDiscountValue < 0 || couponCode.CouponDiscountValue > 100))
                        result.Usable = false;
                    else if (couponCode.CouponDiscountType == CouponDiscountTypes.ByPrice && couponCode.CouponDiscountValue < 0)
                        result.Usable = false;
                    else if ((amount != null && currencyType == null) || (amount == null && currencyType != null))
                        result.Usable = false;
                    else if (amount != null && couponCode.CouponDiscountType == CouponDiscountTypes.ByPrice && amount - couponCode.CouponDiscountValue < 0)
                    {
                        result.Usable = highAmountDiscountActive;
                        result.CouponUsageResultType = CouponUsageResultTypes.GreaterThanPaymentAmount;
                    }
                    else if (paidAmount != null && paidAmount != 0 && couponCode.CouponDiscountType == CouponDiscountTypes.ByPrice && paidAmount - couponCode.CouponDiscountValue < 0)
                    {
                        result.Usable = highAmountDiscountActive;
                        result.CouponUsageResultType = CouponUsageResultTypes.GreaterThanPaymentAmount;
                    }
                    else if (amount != null && paidAmount != null && paidAmount != 0 && couponCode.CouponDiscountType == CouponDiscountTypes.ByPercent &&
                        paidAmount - amount * couponCode.CouponDiscountValue / 100 < 0)
                    {
                        result.Usable = highAmountDiscountActive;
                        result.CouponUsageResultType = CouponUsageResultTypes.GreaterThanPaymentAmount;
                    }
                }
                else
                    result.Usable = false;
            }
            else
                result.Usable = false;

            if (!result.Usable && result.CouponUsageResultType == CouponUsageResultTypes.None)
                result.CouponUsageResultType = CouponUsageResultTypes.GeneralError;

            return result;
        }
    }
}
