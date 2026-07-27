using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CommonModels = KolayCAR.Broker.Domain.Models;
using KolayCARResponse = KolayCAR.Broker.Domain.Models.Response;

namespace KolayCAR.Broker.API.Helpers.KolayCAR
{
    public static class MoneyHelper
    {
        public const decimal NotSet = -1m;

        public static decimal ToMoney(float value) =>
            decimal.Parse(value.ToString("R", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

        public static decimal ToMoney(float? value) => value.HasValue ? ToMoney(value.Value) : 0m;

        public static float ToFloat(decimal value) => (float)Math.Round(value, 2);

        public static string Format(decimal value) =>
            Math.Round(value, 2).ToString("0.##", CultureInfo.InvariantCulture);

        public static decimal RoundPrice(decimal price, int decimals) => Math.Round(price, decimals);

        public static decimal RoundPrice(decimal price, PriceRoundingTypes priceRoundingType) =>
            RoundPrice(price, (int)priceRoundingType);

        public static decimal AddProfitMarkup(decimal profitMarkup, PriceRoundingTypes priceRoundingType, decimal price, VendorWorkingTypes vendorWorkingType) =>
            vendorWorkingType == VendorWorkingTypes.ProfitMarkup
                ? RoundPrice(price + price * profitMarkup / 100m, priceRoundingType)
                : RoundPrice(price, priceRoundingType);

        public static decimal AdjustProfitMarkup(decimal profitMarkup, PriceRoundingTypes priceRoundingType, decimal price, int markupType, int type) =>
            markupType == 0
                ? (type == 1
                    ? RoundPrice(price + price * profitMarkup / 100m, priceRoundingType)
                    : RoundPrice(price - price * profitMarkup / 100m, priceRoundingType))
                : (type == 1
                    ? RoundPrice(price + profitMarkup, priceRoundingType)
                    : RoundPrice(price - profitMarkup, priceRoundingType));

        public static decimal RemoveProfitMarkup(decimal profitMarkup, PriceRoundingTypes priceRoundingType, decimal price) =>
            RoundPrice(price * 100m / (100m + profitMarkup), priceRoundingType);

        public static decimal CalculateTotalPrice(PriceRoundingTypes priceRoundingType, int rentalDuration, decimal dailyPrice, decimal extraPrice, decimal oneWayFee) =>
            RoundPrice(dailyPrice * rentalDuration + extraPrice + oneWayFee, priceRoundingType);

        public static decimal MakeDiscount(decimal price, decimal percent) => price * (100m - percent) / 100m;

        public static decimal CurrencyExchange(List<ExchangeRates> exchangeRates, CommonModels.Vendor vendor, decimal price, CurrencyTypes sourceCurrency, CurrencyTypes targetCurrency)
        {
            if (sourceCurrency == targetCurrency)
                return RoundPrice(price, vendor.PriceRoundingType);

            var sourceExchangeRate = exchangeRates?.FirstOrDefault(x => x.CurrencyType == sourceCurrency);
            var targetExchangeRate = exchangeRates?.FirstOrDefault(x => x.CurrencyType == targetCurrency);

            if (sourceExchangeRate == null || targetExchangeRate == null || targetExchangeRate.ExchangeRate == 0)
                return RoundPrice(price, vendor.PriceRoundingType);

            return RoundPrice(price * ToMoney(sourceExchangeRate.ExchangeRate) / ToMoney(targetExchangeRate.ExchangeRate), vendor.PriceRoundingType);
        }

        public static decimal CalculateAPIPrice(decimal requestPrice, decimal apiPrice, bool freePriceActive, bool freePriceShowActive, decimal profitMarkup, PaymentTypes paymentType, VendorWorkingTypes additionalProductWorkingType)
        {
            var requestPriceWithoutProfitMarkup = additionalProductWorkingType == VendorWorkingTypes.ProfitMarkup
                ? RemoveProfitMarkup(profitMarkup, PriceRoundingTypes.DoNotRounding, requestPrice)
                : requestPrice;

            if (paymentType == PaymentTypes.PayOnDelivery)
                return requestPrice;

            if (freePriceShowActive)
                return requestPriceWithoutProfitMarkup >= apiPrice ? apiPrice : apiPrice;

            return apiPrice;
        }

        public static VehiclePriceSnapshot ApplyVehiclePrices(
            CommonModels.Vehicle vehicle,
            KolayCARResponse.VEHICLE apiVehicle,
            CommonModels.Vendor vendor,
            CommonModels.Agency agency,
            List<ExchangeRates> exchangeRates,
            CurrencyTypes baseVendorRequestCurrencyType,
            CurrencyTypes requestCurrencyType,
            List<ProfitMarkup> profitMarkups = null)
        {
            var snapshot = CalculateVehiclePrices(
                vehicle,
                apiVehicle,
                vendor,
                agency,
                exchangeRates,
                baseVendorRequestCurrencyType,
                requestCurrencyType,
                profitMarkups);

            vehicle.DailyPrice = ToFloat(snapshot.DailyPrice);
            vehicle.DailyPricePayNow = ToFloat(snapshot.DailyPricePayNow);
            vehicle.ExtraPrice = ToFloat(snapshot.ExtraPrice);
            vehicle.OneWayFee = ToFloat(snapshot.OneWayFee);
            vehicle.TotalPrice = ToFloat(snapshot.TotalPrice);
            vehicle.TotalPricePayNow = ToFloat(snapshot.TotalPricePayNow);
            vehicle.ApiDailyPrice = ToFloat(snapshot.DisplayApiDailyPrice);
            vehicle.DepositPrice = snapshot.DepositPrice.HasValue ? ToFloat(snapshot.DepositPrice.Value) : null;
            vehicle.ServiceCharge = ToFloat(snapshot.ServiceCharge);
            vehicle.DepositCreditCardRequired = vendor.DepositCreditCardRequired;
            vehicle.PersonalNumberRequired = vendor.PersonelNumberRequired;
            vehicle.BaseVendorCurrencyTypes = baseVendorRequestCurrencyType;

            return snapshot;
        }

        public static VehiclePriceSnapshot CalculateVehiclePrices(
            CommonModels.Vehicle vehicle,
            KolayCARResponse.VEHICLE apiVehicle,
            CommonModels.Vendor vendor,
            CommonModels.Agency agency,
            List<ExchangeRates> exchangeRates,
            CurrencyTypes baseVendorRequestCurrencyType,
            CurrencyTypes requestCurrencyType,
            List<ProfitMarkup> profitMarkups = null)
        {
            var dailyPrice = apiVehicle.DAILYPRICE;
            var dailyPricePayNow = apiVehicle.DAILYPRICEPAYNOW;
            var extraPrice = apiVehicle.EXTRAPRICE;
            var oneWayFee = apiVehicle.ONEWAYFEE;
            decimal? depositPrice = apiVehicle.DEPOSITPRICE;

            if (requestCurrencyType != baseVendorRequestCurrencyType)
            {
                dailyPrice = CurrencyExchange(exchangeRates, vendor, dailyPrice, baseVendorRequestCurrencyType, requestCurrencyType);
                dailyPricePayNow = dailyPrice;
                extraPrice = CurrencyExchange(exchangeRates, vendor, extraPrice, baseVendorRequestCurrencyType, requestCurrencyType);
                oneWayFee = CurrencyExchange(exchangeRates, vendor, oneWayFee, baseVendorRequestCurrencyType, requestCurrencyType);

                if (vendor.UseLocalDeposit != true && depositPrice.HasValue)
                    depositPrice = RoundPrice(CurrencyExchange(exchangeRates, vendor, depositPrice.Value, baseVendorRequestCurrencyType, requestCurrencyType), PriceRoundingTypes.RoundUp);
            }

            if (vendor.UseLocalDeposit == true && vehicle.DepositPrice.HasValue)
                depositPrice = ToMoney(vehicle.DepositPrice.Value);

            depositPrice = vendor.DisableDeposit ? 0m : depositPrice;

            var displayApiDailyPrice = dailyPrice;
            var serviceCharge = CurrencyExchange(exchangeRates, vendor, ToMoney(vendor.ServiceCharge), vendor.ServiceChargeCurrencyType, requestCurrencyType);

            if (vendor.ProfitMarkupDailyPriceActive)
            {
                var profitMarkupDailyPrice = ToMoney(vendor.ProfitMarkupDailyPrice);
                dailyPrice = AddProfitMarkup(profitMarkupDailyPrice, vendor.PriceRoundingType, dailyPrice, vendor.RentalWorkingType);
                dailyPricePayNow = AddProfitMarkup(profitMarkupDailyPrice, vendor.PriceRoundingType, dailyPricePayNow, vendor.RentalWorkingType);
            }

            var profitMarkup = GetFilteredProfitMarkup(profitMarkups, exchangeRates, vendor, dailyPrice, requestCurrencyType, vehicle.RentalDuration);
            string specialProfitApplied = null;

            if (profitMarkup != null)
            {
                var convertedProfitMarkup = profitMarkup.MarkupType == 1
                    ? CurrencyExchange(exchangeRates, vendor, ToMoney(profitMarkup.MarkupValue), (CurrencyTypes)(profitMarkup.CurrencyId - 1), requestCurrencyType)
                    : ToMoney(profitMarkup.MarkupValue);

                dailyPrice = AdjustProfitMarkup(convertedProfitMarkup, vendor.PriceRoundingType, dailyPrice, profitMarkup.MarkupType, profitMarkup.Type);
                dailyPricePayNow = AdjustProfitMarkup(convertedProfitMarkup, vendor.PriceRoundingType, dailyPricePayNow, profitMarkup.MarkupType, profitMarkup.Type);
                specialProfitApplied = StringHelper.SpecialProfitMarkup(ToFloat(convertedProfitMarkup), profitMarkup, requestCurrencyType);
            }

            if (!agency.FreePriceShowActive)
            {
                var agencyProfitMarkup = ToMoney(agency.AgencyRentalProfitMarkup);
                dailyPrice = AddProfitMarkup(agencyProfitMarkup, vendor.PriceRoundingType, dailyPrice, VendorWorkingTypes.ProfitMarkup);
                dailyPricePayNow = AddProfitMarkup(agencyProfitMarkup, vendor.PriceRoundingType, dailyPricePayNow, VendorWorkingTypes.ProfitMarkup);
            }

            if (vendor.ProfitMarkupOneWayFeeActive)
                oneWayFee = AddProfitMarkup(ToMoney(vendor.ProfitMarkupOneWayFee), vendor.PriceRoundingType, oneWayFee, vendor.OneWayFeeWorkingType);

            if (vendor.ProfitMarkupAdditionalProductsActive)
                extraPrice = AddProfitMarkup(ToMoney(vendor.ProfitMarkupAdditionalProducts), vendor.PriceRoundingType, extraPrice, vendor.AdditionalProductWorkingType);

            if (agency.PayAllActive && agency.CreditCardDiscountPercent != 0)
                dailyPricePayNow = RoundPrice(MakeDiscount(dailyPricePayNow, agency.CreditCardDiscountPercent), vendor.PriceRoundingType);

            var totalPrice = CalculateTotalPrice(vendor.PriceRoundingType, vehicle.RentalDuration, dailyPrice, extraPrice, oneWayFee);
            var totalPricePayNow = CalculateTotalPrice(vendor.PriceRoundingType, vehicle.RentalDuration, dailyPricePayNow, extraPrice, oneWayFee);

            vehicle.SpecialProfitApplied = specialProfitApplied;

            return new VehiclePriceSnapshot
            {
                DailyPrice = dailyPrice,
                OneWayFee = oneWayFee,
                ExtraPrice = extraPrice,
                TotalPrice = totalPrice,
                DailyPricePayNow = dailyPricePayNow,
                TotalPricePayNow = totalPricePayNow,
                DisplayApiDailyPrice = displayApiDailyPrice,
                ApiDailyPrice = apiVehicle.DAILYPRICE,
                ApiDailyPricePayNow = apiVehicle.DAILYPRICEPAYNOW,
                ApiOneWayFee = apiVehicle.ONEWAYFEE,
                ApiTotalPrice = apiVehicle.TOTALPRICE,
                DepositPrice = depositPrice,
                ServiceCharge = serviceCharge,
                SpecialProfitApplied = specialProfitApplied
            };
        }

        public static decimal ResolveTokenDailyPrice(ReservationToken reservationToken, CommonModels.Vendor vendor, List<ExchangeRates> exchangeRates) =>
            ResolveRequestCurrencyTokenPrice(reservationToken.KolayCarDailyPrice, ToMoney(reservationToken.DailyPrice), reservationToken, vendor, exchangeRates);

        public static decimal ResolveTokenOneWayFee(ReservationToken reservationToken, CommonModels.Vendor vendor, List<ExchangeRates> exchangeRates) =>
            ResolveRequestCurrencyTokenPrice(reservationToken.KolayCarOneWayFee, ToMoney(reservationToken.OneWayFee), reservationToken, vendor, exchangeRates);

        public static decimal ResolveTokenApiDailyPrice(ReservationToken reservationToken) =>
            reservationToken.KolayCarAPIDailyPrice ?? ToMoney(reservationToken.APIDailyPrice);

        public static decimal ResolveTokenApiOneWayFee(ReservationToken reservationToken) =>
            reservationToken.KolayCarAPIOneWayFee ?? ToMoney(reservationToken.APIOneWayFee);

        private static decimal ResolveRequestCurrencyTokenPrice(decimal? snapshotPrice, decimal fallbackPrice, ReservationToken reservationToken, CommonModels.Vendor vendor, List<ExchangeRates> exchangeRates)
        {
            if (!snapshotPrice.HasValue)
                return fallbackPrice;

            if (reservationToken.CurrencyType == reservationToken.BaseVendorRequestCurrencyType)
                return snapshotPrice.Value;

            return CurrencyExchange(exchangeRates, vendor, snapshotPrice.Value, reservationToken.CurrencyType, reservationToken.BaseVendorRequestCurrencyType);
        }

        private static ProfitMarkup GetFilteredProfitMarkup(
            List<ProfitMarkup> profitMarkups,
            List<ExchangeRates> exchangeRates,
            CommonModels.Vendor vendor,
            decimal dailyPrice,
            CurrencyTypes requestCurrencyType,
            int rentalDuration)
        {
            return profitMarkups?
                .Where(pr =>
                {
                    var convertedMinimum = pr.MinimumAmount.HasValue
                        ? CurrencyExchange(exchangeRates, vendor, pr.MinimumAmount.Value, (CurrencyTypes)(pr.AmountCurrencyId - 1), requestCurrencyType)
                        : (decimal?)null;

                    var convertedMaximum = pr.MaximumAmount.HasValue
                        ? CurrencyExchange(exchangeRates, vendor, pr.MaximumAmount.Value, (CurrencyTypes)(pr.AmountCurrencyId - 1), requestCurrencyType)
                        : (decimal?)null;

                    return (convertedMinimum == 0 || convertedMinimum <= dailyPrice)
                        && (convertedMaximum == 0 || convertedMaximum >= dailyPrice);
                })
                .OrderByDescending(p => p.Priority)
                .FirstOrDefault();
        }
    }

    public class VehiclePriceSnapshot
    {
        public decimal DailyPrice { get; set; }
        public decimal OneWayFee { get; set; }
        public decimal ExtraPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DailyPricePayNow { get; set; }
        public decimal TotalPricePayNow { get; set; }
        public decimal DisplayApiDailyPrice { get; set; }
        public decimal ApiDailyPrice { get; set; }
        public decimal ApiDailyPricePayNow { get; set; }
        public decimal ApiOneWayFee { get; set; }
        public decimal ApiTotalPrice { get; set; }
        public decimal? DepositPrice { get; set; }
        public decimal ServiceCharge { get; set; }
        public string SpecialProfitApplied { get; set; }
    }
}
