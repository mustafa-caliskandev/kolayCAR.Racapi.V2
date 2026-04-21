using kolayCAR.Broker.AWS.Services;
using KolayCAR.Broker.API.Attributes;
using KolayCAR.Broker.API.Extensions;
using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Models.Dtos;
using KolayCAR.Broker.API.Models.MobileAppDtos.ReservationDtos;
using KolayCAR.Broker.API.Models.MobileAppDtos.ResponseDtos;
using KolayCAR.Broker.API.Models.PaymentDto;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using PaymentDto = KolayCAR.Broker.API.Models.PaymentDto.PaymentDto;

namespace KolayCAR.Broker.API.Controllers
{
    [BrokerAuthorize(UserRoles.Admin, UserRoles.Agency, UserRoles.User, UserRoles.MobileAPP)]
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        #region Definations
        private readonly BrokerContext _context;

        private readonly IReservationPaymentDetailService _reservationPaymentDetailService;
        private readonly IPaymentResultService _paymentResultService;
        private readonly IPaymentService _paymentService;
        private readonly IConfigurationService _configurationService;
        private readonly ICouponService _couponService;
        private readonly IBlockedMemberService _blockedMemberService;
        private readonly IPaymentSettingService _paymentSettingService;
        private readonly IReservationService _reservationService;
        private readonly IReservationDetailService _reservationDetailService;
        private readonly HttpContextHelper _httpContextHelper;
        private readonly IConfiguration _configuration;
        private readonly IReservationTokenService _reservationTokenService;
        private readonly IParameterService _parameterService;
        private readonly ISummaryService _summaryService;
        private readonly IAgencyService _agencyService;
        private readonly IReservationStepsService _reservationStepsService;
        private readonly IExtraService _extraService;
        private readonly IBankBinCodeService _bankBinCodeService;
        private readonly ILabelService _labelService;
        private readonly ILocationVendorInstallmentService _locationVendorInstallmentService;
        private readonly IBankService _bankService;
        private readonly IInstallmentService _installmentService;
        private readonly ILanguageService _languageService;
        private readonly ICurrencyService _currencyService;
        private readonly IVendorService _vendorService;
        private readonly IResTokenService _resTokenService;
        private UserRoles _userRole;
        #endregion

        #region Constructor
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IAWSService _awsService;
        public PaymentController(
            IMemoryCacheService memoryCacheService,

            BrokerContext context,
            IReservationPaymentDetailService reservationPaymentDetailService,
            IPaymentResultService paymentResultService,
            IPaymentService paymentService,
            IConfigurationService configurationService,
            IBlockedMemberService blockedMemberService,
            IPaymentSettingService paymentSettingService,
            IReservationService reservationService,
            IReservationDetailService reservationDetailService,
            HttpContextHelper httpContextHelper,
            IConfiguration configuration,
            IReservationTokenService reservationTokenService,
            IParameterService parameterService,
            ISummaryService summaryService,
            IAgencyService agencyService,
            IReservationStepsService reservationStepsService,
            IBankBinCodeService bankBinCodeService,
            ILabelService labelService,
            ILocationVendorInstallmentService locationVendorInstallmentService,
            IBankService bankService,
            IInstallmentService installmentService,
            IExtraService extraService,
            ICouponService couponService,
            ILanguageService languageService,
            ICurrencyService currencyService,
            IVendorService vendorService,
            IAWSService awsService,
            IResTokenService resTokenService)
        {
            _memoryCacheService = memoryCacheService;
            _reservationPaymentDetailService = reservationPaymentDetailService;
            _paymentResultService = paymentResultService;
            _paymentService = paymentService;
            _configurationService = configurationService;
            _context = context;
            _blockedMemberService = blockedMemberService;
            _paymentSettingService = paymentSettingService;
            _reservationService = reservationService;
            _reservationDetailService = reservationDetailService;
            _httpContextHelper = httpContextHelper;
            _configuration = configuration;
            _reservationTokenService = reservationTokenService;
            _parameterService = parameterService;
            _summaryService = summaryService;
            _agencyService = agencyService;
            _reservationStepsService = reservationStepsService;
            _extraService = extraService;
            _bankBinCodeService = bankBinCodeService;
            _labelService = labelService;
            _locationVendorInstallmentService = locationVendorInstallmentService;
            _bankService = bankService;
            _installmentService = installmentService;
            _userRole = _agencyService.GetCurrentUserRole();
            _couponService = couponService;
            _languageService = languageService;
            _currencyService = currencyService;
            _vendorService = vendorService;
            _awsService = awsService;
            _resTokenService = resTokenService;
        }
        #endregion

        #region v1
        [HttpGet("settings")]
        public async Task<ActionResult<HttpResult<GetPaymentSettingsResponse>>> Get(
           int languageId,
           string binNumber,
           bool advancePaymentActive,
           string paymentAmount)
        {
            var getPaymentSettingsRequest = new GetPaymentSettingsRequest
            {
                LanguageId = languageId,
                BINNumber = binNumber.RemoveSpecialCharacters().GetEightOrSixCharactersSafe(),
                AdvancePaymentActive = advancePaymentActive,
                PaymentAmount = paymentAmount.ToFloatNullSafe()
            };

            Serilog.Log.Fatal("{@GetPaymentSettingsRequest}", getPaymentSettingsRequest);

            var result = await _paymentService.GetPaymentSettings(getPaymentSettingsRequest);

            Serilog.Log.Fatal("{@GetPaymentSettingsResponse}", result);

            return HttpResult<GetPaymentSettingsResponse>.Result(
                data: result.Data as GetPaymentSettingsResponse,
                httpResultType: HttpStatusCode.OK,
                success: result.Success,
                message: result.ServiceMessage ?? result.Message);
        }

        [HttpPost("threeDSecureControl")]
        public async Task<ActionResult<HttpResult<PostThreeDSecureControlResponse>>> PostThreeDSecureControl(
            int languageId,
            int currencyId,
            int bankId,
            int bankVendorId,
            string customerMailAddress,
            string creditCardHolder,
            string creditCardNumber,
            int creditCardExpiredYear,
            int creditCardExpiredMonth,
            string securityCode,
            int installmentCount,
            string paymentAmount,
            string orderNo,
            string ipAddress,
            string callbackUrl)
        {
            var postThreeDSecureControlRequest = new PostThreeDSecureControlRequest
            {
                LanguageId = languageId,
                CurrencyId = currencyId,
                BankId = bankId,
                BankVendorId = bankVendorId,
                CustomerMailAddress = customerMailAddress,
                CreditCardHolder = creditCardHolder,
                CreditCardNumber = creditCardNumber,
                CreditCardExpiredYear = creditCardExpiredYear,
                CreditCardExpiredMonth = creditCardExpiredMonth,
                SecurityCode = securityCode,
                InstallmentCount = installmentCount,
                PaymentAmount = paymentAmount.ToFloatNullSafe(),
                OrderNo = orderNo,
                IpAddress = ipAddress,
                CallbackUrl = callbackUrl
            };
            try
            {
                var postThreeDSecureControlRequestLog = new PostThreeDSecureControlRequestV2();
                postThreeDSecureControlRequestLog.LanguageId = languageId;
                postThreeDSecureControlRequestLog.CurrencyId = currencyId;
                postThreeDSecureControlRequestLog.BankId = bankId;
                postThreeDSecureControlRequestLog.BankVendorId = bankVendorId;
                postThreeDSecureControlRequestLog.CustomerMailAddress = customerMailAddress;
                postThreeDSecureControlRequestLog.CreditCardHolder = creditCardHolder;
                postThreeDSecureControlRequestLog.CreditCardNumber = MaskCardNumber(creditCardNumber);
                postThreeDSecureControlRequestLog.CreditCardExpiredYear = MaskExpire(creditCardExpiredYear.ToStringNullSafe());
                postThreeDSecureControlRequestLog.CreditCardExpiredMonth = MaskExpire(creditCardExpiredMonth.ToStringNullSafe());
                postThreeDSecureControlRequestLog.SecurityCode = MaskExpire(securityCode);
                postThreeDSecureControlRequestLog.InstallmentCount = installmentCount;
                postThreeDSecureControlRequestLog.PaymentAmount = paymentAmount.ToFloatNullSafe();
                postThreeDSecureControlRequestLog.OrderNo = orderNo;
                postThreeDSecureControlRequestLog.IpAddress = ipAddress;
                postThreeDSecureControlRequestLog.CallbackUrl = callbackUrl;


                Serilog.Log.Fatal("{@PostThreeDSecureControlRequest}", postThreeDSecureControlRequestLog);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@PaymentMaskError}", ex.ToJson());
            }


            var result = await _paymentService.PostThreeDSecureControl(postThreeDSecureControlRequest);

            Serilog.Log.Fatal("{@PostThreeDSecureControlResponse}", result);

            return HttpResult<PostThreeDSecureControlResponse>.Result(
                data: result.Data as PostThreeDSecureControlResponse,
                httpResultType: HttpStatusCode.OK,
                success: result.Success,
                message: result.ServiceMessage ?? result.Message);
        }

        [HttpPost]
        public async Task<ActionResult<HttpResult<PostPaymentResponse>>> Post(
            int languageId,
            int currencyId,
            int bankId,
            int bankVendorId,
            string customerMailAddress,
            string creditCardHolder,
            string creditCardNumber,
            int creditCardExpiredYear,
            int creditCardExpiredMonth,
            string securityCode,
            int installmentCount,
            string paymentAmount,
            string orderNo,
            string ipAddress,
            bool threeDPaymentActive,
            string status,
            string auth,
            string level,
            string txnid,
            string md,
            string pnOrInfo)
        {
            var postPaymentRequest = new PostPaymentRequest
            {
                LanguageId = languageId,
                CurrencyId = currencyId,
                BankId = bankId,
                BankVendorId = bankVendorId,
                CustomerMailAddress = customerMailAddress.ToStringNullSafe(),
                CreditCardHolder = creditCardHolder.ToStringNullSafe(),
                CreditCardNumber = creditCardNumber.ToStringNullSafe(),
                CreditCardExpiredYear = creditCardExpiredYear,
                CreditCardExpiredMonth = creditCardExpiredMonth,
                SecurityCode = securityCode.ToStringNullSafe(),
                InstallmentCount = installmentCount,
                PaymentAmount = paymentAmount.ToFloatNullSafe(),
                OrderNo = orderNo.ToStringNullSafe(),
                IpAddress = ipAddress.ToStringNullSafe(),
                ThreeDPaymentActive = threeDPaymentActive,
                Status = status.ToStringNullSafe(),
                Auth = auth.ToStringNullSafe(),
                Level = level.ToStringNullSafe(),
                Txnid = txnid.ToStringNullSafe(),
                Md = md.ToStringNullSafe(),
                PnOrInfo = pnOrInfo.ToStringNullSafe()
            };

            try
            {
                var postPaymentRequestLog = new PostPaymentRequestV2
                {
                    LanguageId = languageId,
                    CurrencyId = currencyId,
                    BankId = bankId,
                    BankVendorId = bankVendorId,
                    CustomerMailAddress = customerMailAddress.ToStringNullSafe(),
                    CreditCardNumber = MaskCardNumber(creditCardNumber.ToStringNullSafe()),
                    CreditCardExpiredYear = MaskExpire(creditCardExpiredYear.ToStringNullSafe()),
                    CreditCardExpiredMonth = MaskExpire(creditCardExpiredMonth.ToStringNullSafe()),
                    SecurityCode = MaskCvv(securityCode.ToStringNullSafe()),
                    InstallmentCount = installmentCount,
                    PaymentAmount = paymentAmount.ToFloatNullSafe(),
                    OrderNo = orderNo.ToStringNullSafe(),
                    IpAddress = ipAddress.ToStringNullSafe(),
                    ThreeDPaymentActive = threeDPaymentActive,
                    Status = status.ToStringNullSafe(),
                    Auth = auth.ToStringNullSafe(),
                    Level = level.ToStringNullSafe(),
                    Txnid = txnid.ToStringNullSafe(),
                    Md = md.ToStringNullSafe(),
                    PnOrInfo = pnOrInfo.ToStringNullSafe()
                };

                Serilog.Log.Fatal("{@PostPaymentRequest}", postPaymentRequestLog.ToJson());

            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@PaymentMaskError2}", ex.ToJson());
            }

            var result = await _paymentService.PostPayment(postPaymentRequest);

            Serilog.Log.Fatal("{@PostPaymentResponse}", result.ToJson());

            return HttpResult<PostPaymentResponse>.Result(
                data: result.Data as PostPaymentResponse,
                httpResultType: HttpStatusCode.OK,
                success: result.Success,
                message: result.ServiceMessage ?? result.Message);
        }

        [HttpPost("Refund")]
        public async Task<ActionResult<HttpResult<PostPaymentResponse>>> RefundPartial(PostPaymentRefundRequest postPaymentRefundRequest)
        {
            Serilog.Log.Fatal("{@RefundPartialRequest}", postPaymentRefundRequest);
            string ipAddress = await _configurationService.GetConfigurationValueByFieldName<string>("CustomIpAddress");
            postPaymentRefundRequest.IpAddress = ipAddress == "" ? "37.130.115.31" : ipAddress;
            postPaymentRefundRequest.PaymentRefundType = PaymentRefundTypes.Refund;

            var result = await _paymentService.PostPaymentRefund(postPaymentRefundRequest);

            if (result.Success)
                await _paymentService.UpdateReservationRefundAmount(postPaymentRefundRequest);

            return HttpResult<PostPaymentResponse>.Result(
                data: result.Data as PostPaymentResponse,
                httpResultType: HttpStatusCode.OK,
                success: result.Success,
                message: result.Message
                );
        }
        #endregion

        #region v2

        [HttpPost("installments")]
        public async Task<ActionResult> Installments(InstallmentsRequestDto installmentsRequestDto)
        {
            var errorText = "";
            var token = await _reservationTokenService.GetReservationToken(installmentsRequestDto.ReservationToken);
            var language = await _languageService.Get(installmentsRequestDto.LanguageCode);
            var languageId = language != null ? language.Dilid : 1;

            if (JsonConvert.DeserializeObject<ReservationToken>(EncryptionHelper.DecryptAES256(token)) is ReservationToken _reservationToken)
            {
                installmentsRequestDto.PickupLocationId = _reservationToken.PickupLocationId;
                installmentsRequestDto.VendorId = _reservationToken.VendorId;

                var tokenPaymentAmount = _reservationToken.DailyPrice * _reservationToken.RentalDuration;
                var paymentAmount = !string.IsNullOrEmpty(installmentsRequestDto.CouponCode)
                                           ? await CalculatePaymentAmountWithCouponCode(installmentsRequestDto.ReservationToken, tokenPaymentAmount, installmentsRequestDto.CouponCode, languageId)
                                           : tokenPaymentAmount;

                try
                {
                    var brokerApiPaymentSetting = new GetPaymentSettingsRequest()
                    {
                        LanguageId = languageId,
                        BINNumber = installmentsRequestDto.CardNumber.GetEightOrSixCharactersSafe(),
                        AdvancePaymentActive = installmentsRequestDto.AdvancePaymentActive,
                        PaymentAmount = paymentAmount,
                    };
                    var settingsData = await GetPaymentSettings(brokerApiPaymentSetting);
                    if (settingsData != null)
                    {
                        var settings = settingsData;

                        var installmentSupported = await _bankBinCodeService.IsInstallmentSupported((long)Convert.ToDouble(installmentsRequestDto.CardNumber));
                        var locationVendorInstallment =
                            await _locationVendorInstallmentService.GetLocationVendorInstallmentAsync(installmentsRequestDto.PickupLocationId, installmentsRequestDto.VendorId);
                        var locationVendorMaxInstallment =
                            installmentSupported
                            ? locationVendorInstallment?.MaxInstallmentCount ?? 999
                            : 0;

                        #region Banka bilgileri kayıtlı mı?
                        var bank = await _bankService.GetBankByVendorIdAsync(settings.Bank.BankVendorId);
                        if (bank == null || bank.Id == 0)
                        {
                            bank = new Bank
                            {
                                BankId = settings.Bank.BankId,
                                BankVendorId = settings.Bank.BankVendorId,
                                BankName = settings.Bank.BankName,
                                BankDefinition = settings.Bank.BankDefinition,
                                Account = settings.Bank.Account,
                                IBAN = settings.Bank.IBAN,
                                InstallmentActive = settings.Bank.InstallmentActive,
                                ThreeDPaymentActive = settings.Bank.ThreeDPaymentActive,
                                ThreeDPaymentRequired = settings.Bank.ThreeDPaymentRequired,
                                AmexActive = settings.Bank.AmexActive
                            };

                            bank = await _bankService.AddAsync(bank);
                        }
                        #endregion

                        #region Payment settings kaydediliyor
                        var paymentSetting = new PaymentSetting
                        {
                            BankId = bank.Id,
                            ReservationToken = installmentsRequestDto.ReservationToken,
                            BinNumber = installmentsRequestDto.CardNumber,
                            PaymentAmount = paymentAmount.ToString(),
                            AdvencedPaymentActive = installmentsRequestDto.AdvancePaymentActive,
                            CustomerInfo = installmentsRequestDto.CustomerInfo,
                            Code = settings.Code,
                            Message = settings.Message
                        };

                        paymentSetting = await _paymentSettingService.AddAsync(paymentSetting);
                        #endregion

                        #region Taksitler kaydediliyor

                        var installmentList = settings.Installments
                            //.Where(i => i.InstallmentCount <= locationVendorMaxInstallment)
                            .Select(installmentDto => new Installment
                            {
                                PaymentSettingId = paymentSetting.Id,
                                InstallmentCount = installmentDto.InstallmentCount, // Taksit Sayısı
                                InstallmentTotalAmount = installmentDto.InstallmentTotalAmount, // Taksitli Fiyat
                                InstallmentAmount = installmentDto.InstallmentAmount, // Taksit Tutarı
                                InstallmentPercent = installmentDto.Percent, // Taksit Tutarı
                                Comment = installmentDto.Comment, // Taksit İsmi
                                IsActive = installmentDto.InstallmentCount <= locationVendorMaxInstallment // Taksit Lokasyonda veya Tedarikçide aktif mi?
                            })
                            .ToList();
                        if (installmentList.Any())
                        {
                            await _installmentService.AddRangeAsync(installmentList);
                        }
                        #endregion

                        var result = await _paymentSettingService.GetByTokenWithDetails(installmentsRequestDto.ReservationToken);

                        foreach (var installment in result.Installments)
                        {
                            installment.InstallmentCommisionAmout = installment.InstallmentTotalAmount - paymentAmount;
                        }

                        return Ok(
                            new
                            {
                                success = true,
                                title = "",
                                message = "",
                                data = result
                            });
                    }
                }
                catch (Exception ex)
                {
                    errorText = ex.Message;
                }
            }
            return Ok(new
            {
                success = false,
                title = "Hata",
                message = $"Hata alındı. {errorText}",
                data = ""
            });
        }

        [HttpPost("startPayment")]
        public async Task<ActionResult<HttpResult<PostThreeDSecureControlResponse>>> StartPayment(ReservateNowDtoMobile reservateNowDto)
        {
            var labels = await _labelService.GetAllLabelsByLanguageId(reservateNowDto.LanguageId);
            string errorText;
            var language = await _languageService.Get(reservateNowDto.LanguageCode);
            var currency = await _currencyService.Get(reservateNowDto.CurrencyCode);

            reservateNowDto.LanguageId = language?.Dilid ?? 1;
            reservateNowDto.CurrencyId = currency?.Currencyid ?? 1;

            if (string.IsNullOrEmpty(reservateNowDto.AdditionalProductPricePoa))
            {
                reservateNowDto.AdditionalProductPricePoa = "True";
            }
            if (string.IsNullOrEmpty(reservateNowDto.OneWayFeePoa))
            {
                reservateNowDto.OneWayFeePoa = "True";
            }
            if (reservateNowDto?.ReservationToken == null)
            {
                errorText = "Reservation token error";
                return Ok(new
                {
                    data = new
                    {
                        url = "",
                        redirectionUrl = "",
                        paymentCode = ""
                    },
                    success = false,
                    message = $"{errorText}",
                    resultCode = ResultCodes.Error,
                    title = labels.FirstOrDefault(l => l.LabelKodu == "Hata")?.Labeladi
                });
            }
            if (string.IsNullOrEmpty(reservateNowDto.IdentityNumber ?? ""))
            {
                errorText = "Identity number error";
                return Ok(new
                {
                    data = new
                    {
                        url = "",
                        redirectionUrl = "",
                        paymentCode = ""
                    },
                    success = false,
                    message = $"{errorText}",
                    resultCode = ResultCodes.Error,
                    title = labels.FirstOrDefault(l => l.LabelKodu == "Hata")?.Labeladi
                });
            }
            if ((await _blockedMemberService.IsBlocked(reservateNowDto.IdentityNumber)))
            {
                errorText = "Unknown error";
                return Ok(new
                {
                    data = new
                    {
                        url = "",
                        redirectionUrl = "",
                        paymentCode = ""
                    },
                    success = false,
                    message = $"{errorText}",
                    resultCode = ResultCodes.Error,
                    title = labels.FirstOrDefault(l => l.LabelKodu == "Hata")?.Labeladi
                });
            }

            reservateNowDto = await TokenToReservateNowDto(reservateNowDto);
            try
            {
                var paymentSettings = await _paymentSettingService.GetByTokenWithDetails(reservateNowDto.ReservationToken);

                if (paymentSettings is { Id: > 0 })
                {
                    var brokerApiPaymentSetting = new GetPaymentSettingsRequest()
                    {
                        LanguageId = reservateNowDto.LanguageId,
                        BINNumber = reservateNowDto.CreditCardNumber[..6], // Taksit komisyon hatası nedeniyle eklendi 01.11.2024
                        AdvancePaymentActive = reservateNowDto.AdvencedPayment ?? false,
                        PaymentAmount = (float)reservateNowDto.PaidAmount
                    };
                    var settingsData = GetPaymentSettings(brokerApiPaymentSetting).Result;
                    if (settingsData?.Installments?.Count() > 0)
                    {
                        var installmentCount = (reservateNowDto.InstallmentCount ?? 0) is 0 or 1
                            ? 0
                            : reservateNowDto.InstallmentCount;
                        var percent = settingsData?.Installments?.FirstOrDefault(i =>
                            i.InstallmentCount == installmentCount)?.Percent;
                        if (percent != null)
                        {
                            reservateNowDto.InstallmentPercent =
                                (decimal)percent;
                        }
                        else
                        {
                            return Ok(new
                            {
                                data = new
                                {
                                    url = "",
                                    redirectionUrl = "",
                                    paymentCode = ""
                                },
                                success = false,
                                message = $"Seçilen taksit tutarı geçerli değil!",
                                resultCode = ResultCodes.Error
                            });
                        }
                    }

                    reservateNowDto.PaymentCode = 8.CreateUniqueCode();
                    var reCalculatedReservation = await _reservationService.Recalculate(reservateNowDto);

                    if (reCalculatedReservation != null)
                    {
                        reCalculatedReservation.PaymentCode = reservateNowDto.PaymentCode;
                        var reservationDetail = await _reservationDetailService.AddWithDetail(reCalculatedReservation);
                        var redirectionUrl = $"{_configuration.GetValue<String>("AppSettings:ApiBaseUrl")}/Callback/PaymentFinish";

                        var sessionId =
                            await _reservationTokenService.GetReservationTokenSessionId(reservateNowDto.ReservationToken);
                        var serviceResponse = await _extraService.GetExtras(new GetExtrasRequest
                        {
                            ReservationToken = reservateNowDto.ReservationToken,
                            LanguageCode = reservateNowDto.LanguageCode.ToStringNullSafe().ToUpper()
                        });
                        var vehicle = (serviceResponse.Data as GetExtrasResponse).Vehicle;
                        var vendor = await _vendorService.GetVendorById(vehicle.VendorId);

                        var creditCardNo = reservateNowDto.CreditCardNumber.Replace("-", "").Trim();
                        var installmentCount = reCalculatedReservation.InstallmentCount ?? 0;
                        //var installmentPercent = paymentSettings.Installments
                        //    ?.FirstOrDefault(i => i.InstallmentCount == installmentCount)
                        //    ?.InstallmentPercent;
                        var installmentPercent = reservationDetail.ReservationPaymentDetails.FirstOrDefault()
                            ?.InstallmentCommissionAmount;
                        var userDetail = new kolayCAR.Broker.AWS.Models.AwsModels.Kinesis.UserDetailModel
                        {
                            CustomerEmail = reservateNowDto.Email,
                            CustomerPhone = reservateNowDto.PhoneNumber,
                            PaymentType = "cash",
                            PaymentMethod = "Masterpass Ödeme Sistemi",
                            PaymentCard = $"{creditCardNo[..6]}*****{creditCardNo[^4..]}",
                            InstallmentCount = installmentCount,
                            LateCharge = (decimal)(installmentPercent ?? 0),
                            ContactPermission = reservateNowDto.ContactPermission ?? false
                        };

                        var couponDetail = new kolayCAR.Broker.AWS.Models.AwsModels.Kinesis.CouponModel
                        {
                            CouponCode = reCalculatedReservation.CouponCode,
                            CouponName = reCalculatedReservation.CouponCode,
                            CouponAmount = reCalculatedReservation.CouponDiscountAmount ?? 0
                        };

                        #region Cache 
                        //var resultSetUser = await _memoryCacheService.SetUserDetailModel($"{reservateNowDto.ReservationToken}-{reservateNowDto.PaymentCode}", userDetail);
                        //var resultSetCoupon = await _memoryCacheService.SetCouponDetailModel($"{reservateNowDto.ReservationToken}-{reservateNowDto.PaymentCode}-couponDetail", couponDetail); 
                        #endregion

                        var awsResult = await _awsService.PushCheckoutData(vehicle, vendor, userDetail, couponDetail, sessionId, reservateNowDto.PaymentCode, "Get3d", await _agencyService.GetCurrentAgencyType());

                        if ((reCalculatedReservation.PaidAmount ?? 0) <= 0)
                        {
                            var callbackUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/callback/payResult?ReservationToken={reCalculatedReservation.ReservationToken}&PaymentCode={reCalculatedReservation.PaymentCode}&LanguageId={reservateNowDto.LanguageId}&CurrencyId={reservateNowDto.CurrencyId}";

                            return Ok(new
                            {
                                data = new
                                {
                                    url = callbackUrl,
                                    redirectionUrl = redirectionUrl,
                                    paymentCode = reservateNowDto.PaymentCode
                                },
                                success = true,
                                message = "",
                                resultCode = ResultCodes.Success
                            });
                        }
                        else
                        {
                            var callbackUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/callback/payResult?ReservationToken={reCalculatedReservation.ReservationToken}&PaymentCode={reCalculatedReservation.PaymentCode}&LanguageId={reservateNowDto.LanguageId}&CurrencyId={reservateNowDto.CurrencyId}";

                            var brokerApiThreeDSecure = new PostThreeDSecureControlRequest()
                            {
                                LanguageId = reservateNowDto.LanguageId,
                                CurrencyId = reservateNowDto.CurrencyId,
                                BankId = paymentSettings.Bank.BankId,
                                BankVendorId = paymentSettings.Bank.BankVendorId,
                                CustomerMailAddress = reCalculatedReservation.Email,
                                CreditCardHolder = $"{reCalculatedReservation.Name.TrimStart().TrimEnd()} {reCalculatedReservation.Surname.TrimStart().TrimEnd()}",
                                CreditCardNumber = reCalculatedReservation.CreditCardNumber.Replace("-", ""),
                                CreditCardExpiredMonth = reCalculatedReservation.ExpireMonth ?? -1,
                                CreditCardExpiredYear = reCalculatedReservation.ExpireYear ?? -1,
                                SecurityCode = reCalculatedReservation.Cvc,
                                InstallmentCount = reCalculatedReservation.InstallmentCount ?? 0,
                                PaymentAmount = float.Parse((reCalculatedReservation.PaidAmount ?? 0).ToCurrencyString().Replace(".", "")),
                                OrderNo = Guid.NewGuid().ToString(),
                                IpAddress = _httpContextHelper.GetClientIpAddress(),
                                CallbackUrl = callbackUrl
                            };

                            var threeD = await PostThreeDSecureControl(brokerApiThreeDSecure);
                            if (threeD != null && threeD.Data != null && threeD.Message != null && !threeD.Message.Contains("3D Yönlendirmesi sırasında bir hata oluştu"))
                            {
                                try
                                {
                                    var url = $"{_configuration.GetValue<String>("PaymentUrl")}/kolayPAY/3Dsecure.aspx?APIKEY={threeD.Data.APIKey}&APIPASSWORD={threeD.Data.EncryptedAPIPassword}&VENDORID={threeD.Data.EncryptedVendorId}&CONTENT={threeD.Data.Content}&TYPE=9criOgfG8m0uPwGuP9RXqw==";

                                    return Ok(new
                                    {
                                        data = new
                                        {
                                            url = url,
                                            redirectionUrl = redirectionUrl,
                                            paymentCode = reservateNowDto.PaymentCode
                                        },
                                        success = true,
                                        message = "",
                                        resultCode = 200
                                    });
                                }
                                catch (Exception ex)
                                {
                                    // TODO : Loglama Ekle
                                    errorText = ex.Message;
                                }
                            }
                            else
                            {
                                errorText = threeD?.Message ?? "3D Redirection Error";
                            }
                        }
                    }
                    else
                    {
                        errorText = "Reservation details cannot calculated";
                    }
                }
                else
                {
                    errorText = "PaymentSettings cannot load";
                }
            }
            catch (Exception ex)
            {
                // TODO : Loglama Ekle
                errorText = ex.Message;
            }
            return Ok(new
            {
                data = new
                {
                    url = "",
                    redirectionUrl = "",
                    paymentCode = ""
                },
                success = false,
                message = $"{errorText}",
                resultCode = ResultCodes.Error
            });
        }

        [HttpPost("checkresult")]
        public async Task<ActionResult> CheckResult(CheckPaymentResultRequestDto checkPaymentResultRequestDto)
        {
            var result = await _paymentResultService.GetWithResTokenAndPaymentCodeAsync(checkPaymentResultRequestDto);
            var sessionId =
                await _reservationTokenService.GetReservationTokenSessionId(checkPaymentResultRequestDto.ReservationToken);
            var reservationCrytedData = await _reservationTokenService.GetReservationToken(checkPaymentResultRequestDto.ReservationToken);
            var reservationData = reservationCrytedData.DecryptAES256();
            var reservation =
                reservationData.ValidateJson() ?
                    JsonConvert.DeserializeObject<ReservationToken>(reservationData) :
                    Activator.CreateInstance<ReservationToken>();
            var serviceResponse = await _extraService.GetExtras(new GetExtrasRequest
            {
                ReservationToken = checkPaymentResultRequestDto.ReservationToken,
                LanguageCode = reservation.LanguageType.ToStringNullSafe().ToUpper()
            });
            var vehicle = (serviceResponse.Data as GetExtrasResponse).Vehicle;
            var vendor = await _vendorService.GetVendorById(vehicle.VendorId);

            #region User ve Coupon bilgileri alınıyor

            #region Cache yapısı
            //var userDetail = await _memoryCacheService.GetUserDetailModel(
            //    $"{checkPaymentResultRequestDto.ReservationToken}-{checkPaymentResultRequestDto.PaymentCode}");
            //var couponDetail = await _memoryCacheService.GetCouponDetailModel(
            //    $"{checkPaymentResultRequestDto.ReservationToken}-{checkPaymentResultRequestDto.PaymentCode}-couponDetail"); 
            #endregion

            var userDetails =
                await _reservationDetailService.GetByReservationTokenWithDetail(checkPaymentResultRequestDto
                    .ReservationToken);
            var driverInfo = userDetails.ReservationDriverInfos.FirstOrDefault();
            var paymentInfo = userDetails.ReservationPaymentDetails.FirstOrDefault();
            var creditCardNo = paymentInfo?.CreditCardNumber;
            var installmentCount = paymentInfo?.InstallmentCount;
            var installmentCommission = paymentInfo?.InstallmentCommissionAmount;
            var couponCode = userDetails.CouponCode;
            var couponAmount = userDetails.CouponDiscountAmount;

            var userDetail = new kolayCAR.Broker.AWS.Models.AwsModels.Kinesis.UserDetailModel
            {
                CustomerEmail = driverInfo?.Email,
                CustomerPhone = $"{driverInfo?.CountryPhoneCode}{driverInfo?.PhoneNumber}",
                PaymentType = "cash",
                PaymentMethod = "Masterpass Ödeme Sistemi",
                PaymentCard = $"{creditCardNo[..6]}*****{creditCardNo[^4..]}",
                InstallmentCount = installmentCount ?? 0,
                LateCharge = (decimal)(installmentCommission ?? 0),
                ContactPermission = driverInfo?.ContactPermission ?? false
            };

            var couponDetail = new kolayCAR.Broker.AWS.Models.AwsModels.Kinesis.CouponModel
            {
                CouponCode = couponCode,
                CouponName = couponCode,// TODO : Düzeltme yapılacak
                CouponAmount = couponAmount ?? 0
            };
            #endregion

            if (result is { Id: > 0 })
            {
                //var awsResult = await _awsService.PushCheckoutData(vehicle, vendor, userDetail, couponDetail, sessionId, checkPaymentResultRequestDto.PaymentCode, "PaymentSuccess", await _agencyService.GetCurrentAgencyType());

                return Ok(new
                {
                    data = result,
                    success = result.Result,
                    message = result?.Message ?? ""
                });
            }

            var errorModel = new kolayCAR.Broker.AWS.Models.AwsModels.Kinesis.ErrorDto
            {
                ReservationToken = vehicle.ReservationToken,
                PaymentCode = checkPaymentResultRequestDto.PaymentCode,
                Message = result?.Message ?? "",
                SystemMessage = result?.Message ?? "",
                ErrorStage = "8",
                ErrorType = "Internal_System"
            };
            var awsErrorResult = await _awsService.PushErrorData(vehicle, vendor, userDetail, couponDetail, errorModel, sessionId, checkPaymentResultRequestDto.PaymentCode);

            return Ok(new
            {
                data = "",
                success = false,
                message = result?.Message ?? ""
            });
        }

        #endregion

        #region Functions

        [NonAction]
        private async Task<float> CalculatePaymentAmountWithCouponCode(string reservationToken, float tokenPaymentAmount, string couponCode, int languageId)
        {
            if (string.IsNullOrWhiteSpace(reservationToken) || string.IsNullOrWhiteSpace(couponCode))
                return tokenPaymentAmount;

            var currencies = await _currencyService.GetAllCurrencies();

            var token = await _reservationTokenService.GetReservationToken(reservationToken);

            if (JsonConvert.DeserializeObject<ReservationToken>(EncryptionHelper.DecryptAES256(token)) is ReservationToken _reservationToken)
            {

                if (_reservationToken != null)
                {
                    GetCouponDetailsResponseDto couponProcedureParameters = new GetCouponDetailsResponseDto()
                    {
                        CouponCode = couponCode,
                        CustomerMailAddress = "",
                        LanguageId = languageId,
                        CurrencyId = (int)_reservationToken.CurrencyType + 1,
                        VendorId = _reservationToken.VendorId,
                        TotalPrice = (_reservationToken.DailyPrice * _reservationToken.RentalDuration).ToString(),
                        PickupDate = _reservationToken.PickupDateTime.ToString(),
                        ReturnDate = _reservationToken.ReturnDateTime.ToString(),
                        PickupLocationId = _reservationToken.PickupLocationId,
                        ReturnLocationId = _reservationToken.ReturnLocationId,
                        RentalDuration = _reservationToken.RentalDuration,
                        AgencyId = (int)_reservationToken.AgencyId
                    };

                    var couponResult = await _couponService.GetCouponResults(couponProcedureParameters);

                    if (couponResult != null && couponResult.Result == 1)
                    {
                        return tokenPaymentAmount - (float)couponResult.TotalDiscount;
                    }
                }
            }
            return tokenPaymentAmount;
        }

        [NonAction]
        private async Task<GetPaymentSettingsResponse> GetPaymentSettings(GetPaymentSettingsRequest getPaymentSettingsRequest)
        {
            Serilog.Log.Fatal("{@GetPaymentSettingsRequest}", getPaymentSettingsRequest);

            var result = await _paymentService.GetPaymentSettings(getPaymentSettingsRequest);

            Serilog.Log.Fatal("{@GetPaymentSettingsResponse}", result);

            return result.Data as GetPaymentSettingsResponse;
        }

        [NonAction]
        private async Task<ReservateNowDtoMobile> TokenToReservateNowDto(ReservateNowDtoMobile reservateNowDto)
        {
            var token = await _reservationTokenService.GetReservationToken(reservateNowDto.ReservationToken);


            if (JsonConvert.DeserializeObject<ReservationToken>(EncryptionHelper.DecryptAES256(token)) is ReservationToken _reservationToken)
            {
                var tokenPaymentAmount = _reservationToken.DailyPrice * _reservationToken.RentalDuration;
                var paymentAmount = !string.IsNullOrEmpty(reservateNowDto.CouponCode)
                                           ? await CalculatePaymentAmountWithCouponCode(reservateNowDto.ReservationToken, tokenPaymentAmount, reservateNowDto.CouponCode, reservateNowDto.LanguageId)
                                           : tokenPaymentAmount;

                var pickUpLocation = await _context.Location.FirstOrDefaultAsync(l => l.Id == _reservationToken.PickupLocationId);
                var returnLocation = await _context.Location.FirstOrDefaultAsync(l => l.Id == _reservationToken.ReturnLocationId);

                var vehicleClassVendor = await _context.Vehicleclassvendor.FirstOrDefaultAsync(v => v.Vendorid == _reservationToken.VendorId && v.Apivehicleclasscode == _reservationToken.VehicleCode);

                if (vehicleClassVendor == null)
                {
                    vehicleClassVendor = await _context.Vehicleclassvendor.FirstOrDefaultAsync(v => v.Vendorid == _reservationToken.VendorId && v.Apivehicleclasscode == _reservationToken.VehicleId.ToString());
                }

                var vehicle = await _context.Vehicleclass.FirstOrDefaultAsync(l => l.Vehicleclassid == vehicleClassVendor.Vehicleclassid);
                var vehicleBrand = await _context.Vehiclebrand.FirstOrDefaultAsync(b => b.Brandid == vehicle.Brandid);
                var vehicleModel = await _context.Vehiclemodel.FirstOrDefaultAsync(b => b.Modelid == vehicle.Modelid);
                var vehicleName = await _context.Vehicleclasslang.FirstOrDefaultAsync(l => l.Langid == reservateNowDto.LanguageId && l.Vehicleclassid == _reservationToken.VehicleId);
                var vehicleFuel = await _context.Vehiclefuellang.FirstOrDefaultAsync(l => l.Fuelid == vehicle.Fuelid && l.Langid == reservateNowDto.LanguageId);
                var vehicleTransmission = await _context.Vehicletransmissionlang.FirstOrDefaultAsync(l => l.Transmissionid == vehicle.Transmissionid && l.Langid == reservateNowDto.LanguageId);
                var vehiclePersonLang = await _context.Vehiclepersonlang.FirstOrDefaultAsync(l => l.Personid == vehicle.Personid && l.Langid == reservateNowDto.LanguageId);
                var vehicleBaggegeLang = await _context.Vehiclebaggagelang.FirstOrDefaultAsync(l => l.Baggageid == vehicle.Baggageid && l.Langid == reservateNowDto.LanguageId);
                var vehicleCategoryLang = await _context.Vehiclecategorylang.FirstOrDefaultAsync(l => l.Categoryid == vehicle.Categoryid && l.Langid == reservateNowDto.LanguageId);
                var vehicleType = await _context.Vehicletypelang.FirstOrDefaultAsync(l => l.Typeid == vehicle.Typeid && l.Langid == reservateNowDto.LanguageId);

                reservateNowDto.TotalPrice = (decimal)tokenPaymentAmount;
                reservateNowDto.PaidAmount = (decimal)paymentAmount;
                reservateNowDto.VendorId = _reservationToken.VendorId;
                reservateNowDto.AgencyId = (int)_reservationToken.AgencyId;
                reservateNowDto.PickupLocationId = _reservationToken.PickupLocationId;
                reservateNowDto.PickupLocation = pickUpLocation.Locationname;
                reservateNowDto.ReturnLocationId = _reservationToken.ReturnLocationId;
                reservateNowDto.ReturnLocation = returnLocation.Locationname;
                reservateNowDto.PickupDate = _reservationToken.PickupDateTime.ToString();
                reservateNowDto.ReturnDate = _reservationToken.ReturnDateTime.ToString();
                reservateNowDto.VendorName = _reservationToken.APIVendorName;
                reservateNowDto.ServiceCharge = (decimal)_reservationToken.ServiceCharge;
                reservateNowDto.FullCredit = _reservationToken.APIFullCredit;
                reservateNowDto.VehicleBrandName = vehicleBrand?.Brandname;
                reservateNowDto.VehicleModelName = vehicleModel?.Modelname;
                reservateNowDto.VehicleName = vehicleName?.Vehicleclassname;
                reservateNowDto.FuelName = vehicleFuel?.Fuelname;
                reservateNowDto.TransmissionName = vehicleTransmission?.Transmissionname;
                reservateNowDto.Person = (int)vehiclePersonLang?.Personid;
                reservateNowDto.PersonName = vehiclePersonLang?.Personname;
                reservateNowDto.Baggage = vehicleBaggegeLang.Baggageid;
                reservateNowDto.BaggageName = vehicleBaggegeLang?.Baggagename;
                reservateNowDto.CategoryName = vehicleCategoryLang?.Categoryname;
                reservateNowDto.TypeName = vehicleType?.Typename;
                reservateNowDto.DailyPrice = _reservationToken.DailyPrice.ToString();
                reservateNowDto.Deposit = _reservationToken.DepositPrice.ToString();
                reservateNowDto.OneWayFee = _reservationToken.OneWayFee.ToString();
                reservateNowDto.RentalDuration = _reservationToken.RentalDuration.ToString();
                reservateNowDto.MinimumAge = _reservationToken.VendorMinimumDriverAge.ToString();
                reservateNowDto.MinimumLicenseAge = _reservationToken.VendorMinimumDrivingLicenseAge.ToString();
            }

            return reservateNowDto;
        }

        [NonAction]
        private async Task<PostPaymentResponseBase> PostPayment(PaymentDto paymentDto)
        {
            var postPaymentRequest = new PostPaymentRequest
            {
                LanguageId = paymentDto.languageId,
                CurrencyId = paymentDto.currencyId,
                BankId = paymentDto.bankId,
                BankVendorId = paymentDto.bankVendorId,
                CustomerMailAddress = paymentDto.customerMailAddress.ToStringNullSafe(),
                CreditCardHolder = paymentDto.creditCardHolder.ToStringNullSafe(),
                CreditCardNumber = paymentDto.creditCardNumber.ToStringNullSafe(),
                CreditCardExpiredYear = paymentDto.creditCardExpiredYear,
                CreditCardExpiredMonth = paymentDto.creditCardExpiredMonth,
                SecurityCode = paymentDto.securityCode.ToStringNullSafe(),
                InstallmentCount = paymentDto.installmentCount,
                PaymentAmount = paymentDto.paymentAmount.ToFloatNullSafe(),
                OrderNo = paymentDto.orderNo.ToStringNullSafe(),
                IpAddress = paymentDto.ipAddress.ToStringNullSafe(),
                ThreeDPaymentActive = paymentDto.threeDPaymentActive,
                Status = paymentDto.status.ToStringNullSafe(),
                Auth = paymentDto.auth.ToStringNullSafe(),
                Level = paymentDto.level.ToStringNullSafe(),
                Txnid = paymentDto.txnid.ToStringNullSafe(),
                Md = paymentDto.md.ToStringNullSafe(),
                PnOrInfo = paymentDto.pnOrInfo.ToStringNullSafe()
            };

            Serilog.Log.Fatal("{@PostPaymentRequest}", postPaymentRequest);

            var result = await _paymentService.PostPayment(postPaymentRequest);

            Serilog.Log.Fatal("{@PostPaymentResponse}", result);


            return new PostPaymentResponseBase
            {
                Success = result.Success,
                PostPaymentResponse = result.Data as PostPaymentResponse
            };
        }

        [NonAction]
        private async Task<string> CheckReservationModel(ReservationDetail reservationDetail, PaymentTypes paymentTypes, int languageId)
        {
            var labels = await _context.Label.Where(l => l.Dilid == languageId).ToListAsync();
            string message = null;

            #region Token Var mı?
            if (!CheckTokenExists(reservationDetail.ReservationToken))
            {
                //CreateLog(LogTypes.Error, "ReservateNow [POST]", "CheckTokenExists - Token bulunamadı!");
                return "CheckTokenExists - Token bulunamadı!";
            }
            #endregion

            var reservationModel = await CreateReservationModel(reservationDetail, paymentTypes);

            #region Yorumda

            //#region Veri kontrolü sağlanıyor
            //try
            //{
            //    var resultData = await GetSummary(reservationModel.ExtraList, reservationModel.ReservationToken, reservationModel.MemberId, reservationModel.CouponCode, reservationModel.LanguageCode, reservationModel.PaidAmount, reservationModel.HighAmountDiscountActive);

            //    if (resultData == null)
            //    {
            //        // TODO: Hata oluştu
            //    }
            //    else
            //    {
            //        bool
            //            isError = false,
            //            tutarsizVeri = false;

            //        var result = resultData;

            //        #region Kupon Kontrolü
            //        switch (result.CouponUsageResultType)
            //        {
            //            case BrokerApiCouponUsageResultTypes.None:
            //                // TODO: Kupon uygulama başarılı
            //                break;
            //            case BrokerApiCouponUsageResultTypes.GeneralError:
            //                // TODO: Kupon uygulama başarısız
            //                isError = true;
            //                message = labels.FirstOrDefault(l => l.LabelKodu == "KuponKoduGecersiz")?.Labeladi;
            //                break;
            //            case BrokerApiCouponUsageResultTypes.GreaterThanPaymentAmount:
            //                // TODO: Ödeme tutarından büyük kupon kullanımı
            //                break;
            //            case BrokerApiCouponUsageResultTypes.GreaterThanTotalAmount:
            //                // TODO: Toplam tutarından büyük kupon kullanımı
            //                break;
            //        }
            //        #endregion

            //        #region Tutarsız Veri Kontrolü
            //        if (result.Extras != null)
            //        {
            //            #region Değiştirilmiş veri kontrolü
            //            foreach (var selectedExtra in reservationDetail.ReservationSelectedExtras)
            //            {
            //                var orginalExtra = result.Extras.Where(e => e.ExtraId == selectedExtra.Id).FirstOrDefault();
            //                var isEqual = CheckEquality(selectedExtra, orginalExtra, result.Vehicle.RentalDuration);

            //                if (!isEqual)
            //                {
            //                    tutarsizVeri = true;
            //                    break;
            //                }
            //            }
            //            if (tutarsizVeri) // Tutarsız veri tespit edildi
            //            {
            //                isError = true;
            //                message = "TutarsizVeriBulundu";
            //            }
            //            #endregion
            //        }
            //        else
            //        {
            //            #region Extra Yok
            //            // TODO: Extra gelmemişse durumu yazılacak
            //            #endregion
            //        }
            //        #endregion

            //        #region Ödeme Tipi Kontrolü
            //        // TODO : BU kısma odaklan!!!!
            //        #endregion

            //        if (!isError) // TODO: Hata mesajlarını özelleştir
            //        {
            //            return "";
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    //CreateLog(LogTypes.Error, "ReservateNow [POST] - CheckExtras", ex.Message);
            //}
            //#endregion 
            #endregion

            return message;
        }

        [NonAction]
        private async Task<GetSummaryResponseDto> GetSummary(
            string extraList,
            string reservationToken,
            int? memberId,
            string couponCode,
            string languageCode,
            float paidAmount,
            bool highAmountDiscountActive
            )
        {
            if (string.IsNullOrWhiteSpace(reservationToken))
                return null;

            //var reservationTokenObj = await _reservationStepsService.GetReservationToken(reservationToken);
            var reservationTokenObj = await _resTokenService.GetReservationTokenByUniqueId(reservationToken);

            var getSummaryRequest = new GetSummaryRequest
            {
                ExtraList = extraList,
                ReservationToken = reservationToken,
                MemberId = memberId,
                CouponCode = couponCode,
                LanguageCode = languageCode.ToStringNullSafe().ToUpper(),
                PaidAmount = paidAmount,
                HighAmountDiscountActive = highAmountDiscountActive
            };

            var getExtrasRequest = new GetExtrasRequest
            {
                ReservationToken = reservationToken.TrimNullSafe(),
                LanguageCode = getSummaryRequest.LanguageCode,
                CurrencyCode = reservationTokenObj.CurrencyType.ToString()
            };

            var extraServiceResponse = !string.IsNullOrEmpty(getSummaryRequest.ExtraList) ?
                        await _extraService.GetExtras(getExtrasRequest) :
                        new ServiceResponseBase
                        {
                            Success = true,
                            Data = new GetExtrasResponse
                            {
                                Extras = new List<Extra>(),
                            }
                        };

            var getExtrasResponse = extraServiceResponse.Data as GetExtrasResponse;

            var serviceResponse = await _summaryService.GetSummary(getSummaryRequest, getExtrasResponse);

            if (serviceResponse.Success)
            {
                GetSummaryResponseRestricted restrictedServiceResponse = new GetSummaryResponseRestricted();
                if (_userRole == UserRoles.External)
                {
                    var summaryVehicle = serviceResponse.Data as GetSummaryResponse;
                    restrictedServiceResponse.Vehicle = _agencyService.ChechAgencyRestricted<Models.Dtos.RestrictedVehicle>(summaryVehicle.Vehicle);
                    restrictedServiceResponse.Extras = _agencyService.ChechAgencyRestrictedList<ReservationExtra, RestrictedReservationExtra>(summaryVehicle.Extras as List<ReservationExtra>);
                }

                object getSummaryResponse = _userRole == UserRoles.External ? restrictedServiceResponse : serviceResponse.Data;

                return getSummaryResponse as GetSummaryResponseDto;
            }

            return null;
        }

        [NonAction]
        private bool CheckEquality(ReservationSelectedExtra selectedExtra, BrokerApiExtraDto orginalExtra, int rentalDuration)
        {
            var result = true;
            if (orginalExtra == null) return true;
            if (rentalDuration != selectedExtra.RentalDuration) result = false;
            if (orginalExtra.ExtraRentalType != selectedExtra.ExtraRentalType) result = false;
            if (Math.Abs((decimal)orginalExtra.Price - selectedExtra.Price) > 0) result = false;
            if (orginalExtra.ExtraName != selectedExtra.Name) result = false;
            return result;
        }

        [NonAction]
        private bool CheckTokenExists(string reservationToken)
        {
            return _reservationTokenService.GetReservationTokenExists(reservationToken).Result;
        }

        [NonAction]
        private async Task<ReservationsRequestDto> CreateReservationModel(ReservationDetail reservationDetail, PaymentTypes paymentType, PaymentSetting paymentSetting = null)
        {
            if (!Boolean.TryParse(_parameterService.GetParameterValue("HighAmountDiscountActive"), out bool highAmountDiscountActive))
            {
                highAmountDiscountActive = false;
            }

            return reservationDetail.ToRequestModel(paymentType, paymentSetting, highAmountDiscountActive);
        }

        [NonAction]
        private async Task<HttpResult<PostThreeDSecureControlResponse>> PostThreeDSecureControl(PostThreeDSecureControlRequest secureDto)
        {
            Serilog.Log.Fatal("{@PostThreeDSecureControlRequest}", secureDto);

            var result = await _paymentService.PostThreeDSecureControl(secureDto);

            Serilog.Log.Fatal("{@PostThreeDSecureControlResponse}", result);

            return HttpResult<PostThreeDSecureControlResponse>.Result(
                data: result.Data as PostThreeDSecureControlResponse,
                httpResultType: HttpStatusCode.OK,
                success: result.Success,
                message: result.ServiceMessage ?? result.Message);
        }

        #endregion

        public static string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 10)
                return "************";

            var first6 = cardNumber.Substring(0, 6);
            var last4 = cardNumber.Substring(cardNumber.Length - 4);

            return $"{first6}******{last4}";
        }
        public static string MaskExpire(string value)
        {
            return string.IsNullOrEmpty(value) ? "" : new string('*', value.Length);
        }
        public static string MaskCvv(string cvv)
        {
            return string.IsNullOrEmpty(cvv) ? "" : "***";
        }
    }
}
