using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Avec3;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Avec3
{
    public class VehicleProvider : IVehicleProvider
    {
        public HttpManager _httpManager;
        public AuthProvider _authProvider;
        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            _httpManager = new HttpManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
            _authProvider = new AuthProvider(vendor.APIBaseUrl);
        }
        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var user = await _authProvider.GetTokenAsync(vendor.ApiKey, vendor.ApiPassword);
            var result = await _httpManager.GetAsync2<Avec3ResponseBase.VehicleResponseBase>
                (
                   requestPath: @"branch_base/vehicle/models",
                   headers: _authProvider.CreateAuthHeader(user.Data.access_token)
                );
            if (result.Success && result.Data.data.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = result.Data.data.Map()
                };
            }
            return new ServiceResponseBase
            {
                Success = false,
                Message = "Serviste bir hata oluştu"
            };
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var diffDay = (getVehiclesRequest.PickupDate.ToDateTimeNullSafe().Day - DateTime.Now.Day);
            var diffHour = (getVehiclesRequest.PickupTime.ToDateTimeNullSafe().Hour - DateTime.Now.Hour);

            var timeDifference = ($"{getVehiclesRequest.PickupDate} {getVehiclesRequest.PickupTime}".ToDateTimeNullSafe() - DateTime.Now).TotalHours;

            if (timeDifference >= 3.0)
            {
                var user = await _authProvider.GetTokenAsync(vendor.ApiKey, vendor.ApiPassword);
                var result = await _httpManager.GetAsync2<Avec3ResponseBase.AvailableVehicleResponse>
                    (
                        requestPath: @"/branch_base/vehicle/search",
                        headers: _authProvider.CreateAuthHeader(user.Data.access_token),
                        parameters: GetVehiclesRequestParameters(additionalInformation),
                        encode: false
                    );
                if (result?.Data?.total_count > 0)
                {
                    var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result.Data.data, v => v.vehicle.id, v => v.price_details.total_price);
                    //var mappedVehicleList = result.Data.data.Map(additionalInformation, vendor);
                    var mappedVehicleList = apiVehicleList.Map(additionalInformation, vendor);
                    var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                    if (vendor.VehicleMappingActive)
                    {
                        mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                        apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.vehicle.id.ToString()));
                    }

                    if (mappedVehicleList.Any(vehicle => Math.Abs(vehicle.RentalDuration - additionalInformation.RentalDuration) > 1))
                        return new ServiceResponseBase(null, false, "Yanlış gün sayısı");

                    CalculationHelper.SetVehiclesPrices(mappedVehicleList, vendor, exchangeRates, requestCurrencyType, baseVendorRequestCurrencyType);
                    VehicleHelper.SetVehiclesProperties(mappedVehicleList, vendor, additionalInformation.Agency, exchangeRates, baseVendorRequestCurrencyType, requestCurrencyType, profitMarkups);

                    var pickupDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime);
                    var returnDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime);
                    var languageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>();

                    foreach (var vehicle in apiVehicleList.Select((value, index) => new { value, index }))
                    {
                        var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.vehicle.id.ToString()).ToList();

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
                                VehicleCode = vehicle.value.vehicle.id.ToStringNullSafe(),
                                APIPickupLocationId = additionalInformation.APIPickupLocationId,
                                APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                                APIReturnLocationId = additionalInformation.APIReturnLocationId,
                                APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                                CurrencyType = requestCurrencyType,
                                RentalDuration = mappedVehicle.RentalDuration,
                                DailyPrice = mappedVehicle.DailyPrice,
                                OneWayFee = mappedVehicle.OneWayFee,
                                DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                                APIDailyPrice = vehicle.value.price_details.total_price / vehicle.value.days,
                                APIDailyPricePayNow = vehicle.value.price_details.total_price / vehicle.value.days,
                                APIOneWayFee = vehicle.value.price_details.dropoff_price,
                                APIReferenceCode = vehicle.value.booking_id.ToStringNullSafe(),
                                APIReferenceCode2 = vehicle.value.vehicle.id.ToStringNullSafe(),
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
                Message = "Avec servisinden araç bulunamadı!"
            };
        }

        private Dictionary<string, object> GetVehiclesRequestParameters(ResponseReservationStepsAdditionalInformation additionalInformation)
        {

            CultureInfo culture = CultureInfo.InvariantCulture;
            return new Dictionary<string, object>()
            {
                //{ "dropoff_date", Uri.EscapeDataString(additionalInformation.ReturnDateTime.ToString("yyyy-MM-ddTHH:mm:sszzz"))},
                { "dropoff_date", Uri.EscapeDataString(additionalInformation.ReturnDateTime.ToString("s",culture))},
                { "pickup_branch_id", additionalInformation.APIPickupLocationCode },
                //{ "pickup_date", Uri.EscapeDataString(additionalInformation.PickupDateTime.ToString("yyyy-MM-ddTHH:mm:sszzz"))},
                 { "pickup_date", Uri.EscapeDataString(additionalInformation.PickupDateTime.ToString("s",culture))},
                { "dropoff_branch_id", additionalInformation.APIReturnLocationCode },

                    { "show_addons_in_response", true },
            };
        }

    }
}
