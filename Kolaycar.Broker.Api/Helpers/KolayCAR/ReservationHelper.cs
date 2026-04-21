using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using System.Collections.Generic;
using BrokerReservationHelper = KolayCAR.Broker.API.Helpers.ReservationHelper;

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
            switch (postReservationRequest.PaymentType)
            {
                default:
                case PaymentTypes.CommissionFree:
                case PaymentTypes.PayToAgency:
                case PaymentTypes.PayAll:
                    {
                        if (postReservationRequest.PaidAmount >= reservation.APITotalAmount ||
                        postReservationRequest.ExtraPricePayToDelivery ||
                        postReservationRequest.OneWayFeePayToDelivery ||
                        (!vendor.SellingBelowCostForCouponCode && !string.IsNullOrEmpty(postReservationRequest.CouponCode) && postReservationRequest.PaidAmount < reservation.APITotalAmount))
                        {
                            if (agency.FreePriceShowActive)
                            {
                                if (postReservationRequest.SpecialDailyPrice != -1)
                                {
                                    if (postReservationRequest.SpecialDailyPrice < reservationToken.APIDailyPrice)
                                    {
                                        return postReservationRequest.SpecialDailyPrice;
                                    }
                                    else
                                    {
                                        return -1;
                                    }
                                }
                                else
                                {
                                    return -1;
                                }
                            }
                            else
                            {
                                return -1;
                            }
                        }
                        else
                        {
                            //return reservationToken.DailyPrice;
                            return reservationToken.APIDailyPrice;
                        }
                    }
                case PaymentTypes.PayOnDelivery:
                    {
                        if (agency.FreePriceShowActive)
                        {
                            return postReservationRequest.SpecialDailyPrice != -1 ? postReservationRequest.SpecialDailyPrice : reservationToken.DailyPrice;
                        }
                        else
                        {
                            return reservationToken.DailyPrice;
                        }
                    }
                case PaymentTypes.AdvancePayment:
                    {
                        var isSpecialExtraPriceUse = BrokerReservationHelper.IsSpecialExtraPriceUse(postReservationRequest.ExtraList, apiExtras, vendor.ProfitMarkupAdditionalProducts, postReservationRequest.PaymentType, postReservationRequest, agency, vendor);

                        if (agency.FreePriceShowActive && postReservationRequest.SpecialDailyPrice != -1)
                        {
                            return postReservationRequest.SpecialDailyPrice;
                        }
                        else if (agency.AdvancePaymentAmountByAgencyCommissionActive || isSpecialExtraPriceUse)
                        {
                            return reservationToken.DailyPrice;
                        }
                        else
                        {
                            return -1;
                        }
                    }
            }
        }

        public static float GetKolayCARSpecialOneWayFee(Agency agency, ReservationToken reservationToken, PostReservationRequest postReservationRequest, List<Extra> apiExtras, Vendor vendor, Reservation reservation)
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
                            if (postReservationRequest.SpecialOneWayFee != -1)
                            {
                                if (postReservationRequest.SpecialOneWayFee < reservationToken.APIOneWayFee || postReservationRequest.OneWayFeePayToDelivery)
                                {
                                    return postReservationRequest.SpecialOneWayFee;
                                }
                                else
                                {
                                    return -1;
                                }
                            }
                            else
                            {
                                return -1;
                            }
                        }
                        else
                        {
                            return -1;
                        }
                    }
                case PaymentTypes.PayOnDelivery:
                    {
                        if (agency.FreePriceShowActive)
                        {
                            return postReservationRequest.SpecialOneWayFee != -1 ? postReservationRequest.SpecialOneWayFee : reservationToken.OneWayFee;
                        }
                        else
                        {
                            return reservationToken.OneWayFee;
                        }
                    }
                case PaymentTypes.AdvancePayment:
                    {
                        var isSpecialExtraPriceUse = BrokerReservationHelper.IsSpecialExtraPriceUse(postReservationRequest.ExtraList, apiExtras, vendor.ProfitMarkupAdditionalProducts, postReservationRequest.PaymentType, postReservationRequest, agency, vendor);

                        if (agency.FreePriceShowActive && postReservationRequest.SpecialOneWayFee != -1)
                        {
                            return postReservationRequest.SpecialOneWayFee;
                        }
                        else if (agency.AdvancePaymentAmountByAgencyCommissionActive || isSpecialExtraPriceUse)
                        {
                            return reservationToken.OneWayFee;
                        }
                        else
                        {
                            return -1;
                        }
                    }
            }
        }

        public static string GetSoapRemoteAddress(string remoteAddress) => string.IsNullOrEmpty(remoteAddress) ? "https://resws.kolaycar.com/service.asmx" : remoteAddress;
    }
}
