using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Helpers.KolayCAR
{
    public class ExtraHelper
    {
        public static string FormatExtras(string extras)
        {
            string extraListStr = string.Empty;
            if (!string.IsNullOrEmpty(extras))
            {
                var extraList = extras.Split('|');

                for (int i = 0; i < extraList.Length; i++)
                {
                    if (extraList[i].Split('~').Length > 2)
                    extraList[i] = $"{extraList[i].Split('~')[0]}-{extraList[i].Split('~')[1]}-{extraList[i].Split('~')[2]}";
                }
                extraListStr = string.Join(",", extraList);
            }

            return extraListStr;
        }

        public static float GetTotalExtraPrice(string extraListStr, List<Extra> apiExtras, List<FormattedExtra> formattedExtras, int dayCount)
        {
            float price = 0;
            if (!string.IsNullOrEmpty(extraListStr))
            {
                if (apiExtras != null && apiExtras.Count > 0)
                {
                    var extraList = extraListStr.Split(',');
                    for (int i = 0; i < extraList.Length; i++)
                    {
                        string extraItemId = extraList[i].Split('-')[0];
                        int extraItemPiece = extraList[i].Split('-')[1].ToIntNullAvailable() ?? 1;
                        var apiExtra = apiExtras.Where(x => x.ExtraCode == extraItemId && x.ExtraType != AdditionalProductTypes.Compulsory).FirstOrDefault();
                        float extraItemPrice = (extraList[i].Split('-').Length == 3 ? extraList[i].Split('-')[2].ToFloatNullSafe() : 0) * extraItemPiece;

                        if (apiExtra != null)
                        {
                            if (apiExtra.ExtraRentalType == ExtraRentalTypes.Daily)
                                extraItemPrice *= dayCount;

                            price += extraItemPrice;
                        }
                    }
                }
                else
                {
                    price = formattedExtras.Sum(e => ((int)e.RentalType == 0 ? e.Piece * e.ApiPrice : e.Piece * e.ApiPrice * dayCount));
                }
            }
            return price;
        }

        public static string FormatExtrasWithAPIPrices(string extras, List<Extra> apiExtras, List<FormattedExtra> formattedExtras, bool freePriceActive, bool freePriceShowActive, float profitMarkup, PaymentTypes paymentType, List<ExchangeRates> exchangeRates, Domain.Models.Vendor vendor, PostReservationRequest postReservationRequest, Domain.Models.Agency agency, CurrencyTypes baseVendorRequestCurrencyType)
        {
            string extraListStr = string.Empty;
            if (!string.IsNullOrEmpty(extras))
            {
                if (apiExtras != null && apiExtras.Count > 0)
                {
                    {
                        var extraList = extras.Split('|');
                        extraList = extraList.Where(x => !x.Contains("BrokerSpecialPack001")).ToArray();

                        for (int i = 0; i < extraList.Length; i++)
                        {
                            float extraPrice = 0;
                            string requestExtraCode = extraList[i].Split('~')[0];
                            float requestExtraPrice = extraList[i].Split('~')[2].ToFloatNullSafe();
                            float requestExtraPriceWithoutProfitMarkup = vendor.AdditionalProductWorkingType == VendorWorkingTypes.ProfitMarkup ?
                                CalculationHelper.RemoveProfitMarkup(profitMarkup, PriceRoundingTypes.DoNotRounding, requestExtraPrice) : requestExtraPrice;
                            var apiExtra = apiExtras.Where(x => x.ExtraCode == requestExtraCode).FirstOrDefault();
                            if (paymentType == PaymentTypes.PayOnDelivery ||
                                postReservationRequest.ExtraPricePayToDelivery ||
                                (paymentType == PaymentTypes.AdvancePayment && agency.AdvancePaymentAmountByAgencyCommissionActive) ||
                                (paymentType == PaymentTypes.AdvancePayment && !agency.AdvancePaymentAmountByAgencyCommissionActive && requestExtraPriceWithoutProfitMarkup != apiExtra.Price) ||
                                (paymentType == PaymentTypes.AdvancePayment && (postReservationRequest.SpecialDailyPrice != -1 || postReservationRequest.SpecialOneWayFee != -1)))

                            {
                                extraPrice = CalculationHelper.CurrencyExchange(exchangeRates, vendor, requestExtraPrice, postReservationRequest.CurrencyCode.ToEnum<CurrencyTypes>(), baseVendorRequestCurrencyType);
                            }
                            else
                            {
                                extraPrice = CalculationHelper.CalculateAPIPrice(requestExtraPrice, apiExtra.Price, freePriceActive, freePriceShowActive, profitMarkup, paymentType, vendor.AdditionalProductWorkingType);
                            }

                            extraList[i] = $"{extraList[i].Split('~')[0]}-{extraList[i].Split('~')[1]}-{extraPrice.ToStringNullSafe().Replace(",", ".")}";
                        }

                        extraListStr = string.Join(",", extraList);
                    }
                }
                else
                {
                    //string[] extraArr = extras.Split("|");
                    //List<string> extraAdded = new List<string>();

                    //foreach (var item in extraArr)
                    //    if (item.Split("~")[4] != "PRMPKT-1")
                    //        extraAdded.Add($"{item.Split('~')[4]}-{item.Split('~')[1]}-{item.Split("~")[6].Replace(",", ".")}");

                    //extraListStr = string.Join(",", extraAdded);

                    var extraAdded = new List<string>();

                    foreach (var extra in formattedExtras)
                        if (extra.ApiCode != "PRMPKT-1")
                            extraAdded.Add($"{extra.ApiCode}-{extra.Piece}-{extra.ApiPrice.ToStringNullSafe().Replace(",", ".")}");

                    extraListStr = string.Join(",", extraAdded);
                }
            }

            return extraListStr;
        }
    }
}
