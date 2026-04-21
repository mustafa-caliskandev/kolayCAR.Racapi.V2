using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Garajlar;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.GarajlarRequestBase;
using static KolayCAR.Broker.Domain.Models.Response.GarajlarResponseBase;

namespace KolayCAR.Broker.API.Providers.Garajlar
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
            var token = await _authProvider.GetTokenHeader(vendor);
            if (token is null)
                return new(null, false, $"{vendor.VendorName} token bilgisi alınamadı!");

            var vehicleList = await _httpManager.GetAsyncWithModel<ResponseBase<List<GarajlarVehicle>>>("/api/obilet/get-vehicles", headers: token);

            return vehicleList?.data?.Count > 0
                ? new(vehicleList.data.Map(vendor), true)
                : new(null, false, $"{vendor.VendorName} araç listesi alınamadı!");

        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var token = await _authProvider.GetTokenHeader(vendor);
            if (token is null)
                return new(null, false, $"{vendor.VendorName} token bilgisi alınamadı!");

            var result = await _httpManager.PostAsyncWithModel<object, ResponseBase<List<AvailabilityVehiclesResponse>>>(
             requestPath: "/api/obilet/query-reservation",
             headers: token,
             entity: GetEntity(additionalInformation));

            if (!(result?.data?.Any() ?? false))
                return new(null, false, $"{vendor.VendorName} servisinden müsait araç bulunamadı!");

            var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result.data, v => v.sub_group_short_name, v => v.daily_price.ToFloatNullSafe());
            var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var mappedVehicleList = result.data.Map(vendor, additionalInformation);

            if (vendor.VehicleMappingActive)
            {
                mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.sub_group_short_name));
            }

            CalculationHelper.SetVehiclesPrices(mappedVehicleList, vendor, exchangeRates, requestCurrencyType, baseVendorRequestCurrencyType);
            VehicleHelper.SetVehiclesProperties(mappedVehicleList, vendor, additionalInformation.Agency, exchangeRates, baseVendorRequestCurrencyType, requestCurrencyType, profitMarkups);
            if (mappedVehicleList.Any(vehicle => Math.Abs(vehicle.RentalDuration - additionalInformation.RentalDuration) > 1))
                return new ServiceResponseBase(null, false, "Yanlış gün sayısı");

            var pickupDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime);
            var returnDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime);
            var languageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>();

            foreach (var vehicle in apiVehicleList)
            {
                var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.sub_group_short_name).ToList();

                for (int i = 0; i < tempMappedVehicleList.Count; i++)
                {
                    var mappedVehicle = tempMappedVehicleList[i];
                    var campaignId = vehicle.campaigns?.FirstOrDefault()?.campaign_id.ToStringNullSafe() ?? "";
                    var reservationToken = new ReservationToken
                    {
                        AgencyId = additionalInformation.Agency.AgencyId,
                        VendorId = vendor.VendorId,
                        APIVendorId = vendor.VendorId,
                        APIVendorName = vendor.VendorName,
                        APIVendorPhone = vendor.VendorPhone,
                        APIVendorEmail = vendor.VendorEmail,
                        APIVendorLogo = vendor.Logo,
                        VehicleId = mappedVehicle.VehicleId,
                        VehicleCode = vehicle.sub_group_short_name,
                        APIPickupLocationId = additionalInformation.APIPickupLocationId,
                        APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                        APIReturnLocationId = additionalInformation.APIReturnLocationId,
                        APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                        CurrencyType = requestCurrencyType,
                        RentalDuration = mappedVehicle.RentalDuration,
                        DailyPrice = mappedVehicle.DailyPrice,
                        OneWayFee = mappedVehicle.OneWayFee,
                        DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                        APIDailyPrice = vehicle.daily_price.ToFloatNullSafe(),
                        APIDailyPricePayNow = vehicle.daily_price.ToFloatNullSafe(),
                        APIOneWayFee = vehicle.drop_price.ToFloatNullSafe(),
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
                        APIReferenceCode = $"{vehicle.main_group_id}-{vehicle.sub_group_id}-{campaignId}-{vehicle.main_rule_id}",
                        APIReferenceCode2 = token.ToJson(),
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
            }
            return new(mappedVehicleList, mappedVehicleList.Count > 0);
        }
        

        private AvailabilityVehiclesRequest GetEntity(ResponseReservationStepsAdditionalInformation additionalInformation) => new() { startLocationCode = additionalInformation.APIPickupLocationCode, endLocationCode = additionalInformation.APIReturnLocationCode, startDateTime = additionalInformation.PickupDateTime.ToString("yyyy-MM-dd HH:mm"), endDateTime = additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd HH:mm") };
    }
}
