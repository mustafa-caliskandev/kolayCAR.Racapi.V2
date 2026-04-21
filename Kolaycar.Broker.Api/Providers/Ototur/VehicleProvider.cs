using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Ototur;
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
using static KolayCAR.Broker.Domain.Models.Response.OtoturResponseBase;

namespace KolayCAR.Broker.API.Providers.Ototur
{
    public class VehicleProvider : IVehicleProvider
    {
        HttpManager _httpManager { get; set; }
        AuthProvider _authProvider { get; set; }

        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            _httpManager = new HttpManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
            _authProvider = new AuthProvider(vendor.APIBaseUrl);
        }
        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var token = await _authProvider.GetToken();

            if (token != null)
            {
                var result = await _httpManager.GetAsync2<OtoturVehicleResponseBase>(
                requestPath: "vehicleGroups",
                headers: GetHeaders(token));

                return new ServiceResponseBase
                {
                    Success = result != null,
                    Data = result.Data.result.content.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Ototur kullanıcı girişi yapılamadı!"
            };
        }

        private IDictionary<string, object> GetHeaders(OtoturAuthResponse ototurAuthResponse)
        {
            return new Dictionary<string, object>() { { "Authorization", $"Bearer {ototurAuthResponse.token}" } };
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var token = await _authProvider.GetToken();

            var diffDay = (getVehiclesRequest.PickupDate.ToDateTimeNullSafe().Day - DateTime.Now.Day);
            var diffHour = (getVehiclesRequest.PickupTime.ToDateTimeNullSafe().Hour - DateTime.Now.Hour);
            if (token != null)
            {
                var result = await _httpManager.GetAsync2<OtoturAvailableVehicleResponseBase>(
                    requestPath: "vehicle/avaliable",
                    parameters: GetVehiclesParams(getVehiclesRequest, additionalInformation, baseVendorRequestCurrencyType),
                    headers: GetHeaders(token),
                    encode: false);

                if (result?.Data?.result?.totalElements > 0)
                {
                    var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result.Data.result.content, v => v.aracid, v => v.dailyRentalPrice.ToFloatNullSafe());
                    var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();
                    //var mappedVehicleList = result.Data.result.content.Map(additionalInformation, vendor);
                    var mappedVehicleList = apiVehicleList.Map(additionalInformation, vendor);

                    if (vendor.VehicleMappingActive)
                    {
                        mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                        apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.aracid.ToStringNullSafe()));
                    }

                    CalculationHelper.SetVehiclesPrices(mappedVehicleList, vendor, exchangeRates, requestCurrencyType, baseVendorRequestCurrencyType);
                    VehicleHelper.SetVehiclesProperties(mappedVehicleList, vendor, additionalInformation.Agency, exchangeRates, baseVendorRequestCurrencyType, requestCurrencyType, profitMarkups);
                    if (mappedVehicleList.Any(vehicle => Math.Abs(vehicle.RentalDuration - additionalInformation.RentalDuration) > 1))
                        return new ServiceResponseBase(null, false, "Yanlış gün sayısı");

                    var pickupDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime);
                    var returnDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime);
                    var languageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>();

                    foreach (var vehicle in apiVehicleList.Select((value, index) => new { value, index }))
                    {
                        var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.aracid).ToList();
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
                                VehicleCode = vehicle.value.aracid.ToStringNullSafe(),
                                APIPickupLocationId = additionalInformation.APIPickupLocationId,
                                APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                                APIReturnLocationId = additionalInformation.APIReturnLocationId,
                                APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                                CurrencyType = requestCurrencyType,
                                RentalDuration = mappedVehicle.RentalDuration,
                                DailyPrice = mappedVehicle.DailyPrice,
                                OneWayFee = mappedVehicle.OneWayFee,
                                DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                                APIDailyPrice = vehicle.value.dailyRentalPrice.Replace(",", "").ToFloatNullSafe(),
                                APIDailyPricePayNow = vehicle.value.dailyRentalPrice.Replace(",", "").ToFloatNullSafe(),
                                APIOneWayFee = vehicle.value.dropPrice.Replace(",", "").ToFloatNullSafe(),
                                //APIReferenceCode = vehicle.value.offerId.ToStringNullSafe(),
                                APIReferenceCode2 = mappedVehicle.ApiVendorName,
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
                Message = "Renticar kullanıcı girişi yapılamadı!"
            };
        }
        private IDictionary<string, object> GetVehiclesParams(GetVehiclesRequest getVehiclesRequest, ResponseReservationStepsAdditionalInformation additionalInformation, CurrencyTypes baseVendorRequestCurrencyType)
        {
            return new Dictionary<string, object>()
            {
                { "language", $"{getVehiclesRequest.LanguageCode}" },
                { "currency", $"{baseVendorRequestCurrencyType.ToString()}" } ,
                { "pickupDate", $"{getVehiclesRequest.PickupDate}" },
                { "pickupLocationId", $"{additionalInformation.APIPickupLocationCode}" } ,
                { "pickupTime", $"{getVehiclesRequest.PickupTime}" } ,
                { "returnDate", $"{getVehiclesRequest.ReturnDate}" } ,
                { "returnLocationId", $"{additionalInformation.APIReturnLocationCode}" } ,
                { "returnTime", $"{getVehiclesRequest.ReturnTime}" }
            };

        }
    }
}
