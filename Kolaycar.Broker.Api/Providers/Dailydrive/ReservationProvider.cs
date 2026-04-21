using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Response.Dailydrive;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Dailydrive
{
    public class ReservationProvider : IReservationProvider
    {
        RestManager RestManager { get; set; }
        private readonly IConfigurationService _configurationService;
        ILocationProvider locationProvider { get; set; }

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            RestManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            _configurationService = configurationService;
            locationProvider = new Dailydrive.LocationProvider(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var content = GetReservationCancelRequest(localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(content.ReadAsStringAsync().Result),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            Serilog.Log.Error("{@DailydrivePostCancelReservationRequest}", content);

            var result = await RestManager.PostAsyncHttpContent<ReservationCancelResponseBody>(
                entity: content,
                requestPath: "",
                headers: GetHeaderRequestParameters(vendor),
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                }, isReservationRequest: true);

            if (result?.SOAPENVEnvelope?.SOAPENVBody?.Ns2CancelReservationResponse?.Ns2OperationResult != null
                && result?.SOAPENVEnvelope?.SOAPENVBody?.Ns2CancelReservationResponse?.Ns2OperationResult?.Ns2Success == "true"
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
                Message = "Dailydrive servisinde iptal edilemedi"
            };
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);
            var content = GetReservationsRequestContent(additionalInformation, reservationToken, postReservationRequest, localReservation, apiPaidAmount, vendor, additionalInformation.Agency);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                //Content = JsonConvert.SerializeObject(content.ReadAsStringAsync().Result),
                Content = content.ReadAsStringAsync().Result,
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@DailydrivePostReservationRequestParameters}", content);

            var result = await RestManager.PostAsyncHttpContent<ReservationResponseBody>(
                entity: content,
                requestPath: "",
                headers: GetHeaderRequestParameters(vendor),
                brokerLogModel: new BrokerLogModel()
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@DailydrivePostReservationResult}", result);

            if (result?.SOAPENVEnvelope?.SOAPENVBody?.Ns2InsertReservationResponse?.Ns2OperationResult != null
                && result?.SOAPENVEnvelope?.SOAPENVBody?.Ns2InsertReservationResponse?.Ns2OperationResult?.Ns2Success == "true"
                )
            {
                //var paidAmount = localReservation.CouponDiscountAmount > 0 ? localReservation.PaidAmount + localReservation.APIPaidAmount : localReservation.PaidAmount;
                var paidAmount = localReservation.APIPaidAmount;
                var reservationPayServiceContent = GetReservationPayServiceContent(postReservationRequest,
                            result.SOAPENVEnvelope.SOAPENVBody.Ns2InsertReservationResponse.Ns2OperationResult.Ns2ResNo,
                            result.SOAPENVEnvelope.SOAPENVBody.Ns2InsertReservationResponse.Ns2OperationResult.Ns2ResCorpNo,
                            paidAmount,
                            vendor);

                Serilog.Log.Error("{@DailydrivePayServiceRequest", reservationPayServiceContent);

                var payServiceResult = await RestManager.PostAsyncHttpContent<SendBankTransactionResponse>(
                    entity: reservationPayServiceContent,
                    requestPath: "",
                    headers: GetHeaderRequestParameters(vendor),
                    serilog: true,
                    isReservationRequest: true
                    );

                Serilog.Log.Error("{@DailydrivePayServiceResponse}", payServiceResult);

                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.SOAPENVEnvelope.SOAPENVBody.Ns2InsertReservationResponse.Ns2OperationResult.Ns2ResNo + "-" + result.SOAPENVEnvelope.SOAPENVBody.Ns2InsertReservationResponse.Ns2OperationResult.Ns2ResCorpNo;

                var location = await locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);

                Serilog.Log.Error("{@DailydriveGetLocationsResponse}", location);

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

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Dailydrive servisine ulaşılamadı"
            };
        }

        private static StringContent GetReservationPayServiceContent(PostReservationRequest postReservationRequest, string resNo, string resCorpNo, float paidAmount, Vendor vendor)
        {
            var content = @$"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'
                            xmlns:dto='http://ws.naryaz.com/model/dto'>
                            <soapenv:Header/>
                                <soapenv:Body>
                                    <dto:SendBankTransactionRequest>
                                        <dto:bankTransaction>
                                            <dto:resNo>{resNo}</dto:resNo>
                                            <dto:resCorpNo>{resCorpNo}</dto:resCorpNo>
                                            <dto:bankTxNo>Success</dto:bankTxNo>
                                            <dto:bankTxError></dto:bankTxError>
                                            <dto:vposIntegrator></dto:vposIntegrator>
                                            <dto:orderId>{resNo}</dto:orderId>
                                            <dto:paidAmount>{paidAmount.ToString().Replace(',', '.')}</dto:paidAmount>
                                            <dto:paidAmountLocalCurrency>{paidAmount.ToString().Replace(',', '.')}</dto:paidAmountLocalCurrency>
                                            <dto:currency>{postReservationRequest.CurrencyCode}</dto:currency>
                                            <dto:exchangeRate>{postReservationRequest.CurrencyCode}</dto:exchangeRate>
                                            <dto:paymentTypeNo>{vendor.ApiClientId.Split('-')[0]}</dto:paymentTypeNo>
                                            <dto:cardNo></dto:cardNo>
                                            <dto:cardCvv></dto:cardCvv>
                                            <dto:cardHolder></dto:cardHolder>
                                            <dto:cardExpiryMonth></dto:cardExpiryMonth>
                                            <dto:cardExpiryYear></dto:cardExpiryYear>
                                        </dto:bankTransaction>
                                    </dto:SendBankTransactionRequest>
                                </soapenv:Body>
                            </soapenv:Envelope>".Trim();

            Serilog.Log.Error("{@DailydrivePayServiceRequestBody}", content);

            return new StringContent(content, Encoding.UTF8, "text/xml");
        }

        private Dictionary<string, object> GetHeaderRequestParameters(Vendor vendor)
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

        private static StringContent GetReservationsRequestContent(ResponseReservationStepsAdditionalInformation additionalInformation, ReservationToken reservationToken, PostReservationRequest postReservationRequest, Reservation localReservation, float apiPaidAmount, Vendor vendor, Agency agency)
        {
            DateTime birhday;
            string flightNumberAir = "";
            string fligtNumberFlghtCode = "";
            var tarifNos = vendor.SecretKey.Split('-');
            var idType = (System.Text.RegularExpressions.Regex.IsMatch(postReservationRequest.CustomerPersonalNumber, "[a-zA-Z]") || postReservationRequest.CustomerPersonalNumber.Length < 11) ? "P" : "I";

            bool isDate = DateTime.TryParseExact(postReservationRequest.CustomerBirthDay, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out birhday);

            string birthdateString = isDate ? birhday.ToString("yyyy-MM-dd") : "1995-01-01";

            //var totalRentalPrice = reservationToken.APITotalPrice.ToStringNullSafe().Replace(',', '.');
            var totalRentalPrice = reservationToken.APIReferenceCode2.Replace(',', '.');

            //var campaignNo = localReservation.SIPPCode.Split('-').Length > 1 ? localReservation.SIPPCode.Split('-')[1].ToString() : string.Empty;
            var campaignNo = reservationToken.APIReferenceCode3;

            var extraListXml = GetProductsXml(localReservation, vendor);
            string companyNameFirstPart, companyNameLastPart = "";
            var paymentTypeNo = vendor.ApiClientId.Contains('-') ? vendor.ApiClientId.Split('-') : "202-170".Split('-');

            if (postReservationRequest.CompanyTitle.Length > 65)
            {
                companyNameFirstPart = postReservationRequest.CompanyTitle.Substring(0, 60);
                companyNameLastPart = postReservationRequest.CompanyTitle.Substring(60);
            }
            else
            {
                companyNameFirstPart = postReservationRequest.CompanyTitle;
            }
            if (postReservationRequest.FlightNumberArrival != "" && postReservationRequest.FlightNumberArrival != null && postReservationRequest.FlightNumberArrival.Length > 3)
            {
                flightNumberAir = postReservationRequest.FlightNumberArrival.Substring(0, 2);
                fligtNumberFlghtCode = postReservationRequest.FlightNumberArrival.Substring(2);
            }

            var brokerAddress = GetAddressXml();// Obilet bilgileri mevcut
            //15.12.2023 Uçuş kodu ve numarasının ayrı gönderilmesi istendi. MCaliskan
            //<dto:airline>{postReservationRequest.FlightNumberDeparture}</dto:airline>
            //<dto:flightNo>{ postReservationRequest.FlightNumberArrival}</dto:flightNo>
            var content = new StringContent(@$"<soapenv:Envelope
			xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'
			xmlns:dto='http://ws.naryaz.com/model/dto'>
			<soapenv:Header/>
			<soapenv:Body>
				<dto:InsertReservationRequest>
					<dto:reservation>
						<dto:pickupDate>{additionalInformation.PickupDateTime.ToString("yyyy-MM-dd HH:mm")}</dto:pickupDate>
						<dto:returnDate>{additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd HH:mm")}</dto:returnDate>
						<dto:pickupLocNo>{additionalInformation.APIPickupLocationCode}</dto:pickupLocNo>
						<dto:returnLocNo>{additionalInformation.APIReturnLocationCode}</dto:returnLocNo>
						<dto:classNo>{reservationToken.VehicleCode.Split("-")[0]}</dto:classNo>
						<dto:typeNo>{reservationToken.VehicleId}</dto:typeNo>
						<dto:rentalDays>{additionalInformation.RentalDuration}</dto:rentalDays>
						<dto:extraHours>0</dto:extraHours>
						<dto:onewayPrice>{reservationToken.OneWayFee}</dto:onewayPrice>
						<dto:onewayCurrency>{reservationToken.BaseVendorRequestCurrencyType}</dto:onewayCurrency>
						<dto:totalRentalPrice>{totalRentalPrice}</dto:totalRentalPrice>
						<dto:rentalPriceCurrency>{reservationToken.BaseVendorRequestCurrencyType}</dto:rentalPriceCurrency>
						<dto:addresses>
							<dto:addressType>1</dto:addressType>
							<dto:billingAddress>false</dto:billingAddress>
							<dto:shippingAddress>true</dto:shippingAddress>
							<dto:name>{postReservationRequest.CustomerName}</dto:name>
							<dto:lastname>{postReservationRequest.CustomerSurname}</dto:lastname>
							<dto:corpName>{companyNameFirstPart}</dto:corpName>
							<dto:corpStyle>{companyNameLastPart}</dto:corpStyle>
							<dto:address>{postReservationRequest.CustomerAddress}</dto:address>
							<dto:cityNo></dto:cityNo>
							<dto:countryCode>TR</dto:countryCode>
							<dto:postalZone></dto:postalZone>
							<dto:taxOffice>{postReservationRequest.CompanyTaxOffice}</dto:taxOffice>
							<dto:taxId>{postReservationRequest.CompanyTaxNumber}</dto:taxId>
							<dto:phone1>00{postReservationRequest.CustomerTelephone}</dto:phone1>
							<dto:phone2>00{postReservationRequest.CustomerTelephone}</dto:phone2>
							<dto:fax/>
							<dto:mobPhone1>00{postReservationRequest.CustomerTelephone}</dto:mobPhone1>
							<dto:mobPhone2/>
							<dto:phoneHome/>
							<dto:email>{postReservationRequest.CustomerEmail}</dto:email>
							<dto:idType>{idType}</dto:idType>
							<dto:idNo>{postReservationRequest.CustomerPersonalNumber}</dto:idNo>
							<dto:idIssuePlace></dto:idIssuePlace>
							<dto:idIssueDate></dto:idIssueDate>
							<dto:birthPlace></dto:birthPlace>
							<dto:birthDate>{birthdateString}</dto:birthDate>
							<dto:licenseNo></dto:licenseNo>
							<dto:licenseIssuePlace></dto:licenseIssuePlace>
							<dto:licenseIssueDate></dto:licenseIssueDate>
						</dto:addresses>
                        <dto:addresses>
							{brokerAddress}
                        </dto:addresses>
						{extraListXml}
						<dto:landingFlight>
							<dto:airline>{flightNumberAir}</dto:airline>
							<dto:flightNo>{fligtNumberFlghtCode}</dto:flightNo>
							<dto:time></dto:time>
						</dto:landingFlight>
						<dto:takeoffFlight>
							<dto:airline/>
							<dto:flightNo/>
							<dto:time></dto:time>
						</dto:takeoffFlight>
						<dto:referenceNo>{localReservation.ReservationNumber}</dto:referenceNo>
						<dto:voucherNo></dto:voucherNo>
						<dto:deliveryPlace>{reservationToken.APIPickupLocationCode}</dto:deliveryPlace>
						<dto:dropPlace>{reservationToken.APIReturnLocationCode}</dto:dropPlace>
						<dto:pickupFromAirport>{reservationToken.IsAirport}</dto:pickupFromAirport>
						<dto:returnToAirport></dto:returnToAirport>
						<dto:resSourceNo>196</dto:resSourceNo>
						<dto:paymentTypeNo>{paymentTypeNo[0]}</dto:paymentTypeNo>
						<dto:tariffNo>{tarifNos[1]}</dto:tariffNo>
						<dto:campaignNo>{campaignNo}</dto:campaignNo>
						<dto:note/>{postReservationRequest.CustomerNote}
						<dto:brokerUserNo></dto:brokerUserNo>
					</dto:reservation>
				</dto:InsertReservationRequest>
			</soapenv:Body>
		</soapenv:Envelope>", Encoding.UTF8, "text/xml");


            return content;
        }

        private static string GetProductsXml(Reservation localReservation, Vendor vendor)
        {
            var products = "";

            if (localReservation.ReservationExtras.Count > 0)
            {
                foreach (var extra in localReservation.ReservationExtras)
                {
                    int extraType = (int)extra.ExtraRentalType + 1;
                    //              products += @$"
                    //<dto:products>
                    //	<dto:selected>true</dto:selected>
                    //	<dto:productNo>{extra.ExtraCode}</dto:productNo>
                    //	<dto:count>1</dto:count>
                    //	<dto:salesType>{extraType}</dto:salesType>
                    //	<dto:unitPrice>{extra.APIPrice / localReservation.RentalDuration}</dto:unitPrice>
                    //	<dto:totalPrice>{extra.APIPrice}</dto:totalPrice>
                    //	<dto:currency>{extra.CurrencyCode}</dto:currency> 
                    //	<dto:included>false</dto:included>
                    //	<dto:incremental>false</dto:incremental>
                    //	<dto:calculateTax>true</dto:calculateTax>
                    //</dto:products>";
                    products += @$"
						<dto:products>
							<dto:selected>true</dto:selected>
							<dto:productNo>{extra.ExtraCode}</dto:productNo>
							<dto:count>1</dto:count>
							<dto:salesType>{extraType}</dto:salesType>
							<dto:unitPrice>{extra.ApiPrice / localReservation.RentalDuration}</dto:unitPrice>
							<dto:totalPrice>{extra.ApiPrice}</dto:totalPrice>
							<dto:currency>{extra.CurrencyCode}</dto:currency> 
							<dto:included>false</dto:included>
							<dto:incremental>false</dto:incremental>
							<dto:calculateTax>true</dto:calculateTax>
						</dto:products>";
                }
            }

            return string.IsNullOrEmpty(products) ? string.Empty : products;
        }

        private static string GetAddressXml()
        {
            //Bu bilgiler sadece Obilet bilgileri. Yeni bir brokera Dailydrive eklenirse bunun için ayrı bir tanımlama yapılmalıdır.
            return @"
                    <dto:adNo>109406</dto:adNo> 
                    <dto:addressType>3</dto:addressType> 
                    <dto:billingAddress>true</dto:billingAddress> 
                    <dto:shippingAddress>false</dto:shippingAddress> 
                    <dto:name></dto:name> 
                    <dto:lastname></dto:lastname> 
                    <dto:corpName>OBİLET BİLİŞİM SİSTEMLERİ A.Ş.</dto:corpName> 
                    <dto:corpStyle></dto:corpStyle> 
                    <dto:address>SULTAN SELİM MAH., YUNUS EMRE CAD., NO:1/11, 34415, KAĞITHANE/İSTANBUL</dto:address> 
                    <dto:roomNo>11</dto:roomNo> 
                    <dto:buildingNo>1</dto:buildingNo> 
                    <dto:cityNo>597</dto:cityNo> 
                    <dto:countryCode>TR</dto:countryCode> 
                    <dto:postalZone>34415</dto:postalZone> 
                    <dto:taxOffice>MASLAK</dto:taxOffice> 
                    <dto:taxId>6320481434</dto:taxId> 
                    <dto:phone1>00902129630353</dto:phone1> 
                    <dto:phone2></dto:phone2> 
                    <dto:fax/> 
                    <dto:mobPhone1></dto:mobPhone1> 
                    <dto:mobPhone2/> 
                    <dto:phoneHome/> 
                    <dto:email></dto:email> 
                    <dto:idType>I</dto:idType> 
                    <dto:idNo></dto:idNo> 
                    <dto:idIssuePlace></dto:idIssuePlace> 
                    <dto:idIssueDate></dto:idIssueDate> 
                    <dto:birthPlace></dto:birthPlace> 
                    <dto:birthDate></dto:birthDate> 
                    <dto:licenseNo></dto:licenseNo> 
                    <dto:licenseIssuePlace></dto:licenseIssuePlace> 
                    <dto:licenseIssueDate></dto:licenseIssueDate>";
        }

        private static StringContent GetReservationCancelRequest(Reservation localReservation)
        {
            var resDetailsList = localReservation.APIReservationNumber?.Split('-');
            if (resDetailsList == null || resDetailsList.Length != 2)
                return new StringContent(string.Empty, Encoding.UTF8, "text/xml");

            return new StringContent(@$"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'
						 xmlns:dto='http://ws.naryaz.com/model/dto'>
						 <soapenv:Header/>
						 <soapenv:Body>
						 <dto:CancelReservationRequest>
						 <dto:resNo >{resDetailsList[0].Trim()}</dto:resNo>
						 <dto:resCorpNo >{resDetailsList[1].Trim()}</dto:resCorpNo>
						 </dto:CancelReservationRequest>
						 </soapenv:Body>
						</soapenv:Envelope>
						".Trim(), Encoding.UTF8, "text/xml");
        }
    }
}
