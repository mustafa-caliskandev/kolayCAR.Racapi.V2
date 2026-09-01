using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Helpers.Garenta;
using KolayCAR.Broker.API.Mappers.Garenta;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Garenta
{
    public class ExtraProvider : IExtraProvider
    {
        private readonly RestManager _restManager;
        private readonly AuthProvider _authProvider;
        private VehicleProvider _vehicleProvider { get; set; }

        public ExtraProvider(Vendor vendor)
        {
            _restManager = new RestManager(vendor.APIBaseUrl, timeout: vendor.APITimeout);
            _authProvider = new AuthProvider();
        }

        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            // Eski Excel akisi:
            // var extras = GetExtrasListFromExcel(vendor);
            // return new ServiceResponseBase(extras, extras.Count > 0);

            var apiExtras = await GetApiExtraList(vendor, languageType);
            var extras = apiExtras.Map();

            return new ServiceResponseBase(extras, extras.Count > 0);
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            _vehicleProvider = new VehicleProvider(vendor, false);
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var requestLanguageType = getExtrasRequest.LanguageCode.ToEnum<LanguageTypes>();

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await _vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == reservationToken.VehicleCode);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            // Eski Excel akisi:
            // selectedVehicle.Extras = GetExtrasListFromExcel(vendor);

            var apiExtras = await GetApiExtraList(vendor, requestLanguageType);
            var extras = await SearchExtras(getExtrasRequest, vendor, additionalInformation, reservationToken, apiExtras, requestLanguageType);
            extras.RemoveAll(extra => extra == null || string.IsNullOrWhiteSpace(extra.ExtraCode) || extra.IsRequired == true);
            selectedVehicle.Extras = extras;

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(selectedVehicle.Extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(selectedVehicle.Extras), selectedVehicle, vehicles);
            return new ServiceResponseBase(getExtrasResponse, true);
        }

        private async Task<List<GarentaResponseBase.EXTRA>> GetApiExtraList(Vendor vendor, LanguageTypes languageType)
        {
            var result = await _restManager.PostAsync<GarentaRequestBase, GarentaResponseBase>(
                requestPath: string.Empty,
                entity: GetExtraListRequestBody(vendor, languageType),
                headers: _authProvider.CreateAuthHeaderWithContentType(vendor));

            return result?.EXPORT?.ES_OUTPUT?.EXTRAS ?? new List<GarentaResponseBase.EXTRA>();
        }

        private async Task<List<Extra>> SearchExtras(
            GetExtrasRequest getExtrasRequest,
            Vendor vendor,
            ResponseReservationStepsAdditionalInformation additionalInformation,
            ReservationToken reservationToken,
            List<GarentaResponseBase.EXTRA> apiExtras,
            LanguageTypes languageType)
        {
            var searchExtras = apiExtras?
                .Where(extra => !string.IsNullOrWhiteSpace(extra.PRODUCT_ID))
                .Select(extra => new GarentaRequestBase.EXTRA
                {
                    PRODUCT_ID = extra.PRODUCT_ID,
                    COUNT = "1"
                })
                .ToList() ?? new List<GarentaRequestBase.EXTRA>();

            if (searchExtras.Count == 0)
                return new List<Extra>();

            var result = await _restManager.PostAsync<GarentaRequestBase, GarentaResponseBase>(
                requestPath: string.Empty,
                entity: GetSearchExtrasRequestBody(getExtrasRequest, vendor, additionalInformation, reservationToken, searchExtras, languageType),
                headers: _authProvider.CreateAuthHeaderWithContentType(vendor));

            return result?.EXPORT?.ES_OUTPUT?.EXTRAS?.Map() ?? new List<Extra>();
        }

        private static GarentaRequestBase GetExtraListRequestBody(Vendor vendor, LanguageTypes languageType) =>
            new GarentaRequestBase
            {
                sap_props = RequestHelper.GetSapProps(GarentaRequestBase.ServiceTypes.GET_EXTRAS, vendor.ApiKey, vendor.ApiPassword),
                import = new GarentaRequestBase.Import
                {
                    IS_INPUT = new GarentaRequestBase.ISINPUT_BASE
                    {
                        BROKER_CODE = vendor.ApiKey,
                        LANGU = RequestHelper.GetLanguageType(languageType)
                    }
                }
            };

        private static GarentaRequestBase GetSearchExtrasRequestBody(
            GetExtrasRequest getExtrasRequest,
            Vendor vendor,
            ResponseReservationStepsAdditionalInformation additionalInformation,
            ReservationToken reservationToken,
            List<GarentaRequestBase.EXTRA> extras,
            LanguageTypes languageType) =>
            new GarentaRequestBase
            {
                sap_props = RequestHelper.GetSapProps(GarentaRequestBase.ServiceTypes.SEARCH_EXTRAS, vendor.ApiKey, vendor.ApiPassword),
                import = new GarentaRequestBase.Import
                {
                    IS_INPUT = new GarentaRequestBase.ISINPUT_SEARCH_EXTRAS
                    {
                        SEARCH = new GarentaRequestBase.SEARCH_EXTRAS
                        {
                            EXTRAS = extras,
                            SIPP_CODE = reservationToken.VehicleCode,
                            PICKUP_DATE = getExtrasRequest.PickupDate,
                            PICKUP_TIME = FormatTime(getExtrasRequest.PickupTime),
                            DROPOFF_DATE = getExtrasRequest.ReturnDate,
                            DROPOFF_TIME = FormatTime(getExtrasRequest.ReturnTime),
                            PICKUP_OFFICE = additionalInformation.APIPickupLocationCode,
                            RETURN_OFFICE = additionalInformation.APIReturnLocationCode
                        },
                        BROKER_CODE = vendor.ApiKey,
                        LANGU = RequestHelper.GetLanguageType(languageType)
                    }
                }
            };

        private static string FormatTime(string time)
        {
            if (string.IsNullOrWhiteSpace(time))
                return time;

            return time.Count(character => character == ':') == 1 ? $"{time}:00" : time;
        }

        // Eski Excel akisi:
        // private List<Extra> GetExtrasListFromExcel(Vendor vendor)
        // {
        //     var excelResult = ExcelHelper.ReadExcel($@"Docs\Garenta\{vendor.VendorName}\AdditionalProducts.xlsx");
        //     var extras = new List<Extra>();
        //
        //     if (excelResult.Rows.Count > 0)
        //     {
        //         for (int i = 0; i < excelResult.Rows.Count; i++)
        //         {
        //             extras.Add(new Extra
        //             {
        //                 ExtraId = i + 1,
        //                 ExtraCode = excelResult.Rows[i]["Product_Id"].ToStringNullSafe(),
        //                 ExtraName = excelResult.Rows[i]["Ek Ürünler"].ToStringNullSafe(),
        //                 ExtraRentalType = ExtraRentalTypes.Daily,
        //                 ExtraQuantityIncreasable = false,
        //                 Price = excelResult.Rows[i]["Günlük Tutar"].ToFloatNullSafe(),
        //                 ApiPrice = excelResult.Rows[i]["Günlük Tutar"].ToFloatNullSafe()
        //             });
        //         }
        //     }
        //     return extras;
        // }
    }
}
