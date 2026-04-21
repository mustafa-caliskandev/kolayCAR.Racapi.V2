using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Sixt;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Sixt.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Sixt
{
    public class VehicleProvider : IVehicleProvider
    {
        public RestManager RestManager { get; set; }
        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            RestManager = new RestManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var result = await RestManager.GetXmlAsync<VehicleResponseBase>(
                requestPath: "",
                parameters: GetVehicleListRequestParameters(vendor));

            if (result is { SIXTTURKEYWEBSERVICES.VEHICLES: not null })
            {
                //result = result.SIXTTURKEYWEBSERVICES.VEHICLES.OrderBy(x => x.VEHICLE.Count).ToList()[0];
                var apiVehicles = result.SIXTTURKEYWEBSERVICES.VEHICLES?.Where(x => x.VEHICLE != null).OrderByDescending(x => x.VEHICLE.Count).FirstOrDefault();
                return new ServiceResponseBase
                {
                    Success = true,
                    //Data = result.SIXTTURKEYWEBSERVICES.VEHICLES[0].VEHICLE.Map()
                    Data = apiVehicles?.VEHICLE.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Data = null,
                Message = "Sixt servisine ulaşılamadı."
            };
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var result = await RestManager.GetXmlAsyncForSixt<VehicleQueryResponseBase>(
                requestPath: "",
                parameters: GetVehiclesRequestParameters(additionalInformation, vendor, getVehiclesRequest));

            if (result?.SIXTTURKEYWEBSERVICES?.VEHICLES?.VEHICLE?.Count > 0)
            {
                result.SIXTTURKEYWEBSERVICES.VEHICLES.VEHICLE.RemoveAll(x => x == null);
                var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result.SIXTTURKEYWEBSERVICES.VEHICLES.VEHICLE, v => v.GROUPNAME, v => v.DAILYPRICE.ToFloatNullSafe());
                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();
                var mappedVehicleList = apiVehicleList.Map(additionalInformation, result.SIXTTURKEYWEBSERVICES.ONEWAY, vendor);

                if (vendor.VehicleMappingActive)
                {
                    mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                    apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.GROUPNAME));
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
                    var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.GROUPNAME.ToString()).ToList();

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
                            VehicleCode = vehicle.value.GROUPNAME,
                            APIPickupLocationId = additionalInformation.APIPickupLocationId,
                            APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                            APIReturnLocationId = additionalInformation.APIReturnLocationId,
                            APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                            CurrencyType = requestCurrencyType,
                            RentalDuration = mappedVehicle.RentalDuration,
                            DailyPrice = mappedVehicle.DailyPrice,
                            OneWayFee = mappedVehicle.OneWayFee,
                            DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                            APIDailyPrice = vehicle.value.DAILYPRICE.ToFloatNullSafe(),
                            APIDailyPricePayNow = vehicle.value.DAILYPRICE.ToFloatNullSafe(),
                            APIOneWayFee = result.SIXTTURKEYWEBSERVICES.ONEWAY.ToFloatNullSafe(),
                            APIReferenceCode = result.SIXTTURKEYWEBSERVICES.VEHICLES.Token.ToStringNullSafe(),
                            APIReferenceCode2 = vehicle.value.INCLUDED != null ? GetIncluded(vehicle.value.INCLUDED.INCLUDE) : null, // sixt servisi dahili hizmetlerin de gönderilmesini istiyor.
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
                return new ServiceResponseBase(mappedVehicleList, mappedVehicleList.Count > 0);
            }
            return new ServiceResponseBase(null, false, "Sixt servisine ulaşılamadı.");
        }

        private Dictionary<string, object> GetVehicleListRequestParameters(Vendor vendor) =>
            new Dictionary<string, object>
            {
                { "aid", vendor.ApiKey },
                { "p", vendor.ApiPassword },
                { "m", "getList" },
                { "o", "vehicle" }
            };

        private Dictionary<string, object> GetVehiclesRequestParameters(ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, GetVehiclesRequest getVehiclesRequest)
        {
            var apiPickupLocationCodes = additionalInformation.APIPickupLocationCode.Split('-');
            var apiReturnLocationCodes = additionalInformation.APIReturnLocationCode.Split('-');

            var xmlString = @$"
                 <VEHICLEQUERY>
                     <PICKUPSTATIONCODE>{apiPickupLocationCodes[0]}</PICKUPSTATIONCODE>
                     <PICKUPSTATIONID>{apiPickupLocationCodes[1]}</PICKUPSTATIONID>
                     <RETURNSTATIONCODE>{apiReturnLocationCodes[0]}</RETURNSTATIONCODE>
                     <RETURNSTATIONID>{apiReturnLocationCodes[1]}</RETURNSTATIONID>
                     <PICKUP>
                         <DATE>{additionalInformation.PickupDateTime.ToString("dd.MM.yyyy")}</DATE>
                         <HOUR>{additionalInformation.PickupDateTime.ToString("HH")}</HOUR>
                         <MINUTE>{additionalInformation.PickupDateTime.ToString("mm")}</MINUTE>
                     </PICKUP>
                     <RETURN>
                         <DATE>{additionalInformation.ReturnDateTime.ToString("dd.MM.yyyy")}</DATE>
                         <HOUR>{additionalInformation.ReturnDateTime.ToString("HH")}</HOUR>
                         <MINUTE>{additionalInformation.ReturnDateTime.ToString("mm")}</MINUTE>
                     </RETURN>
                 </VEHICLEQUERY>";

            return new Dictionary<string, object>
            {
                { "aid", vendor.ApiKey },
                { "p", vendor.ApiPassword },
                { "m", "getList" },
                { "o", "vehiclequery" },
                { "xml", xmlString }
            };
        }

        private static string GetIncluded(List<INCLUDE> includes)
        {
            var xmlString = "";
            if (includes?.Count > 0)
                foreach (var item in includes)
                    xmlString += $@"<INCLUDE><NAME>{item.NAME}</NAME><CODE>{item.CODE}</CODE></INCLUDE>";

            return xmlString;
        }
    }
}
