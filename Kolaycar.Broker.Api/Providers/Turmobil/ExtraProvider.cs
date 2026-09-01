using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Turmobil;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.TurmobilResponseBase;
using TurmobilProvider = KolayCAR.Broker.API.Providers.Turmobil;

namespace KolayCAR.Broker.API.Providers.Turmobil
{

    public class ExtraProvider : IExtraProvider
    {
        IVehicleProvider vehicleProvider;
        RestManager RestManager { get; set; }

        public ExtraProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            #region Yanlış fonksiyon tanımı yapılmış
            //            var content = new StringContent(@"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'
            // xmlns:dto='http://ws.naryaz.com/model/dto'>
            // <soapenv:Header/>
            // <soapenv:Body>
            // <dto:ExtraProductsRequest>
            // </dto:ExtraProductsRequest>
            // </soapenv:Body>
            //</soapenv:Envelope>
            //", Encoding.UTF8, "text/xml");
            //            var result = HttpManager.PostAsyncHttpContent<DailydriveLocationResponseBase>(
            //                 entity: content,
            //                 requestPath: "",
            //                 headers: GetLocationsRequestParameters(vendor)).Result;

            //            if (result != null)
            //            {
            //                if (result != null)
            //                    return new ServiceResponseBase
            //                    {
            //                        Success = result.SOAPENVEnvelope.SOAPENVBody.Ns2LocationsResponse.Ns2Locations.Count > 0,
            //                        //Data = result.SOAPENVEnvelope.SOAPENVBody.Ns2LocationsResponse.Ns2Locations.Map()
            //                    };
            //            }

            //            return new ServiceResponseBase
            //            {
            //                Success = false,
            //                Message = "Dailydrive lokasyonları gelmiyor!"
            //            }; 
            #endregion

            var result = await RestManager.PostAsync<TurmobilRequestBase.Extra, ExtrasTurmobil>(
                requestPath: "rest/dailyrezervation/additionalServiceList",
                entity: new TurmobilRequestBase.Extra
                {
                    deliveryDate = DateTime.Now.AddDays(4).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                    returnDate = DateTime.Now.AddDays(4).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                    groupType = "",
                    lang = "TR",
                    curr = "TRY"
                },
                headers: new Dictionary<string, object>
                    {
                        {"username" , vendor.ApiKey},
                        {"password" , vendor.ApiPassword}
                    }
                );

            if (result != null)
            {
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = result.data.Map(vendor.VendorId)
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Data = null
            };
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            vehicleProvider = new VehicleProvider(vendor, false);
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();

            var result = await RestManager.PostAsync<TurmobilRequestBase.Extra, ExtrasTurmobil>(
                requestPath: "rest/dailyrezervation/additionalServiceList",
                entity: new TurmobilRequestBase.Extra
                {
                    deliveryDate = getExtrasRequest.PickupDate.Replace('.', '/') + " " + getExtrasRequest.PickupTime,
                    returnDate = getExtrasRequest.ReturnDate.Replace('.', '/') + " " + getExtrasRequest.ReturnTime,
                    groupType = "",
                    lang = "TR",
                    curr = "TRY"
                },
                headers: new Dictionary<string, object>
                    {
                        {"username" , vendor.ApiKey},
                        {"password" , vendor.ApiPassword}
                    }
                );
            

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleId == additionalInformation.VehicleId);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);

            var extras = new List<Domain.Models.Extra>();

            if (result?.data?.Count > 0)
            {
                extras = result.data.Map(vendor.VendorId);
                CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);
            }

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle, vehicles);
            return new ServiceResponseBase(getExtrasResponse, true);
        }

        private Dictionary<string, object> GetLocationsRequestParameters(Vendor vendor)
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

        private TurmobilRequestBase.Extra GetAdditionalProductBodyEntity(GetExtrasRequest getExtrasRequest)
        {
            return new TurmobilRequestBase.Extra
            {
                deliveryDate = getExtrasRequest.PickupDate.Replace('.', '/') + " " + getExtrasRequest.PickupTime,
                returnDate = getExtrasRequest.ReturnDate.Replace('.', '/') + " " + getExtrasRequest.ReturnTime,
                groupType = "",
                lang = getExtrasRequest.LanguageCode.ToUpper(),
                curr = getExtrasRequest.CurrencyCode
            };
            //return new TurmobilRequestBase.Extra
            //{
            //    deliveryDate = DateTime.ParseExact(getExtrasRequest.PickupDate.Replace('.', '/') + " " + getExtrasRequest.PickupTime, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy HH:mm"),
            //    returnDate = DateTime.ParseExact(getExtrasRequest.ReturnDate.Replace('.', '/') + " " + getExtrasRequest.ReturnTime, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy HH:mm"),
            //    groupType = "",
            //    lang = getExtrasRequest.LanguageCode.ToUpper(),
            //    curr = getExtrasRequest.CurrencyCode
            //};
        }
    }
}
