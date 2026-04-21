using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Enuygun
{
    public class ReservationProvider : IReservationProvider
    {

        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }

        private readonly IConfigurationService configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            RestManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            AuthProvider = new AuthProvider(apiBaseUrl);
            this.configurationService = configurationService;
        }


        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var reservationCancelRequest = CreatereservationCancelRequest(localReservation);

            await configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(reservationCancelRequest),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest,


            });

            Serilog.Log.Error("{@EnUygunPostCancelReservationsRequestParameters}", reservationCancelRequest);

            var auth = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);

            if (auth?.Status == "OK")
            {
                var result = await RestManager.PostAsync<EnuygunRequest.Reservation.Cancel, EnuygunResponse.Reservation.Cancel>(
                        requestPath: "/api/v1/cancel",
                        headers: AuthProvider.CreateHeaderWithToken(auth.Data.Token),
                        entity: reservationCancelRequest,
                        brokerLogModel: new BrokerLogModel
                        {
                            LogKey = localReservation.ReservationNumber,
                            LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                        },
                         isReservationRequest: true
                    );

                Serilog.Log.Error("{@EnuygunPostCancelReservationsResponse}", result);

                if (result?.status == "OK")
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
                    Message = result?.userMessage
                };

            }


            return new ServiceResponseBase
            {
                Success = false,
                Message = "Enuygun rezervasyon iptal servisine ulaşılamadı (Message =" + auth?.UserMessage + ")"
            };
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {

            var reservationRequest = CreateReservationRequest(postReservationRequest, reservationToken);

            await configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(reservationRequest),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest,

            });

            Serilog.Log.Error("{@EnuygunPostReservationRequestParameters}", reservationRequest);

            var auth = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);

            if (auth?.Status == "OK")
            {

                var result = await RestManager.PostAsync<EnuygunRequest.Reservation.Root, EnuygunResponse.Reservation.Root>(
                    requestPath: "/api/v1/book",
                    headers: AuthProvider.CreateHeaderWithToken(auth.Data.Token),
                    entity: reservationRequest,
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationVendorAPIResponse,
                    },
                    isReservationRequest: true
                    );

                Serilog.Log.Error("{@EnuygunPostReservationResult}", result);

                localReservation.ReservationPostedToAPI = true;

                if (result?.status == "OK")
                {
                    localReservation.APIReservationSuccessfully = true;
                    localReservation.APIReservationNumber = result.data.orderId;
                    localReservation.APIVendorName = vendor.VendorName;

                    var officeInfo = reservationToken.APIReferenceCode3?.Split("|");

                    if (officeInfo.Length == 3)
                    {

                        //var vendorName = officeInfo[0];
                        var info2 = officeInfo[2].Trim().Split("~");
                        var info = officeInfo[1].Trim().Split("~");

                        //localReservation.APIVendorName = vendorName;

                        if (info.Length == 3)
                        {
                            localReservation.APIVendorPickupAddress = info[1];
                            localReservation.APIVendorPickupPhone = info[2];
                        }

                        if (info2.Length == 3)
                        {
                            localReservation.APIVendorReturnAddress = info2[1];
                            localReservation.APIVendorReturnPhone = info[2];
                        }


                    }

                    return new ServiceResponseBase
                    {
                        Success = true,
                        Data = localReservation,
                    };

                }

                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "Enuygun için rezervasyon gerçekleşmedi (Message = )" + result?.userMessage + ")"
                };

            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Enuygun rezervasyon servisine ulaşılamadı. (Message = )" + auth?.UserMessage + " )"
            };
        }

        private EnuygunRequest.Reservation.Cancel CreatereservationCancelRequest(Reservation localReservation)
        {
            var requestBody = new EnuygunRequest.Reservation.Cancel
            {
                requestId = localReservation.ReservationToken.APIReferenceCode,
                referenceId = localReservation.ReservationToken.APIReferenceCode2,

            };

            return requestBody;
        }

        private EnuygunRequest.Reservation.Root CreateReservationRequest(PostReservationRequest postReservationRequest, ReservationToken reservationToken)
        {

            string countryCode = "";
            string phoneNumber = "";

            if (postReservationRequest.CustomerTelephone.Contains(" "))
            {
                countryCode = postReservationRequest.CustomerTelephone.Split(" ")[0];
                phoneNumber = postReservationRequest.CustomerTelephone.Split(" ")[1];
            }
            else if (postReservationRequest.CustomerTelephone.StartsWith("+"))
            {
                countryCode = postReservationRequest.CustomerTelephone.Substring(0, 3);
                phoneNumber = postReservationRequest.CustomerTelephone.Substring(3);
            }
            else
            {
                countryCode = postReservationRequest.CustomerTelephone.Substring(0, 2);
                phoneNumber = postReservationRequest.CustomerTelephone.Substring(2);
            }

            //string rentalType = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? "0" : "1";
            //string onewayFeeType = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? "0" : postReservationRequest.OneWayFeePayToDelivery ? "0" : "1";
            //string extraType = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? "0" : postReservationRequest.ExtraPricePayToDelivery ? "0" : "1";


            var reservationRequest = new EnuygunRequest.Reservation.Root
            {
                requestId = reservationToken.APIReferenceCode,
                referenceId = reservationToken.APIReferenceCode2,
                contact = new EnuygunRequest.Reservation.Contact
                {
                    subscribe = false,
                    email = postReservationRequest.CustomerEmail,
                    phoneCountryCode = countryCode,
                    phoneNumber = phoneNumber,
                    firstName = postReservationRequest.CustomerName,
                    lastName = postReservationRequest.CustomerSurname,
                    birthDay = postReservationRequest.CustomerBirthDay.Replace(".", "-"),
                    //citizenNumber = postReservationRequest.CustomerPersonalNumber,
                    flightNumber = postReservationRequest.FlightNumberArrival,
                },
                invoice = new EnuygunRequest.Reservation.Invoice
                {
                    type = "1",
                    country = " ",
                    city = " ",
                    address = " ",
                    taxNumber = " ",
                    taxOffice = " ",
                    postCode = " ",
                    corporateName = " ",
                    isPersonalCompany = true,

                },

                salePrice = postReservationRequest.SpecialDailyPrice != -1 ?
                            Math.Round(postReservationRequest.SpecialDailyPrice * reservationToken.RentalDuration, 2).ToFloatNullSafe() : null

                //paymentType = rentalType + onewayFeeType + extraType
            };

            var customerPersonalNumber = postReservationRequest.CustomerPersonalNumber;


            if (!string.IsNullOrEmpty(customerPersonalNumber) && customerPersonalNumber.Length == 11 && customerPersonalNumber.All(char.IsDigit))
            {
                reservationRequest.contact.citizenNumber = customerPersonalNumber;
            }
            else if (!string.IsNullOrEmpty(customerPersonalNumber))
            {
                reservationRequest.contact.passportNumber = customerPersonalNumber;
            }

            reservationRequest.extraServices = new List<EnuygunRequest.Reservation.ExtraService>();

            if (postReservationRequest.PostReservationRequestV2?.Extras?.Count > 0)
            {
                foreach (var extra in postReservationRequest.PostReservationRequestV2.Extras)
                {
                    var extraService = new EnuygunRequest.Reservation.ExtraService
                    {
                        count = extra.Piece.ToString(),
                        slug = extra.ApiExtraCode
                    };
                    reservationRequest.extraServices.Add(extraService);
                }
            }

            if (!string.IsNullOrWhiteSpace(postReservationRequest.ExtraList))
            {
                var extraItems = postReservationRequest.ExtraList.Split("|", StringSplitOptions.RemoveEmptyEntries);

                foreach (var extraItem in extraItems)
                {
                    var item = extraItem.Trim().Split("~");
                    if (item.Length <= 3) continue;

                    var extra = new EnuygunRequest.Reservation.ExtraService
                    {
                        count = item[0],
                        slug = item[3]

                    };
                    reservationRequest.extraServices.Add(extra);
                }
            }
            return reservationRequest;
        }
    }
}
