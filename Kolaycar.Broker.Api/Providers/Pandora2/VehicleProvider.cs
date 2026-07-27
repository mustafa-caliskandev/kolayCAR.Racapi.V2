using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Pandora2;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Pandora2
{
    public class VehicleProvider : IVehicleProvider
    {
        private const string PayNowPaymentType = "Now";
        private const string PayLocalPaymentType = "Local";

        private RestManager RestManager { get; set; }
        private AuthProvider AuthProvider { get; set; }

        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            RestManager = new RestManager(AuthProvider.NormalizeBaseUrl(vendor.APIBaseUrl), timeout: disableTimeout ? 0 : vendor.APITimeout);
            AuthProvider = new AuthProvider(vendor.APIBaseUrl);
        }

        public Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            return Task.FromResult(new ServiceResponseBase(new List<Vehicle>(), true));
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var auth = await AuthProvider.GetAccessToken(vendor);

            if (auth != null && !string.IsNullOrWhiteSpace(auth.access_token))
            {
                var result = await RestManager.PostAsyncRestClient<List<Pandora2ResponseBase.AvailableVehicle>>(
                     requestPath: "bookings/availability",
                     parameterType: ParameterType.RequestBody,
                     headers: AuthProvider.CreateAuthHeaderWithContentType(auth.access_token, getVehiclesRequest.LanguageCode),
                     entity: AuthProvider.CreateJsonBody(CreateAvailabilityRequest(additionalInformation)));

                if (result?.Count > 0)
                {
                    var oneWayFeePayToDelivery = GetOneWayFeePayToDelivery(additionalInformation);

                    result = result
                        .Where(vehicle =>
                            !HasMandatoryExtra(vehicle) &&
                            !HasOtherFee(vehicle) &&
                            IsDropOffFeePaymentTypeAllowed(vehicle, oneWayFeePayToDelivery))
                        .ToList();

                    if (result.Count == 0)
                        return new ServiceResponseBase(new List<Vehicle>(), true);

                    var extraPricePayToDelivery = GetExtraPricePayToDelivery(additionalInformation);
                    var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result, x => x.Id, x => Pandora2MapperHelper.ToMoney(x.NetTotalAmount));
                    var mappedVehicleList = apiVehicleList.Map(additionalInformation, vendor, extraPricePayToDelivery);
                    var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                    if (vendor.VehicleMappingActive)
                    {
                        mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                        apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.Id));
                    }

                    CalculationHelper.SetVehiclesPrices(mappedVehicleList, vendor, exchangeRates, requestCurrencyType, baseVendorRequestCurrencyType);
                    VehicleHelper.SetVehiclesProperties(mappedVehicleList, vendor, additionalInformation.Agency, exchangeRates, baseVendorRequestCurrencyType, requestCurrencyType, profitMarkups);

                    if (mappedVehicleList.Any(vehicle => Math.Abs(vehicle.RentalDuration - additionalInformation.RentalDuration) > 1))
                        return new ServiceResponseBase(null, false, "Yanlis gun sayisi");

                    var pickupDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime);
                    var returnDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime);
                    var languageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>();

                    foreach (var vehicle in apiVehicleList.Select((value, index) => new { value, index }))
                    {
                        var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.Id).ToList();

                        for (int i = 0; i < tempMappedVehicleList.Count; i++)
                        {
                            var mappedVehicle = tempMappedVehicleList[i];
                            var apiDailyPrice = Pandora2MapperHelper.ToMoney(vehicle.value.NetDailyRentAmount);
                            var netTotalRent = Pandora2MapperHelper.ToMoney(vehicle.value.NetTotalRentAmount);
                            var netTotalAmount = Pandora2MapperHelper.ToMoney(vehicle.value.NetTotalAmount);
                            var apiOneWayFee = netTotalAmount > netTotalRent ? netTotalAmount - netTotalRent : 0;

                            var reservationToken = new ReservationToken
                            {
                                AgencyId = additionalInformation.Agency.AgencyId,
                                VendorId = vendor.VendorId,
                                APIVendorId = vendor.VendorId,
                                APIVendorName = vehicle.value.Supplier?.Name ?? vendor.VendorName,
                                APIVendorPhone = vehicle.value.Supplier?.Phone ?? vendor.VendorPhone,
                                APIVendorEmail = vendor.VendorEmail,
                                APIVendorLogo = vehicle.value.Supplier?.Logo ?? vendor.Logo,
                                VehicleId = mappedVehicle.VehicleId,
                                VehicleCode = vehicle.value.Id,
                                APIPickupLocationId = additionalInformation.APIPickupLocationId,
                                APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                                APIReturnLocationId = additionalInformation.APIReturnLocationId,
                                APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                                CurrencyType = requestCurrencyType,
                                RentalDuration = mappedVehicle.RentalDuration,
                                DailyPrice = mappedVehicle.DailyPrice,
                                OneWayFee = mappedVehicle.OneWayFee,
                                DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                                APIDailyPrice = apiDailyPrice,
                                APIDailyPricePayNow = apiDailyPrice,
                                APITotalPrice = netTotalAmount,
                                APITotalPricePayNow = netTotalAmount,
                                APIOneWayFee = apiOneWayFee,
                                APIReferenceCode = netTotalAmount.ToString(),
                                APIReferenceCode2 = vehicle.value.Supplier?.Id,
                                APIReferenceCode3 = vehicle.value.TotalAmount,
                                DepositPrice = mappedVehicle.DepositPrice,
                                VendorMinimumDriverAge = mappedVehicle.VendorMinimumDriverAge ?? 0,
                                VendorMinimumDrivingLicenseAge = mappedVehicle.VendorMinimumDrivingLicenseAge ?? 0,
                                ServiceCharge = mappedVehicle.ServiceCharge,
                                FuelType = mappedVehicle.FuelType,
                                TransmissionType = mappedVehicle.TransmissionType,
                                DepositCreditCardRequired = mappedVehicle.DepositCreditCardRequired,
                                PickupLocationId = getVehiclesRequest.PickupLocationId,
                                ReturnLocationId = getVehiclesRequest.ReturnLocationId,
                                LanguageType = languageType,
                                PickupDateTime = pickupDateTime,
                                ReturnDateTime = returnDateTime,
                                VehicleName = mappedVehicle.VehicleName,
                                VehicleImageUrl = mappedVehicle.VehicleImages.Count > 0 ? mappedVehicle.VehicleImages[0].Url : string.Empty,
                                BaseVendorRequestCurrencyType = baseVendorRequestCurrencyType,
                                SpecialProfitApplied = mappedVehicle.SpecialProfitApplied,
                                BaggageQuantityType = mappedVehicle.BaggageQuantityType,
                                PassangerQuantityType = mappedVehicle.PassangerQuantityType,
                                TotalKmLimit = mappedVehicle.TotalKMLimit ?? 0,
                                VehicleCategoryType = mappedVehicle.VehicleCategoryType,
                                VehicleType = mappedVehicle.VehicleType,
                                VendorFlightPassRequired = vendor.FlightNumberRequired ?? false,
                                FullCredit = mappedVehicle.FullCredit,
                                SippCode = mappedVehicle.SippCode,
                                VendorNote = vehicle.value.Supplier?.SpecialInstructions
                            };

                            mappedVehicle.ReservationToken = reservationToken.ToJson();

                            if (additionalInformation.Agency.SpecialParameters)
                            {
                                mappedVehicle.SpecialVendorId = vendor.VendorId.ToString();
                                mappedVehicle.SpecialVendorName = vendor.VendorName;
                                mappedVehicle.SpecialVendorLogo = vendor.Logo;
                            }
                        }
                    }

                    return new ServiceResponseBase(mappedVehicleList, mappedVehicleList.Count > 0);
                }
            }

            return new ServiceResponseBase(null, false, "Kimlik dogrulama islemi basarisiz!");
        }

        private static AvailabilityRequest CreateAvailabilityRequest(ResponseReservationStepsAdditionalInformation additionalInformation) =>
            new AvailabilityRequest
            {
                OfficeOutId = additionalInformation.APIPickupLocationCode,
                OfficeInId = additionalInformation.APIReturnLocationCode,
                DateOut = FormatApiDate(additionalInformation.PickupDateTime),
                DateIn = FormatApiDate(additionalInformation.ReturnDateTime),
                DriverAge = 30,
                ResidenceCountryCode = "TR"
            };

        internal static string FormatApiDate(DateTime dateTime) =>
            dateTime.ToString("yyyy-MM-ddTHH:mm:ss") + "+03:00";

        private static bool HasMandatoryExtra(Pandora2ResponseBase.AvailableVehicle vehicle) =>
            vehicle?.Extras != null && vehicle.Extras.Any(extra => extra.Mandatory == true);

        private static bool HasOtherFee(Pandora2ResponseBase.AvailableVehicle vehicle) =>
            vehicle?.OtherFees?.Any() == true;

        private static bool IsDropOffFeePaymentTypeAllowed(Pandora2ResponseBase.AvailableVehicle vehicle, bool oneWayFeePayToDelivery)
        {
            var dropOffFee = vehicle?.DropOffFee;

            if (dropOffFee == null || Pandora2MapperHelper.ToMoney(dropOffFee.NetAmount) <= 0)
                return true;

            var expectedPaymentType = oneWayFeePayToDelivery ? PayLocalPaymentType : PayNowPaymentType;

            return string.Equals(dropOffFee.PaymentType?.Trim(), expectedPaymentType, StringComparison.OrdinalIgnoreCase);
        }

        private static bool GetExtraPricePayToDelivery(ResponseReservationStepsAdditionalInformation additionalInformation) =>
            additionalInformation?.Agency?.AdditionalProductAmountDeliveryPayment ?? false;

        private static bool GetOneWayFeePayToDelivery(ResponseReservationStepsAdditionalInformation additionalInformation) =>
            additionalInformation?.Agency?.OneWayAmountDeliveryPayment ?? false;
    }

    public class AvailabilityRequest
    {
        public string OfficeOutId { get; set; }
        public string OfficeInId { get; set; }
        public string DateOut { get; set; }
        public string DateIn { get; set; }
        public int DriverAge { get; set; }
        public string ResidenceCountryCode { get; set; }
    }
}
