using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Pandora2
{
    public class ExtraProvider : IExtraProvider
    {
        private const float DailyPriceTolerance = 0.10f;

        private IVehicleProvider VehicleProvider { get; set; }

        public Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            return Task.FromResult(new ServiceResponseBase(new List<Extra>(), true));
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();

            VehicleProvider = new VehicleProvider(vendor, false);
            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);
            var getVehiclesResponse = await VehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = FindSelectedVehicle(vehicles, reservationToken);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false, "Arac bilgisi alinamadi.");

            reservationToken.VehicleCode = selectedVehicle.VehicleCode;

            var extras = selectedVehicle.Extras ?? new List<Extra>();

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            return new ServiceResponseBase(new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle), true);
        }

        private static Vehicle FindSelectedVehicle(List<Vehicle> vehicles, ReservationToken reservationToken)
        {
            if (vehicles == null || vehicles.Count == 0 || reservationToken == null)
                return null;

            return FindBestVehicle(vehicles, reservationToken, vehicle =>
                       SameText(vehicle.SippCode, reservationToken.SippCode) &&
                       SameText(vehicle.VehicleName, reservationToken.VehicleName) &&
                       SamePrice(vehicle.DailyPrice, reservationToken.DailyPrice))
                   ?? FindBestVehicle(vehicles, reservationToken, vehicle =>
                       SameText(vehicle.SippCode, reservationToken.SippCode) &&
                       SameText(vehicle.VehicleName, reservationToken.VehicleName))
                   ?? FindBestVehicle(vehicles, reservationToken, vehicle =>
                       SameText(vehicle.VehicleCode, reservationToken.VehicleCode));
        }

        private static Vehicle FindBestVehicle(List<Vehicle> vehicles, ReservationToken reservationToken, Func<Vehicle, bool> predicate) =>
            vehicles
                .Where(predicate)
                .OrderBy(vehicle => GetPriceDifference(vehicle, reservationToken))
                .FirstOrDefault();

        private static float GetPriceDifference(Vehicle vehicle, ReservationToken reservationToken)
        {
            var vehicleDailyPrice = vehicle.DailyPrice;
            var tokenDailyPrice = reservationToken.DailyPrice;

            if (vehicleDailyPrice <= 0 || tokenDailyPrice <= 0)
                return 0;

            return Math.Abs(vehicleDailyPrice - tokenDailyPrice);
        }

        private static bool SamePrice(float left, float right) =>
            Math.Abs(left - right) <= DailyPriceTolerance;

        private static bool SameText(string left, string right)
        {
            var normalizedLeft = NormalizeText(left);
            var normalizedRight = NormalizeText(right);

            return !string.IsNullOrEmpty(normalizedLeft) &&
                   !string.IsNullOrEmpty(normalizedRight) &&
                   string.Equals(normalizedLeft, normalizedRight, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeText(string value) =>
            string.Join(" ", value.ToStringNullSafe().Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
