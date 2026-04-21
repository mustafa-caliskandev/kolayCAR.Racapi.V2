using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Circular;
using KolayCAR.Broker.API.Mappers.Cizgi;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Circular
{
    public class VehicleProvider : IVehicleProvider
    {
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            RestManager = new RestManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
            AuthProvider = new AuthProvider(vendor.APIBaseUrl);
        }
        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var auth = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);

            if (auth != null && !string.IsNullOrEmpty(auth.token))
            {
                var result = await RestManager.GetAsync<CircularResponseBase.VehicleList>(
                       requestPath: $"car/cargroup/list?token={auth.token}&listsize=100",
                       headers: AuthProvider.CreateAuthHeaderWithContentType(auth.token));

                if (result != null && result.list != null && result.list.Count > 0)
                {
                    return new ServiceResponseBase
                    {
                        Success = result != null,
                        Data = result.Map()
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
            var auth = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);
            if (auth != null && auth.success && !string.IsNullOrEmpty(auth.token))
            {
                //https://crmtest.circularcarhire.com.tr/contract/cargroup/location/
                //16-16
                //?token=78jrrk60jf8cgwsri03k3gb04aoe21ub
                //&listsize=50
                //&referralagent=123
                //&start=2021-10-16 10:00
                //&end=2021-10-30 10:00
                var url = $"contract/cargroup/location/{additionalInformation.APIPickupLocationCode}-{additionalInformation.APIReturnLocationCode}?token={auth.token}&referralagent={vendor.ApiPassword}&start={additionalInformation.PickupDateTime.ToString("yyyy-MM-dd HH:mm")}&end={additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd HH:mm")}";
                var result = await RestManager.PostAsync<CircularRequestBase.AvailabilityRequest, CircularResponseBase.AvaibilityVehicleResponse>(
                 requestPath: url,
                 entity: GetAvailabilityRequestBodyEntity(additionalInformation, vendor),
                 headers: AuthProvider.CreateAuthHeaderWithContentType(auth.token));

                if (result?.location?.groups?.Count > 0)
                {
                    var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result.location.groups, v => v.id, v => v.dateranges.First().periods.First().dailyprice.ToFloatNullSafe());
                    result.location.groups = apiVehicleList;
                    var mappedVehicleList = result.Map(additionalInformation,vendor, baseUrl: vendor.APIBaseUrl);
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
                                APIOneWayFee = 0,
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
                Success = false,
                Data = null
            };
        }

        private CircularRequestBase.AvailabilityRequest GetAvailabilityRequestBodyEntity(ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
       new CircularRequestBase.AvailabilityRequest
       {

           pickup_location = additionalInformation.APIPickupLocationCode.ToIntNullSafe(),
           pickup_date = additionalInformation.PickupDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
           dropoff_location = additionalInformation.APIReturnLocationCode.ToIntNullSafe(),
           dropoff_date = additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
           rate_codes = null
       };
    }
}
