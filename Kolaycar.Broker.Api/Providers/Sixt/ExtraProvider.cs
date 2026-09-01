using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Sixt
{
    public class ExtraProvider : IExtraProvider
    {
        IVehicleProvider VehicleProvider { get; set; }
        public ExtraProvider()
        {

        }
        public ExtraProvider(Vendor vendor)
        {
            VehicleProvider = new VehicleProvider(vendor, false);
        }
        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var extras = new List<Domain.Models.Extra>
            {
                new Extra
                {
                    ExtraId = 1,
                    ExtraCode = "SCT",
                    ExtraName = "Scooter",
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false,
                    ExtraType = AdditionalProductTypes.Extra
                },
                new Extra
                {
                    ExtraId = 2,
                    ExtraCode = "ADD",
                    ExtraName = "Ek Sürücü Paketi",
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false,
                    ExtraType = AdditionalProductTypes.Extra
                },
                new Extra
                {
                    ExtraId = 3,
                    ExtraCode = "BBK",
                    ExtraName = "Bebek Koltuğu",
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false,
                    ExtraType = AdditionalProductTypes.Extra
                },
                new Extra
                {
                    ExtraId = 4,
                    ExtraCode = "CCK",
                    ExtraName = "Çocuk Koltuğu",
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false,
                    ExtraType = AdditionalProductTypes.Extra
                },
                new Extra
                {
                    ExtraId = 5,
                    ExtraCode = "ADP",
                    ExtraName = "Koltuk Adaptörü",
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false,
                    ExtraType = AdditionalProductTypes.Extra
                },
                new Extra
                {
                    ExtraId = 6,
                    ExtraCode = "GSP",
                    ExtraName = "Genç Sürücü Paketi",
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false,
                    ExtraType = AdditionalProductTypes.Extra
                },
                new Extra
                {
                    ExtraId = 7,
                    ExtraCode = "WIFI",
                    ExtraName = "Wi-Fi Internet Kit",
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false,
                    ExtraType = AdditionalProductTypes.Extra
                },
                new Extra
                {
                    ExtraId = 8,
                    ExtraCode = "WT",
                    ExtraName = "Kış Lastiği (stoklarla sınırlıdır.)",
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false,
                    ExtraType = AdditionalProductTypes.Extra
                },
                //insurance
                new Extra
                {
                    ExtraId = 9,
                    ExtraCode = "MS-1",
                    ExtraName = "Muafiyetsiz Kaza Güvencesi",
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false,
                    ExtraType = AdditionalProductTypes.Insurance
                },
                new Extra
                {
                    ExtraId = 10,
                    ExtraCode = "BKS",
                    ExtraName = "Bireysel Kaza Güvencesi",
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false,
                    ExtraType = AdditionalProductTypes.Insurance
                },
                new Extra
                {
                    ExtraId = 11,
                    ExtraCode = "TPI",
                    ExtraName = "3. Şahıs Sorumluluk Güvencesi",
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false,
                    ExtraType = AdditionalProductTypes.Insurance
                },
                new Extra
                {
                    ExtraId = 12,
                    ExtraCode = "LCF-1",
                    ExtraName = "Lastik-Cam-Far Güvencesi",
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false,
                    ExtraType = AdditionalProductTypes.Insurance
                },
                new Extra
                {
                    ExtraId = 13,
                    ExtraCode = "MHG3",
                    ExtraName = "Mega Mini Hasar Güvencesi",
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false,
                    ExtraType = AdditionalProductTypes.Insurance
                },
                new Extra
                {
                    ExtraId = 14,
                    ExtraCode = "MHG1",
                    ExtraName = "Mini Hasar Güvencesi",
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false,
                    ExtraType = AdditionalProductTypes.Insurance
                },
                new Extra
                {
                    ExtraId = 15,
                    ExtraCode = "MHG2",
                    ExtraName = "Süper Mini Hasar Güvencesi",
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false,
                    ExtraType = AdditionalProductTypes.Insurance
                },
                new Extra
                {
                    ExtraId = 16,
                    ExtraCode = "EXP-1",
                    ExtraName = "Express Paket",
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false,
                    ExtraType = AdditionalProductTypes.Insurance
                },
                new Extra
                {
                    ExtraId = 17,
                    ExtraCode = "GLD-1",
                    ExtraName = "Gold Paket",
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false,
                    ExtraType = AdditionalProductTypes.Insurance
                },
                new Extra
                {
                    ExtraId = 18,
                    ExtraCode = "PLT-1",
                    ExtraName = "Platinum Paket",
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false,
                    ExtraType = AdditionalProductTypes.Insurance
                }
            };

            return new ServiceResponseBase
            {
                Success = true,
                Data = extras
            };
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var requestLanguageType = getExtrasRequest.LanguageCode.ToEnum<LanguageTypes>();

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await VehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == reservationToken.VehicleCode);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            var extras = selectedVehicle.Extras;
            
            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle, vehicles);
            return new ServiceResponseBase(getExtrasResponse, true);
        }
    }
}
