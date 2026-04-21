using Kolaycar.Broker.Api.Mappers.Eren;
using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Requests.Eren;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Responses.Eren;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kolaycar.Broker.Api.Providers.Eren
{
    public class VehicleProvider : IVehicleProvider
    {
        private readonly HttpManager _httpManager;
        private readonly AuthProvider _authProvider;

        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            _httpManager = new HttpManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
            _authProvider = new AuthProvider(vendor.APIBaseUrl);
        }
        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var headers = await _authProvider.GetTokenAsync(vendor);

            var response = await _httpManager.PostAsyncWithModel<object, ErenVehicleGroupResponse>(
                "/v1/vehicle-groups",
                headers: headers
            );

            if (response != null && response.VehicleGroups != null && response.VehicleGroups.Count > 0)
            {
                var localVehicles = response.VehicleGroups.Map(vendor.VendorName);
                return new ServiceResponseBase(localVehicles, true);
            }

            return new ServiceResponseBase(null, false, "Araç listesi alınamadı!");
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var accessToken = await _authProvider.GetTokenAsync(vendor);

            var result = await _httpManager.PostAsyncWithModel<ErenSearchRequest, ErenSearchResponse>(
                "/v1/search",
                entity: new ErenSearchRequest
                {
                    PickupId = additionalInformation.APIPickupLocationCode.ToIntNullSafe(),
                    DropoffId = additionalInformation.APIReturnLocationCode.ToIntNullSafe(),
                    PickupDate = additionalInformation.PickupDateTime.ToString("yyyy-MM-ddTHH:mmZ"),
                    DropoffDate = additionalInformation.ReturnDateTime.ToString("yyyy-MM-ddTHH:mmZ"),
                    CurrencyCode = GetCurrencyCode(baseVendorRequestCurrencyType)
                },
                headers: accessToken
            );

            if (result != null && result.AvailableCars != null && result.AvailableCars.Count > 0)
            {
                var apiVehicleList = result.AvailableCars;
                var mappedVehicleList = apiVehicleList.Map(result.SearchRequestId, additionalInformation, vendor);
                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                if (vendor.VehicleMappingActive)
                {
                    mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                    apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.GroupId.ToString()));
                }

                CalculationHelper.SetVehiclesPrices(mappedVehicleList, vendor, exchangeRates, requestCurrencyType, baseVendorRequestCurrencyType);
                VehicleHelper.SetVehiclesProperties(mappedVehicleList, vendor, additionalInformation.Agency, exchangeRates, baseVendorRequestCurrencyType, requestCurrencyType, profitMarkups);
                if (mappedVehicleList.Any(vehicle => Math.Abs(vehicle.RentalDuration - additionalInformation.RentalDuration) > 1))
                    return new ServiceResponseBase(null, false, "Yanlış gün sayısı");

                var pickupDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime);
                var returnDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime);
                var languageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>();

                foreach (var mappedVehicle in mappedVehicleList)
                {
                    var apiVehicle = apiVehicleList.FirstOrDefault(x => x.GroupId.ToString() == mappedVehicle.VehicleCode);
                    if (apiVehicle == null) continue;

                    var reservationToken = new ReservationToken
                    {
                        AgencyId = additionalInformation.Agency.AgencyId,
                        VendorId = vendor.VendorId,
                        APIVendorName = vendor.VendorName,
                        APIVendorPhone = vendor.VendorPhone,
                        APIVendorEmail = vendor.VendorEmail,
                        APIVendorLogo = vendor.Logo,
                        VehicleId = mappedVehicle.VehicleId,
                        VehicleCode = apiVehicle.GroupId.ToStringNullSafe(),
                        APIPickupLocationId = additionalInformation.APIPickupLocationId,
                        APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                        APIReturnLocationId = additionalInformation.APIReturnLocationId,
                        APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                        CurrencyType = requestCurrencyType,
                        RentalDuration = mappedVehicle.RentalDuration,
                        DailyPrice = mappedVehicle.DailyPrice,
                        OneWayFee = mappedVehicle.OneWayFee,
                        DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                        APIDailyPrice = apiVehicle.DailyPrice.ToFloatNullSafe(),
                        APIDailyPricePayNow = apiVehicle.DailyPrice.ToFloatNullSafe(),
                        APIOneWayFee = apiVehicle.DropPrice.ToFloatNullSafe(),
                        APIReferenceCode = result.SearchRequestId,
                        APIReferenceCode2 = apiVehicle.QuoteId,
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
                        SippCode = mappedVehicle.SippCode
                    };

                    mappedVehicle.ReservationToken = reservationToken.ToJson();

                    if (additionalInformation.Agency.SpecialParameters)
                    {
                        mappedVehicle.SpecialVendorId = vendor.VendorId.ToString();
                        mappedVehicle.SpecialVendorName = vendor.VendorName;
                        mappedVehicle.SpecialVendorLogo = vendor.Logo;
                    }
                }
                return new ServiceResponseBase(mappedVehicleList, mappedVehicleList.Count > 0);
            }
            return new ServiceResponseBase(null, false, $"{vendor.VendorName} servisinden araç bulunamadı!");
        }

        private string GetCurrencyCode(CurrencyTypes baseVendorRequestCurrencyType) => baseVendorRequestCurrencyType switch { CurrencyTypes.TRY => "TRY", CurrencyTypes.USD => "USD", CurrencyTypes.EUR => "EUR", _ => "" };

    }
}
