using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace KolayCAR.Broker.API.Providers.Otorento
{
    public class ReservationProvider : IReservationProvider
    {
        public readonly RestManager _restManager;
        public readonly AuthProvider _authProvider;
        ILocationProvider _locationprovider { get; set; }
        private IConfigurationService _configurationService { get; set; }

        public ReservationProvider(string apiBaseUrl, IConfigurationService configrationService)
        {
            _restManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            _authProvider = new AuthProvider(apiBaseUrl);
            _locationprovider = new LocationProvider(apiBaseUrl);
            _configurationService = configrationService;
        }
        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var gettoken = _authProvider.GetTicket(vendor, 1);
            var token = gettoken.Result.GetTicketResult.ToString();

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = PostReservationCancelRequestXMLBodyParameters(vendor, token, postCancelReservationRequest, localReservation),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            Serilog.Log.Error("{@OtorentoPostCancelReservationRequestParameters}", PostReservationtCancelRequestParameters(vendor, token, postCancelReservationRequest, localReservation));

            var result = await _restManager.PostAsyncXMLRestClient<OtorentoResponseBase.Root>(
                requestPath: "/BookingService.asmx",
                parameterType: RestSharp.ParameterType.RequestBody,
                entity: PostReservationtCancelRequestParameters(vendor, token, postCancelReservationRequest, localReservation),
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@OtorentoPostCancelReservationResult}", result);

            if (result != null &&
                result.soapEnvelope != null &&
                result.soapEnvelope.soapBody != null &&
                result.soapEnvelope.soapBody.CancelServiceResponse != null &&
                result.soapEnvelope.soapBody.CancelServiceResponse.CancelServiceResult != null &&
                result.soapEnvelope.soapBody.CancelServiceResponse.CancelServiceResult.IsSuccess == true)
            {
                localReservation.APIReservationCancel = result.soapEnvelope.soapBody.CancelServiceResponse.CancelServiceResult.IsSuccess;

                return new ServiceResponseBase
                {
                    Success = true,
                    Data = localReservation
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Otorento servisi rezervasyon iptali başarısız!",
            };
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            var gettoken = _authProvider.GetTicket(vendor, 1);

            if (gettoken == null)
            {
                Serilog.Log.Error("Otorento ticket servisinden herhangi bir veri alınamadı!");
                return new ServiceResponseBase
                {
                    Success = false,
                    Data = localReservation,
                    Message = "Otorento ticket servisinden yanıt alınamadı!"
                };
            }

            var token = gettoken.Result.GetTicketResult.ToString();
            var requestBody = PostReservationtRequestParameters(vendor, token, postReservationRequest, additionalInformation, reservationToken, localReservation);
            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(requestBody),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });
            var result = await _restManager.PostAsyncXMLRestClient<OtorentoResponseBase.Root>(
                requestPath: "/BookingService.asmx",
                parameterType: RestSharp.ParameterType.RequestBody,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                entity: requestBody,
                isReservationRequest: true
                );


            if (result?.soapEnvelope?.soapBody?.SaveResponse?.SaveResult != null)
            {

                var rezNo = result.soapEnvelope.soapBody.SaveResponse.SaveResult.No;

                string paymentRequest = PostReservationRequestXMLPaymentBody(vendor, token, postReservationRequest, additionalInformation, reservationToken, localReservation, rezNo);
                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = paymentRequest,
                    LogType = BrokerLogTypes.ReservationPaymentVendorAPIRequest
                });

                Serilog.Log.Error("{@OtorentoPostReservationRequestParameters}", paymentRequest);


                var paymentResult = await _restManager.PostAsyncXMLRestClient<OtorentoResponseBase.Root>(
                    requestPath: "/PaymentService.asmx",
                    parameterType: RestSharp.ParameterType.RequestBody,
                    entity: PostReservationtPaymentRequest(vendor, token, postReservationRequest, additionalInformation, reservationToken, localReservation, rezNo),
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationPaymentVendorAPIResponse
                    },
                    isReservationRequest: true
                    );

                Serilog.Log.Error("{@OtorentoPostReservationResult}", paymentResult);

                localReservation.ReservationPostedToAPI = true;

                if (paymentResult?.soapEnvelope?.soapBody?.StartChargeResponse?.StartChargeResult != null &&
                    paymentResult?.soapEnvelope?.soapBody?.StartChargeResponse?.StartChargeResult?.IsSuccess == true)
                {
                    localReservation.APIReservationSuccessfully = true;
                    localReservation.APIReservationNumber = result.soapEnvelope.soapBody.SaveResponse.SaveResult.No;

                    var location = await _locationprovider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                    Serilog.Log.Error("{@OtorentoGetLocationsResponse}", location);
                    var reservationLocation = new List<Domain.Models.Location>();

                    if (location.Success)
                    {
                        reservationLocation = location.Data as List<Domain.Models.Location>;
                        var reservationPickupLocation = reservationLocation.Where(x => x.LocationCode == additionalInformation.APIPickupLocationCode).FirstOrDefault();
                        var reservationReturnLocation = reservationLocation.Where(x => x.LocationCode == additionalInformation.APIReturnLocationCode).FirstOrDefault();

                        if (reservationPickupLocation != null)
                        {
                            localReservation.APIVendorPickupAddress = reservationPickupLocation.Address;
                            localReservation.APIVendorPickupPhone = reservationPickupLocation.PhoneNumber;
                        }

                        if (reservationReturnLocation != null)
                        {
                            localReservation.APIVendorReturnAddress = reservationReturnLocation.Address;
                            localReservation.APIVendorReturnPhone = reservationReturnLocation.PhoneNumber;
                        }
                    }

                    return new ServiceResponseBase
                    {
                        Success = true,
                        Data = localReservation
                    };
                }


                Serilog.Log.Error("Otorento servisinden ödeme gerçekleşmedi!");
                localReservation.APIMessage = "Otorento servisinden ödeme servisinden yanıt alınamadı!";

                return new ServiceResponseBase
                {
                    Success = false,
                    Data = localReservation,
                    Message = "Otorento servisinden ödeme servisinden yanıt alınamadı!"
                };
            }

            Serilog.Log.Error("Otorento servisinden herhangi bir veri alınamadı!");
            localReservation.APIMessage = "Otorento servisinden herhangi bir veri alınamadı!";

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Otorento servisinden herhangi bir veri alınamadı!"
            };

        }
        private Dictionary<string, object> PostReservationtRequestParameters(Vendor vendor, string token, PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, ReservationToken reservationToken, Reservation localReservation)
        {
            return new Dictionary<string, object>()
            {
                {"text/xml; charset=utf-8",  PostReservationRequestXMLBodyParameters(vendor,token,postReservationRequest,additionalInformation,reservationToken,localReservation) },
            };
        }

        private Dictionary<string, object> PostReservationtPaymentRequest(Vendor vendor, string token, PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, ReservationToken reservationToken, Reservation localReservation, string rezNo)
        {
            return new Dictionary<string, object>()
            {
                {"text/xml; charset=utf-8",  PostReservationRequestXMLPaymentBody(vendor,token,postReservationRequest,additionalInformation,reservationToken,localReservation, rezNo)  },
            };
        }
        private Dictionary<string, object> PostReservationtCancelRequestParameters(Vendor vendor, string token, PostCancelReservationRequest postCancelReservationRequest, Reservation localReservation)
        {
            return new Dictionary<string, object>()
            {
                {"text/xml; charset=utf-8",  PostReservationCancelRequestXMLBodyParameters(vendor,token, postCancelReservationRequest, localReservation) },
            };
        }
        private string PostReservationCancelRequestXMLBodyParameters(Vendor vendor, string token, PostCancelReservationRequest postCancelReservationRequest, Reservation localReservation)
        {
            //!!! dil kısmını dinamik olarak çekmek için tekrardan incelenecek!!!!


            string PostReservationXmlBody = @$"<?xml version=""1.0"" encoding=""utf-8""?>
<soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
  <soap12:Body>
    <CancelService xmlns=""http://tempuri.org/"">
      <_Params>
        <ServiceParam>
          <Key>bn</Key>
          <Value>{localReservation.APIReservationNumber}</Value>
        </ServiceParam>
      </_Params>
      <_CancelNote>{postCancelReservationRequest.CancelNote}</_CancelNote>
      <_ServiceAuthenticationTicket>{token}</_ServiceAuthenticationTicket>
    </CancelService>
  </soap12:Body>
</soap12:Envelope>";

            return PostReservationXmlBody;
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

        public string CustomerPhoneNoConverter(string CustomerPhoneNo)
        {
            string phone = CustomerPhoneNo.Replace("+", "");
            string NewCustomerPhone = "+" + phone.Replace(" ", "");
            return (NewCustomerPhone);
        }
        private string PostReservationRequestXMLBodyParameters(Vendor vendor, string token, PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, ReservationToken reservationToken, Reservation localReservation)
        {
            //!!! dil kısmını dinamik olarak çekmek için tekrardan incelenecek!!!!


            //Payment Type   O--> prepayment   E --> full payment  T --> will pay to supplier  A --> /paid to agency
            string pt = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery
                ? "T"
                : postReservationRequest.PaymentType != PaymentTypes.AdvancePayment
                ? "A" : "";

            string PostReservationXmlBody = @$"<?xml version=""1.0"" encoding=""utf-8""?>
<soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
  <soap12:Body>
    <Save xmlns=""http://tempuri.org/"">
         <_Params>
            <ServiceParam>
               <Key>vid</Key>
               <Value>{localReservation.VehicleCode}</Value>
            </ServiceParam>
             <ServiceParam>
              <Key>dlid</Key>
               <Value>{additionalInformation.APIPickupLocationCode}</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>ddt</Key>
               <Value>{DateTimeConverter(postReservationRequest.PickupDate, postReservationRequest.PickupTime)}</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>rlid</Key>
               <Value>{additionalInformation.APIReturnLocationCode}</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>rdt</Key>
               <Value>{DateTimeConverter(postReservationRequest.ReturnDate, postReservationRequest.ReturnTime)}</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>fc</Key>
               <Value>{CurrencyCode(postReservationRequest.CurrencyCode)}</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>idta</Key>
               <Value>false</Value>
            </ServiceParam>
              <ServiceParam>
               <Key>irfa</Key>
               <Value>false</Value>
            </ServiceParam>
              <ServiceParam>
               <Key>cn</Key>
               <Value>{postReservationRequest.CustomerNote}</Value>
            </ServiceParam>
              <ServiceParam>
                <Key>aid</Key>
                <Value>{vendor.ApiKey}</Value>
            </ServiceParam>
              <ServiceParam>
               <Key>fi</Key>
               <Value>{postReservationRequest.FlightNumberDeparture}</Value>
            </ServiceParam>
            <ServiceParam>
               <Key>exgps</Key>
               <Value>false</Value>
            </ServiceParam>
            <ServiceParam>
               <Key>exbs</Key>
               <Value>false</Value>
            </ServiceParam>
            <ServiceParam>
               <Key>exad</Key>
               <Value>false</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>exscdw</Key>
               <Value>false</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>dfn</Key>
               <Value>{postReservationRequest.CustomerName}</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>dln</Key>
               <Value>{postReservationRequest.CustomerSurname}</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>dc</Key>
               <Value>{CustomerPhoneNoConverter(postReservationRequest.CustomerTelephone)}</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>de</Key>
               <Value>{postReservationRequest.CustomerEmail}</Value>
            </ServiceParam>
            <ServiceParam>
               <Key>pt</Key>
               <Value>A</Value>
            </ServiceParam>
            <ServiceParam>
               <Key>pm</Key>
               <Value>U</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>btc</Key>
               <Value>false</Value>
            </ServiceParam>
         </_Params>
      <_Lang>tr-TR</_Lang>  
      <_ServiceAuthenticationTicket>{token}</_ServiceAuthenticationTicket>
    </Save>
  </soap12:Body>
</soap12:Envelope>";

            //_Lang kısmı tr den tr-TR olarak değiştirildi.

            return PostReservationXmlBody;
        }

        private string PostReservationRequestXMLPaymentBody(Vendor vendor, string token, PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, ReservationToken reservationToken, Reservation localReservation, string rezNo)
        {
            //!!! dil kısmını dinamik olarak çekmek için tekrardan incelenecek!!!!


            //Payment Type   O--> prepayment   E --> full payment  T --> will pay to supplier  A --> /paid to agency
            string pt = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery
                ? "T"
                : postReservationRequest.PaymentType != PaymentTypes.AdvancePayment
                ? "A" : "";

            string PostReservationXmlBody = @$"<?xml version=""1.0"" encoding=""utf-8""?>
<soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
  <soap12:Body>
    <StartCharge xmlns=""http://tempuri.org/"">
         <_Params>
            <ServiceParam>
               <Key>vid</Key>
               <Value>{localReservation.VehicleCode}</Value>
            </ServiceParam>
             <ServiceParam>
              <Key>dlid</Key>
               <Value>{additionalInformation.APIPickupLocationCode}</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>ddt</Key>
               <Value>{DateTimeConverter(postReservationRequest.PickupDate, postReservationRequest.PickupTime)}</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>rlid</Key>
               <Value>{additionalInformation.APIReturnLocationCode}</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>rdt</Key>
               <Value>{DateTimeConverter(postReservationRequest.ReturnDate, postReservationRequest.ReturnTime)}</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>fc</Key>
               <Value>{CurrencyCode(postReservationRequest.CurrencyCode)}</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>idta</Key>
               <Value>false</Value>
            </ServiceParam>
              <ServiceParam>
               <Key>irfa</Key>
               <Value>false</Value>
            </ServiceParam>
              <ServiceParam>
               <Key>cn</Key>
               <Value>{postReservationRequest.CustomerNote}</Value>
            </ServiceParam>
              <ServiceParam>
                <Key>aid</Key>
                <Value>{vendor.ApiKey}</Value>
            </ServiceParam>
              <ServiceParam>
               <Key>fi</Key>
               <Value>{postReservationRequest.FlightNumberDeparture}</Value>
            </ServiceParam>
            <ServiceParam>
               <Key>exgps</Key>
               <Value>false</Value>
            </ServiceParam>
            <ServiceParam>
               <Key>exbs</Key>
               <Value>false</Value>
            </ServiceParam>
            <ServiceParam>
               <Key>exad</Key>
               <Value>false</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>exscdw</Key>
               <Value>false</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>dfn</Key>
               <Value>{postReservationRequest.CustomerName}</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>dln</Key>
               <Value>{postReservationRequest.CustomerSurname}</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>dc</Key>
               <Value>{CustomerPhoneNoConverter(postReservationRequest.CustomerTelephone)}</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>de</Key>
               <Value>{postReservationRequest.CustomerEmail}</Value>
            </ServiceParam>
            <ServiceParam>
               <Key>pt</Key>
               <Value>A</Value>
            </ServiceParam>
            <ServiceParam>
               <Key>pm</Key>
               <Value>U</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>btc</Key>
               <Value>false</Value>
            </ServiceParam>
             <ServiceParam>
               <Key>bn</Key>
               <Value>{rezNo}</Value>
            </ServiceParam>

         </_Params>
      <_Lang>tr-TR</_Lang>  
      <_ServiceAuthenticationTicket>{token}</_ServiceAuthenticationTicket>
    </StartCharge>
  </soap12:Body>
</soap12:Envelope>";

            //_Lang kısmı tr den tr-TR olarak değiştirildi.

            return PostReservationXmlBody;
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
