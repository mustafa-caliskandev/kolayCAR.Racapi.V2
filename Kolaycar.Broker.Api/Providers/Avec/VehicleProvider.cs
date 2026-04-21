using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Avec;
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
using AvecHelper = KolayCAR.Broker.API.Helpers.Avec;

namespace KolayCAR.Broker.API.Providers.Avec
{
    public class VehicleProvider : IVehicleProvider
    {
        HttpManager HttpManager { get; set; }

        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            HttpManager = new HttpManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            //var timeOut = vendor.APITimeout != 0  ? vendor.APITimeout * 1000 : 3000;
            int timeout = vendor.APITimeout.ToIntNullSafe() != 0 ? vendor.APITimeout * 1000 : -1;
            var result = await HttpManager.GetXmlAsync<AvecResponseBase>(
                requestPath: "xml_Rez.asp",
                parameters: GetVehiclesRequestParameters(additionalInformation, baseVendorRequestCurrencyType),
                culture: "tr-TR");

            if (result != null && result.Avekrent != null)
            {
                var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result.Avekrent.Cars, v => v.Cars_General_ID, v => v.Daily_Rental);
                //var mappedVehicleList = result.Avekrent.Cars.Map(additionalInformation);
                var mappedVehicleList = apiVehicleList.Map(additionalInformation, vendor);
                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                if (vendor.VehicleMappingActive)
                {
                    mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                    apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.Cars_General_ID.ToString()));
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
                    var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.Car_ID.ToString()).ToList();
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
                            VehicleId = vehicle.value.Car_ID,
                            VehicleCode = vehicle.value.Car_ID.ToString(),
                            APIPickupLocationId = additionalInformation.APIPickupLocationId,
                            APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                            APIReturnLocationId = additionalInformation.APIReturnLocationId,
                            APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                            CurrencyType = requestCurrencyType,
                            RentalDuration = mappedVehicle.RentalDuration,
                            DailyPrice = mappedVehicle.DailyPrice,
                            OneWayFee = mappedVehicle.OneWayFee,
                            DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                            APIDailyPrice = vehicle.value.Daily_Rental,
                            APIDailyPricePayNow = vehicle.value.Total_Rental,
                            APIOneWayFee = vehicle.value.Drop,
                            APIReferenceCode = vehicle.value.Rez_ID.ToString(),
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

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Avec servisinden araç bulunamadı!"
            };
        }

        private Dictionary<string, object> GetVehiclesRequestParameters(ResponseReservationStepsAdditionalInformation additionalInformation, CurrencyTypes baseVendorRequestCurrencyType)
        {
            return new Dictionary<string, object>()
            {
                { "Pickup_ID", additionalInformation.APIPickupLocationCode },
                { "Pickup_Day",  additionalInformation.PickupDateTime.Day },
                { "Pickup_Month",  additionalInformation.PickupDateTime.Month },
                { "Pickup_Year",  additionalInformation.PickupDateTime.Year },
                { "Pickup_Hour",  additionalInformation.PickupDateTime.Hour },
                { "Pickup_Min",  additionalInformation.PickupDateTime.Minute },
                { "Drop_Off_ID",  additionalInformation.APIReturnLocationCode },
                { "Drop_Off_Day",  additionalInformation.ReturnDateTime.Day },
                { "Drop_Off_Month",  additionalInformation.ReturnDateTime.Month },
                { "Drop_Off_Year",  additionalInformation.ReturnDateTime.Year },
                { "Drop_Off_Hour",  additionalInformation.ReturnDateTime.Hour },
                { "Drop_Off_Min",  additionalInformation.ReturnDateTime.Minute },
                { "Currency", AvecHelper.CurrencyHelper.GetLongCurrencyType(baseVendorRequestCurrencyType) },
                { "User_Name",  additionalInformation.Vendor.ApiKey },
                { "User_Pass",  additionalInformation.Vendor.ApiPassword }
            };
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var result = await HttpManager.GetXmlAsync<AvecResponseBase>(
            requestPath: "Xml_cars.asp",
            culture: "tr-TR");

            if (result != null && result.AvekRent != null)
            {
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = result.AvekRent.Cars.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Avec servisine ulaşılamadı!"
            };
        }
    }
}
