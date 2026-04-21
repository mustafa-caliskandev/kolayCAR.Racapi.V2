using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Dailydrive;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Response.Dailydrive;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Dailydrive
{
    public class VehicleProvider : IVehicleProvider
    {
        RestManager HttpManager { get; set; }

        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            HttpManager = new RestManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var contentClass = new StringContent(@"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'
             xmlns:dto='http://ws.naryaz.com/model/dto'>
             <soapenv:Header/>
             <soapenv:Body>
             <dto:VehicleClassesRequest>
             </dto:VehicleClassesRequest>
             </soapenv:Body>
            </soapenv:Envelope>", Encoding.UTF8, "text/xml");
            var resultClass = HttpManager.PostAsyncHttpContent<DailydriveVehicleResponseBase>(
             entity: contentClass,
             requestPath: "",
             headers: GetLocationsRequestHeader(vendor)).Result;
            var contentType = new StringContent(@"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'
             xmlns:dto='http://ws.naryaz.com/model/dto'>
             <soapenv:Header/>
             <soapenv:Body>
           <dto:VehicleTypesRequest>
                <dto:language>TR</dto:language>
             </dto:VehicleTypesRequest>
             </soapenv:Body>
            </soapenv:Envelope>", Encoding.UTF8, "text/xml");
            var resultType = HttpManager.PostAsyncHttpContent<DailydriveVehicleListResponseBase>(
                 entity: contentType,
                 requestPath: "",
                 headers: GetLocationsRequestHeader(vendor)).Result;

            if (resultType?.SOAPENVEnvelope?.SOAPENVBody?.Ns2VehicleTypesResponse?.ns2vehicleTypes?.Count > 0 )
            {
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = resultType.SOAPENVEnvelope.SOAPENVBody.Ns2VehicleTypesResponse.ns2vehicleTypes.Map(resultClass?.SOAPENVEnvelope?.SOAPENVBody?.Ns2VehicleClassesResponse?.Ns2VehicleClasses)
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Dailydrive araçlar gelmiyor!"
            };
        }

        /// <summary>
        /// Capacities üzerinden ilgili veriye erişim sağlanmakta. 
        /// </summary>
        /// <param name="getVehiclesRequest"></param>
        /// <param name="vendor"></param>
        /// <param name="additionalInformation"></param>
        /// <param name="exchangeRates"></param>
        /// <param name="localVehicles"></param>
        /// <param name="subVendors"></param>
        /// <param name="baseVendorRequestCurrencyType"></param>
        /// <returns></returns>
        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var tarifNos = vendor.SecretKey.Split('-');
            var tarif1 = tarifNos.Length > 0 ? tarifNos[0] : "";
            var tarif2 = tarifNos.Length > 1 ? tarifNos[1] : "";
            // TODO: tariffNos => 202 olarak api dökümanında verilmiş
            string s = @$"<?xml version=""1.0"" encoding=""utf-8""?>
            <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'
             xmlns:dto='http://ws.naryaz.com/model/dto'>
             <soapenv:Header/>
             <soapenv:Body>
             <dto:CapacitiesRequest>
                <dto:pickupLocNo>{additionalInformation.APIPickupLocationCode}</dto:pickupLocNo>
                <dto:pickupDate>{additionalInformation.PickupDateTime.ToString("yyyy-MM-dd HH:mm")}</dto:pickupDate>
                <dto:returnLocNo>{additionalInformation.APIReturnLocationCode}</dto:returnLocNo>
                <dto:returnDate>{additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd HH:mm")}</dto:returnDate>
                <dto:classNos>0</dto:classNos>
                <dto:tariffNos>{tarif1}</dto:tariffNos>
                <dto:tariffNos>{tarif2}</dto:tariffNos>
             </dto:CapacitiesRequest>
             </soapenv:Body>
            </soapenv:Envelope>";
            var content = new StringContent(@$"<?xml version=""1.0"" encoding=""utf-8""?>
            <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'
             xmlns:dto='http://ws.naryaz.com/model/dto'>
             <soapenv:Header/>
             <soapenv:Body>
             <dto:CapacitiesRequest>
                <dto:pickupLocNo>{additionalInformation.APIPickupLocationCode}</dto:pickupLocNo>
                <dto:pickupDate>{additionalInformation.PickupDateTime.ToString("yyyy-MM-dd HH:mm")}</dto:pickupDate>
                <dto:returnLocNo>{additionalInformation.APIReturnLocationCode}</dto:returnLocNo>
                <dto:returnDate>{additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd HH:mm")}</dto:returnDate>
                <dto:classNos>0</dto:classNos>
                <dto:tariffNos>{tarif1}</dto:tariffNos>
                <dto:tariffNos>{tarif2}</dto:tariffNos>
             </dto:CapacitiesRequest>
             </soapenv:Body>
            </soapenv:Envelope>", Encoding.UTF8, "text/xml");


            var result = HttpManager.PostAsyncHttpContent<DailydriveCapacitiesResponseBase>(
                 entity: content,
                 requestPath: "",
                 headers: GetLocationsRequestHeader(vendor)).Result;

            if (result?.SOAPENVEnvelope?.SOAPENVBody?.N2CapacitiesResponse?.Ns2CapacitiesClasses?.Count > 0)
            {
                var apiVehicleList = result.SOAPENVEnvelope.SOAPENVBody.N2CapacitiesResponse.Ns2CapacitiesClasses;
                var mappedVehicleList = result.SOAPENVEnvelope.SOAPENVBody.N2CapacitiesResponse.Ns2CapacitiesClasses.Map(additionalInformation, vendor);
                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                if (vendor.VehicleMappingActive)
                {
                    List<string> list = new List<string>();
                    mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                    //apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == (p.Ns2ClassNo.ToString()+"-"+p.Ns2VehicleTypes)));
                    //foreach (var kvp in apiVehicleList) 
                    //{
                    //    foreach (var item in kvp.Ns2VehicleTypes)
                    //    {
                    //        list.Add(kvp.Ns2ClassNo + "-" + item.Ns2TypeNo);
                    //    }
                    //}

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
                    foreach (var vehicleTypes in vehicle.value.Ns2VehicleTypes)
                    {
                        var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.Ns2ClassNo+"-"+vehicleTypes.Ns2TypeNo).ToList();

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
                                //VehicleId = vehicle.value.Ns2VehicleTypes[i].Ns2TypeNo.ToIntNullSafe(),
                                VehicleId = vehicleTypes.Ns2TypeNo.ToIntNullSafe(),
                                VehicleCode = mappedVehicle.VehicleCode,
                                APIPickupLocationId = additionalInformation.APIPickupLocationId,
                                APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                                APIReturnLocationId = additionalInformation.APIReturnLocationId,
                                APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                                CurrencyType = requestCurrencyType,
                                RentalDuration = mappedVehicle.RentalDuration,
                                DailyPrice = mappedVehicle.DailyPrice,
                                OneWayFee = mappedVehicle.OneWayFee,
                                DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                                //APIDailyPrice = vehicle.value.Ns2Tariffs.Ns2TotalRentalPrice.ToFloatNullSafe() / Math.Ceiling(vehicle.value.Ns2RentalDays.ToDecimalNullSafe()).ToFloatNullSafe(), // ????????????
                                APIDailyPrice = vehicle.value.Ns2Tariffs[0].Ns2TotalRentalPrice.ToFloatNullSafe() / vehicle.value.Ns2RentalDays,
                                APIDailyPricePayNow = vehicle.value.Ns2Tariffs[0].Ns2TotalRentalPrice.ToFloatNullSafe() / vehicle.value.Ns2RentalDays, // ????????????
                                APIOneWayFee = vehicle.value.Ns2OnewayPrice.ToFloatNullSafe(),
                                APITotalPrice = vehicle.value.Ns2Tariffs[0].Ns2TotalRentalPrice.ToFloatNullSafe(),
                                APITotalPricePayNow = vehicle.value.Ns2Tariffs[0].Ns2TotalRentalPrice.ToFloatNullSafe(),
                                APIReferenceCode = vehicle.value.Ns2ClassCode.ToStringNullSafe(), // ????????????
                                APIReferenceCode2 = vehicle.value.Ns2Tariffs[1].Ns2TotalRentalPrice.ToFloatNullSafe().ToString(), // ????????????
                                APIReferenceCode3 = vehicle.value.Ns2CampaignNo.ToStringNullSafe(), // ????????????
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
                                VehicleImageUrl = mappedVehicle.VehicleImages != null ? mappedVehicle.VehicleImages.Count > 0 ? mappedVehicle.VehicleImages[0].Url : string.Empty : string.Empty,
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
                Message = "Dailydrive araçlar gelmiyor!"
            };
        }

        private Dictionary<string, object> GetLocationsRequestHeader(Vendor vendor)
        {
            var authToken = Encoding.ASCII.GetBytes($"{vendor.ApiKey}:{vendor.ApiPassword}");
            var token = Convert.ToBase64String(authToken);

            return new Dictionary<string, object>()
            {
                { "Content-Type", "text/xml"  },
                { "Accept", "*/*"  },
                { "Authorization", $"Basic {token}" },
            };
        }

    }
}
