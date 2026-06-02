using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Requests.RentGo;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Responses.RentGo;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kolaycar.Broker.Api.Providers.RentGo
{
    public class ReservationProvider : IReservationProvider
    {
        private readonly HttpManager _httpManager;
        private readonly IConfigurationService _configurationService;
        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            _httpManager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            _configurationService = configurationService;
        }
        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var reservationId = localReservation.APIReservationNumber;

            if (string.IsNullOrWhiteSpace(reservationId))
                return new ServiceResponseBase(localReservation, false, "RentGo rezervasyon id bulunamadi!");

            var cancelRequest = new RentGoCancelReservationRequest
            {
                Reason = postCancelReservationRequest.CancelNote
            };

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(cancelRequest),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            var headers = new Dictionary<string, object>
            {
                { "Content-Type", "application/json" },
                { "Authorization", $"Bearer {vendor.ApiClientId}" }
            };

            Serilog.Log.Error("{@RentGoPostCancelReservationRequestParameters}", new { ReservationId = reservationId, Request = cancelRequest });

            var response = await _httpManager.PostAsyncWithModelResult<RentGoCancelReservationRequest, string>(
                $"/reservation/cancel/{Uri.EscapeDataString(reservationId)}",
                cancelRequest,
                headers: headers,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                },
                isReservationRequest: true
            );

            Serilog.Log.Error("{@RentGoPostCancelReservationResponse}", response);

            localReservation.APIMessage = response?.ServiceMessage ?? response?.Message;

            if (response?.Success == true)
            {
                localReservation.APIReservationCancel = true;
                return new ServiceResponseBase(localReservation, true, "Iptal islemi basarili.");
            }

            return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, response, "iptal islemi basarisiz!");
        }
        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            var bookRequest = GetEntity(postReservationRequest, reservationToken, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(bookRequest),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            var headers = new Dictionary<string, object>
            {
                { "Content-Type", "application/json" },
                { "Authorization", $"Bearer {vendor.ApiClientId}" }
            };

            Serilog.Log.Error("{@RentGoPostReservationRequestParameters}", bookRequest);

            var response = await _httpManager.PostAsyncWithModelResult<RentGoReservationRequest, RentGoReservationResponse>(
                "/reservation",
                bookRequest,
                headers: headers,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true
            );

            Serilog.Log.Error("{@RentGoPostReservationResponse}", response);

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;
            localReservation.APIMessage = response?.ServiceMessage ?? response?.Message;

            if (response?.Success == true && response.Data != null)
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = response.Data.Pnr;

                return new ServiceResponseBase(localReservation, true, "Rezervasyon basarili." + (!string.IsNullOrWhiteSpace(response.Data.Pnr) ? " PNR: " + response.Data.Pnr : string.Empty));
            }

            return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, response, "RentGo rezervasyon hatasi!");
        }

        private RentGoReservationRequest GetEntity(PostReservationRequest postReservationRequest, ReservationToken reservationToken, Reservation localReservation)
        {
            var extras = GetAdditionalProducts(localReservation);
            var packages = GetAdditionalPackages(localReservation);

            return new RentGoReservationRequest
            {
                ResType = 1,
                ListId = reservationToken.APIReferenceCode,
                VersionId = reservationToken.VehicleCode,
                AdditionalProducts = extras,
                AdditionalPackages = packages,
                CustomerInfo = new RentGoCustomerInfo
                {
                    CustomerType = 1,
                    FirstName = postReservationRequest.CustomerName,
                    LastName = postReservationRequest.CustomerSurname,
                    Email = postReservationRequest.CustomerEmail,
                    MobileNumber = NormalizeMobileNumber(postReservationRequest.CustomerTelephone),
                    DialCode = "+90",
                    IsTurkish = true,
                    BirthDate = ToRentGoDate(postReservationRequest.CustomerBirthDay.ToDateTimeNullSafe().ToString("yyyy-MM-dd"))
                }
            };
        }

        private static List<string> GetAdditionalProducts(Reservation localReservation)
        {
            if (localReservation.ReservationExtras?.Any() == true)
                return localReservation.ReservationExtras.Where(e => e.ExtraType != AdditionalProductTypes.Insurance).Select(e => e.ExtraCode).ToList();

            return new List<string>();
        }
        private static List<string> GetAdditionalPackages(Reservation localReservation)
        {
            if (localReservation.ReservationExtras?.Any() == true)
                return localReservation.ReservationExtras.Where(e => e.ExtraType == AdditionalProductTypes.Insurance).Select(e => e.ExtraCode).ToList();

            return new List<string>();
        }

        private static string NormalizeMobileNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return phoneNumber;

            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

            if (digits.StartsWith("90") && digits.Length > 10)
                digits = digits.Substring(2);

            if (digits.StartsWith("0") && digits.Length > 10)
                digits = digits.Substring(1);

            return digits;
        }

        private static string ToRentGoDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (!DateTime.TryParse(value, out var date))
                return value;

            return date.ToString("yyyy-MM-ddT00:00:00.000Z");
        }
    }
}

