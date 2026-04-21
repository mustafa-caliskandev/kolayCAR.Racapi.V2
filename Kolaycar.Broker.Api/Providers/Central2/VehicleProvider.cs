using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Central2;
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
using static KolayCAR.Broker.Domain.Models.Response.Central2ResponseBase;
//using CentralHelper = KolayCAR.Broker.API.Helpers.Central2;//Düzenleme yapılacak

namespace KolayCAR.Broker.API.Providers.Central2
{
    public class VehicleProvider : IVehicleProvider
    {
        RestManager RestManager { get; set; }

        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            RestManager = new RestManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var result = await RestManager.GetXmlAsync<Domain.Models.Response.Central2ResponseBase.CentralResponse>(//Düzenleme yapıalcak !!!!!!!!!!!
                requestPath: "xml_Group.Aspx",
                parameters: GetVehicleListRequestParameters(vendor),
                culture: "tr-TR");

            if (result != null && result.Sistemrent.Group != null && result.Sistemrent.Group != null && result.Sistemrent.Group.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = result.Sistemrent.Group.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Central servisine ulaşılamadı!"
            };
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var requestParameters = GetVehiclesRequestParameters(additionalInformation, baseVendorRequestCurrencyType, getVehiclesRequest, vendor);

            var result = await RestManager.GetXmlAsync<CentralResponse>(
                requestPath: "xml_Rez.Aspx",
                parameters: requestParameters
                );

            if (result?.Turevsistem?.Car?.Count > 0)
            {
                result.Turevsistem.Car.RemoveAll(x => x == null);
                var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result.Turevsistem.Car, v => v.Group_ID, v => v.Daily_Rental.ToFloatNullSafe());
                //var mappedVehicleList = result.Turevsistem.Car.Map(additionalInformation, vendor);
                var mappedVehicleList = apiVehicleList.Map(additionalInformation, vendor);
                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                if (vendor.VehicleMappingActive)
                {
                    mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                    apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.Group_ID));
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
                    var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.Group_ID.ToString()).ToList();

                    if (tempMappedVehicleList.First().DailyPrice < 1)
                        mappedVehicleList.Remove(tempMappedVehicleList.First());

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
                            VehicleCode = vehicle.value.Group_ID.ToStringNullSafe(),
                            APIPickupLocationId = additionalInformation.APIPickupLocationId,
                            APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                            APIReturnLocationId = additionalInformation.APIReturnLocationId,
                            APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                            CurrencyType = requestCurrencyType,
                            RentalDuration = mappedVehicle.RentalDuration,
                            DailyPrice = mappedVehicle.DailyPrice,
                            OneWayFee = mappedVehicle.OneWayFee,
                            DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                            APIDailyPrice = vehicle.value.Daily_Rental.ToFloatNullSafe(),
                            APIDailyPricePayNow = vehicle.value.Daily_Rental.ToFloatNullSafe(),
                            APIOneWayFee = vehicle.value.Drop.ToFloatNullSafe(),
                            APIReferenceCode = vehicle.value.Rez_ID.ToStringNullSafe(),
                            APIReferenceCode2 = vehicle.value.Cars_Park_ID.ToStringNullSafe(),
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
                Message = "Central servisinden araç bulunamadı!"
            };
        }

        private Dictionary<string, object> GetVehiclesRequestParameters(ResponseReservationStepsAdditionalInformation additionalInformation, CurrencyTypes baseVendorRequestCurrencyType, GetVehiclesRequest getVehiclesRequest, Vendor vendor)
        {
            return new Dictionary<string, object>()
            {
                { "Pickup_ID", additionalInformation.APIPickupLocationCode },
                { "Pickup_Day",  additionalInformation.PickupDateTime.ToString("dd") },
                { "Pickup_Month",  additionalInformation.PickupDateTime.ToString("MM") },
                { "Pickup_Year",  additionalInformation.PickupDateTime.ToString("yyyy") },
                { "Pickup_Hour",  additionalInformation.PickupDateTime.ToString("HH") },
                { "Pickup_Min",  additionalInformation.PickupDateTime.ToString("mm") },
                { "Drop_Off_ID",  additionalInformation.APIReturnLocationCode },
                { "Drop_Off_Day",  additionalInformation.ReturnDateTime.ToString("dd") },
                { "Drop_Off_Month",  additionalInformation.ReturnDateTime.ToString("MM") },
                { "Drop_Off_Year",  additionalInformation.ReturnDateTime.ToString("yyyy") },
                { "Drop_Off_Hour",  additionalInformation.ReturnDateTime.ToString("HH") },
                { "Drop_Off_Min",  additionalInformation.ReturnDateTime.ToString("mm") },
                { "Currency", GetCentralResCurrencyType(baseVendorRequestCurrencyType.ToString()) }, // Düzenleme yapılacak
                { "Key_Hack",  additionalInformation.Vendor.SecretKey },
                { "User_Name",  additionalInformation.Vendor.ApiKey },
                { "User_Pass",  additionalInformation.Vendor.ApiPassword }
            };
        }

        private Dictionary<string, object> GetVehicleListRequestParameters(Vendor vendor) =>
            new Dictionary<string, object>()
            {
                { "Key_Hack",  vendor.SecretKey },
            };
        private string GetCentralResCurrencyType(string currency)
        {
            if (currency == CurrencyTypes.TRY.ToString())
                return "TL";
            if (currency == CurrencyTypes.EUR.ToString())
                return "EURO";
            if (currency == CurrencyTypes.USD.ToString())
                return "USD";
            if (currency == CurrencyTypes.GBP.ToString())
                return "GBP";
            return "TL";
        }
    }
}