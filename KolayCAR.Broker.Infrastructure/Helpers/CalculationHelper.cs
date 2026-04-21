using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.Infrastructure.Helpers
{
    public class CalculationHelper
    {
        public static float RoundPrice(float price, int decimals) =>
            Math.Round(price.ToDecimalNullSafe(), decimals).ToFloatNullSafe();

        public static float AddProfitMarkup(float profitMarkup, PriceRoundingTypes priceRoundingType, float price, VendorWorkingTypes vendorWorkingType) =>
           vendorWorkingType == VendorWorkingTypes.ProfitMarkup ?
                RoundPrice(price + price * profitMarkup / 100, (int)priceRoundingType) :
                RoundPrice(price, (int)priceRoundingType);

        public static float AdjustProfitMarkup(float profitMarkup, PriceRoundingTypes priceRoundingType, float price, int markupType, int type) =>
            markupType == 0
                ? (type == 1
                    ? RoundPrice(price + price * profitMarkup / 100, (int)priceRoundingType)  // Yüzdelik artış
                    : RoundPrice(price - price * profitMarkup / 100, (int)priceRoundingType)) // Yüzdelik indirim
                : (type == 1
                    ? RoundPrice(price + profitMarkup, (int)priceRoundingType)  // Sabit artış
                    : RoundPrice(price - profitMarkup, (int)priceRoundingType)); // Sabit indirim

        public static float ExtractCommission(float commission, PriceRoundingTypes priceRoundingType, float price) =>
                RoundPrice(price * (100 - commission) / 100, (int)priceRoundingType);

        public static float GetCommissionPrice(float commission, PriceRoundingTypes priceRoundingType, float price) =>
                RoundPrice(price * commission / 100, (int)priceRoundingType);

        public static float RemoveProfitMarkup(float profitMarkup, PriceRoundingTypes priceRoundingType, float price) =>
            RoundPrice(price * 100 / (100 + profitMarkup), (int)priceRoundingType);

        public static float CalculateTotalPrice(PriceRoundingTypes priceRoundingType, int rentalDuration, float dailyPrice, float extraPrice, float oneWayFee) =>
            RoundPrice(dailyPrice * rentalDuration + extraPrice + oneWayFee, (int)priceRoundingType);

        public static float CalculateFinalPrice(float profitMarkup, PriceRoundingTypes priceRoundingType, float price, VendorWorkingTypes vendorWorkingType) =>
            AddProfitMarkup(profitMarkup, priceRoundingType, price, vendorWorkingType);

        public static float SubtractVATFromPrice(float price, int vat) =>
            price * (100 - vat) / 100;

        public static float CalculateVATAmount(float price, int vat) =>
            price * vat / 100;

        public static float MakeDiscount(float price, float percent) => price * (100 - percent) / 100;

        public static Vehicle CalculateFinalVehiclePrices(Vendor vendor, Vehicle vehicle, Agency agency, CurrencyTypes requestCurrencyType = CurrencyTypes.EUR, List<ProfitMarkup> profitMarkups = null, List<ExchangeRates> exchangeRates = null)
        {
            vehicle.ApiDailyPrice = vehicle.DailyPrice;
            if (vendor.ProfitMarkupDailyPriceActive)
            {
                vehicle.DailyPrice = AddProfitMarkup(vendor.ProfitMarkupDailyPrice, vendor.PriceRoundingType, vehicle.DailyPrice, vendor.RentalWorkingType);
                vehicle.DailyPricePayNow = AddProfitMarkup(vendor.ProfitMarkupDailyPrice, vendor.PriceRoundingType, vehicle.DailyPricePayNow, vendor.RentalWorkingType);
            }

            if (profitMarkups?.Count > 0)
            {
                //var profitMarkup = profitMarkups?.Where(pr =>
                //    (pr.MinimumAmount == 0 || pr.MinimumAmount.ToFloatNullSafe() <= vehicle.DailyPrice) &&
                //    (pr.MaximumAmount == 0 || pr.MaximumAmount.ToFloatNullSafe() >= vehicle.DailyPrice))
                //    .OrderBy(p => p.Priority == 0 ? int.MaxValue : p.Priority ?? int.MaxValue)
                //    .FirstOrDefault();

                var profitMarkup = GetFilteredProfitMarkup(profitMarkups, exchangeRates, vendor, vehicle.DailyPrice, requestCurrencyType, vehicle.RentalDuration);

                if (profitMarkup != null)
                {
                    float convertedProfitMarkup = (profitMarkup.MarkupType == 1)
                        ? CurrencyExchange(exchangeRates, vendor, profitMarkup.MarkupValue.ToFloatNullSafe(), (CurrencyTypes)(profitMarkup.CurrencyId - 1), requestCurrencyType)
                        : profitMarkup.MarkupValue.ToFloatNullSafe();

                    vehicle.DailyPrice = AdjustProfitMarkup(convertedProfitMarkup, vendor.PriceRoundingType, vehicle.DailyPrice, profitMarkup.MarkupType, profitMarkup.Type);
                    vehicle.DailyPricePayNow = AdjustProfitMarkup(convertedProfitMarkup, vendor.PriceRoundingType, vehicle.DailyPricePayNow, profitMarkup.MarkupType, profitMarkup.Type);
                    vehicle.SpecialProfitApplied = StringHelper.SpecialProfitMarkup(convertedProfitMarkup, profitMarkup, requestCurrencyType);

                    //vehicle.SpecialProfitApplied = "Markup = " + (profitMarkup.Type == 1 ? "+" : "-") + convertedProfitMarkup 
                    //    + (profitMarkup.MarkupType == 1 
                    //        ? convertedProfitMarkup != profitMarkup.MarkupValue 
                    //            ? " " +(CurrencyTypes)(requestCurrencyType) + $" ({profitMarkup.MarkupValue} {(CurrencyTypes)(profitMarkup.CurrencyId - 1)})"  
                    //            : " " +(CurrencyTypes)(profitMarkup.CurrencyId - 1)
                    //        : "%")
                    //    + "  MarkupID = " + profitMarkup.Id + "  MarkupName = " + profitMarkup.Name;

                    //vehicle.SpecialProfitApplied = $"Markup Value = {convertedProfitMarkup} - Markup Type = {profitMarkup.MarkupType}, Profit Markup = {profitMarkup.Type}, Name ={profitMarkup.Name} ";
                    //vehicle.DailyPricePayNow = AdjustProfitMarkup(profitMarkup.MarkupValue.ToFloatNullSafe(), vendor.PriceRoundingType, vehicle.DailyPrice, profitMarkup.MarkupType, profitMarkup.Type);
                }
            }


            if (!agency.FreePriceShowActive)
            {
                vehicle.DailyPrice = AddProfitMarkup(agency.AgencyRentalProfitMarkup, vendor.PriceRoundingType, vehicle.DailyPrice, VendorWorkingTypes.ProfitMarkup);
                vehicle.DailyPricePayNow = AddProfitMarkup(agency.AgencyRentalProfitMarkup, vendor.PriceRoundingType, vehicle.DailyPricePayNow, VendorWorkingTypes.ProfitMarkup);
            }

            if (vendor.ProfitMarkupOneWayFeeActive)
                vehicle.OneWayFee = AddProfitMarkup(vendor.ProfitMarkupOneWayFee, vendor.PriceRoundingType, vehicle.OneWayFee, vendor.OneWayFeeWorkingType);

            if (vendor.ProfitMarkupAdditionalProductsActive)
                vehicle.ExtraPrice = AddProfitMarkup(vendor.ProfitMarkupAdditionalProducts, vendor.PriceRoundingType, vehicle.ExtraPrice, vendor.AdditionalProductWorkingType);

            if (agency.PayAllActive && agency.CreditCardDiscountPercent != 0)
            {
                vehicle.DailyPricePayNow = RoundPrice(MakeDiscount(vehicle.DailyPricePayNow, agency.CreditCardDiscountPercent), (int)vendor.PriceRoundingType);
            }

            vehicle.TotalPrice = CalculateTotalPrice(vendor.PriceRoundingType, vehicle.RentalDuration, vehicle.DailyPrice, vehicle.ExtraPrice, vehicle.OneWayFee);
            vehicle.TotalPricePayNow = CalculateTotalPrice(vendor.PriceRoundingType, vehicle.RentalDuration, vehicle.DailyPricePayNow, vehicle.ExtraPrice, vehicle.OneWayFee);

            return vehicle;
        }

        public static float CurrencyExchange(List<ExchangeRates> exchangeRates, Vendor vendor, float price, CurrencyTypes sourceCurrency, CurrencyTypes targetCurrency)
        {
            var sourceExchangeRate = exchangeRates.Where(x => x.CurrencyType == sourceCurrency).FirstOrDefault();
            var targetExchangeRate = exchangeRates.Where(x => x.CurrencyType == targetCurrency).FirstOrDefault();

            return RoundPrice(price * sourceExchangeRate.ExchangeRate / targetExchangeRate.ExchangeRate, (int)vendor.PriceRoundingType);
        }

        public static float CalculateAPIPrice(float requestPrice, float apiPrice, bool freePriceActive, bool freePriceShowActive, float profitMarkup, PaymentTypes paymentType, VendorWorkingTypes additionalProductWorkingType)
        {
            float requestPriceWithoutProfitMarkup = additionalProductWorkingType == VendorWorkingTypes.ProfitMarkup ? RemoveProfitMarkup(profitMarkup, PriceRoundingTypes.DoNotRounding, requestPrice) : requestPrice;

            float resultPrice;
            if (paymentType != PaymentTypes.PayOnDelivery)
            {
                if (freePriceShowActive)
                {
                    if (requestPriceWithoutProfitMarkup >= apiPrice)
                        resultPrice = apiPrice;
                    else
                        resultPrice = apiPrice; // requestPriceWithoutProfitMarkup yada apiPrice kullanılacak. Test ediliyor
                }
                else
                    resultPrice = apiPrice;
            }
            else
            {
                resultPrice = requestPrice;
            }

            return resultPrice;
        }

        public static float GetAPIDailyPrice(Agency agency, ReservationToken reservationToken, PostReservationRequest postReservationRequest, Reservation reservation)
        {
            switch (postReservationRequest.PaymentType)
            {
                default:
                case PaymentTypes.CommissionFree:
                case PaymentTypes.PayAll:
                    {
                        if (postReservationRequest.PaidAmount >= reservation.APITotalAmount)
                        {
                            if (agency.FreePriceShowActive)
                            {
                                if (postReservationRequest.SpecialDailyPrice == -1)
                                {
                                    return reservationToken.APIDailyPrice;
                                }
                                else if (postReservationRequest.SpecialDailyPrice < reservationToken.APIDailyPrice)
                                {
                                    return postReservationRequest.SpecialDailyPrice;
                                }
                                else
                                {
                                    return reservationToken.APIDailyPrice;
                                }
                            }
                            else
                            {
                                return reservationToken.APIDailyPrice;
                            }
                        }
                        else
                        {
                            return reservationToken.DailyPrice;
                        }
                    }
                case PaymentTypes.PayToAgency:
                    {
                        if (postReservationRequest.PaidAmount >= reservation.APIDailyPrice * reservation.RentalDuration)
                        {
                            if (agency.FreePriceShowActive)
                            {
                                if (postReservationRequest.SpecialDailyPrice == -1)
                                {
                                    return reservationToken.APIDailyPrice;
                                }
                                else if (postReservationRequest.SpecialDailyPrice < reservationToken.APIDailyPrice)
                                {
                                    return postReservationRequest.SpecialDailyPrice;
                                }
                                else
                                {
                                    return reservationToken.APIDailyPrice;
                                }
                            }
                            else
                            {
                                return reservationToken.APIDailyPrice;
                            }
                        }
                        else
                        {
                            return reservationToken.DailyPrice;
                        }
                    }
                case PaymentTypes.PayOnDelivery:
                    {
                        if (agency.FreePriceShowActive)
                        {
                            if (postReservationRequest.SpecialDailyPrice == -1)
                            {
                                return reservationToken.APIDailyPrice;
                            }
                            else
                            {
                                return postReservationRequest.SpecialDailyPrice;
                            }
                        }

                        return reservationToken.APIDailyPrice;
                    }
                case PaymentTypes.AdvancePayment:
                    {
                        if ((agency.FreePriceShowActive && postReservationRequest.SpecialDailyPrice != -1) || agency.AdvancePaymentAmountByAgencyCommissionActive)
                        {
                            return postReservationRequest.PaidAmount;
                        }
                        else
                        {
                            return 0;
                        }
                    }
            }
        }
        public static float GetAPIDailyPrice(Agency agency, ReservationToken reservationToken, PostReservationRequestV2 postReservationRequest, Reservation reservation)
        {
            switch (postReservationRequest.Payment.PaymentType)
            {
                default:
                case PaymentTypes.CommissionFree:
                case PaymentTypes.PayAll:
                    {
                        if (postReservationRequest.Pricing.PaidAmount >= reservation.APITotalAmount)
                        {
                            if (agency.FreePriceShowActive)
                            {
                                if (postReservationRequest.Pricing.SpecialDailyPrice == -1)
                                {
                                    return reservationToken.APIDailyPrice;
                                }
                                else if (postReservationRequest.Pricing.SpecialDailyPrice < reservationToken.APIDailyPrice)
                                {
                                    return postReservationRequest.Pricing.SpecialDailyPrice;
                                }
                                else
                                {
                                    return reservationToken.APIDailyPrice;
                                }
                            }
                            else
                            {
                                return reservationToken.APIDailyPrice;
                            }
                        }
                        else
                        {
                            return reservationToken.DailyPrice;
                        }
                    }
                case PaymentTypes.PayToAgency:
                    {
                        if (postReservationRequest.Pricing.PaidAmount >= reservation.APIDailyPrice * reservation.RentalDuration)
                        {
                            if (agency.FreePriceShowActive)
                            {
                                if (postReservationRequest.Pricing.SpecialDailyPrice == -1)
                                {
                                    return reservationToken.APIDailyPrice;
                                }
                                else if (postReservationRequest.Pricing.SpecialDailyPrice < reservationToken.APIDailyPrice)
                                {
                                    return postReservationRequest.Pricing.SpecialDailyPrice;
                                }
                                else
                                {
                                    return reservationToken.APIDailyPrice;
                                }
                            }
                            else
                            {
                                return reservationToken.APIDailyPrice;
                            }
                        }
                        else
                        {
                            return reservationToken.DailyPrice;
                        }
                    }
                case PaymentTypes.PayOnDelivery:
                    {
                        if (agency.FreePriceShowActive)
                        {
                            if (postReservationRequest.Pricing.SpecialDailyPrice == -1)
                            {
                                return reservationToken.APIDailyPrice;
                            }
                            else
                            {
                                return postReservationRequest.Pricing.SpecialDailyPrice;
                            }
                        }

                        return reservationToken.APIDailyPrice;
                    }
                case PaymentTypes.AdvancePayment:
                    {
                        if ((agency.FreePriceShowActive && postReservationRequest.Pricing.SpecialDailyPrice != -1) || agency.AdvancePaymentAmountByAgencyCommissionActive)
                        {
                            return postReservationRequest.Pricing.PaidAmount;
                        }
                        else
                        {
                            return 0;
                        }
                    }
            }
        }
        public static float GetAPIOneWayFee(Agency agency, ReservationToken reservationToken, PostReservationRequest postReservationRequest, Reservation reservation)
        {
            switch (postReservationRequest.PaymentType)
            {
                default:
                case PaymentTypes.PayToAgency:
                case PaymentTypes.CommissionFree:
                case PaymentTypes.PayAll:
                    {
                        if (postReservationRequest.OneWayFeePayToDelivery) //Ödeme tipi PayToAgency değil ise her zaman false geleceği için ödeme tipi kontrolü yapılmasına gerek yok
                        {
                            return 0;
                        }
                        else
                        {
                            if (agency.FreePriceShowActive)
                            {
                                if (postReservationRequest.SpecialOneWayFee == -1)
                                {
                                    return reservationToken.APIOneWayFee;
                                }
                                else if (postReservationRequest.SpecialOneWayFee < reservationToken.APIOneWayFee)
                                {
                                    return postReservationRequest.SpecialOneWayFee;
                                }
                                else
                                {
                                    return reservationToken.APIOneWayFee;
                                }
                            }
                            else
                            {
                                return reservationToken.APIOneWayFee;
                            }
                        }
                    }
                case PaymentTypes.PayOnDelivery:
                    {
                        if (agency.FreePriceShowActive)
                        {
                            if (postReservationRequest.SpecialOneWayFee == -1)
                            {
                                return reservationToken.APIOneWayFee;
                            }
                            else
                            {
                                return postReservationRequest.SpecialOneWayFee;
                            }
                        }

                        return reservationToken.APIOneWayFee;
                    }
                case PaymentTypes.AdvancePayment:
                    {
                        if ((agency.FreePriceShowActive && postReservationRequest.SpecialOneWayFee != -1) || agency.AdvancePaymentAmountByAgencyCommissionActive)
                        {
                            return postReservationRequest.PaidAmount;
                        }
                        else
                        {
                            return 0;
                        }
                    }
            }
        }

        public static float GetAPIOneWayFee(Agency agency, ReservationToken reservationToken, PostReservationRequestV2 postReservationRequest, Reservation reservation)
        {
            switch (postReservationRequest.Payment.PaymentType)
            {
                default:
                case PaymentTypes.PayToAgency:
                case PaymentTypes.CommissionFree:
                case PaymentTypes.PayAll:
                    {
                        if (postReservationRequest.Payment.OneWayFeePayToDelivery) //Ödeme tipi PayToAgency değil ise her zaman false geleceği için ödeme tipi kontrolü yapılmasına gerek yok
                        {
                            return 0;
                        }
                        else
                        {
                            if (agency.FreePriceShowActive)
                            {
                                if (postReservationRequest.Pricing.SpecialOneWayFee == -1)
                                {
                                    return reservationToken.APIOneWayFee;
                                }
                                else if (postReservationRequest.Pricing.SpecialOneWayFee < reservationToken.APIOneWayFee)
                                {
                                    return postReservationRequest.Pricing.SpecialOneWayFee;
                                }
                                else
                                {
                                    return reservationToken.APIOneWayFee;
                                }
                            }
                            else
                            {
                                return reservationToken.APIOneWayFee;
                            }
                        }
                    }
                case PaymentTypes.PayOnDelivery:
                    {
                        if (agency.FreePriceShowActive)
                        {
                            if (postReservationRequest.Pricing.SpecialOneWayFee == -1)
                            {
                                return reservationToken.APIOneWayFee;
                            }
                            else
                            {
                                return postReservationRequest.Pricing.SpecialOneWayFee;
                            }
                        }

                        return reservationToken.APIOneWayFee;
                    }
                case PaymentTypes.AdvancePayment:
                    {
                        if ((agency.FreePriceShowActive && postReservationRequest.Pricing.SpecialOneWayFee != -1) || agency.AdvancePaymentAmountByAgencyCommissionActive)
                        {
                            return postReservationRequest.Pricing.PaidAmount;
                        }
                        else
                        {
                            return 0;
                        }
                    }
            }
        }

        public static float GetAPIExtraPrice(Agency agency, ReservationToken reservationToken, PostReservationRequest postReservationRequest, Reservation reservation)
        {
            switch (postReservationRequest.PaymentType)
            {
                default:
                case PaymentTypes.PayOnDelivery:
                case PaymentTypes.PayToAgency:
                case PaymentTypes.CommissionFree:
                case PaymentTypes.PayAll:
                    {
                        if (postReservationRequest.ExtraPricePayToDelivery)  //Ödeme tipi PayToAgency değil ise her zaman false geleceği için ödeme tipi kontrolü yapılmasına gerek yok
                        {
                            return 0;
                        }
                        else
                        {
                            if (agency.FreePriceShowActive)
                            {
                                if (postReservationRequest.ExtraAmount < reservation.APIExtraAmount)
                                {
                                    return postReservationRequest.ExtraAmount;
                                }
                                else
                                {
                                    return reservation.APIExtraAmount;
                                }
                            }
                            else
                            {
                                return reservation.APIExtraAmount;
                            }
                        }
                    }
                case PaymentTypes.AdvancePayment:
                    {
                        return reservation.APIExtraAmount;
                    }
            }
        }
        public static float GetAPIExtraPrice(Agency agency, ReservationToken reservationToken, PostReservationRequestV2 postReservationRequest, Reservation reservation)
        {
            switch (postReservationRequest.Payment.PaymentType)
            {
                default:
                case PaymentTypes.PayOnDelivery:
                case PaymentTypes.PayToAgency:
                case PaymentTypes.CommissionFree:
                case PaymentTypes.PayAll:
                    {
                        if (postReservationRequest.Payment.ExtraPricePayToDelivery)  //Ödeme tipi PayToAgency değil ise her zaman false geleceği için ödeme tipi kontrolü yapılmasına gerek yok
                        {
                            return 0;
                        }
                        else
                        {
                            if (agency.FreePriceShowActive)
                            {
                                if (postReservationRequest.Pricing.ExtraAmount < reservation.APIExtraAmount)
                                {
                                    return postReservationRequest.Pricing.ExtraAmount;
                                }
                                else
                                {
                                    return reservation.APIExtraAmount;
                                }
                            }
                            else
                            {
                                return reservation.APIExtraAmount;
                            }
                        }
                    }
                case PaymentTypes.AdvancePayment:
                    {
                        return reservation.APIExtraAmount;
                    }
            }
        }
        public static float GetAPIPaidAmount(Agency agency, Vendor vendor, ReservationToken reservationToken, Reservation reservation, PostReservationRequest postReservationRequest, List<ExchangeRates> exchangeRates = null)
        {
            switch (postReservationRequest.PaymentType)
            {
                default:
                case PaymentTypes.PayToAgency:
                case PaymentTypes.CommissionFree:
                case PaymentTypes.PayAll:
                    {
                        float dailyPrice = CalculateAPIPrice(postReservationRequest.SpecialDailyPrice, reservationToken.APIDailyPrice, agency.FreePriceActive, agency.FreePriceShowActive, vendor.ProfitMarkupDailyPrice, postReservationRequest.PaymentType, vendor.AdditionalProductWorkingType);
                        float oneWayFee = CalculateAPIPrice(postReservationRequest.SpecialOneWayFee, reservationToken.APIOneWayFee, agency.FreePriceActive, agency.FreePriceShowActive, vendor.ProfitMarkupOneWayFee, postReservationRequest.PaymentType, vendor.AdditionalProductWorkingType);

                        return CalculateTotalPrice(
                            vendor.PriceRoundingType,
                            reservationToken.RentalDuration,
                            dailyPrice,
                            agency.FreePriceActive ? postReservationRequest.ExtraAmount : reservation.APIExtraAmount,
                            oneWayFee);
                    }
                case PaymentTypes.AdvancePayment:
                    {
                        return postReservationRequest.PaidAmount;
                    }
                case PaymentTypes.PayOnDelivery:
                    {
                        return 0;
                    }
            }
        }

        public static void SetVehiclesPrices(List<Vehicle> vehicleList, Vendor vendor, List<ExchangeRates> exchangeRates, CurrencyTypes requestCurrencyType, CurrencyTypes baseVendorRequestCurrencyType)
        {
            if (requestCurrencyType != baseVendorRequestCurrencyType)
                foreach (var vehicle in vehicleList)
                {
                    vehicle.DailyPrice = CurrencyExchange(exchangeRates, vendor, vehicle.DailyPrice, baseVendorRequestCurrencyType, requestCurrencyType);
                    vehicle.DailyPricePayNow = vehicle.DailyPrice;
                    vehicle.ExtraPrice = CurrencyExchange(exchangeRates, vendor, vehicle.ExtraPrice, baseVendorRequestCurrencyType, requestCurrencyType);
                    vehicle.OneWayFee = CurrencyExchange(exchangeRates, vendor, vehicle.OneWayFee, baseVendorRequestCurrencyType, requestCurrencyType);
                    vehicle.DepositPrice = (vendor.UseLocalDeposit != null && (bool)vendor.UseLocalDeposit) ? vehicle.DepositPrice : vehicle.DepositPrice != null ? RoundPrice(CurrencyExchange(exchangeRates, vendor, vehicle.DepositPrice.ToFloatNullSafe(), baseVendorRequestCurrencyType, requestCurrencyType), (int)PriceRoundingTypes.RoundUp) : vehicle.DepositPrice;
                }
        }

        public static void SetVehiclePrices(Vehicle vehicle, Vendor vendor, List<ExchangeRates> exchangeRates, CurrencyTypes requestCurrencyType, ReservationToken reservationToken, int rentalDuration, bool useVendorProps = true)
        {
            if (vehicle != null)
            {
                vehicle.ServiceCharge = CurrencyExchange(exchangeRates, vendor, vendor.ServiceCharge, vendor.ServiceChargeCurrencyType, requestCurrencyType);

                if (useVendorProps)
                {
                    vehicle.DepositCreditCardRequired = vendor.DepositCreditCardRequired;
                    vehicle.PersonalNumberRequired = vendor.PersonelNumberRequired;
                }

                vehicle.DailyPrice = reservationToken.DailyPrice;
                vehicle.DailyPricePayNow = reservationToken.DailyPricePayNow;
                vehicle.OneWayFee = reservationToken.OneWayFee;
                vehicle.DepositPrice = reservationToken.DepositPrice;
                vehicle.TotalPrice = reservationToken.DailyPrice * rentalDuration + reservationToken.OneWayFee;
                vehicle.TotalPricePayNow = reservationToken.DailyPricePayNow * rentalDuration + reservationToken.OneWayFee;
            }
        }

        public static void SetExtraPrices(List<Extra> extras, Vendor vendor, List<ExchangeRates> exchangeRates, CurrencyTypes requestCurrencyType, bool addProfitMarkup, bool getAPIPrices, CurrencyTypes baseVendorRequestCurrencyType)
        {
            if (extras != null)
            {
                foreach (var extra in extras)
                {
                    extra.ApiPrice = extra.Price;

                    if (!getAPIPrices && requestCurrencyType != baseVendorRequestCurrencyType)
                        extra.Price = CurrencyExchange(exchangeRates, vendor, extra.Price, baseVendorRequestCurrencyType, requestCurrencyType);
                    if (addProfitMarkup && vendor.ProfitMarkupAdditionalProductsActive)
                        extra.Price = AddProfitMarkup(vendor.ProfitMarkupAdditionalProducts, vendor.PriceRoundingType, extra.Price, vendor.AdditionalProductWorkingType);

                    extra.CurrencyCode = requestCurrencyType.ToString();
                }
            }
        }

        //public static void SetExtraPricesForSpecialExtras(List<Extra> extras, Vendor vendor, List<ExchangeRates> exchangeRates, CurrencyTypes requestCurrencyType, bool addProfitMarkup, bool getAPIPrices)
        //{
        //    if (extras != null)
        //    {
        //        foreach (var extra in extras)
        //        {
        //            if (!getAPIPrices && requestCurrencyType != (CurrencyTypes)extra.CurrencyType)
        //                extra.Price = CurrencyExchange(exchangeRates, vendor, extra.Price, (CurrencyTypes)extra.CurrencyType, requestCurrencyType);
        //            if (addProfitMarkup && vendor.ProfitMarkupAdditionalProductsActive)
        //                extra.Price = AddProfitMarkup(vendor.ProfitMarkupAdditionalProducts, vendor.PriceRoundingType, extra.Price, vendor.AdditionalProductWorkingType);

        //            extra.CurrencyCode = requestCurrencyType.ToString();
        //            extra.CurrencyType = requestCurrencyType;
        //        }
        //    }
        //}

        public static void SetExtraPrices(List<ReservationExtra> extras, Vendor vendor, List<ExchangeRates> exchangeRates, CurrencyTypes requestCurrencyType, bool addProfitMarkup, bool getAPIPrices, CurrencyTypes baseVendorRequestCurrencyType)
        {
            if (extras != null)
            {
                foreach (var extra in extras)
                {
                    if (!getAPIPrices && requestCurrencyType != baseVendorRequestCurrencyType)
                        extra.Price = CurrencyExchange(exchangeRates, vendor, extra.Price, baseVendorRequestCurrencyType, requestCurrencyType);
                    if (addProfitMarkup && vendor.ProfitMarkupAdditionalProductsActive)
                        extra.Price = AddProfitMarkup(vendor.ProfitMarkupAdditionalProducts, vendor.PriceRoundingType, extra.Price, vendor.AdditionalProductWorkingType);
                }
            }
        }

        public static ProfitMarkup GetFilteredProfitMarkup(List<ProfitMarkup> profitMarkups, List<ExchangeRates> exchangeRates, Vendor vendor, float dailyPrice, CurrencyTypes requestCurrencyType, int rentalDuration)
        {
            return profitMarkups?
                .Where(pr =>
                {
                    float? convertedMinimum = pr.MinimumAmount.HasValue
                        ? CurrencyExchange(exchangeRates, vendor, pr.MinimumAmount.ToFloatNullSafe(), (CurrencyTypes)(pr.AmountCurrencyId - 1), requestCurrencyType)
                        : null;

                    float? convertedMaximum = pr.MaximumAmount.HasValue
                        ? CurrencyExchange(exchangeRates, vendor, pr.MaximumAmount.ToFloatNullSafe(), (CurrencyTypes)(pr.AmountCurrencyId - 1), requestCurrencyType)
                        : null;

                    return
                        (convertedMinimum == 0 || convertedMinimum <= (dailyPrice)) &&
                        (convertedMaximum == 0 || convertedMaximum >= (dailyPrice));
                })
                .OrderBy(p => p.Priority == 0 ? int.MaxValue : p.Priority ?? int.MaxValue)
                .FirstOrDefault();
        }

    }
}
