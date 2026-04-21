using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Garenta
{
    public class ExtraProvider : IExtraProvider
    {
        VehicleProvider _vehicleProvider { get; set; }
        public ExtraProvider(Vendor vendor)
        {

        }

        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var extras = GetExtrasListFromExcel(vendor);
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

            selectedVehicle.Extras = GetExtrasListFromExcel(vendor);

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(selectedVehicle.Extras), selectedVehicle);
            return new ServiceResponseBase(getExtrasResponse, true);
        }

        private List<Extra> GetExtrasListFromExcel(Vendor vendor)
        {
            var excelResult = ExcelHelper.ReadExcel($@"Docs\Garenta\{vendor.VendorName}\AdditionalProducts.xlsx");
            var extras = new List<Extra>();

            if (excelResult.Rows.Count > 0)
            {
                for (int i = 0; i < excelResult.Rows.Count; i++)
                {
                    extras.Add(new Extra
                    {
                        ExtraId = i + 1,
                        ExtraCode = excelResult.Rows[i]["Product_Id"].ToStringNullSafe(),
                        ExtraName = excelResult.Rows[i]["Ek Ürünler"].ToStringNullSafe(),
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = excelResult.Rows[i]["Günlük Tutar"].ToFloatNullSafe(),
                        ApiPrice = excelResult.Rows[i]["Günlük Tutar"].ToFloatNullSafe()
                    });
                }
            }
            return extras;
        }
    }
}
