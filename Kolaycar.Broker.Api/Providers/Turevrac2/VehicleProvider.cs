using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Turevrac2;
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
using TurevracHelper = KolayCAR.Broker.API.Helpers.Turevrac;

namespace KolayCAR.Broker.API.Providers.Turevrac2
{
    public class VehicleProvider : IVehicleProvider
    {
        HttpManager _httpManager;
        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            _httpManager = new HttpManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
        }
        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var vehicleResult = await _httpManager.GetAsync2<List<Turevrac2ResponseBase.Vehicle>>
                (
                    requestPath: "/JsonGroup.aspx",
                    parameters: GetVehicleParameters(vendor),
                    isReservationRequest: true
                );
            if (vehicleResult != null)
                if (vehicleResult.Data != null)
                {
                    return new ServiceResponseBase
                    {
                        Data = vehicleResult.Data.Map(vendor),
                        Success = vehicleResult.Success
                    };
                }
            return new ServiceResponseBase
            {
                Data = null,
                Success = (bool)vehicleResult?.Success,
                Message = $"{vendor.VendorName} - Turevrac2 araç servisinden yanıt alınamadı!"
            };

        }

        private IDictionary<string, object> GetVehicleParameters(Vendor vendor)
        {
            return new Dictionary<string, object>
            {
                { "Key_Hack", vendor.ApiClientId }

            };
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            try
            {
                var availableVehicleList = await _httpManager.GetAsync2<List<Turevrac2ResponseBase.AvailableVehicle>>
                                          (
                                              requestPath: "/JsonRez.aspx",
                                              parameters: GetAvailableVehiclesParameters(getVehiclesRequest, additionalInformation, vendor, baseVendorRequestCurrencyType)
                                          );

                if (availableVehicleList?.Data?.Count > 0)
                {
                    var apiVehicleList = VehicleHelper.SelectCheapestByGroup(availableVehicleList.Data, v => v.group_id,
                                v => v.daily_rental.ToFloatNullSafe());

                    var mappedVehicleList = apiVehicleList.Map(additionalInformation, vendor);
                    var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                    if (vendor.VehicleMappingActive)
                    {
                        mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                        apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.group_id.ToStringNullSafe()));
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
                        var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.group_id.ToStringNullSafe()).ToList();
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
                                VehicleCode = vehicle.value.group_id.ToString(),
                                APIPickupLocationId = additionalInformation.APIPickupLocationId,
                                APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                                APIReturnLocationId = additionalInformation.APIReturnLocationId,
                                APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                                CurrencyType = requestCurrencyType,
                                RentalDuration = mappedVehicle.RentalDuration,
                                DailyPrice = mappedVehicle.DailyPrice,
                                OneWayFee = mappedVehicle.OneWayFee,
                                DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                                APIDailyPrice = vehicle.value.daily_rental.ToFloatNullSafe(),
                                APIDailyPricePayNow = vehicle.value.daily_rental.ToFloatNullSafe(),
                                APIOneWayFee = vehicle.value.drop.ToFloatNullSafe(),
                                APIReferenceCode = vehicle.value.rez_id.ToString(),
                                APIReferenceCode2 = vehicle.value.cars_park_id.ToString(),
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
                    return new(mappedVehicleList, mappedVehicleList.Count > 0);
                }
                return new(null, false, $"{vendor.VendorName} servisinden araç bulunamadı!");
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        private IDictionary<string, object> GetAvailableVehiclesParameters(GetVehiclesRequest getVehiclesRequest, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, CurrencyTypes baseVendorRequestCurrencyType)
        {
            var parameters = new Dictionary<string, object>()
                        {
                            {"Key_Hack", vendor.ApiClientId},
                            {"User_Name", vendor.ApiKey},
                            {"User_Pass", vendor.ApiPassword},
                            {"Pickup_ID", additionalInformation.APIPickupLocationCode},
                            {"Drop_Off_ID", additionalInformation.APIReturnLocationCode},
                            {"Pickup_Day", additionalInformation.PickupDateTime.ToString("dd") },
                            {"Pickup_Month", additionalInformation.PickupDateTime.ToString("MM") },
                            {"Pickup_Year", additionalInformation.PickupDateTime.ToString("yyyy") },
                            {"Drop_Off_Day", additionalInformation.ReturnDateTime.ToString("dd") },
                            {"Drop_Off_Month", additionalInformation.ReturnDateTime.ToString("MM") },
                            {"Drop_Off_Year", additionalInformation.ReturnDateTime.ToString("yyyy") },
                            {"Pickup_Hour", additionalInformation.PickupDateTime.ToString("HH") },
                            {"Pickup_Min", additionalInformation.PickupDateTime.ToString("mm") },
                            {"Drop_Off_Hour", additionalInformation.ReturnDateTime.ToString("HH") },
                            {"Drop_Off_Min", additionalInformation.ReturnDateTime.ToString("mm") },
                            {"Currency", TurevracHelper.CurrencyHelper.GetLongCurrencyType(baseVendorRequestCurrencyType)}
                        };
            //if (!string.IsNullOrEmpty(getVehiclesRequest.ReservationToken))
            //{
            //    var reservationToken = JsonConvert.DeserializeObject<ReservationToken>(getVehiclesRequest.ReservationToken);
            //    parameters.Add("Group_ID", reservationToken.VehicleCode);
            //    parameters.Add("Cars_Park_ID", reservationToken.APIReferenceCode2);
            //}
            return parameters;
        }
    }
}
