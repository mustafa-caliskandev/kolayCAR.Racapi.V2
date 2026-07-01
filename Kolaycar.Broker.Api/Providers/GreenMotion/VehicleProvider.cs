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
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
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
            var excelResult = ExcelHelper.ReadExcel(@$"Docs\GreenMotion\{vendor.VendorName}\VehicleList.xlsx");
            var vehicles = new List<Vehicle>();

            if (excelResult.Rows.Count > 0)
            {
                for (int i = 0; i < excelResult.Rows.Count; i++)
                {
                    var row = excelResult.Rows[i];
                    var vehicleCode = GetExcelValue(row, "Araç Grubu", "Arac Grubu", "Vehicle Group", "Group");
                    var model = GetExcelValue(row, "Model", "Vehicle Model");

                    if (string.IsNullOrWhiteSpace(vehicleCode) && string.IsNullOrWhiteSpace(model))
                        continue;

                    var fuelType = GetExcelValue(row, "Yakıt Türü", "Yakit Turu", "Fuel Type", "Fuel");
                    var transmissionType = GetExcelValue(row, "Manuel - Otomatik", "Manuel Otomatik", "Transmission");
                    var bodyType = GetExcelValue(row, "Sedan - Hatchback - Station Vagon - Suv", "Body Type", "Vehicle Type");

                    vehicles.Add(new Vehicle
                    {
                        VehicleId = vehicles.Count + 1,
                        VehicleCode = vehicleCode,
                        VehicleName = BuildVehicleName(model, fuelType, transmissionType, bodyType),
                        DepositPrice = GetExcelValue(row, "Depozito", "Deposit", "Deposit Amount").ToFloatNullSafe(),
                        VendorMinimumDriverAge = GetExcelValue(row, "Minimum Yaş", "Minimum Yas", "Min Age", "Minimum Age").ToIntNullSafe(),
                        VendorMinimumDrivingLicenseAge = GetExcelValue(row, "Minimum Ehliyet Süresi", "Minimum Ehliyet Suresi", "Min License Age", "Minimum License Age").ToIntNullSafe()
                    });
                }
            }

            return new ServiceResponseBase
            {
                Success = true,
                Data = vehicles
            };
        }

        private static string GetExcelValue(DataRow row, params string[] columnNames)
        {
            foreach (var columnName in columnNames)
            {
                var normalizedColumnName = NormalizeExcelColumnName(columnName);
                var column = row.Table.Columns
                    .Cast<DataColumn>()
                    .FirstOrDefault(x => NormalizeExcelColumnName(x.ColumnName) == normalizedColumnName);

                if (column != null)
                    return row[column].ToStringNullSafe();
            }

            return string.Empty;
        }

        private static string NormalizeExcelColumnName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalizedValue = value.Trim().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalizedValue.Length);

            foreach (var character in normalizedValue)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                    continue;

                if (char.IsLetterOrDigit(character))
                    builder.Append(char.ToUpperInvariant(character));
            }

            return builder.ToString();
        }

        private static string BuildVehicleName(string model, params string[] details)
        {
            var filteredDetails = details
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            if (filteredDetails.Count == 0)
                return model;

            return $"{model} ({string.Join("-", filteredDetails)}) ";
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
                full_credit = additionalInformation.Vendor.CreditType == CreditType.FullCredit
                    && additionalInformation.Agency.CreditType == CreditType.FullCredit
                        ? "Yes"
                        : ""
            };

            var body = GreenMotionHelper.RequestHelper.GetGreenMotionRequestObject(additionalInformation.Vendor, getVehiclesPayload);

            return ObjectHelper.ObjectToXML(body);
        }

        public Dictionary<string, object> CreateAvailabilityForm(ResponseReservationStepsAdditionalInformation additionalınformation, CurrencyTypes baseVendorRequestCurrencyType) =>
            new Dictionary<string, object>()
            {
                { "application/xml", GetAvailabilityRequestBodyEntity(additionalınformation, baseVendorRequestCurrencyType) },
            };
    }
}
