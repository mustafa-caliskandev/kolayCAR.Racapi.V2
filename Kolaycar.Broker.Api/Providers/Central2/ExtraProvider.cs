using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Central2;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.Central2ResponseBase;
using Central2Provider = KolayCAR.Broker.API.Providers.Central2;

namespace KolayCAR.Broker.API.Providers.Central2
{
    public class ExtraProvider : IExtraProvider
    {
        RestManager RestManager { get; set; }
        IVehicleProvider vehicleProvider;

        public ExtraProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var result = await RestManager.GetXmlAsync<CentralResponse>(
                requestPath: "XML_insurance_service.Aspx",
                parameters: GetExtraListRequestParameters(vendor)
                );

            if (result != null && result.Turevsistem != null && result.Turevsistem.ExtraList != null && result.Turevsistem.ExtraList.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = result.Turevsistem.ExtraList.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Data = null
            };

            #region silinecek
            //var extras = new List<Extra>
            //   {
            //        new Extra
            //        {
            //            ExtraId = 1,
            //            ExtraCode = "Baby_Seat",
            //            ExtraName = "Bebek koltuğu",
            //            ExtraRentalType = ExtraRentalTypes.PerRental,
            //            ExtraQuantityIncreasable = false
            //        },
            //        new Extra
            //        {
            //            ExtraId = 2,
            //            ExtraCode = "Navigation",
            //            ExtraName = "Navigasyon",
            //            ExtraRentalType = ExtraRentalTypes.PerRental,
            //            ExtraQuantityIncreasable = false
            //        },
            //        new Extra
            //        {
            //            ExtraId = 3,
            //            ExtraCode = "Private_Driver",
            //            ExtraName = "Özel şoför",
            //            ExtraRentalType = ExtraRentalTypes.PerRental,
            //            ExtraQuantityIncreasable = false
            //        },
            //        new Extra
            //        {
            //            ExtraId = 4,
            //            ExtraCode = "Additional_Driver",
            //            ExtraName = "Ek sürücü paketi",
            //            ExtraRentalType = ExtraRentalTypes.PerRental,
            //            ExtraQuantityIncreasable = false
            //        },
            //        new Extra
            //        {
            //            ExtraId = 5,
            //            ExtraCode = "Child_Seat",
            //            ExtraName = "Çocuk koltuğu",
            //            ExtraRentalType = ExtraRentalTypes.PerRental,
            //            ExtraQuantityIncreasable = false
            //        },
            //        new Extra
            //        {
            //            ExtraId = 6,
            //            ExtraCode = "Additional_KM",
            //            ExtraName = "Ek km paketi",
            //            ExtraRentalType = ExtraRentalTypes.PerRental,
            //            ExtraQuantityIncreasable = false
            //        },
            //        new Extra
            //        {
            //            ExtraId = 8,
            //            ExtraCode = "Young_Drive",
            //            ExtraName = "Genç sürücü paketi",
            //            ExtraRentalType = ExtraRentalTypes.PerRental,
            //            ExtraQuantityIncreasable = false
            //        },
            //        new Extra
            //        {
            //            ExtraId = 9,
            //            ExtraCode = "SCDW",
            //            ExtraName = "Süper kasko paketi",
            //            ExtraRentalType = ExtraRentalTypes.PerRental,
            //            ExtraQuantityIncreasable = false
            //        },
            //        new Extra
            //        {
            //            ExtraId = 10,
            //            ExtraCode = "CDW",
            //            ExtraName = "Hasar Sigortası",
            //            ExtraRentalType = ExtraRentalTypes.PerRental,
            //            ExtraQuantityIncreasable = false
            //        },
            //        new Extra
            //        {
            //            ExtraId = 11,
            //            ExtraCode = "PAI",
            //            ExtraName = "Ferdi Kaza Sigortası",
            //            ExtraRentalType = ExtraRentalTypes.PerRental,
            //            ExtraQuantityIncreasable = false
            //        },
            //        new Extra
            //        {
            //            ExtraId = 12,
            //            ExtraCode = "TGI",
            //            ExtraName = "Lastik-Cam-Far Sigortası",
            //            ExtraRentalType = ExtraRentalTypes.PerRental,
            //            ExtraQuantityIncreasable = false
            //        },
            //        new Extra
            //        {
            //            ExtraId = 13,
            //            ExtraCode = "KM_150",
            //            ExtraName = "İlave 150km",
            //            ExtraRentalType = ExtraRentalTypes.PerRental,
            //            ExtraQuantityIncreasable = false
            //        },
            //        new Extra
            //        {
            //            ExtraId = 14,
            //            ExtraCode = "KM_400",
            //            ExtraName = "İlave 400km",
            //            ExtraRentalType = ExtraRentalTypes.PerRental,
            //            ExtraQuantityIncreasable = false
            //        },
            //        new Extra
            //        {
            //            ExtraId = 15,
            //            ExtraCode = "Tablet_Navigation",
            //            ExtraName = "Tablet Navigation",
            //            ExtraRentalType = ExtraRentalTypes.PerRental,
            //            ExtraQuantityIncreasable = false
            //        }
            //   };

            //return new ServiceResponseBase
            //{
            //    Success = true,
            //    Data = extras
            //}; 
            #endregion
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            vehicleProvider = new VehicleProvider(vendor, false);
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();            

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
        private Dictionary<string, object> GetExtraListRequestParameters(Vendor vendor)
        {
            return new Dictionary<string, object>()
            {
                { "Key_Hack", vendor.SecretKey }
            };
        }
    }
}
