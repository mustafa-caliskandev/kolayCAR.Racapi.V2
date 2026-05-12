using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        #region Definations
        private readonly IReservationService _reservationService;
        private readonly IReservationStepsService _reservationStepsService;
        private readonly IExtraService _extraService;
        private readonly IPaymentService _paymentService;
        private readonly IAgencyService _agencyService;
        private readonly IVehicleService _vehicleService;
        private readonly IVendorService _vendorService;
        private readonly UserRoles _userRole;
        private readonly IInvoiceService _invoiceService;
        private readonly IConfigurationService _configurationService;
        private readonly IResTokenService _resTokenService;
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ICurrentAccountService _currentAccountService;
        private readonly NetResysService _netResysService;

        #endregion

        #region Constructor
        public ReservationsController(
            IReservationService reservationService,
            IExtraService extraService,
            IPaymentService paymentService,
            IAgencyService agencyService,
            IReservationStepsService reservationStepsService,
            IVehicleService vehicleService,
            IVendorService vendorService,
            IInvoiceService invoiceService,
            IConfigurationService configurationService,
            IMemoryCache memoryCache,
            IHttpContextAccessor httpContextAccessor,
            NetResysService netResysService,
            IResTokenService resTokenService,
            IConfiguration configuration,
            ICurrentAccountService currentAccountService
            )
        {
            _reservationService = reservationService;
            _extraService = extraService;
            _paymentService = paymentService;
            _agencyService = agencyService;
            _userRole = _agencyService.GetCurrentUserRole();
            _reservationStepsService = reservationStepsService;
            _vendorService = vendorService;
            _vehicleService = vehicleService;
            _invoiceService = invoiceService;
            _configurationService = configurationService;
            _memoryCache = memoryCache;
            _httpContextAccessor = httpContextAccessor;
            _netResysService = netResysService;
            _resTokenService = resTokenService;
            _configuration = configuration;
            _currentAccountService = currentAccountService;
        }
        #endregion

        #region Get Reservation
        [HttpGet]
        public async Task<ActionResult<HttpResult<object>>> Get(
            string reservationNumber,
            string customerEmail,
            LanguageTypes? languageType = null)
        {
            if (string.IsNullOrWhiteSpace(reservationNumber))
                return BadRequest(HttpResult<object>.Result(
               data: null,
               httpResultType: HttpStatusCode.BadRequest,
               success: false,
               message: $"{nameof(reservationNumber)} is missing"));

            if (string.IsNullOrWhiteSpace(customerEmail))
                return BadRequest(HttpResult<object>.Result(
                data: null,
                httpResultType: HttpStatusCode.BadRequest,
                success: false,
                message: $"{nameof(customerEmail)} is missing"));

            var getReservationsRequest = new GetReservationsRequest
            {
                ReservationNumber = reservationNumber,
                CustomerEmail = customerEmail.TrimNullSafe().ToLower(),
                LanguageType = languageType
            };

            var localReservation = await _reservationService.GetReservation(getReservationsRequest);

            if (localReservation == null)
            {
                return HttpResult<object>.Result(
                  data: null,
                  httpResultType: HttpStatusCode.OK,
                  success: false,
                  message: "No reservation found!");
            }

            localReservation.IsCancelable = true;
            var resultReservation = _agencyService.ChechAgencyRestricted<RestrictedReservation>(localReservation);

            return HttpResult<object>.Result(
                    data: resultReservation,
                    httpResultType: HttpStatusCode.OK,
                    success: resultReservation != null,
                    message: resultReservation == null ? "No reservation found!" : string.Empty);
        }
        #endregion

        #region Get Reservation List
        //[BrokerAuthorize(UserRoles.Admin, UserRoles.Agency)]
        [HttpGet("list")]
        public async Task<ActionResult<HttpResult<object>>> Get(
            int? agencyId = null,
            int? memberId = null,
            string agencyName = null,
            string pickupDateStart = null,
            string pickupDateEnd = null,
            string returnDateStart = null,
            string returnDateEnd = null,
            string reservationDateStart = null,
            string reservationDateEnd = null,
            string reservationNumber = null,
            string apiReservationNumber = null,
            string reservationInfo = null,
            LanguageTypes? languageType = null)
        {
            var getReservationsRequest = new GetReservationsRequest
            {
                AgencyId = agencyId,
                AgencyName = agencyName,
                MemberId = memberId,
                PickupDateStart = pickupDateStart,
                PickupDateEnd = pickupDateEnd,
                ReturnDateStart = returnDateStart,
                ReturnDateEnd = returnDateEnd,
                ReservationDateStart = reservationDateStart,
                ReservationDateEnd = reservationDateEnd,
                ReservationNumber = reservationNumber,
                APIReservationNumber = apiReservationNumber,
                ReservationInfo = reservationInfo,
                LanguageType = languageType
            };

            var localReservation = await _reservationService.GetReservations(getReservationsRequest);

            var resultReservation = _agencyService.ChechAgencyRestrictedList<Reservation, RestrictedReservation>(localReservation);

            return HttpResult<object>.Result(
                    data: resultReservation,
                    httpResultType: HttpStatusCode.OK,
                    success: localReservation != null && localReservation.Count > 0,
                    message: resultReservation == null ? "No reservation found!" : string.Empty);
        }
        [HttpGet("fastList")]
        public async Task<ActionResult<HttpResult<object>>> Get(
          string customerName = "",
          string customerSurname = "",
          string customerPhone = "",
          string customerEmail = "")
        {

            var localReservation = await _reservationService.GetReservations(customerName, customerSurname, customerPhone, customerEmail);

            var resultReservation = _agencyService.ChechAgencyRestrictedList<Reservation, RestrictedReservation>(localReservation);

            return HttpResult<object>.Result(
                    data: resultReservation,
                    httpResultType: HttpStatusCode.OK,
                    success: localReservation != null && localReservation.Count > 0,
                    message: resultReservation == null ? "No reservation found!" : string.Empty);
        }
        #endregion      

        #region Post Reservation
        [HttpPost]
        public async Task<ActionResult<HttpResult<object>>> Post(
            int? memberId,
            string languageCode,
            string extraList,
            string reservationToken,
            int? customerInstutionTypeNumber,
            string customerName,
            string customerSurname,
            string customerTelephone,
            string customerEmail,
            string customerPersonalNumber,
            string customerNote,
            string customerExplanation,
            string companyTitle,
            string customerAddress,
            string companyTaxOffice,
            string companyTaxNumber,
            string flightNumberArrival,
            string flightNumberDeparture,
            string customerIPAddress,
            string paidAmount,
            string updateReservationNumber,
            bool? creditCardPaymentTypeActive,
            bool? advancePaymentTypeActive,
            int? bankId,
            int? bankVendorId,
            string creditCardHolder,
            string creditCardNumber,
            int? expiredYear,
            int? expiredMonth,
            string securityCode,
            int? installmentCount,
            bool? threeDPaymentActive,
            string threeDStatus,
            string threeDAuth,
            string threeDLevel,
            string threeDTxnId,
            string threeDMd,
            string threeDPnOrInfo,
            string specialDailyPrice,
            string specialOneWayFee,
            string extraAmount,
            bool? isCommissionFreePrice,
            bool sendReservationMail,
            string customerBirthDay,
            string departureInfo,
            string vehicleImageURL,
            string agencyReservationReference,
            PaymentTypes? paymentType,
            bool extraPricePayToDelivery,
            bool oneWayFeePayToDelivery,
            string couponCode,
            bool isSpecialWebSiteAgency,
            string skyscannerRedirectID,
            bool commercialAllowance,
            int? reservationSourceId,
            bool advancedPaymentWithoutPayment,
            string paidAmountAfterUsingCouponCode,
            bool highAmountDiscountActive,
            bool? fullCredit,
            string bank,
            string bankAccountCode,
            string bankAccountingCode,
            string country,
            string city,
            string disctrict,
            string creditCardBank,
            string provisionNumber,
            string orderNumber,
            bool? contactPermission,
            string installmentCommissionAmount,
            string paymentCode,
            string ExternalCreditCardInfo,
            float? installmentFee,
            string apiExtras,
            bool sendAgencyReservationNumber = true,
            int subAgencyId = 0
            )
        {
            var agencyId = _agencyService.GetCurrentAgencyId();
            var agencyCode = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData).Value;
            var jwtToken = _agencyService.GetCurrentBearerToken();
            var postReservationResponse = new HttpResult<object>();
            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.NumberFormat.NumberDecimalSeparator = ".";
            culture.NumberFormat.NumberGroupSeparator = ",";
            var postReservationRequest = new PostReservationRequest
            {
                AgencyId = agencyId,
                AgencyCode = agencyCode.TrimNullSafe(),
                MemberId = memberId,
                ExtraList = extraList.TrimNullSafe(),
                ReservationToken = reservationToken.TrimNullSafe(),
                CustomerInstutionTypeNumber = customerInstutionTypeNumber ?? 0,
                CustomerName = customerName.TrimNullSafe(),
                CustomerSurname = customerSurname.TrimNullSafe(),
                CustomerTelephone = customerTelephone.TrimNullSafe(),
                CustomerEmail = customerEmail.TrimNullSafe().ToLower(),
                CustomerPersonalNumber = customerPersonalNumber.TrimNullSafe(),
                CustomerNote = customerNote.TrimNullSafe(),
                CustomerExplanation = customerExplanation.TrimNullSafe(),
                CompanyTitle = companyTitle.TrimNullSafe(),
                CustomerAddress = customerAddress.TrimNullSafe(),
                CompanyTaxOffice = companyTaxOffice.TrimNullSafe(),
                CompanyTaxNumber = companyTaxNumber.TrimNullSafe(),
                FlightNumberArrival = flightNumberArrival.TrimNullSafe(),
                FlightNumberDeparture = flightNumberDeparture.TrimNullSafe(),
                CustomerIPAddress = customerIPAddress.TrimNullSafe(),
                PaidAmount = paidAmount.ToFloatNullSafe(),
                UpdateReservationNumber = updateReservationNumber.TrimNullSafe(),
                CreditCardPaymentTypeActive = creditCardPaymentTypeActive ?? false,
                AdvancePaymentTypeActive = advancePaymentTypeActive ?? false,
                BankId = bankId ?? 0,
                BankVendorId = bankVendorId ?? 0,
                CreditCardHolder = creditCardHolder.TrimNullSafe(),
                CreditCardNumber = creditCardNumber.TrimNullSafe(),
                ExpiredYear = expiredYear,
                ExpiredMonth = expiredMonth,
                SecurityCode = securityCode.TrimNullSafe(),
                InstallmentCount = installmentCount ?? 0,
                ThreeDPaymentActive = threeDPaymentActive ?? false,
                ThreeDStatus = threeDStatus.TrimNullSafe(),
                ThreeDAuth = threeDAuth.TrimNullSafe(),
                ThreeDLevel = threeDLevel.TrimNullSafe(),
                ThreeDTxnId = threeDTxnId.TrimNullSafe(),
                ThreeDMd = threeDMd.TrimNullSafe(),
                ThreeDPnOrInfo = threeDPnOrInfo.TrimNullSafe(),
                SpecialDailyPrice = !string.IsNullOrEmpty(specialDailyPrice) ? specialDailyPrice.ToFloatNullSafe() : -1,
                SpecialOneWayFee = !string.IsNullOrEmpty(specialOneWayFee) ? specialOneWayFee.ToFloatNullSafe() : -1,
                IsCommissionFreePrice = isCommissionFreePrice ?? false,
                ExtraAmount = extraAmount.ToFloatNullSafe(),
                SendReservationMail = sendReservationMail,
                DepartureInfo = departureInfo.TrimNullSafe(),
                CustomerBirthDay = customerBirthDay,
                VehicleImageURL = vehicleImageURL.TrimNullSafe(),
                AgencyReservationReference = agencyReservationReference.TrimNullSafe(),
                PaymentType = paymentType ?? PaymentTypes.PayToAgency,
                ExtraPricePayToDelivery = extraPricePayToDelivery,
                OneWayFeePayToDelivery = oneWayFeePayToDelivery,
                CouponCode = couponCode,
                IsSpecialWebSiteAgency = isSpecialWebSiteAgency,
                SkyscannerRedirectID = skyscannerRedirectID.TrimNullSafe(),
                CommercialAllowance = commercialAllowance,
                ReservationSourceId = reservationSourceId,
                AdvancedPaymentWithoutPayment = advancedPaymentWithoutPayment,
                PaidAmountAfterUsingCouponCode = paidAmountAfterUsingCouponCode.ToFloatNullSafe(),
                HighAmountDiscountActive = highAmountDiscountActive,
                FullCredit = fullCredit,
                Bank = bank,
                BankAccountCode = bankAccountCode,
                BankAccountingCode = bankAccountingCode,
                Country = country,
                City = city,
                District = disctrict,
                CreditCardBank = creditCardBank,
                ProvisionNumber = provisionNumber,
                OrderNumber = orderNumber,
                ContactPermission = contactPermission,
                InstallmentCommissionAmount = installmentCommissionAmount.ToDecimalNullSafe(),
                LanguageCode = languageCode,
                PaymentCode = paymentCode,
                RequestId = Guid.NewGuid().ToString(),
                ApiExtras = apiExtras,
                ExternalCreditCardInfo = ExternalCreditCardInfo,
                SubAgencyId = subAgencyId
            };

            Serilog.Log.Error("{@PostReservationRequest}", postReservationRequest);
            return await PostReservation(postReservationRequest);
        }
        #endregion

        [HttpPost]
        [Route("V2")]
        public async Task<ActionResult<HttpResult<object>>> V2([FromBody] PostReservationRequest postReservationRequest)
        {
            Serilog.Log.Error("{@PostReservationRequest}", postReservationRequest.ToJson());
            return await PostReservation(ObjectHelper.PostReservationRequestObjectEdit(postReservationRequest, _agencyService.GetCurrentAgencyId(), User.Claims.Where(x => x.Type == ClaimTypes.UserData).FirstOrDefault().Value));
        }
        [HttpPost]
        [Route("Save")]
        public async Task<ActionResult<HttpResult<object>>> SaveReservation([FromBody] PostReservationRequestV2 request)
        {
            Serilog.Log.Error("{@PostReservationRequest}", request.ToJson());

            var postReservationResponse = new HttpResult<object>();
            var reservationId = await _reservationStepsService.CreateNewResIdIfExist();
            var reservationNumber = ReservationHelper.GenerateReservationNumber(reservationId);
            await _configurationService.WriteLog(new BrokerLogModel(reservationNumber, request.ToJson(), BrokerLogTypes.ReservationRequest));

            var agency = await _agencyService.GetAgency(_agencyService.GetCurrentAgencyId());

            var checkPostReservationRequestResult = ReservationHelper.CheckPostReservationRequestRequireProps(request, _userRole, agency);

            if (!string.IsNullOrEmpty(checkPostReservationRequestResult))
                return await CreateAndLogErrorResult(null, HttpStatusCode.BadRequest, false, checkPostReservationRequestResult, ResultCodes.Error, "{@PostReservationResponse}", reservationNumber, BrokerLogTypes.ReservationResponse);

            var token = await _resTokenService.GetReservationTokenByUniqueId(request.ReservationToken);

            var vendor = await _vendorService.GetVendorById(token.VendorId, agency);

            if (request.Payment?.PaymentType == PaymentTypes.PayAll)
                agency.CreditCardDiscountPercent = request.Agency?.CreditCardDiscountPercent ?? 0;

            Serilog.Log.Error("{@ReservationToken}", token);

            request.LanguageCode = !string.IsNullOrEmpty(request.LanguageCode) ? request.LanguageCode.TrimNullSafe().ToUpper() : token.LanguageType.ToString();
            var subAgencyId = request.Agency?.SubAgencyId ?? agency.SubAgencyId;
            request.Agency = agency;
            request.Agency.SubAgencyId = subAgencyId;

            if (ReservationHelper.CheckPickUpDate(token))
            {
                var message = await _configurationService.GetLabel(2277, request.LanguageCode.ToEnum<LanguageTypes>());
                return await CreateAndLogErrorResult(null, HttpStatusCode.BadRequest, false, message.Replace("{time}", DateTime.Now.ToString("dd.MM.yyyy HH:mm")), ResultCodes.Error, "{@PostReservationResponse}", reservationNumber, BrokerLogTypes.ReservationResponse);
            }

            if (request.FullCredit.ToBoolNullSafe())
                if (!ReservationHelper.CheckFullCreditPermission(agency, token.FullCredit))
                    return await CreateAndLogErrorResult(null, HttpStatusCode.OK, false, "No Full-Credit permit", ResultCodes.NoFullCreditPermit, "{@PostReservationResponse}", reservationNumber, BrokerLogTypes.ReservationResponse);

            if (_userRole == UserRoles.External && request.Payment.PaymentType == PaymentTypes.AdvancePayment && request.Pricing.PaidAmount > 0)
            {
                request.Payment.PaymentType = PaymentTypes.PayOnDelivery;
                request.Payment.AdvancedPaymentWithoutPayment = true;
            }

            var extras = new List<Extra>();
            try
            {
                extras = request.Extras?.Where(e => e.Code != null).Select(e => e.MapCyrpt(token.CyrptExtras)).ToList() ?? new List<Extra>();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@CyrptExtraDecodeError}", ex.ToJson());
                return await CreateAndLogErrorResult(null, HttpStatusCode.OK, false, "Please check exra code fields!", ResultCodes.Error, "{@PostReservationResponse}", reservationNumber, BrokerLogTypes.ReservationResponse);
            }

            Serilog.Log.Error("{ReservationId}", reservationId);
            Serilog.Log.Error("{ReservationNumber:l}", reservationNumber);

            var localResponse = await _reservationService.PostReservationLocalV2(request, agency, token, vendor, reservationId, extras);

            Serilog.Log.Error("{@PostReservationLocalResult}", localResponse);

            if (!localResponse.Success)
                return await CreateAndLogErrorResult(null, HttpStatusCode.OK, false, localResponse.Message, ResultCodes.Error, "{@PostReservationResponse}", reservationNumber, BrokerLogTypes.ReservationResponse);

            Serilog.Log.Error("{@LocaleReservation}", localResponse.Data);

            string customerMail = request.Customer.Email;

            extras.RemoveAll(e => e.VendorExtraExists == false && e.ExtraType == AdditionalProductTypes.Premium);
            request.Extras?.RemoveAll(e => e.VendorExtraExists == false && e.ExtraType == AdditionalProductTypes.Premium);

            var serviceReservation = await _reservationService.PostReservationToVendorAPI(request, token, agency, vendor, reservationId, extras);

            Serilog.Log.Error("{@PostReservationVendorAPIResult}", serviceReservation);

            var serviceReservationData = serviceReservation.Data as Reservation;
            var vendorReservationMessage = GetVendorReservationMessage(serviceReservation, "Rezervasyon tedarikçi API tarafına iletilemedi!");
            var isVendorReservationSuccess = serviceReservation.Success && serviceReservationData?.APIReservationSuccessfully == true;

            var configurations = await _configurationService.GetConfigurations();

            if (configurations.AutoCancel && serviceReservationData?.APIReservationSuccessfully != true)
            {
                var postCancelReservationRequest = new PostCancelReservationRequest
                {
                    ReservationNumber = reservationNumber,
                    CustomerEmail = customerMail.TrimNullSafe().ToLower(),
                    CancelNote = "Otomatik İptal - Racapi",
                    LanguageCode = LanguageTypes.TR.ToString(),
                    IsBrokerReservation = false,
                    PenaltyStatus = _penaltyStatus.None,
                    CancelReasonId = 0,
                    UserId = string.Empty,
                    IsKpanelAdmin = true
                };

                await _configurationService.WriteLog(new BrokerLogModel(reservationNumber, GetReservationCancelRequestObjectForLog(_userRole, postCancelReservationRequest).ToJson(), BrokerLogTypes.ReservationCancelRequest));

                var cancelResponse = await _reservationService.CancelReservationLocal(postCancelReservationRequest);

                await _configurationService.WriteLog(new BrokerLogModel(reservationNumber, "Rezervasyon tedarikçi API iletilemediği için otomatik iptal!", BrokerLogTypes.ReservationCancelVendorAPIRequest));

                await _configurationService.WriteLog(new BrokerLogModel(reservationNumber, "Rezervasyon tedarikçi API iletilemediği için otomatik iptal!", BrokerLogTypes.ReservationCancelVendorAPIResponse));

                postReservationResponse = HttpResult<object>.Result(
                     data: null,
                     httpResultType: HttpStatusCode.BadGateway,
                     success: false,
                     message: vendorReservationMessage,
                     resultCode: ResultCodes.Error,
                     serviceMessage: GetVendorServiceMessage(serviceReservation));

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = reservationNumber,
                    Content = JsonConvert.SerializeObject(postReservationResponse),
                    LogType = BrokerLogTypes.ReservationResponse
                });

                return postReservationResponse;
            }


            await _reservationService.SetVendorLocalContactInformations(serviceReservationData);

            serviceReservation.Data = _reservationService.UpdateReservationWhenPostReservationToServiceSuccessfully(serviceReservation.Data as Reservation);

            Serilog.Log.Error("{@UpdatedLocalReservation}", serviceReservation.Data);

            await _reservationService.SetVendorAddress(serviceReservation.Data as Reservation);
            var reservationData2 = serviceReservationData;
            var reservationData = reservationData2.ResMap();

            try
            {
                if (!string.IsNullOrEmpty(reservationData?.APIReservationNumber))
                {
                    if (_configuration["AppSettings:BrokerName"].ToStringNullSafe() == "Airtuerk" && localResponse.Data != null)
                    {
                        var reservation = await _reservationService.GetMappedReservationByRezNo(reservationNumber);
                        var netresys = await _netResysService.SendNetresysRequestAsync(CreateBrokerAuthRequestHeader(_agencyService.GetCurrentBearerToken()), reservation, agency);
                    }
                    await _reservationService.PostReservationMail(reservationData);
                }
                else
                {
                    await _currentAccountService.DeleteCurrentAccountByReservationNumber(reservationNumber);
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@PostReservationMail}", ex.Message);
            }

            var resultReservation = _agencyService.ChechAgencyRestricted<RestrictedReservation>(reservationData);

            return await CreateAndLogErrorResult(
                resultReservation,
                HttpStatusCode.OK,
                localResponse.Data != null && isVendorReservationSuccess,
                isVendorReservationSuccess ? localResponse.Message ?? vendorReservationMessage : vendorReservationMessage,
                ResultCodes.Success,
                "{@PostReservationResponse}",
                reservationNumber,
                BrokerLogTypes.ReservationResponse,
                GetVendorServiceMessage(serviceReservation));
        }

        public async Task<ActionResult<HttpResult<object>>> PostReservation(PostReservationRequest postReservationRequest)
        {
            var postReservationResponse = new HttpResult<object>();
            var reservationId = await _reservationStepsService.CreateNewResIdIfExist();
            var reservationNumber = ReservationHelper.GenerateReservationNumber(reservationId);
            await _configurationService.WriteLog(new BrokerLogModel(reservationNumber, GetReservationRequestObjectForLog(_userRole, postReservationRequest).ToJson(), BrokerLogTypes.ReservationRequest));

            var checkPostReservationRequestResult = ReservationHelper.CheckPostReservationRequestRequireProps(postReservationRequest, _userRole);

            if (!string.IsNullOrEmpty(checkPostReservationRequestResult))
                return await CreateAndLogErrorResult(null, HttpStatusCode.BadRequest, false, checkPostReservationRequestResult, ResultCodes.Error, "{@PostReservationResponse}", reservationNumber, BrokerLogTypes.ReservationResponse);

            var reservationTokenObj = await _resTokenService.GetReservationTokenByUniqueId(postReservationRequest.ReservationToken);

            Serilog.Log.Error("{@ReservationToken}", reservationTokenObj);

            postReservationRequest.LanguageCode = !string.IsNullOrEmpty(postReservationRequest.LanguageCode) ? postReservationRequest.LanguageCode.TrimNullSafe().ToUpper() : reservationTokenObj.LanguageType.ToString();

            var vendor = await _vendorService.GetVendorById(reservationTokenObj.VendorId);
            var agency = await _agencyService.GetAgency((long)postReservationRequest.AgencyId);
            var sendAvailabilityRequest = vendor.SendAvailabilityRequest || (agency.UserRole == UserRoles.External && !(agency.AgencyName.Contains("Airtuerk") || agency.AgencyName.Contains("Tatil")));
            #region Alış tarihi kontolü
            bool checkPickUpDate = ReservationHelper.CheckPickUpDate(reservationTokenObj);
            if (checkPickUpDate)
            {
                var message = await _configurationService.GetLabel(2277, postReservationRequest.LanguageCode.ToEnum<LanguageTypes>());
                return await CreateAndLogErrorResult(null, HttpStatusCode.BadRequest, false, message.Replace("{time}", DateTime.Now.ToString("dd.MM.yyyy HH:mm")), ResultCodes.Error, "{@PostReservationResponse}", reservationNumber, BrokerLogTypes.ReservationResponse);
            }
            #endregion

            var vehicles = new List<Vehicle>();
            Vehicle vehicleFullCreditInfo = null;
            if (sendAvailabilityRequest)
            {
                var getVehiclesRequest = new GetVehiclesRequest
                {
                    VendorType = vendor.VendorType,
                    ApiKey = vendor.ApiKey,
                    ApiPassword = vendor.ApiPassword,
                    ApiClientId = vendor.ApiClientId.ToStringNullSafe(),
                    ApiLocationCode = reservationTokenObj.APIPickupLocationCode,
                    LanguageCode = postReservationRequest.LanguageCode,
                    CurrencyCode = reservationTokenObj.CurrencyType.ToString(),
                    PickupLocationId = reservationTokenObj.PickupLocationId,
                    ReturnLocationId = reservationTokenObj.ReturnLocationId,
                    PickupDate = reservationTokenObj.PickupDateTime.ToString("dd.MM.yyyy"),
                    ReturnDate = reservationTokenObj.ReturnDateTime.ToString("dd.MM.yyyy"),
                    PickupTime = reservationTokenObj.PickupDateTime.ToString("HH:mm"),
                    ReturnTime = reservationTokenObj.ReturnDateTime.ToString("HH:mm"),
                    ReservationToken = reservationTokenObj.ToJson()
                };

                Serilog.Log.Error("{@GetVehiclesRequest}", getVehiclesRequest);
                var sessionId = _httpContextAccessor?.HttpContext?.Session.GetString("user-code") ?? "";
                var vehiclesResponse = await _vehicleService.GetVehicles(getVehiclesRequest, reservationTokenObj.AgencyId.ToIntNullSafe(), sessionId, disableTimeOut: true);

                Serilog.Log.Error("{@VehiclesResponse}", vehiclesResponse);

                vehicles = vehiclesResponse.Data as List<Vehicle>;
                var checkVehicleIsAvailable = await _reservationStepsService.CheckReservationVehicleIsAvailable(vehicles, reservationTokenObj, postReservationRequest.LanguageCode.ToEnum<LanguageTypes>(), vendor);
                Serilog.Log.Error("{@CheckVehicleIsAvailable}", checkVehicleIsAvailable);
                vehicleFullCreditInfo = checkVehicleIsAvailable.Data as Vehicle;

                if (!checkVehicleIsAvailable.Success)
                    return await CreateAndLogErrorResult(null, HttpStatusCode.OK, false, checkVehicleIsAvailable.Message, (ResultCodes)checkVehicleIsAvailable.ResultCode, "{@PostReservationResponse}", reservationNumber, BrokerLogTypes.ReservationResponse);
            }
            #region Full-Credit kontrolü
            if (postReservationRequest.FullCredit.ToBoolNullSafe())
            {
                //bool fullCreditPermission = ReservationHelper.CheckFullCreditPermission(await _agencyService.GetAgency((long)postReservationRequest.AgencyId), checkVehicleIsAvailable.Data as Vehicle);
                bool fullCreditPermission = ReservationHelper.CheckFullCreditPermission(agency, vehicleFullCreditInfo?.FullCredit ?? reservationTokenObj.FullCredit);

                if (!fullCreditPermission)
                    return await CreateAndLogErrorResult(null, HttpStatusCode.OK, false, "No Full-Credit permit", ResultCodes.NoFullCreditPermit, "{@PostReservationResponse}", reservationNumber, BrokerLogTypes.ReservationResponse);
            }
            #endregion

            var getExtrasRequest = new GetExtrasRequest
            {
                ReservationToken = postReservationRequest.ReservationToken.TrimNullSafe(),
                LanguageCode = postReservationRequest.LanguageCode,
                CurrencyCode = reservationTokenObj.CurrencyType.ToString()
            };

            Serilog.Log.Error("{@PostReservationRequest}, {@GetExtrasRequest}, {@User}", postReservationRequest, getExtrasRequest, new { AgencyId = postReservationRequest.AgencyId, AgencyCode = postReservationRequest.AgencyCode });

            if (_userRole == UserRoles.External && postReservationRequest.PaymentType == PaymentTypes.AdvancePayment && postReservationRequest.PaidAmount > 0)
            {
                postReservationRequest.PaymentType = PaymentTypes.PayOnDelivery;
                postReservationRequest.AdvancedPaymentWithoutPayment = true;
            }

            var extraServiceResponse = ((!string.IsNullOrEmpty(postReservationRequest.ExtraList) && sendAvailabilityRequest)) ?
                    await _extraService.GetExtras(getExtrasRequest, getMarkupPrice: false, getAPIPrices: true, isReservationStep: true) :
                    new ServiceResponseBase
                    {
                        Success = true,
                        Data = new GetExtrasResponse
                        {
                            Extras = new List<Extra>(),
                        }
                    };

            Serilog.Log.Error("{@ExtraServiceResponse}", extraServiceResponse);

            if (extraServiceResponse.Success && extraServiceResponse.Data != null)
            {
                var getExtrasResponse = extraServiceResponse.Data as GetExtrasResponse;

                Serilog.Log.Error("{ReservationId}", reservationId);
                Serilog.Log.Error("{ReservationNumber:l}", reservationNumber);

                var postPaymentRequest = new PostPaymentRequest
                {
                    LanguageId = (int)getExtrasRequest.LanguageCode.ToEnum<LanguageTypes>() + 1,
                    CurrencyId = (int)getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>() + 1,
                    BankId = postReservationRequest.BankId ?? 0,
                    BankVendorId = postReservationRequest.BankVendorId ?? 0,
                    CustomerMailAddress = postReservationRequest.CustomerEmail.TrimNullSafe().ToLower(),
                    CreditCardHolder = postReservationRequest.CreditCardHolder.TrimNullSafe(),
                    CreditCardNumber = postReservationRequest.CreditCardNumber.TrimNullSafe(),
                    CreditCardExpiredYear = postReservationRequest.ExpiredYear ?? 0,
                    CreditCardExpiredMonth = postReservationRequest.ExpiredMonth ?? 0,
                    SecurityCode = postReservationRequest.SecurityCode.TrimNullSafe(),
                    InstallmentCount = postReservationRequest.InstallmentCount ?? 0,
                    PaymentAmount = postReservationRequest.PaidAmount.ToFloatNullSafe(),
                    OrderNo = reservationNumber,
                    IpAddress = postReservationRequest.CustomerIPAddress.TrimNullSafe(),
                    ThreeDPaymentActive = postReservationRequest.ThreeDPaymentActive ?? false,
                    Status = postReservationRequest.ThreeDStatus.TrimNullSafe(),
                    Auth = postReservationRequest.ThreeDAuth.TrimNullSafe(),
                    Level = postReservationRequest.ThreeDLevel.TrimNullSafe(),
                    Txnid = postReservationRequest.ThreeDTxnId.TrimNullSafe(),
                    Md = postReservationRequest.ThreeDMd.TrimNullSafe(),
                    PnOrInfo = postReservationRequest.ThreeDPnOrInfo.TrimNullSafe()
                };

                Serilog.Log.Fatal("{@PostPaymentRequest}", postPaymentRequest);

                var RacapiPayment = await _configurationService.GetConfigurationByDegisken("RacapiPayment");// Ödeme servisini racapi mi alacak yoksa contentapi mi işlemi
                var checkPayment = RacapiPayment.Deger.ToBoolNullSafe() && (postPaymentRequest.PaymentAmount > 0 && PaymentHelper.CheckPayment(postReservationRequest) &&
                    (postReservationRequest.PaymentType == PaymentTypes.PayAll || postReservationRequest.PaymentType == PaymentTypes.AdvancePayment || postReservationRequest.PaymentType == PaymentTypes.CommissionFree));
                var postPaymentResult = checkPayment ? await _paymentService.PostPayment(postPaymentRequest) : null;

                Serilog.Log.Fatal("{@PostPaymentResult}", postPaymentResult);

                if ((checkPayment && postPaymentResult != null && postPaymentResult.Success && postPaymentResult.Data != null) || !checkPayment)
                {
                    var postPaymentResultData = postPaymentResult != null ? postPaymentResult.Data as PostPaymentResponse : null;

                    //yolcu için ayrı kontrol yapıldı. yolcuda sabit araç id veya sabit rezervation token bulunmamakta
                    //var selectedVehicle = vehicles.Where(x => x.VehicleCode == reservationTokenObj.VehicleCode).FirstOrDefault();
                    //var selectedVehicle = vendor.VendorType != VendorTypes.Yolcu360 || reservationTokenObj.ApiVendorType != VendorTypes.Yolcu360 ? vehicles.Where(x => x.VehicleCode == reservationTokenObj.VehicleCode).FirstOrDefault() : vehicles.Where(x => x.VendorId == reservationTokenObj.APIVendorId && x.VehicleName == reservationTokenObj.VehicleName && x.FuelType == reservationTokenObj.FuelType && x.TransmissionType == reservationTokenObj.TransmissionType && x.VehicleCategoryType == reservationTokenObj.VehicleCategoryTypes).FirstOrDefault();
                    Vehicle selectedVehicle = vehicles != null ?
                        (vendor.VendorType == VendorTypes.Yolcu360 || reservationTokenObj.ApiVendorType == VendorTypes.Yolcu360
                            ? vehicles.Where(x => x.VendorId == reservationTokenObj.APIVendorId && x.VehicleName == reservationTokenObj.VehicleName && x.FuelType == reservationTokenObj.FuelType && x.TransmissionType == reservationTokenObj.TransmissionType && x.VehicleCategoryType == reservationTokenObj.VehicleCategoryType).FirstOrDefault()
                            : vehicles.Where(x => x.VehicleCode == reservationTokenObj.VehicleCode).FirstOrDefault())
                            : null;

                    var localResponse = await _reservationService.PostReservationLocal(postReservationRequest, reservationId, getExtrasResponse, postPaymentResultData, selectedVehicle);

                    Serilog.Log.Error("{@PostReservationLocalResult}", localResponse);

                    if (localResponse.Success)
                    {
                        Serilog.Log.Error("{@LocaleReservation}", localResponse.Data);

                        // Yolcu360 araçları için hem air hem net tarafında araç sorgusu müsaitlik kontrolü yapılmakta. Bunu tek tarafta yapılacak şekilde ayarlama için kullanılacak(tamamlanmadı!!)
                        //var ReferenceCode = vendor.VendorType == VendorTypes.KolayCARBroker && reservationTokenObj.ApiVendorType == VendorTypes.Yolcu360 ? reservationTokenObj.APIReferenceCode : null;

                        //VarsayılanMailGönder ayarından dolayı CustomerMailAddress değişiyor. 
                        string customerMail = postPaymentRequest.CustomerMailAddress;

                        var serviceReservation = await _reservationService.PostReservationToVendorAPI(postReservationRequest, reservationId, getExtrasResponse.Extras);

                        Serilog.Log.Error("{@PostReservationVendorAPIResult}", serviceReservation);

                        var serviceReservationData = serviceReservation.Data as Reservation;
                        var vendorReservationMessage = GetVendorReservationMessage(serviceReservation, "Rezervasyon tedarikçi API tarafına iletilemedi!");
                        var isVendorReservationSuccess = serviceReservation.Success && serviceReservationData?.APIReservationSuccessfully == true;
                        await _reservationService.SetVendorLocalContactInformations(serviceReservationData);

                        serviceReservation.Data = _reservationService.UpdateReservationWhenPostReservationToServiceSuccessfully(serviceReservation.Data as Reservation);

                        Serilog.Log.Error("{@UpdatedLocalReservation}", serviceReservation.Data);

                        var configurations = await _configurationService.GetConfigurations();

                        if (configurations.AutoCancel && serviceReservationData?.APIReservationSuccessfully != true)
                        {
                            var postCancelReservationRequest = new PostCancelReservationRequest
                            {
                                ReservationNumber = reservationNumber,
                                CustomerEmail = customerMail.TrimNullSafe().ToLower(),
                                CancelNote = "Otomatik İptal - Racapi",
                                LanguageCode = LanguageTypes.TR.ToString(),
                                IsBrokerReservation = false,
                                PenaltyStatus = _penaltyStatus.None,
                                CancelReasonId = 0,
                                UserId = string.Empty,
                                IsKpanelAdmin = true
                            };

                            await _configurationService.WriteLog(new BrokerLogModel(reservationNumber, GetReservationCancelRequestObjectForLog(_userRole, postCancelReservationRequest).ToJson(), BrokerLogTypes.ReservationCancelRequest));

                            var cancelResponse = await _reservationService.CancelReservationLocal(postCancelReservationRequest);

                            await _configurationService.WriteLog(new BrokerLogModel(reservationNumber, "Rezervasyon tedarikçi API iletilemediği için otomatik iptal!", BrokerLogTypes.ReservationCancelVendorAPIRequest));

                            await _configurationService.WriteLog(new BrokerLogModel(reservationNumber, "Rezervasyon tedarikçi API iletilemediği için otomatik iptal!", BrokerLogTypes.ReservationCancelVendorAPIResponse));

                            postReservationResponse = HttpResult<object>.Result(
                                 data: null,
                                 httpResultType: HttpStatusCode.BadGateway,
                                 success: false,
                                 message: vendorReservationMessage,
                                 resultCode: ResultCodes.Error,
                                 serviceMessage: GetVendorServiceMessage(serviceReservation));

                            await _configurationService.WriteLog(new BrokerLogModel
                            {
                                LogKey = reservationNumber,
                                Content = JsonConvert.SerializeObject(postReservationResponse),
                                LogType = BrokerLogTypes.ReservationResponse
                            });

                            return postReservationResponse;
                        }

                        await _reservationService.SetVendorAddress(serviceReservation.Data as Reservation);
                        var reservationData2 = serviceReservationData;
                        var reservationData = reservationData2.ResMap();

                        try
                        {
                            if (!string.IsNullOrEmpty(reservationData?.APIReservationNumber))
                            {
                                var brokerName = _configuration["AppSettings:BrokerName"].ToStringNullSafe();

                                if (brokerName == "Airtuerk" && localResponse.Data != null)
                                {
                                    var reservation = await _reservationService.GetMappedReservationByRezNo(reservationNumber);
                                    var netresys = await _netResysService.SendNetresysRequestAsync(CreateBrokerAuthRequestHeader(_agencyService.GetCurrentBearerToken()), reservation, agency);
                                }
                                await _reservationService.PostReservationMail(reservationData);
                            }
                        }
                        catch (Exception ex)
                        {
                            Serilog.Log.Error("{@PostReservationMail}", ex.Message);
                        }

                        var resultReservation = _agencyService.ChechAgencyRestricted<RestrictedReservation>(reservationData);

                        return await CreateAndLogErrorResult(
                            resultReservation,
                            HttpStatusCode.OK,
                            localResponse.Data != null && isVendorReservationSuccess,
                            isVendorReservationSuccess ? localResponse.Message ?? vendorReservationMessage : vendorReservationMessage,
                            ResultCodes.Success,
                            "{@PostReservationResponse}",
                            reservationNumber,
                            BrokerLogTypes.ReservationResponse,
                            GetVendorServiceMessage(serviceReservation));
                    }
                    else
                        return await CreateAndLogErrorResult(null, HttpStatusCode.OK, false, localResponse.Message, ResultCodes.Error, "{@PostReservationResponse}", reservationNumber, BrokerLogTypes.ReservationResponse);
                }
                else
                    return await CreateAndLogErrorResult(null, HttpStatusCode.OK, false, postPaymentResult.Message, ResultCodes.Error, "{@PostReservationResponse}", reservationNumber, BrokerLogTypes.ReservationResponse);
            }
            return await CreateAndLogErrorResult(null, HttpStatusCode.OK, false, "Source service could not be reached. Please try again!", ResultCodes.Error, "{@PostReservationResponse}", reservationNumber, BrokerLogTypes.ReservationResponse);
        }

        #region Cancel Reservation
        //[HttpPost("cancel")]
        //public async Task<ActionResult<HttpResult<object>>> CancelReservation(
        //    string reservationNumber,
        //    string customerEmail,
        //    string cancelNote,
        //    string languageCode,
        //    int cancelReasonId,
        //    string userId,
        //    bool isKpanelAdmin = false,
        //    bool isBrokerReservation = false,
        //    _penaltyStatus PenaltyStatus = _penaltyStatus.None
        //    )
        //{
        //    var postCancelReservationRequest = new PostCancelReservationRequest
        //    {
        //        ReservationNumber = reservationNumber,
        //        CustomerEmail = customerEmail.TrimNullSafe().ToLower(),
        //        CancelNote = cancelNote,
        //        LanguageCode = languageCode ?? LanguageTypes.EN.ToString(),
        //        IsBrokerReservation = isBrokerReservation,
        //        PenaltyStatus = PenaltyStatus,
        //        CancelReasonId = cancelReasonId,
        //        UserId = userId,
        //        IsKpanelAdmin = isKpanelAdmin
        //    };

        //    await _configurationService.WriteLog(new BrokerLogModel(reservationNumber, GetReservationCancelRequestObjectForLog(_userRole, postCancelReservationRequest).ToJson(), BrokerLogTypes.ReservationCancelRequest));

        //    if (string.IsNullOrWhiteSpace(reservationNumber))
        //        return await CreateAndLogErrorResult(null, HttpStatusCode.BadRequest, false, $"{nameof(reservationNumber)} is missing", ResultCodes.Success, "{@CancelReservationResponse}", reservationNumber, BrokerLogTypes.ReservationCancelResponse);

        //    if (string.IsNullOrWhiteSpace(customerEmail))
        //        return await CreateAndLogErrorResult(null, HttpStatusCode.BadRequest, false, $"{nameof(customerEmail)} is missing", ResultCodes.Success, "{@CancelReservationResponse}", reservationNumber, BrokerLogTypes.ReservationCancelResponse);

        //    Serilog.Log.Error("{CancelReservationNumber:l}", reservationNumber);
        //    Serilog.Log.Error("{@PostCancelReservationRequest}", postCancelReservationRequest);

        //    var configurations = await _configurationService.GetConfigurations();

        //    ServiceResponseBase localResponse, serviceCancelReservation;
        //    Reservation serviceCancelReservationObject;

        //    if (configurations.IsCancelledOnTheApiFirst)
        //    {
        //        serviceCancelReservation = await _reservationService.CancelReservationService(postCancelReservationRequest);

        //        Serilog.Log.Error("{@CancelReservationServiceResponse}", serviceCancelReservation);

        //        serviceCancelReservationObject = serviceCancelReservation.Data as Reservation;

        //        if (!serviceCancelReservationObject.APIReservationCancel)
        //        {
        //            return await CreateAndLogErrorResult(null, HttpStatusCode.OK, false, "The reservation could not be canceled!", ResultCodes.Error, "{@CancelReservationResponse}", reservationNumber, BrokerLogTypes.ReservationCancelResponse);
        //        }

        //        localResponse = await _reservationService.CancelReservationLocal(postCancelReservationRequest);
        //        Serilog.Log.Error("{@CancelReservationLocalResponse}", localResponse);
        //    }
        //    else
        //    {
        //        localResponse = await _reservationService.CancelReservationLocal(postCancelReservationRequest);
        //        Serilog.Log.Error("{@CancelReservationLocalResponse}", localResponse);

        //        serviceCancelReservation = await _reservationService.CancelReservationService(postCancelReservationRequest);

        //        Serilog.Log.Error("{@CancelReservationServiceResponse}", serviceCancelReservation);

        //        serviceCancelReservationObject = serviceCancelReservation.Data as Reservation;
        //    }

        //    var isRefunded = configurations.NewSetting || _reservationService.CheckReservationRefundStatus(serviceCancelReservationObject);

        //    if (configurations.PaymentRefundActive && serviceCancelReservationObject != null && serviceCancelReservationObject.CancellationRefundedAmount > 0 &&
        //        (serviceCancelReservationObject.PaymentType == PaymentTypes.PayAll || serviceCancelReservationObject.PaymentType == PaymentTypes.AdvancePayment) && isRefunded)
        //    {
        //        var postPaymentRefundRequest = new PostPaymentRefundRequest
        //        {
        //            LanguageCode = serviceCancelReservationObject.LanguageType.ToString(),
        //            CurrencyCode = serviceCancelReservationObject.CurrencyCode,
        //            IpAddress = serviceCancelReservationObject.IpAddress,
        //            // OrderNumber = configurations.IyzicoRefundActive == true ? serviceCancelReservationObject.ProvizyonNo : serviceCancelReservationObject.ProvizyonNo,
        //            OrderNumber = serviceCancelReservationObject.ProvizyonNo,
        //            //OrderNumber = serviceCancelReservationObject.ReservationNumber,
        //            RefundAmount = serviceCancelReservationObject.CancellationRefundedAmount,
        //            PaymentRefundType = PaymentRefundTypes.Cancel
        //        };

        //        var postPaymentRefundResult = new ServiceResponseBase();

        //        if (DateTime.Now <= serviceCancelReservationObject.ReservationDate.AddDays(1) &&
        //            serviceCancelReservationObject.CancellationRefundedAmount == serviceCancelReservationObject.TotalPrice)
        //            postPaymentRefundResult = await _paymentService.PostPaymentRefund(postPaymentRefundRequest);

        //        if (!postPaymentRefundResult.Success)
        //        {
        //            postPaymentRefundRequest.PaymentRefundType = PaymentRefundTypes.Refund;
        //            var postPaymentCancelResult = await _paymentService.PostPaymentRefund(postPaymentRefundRequest);
        //            serviceCancelReservationObject.PaymentRefundSuccess = postPaymentCancelResult.Success;
        //        }
        //        else
        //            serviceCancelReservationObject.PaymentRefundSuccess = true;

        //        _reservationService.UpdateReservationWhenPaymentRefundSuccessfully(serviceCancelReservationObject);
        //    }

        //    if (serviceCancelReservation != null && serviceCancelReservation.Success && serviceCancelReservation.Data != null)
        //        serviceCancelReservation.Data = _reservationService.UpdateReservationWhenCancelReservationToServiceSuccessfully(serviceCancelReservationObject);

        //    if (localResponse.Success)
        //    {
        //        var reservation = localResponse.Data as Reservation;

        //        await _invoiceService.CancelReservationInvoice(reservation.ReservationNumber, reservation.CustomerMail);

        //        if (!string.IsNullOrEmpty(reservation?.APIReservationNumber) && (bool)reservation?.APIReservationSuccessfully)
        //        {
        //            try
        //            {
        //                var brokerName = _configuration["AppSettings:BrokerName"].ToStringNullSafe();

        //                if (localResponse.Data != null && brokerName == "Airtuerk")
        //                {
        //                    var reservationMapped = await _reservationService.GetMappedReservationByRezNo(reservationNumber);
        //                    var netresys = await _netResysService.SendNetresysCancelRequestAsync(CreateBrokerAuthRequestHeader(_agencyService.GetCurrentBearerToken()), reservationMapped);
        //                }

        //                await _reservationService.PostReservationMail(reservation);
        //            }
        //            catch (Exception ex)
        //            {
        //                Serilog.Log.Error("{@PostCancelMailError}", ex.ToJson());
        //            }
        //        }
        //        else
        //        {
        //            try
        //            {
        //                await _currentAccountService.DeleteCurrentAccountByReservationNumber(reservationNumber);
        //            }
        //            catch (Exception ex)
        //            {
        //                Serilog.Log.Error("{@DeleteCurrentAccountError}", ex.ToJson());
        //            }
        //        }

        //    }

        //    var reservationData = serviceCancelReservation.Success ?
        //       serviceCancelReservation :
        //       localResponse;


        //    var resultReservation = _agencyService.ChechAgencyRestricted<RestrictedReservation>(reservationData.Data as Reservation);

        //    return await CreateAndLogErrorResult(resultReservation, HttpStatusCode.OK, reservationData.Success, resultReservation != null ? "The reservation has been successfully canceled!" : string.Empty, ResultCodes.Success, "{@CancelReservationResponse}", reservationNumber, BrokerLogTypes.ReservationCancelResponse);
        //}
        #endregion
        [HttpPost("cancel")]
        public async Task<ActionResult<HttpResult<object>>> CancelReservation(
         string reservationNumber,
         string customerEmail,
         string cancelNote,
         string languageCode,
         int cancelReasonId,
         string userId,
         bool isKpanelAdmin = false,
         bool isBrokerReservation = false,
         _penaltyStatus PenaltyStatus = _penaltyStatus.None
         )
        {
            var postCancelReservationRequest = new PostCancelReservationRequest
            {
                ReservationNumber = reservationNumber,
                CustomerEmail = customerEmail.TrimNullSafe().ToLower(),
                CancelNote = cancelNote,
                LanguageCode = languageCode ?? LanguageTypes.EN.ToString(),
                IsBrokerReservation = isBrokerReservation,
                PenaltyStatus = PenaltyStatus,
                CancelReasonId = cancelReasonId,
                UserId = userId,
                IsKpanelAdmin = isKpanelAdmin
            };

            await _configurationService.WriteLog(new BrokerLogModel(reservationNumber, GetReservationCancelRequestObjectForLog(_userRole, postCancelReservationRequest).ToJson(), BrokerLogTypes.ReservationCancelRequest));

            if (string.IsNullOrWhiteSpace(reservationNumber))
                return await CreateAndLogErrorResult(null, HttpStatusCode.BadRequest, false, $"{nameof(reservationNumber)} is missing", ResultCodes.Success, "{@CancelReservationResponse}", reservationNumber, BrokerLogTypes.ReservationCancelResponse);

            if (string.IsNullOrWhiteSpace(customerEmail))
                return await CreateAndLogErrorResult(null, HttpStatusCode.BadRequest, false, $"{nameof(customerEmail)} is missing", ResultCodes.Success, "{@CancelReservationResponse}", reservationNumber, BrokerLogTypes.ReservationCancelResponse);

            Serilog.Log.Error("{CancelReservationNumber:l}", reservationNumber);
            Serilog.Log.Error("{@PostCancelReservationRequest}", postCancelReservationRequest);

            var configurations = await _configurationService.GetConfigurations();

            ServiceResponseBase localResponse, serviceCancelReservation;
            Reservation serviceCancelReservationObject;

            var getReservationsRequest = new GetReservationsRequest
            {
                ReservationNumber = postCancelReservationRequest.ReservationNumber,
                CustomerEmail = postCancelReservationRequest.CustomerEmail
            };

            var reservation = await _reservationService.GetReservation(getReservationsRequest, isBrokerReservation = postCancelReservationRequest.IsBrokerReservation);

            if (reservation.ReservationStatusType == ReservationStatusTypes.Cancelled)
                return await CreateAndLogErrorResult(null, HttpStatusCode.OK, false, "The reservation has already been canceled.", ResultCodes.Error, "{@CancelReservationResponse}", reservationNumber, BrokerLogTypes.ReservationCancelResponse);

            if (string.IsNullOrEmpty(reservation.APIReservationNumber))
            {
                localResponse = await _reservationService.CancelReservationLocalV2(postCancelReservationRequest, reservation);
                Serilog.Log.Error("{@CancelReservationLocalResponse}", localResponse);

                serviceCancelReservation = new ServiceResponseBase(reservation, false, "Tedarikçiye iletilemediği için sadece lokal de iptal edildi!");
                serviceCancelReservationObject = reservation;
            }
            else if (configurations.IsCancelledOnTheApiFirst)
            {
                serviceCancelReservation = await _reservationService.CancelReservationServiceV2(postCancelReservationRequest, reservation);

                Serilog.Log.Error("{@CancelReservationServiceResponse}", serviceCancelReservation);

                serviceCancelReservationObject = serviceCancelReservation.Data as Reservation;

                if (!serviceCancelReservationObject.APIReservationCancel)
                {
                    return await CreateAndLogErrorResult(null, HttpStatusCode.OK, false, "The reservation could not be canceled!", ResultCodes.Error, "{@CancelReservationResponse}", reservationNumber, BrokerLogTypes.ReservationCancelResponse);
                }

                localResponse = await _reservationService.CancelReservationLocalV2(postCancelReservationRequest, reservation);
                Serilog.Log.Error("{@CancelReservationLocalResponse}", localResponse);
                serviceCancelReservationObject.ReservationStatusType = ReservationStatusTypes.Cancelled;
            }
            else
            {
                localResponse = await _reservationService.CancelReservationLocalV2(postCancelReservationRequest, reservation);
                Serilog.Log.Error("{@CancelReservationLocalResponse}", localResponse);

                serviceCancelReservation = await _reservationService.CancelReservationServiceV2(postCancelReservationRequest, reservation);

                Serilog.Log.Error("{@CancelReservationServiceResponse}", serviceCancelReservation);
                serviceCancelReservationObject = serviceCancelReservation.Data as Reservation;
                serviceCancelReservationObject.ReservationStatusType = ReservationStatusTypes.Cancelled;
            }

            var isRefunded = configurations.NewSetting || _reservationService.CheckReservationRefundStatus(serviceCancelReservationObject);

            if (configurations.PaymentRefundActive && serviceCancelReservationObject != null && serviceCancelReservationObject.CancellationRefundedAmount > 0 &&
                (serviceCancelReservationObject.PaymentType == PaymentTypes.PayAll || serviceCancelReservationObject.PaymentType == PaymentTypes.AdvancePayment) && isRefunded)
            {
                var postPaymentRefundRequest = new PostPaymentRefundRequest
                {
                    LanguageCode = serviceCancelReservationObject.LanguageType.ToString(),
                    CurrencyCode = serviceCancelReservationObject.CurrencyCode,
                    IpAddress = serviceCancelReservationObject.IpAddress,
                    // OrderNumber = configurations.IyzicoRefundActive == true ? serviceCancelReservationObject.ProvizyonNo : serviceCancelReservationObject.ProvizyonNo,
                    OrderNumber = serviceCancelReservationObject.ProvizyonNo,
                    //OrderNumber = serviceCancelReservationObject.ReservationNumber,
                    RefundAmount = serviceCancelReservationObject.CancellationRefundedAmount,
                    PaymentRefundType = PaymentRefundTypes.Cancel
                };

                var postPaymentRefundResult = new ServiceResponseBase();

                if (DateTime.Now <= serviceCancelReservationObject.ReservationDate.AddDays(1) &&
                    serviceCancelReservationObject.CancellationRefundedAmount == serviceCancelReservationObject.TotalPrice)
                    postPaymentRefundResult = await _paymentService.PostPaymentRefund(postPaymentRefundRequest);

                if (!postPaymentRefundResult.Success)
                {
                    postPaymentRefundRequest.PaymentRefundType = PaymentRefundTypes.Refund;
                    var postPaymentCancelResult = await _paymentService.PostPaymentRefund(postPaymentRefundRequest);
                    serviceCancelReservationObject.PaymentRefundSuccess = postPaymentCancelResult.Success;
                }
                else
                    serviceCancelReservationObject.PaymentRefundSuccess = true;

                _reservationService.UpdateReservationWhenPaymentRefundSuccessfully(serviceCancelReservationObject);
            }

            if (serviceCancelReservation != null && serviceCancelReservation.Success && serviceCancelReservation.Data != null)
                serviceCancelReservation.Data = _reservationService.UpdateReservationWhenCancelReservationToServiceSuccessfully(serviceCancelReservationObject);

            if (localResponse.Success)
            {

                await _invoiceService.CancelReservationInvoice(reservation.ReservationNumber, reservation.CustomerMail);

                if (!string.IsNullOrEmpty(reservation?.APIReservationNumber) && (bool)reservation?.APIReservationSuccessfully)
                {
                    try
                    {
                        var brokerName = _configuration["AppSettings:BrokerName"].ToStringNullSafe();

                        if (localResponse.Data != null && brokerName == "Airtuerk")
                        {
                            var reservationMapped = await _reservationService.GetMappedReservationByRezNo(reservationNumber);
                            var netresys = await _netResysService.SendNetresysCancelRequestAsync(CreateBrokerAuthRequestHeader(_agencyService.GetCurrentBearerToken()), reservationMapped);
                        }

                        await _reservationService.PostReservationMail(reservation);
                    }
                    catch (Exception ex)
                    {
                        Serilog.Log.Error("{@PostCancelMailError}", ex.ToJson());
                    }
                }
                else
                {
                    try
                    {
                        await _currentAccountService.DeleteCurrentAccountByReservationNumber(reservationNumber);
                    }
                    catch (Exception ex)
                    {
                        Serilog.Log.Error("{@DeleteCurrentAccountError}", ex.ToJson());
                    }
                }

            }

            var reservationData = serviceCancelReservation.Success ?
               serviceCancelReservation :
               localResponse;

            var resultReservation = _agencyService.ChechAgencyRestricted<RestrictedReservation>(reservationData.Data as Reservation);

            return await CreateAndLogErrorResult(resultReservation, HttpStatusCode.OK, reservationData.Success, resultReservation != null ? "The reservation has been successfully canceled!" : string.Empty, ResultCodes.Success, "{@CancelReservationResponse}", reservationNumber, BrokerLogTypes.ReservationCancelResponse);
        }
        #region Resend Vouchers
        [HttpGet]
        [Route("resend-vouchers")]
        public async Task<IActionResult> ResendVouchers(string reservationNumber,
            string customerEmail,
            LanguageTypes? languageType = null)
        {
            try
            {
                var getReservationsRequest = new GetReservationsRequest
                {
                    ReservationNumber = reservationNumber,
                    CustomerEmail = customerEmail.TrimNullSafe().ToLower(),
                    LanguageType = languageType
                };
                Serilog.Log.Error("{@ResendVouchers}", $"{reservationNumber} - {customerEmail}");
                var reservationData = await _reservationService.GetReservation(getReservationsRequest);
                await _reservationService.PostReservationMail(reservationData);
                return Ok("Mail send successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest($"Mail could not send. Exception: {ex.Message}");
            }
        }
        #endregion

        #region Functions     
        private object GetReservationRequestObjectForLog(UserRoles userRole, PostReservationRequest postReservationRequest)
        {
            switch (userRole)
            {
                case UserRoles.External:
                    {
                        return new
                        {
                            postReservationRequest.ReservationToken,
                            postReservationRequest.ExtraList,
                            postReservationRequest.CustomerName,
                            postReservationRequest.CustomerSurname,
                            postReservationRequest.CustomerTelephone,
                            postReservationRequest.CustomerEmail,
                            postReservationRequest.CustomerBirthDay,
                            postReservationRequest.CustomerPersonalNumber,
                            postReservationRequest.CustomerNote,
                            postReservationRequest.CustomerAddress,
                            postReservationRequest.FlightNumberArrival,
                            postReservationRequest.FlightNumberDeparture,
                            postReservationRequest.CustomerIPAddress,
                            postReservationRequest.PaidAmount,
                            postReservationRequest.ExtraAmount,
                            postReservationRequest.SendReservationMail,
                            postReservationRequest.DepartureInfo,
                            postReservationRequest.AgencyReservationReference,
                            postReservationRequest.ExtraPricePayToDelivery,
                            postReservationRequest.OneWayFeePayToDelivery,
                            postReservationRequest.PaymentType,
                            postReservationRequest.FullCredit
                        };
                    }
                default: return StringHelper.MaskCreditCard(JsonConvert.DeserializeObject<PostReservationRequest>(JsonConvert.SerializeObject(postReservationRequest)));
            }
        }
        private object GetReservationRequestObjectForLog(UserRoles userRole, Domain.Models.Requests.PostReservationRequestV2 postReservationRequest)
        {
            switch (userRole)
            {
                case UserRoles.External:
                    {
                        return new
                        {
                            postReservationRequest.ReservationToken,
                            postReservationRequest.Extras,
                            postReservationRequest.Customer.Name,
                            postReservationRequest.Customer.Surname,
                            postReservationRequest.Customer.PhoneNumber,
                            postReservationRequest.Customer.Email,
                            postReservationRequest.Customer.BirthDay,
                            postReservationRequest.Customer.PersonalNumber,
                            postReservationRequest.Customer.Note,
                            postReservationRequest.Customer.Address,
                            postReservationRequest.FlightNumberArrival,
                            postReservationRequest.FlightNumberDeparture,
                            postReservationRequest.Customer.IPAddress,
                            postReservationRequest.Pricing.PaidAmount,
                            postReservationRequest.Pricing.ExtraAmount,
                            postReservationRequest.SendReservationMail,
                            postReservationRequest.DepartureInfo,
                            postReservationRequest.AgencyReservationReference,
                            postReservationRequest.Payment.ExtraPricePayToDelivery,
                            postReservationRequest.Payment.OneWayFeePayToDelivery,
                            postReservationRequest.Payment.PaymentType,
                            postReservationRequest.FullCredit
                        };
                    }
                default: return StringHelper.MaskCreditCard(JsonConvert.DeserializeObject<PostReservationRequest>(JsonConvert.SerializeObject(postReservationRequest)));
            }
        }


        private object GetReservationCancelRequestObjectForLog(UserRoles userRole, dynamic postCancelReservationRequest)
        {
            switch (userRole)
            {
                case UserRoles.External:
                    {
                        return new
                        {
                            postCancelReservationRequest.ReservationNumber,
                            postCancelReservationRequest.CustomerEmail,
                            postCancelReservationRequest.CancelNote
                        };
                    }
                default: return postCancelReservationRequest;
            }
        }
        #endregion

        public static Dictionary<string, object> CreateBrokerAuthRequestHeader(string bearer) =>
            new Dictionary<string, object>()
            {
                { "Authorization", $"Bearer {bearer}" }
            };

        public async Task<HttpResult<object>> CreateAndLogErrorResult(object data, HttpStatusCode httpStatusCode, bool success, string message, ResultCodes resultCode, string logKey, string reservationNumber, BrokerLogTypes brokerLogType, string serviceMessage = null)
        {
            var response = HttpResult<object>.Result(
              data: data,
              httpResultType: httpStatusCode,
              success: success,
              message: message,
              resultCode: resultCode,
              serviceMessage: serviceMessage);

            Serilog.Log.Error(logKey, response);
            await _configurationService.WriteLog(new BrokerLogModel(reservationNumber, response.ToJson(), brokerLogType));

            return response;
        }

        private static string GetVendorReservationMessage(ServiceResponseBase serviceReservation, string fallbackMessage = "")
            => serviceReservation?.Message ?? fallbackMessage;

        private static string GetVendorServiceMessage(ServiceResponseBase serviceReservation)
        {
            if (!string.IsNullOrWhiteSpace(serviceReservation?.ServiceMessage))
                return serviceReservation.ServiceMessage;

            if (serviceReservation?.Data is Reservation reservation && !string.IsNullOrWhiteSpace(reservation.APIMessage))
                return reservation.APIMessage;

            return null;
        }
    }
}
