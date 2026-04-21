using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Otorento;
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
namespace KolayCAR.Broker.API.Providers.Otorento
{
    public class VehicleProvider : IVehicleProvider
    {
        public readonly RestManager _restManager;
        public readonly AuthProvider _authProvider;

        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            _restManager = new RestManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
            _authProvider = new AuthProvider(vendor.APIBaseUrl);
        }
        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            //var token = _authProvider.GetTicket(vendor, 1);

            //var result = await _restManager.PostAsyncXMLRestClient<OtorentoResponseBase.GetTicketResponseBase>(
            //requestPath: "/VehicleService.asmx",
            //parameterType: RestSharp.ParameterType.RequestBody,
            //entity: GetVehicleListRequestParameters(vendor, token.Result.GetTicketResult.ToString()));

            //if (result != null && 
            //    result._Envelope != null && 
            //    result._Envelope.Body != null && 
            //    result._Envelope.Body.SearchResponse != null &&
            //    result._Envelope.Body.SearchResponse.SearchResult != null &&
            //    result._Envelope.Body.SearchResponse.SearchResult.VehicleModel != null &&
            //    result._Envelope.Body.SearchResponse.SearchResult.VehicleModel.Count > 0)
            //{
            //    return new ServiceResponseBase
            //    {
            //        Success = result != null,
            //        Data = result._Envelope.Body.SearchResponse.SearchResult.VehicleModel.Map()
            //    };
            //}

            throw new System.NotImplementedException();
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var token = await _authProvider.GetTicket(vendor, 1);

            if (token != null)
            {
                var result = await _restManager.PostAsyncXMLRestClient<OtorentoResponseBase.Root>(
                 requestPath: "/VehicleService.asmx",
                 parameterType: RestSharp.ParameterType.RequestBody,
                 entity: GetVehicleListRequestParameters(vendor, token.GetTicketResult.ToString(), additionalInformation, getVehiclesRequest, baseVendorRequestCurrencyType));

              if (result?.soapEnvelope?.soapBody?.SearchResponse?.SearchResult?.VehicleModel?.Count > 0)
                {
                    var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result.soapEnvelope.soapBody.SearchResponse.SearchResult.VehicleModel, v => v.Id, v => v.Price.ToFloatNullSafe());
                    apiVehicleList.RemoveAll(e => e.NumberOfDays == "0");
                    //var mappedVehicleList = result.soapEnvelope.soapBody.SearchResponse.SearchResult.VehicleModel.Map(additionalInformation);
                    var mappedVehicleList = apiVehicleList.Map(additionalInformation, vendor);
                    mappedVehicleList.RemoveAll(e => e.RentalDuration == 0);
                    var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                    if (vendor.VehicleMappingActive)
                    {
                        //mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles);
                        // apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.Id));
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
                        var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.Id.ToStringNullSafe()).ToList();
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
                                VehicleId = mappedVehicle.VehicleId,
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
                                //APIDailyPrice = vehicle.value.IntegralDailyPrice.ToFloatNullSafe(),
                                APIDailyPricePayNow = vehicle.value.Price.ToFloatNullSafe() / mappedVehicle.RentalDuration,
                                APIDailyPrice = vehicle.value.Price.ToFloatNullSafe() / mappedVehicle.RentalDuration,
                                APITotalPrice = vehicle.value.Price.ToFloatNullSafe(),
                                APIOneWayFee = vehicle.value.DropPrice.ToFloatNullSafe(),
                                DepositPrice = mappedVehicle.DepositPrice,
                                VendorMinimumDriverAge = mappedVehicle.VendorMinimumDriverAge ?? 0,
                                VendorMinimumDrivingLicenseAge = mappedVehicle.VendorMinimumDrivingLicenseAge ?? 0,
                                //ServiceCharge = mappedVehicle.ServiceCharge,
                                FuelType = mappedVehicle.FuelType,
                                TransmissionType = mappedVehicle.TransmissionType,
                                SippCode = mappedVehicle.SippCode,
                                //DepositCreditCardRequired = mappedVehicle.DepositCreditCardRequired,
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
                                FullCredit = mappedVehicle.FullCredit
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

                    return new ServiceResponseBase
                    {
                        Success = mappedVehicleList.Count > 0,
                        Data = mappedVehicleList
                    };
                }

            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = vendor.VendorName + " servisinden araç bulunamadı!"
            };

        }
        public static string VehicleID(string vehicle)
        {
            string vehicleids = vehicle;
            string[] vehicleid = vehicleids.Split(',');
            string[] ID = vehicleid[0].Split(":");

            return ID[1];
        }

        private Dictionary<string, object> GetVehicleListRequestParameters(Vendor vendor, string token, ResponseReservationStepsAdditionalInformation additionalInformation, GetVehiclesRequest getVehiclesRequest, CurrencyTypes baseVendorRequestCurrencyType)
        {
            return new Dictionary<string, object>()
            {
                {"text/xml; charset=utf-8",  GetVehicleRequestParameters(vendor,token,additionalInformation,getVehiclesRequest,baseVendorRequestCurrencyType) },
            };
        }
        public string DateTimeConverter(object Date, object Time)
        {
            string[] Dateparts = Date.ToString().Split(".");
            string day = Dateparts[0];
            string mounth = Dateparts[1];
            string year = Dateparts[2];
            string newtime = Time.ToString().Replace(":", "-");
            return (year + "-" + mounth + "-" + day + "T" + newtime);

        }



        private string GetVehicleRequestParameters(Vendor vendor, string token, ResponseReservationStepsAdditionalInformation additionalInformation, GetVehiclesRequest getVehiclesRequest, CurrencyTypes baseVendorRequestCurrencyType)
        {
            //!!! dil kısmını dinamik olarak çekmek için tekrardan incelenecek!!!!
            //     <Value>{additionalInformation.APICurrencyType}</Value>



            string VehicleListXmlBody = @$"<?xml version=""1.0"" encoding=""utf-8""?>
<soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
  <soap12:Body>
    <Search xmlns=""http://tempuri.org/"">
      <_Params>
        <ServiceParam>
          <Key>dlid</Key>
          <Value>{additionalInformation.APIPickupLocationCode}</Value>
        </ServiceParam>
        <ServiceParam>
          <Key>ddt</Key>
          <Value>{DateTimeConverter(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime)}</Value>
        </ServiceParam>
        <ServiceParam>
          <Key>rlid</Key>
          <Value>{additionalInformation.APIReturnLocationCode}</Value>
        </ServiceParam>
        <ServiceParam>
          <Key>rdt</Key>
          <Value>{DateTimeConverter(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime)}</Value>
        </ServiceParam>
        <ServiceParam>
          <Key>fc</Key>
          <Value>{CurrencyCode(baseVendorRequestCurrencyType.ToString())}</Value>
        </ServiceParam>
        <ServiceParam>
          <Key>aid</Key>
          <Value>{vendor.ApiKey}</Value>
        </ServiceParam>
        <ServiceParam>
          <Key>idta</Key>
          <Value>false</Value>
        </ServiceParam>
        <ServiceParam>
          <Key>irfa</Key>
          <Value>false</Value>
        </ServiceParam>
      </_Params>
      <_Lang>tr</_Lang>
      <_ServiceAuthenticationTicket>{token}</_ServiceAuthenticationTicket>
    </Search>
  </soap12:Body>
</soap12:Envelope>";

            return VehicleListXmlBody;
        }

        public string CurrencyCode(string currencyTypes)
        {
            string currencyCode;
            switch (currencyTypes)
            {
                case "TRY":
                    currencyCode = "TRY";
                    break;
                case "EURO":
                    currencyCode = "EUR";
                    break;
                case "EUR":
                    currencyCode = "EUR";
                    break;
                case "TL":
                    currencyCode = "TRY";
                    break;
                default:
                    currencyCode = "TRY";
                    break;
            }
            return currencyCode;
        }
    }
}
