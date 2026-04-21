using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.GreenMotion;
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
using GreenMotionHelper = KolayCAR.Broker.API.Helpers.GreenMotion;

namespace KolayCAR.Broker.API.Providers.GreenMotion
{
    public class VehicleProvider : IVehicleProvider
    {
        RestManager RestManager { get; set; }
        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            RestManager = new RestManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var arrayProps = new List<string>
            {
                "option"
            };

            var result = await RestManager.PostAsyncXMLRestClient<GreenMotionResponseBase>(
                    requestPath: string.Empty,
                    parameterType: RestSharp.ParameterType.RequestBody,
                    headers: GreenMotionHelper.RequestHelper.GetGreenMotionRequestHeader(),
                    entity: CreateAvailabilityForm(additionalInformation, baseVendorRequestCurrencyType),
                    xmlArraysPropNames: arrayProps);

            if (result?.gm_webservice?.response?.vehicles?.vehicle?.Count > 0)
            {
                //if(vendor.CreditType == CreditType.FullCredit && result.gm_webservice.response.full_credit != "yes")
                //{
                //    return new ServiceResponseBase
                //    {
                //        Success = false,
                //        Message = "Green Motion full-credit reddetti!",
                //    };
                //}
                //float onewayfee = result.gm_webservice.response.oneway_fee.ToFloatNullSafe(); //GreenMotion'da ayrı olarak tek yön tutarı gönderilmiyor. Toplam tutarın içerisinde geliyor.
                float onewayfee = 0;
                var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result.gm_webservice.response.vehicles.vehicle, v => v.groupName, v => v.total.__text.ToFloatNullSafe());
                result.gm_webservice.response.vehicles.vehicle = apiVehicleList;
                var mappedVehicleList = result.gm_webservice.response.Map(additionalInformation, vendor, onewayFee: onewayfee);
                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();
                var apiReferenceCode = result.gm_webservice.response.quoteid;

                {
                    mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                    apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.groupName.ToString()));
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
                    var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.groupName.ToString()).ToList();
                    for (int i = 0; i < tempMappedVehicleList.Count; i++)
                    {
                        var mappedVehicle = tempMappedVehicleList[i];
                        var apiDailyPrice = vehicle.value.total.__text.ToFloatNullSafe() / mappedVehicle.RentalDuration;

                        var reservationToken = new ReservationToken
                        {
                            AgencyId = additionalInformation.Agency.AgencyId,
                            VendorId = vendor.VendorId,
                            APIVendorId = vendor.VendorId,
                            APIVendorName = vendor.VendorName,
                            APIVendorPhone = vendor.VendorPhone,
                            APIVendorEmail = vendor.VendorEmail,
                            APIVendorLogo = vendor.Logo,
                            VehicleId = vehicle.value._id.ToIntNullSafe(),
                            VehicleCode = vehicle.value.groupName.ToString(),
                            APIPickupLocationId = additionalInformation.APIPickupLocationId,
                            APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                            APIReturnLocationId = additionalInformation.APIReturnLocationId,
                            APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                            CurrencyType = requestCurrencyType,
                            RentalDuration = mappedVehicle.RentalDuration,
                            DailyPrice = mappedVehicle.DailyPrice,
                            OneWayFee = 0,//GreenMotion'da ayrı olarak tek yön tutarı gönderilmiyor. Toplam tutarın içerisinde geliyor.
                            DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                            APIDailyPrice = apiDailyPrice,
                            APIDailyPricePayNow = apiDailyPrice,
                            //APIOneWayFee = 0, //GreenMotion'da ayrı olarak tek yön tutarı gönderilmiyor. Toplam tutarın içerisinde geliyor.
                            APIOneWayFee = mappedVehicle.OneWayFee, //GreenMotion'da ayrı olarak tek yön tutarı gönderilmiyor. Toplam tutarın içerisinde geliyor.
                            APIReferenceCode = $"{apiReferenceCode}|{additionalInformation.APIPickupLocationCode}",
                            APIReferenceCode2 = vehicle.value.total.__text.ToFloatNullSafe().ToString(),
                            APIReferenceCode3 = (vehicle.value.total.__text.ToFloatNullSafe() / mappedVehicle.RentalDuration).ToString(),
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

                        mappedVehicle.FullCredit = result.gm_webservice.response.full_credit == "yes" ? true : false;
                        if (additionalInformation.Agency.SpecialParameters)
                        {
                            mappedVehicle.SpecialVendorId = vendor.VendorId.ToString();
                            mappedVehicle.SpecialVendorName = vendor.VendorName;
                            mappedVehicle.SpecialVendorLogo = vendor.Logo;
                        }
                    }
                }
                mappedVehicleList.RemoveAll(x => x.DailyPrice == 0);
                return new ServiceResponseBase
                {
                    Success = mappedVehicleList.Count > 0,
                    Data = mappedVehicleList
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Green Motion servisinden sonuç alınamadı!",
            };
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var vehicles = GetVehicleList();

            #region Silinecek
            //var vehicles = new List<Vehicle>
            //  {

            //        new Vehicle
            //        {
            //            VehicleId = 1,
            //            VehicleCode = "TR-A",
            //            VehicleName = "Renault Symbol, Diesel guaranteed or similar",
            //            DepositPrice = 500,
            //            VendorMinimumDriverAge = 20,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 2,
            //            VehicleCode = "TR-B",
            //            VehicleName = "Renault Clio, Diesel guaranteed or similar",
            //            DepositPrice = 500,
            //            VendorMinimumDriverAge = 20,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 3,
            //            VehicleCode = "TR-BA",
            //            VehicleName = "Ford Fiesta, Diesel guaranteed, Automatic or similar",
            //            DepositPrice = 500,
            //            VendorMinimumDriverAge = 20,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 4,
            //            VehicleCode = "TR-BB",
            //            VehicleName = "Renault Clio Sport Tourer or similar",
            //            DepositPrice = 500,
            //            VendorMinimumDriverAge = 20,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 5,
            //            VehicleCode = "TR-BBA",
            //            VehicleName = "Renault Clio Sport Tourer, Automatic or similar",
            //            DepositPrice = 750,
            //            VendorMinimumDriverAge = 20,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 6,
            //            VehicleCode = "TR-BC",
            //            VehicleName = "Citroen C-Elysee or similar",
            //            DepositPrice = 500,
            //            VendorMinimumDriverAge = 20,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 7,
            //            VehicleCode = "TR-BCA",
            //            VehicleName = "Opel Corsa or similar",
            //            DepositPrice = 500,
            //            VendorMinimumDriverAge = 20,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 8,
            //            VehicleCode = "TR-C",
            //            VehicleName = "Renault Megane, Diesel guaranteed or similar",
            //            DepositPrice = 750,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 9,
            //            VehicleCode = "TR-C1",
            //            VehicleName = "Renault Megane Sedan Joy, or similar",
            //            DepositPrice = 750,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 10,
            //            VehicleCode = "TR-C2",
            //            VehicleName = "Opel Astra, Automatic or similar",
            //            DepositPrice = 750,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 11,
            //            VehicleCode = "TR-C3",
            //            VehicleName = "Volkswagen Golf, or similar",
            //            DepositPrice = 750,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 12,
            //            VehicleCode = "TR-CA",
            //            VehicleName = "Ford Focus, Diesel guaranteed, Automatic or similar",
            //            DepositPrice = 750,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 13,
            //            VehicleCode = "TR-CH",
            //            VehicleName = "Toyota Corolla,  Hybrid, Automatic or similar",
            //            DepositPrice = 750,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 14,
            //            VehicleCode = "TR-D",
            //            VehicleName = "Renault Megane Hatchback, Diesel guaranteed, Automatic or similar",
            //            DepositPrice = 750,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 15,
            //            VehicleCode = "TR-D1",
            //            VehicleName = "BMW I3, Automatic or Similar",
            //            DepositPrice = 2000,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 16,
            //            VehicleCode = "TR-D2",
            //            VehicleName = "Mini Cooper, Automatic or similar",
            //            DepositPrice = 750,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 17,
            //            VehicleCode = "TR-EA",
            //            VehicleName = "Skoda Superb, Diesel guaranteed, Automatic or similar",
            //            DepositPrice = 2000,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 18,
            //            VehicleCode = "TR-ES",
            //            VehicleName = "Ford EcoSport, Automatic or similar",
            //            DepositPrice = 750,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 19,
            //            VehicleCode = "TR-F",
            //            VehicleName = "Peugeot 2008 Diesel guaranteed, Automatic or similar",
            //            DepositPrice = 750,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 20,
            //            VehicleCode = "TR-F1",
            //            VehicleName = "Opel Crossland X, Manual or similar model",
            //            DepositPrice = 750,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 21,
            //            VehicleCode = "TR-G",
            //            VehicleName = "Peugeot 3008, Diesel guaranteed, Automatic or similar",
            //            DepositPrice = 2000,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 22,
            //            VehicleCode = "TR-G1",
            //            VehicleName = "Seat Ateca 1.5 Ecotsi, Automatic or similar",
            //            DepositPrice = 1500,
            //            VendorMinimumDriverAge = 24,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 23,
            //            VehicleCode = "TR-H",
            //            VehicleName = "VW Jetta, Automatic or similar",
            //            DepositPrice = 750,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 24,
            //            VehicleCode = "TR-H1",
            //            VehicleName = "Skoda Octavia, Diesel guaranteed, Automatic or similar",
            //            DepositPrice = 750,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 25,
            //            VehicleCode = "TR-HI",
            //            VehicleName = "Hyundai i20 or similar",
            //            DepositPrice = 750,
            //            VendorMinimumDriverAge = 20,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 26,
            //            VehicleCode = "TR-I",
            //            VehicleName = "BMW 216d GRAN COUPE, Diesel guaranteed, Automatic or similar",
            //            DepositPrice = 2000,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 26,
            //            VehicleCode = "TR-I1",
            //            VehicleName = "Audi A3, Automatic or Similar",
            //            DepositPrice = 1500,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 27,
            //            VehicleCode = "TR-MC",
            //            VehicleName = "Mercedes C Class 200 AMG, Automatic, Diesel guaranteed or similar",
            //            DepositPrice = 7500,
            //            VendorMinimumDriverAge = 24,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 28,
            //            VehicleCode = "TR-PA",
            //            VehicleName = "BMW 3.18 Automatic or similar",
            //            DepositPrice = 5000,
            //            VendorMinimumDriverAge = 24,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 29,
            //            VehicleCode = "TR-PB",
            //            VehicleName = "BMW 5.20, Automatic or similar",
            //            DepositPrice = 10000,
            //            VendorMinimumDriverAge = 24,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 30,
            //            VehicleCode = "TR-S",
            //            VehicleName = "Skoda Kodiaq, Automatic or similar",
            //            DepositPrice = 2000,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 31,
            //            VehicleCode = "TR-TK",
            //            VehicleName = "VW Transporter Kombi, Diesel Guaranteed, Automatic or similar",
            //            DepositPrice = 1000,
            //            VendorMinimumDriverAge = 24,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 32,
            //            VehicleCode = "TR-VQ",
            //            VehicleName = "Volvo XC90 2.0 B5 Inscription Geartronic  AWD - 7Seater, Automatic or similar",
            //            DepositPrice = 10000,
            //            VendorMinimumDriverAge = 30,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 33,
            //            VehicleCode = "TR-VS",
            //            VehicleName = "Audi A6, Automatic, Diesel guaranteed or similar",
            //            DepositPrice = 10000,
            //            VendorMinimumDriverAge = 24,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 34,
            //            VehicleCode = "TR-VX",
            //            VehicleName = "Volvo XC60, Automatic, Diesel guaranteed or similar",
            //            DepositPrice = 750,
            //            VendorMinimumDriverAge = 27,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 35,
            //            VehicleCode = "TR-Y",
            //            VehicleName = "Dacia Lodgy, Diesel guaranteed, or similar",
            //            DepositPrice = 750,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },
            //        new Vehicle
            //        {
            //            VehicleId = 36,
            //            VehicleCode = "TR-YA",
            //            VehicleName = "Ford Tourneo, Diesel guaranteed, or similar",
            //            DepositPrice = 750,
            //            VendorMinimumDriverAge = 21,
            //            VendorMinimumDrivingLicenseAge=1,
            //        },



            //  };
            #endregion
            return new ServiceResponseBase
            {
                Success = true,
                Data = vehicles
            };
        }

        public string GetAvailabilityRequestBodyEntity(ResponseReservationStepsAdditionalInformation additionalInformation, CurrencyTypes baseVendorRequestCurrencyType)
        {
            var getVehiclesPayload = new GreenMotionRequestBase.GreenMotionGetVehiclesRequest
            {
                location_id = additionalInformation.APIPickupLocationCode.ToIntNullSafe(),
                dropoff_location_id = additionalInformation.APIReturnLocationCode.ToIntNullSafe(),
                start_date = additionalInformation.PickupDateTime.ToString("yyyy-MM-dd"),
                start_time = additionalInformation.PickupDateTime.ToString("HH:mm"),
                end_date = additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd"),
                end_time = additionalInformation.ReturnDateTime.ToString("HH:mm"),
                age = 35,
                fuel = string.Empty,
                currency = baseVendorRequestCurrencyType.ToString(),
                userid = string.Empty,
                username = string.Empty,
                type = "GetVehicles",
                full_credit = additionalInformation.Vendor.CreditType == CreditType.FullCredit ? "Yes" : ""
            };

            var body = GreenMotionHelper.RequestHelper.GetGreenMotionRequestObject(additionalInformation.Vendor, getVehiclesPayload);

            return ObjectHelper.ObjectToXML(body);
        }

        public Dictionary<string, object> CreateAvailabilityForm(ResponseReservationStepsAdditionalInformation additionalınformation, CurrencyTypes baseVendorRequestCurrencyType) =>
            new Dictionary<string, object>()
            {
                { "application/xml", GetAvailabilityRequestBodyEntity(additionalınformation, baseVendorRequestCurrencyType) },
            };

        private List<Vehicle> GetVehicleList()
        {
            var excelResult = ExcelHelper.ReadExcel(@"Docs\GreenMotion\GreenmotionVehicleList.xlsx");
            var vehicles = new List<Vehicle>();

            if (excelResult.Rows.Count > 0)
            {
                for (int i = 0; i < excelResult.Rows.Count; i++)
                {
                    vehicles.Add(new Vehicle
                    {
                        VehicleId = i + 1,
                        VehicleCode = excelResult.Rows[i]["Araç Grubu"].ToStringNullSafe(),
                        VehicleName = excelResult.Rows[i]["Model"].ToStringNullSafe() + " (" + excelResult.Rows[i]["Yakıt Türü"].ToStringNullSafe() + "-" + excelResult.Rows[i]["Manuel - Otomatik"].ToStringNullSafe() + "-" + excelResult.Rows[i]["Sedan - Hatchback - Station Vagon - Suv"].ToStringNullSafe() + ") ",
                        DepositPrice = excelResult.Rows[i]["Depozito"].ToFloatNullSafe(),
                        VendorMinimumDriverAge = excelResult.Rows[i]["Minimum Yaş"].ToIntNullSafe(),
                        VendorMinimumDrivingLicenseAge = excelResult.Rows[i]["Minimum Ehliyet Süresi"].ToIntNullSafe()
                        //VehicleId = i + 1,
                        //VehicleCode = excelResult.Rows[i]["Araç Grubu"].ToStringNullSafe(),
                        //VehicleName = excelResult.Rows[i]["Araç Grubu Açıklaması"].ToStringNullSafe() + " (" + excelResult.Rows[i]["Yakıt Tipi"].ToStringNullSafe() + "-" + excelResult.Rows[i]["Vites Tipi"].ToStringNullSafe() + "-" + excelResult.Rows[i]["Kasa Tipi"].ToStringNullSafe() + ") ",
                        //DepositPrice = excelResult.Rows[i]["Depozito Tutarı"].ToFloatNullSafe(),
                        //VendorMinimumDriverAge = excelResult.Rows[i]["Minimum Kiralama Yaşı"].ToIntNullSafe(),
                        //VendorMinimumDrivingLicenseAge = excelResult.Rows[i]["Minimum Ehliyet Yaşı"].ToIntNullSafe()
                    });
                }
            }

            return vehicles;
        }
    }
}
