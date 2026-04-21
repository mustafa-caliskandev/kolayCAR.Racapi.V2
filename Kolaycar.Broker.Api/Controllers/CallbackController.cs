using KolayCAR.Broker.API.Extensions;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Models.Dtos;
using KolayCAR.Broker.API.Models.PaymentDto;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        private readonly ILabelService _labelService;
        private readonly IParameterService _parameterService;
        private readonly IPaymentResultService _paymentResultService;
        private readonly IPaymentService _paymentService;
        private readonly IPaymentSettingService _paymentSettingService;
        private readonly IReservationDetailService _reservationDetailService;
        private readonly IReservationPaymentDetailService _reservationPaymentDetailService;
        private readonly IReservationTokenService _reservationTokenService;

        public CallbackController(
            ILabelService labelService,
            IParameterService parameterService,
            IPaymentService paymentService,
            IPaymentResultService paymentResultService,
            IPaymentSettingService paymentSettingService,
            IReservationDetailService reservationDetailService,
            IReservationPaymentDetailService reservationPaymentDetailService,
            IReservationTokenService reservationTokenService)
        {
            _labelService = labelService;
            _parameterService = parameterService;
            _paymentService = paymentService;
            _paymentResultService = paymentResultService;
            _paymentSettingService = paymentSettingService;
            _reservationDetailService = reservationDetailService;
            _reservationPaymentDetailService = reservationPaymentDetailService;
            _reservationTokenService = reservationTokenService;
        }

        #region Payment Result Callback Events
        [HttpGet("payResult")]
        public async Task<ActionResult> Callback([FromQuery] CallbackRequestDto callbackRequestDto)
        {
            if (string.IsNullOrEmpty(callbackRequestDto.PaymentCode) || string.IsNullOrEmpty(callbackRequestDto.ReservationToken))
            {
                return Ok(new
                {
                    Success = false
                });
            }

            var status = Request.Query["Status"].ToString()?.Replace("%*2", "#").Replace("%*3", "/").Replace(" ", "+");
            var auth = Request.Query["Auth"].ToString()?.Replace("%*2", "#").Replace("%*3", "/").Replace(" ", "+");
            var level = Request.Query["Level"].ToString()?.Replace("%*2", "#").Replace("%*3", "/").Replace(" ", "+");
            var txnId = Request.Query["Txnid"].ToString()?.Replace("%*2", "#").Replace("%*3", "/").Replace(" ", "+");
            var md = Request.Query["Md"].ToString()?.Replace("%*2", "#").Replace("%*3", "/").Replace(" ", "+");

            try
            {
                var (checkPaymenResult, errorMessage) = await CheckPayment(new PaymentControlDto
                {
                    ReservationToken = callbackRequestDto.ReservationToken,
                    PaymentCode = callbackRequestDto.PaymentCode,
                    Status = status,
                    Auth = auth,
                    Level = level,
                    TxnId = txnId,
                    Md = md,
                    MdText = callbackRequestDto.MdText,
                    Successfully = callbackRequestDto.Successfully,
                    LanguageId = callbackRequestDto.LanguageId,
                    CurrencyId = callbackRequestDto.CurrencyId,
                });

                var entity = new Models.PaymentResult
                {
                    ResToken = callbackRequestDto.ReservationToken,
                    PaymentCode = callbackRequestDto.PaymentCode,
                    Result = checkPaymenResult,
                    Message = errorMessage,
                    Date = DateTime.Now,
                };
                var addPaymentResult = await _paymentResultService.AddAsync(entity);

                return RedirectToAction("PaymentFinish");
            }
            catch (Exception)
            {
                return Ok(new
                {
                    Success = false
                });
                throw;
            }
        }

        [HttpGet("PaymentFinish")]
        public IActionResult PaymentFinish()
        {
            return Ok(new
            {
                Success = true
            });
        }

        [NonAction]
        private async Task<(bool, string)> CheckPayment(PaymentControlDto paymentControlDto)
        {
            var labels = await _labelService.GetAllLabelsByLanguageId(paymentControlDto.LanguageId);
            #region Veri kontrolü sağlanıyor
            var errorText = paymentControlDto.MdText;
            var reservationDetailModel = await _reservationDetailService.GetByReservationTokenWithDetail(paymentControlDto.ReservationToken);
            var paymentSetting = await _paymentSettingService.GetByTokenWithDetails(paymentControlDto.ReservationToken);
            var paymentType = (paymentSetting.AdvencedPaymentActive ?? false)
                ? PaymentTypes.AdvancePayment
                : PaymentTypes.PayAll;

            var checkSummaryMessage = await CheckReservationModel(reservationDetailModel, paymentType, paymentControlDto.LanguageId);
            if (!string.IsNullOrEmpty(checkSummaryMessage))
            {
                return (false, checkSummaryMessage);
            }
            #endregion

            #region Ödeme Bilgileri alınıyor
            var paymentDto = new Models.PaymentDto.PaymentDto
            {
                languageId = paymentControlDto.LanguageId,
                currencyId = paymentControlDto.CurrencyId,
                auth = paymentControlDto.Auth,
                level = paymentControlDto.Level,
                status = paymentControlDto.Status, // PARAM1 (Zorunlu)
                txnid = paymentControlDto.TxnId, // PARAM4 (Zorunlu)
                md = paymentControlDto.Md, // PARAM5 (Zorunlu)
                pnOrInfo = paymentControlDto.MdText, // PARAM6 (Zorunlu)
                threeDPaymentActive = true,

                bankVendorId = paymentSetting.Bank.BankVendorId,
                bankId = paymentSetting.Bank.BankId,
                orderNo = paymentSetting.Message,

                creditCardExpiredMonth = reservationDetailModel.ReservationPaymentDetails.FirstOrDefault().ExpireMonth,
                creditCardExpiredYear = reservationDetailModel.ReservationPaymentDetails.FirstOrDefault().ExpireYear,
                creditCardHolder =
                    reservationDetailModel.ReservationPaymentDetails.FirstOrDefault().CreditCardOwnerName,
                creditCardNumber = reservationDetailModel.ReservationPaymentDetails.FirstOrDefault().CreditCardNumber,
                paymentAmount = reservationDetailModel.ReservationPaymentDetails.FirstOrDefault().PaidAmount.ToString(),
                securityCode = reservationDetailModel.ReservationPaymentDetails.FirstOrDefault().Cvc,
                customerMailAddress = reservationDetailModel.ReservationDriverInfos.FirstOrDefault().Email,
                installmentCount = reservationDetailModel.ReservationPaymentDetails.FirstOrDefault().InstallmentCount ?? 0,
                ipAddress = reservationDetailModel.IpAddress,
            };

            var paymentResultData = PostPayment(paymentDto).Result;
            var paymentResult =
                            paymentResultData.PostPaymentResponse != null ?
                            paymentResultData :
                            Activator.CreateInstance<PostPaymentResponseBase>();

            //await _telegramBot.SendMessage($"Site: {_httpContextHelper._httpContextAccessor.HttpContext.Request.Host}, Payment Result Check, Model: {JsonConvert.SerializeObject(paymentResultData)} Result: {JsonConvert.SerializeObject(paymentResultData.Data)}", TelegramMessageType.Info);

            var resultCode = paymentResult?.PostPaymentResponse?.PaymentResult?.LogResultNumber ?? "-1";
            var errorMessage = labels.FirstOrDefault(l => l.LabelKodu == $"PaymentErrorCodes.Iyzico.{resultCode}")?.Labeladi == $"PaymentErrorCodes.Iyzico.{resultCode}"
                            ? (string.IsNullOrEmpty(paymentResult?.PostPaymentResponse?.Message ?? "") ? $"{labels.FirstOrDefault(l => l.LabelKodu == "Payment.ErrorText")?.Labeladi}" : $"{paymentResult?.PostPaymentResponse?.Message}")
                            : labels.FirstOrDefault(l => l.LabelKodu == $"PaymentErrorCodes.Iyzico.{resultCode}")?.Labeladi;
            var resultMessage = resultCode != "0" && !string.IsNullOrEmpty(resultCode)
                ? errorMessage
                : (string.IsNullOrEmpty(paymentResult?.PostPaymentResponse?.Message ?? "")
                    ? $"{labels.FirstOrDefault(l => l.LabelKodu == "Payment.ErrorText")?.Labeladi}"
                    : $"{paymentResult?.PostPaymentResponse?.Message}");

            #region ReservationPaymentDetail Update

            try
            {
                await _reservationPaymentDetailService.ResultUpdate(new PaymentResultUpdateDto()
                {
                    ReservationToken = paymentControlDto.ReservationToken,
                    PaymentResultCode = resultCode,
                    PaymentResultMessage = resultMessage,
                    BankResultMessage = paymentResult?.PostPaymentResponse?.PaymentResult?.LogErrorCode ?? "",
                    ProvisionNumber = paymentResult?.PostPaymentResponse?.PaymentResult?.ProvisionNumber ?? "--"
                });
            }
            catch (Exception ex)
            {
                // TODO: LOGLAMA YAPILACAK
                // await _telegramBot.SendMessage($"Site: {_httpContextHelper._httpContextAccessor.HttpContext.Request.Host}, PaymentResultUpdateDto, Error Message: {ex.Message}", TelegramMessageType.Error);
            }

            try
            {
                paymentSetting.ProvisionNumber = paymentResult?.PostPaymentResponse?.PaymentResult.ProvisionNumber ?? "";
                await _paymentSettingService.UpdateAsync(paymentSetting.Id, paymentSetting);
            }
            catch (Exception ex)
            {
                // TODO : LOGLAMA YAPILACAK
                // await _telegramBot.SendMessage($"Site: {_httpContextHelper._httpContextAccessor.HttpContext.Request.Host}, PaymentSettingUpdate, Error Message: {ex.Message}", TelegramMessageType.Error);
            }

            #endregion

            return (paymentResult.Success, resultMessage);
            #endregion
        }

        [NonAction]
        private async Task<string> CheckReservationModel(ReservationDetail reservationDetail, PaymentTypes paymentTypes, int languageId)
        {
            var labels = await _labelService.GetAllLabelsByLanguageId(languageId);
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
        private async Task<PostPaymentResponseBase> PostPayment(Models.PaymentDto.PaymentDto paymentDto)
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
        #endregion
    }
}
