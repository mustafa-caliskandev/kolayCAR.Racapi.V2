using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using System.Collections.Generic;
using System.Linq;
using KolayCAR.Broker.Infrastructure.Extensions;

namespace KolayCAR.Broker.API.Helpers.KolayCAR
{
    public class ReservationHelper
    {
        ///KolayCAR API'ına gönderilecek özel günlük fiyat parametresi
        ///Girilen özel günlük fiyat, API fiyatından küçük ise özel günlük fiyat gönderilir.
        ///Girilen özel günlük fiyat, API fiyatından büyük ise özel günlük fiyat 0 gönderilir.
        ///SellingBelowCostForCouponCode=>false ise ve kupon kodu kullanıldıysa, ödenen tutar API tutarından küçük olsa dahi API'a tamamı ödenmiş şekilde iletilir. (Net talebi)
        public static float GetKolayCARSpecialDailyPrice(Agency agency, ReservationToken reservationToken, PostReservationRequest postReservationRequest, List<Extra> apiExtras, Vendor vendor, Reservation reservation)
        {
            return MoneyHelper.ToFloat(GetKolayCARSpecialDailyPriceDecimal(agency, reservationToken, postReservationRequest, apiExtras, vendor, reservation, exchangeRates: null));
        }

        public static decimal GetKolayCARSpecialDailyPriceDecimal(Agency agency, ReservationToken reservationToken, PostReservationRequest postReservationRequest, List<Extra> apiExtras, Vendor vendor, Reservation reservation, List<ExchangeRates> exchangeRates)
        {
            switch (postReservationRequest.PaymentType)
            {
                default:
                case PaymentTypes.CommissionFree:
                case PaymentTypes.PayToAgency:
                case PaymentTypes.PayAll:
                    {
                        var paidAmount = MoneyHelper.ToMoney(postReservationRequest.PaidAmount);
                        var apiTotalAmount = MoneyHelper.ToMoney(reservation.APITotalAmount);
                        var specialDailyPrice = MoneyHelper.ToMoney(postReservationRequest.SpecialDailyPrice);
                        var tokenApiDailyPrice = MoneyHelper.ResolveTokenApiDailyPrice(reservationToken);

                        if (paidAmount >= apiTotalAmount ||
                        postReservationRequest.ExtraPricePayToDelivery ||
                        postReservationRequest.OneWayFeePayToDelivery ||
                        (!vendor.SellingBelowCostForCouponCode && !string.IsNullOrEmpty(postReservationRequest.CouponCode) && paidAmount < apiTotalAmount))
                        {
                            if (agency.FreePriceShowActive)
                            {
                                if (specialDailyPrice != MoneyHelper.NotSet)
                                {
                                    if (specialDailyPrice < tokenApiDailyPrice)
                                    {
                                        return specialDailyPrice;
                                    }
                                    else
                                    {
                                        return MoneyHelper.NotSet;
                                    }
                                }
                                else
                                {
                                    return MoneyHelper.NotSet;
                                }
                            }
                            else
                            {
                                return MoneyHelper.NotSet;
                            }
                        }
                        else
                        {
                            //return reservationToken.DailyPrice;
                            return tokenApiDailyPrice;
                        }
                    }
                case PaymentTypes.PayOnDelivery:
                    {
                        var specialDailyPrice = MoneyHelper.ToMoney(postReservationRequest.SpecialDailyPrice);
                        var tokenDailyPrice = MoneyHelper.ResolveTokenDailyPrice(reservationToken, vendor, exchangeRates);

                        if (agency.FreePriceShowActive)
                        {
                            return specialDailyPrice != MoneyHelper.NotSet ? specialDailyPrice : tokenDailyPrice;
                        }
                        else
                        {
                            return tokenDailyPrice;
                        }
                    }
                case PaymentTypes.AdvancePayment:
                    {
                        var specialDailyPrice = MoneyHelper.ToMoney(postReservationRequest.SpecialDailyPrice);
                        var isSpecialExtraPriceUse = IsSpecialExtraPriceUseDecimal(postReservationRequest.ExtraList, apiExtras, MoneyHelper.ToMoney(vendor.ProfitMarkupAdditionalProducts), postReservationRequest.PaymentType, postReservationRequest, agency, vendor);

                        if (agency.FreePriceShowActive && specialDailyPrice != MoneyHelper.NotSet)
                        {
                            return specialDailyPrice;
                        }
                        else if (agency.AdvancePaymentAmountByAgencyCommissionActive || isSpecialExtraPriceUse)
                        {
                            return MoneyHelper.ResolveTokenDailyPrice(reservationToken, vendor, exchangeRates);
                        }
                        else
                        {
                            return MoneyHelper.NotSet;
                        }
                    }
            }
        }

        public static float GetKolayCARSpecialOneWayFee(Agency agency, ReservationToken reservationToken, PostReservationRequest postReservationRequest, List<Extra> apiExtras, Vendor vendor, Reservation reservation)
        {
            return MoneyHelper.ToFloat(GetKolayCARSpecialOneWayFeeDecimal(agency, reservationToken, postReservationRequest, apiExtras, vendor, reservation, exchangeRates: null));
        }

        public static decimal GetKolayCARSpecialOneWayFeeDecimal(Agency agency, ReservationToken reservationToken, PostReservationRequest postReservationRequest, List<Extra> apiExtras, Vendor vendor, Reservation reservation, List<ExchangeRates> exchangeRates)
        {
            switch (postReservationRequest.PaymentType)
            {
                default:
                case PaymentTypes.CommissionFree:
                case PaymentTypes.PayToAgency:
                case PaymentTypes.PayAll:
                    {
                        if (agency.FreePriceShowActive || postReservationRequest.OneWayFeePayToDelivery)
                        {
                            var specialOneWayFee = MoneyHelper.ToMoney(postReservationRequest.SpecialOneWayFee);
                            var tokenApiOneWayFee = MoneyHelper.ResolveTokenApiOneWayFee(reservationToken);

                            if (specialOneWayFee != MoneyHelper.NotSet)
                            {
                                if (specialOneWayFee < tokenApiOneWayFee || postReservationRequest.OneWayFeePayToDelivery)
                                {
                                    return specialOneWayFee;
                                }
                                else
                                {
                                    return MoneyHelper.NotSet;
                                }
                            }
                            else
                            {
                                return MoneyHelper.NotSet;
                            }
                        }
                        else
                        {
                            return MoneyHelper.NotSet;
                        }
                    }
                case PaymentTypes.PayOnDelivery:
                    {
                        var specialOneWayFee = MoneyHelper.ToMoney(postReservationRequest.SpecialOneWayFee);
                        var tokenOneWayFee = MoneyHelper.ResolveTokenOneWayFee(reservationToken, vendor, exchangeRates);

                        if (agency.FreePriceShowActive)
                        {
                            return specialOneWayFee != MoneyHelper.NotSet ? specialOneWayFee : tokenOneWayFee;
                        }
                        else
                        {
                            return tokenOneWayFee;
                        }
                    }
                case PaymentTypes.AdvancePayment:
                    {
                        var specialOneWayFee = MoneyHelper.ToMoney(postReservationRequest.SpecialOneWayFee);
                        var isSpecialExtraPriceUse = IsSpecialExtraPriceUseDecimal(postReservationRequest.ExtraList, apiExtras, MoneyHelper.ToMoney(vendor.ProfitMarkupAdditionalProducts), postReservationRequest.PaymentType, postReservationRequest, agency, vendor);

                        if (agency.FreePriceShowActive && specialOneWayFee != MoneyHelper.NotSet)
                        {
                            return specialOneWayFee;
                        }
                        else if (agency.AdvancePaymentAmountByAgencyCommissionActive || isSpecialExtraPriceUse)
                        {
                            return MoneyHelper.ResolveTokenOneWayFee(reservationToken, vendor, exchangeRates);
                        }
                        else
                        {
                            return MoneyHelper.NotSet;
                        }
                    }
            }
        }

        private static bool IsSpecialExtraPriceUseDecimal(string extras, List<Extra> apiExtras, decimal profitMarkup, PaymentTypes paymentType, PostReservationRequest postReservationRequest, Agency agency, Vendor vendor)
        {
            if (apiExtras == null || apiExtras.Count == 0 || string.IsNullOrEmpty(extras))
                return false;

            var extraList = extras.Split('|');
            for (int i = 0; i < extraList.Length; i++)
            {
                var extraParts = extraList[i].Split('~');
                if (extraParts.Length < 3)
                    continue;

                string requestExtraCode = extraParts[0];
                decimal requestExtraPrice = extraParts[2].ToDecimalNullSafe();
                decimal requestExtraPriceWithoutProfitMarkup = vendor.AdditionalProductWorkingType == VendorWorkingTypes.ProfitMarkup ?
                    MoneyHelper.RemoveProfitMarkup(profitMarkup, PriceRoundingTypes.DoNotRounding, requestExtraPrice) :
                    requestExtraPrice;
                var apiExtra = apiExtras.Where(x => x.ExtraCode == requestExtraCode).FirstOrDefault();
                if (apiExtra == null)
                    continue;

                if (paymentType == PaymentTypes.PayOnDelivery ||
                    (paymentType == PaymentTypes.AdvancePayment && agency.AdvancePaymentAmountByAgencyCommissionActive) ||
                    (paymentType == PaymentTypes.AdvancePayment && !agency.AdvancePaymentAmountByAgencyCommissionActive && requestExtraPriceWithoutProfitMarkup != MoneyHelper.ToMoney(apiExtra.Price)) ||
                    (paymentType == PaymentTypes.AdvancePayment && (postReservationRequest.SpecialDailyPrice != -1 || postReservationRequest.SpecialOneWayFee != -1)))
                    return true;
            }

            return false;
        }

        public static string GetSoapRemoteAddress(string remoteAddress) => string.IsNullOrEmpty(remoteAddress) ? "https://resws.kolaycar.com/service.asmx" : remoteAddress;
    }
}
