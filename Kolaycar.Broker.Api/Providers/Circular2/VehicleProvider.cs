using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Circular2;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Circular2
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
            var auth = await _authProvider.GetTokenAsync(vendor.ApiKey);

            if (auth != null && !string.IsNullOrEmpty(auth.token))
            {
                var result = await _httpManager.GetAsync2<Circular2ResponseBase.ApiResponse>(
                       requestPath: $"car/cargroup/list?token={auth.token}&listsize=100",
                       headers: _authProvider.CreateAuthHeaderWithContentType(auth.token));

                if (result != null && result.Data.list != null && result.Data.list.Count > 0)
                {
                    return new ServiceResponseBase
                    {
                        Success = result != null,
                        Data = result.Data.list.Map()
                    };
                }
            }

            return new ServiceResponseBase
            {
                Success = false,
                Data = null
            };

        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var auth = await _authProvider.GetTokenAsync(vendor.ApiKey);
            if (auth != null && auth.success && !string.IsNullOrEmpty(auth.token))
            {
                var result = await _httpManager.GetAsync2<Circular2ResponseBase.AvailableVehicleResponse>(
                 requestPath: $"contract/cargroup/location/{additionalInformation.APIPickupLocationCode}-{additionalInformation.APIReturnLocationCode}?token={auth.token}&referralagent={vendor.ApiPassword}&start={additionalInformation.PickupDateTime.ToString("yyyy-MM-dd HH:mm")}&end={additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd HH:mm")}"
                 );
                if (result?.Data?.location?.groups?.Count > 0)
                {
                    var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result.Data.location.groups, v => v.id, v => v.dateranges.First().periods.FirstOrDefault().dailyprice.ToFloatNullSafe());
                    result.Data.location.groups = apiVehicleList;
                    var mappedVehicleList = result.Data.location.Map(additionalInformation, vendor, baseUrl: vendor.APIBaseUrl);
                    var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                    if (vendor.VehicleMappingActive)
                    {
                        mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                        apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.id.ToString()));
                    }

                    CalculationHelper.SetVehiclesPrices(mappedVehicleList, vendor, exchangeRates, requestCurrencyType, baseVendorRequestCurrencyType);
                    VehicleHelper.SetVehiclesProperties(mappedVehicleList, vendor, additionalInformation.Agency, exchangeRates, baseVendorRequestCurrencyType, requestCurrencyType, profitMarkups);

                    var pickupDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime);
                    var returnDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime);
                    var languageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>();

                    foreach (var vehicle in apiVehicleList.Select((value, index) => new { value, index }))
                    {
                        var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.id.ToString()).ToList();
                        foreach (var mappedVehicle in tempMappedVehicleList)
                        {
                            var dailyprice = vehicle.value.dateranges.FirstOrDefault().periods.FirstOrDefault().dailyprice.ToFloatNullSafe();
                            var reservationToken = new ReservationToken
                            {
                                AgencyId = additionalInformation.Agency.AgencyId,
                                VendorId = vendor.VendorId,
                                APIVendorId = vendor.VendorId,
                                APIVendorName = vendor.VendorName,
                                APIVendorPhone = vendor.VendorPhone,
                                APIVendorEmail = vendor.VendorEmail,
                                APIVendorLogo = vendor.Logo,
                                VehicleId = mappedVehicle.VehicleId.ToIntNullSafe(),
                                VehicleCode = vehicle.value.id.ToString(),
                                APIPickupLocationId = additionalInformation.APIPickupLocationId,
                                APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                                APIReturnLocationId = additionalInformation.APIReturnLocationId,
                                APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                                CurrencyType = requestCurrencyType,
                                RentalDuration = mappedVehicle.RentalDuration,
                                DailyPrice = mappedVehicle.DailyPrice,
                                OneWayFee = mappedVehicle.OneWayFee,
                                DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                                APIDailyPrice = dailyprice,
                                APIDailyPricePayNow = dailyprice,
                                //TODO:ismet drop ücreti
                                APIOneWayFee = mappedVehicle.OneWayFee,
                                APIReferenceCode = vehicle.value.id.ToString(),
                                APIReferenceCode2 = vehicle.value.title.ToString(),
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
                                VehicleImageUrl = mappedVehicle.VehicleImages.Count > 0 ? mappedVehicle.VehicleImages[0].Url : vendor.APIBaseUrl + "/" + vehicle.value.photo.url,
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
                        }
                    }
                    return new ServiceResponseBase
                    {
                        Success = mappedVehicleList.Count > 0,
                        Data = mappedVehicleList
                    };
                }
            }

            return new ServiceResponseBase
            {
                Data = null,
                Success = false
            };
        }
    }
}
