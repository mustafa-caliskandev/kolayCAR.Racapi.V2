using KolayCAR.Broker.API.Mappers.Reservaway;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Requests.Reservaway;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Response.Reservaway;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Reservaway
{
    public class ReservationProvider : IReservationProvider
    {
        private readonly HttpManager _httpManager;
        private readonly IConfigurationService _configurationService;
        private readonly VehicleProvider _vehicleProvider;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            _httpManager = new HttpManager(ReservawayMapperHelper.NormalizeBaseUrl(apiBaseUrl));
            _configurationService = configurationService;
            _vehicleProvider = new VehicleProvider(new Vendor { APIBaseUrl = apiBaseUrl }, false);
        }

        public async Task<ServiceResponseBase> PostReservation(
            PostReservationRequest postReservationRequest,
            Vendor vendor,
            ResponseReservationStepsAdditionalInformation additionalInformation,
            string reservationNumber,
            ReservationToken reservationToken,
            List<ExchangeRates> exchangeRates,
            Reservation localReservation,
            List<Extra> apiExtras)
        {
            if (reservationToken == null)
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} rezervasyon istegi icin reservation token bulunamadi.");

            var detailResult = await _vehicleProvider.GetVehicleDetail(vendor, reservationToken, additionalInformation);
            if (!detailResult.Success)
                return new ServiceResponseBase(localReservation, false, detailResult.Message, serviceMessage: detailResult.ServiceMessage, serviceCode: detailResult.ServiceCode);

            var detail = detailResult.Data as ReservawayVehicleDetailResult;
            if (detail?.ApiVehicle == null)
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} rezervasyon icin arac detayi alinamadi.");

            var planReference = EnsurePlanReference(detail.ApiVehicle, detail.PlanReference);
            if (!ReservawayMapperHelper.IsSellablePlan(planReference))
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} sadece BSC pakette PPFC/PPNC odeme tipleri icin rezervasyon kabul eder.");

            var ratePrice = VehicleMapper.GetSelectedRatePrice(detail.ApiVehicle, planReference);
            if (ratePrice == null)
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} secilen PPFC/PPNC odeme plani artik kullanilabilir degil.");

            var apiReservationToken = ratePrice?.reservation_token.ToStringNullSafe();

            if (string.IsNullOrWhiteSpace(apiReservationToken))
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} rezervasyon tokeni alinamadi.");

            var request = postReservationRequest.MapToCreateCustomerRequest(reservationToken, detail.ApiVehicle, planReference, apiReservationToken, apiExtras);
            if (request.country_id <= 0 || request.payment_type_id <= 0 || request.product_type_id <= 0 || request.vehicle_id <= 0)
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} rezervasyon zorunlu alanlari eksik.");

            try
            {
                await WriteStepRequestLog(localReservation, "create-customer", request);

                var result = await _httpManager.PostAsyncWithModelResult<ReservawayCreateCustomerRequest, ReservawayCustomerResponse>(
                    requestPath: ReservawayRequestHelper.CreateCustomerPath,
                    entity: request,
                    headers: ReservawayRequestHelper.CreateHeaders(reservationToken.APIReferenceCode3, reservationToken.APIReferenceCode),
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationVendorAPIResponse
                    },
                    isReservationRequest: true);

                if (result?.Success == true && result.Data != null)
                {
                    localReservation.ReservationPostedToAPI = true;
                    localReservation.APIReservationSuccessfully = true;
                    localReservation.APIVendorName = vendor.VendorName;
                    localReservation.APIReservationNumber = ResolveReservationNumber(result.Data, reservationNumber);
                    localReservation.APIReferenceCode = apiReservationToken;
                    localReservation.APIReferenceCode2 = reservationToken.APIReferenceCode;
                    localReservation.APIReferenceCode3 = reservationToken.APIReferenceCode3;

                    return new ServiceResponseBase(localReservation, true);
                }

                return CreateVendorErrorResponse(localReservation, vendor, result, "rezervasyon servisi basarisiz.");
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error("{@ReservawayPostReservationError}", $"{vendor.VendorName} - {localReservation?.ReservationNumber} - {ex.ToJson()}");
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} rezervasyon servisinden herhangi bir veri alinamadi.");
            }
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var reservationNo = localReservation?.APIReservationNumber.ToStringNullSafe().TrimNullSafe();
            if (string.IsNullOrWhiteSpace(reservationNo))
                reservationNo = postCancelReservationRequest?.ReservationNumber.ToStringNullSafe().TrimNullSafe();

            if (string.IsNullOrWhiteSpace(reservationNo))
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} iptal istegi icin rezervasyon numarasi bulunamadi.");

            var visitorSessionId = !string.IsNullOrWhiteSpace(localReservation?.APIReferenceCode3)
                ? localReservation.APIReferenceCode3
                : System.Guid.NewGuid().ToString("N");

            var parameters = new Dictionary<string, object>
            {
                ["reference_id"] = reservationNo
            };

            var customerEmail = !string.IsNullOrWhiteSpace(localReservation?.CustomerMail)
                ? localReservation.CustomerMail
                : postCancelReservationRequest?.CustomerEmail;

            if (!string.IsNullOrWhiteSpace(customerEmail))
                parameters["email"] = customerEmail;

            if (!string.IsNullOrWhiteSpace(postCancelReservationRequest?.CancelNote))
                parameters["reason"] = postCancelReservationRequest.CancelNote;

            try
            {
                await WriteStepRequestLog(localReservation, "cancel", parameters, BrokerLogTypes.ReservationCancelVendorAPIRequest);

                var result = await _httpManager.GetAsync2<ReservawayCustomerResponse>(
                    requestPath: ReservawayRequestHelper.CancelPath,
                    parameters: parameters,
                    headers: ReservawayRequestHelper.CreateHeaders(visitorSessionId, localReservation?.APIReferenceCode2),
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation?.ReservationNumber ?? reservationNo,
                        LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                    },
                    isReservationRequest: true);

                if (result?.Success == true)
                {
                    if (localReservation != null)
                        localReservation.APIReservationCancel = true;

                    return new ServiceResponseBase(localReservation, true, "Iptal islemi basarili.");
                }

                return CreateVendorErrorResponse(localReservation, vendor, result, "rezervasyon iptal servisi basarisiz.");
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error("{@ReservawayPostCancelReservationError}", $"{vendor.VendorName} - {localReservation?.ReservationNumber ?? reservationNo} - {ex.ToJson()}");
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} rezervasyon iptal servisinden herhangi bir veri alinamadi.");
            }
        }

        private static ReservawayPlanReference EnsurePlanReference(ReservawayVehicleDetail vehicle, ReservawayPlanReference planReference)
        {
            if (planReference != null && planReference.ProductTypeId > 0 && planReference.PaymentTypeId > 0)
                return planReference;

            var planDefinition = ReservawayMapperHelper.GetBasePlanDefinition(vehicle?.prices);
            return ReservawayMapperHelper.ParsePlanReference(ReservawayMapperHelper.CreatePlanReference(planDefinition));
        }

        private async Task WriteStepRequestLog(Reservation localReservation, string stepName, object request, BrokerLogTypes logType = BrokerLogTypes.ReservationVendorAPIRequest)
        {
            if (_configurationService == null || localReservation == null)
                return;

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = new { Step = stepName, Request = request }.ToJson(),
                LogType = logType
            });
        }

        private static string ResolveReservationNumber(ReservawayCustomerResponse response, string fallbackReservationNumber)
        {
            if (!string.IsNullOrWhiteSpace(response?.customer?.order_id))
                return response.customer.order_id;

            var referenceId = response?.order?.data?.SelectToken("reference_id")?.ToString()
                ?? response?.order?.data?.SelectToken("order_id")?.ToString()
                ?? response?.order?.data?.SelectToken("id")?.ToString();

            return !string.IsNullOrWhiteSpace(referenceId) ? referenceId : fallbackReservationNumber;
        }

        private static ServiceResponseBase CreateVendorErrorResponse<T>(Reservation localReservation, Vendor vendor, HttpResult<T> result, string fallbackMessage)
            where T : ReservawayResponseBase
        {
            var supplierMessage = result?.Data?.message ?? result?.ServiceMessage ?? result?.Message;

            if (!string.IsNullOrWhiteSpace(supplierMessage) && localReservation != null)
                localReservation.APIMessage = supplierMessage;

            return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} {fallbackMessage}", serviceMessage: supplierMessage, serviceCode: result != null ? ((int)result.HttpStatusCode).ToString() : string.Empty);
        }
    }
}
