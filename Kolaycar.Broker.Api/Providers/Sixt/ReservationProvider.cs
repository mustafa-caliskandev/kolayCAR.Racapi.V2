using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Sixt.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Sixt
{
    public class ReservationProvider : IReservationProvider
    {
        private readonly IConfigurationService _configurationService;
        ILocationProvider locationProvider { get; set; }
        RestManager RestManager { get; set; }
        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            RestManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            _configurationService = configurationService;
            locationProvider = new LocationProvider(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var reservationCancelRequestParameters = CreateReservationCancelRequestParameters(vendor, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(reservationCancelRequestParameters),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            Serilog.Log.Error("{@SixtPostCancelReservationsRequestParameters}", reservationCancelRequestParameters);

            var result = await RestManager.GetXmlAsync<ReservationCancelResponseBase.Root>(
                requestPath: "",
                parameters: reservationCancelRequestParameters,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@SixtPostCancelReservationResult}", result);

            if (result is { SIXTTURKEYWEBSERVICES: not null }
                && result.SIXTTURKEYWEBSERVICES.PROCESSSTATUS != null
                && result.SIXTTURKEYWEBSERVICES.PROCESSSTATUS.CODE == "0"
                )
            {
                localReservation.APIReservationCancel = true;

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
                Message = "Sixt servisinden rezervasyon iptal edilemedi!"
            };
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);

            var reservationRequestParameters = CreateReservationSaveRequestParameters(reservationToken, additionalInformation, vendor, postReservationRequest, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(reservationRequestParameters),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@SixtPostReservationRequestParameters}", reservationRequestParameters);

            var result = await RestManager.GetXmlAsync<ReservationSaveResponseBase>(
                requestPath: "",
                parameters: reservationRequestParameters,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@SixtPostReservationReservationResult}", result);

            localReservation.ReservationPostedToAPI = true;

            if (result != null
                && result.SIXTTURKEYWEBSERVICES != null
                && result.SIXTTURKEYWEBSERVICES.PROCESSSTATUS != null
                && result.SIXTTURKEYWEBSERVICES.PROCESSSTATUS.CODE == "0"
                && result.SIXTTURKEYWEBSERVICES.RESERVATIONSTATUS != null
                && !string.IsNullOrEmpty(result.SIXTTURKEYWEBSERVICES.RESERVATIONSTATUS.RESERVATIONCODE)
                )
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.SIXTTURKEYWEBSERVICES.RESERVATIONSTATUS.RESERVATIONCODE;

                var pickupLocation = await locationProvider.GetLocationDetail(vendor, (int)localReservation.LanguageType + 1, reservationToken.APIPickupLocationCode);

                Serilog.Log.Error("{@SixtGetPickupLocationsResponse}", pickupLocation.Data);

                if (pickupLocation.Success)
                {
                    if (pickupLocation.Data != null)
                    {
                        var reservationLocation = pickupLocation.Data as Domain.Models.Location;

                        localReservation.APIVendorPickupAddress = reservationLocation.Address;
                        localReservation.APIVendorPickupPhone = reservationLocation.PhoneNumber;
                    }
                }

                var returnLocation = await locationProvider.GetLocationDetail(vendor, (int)localReservation.LanguageType + 1, reservationToken.APIPickupLocationCode);
                Serilog.Log.Error("{@SixtGetReturnLocationsResponse}", returnLocation.Data);

                if (returnLocation.Success)
                {
                    if (returnLocation.Data != null)
                    {
                        var reservationLocation = returnLocation.Data as Domain.Models.Location;

                        localReservation.APIVendorReturnAddress = reservationLocation.Address;
                        localReservation.APIVendorReturnPhone = reservationLocation.PhoneNumber;
                    }
                }

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
                Message = "Sixt servisinde rezervasyon oluşturulamadı!"
            };
        }

        private static Dictionary<string, object> CreateReservationCancelRequestParameters(Vendor vendor, Reservation localReservation)
        {
            return new Dictionary<string, object>
            {
                { "aid", vendor.ApiKey },
                { "p", vendor.ApiPassword },
                { "m", "doCancel" },
                { "o", "cancel" },
                { "rez", localReservation.APIReservationNumber }
            };
        }


        private static Dictionary<string, object> CreateReservationSaveRequestParameters(ReservationToken reservationToken, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, PostReservationRequest postReservationRequest, Reservation localReservation)
        {
            var apiPickupLocationCodes = additionalInformation.APIPickupLocationCode.Split('-');
            var apiReturnLocationCodes = additionalInformation.APIReturnLocationCode.Split('-');
            var customerBirthDay = DateTime.ParseExact(postReservationRequest.CustomerBirthDay ?? "01.01.1999", "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("dd.MM.yyyy");
            var city = !string.IsNullOrEmpty(postReservationRequest.City) ? postReservationRequest.City : "İstanbul";
            var country = !string.IsNullOrEmpty(postReservationRequest.Country) ? postReservationRequest.Country : "Türkiye";
            var address = !string.IsNullOrEmpty(postReservationRequest.CustomerAddress) ? postReservationRequest.CustomerAddress : "Bostancı / " + city + " " + country;

            //var payment = @"$example_data_array=array('card_pan'=>'kart no','cvv'=>'cvv bilgisi','card_name'=>'kart uzerindenki musteri adi','expire_date'=>'mmYY','customer_user_agent'=>$_SERVER['HTTP_USER_AGENT'],’customer_ip’=>$_SERVER[‘REMOTE_ADDR’]);$example_data = base64_encode(json_encode($example_data_array)); ";

            var additionalProductsXml = GetXmlAdditionalProducts(localReservation.ReservationExtras);
            var xmlString = @$"<RESERVATION><TOKEN>{reservationToken.APIReferenceCode}</TOKEN><AGENTINFO><SMSINFO><STATUS>1</STATUS><GSM>{additionalInformation.Agency.PhoneNumber}</GSM></SMSINFO><EMAILINFO><STATUS>1</STATUS><EMAIL>{additionalInformation.Agency.Email}</EMAIL></EMAILINFO></AGENTINFO><CUSTOMER><DEGREE>Bay</DEGREE><NAME>{postReservationRequest.CustomerName}</NAME><SURNAME>{postReservationRequest.CustomerSurname}</SURNAME><BIRTHDATE>{customerBirthDay}</BIRTHDATE><TCNO>{postReservationRequest.CustomerPersonalNumber}</TCNO><EMAIL>{postReservationRequest.CustomerEmail}</EMAIL><GSM>{postReservationRequest.CustomerTelephone}</GSM></CUSTOMER><ADDRESSINFO><ADDRESS>{address}</ADDRESS><POSTALCODE>34744</POSTALCODE><CITY>{city}</CITY><COUNTRY>{country}</COUNTRY></ADDRESSINFO><FLIGHTINFO><NUMBER>{postReservationRequest.FlightNumberArrival}</NUMBER><AIRPORT>{additionalInformation.PickupLocationName}</AIRPORT></FLIGHTINFO><MEMBERNUMBER>{vendor.ApiClientId}</MEMBERNUMBER><CUSTOMERMESSAGE>{postReservationRequest.CustomerNote}</CUSTOMERMESSAGE><CURRENCY>{postReservationRequest.CurrencyCode}</CURRENCY><VEHICLE><GROUP>{reservationToken.VehicleCode}</GROUP><DAILYPRICE>{reservationToken.DailyPrice.ToString().Replace(',', '.')}</DAILYPRICE><AVAILABILITY><CODE>1</CODE></AVAILABILITY></VEHICLE><PAYMENTOPTIONS><CODE>121</CODE></PAYMENTOPTIONS><PICKUPSTATION><ID>{apiPickupLocationCodes[1]}</ID><CODE>{apiPickupLocationCodes[0]}</CODE><DATE>{additionalInformation.PickupDateTime.ToString("dd.MM.yyyy")}</DATE><HOUR>{additionalInformation.PickupDateTime.ToString("HH")}</HOUR><MINUTE>{additionalInformation.PickupDateTime.ToString("mm")}</MINUTE></PICKUPSTATION><RETURNSTATION><ID>{apiReturnLocationCodes[1]}</ID><CODE>{apiReturnLocationCodes[0]}</CODE><DATE>{additionalInformation.ReturnDateTime.ToString("dd.MM.yyyy")}</DATE><HOUR>{additionalInformation.ReturnDateTime.ToString("HH")}</HOUR><MINUTE>{additionalInformation.ReturnDateTime.ToString("mm")}</MINUTE></RETURNSTATION><INCLUDED>{reservationToken.APIReferenceCode2}</INCLUDED>{additionalProductsXml}</RESERVATION>";

            xmlString = xmlString.Replace("\n", "");
            xmlString = xmlString.Replace("\t", "");

            return new Dictionary<string, object>
            {
                { "aid", vendor.ApiKey },
                { "p", vendor.ApiPassword },
                { "m", "doReservation" },
                { "o", "reservation" },
                //{ "payment", payment },
                { "xml", xmlString }
            };
        }
        //<OTHERCOMPANYINFO><DEGREE>deneme</DEGREE><ADDRESS>İstanbul</ADDRESS><TAXOFFICE>İstanbul</TAXOFFICE><TAXNUMBER>000000</TAXNUMBER><CITY>İstanbul</CITY><COUNTRY>tr</COUNTRY><PHONE>5300000000</PHONE></OTHERCOMPANYINFO>
        private static string GetXmlAdditionalProducts(List<ReservationExtra> extras)
        {
            var xmlInsurances = "";
            var xmlExtras = "";

            foreach (var item in extras)
            {
                var rentalType = item.ExtraRentalType == ExtraRentalTypes.Daily ? 0 : 1;

                if (item.ExtraType == AdditionalProductTypes.Insurance)
                {
                    xmlInsurances += $@"<INSURANCE>
                             <CODE>{item.ExtraCode}</CODE>
                             <CALCULATE>{rentalType}</CALCULATE>
                             <PRICE>{item.Price.ToFloatNullSafe()}</PRICE>
                         </INSURANCE>";
                }
                if (item.ExtraType == AdditionalProductTypes.Extra)
                {
                    xmlExtras += $@"<EXTRA>
                             <CODE>{item.ExtraCode}</CODE>
                             <CALCULATE>{rentalType}</CALCULATE>
                             <PRICE>{item.Price.ToFloatNullSafe()}</PRICE>
                         </EXTRA>";
                }
            }

            return $@"<INSURANCES>
                         {xmlInsurances}
                     </INSURANCES>
                     <EXTRAS>
                         {xmlExtras}
                     </EXTRAS>";
        }

        #region İptal edildi. Kontrollerden sonra silinecek
        private static Dictionary<string, object> CreateReservationSaveRequestParameters2(ReservationToken reservationToken, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, PostReservationRequest postReservationRequest, Reservation localReservation)
        {
            var apiPickupLocationCodes = additionalInformation.APIPickupLocationCode.Split('-');
            var apiReturnLocationCodes = additionalInformation.APIReturnLocationCode.Split('-');

            var additionalProductsXml = GetXmlAdditionalProducts(localReservation.ReservationExtras);

            var xmlString = @$"
                 <RESERVATION>
                     <TOKEN>{reservationToken.APIReferenceCode}</TOKEN>
                     <AGENTINFO>
                         <SMSINFO>
                             <STATUS>{(additionalInformation.Agency.PhoneNumber.Length >= 10).ToIntNullSafe().ToString()}</STATUS>
                             <GSM>{additionalInformation.Agency.PhoneNumber}</GSM>
                         </SMSINFO>
                         <EMAILINFO>
                             <STATUS>{(!string.IsNullOrEmpty(additionalInformation.Agency.Email)).ToIntNullSafe().ToString()}</STATUS>
                             <EMAIL>{additionalInformation.Agency.Email}</EMAIL>
                         </EMAILINFO>
                     </AGENTINFO>
                     <CUSTOMER>
                         <DEGREE>xx</DEGREE>
                         <NAME>{postReservationRequest.CustomerName}</NAME>
                         <SURNAME>{postReservationRequest.CustomerSurname}</SURNAME>
                         <BIRTHDATE>{postReservationRequest.CustomerBirthDay.ToStringNullSafe()}</BIRTHDATE>
                         <TCNO>{postReservationRequest.CustomerPersonalNumber}</TCNO>
                         <EMAIL>>{postReservationRequest.CustomerEmail}</EMAIL>
                         <GSM>{postReservationRequest.CustomerTelephone}</GSM>
                     </CUSTOMER>
                     <ADDRESSINFO>
                         <ADDRESS>{postReservationRequest.CustomerAddress}</ADDRESS>
                         <POSTALCODE>{postReservationRequest.District}</POSTALCODE>
                         <CITY>{postReservationRequest.City}</CITY>
                         <COUNTRY>{postReservationRequest.Country}</COUNTRY>
                     </ADDRESSINFO>
                     <OTHERCOMPANYINFO>
                         <DEGREE>xx</DEGREE>
                         <ADDRESS>{postReservationRequest.CustomerAddress}</ADDRESS>
                         <TAXOFFICE>{postReservationRequest.CompanyTaxOffice}</TAXOFFICE>
                         <TAXNUMBER>{postReservationRequest.CompanyTaxNumber}</TAXNUMBER>
                         <CITY>{postReservationRequest.City}</CITY>
                         <COUNTRY>>{postReservationRequest.Country}</COUNTRY>
                         <PHONE>{postReservationRequest.CustomerTelephone}</PHONE>
                     </OTHERCOMPANYINFO>
                     <FLIGHTINFO>
                         <NUMBER>{postReservationRequest.FlightNumberArrival}</NUMBER>
                         <AIRPORT>{additionalInformation.PickupLocationName}</AIRPORT>
                     </FLIGHTINFO>
                     <MEMBERNUMBER>{vendor.ApiClientId}</MEMBERNUMBER>
                     <CUSTOMERMESSAGE>{postReservationRequest.CustomerNote}</CUSTOMERMESSAGE>
                     <CURRENCY>{postReservationRequest.CurrencyCode}</CURRENCY>
                     <VEHICLE>
                         <GROUP>{reservationToken.VehicleCode}</GROUP>
                         <DAILYPRICE>{reservationToken.DailyPrice}</DAILYPRICE>
                         <AVAILABILITY>
                            <CODE>0</CODE>
                         </AVAILABILITY>
                     </VEHICLE>
                     <PAYMENTOPTIONS>
                        <CODE>121</CODE>
                     </PAYMENTOPTIONS>
                     <PICKUPSTATION>
                         <ID>{apiPickupLocationCodes[1]}</ID>
                         <CODE>{apiPickupLocationCodes[0]}</CODE>
                         <DATE>{additionalInformation.PickupDateTime.ToString("dd.MM.yyyy")}</DATE>
                         <HOUR>{additionalInformation.PickupDateTime.ToString("HH")}</HOUR>
                         <MINUTE>{additionalInformation.PickupDateTime.ToString("mm")}</MINUTE>
                     </PICKUPSTATION>
                     <RETURNSTATION>
                         <ID>{apiReturnLocationCodes[1]}</ID>
                         <CODE>{apiReturnLocationCodes[0]}</CODE>
                         <DATE>{additionalInformation.PickupDateTime.ToString("dd.MM.yyyy")}</DATE>
                         <HOUR>{additionalInformation.PickupDateTime.ToString("HH")}</HOUR>
                         <MINUTE>{additionalInformation.PickupDateTime.ToString("mm")}</MINUTE>
                     </RETURNSTATION>
                     <INCLUDED>
                         {reservationToken.APIReferenceCode2}
                     </INCLUDED>
                     {additionalProductsXml}
                  </ RESERVATION>";

            xmlString = xmlString.Replace("\n", "");
            xmlString = xmlString.Replace("\t", "");

            return new Dictionary<string, object>
            {
                { "aid", vendor.ApiKey },
                { "p", vendor.ApiPassword },
                { "m", "doReservation" },
                { "o", "reservation" },
                { "xml", xmlString }
            };
        }
        #endregion
    }
}
