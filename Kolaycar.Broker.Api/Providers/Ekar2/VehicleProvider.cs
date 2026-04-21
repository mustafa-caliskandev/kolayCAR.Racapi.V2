using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Ekar2;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.Ekar2ResponseBase;
namespace KolayCAR.Broker.API.Providers.Ekar2
{
    public class VehicleProvider : IVehicleProvider
    {
        private HttpManager _httpManager { get; set; }
        private AuthProvider _authProvider { get; set; }
        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            _httpManager = new HttpManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
            _authProvider = new AuthProvider(vendor.APIBaseUrl);
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var result = await _httpManager.GetAsync2<Ekar2ResponseBase.Ekar2VehicleListResponse>(
                    requestPath: "/api/v1/vehicleGroups",
                    headers: _authProvider.GetHeaders(vendor)
                );
            if (result.Success && result.Data.result.content.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = result.Data.result.content.Map()
                };
            }
            return new ServiceResponseBase
            {
                Success = false,
                Message = "Serviste bir hata oluştu"
            };
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Domain.Models.Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var result = await _httpManager.GetAsync2<Ekar2ResponseBase.Ekar2AvailableVehicleResponse>
                (
                    requestPath: @"/api/v1/vehicles/available",
                    headers: _authProvider.GetHeaders(vendor),
                    parameters: GetParamaters(getVehiclesRequest, additionalInformation, baseVendorRequestCurrencyType)
               );
            if (result?.Data?.result != null)
            {
                //var apiVehicles = JsonConvert.DeserializeObject<List<AvailableVehicle>>(JsonConvert.SerializeObject(result.Data.result));
                var apiVehicles = VehicleHelper.SelectCheapestByGroup(result.Data.result, v => v.vehicleGroupId, v => v.dailyRentalPrice.ToFloatNullSafe());
                //var mappedVehicles = result.Data.result.Map(additionalInformation, vendor);
                var mappedVehicles = apiVehicles.Map(additionalInformation, vendor);
                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                if (vendor.VehicleMappingActive)
                {
                    mappedVehicles = VehicleHelper.MapLocalVehicleList(mappedVehicles, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, vendor: vendor);
                    apiVehicles.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.vehicleGroupId.ToString()));
                }
                CalculationHelper.SetVehiclesPrices(mappedVehicles, vendor, exchangeRates, requestCurrencyType, baseVendorRequestCurrencyType);
                VehicleHelper.SetVehiclesProperties(mappedVehicles, vendor, additionalInformation.Agency, exchangeRates, baseVendorRequestCurrencyType, requestCurrencyType, profitMarkups);
                if (mappedVehicles.Any(vehicle => Math.Abs(vehicle.RentalDuration - additionalInformation.RentalDuration) > 1))
                    return new ServiceResponseBase(null, false, "Yanlış gün sayısı");
                foreach (var vehicle in apiVehicles)
                {
                    var tempMappedVehicleList = mappedVehicles.Where(x => x.VehicleCode == vehicle.vehicleGroupId.ToString()).ToList();

                    for (int i = 0; i < tempMappedVehicleList.Count; i++)
                    {
                        var mappedVehicle = tempMappedVehicleList[i];

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
                            VehicleCode = vehicle.vehicleGroupId.ToStringNullSafe(),
                            APIPickupLocationId = additionalInformation.APIPickupLocationId,
                            APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                            APIReturnLocationId = additionalInformation.APIReturnLocationId,
                            APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                            CurrencyType = requestCurrencyType,
                            RentalDuration = mappedVehicle.RentalDuration,
                            DailyPrice = mappedVehicle.DailyPrice,
                            OneWayFee = mappedVehicle.OneWayFee,
                            DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                            APIDailyPrice = (float)vehicle.dailyRentalPrice,
                            APIDailyPricePayNow = (float)vehicle.dailyRentalPrice,
                            APIOneWayFee = (float)vehicle.oneWayPrice,
                            APIReferenceCode = vehicle.vehicleGroupId.ToStringNullSafe(),
                            APIReferenceCode2 = vehicle.vehicleGroupId.ToStringNullSafe(),
                            DepositPrice = mappedVehicle.DepositPrice,
                            VendorMinimumDriverAge = mappedVehicle.VendorMinimumDriverAge ?? 0,
                            VendorMinimumDrivingLicenseAge = mappedVehicle.VendorMinimumDrivingLicenseAge ?? 0,
                            ServiceCharge = mappedVehicle.ServiceCharge,
                            FuelType = mappedVehicle.FuelType,
                            TransmissionType = mappedVehicle.TransmissionType,
                            DepositCreditCardRequired = mappedVehicle.DepositCreditCardRequired,
                            PickupLocationId = getVehiclesRequest.PickupLocationId,
                            ReturnLocationId = getVehiclesRequest.ReturnLocationId,
                            LanguageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>(),
                            PickupDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime),
                            ReturnDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime),
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
                }
                return new ServiceResponseBase
                {
                    Data = mappedVehicles,
                    Success = true
                };

            }
            return null;
        }

        private IDictionary<string, object> GetParamaters(GetVehiclesRequest getVehiclesRequest, ResponseReservationStepsAdditionalInformation additionalInformation, CurrencyTypes baseVendorRequestCurrenyType)
        {
            return new Dictionary<string, object>()
            {
                { "currency", baseVendorRequestCurrenyType.ToString()},
                { "pickupDate", getVehiclesRequest.PickupDate },
                { "pickupLocationId", additionalInformation.APIPickupLocationCode},
                { "pickupTime", getVehiclesRequest.PickupTime },
                { "returnDate", getVehiclesRequest.ReturnDate },
                { "returnLocationId", additionalInformation.APIReturnLocationCode },
                { "returnTime", getVehiclesRequest.ReturnTime }
            };
        }

    }
}
