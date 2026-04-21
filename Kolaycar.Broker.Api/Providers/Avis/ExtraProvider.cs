using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Avis
{
    public class ExtraProvider : IExtraProvider
    {
        private VehicleProvider _vehicleProvider;
        public ExtraProvider()
        {

        }

        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration) => new ServiceResponseBase(GetExtrasListFromExcel(), GetExtrasListFromExcel().Count() > 0);
        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {

            _vehicleProvider = new VehicleProvider(vendor, false);
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await _vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleId == additionalInformation.VehicleId);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            var extras = selectedVehicle.Extras;

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle);
            return new ServiceResponseBase(getExtrasResponse, true);
        }
        public List<Extra> GetExtrasListFromExcel()
        {
            var excelResult = ExcelHelper.ReadExcel(@"Docs\Avis\AvisAdditionalProduct.xlsx");
            var extras = new List<Extra>();

            if (excelResult.Rows.Count > 0)
            {
                for (int i = 0; i < excelResult.Rows.Count; i++)
                {
                    extras.Add(new Extra
                    {
                        ExtraId = excelResult.Rows[i]["productNo"].ToIntNullSafe(),
                        ExtraCode = excelResult.Rows[i]["UrunCode"].ToStringNullSafe(),
                        ExtraName = excelResult.Rows[i]["UrunAdi"].ToStringNullSafe(),
                        ExtraRentalType = ExtraRentalTypes.PerRental,//Sorgulanan gün sayısına göre ek ürün fiyatı dönmektedir
                        ExtraQuantityIncreasable = false,
                    });
                }
            }
            return extras;
        }
    }
}
