using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AkkorProvider = KolayCAR.Broker.API.Providers.Akkor;

namespace KolayCAR.Broker.API.Providers.Aytu
{
    public class ExtraProvider : IExtraProvider
    {
        IVehicleProvider vehicleProvider;
        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var extras = new List<Extra>
               {
                    new Extra
                    {
                        ExtraId = 1,
                        ExtraCode = "Baby_Seat",
                        ExtraName = "Bebek koltuğu",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 2,
                        ExtraCode = "Navigation",
                        ExtraName = "Navigasyon",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 3,
                        ExtraCode = "Private_Driver",
                        ExtraName = "Özel şoför",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 4,
                        ExtraCode = "Additional_Driver",
                        ExtraName = "Ek sürücü paketi",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 5,
                        ExtraCode = "Child_Seat",
                        ExtraName = "Çocuk koltuğu",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 6,
                        ExtraCode = "Additional_KM",
                        ExtraName = "Ek km paketi",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 8,
                        ExtraCode = "Young_Drive",
                        ExtraName = "Genç sürücü paketi",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 9,
                        ExtraCode = "SCDW",
                        ExtraName = "Süper kasko paketi",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 10,
                        ExtraCode = "CDW",
                        ExtraName = "Hasar Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 11,
                        ExtraCode = "PAI",
                        ExtraName = "Ferdi Kaza Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 12,
                        ExtraCode = "TGI",
                        ExtraName = "Lastik-Cam-Far Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 13,
                        ExtraCode = "KM_150",
                        ExtraName = "İlave 150km",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 14,
                        ExtraCode = "KM_400",
                        ExtraName = "İlave 400km",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 15,
                        ExtraCode = "Tablet_Navigation",
                        ExtraName = "Tablet Navigation",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
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

            vehicleProvider = new AkkorProvider.VehicleProvider(vendor, false);

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

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
    }
}
