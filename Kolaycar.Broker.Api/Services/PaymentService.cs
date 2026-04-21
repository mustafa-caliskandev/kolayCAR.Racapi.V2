using KolayCAR.Broker.API.Extensions;
using KolayCAR.Broker.API.Mappers.KolayCAR;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using kolayCARPayment;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IPaymentService
    {
        Task<ServiceResponseBase> GetPaymentSettings(GetPaymentSettingsRequest getPaymentSettingsRequest);
        Task<ServiceResponseBase> PostThreeDSecureControl(PostThreeDSecureControlRequest postThreeDSecureControlRequest);
        Task<ServiceResponseBase> PostPayment(PostPaymentRequest postPaymentRequest);
        Task<ServiceResponseBase> PostPaymentRefund(PostPaymentRefundRequest postPaymentRefundRequest);
        Task UpdateReservationRefundAmount(PostPaymentRefundRequest postPaymentRefundRequest);
    }

    public class PaymentService : IPaymentService
    {
        private readonly BrokerContext _context;
        private readonly ServiceSoapClient _kolayCARService;
        private readonly IConfigurationService _configurationService;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public PaymentService(
            IConfiguration configuration,
            BrokerContext context, IConfigurationService configurationService, IWebHostEnvironment env)
        {
            _context = context;
            _kolayCARService = new ServiceSoapClient(ServiceSoapClient.EndpointConfiguration.ServiceSoap);
            _configurationService = configurationService;
            _env = env;
            _configuration = configuration;

            var paymentUrl =
                !string.IsNullOrEmpty(configuration?.GetSectionValueString("PaymentUrl"))
                    ? $"{configuration.GetSectionValueString("PaymentUrl")}/service.asmx"
                    : "https://trnx.kolaycar.com/service.asmx";

            _kolayCARService.Endpoint.Address = new EndpointAddress(paymentUrl);
            //_telegramBot.SendMessage($"Payment Url Set: {paymentUrl}", TelegramMessageType.Error).ConfigureAwait(false);
        }

        public async Task<ServiceResponseBase> GetPaymentSettings(GetPaymentSettingsRequest getPaymentSettingsRequest)
        {
            if (getPaymentSettingsRequest != null)
            {
                var configurations = await _configurationService.GetConfigurations();

                SetLocalConfigurations(configurations);

                if (configurations != null)
                {
                    var getPaymentSettingsResult = await _kolayCARService.GET_PAYMENT_SETTINGSAsync(
                        configurations.KolayCARPaymentAPIKey,
                        configurations.KolayCARPaymentAPIPassword,
                        ((LanguageTypes)(getPaymentSettingsRequest.LanguageId - 1)).ToString(),
                        configurations.KolayCARPaymentAPIVendorId.ToStringNullSafe(),
                        string.Empty,
                        getPaymentSettingsRequest.BINNumber,
                        getPaymentSettingsRequest.AdvancePaymentActive,
                        getPaymentSettingsRequest.PaymentAmount.ToString()
                    );

                    Serilog.Log.Fatal("{@KolayCARGetPaymentSettingsResponseObject}", getPaymentSettingsResult.Body.GET_PAYMENT_SETTINGSResult);

                    var getPaymentSettingsResponseObject = JsonConvert.DeserializeObject<KolayCARResponseBase>(getPaymentSettingsResult.Body.GET_PAYMENT_SETTINGSResult);

                    return new ServiceResponseBase
                    {
                        Success = getPaymentSettingsResponseObject.RETURNCODE == 0,
                        Message = getPaymentSettingsResponseObject.MESSAGE,
                        ServiceMessage = getPaymentSettingsResponseObject.MESSAGE,
                        ServiceCode = getPaymentSettingsResponseObject.RETURNCODE.ToStringNullSafe(),
                        Data = new GetPaymentSettingsResponse
                        {
                            Message = getPaymentSettingsResponseObject.MESSAGE,
                            Code = getPaymentSettingsResponseObject.RETURNCODE.ToStringNullSafe(),
                            Bank = getPaymentSettingsResponseObject.BANK?.FirstOrDefault().Map(),
                            Installments = getPaymentSettingsResponseObject.INSTALLMENTS.Map(),
                        }
                    };
                }
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Eksik parametre!"
            };
        }

        public async Task<ServiceResponseBase> PostPayment(PostPaymentRequest postPaymentRequest)
        {
            if (postPaymentRequest != null)
            {
                var configurations = await _configurationService.GetConfigurations();

                SetLocalConfigurations(configurations);

                if (configurations != null)
                {
                    var postPaymentResult = await _kolayCARService.POST_PAYMENTAsync(
                        ///*configurations.KolayCARPaymentAPIKey,*/ "Ss4A3CYBHY4mUobwftsqpm8FJ4VsaYBlcW2zwQmBBD+aJWUaFcEA/WHRc2E6rYnF", //payment servisinde broker için bu API static olarak tanımlandı
                        configurations.KolayCARPaymentAPIKey,
                        configurations.KolayCARPaymentAPIPassword,
                        ((LanguageTypes)(postPaymentRequest.LanguageId - 1)).ToString(),
                        ((CurrencyTypes)(postPaymentRequest.CurrencyId - 1)).ToString(),
                        configurations.KolayCARPaymentAPIVendorId.ToStringNullSafe(),
                        string.Empty,
                        postPaymentRequest.BankVendorId.ToString(),
                        postPaymentRequest.CustomerMailAddress,
                        postPaymentRequest.CreditCardHolder,
                        postPaymentRequest.CreditCardNumber.ToString().Replace("-", " "),
                        postPaymentRequest.CreditCardExpiredYear.ToString(),
                        postPaymentRequest.CreditCardExpiredMonth.ToString(),
                        postPaymentRequest.SecurityCode,
                        postPaymentRequest.InstallmentCount.ToString(),
                        postPaymentRequest.PaymentAmount.ToString(),
                        postPaymentRequest.OrderNo,
                        postPaymentRequest.IpAddress,
                        postPaymentRequest.ThreeDPaymentActive,
                        postPaymentRequest.Status,
                        postPaymentRequest.Auth,
                        postPaymentRequest.Level,
                        postPaymentRequest.Txnid,
                        postPaymentRequest.Md,
                        postPaymentRequest.PnOrInfo,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty);

                    Serilog.Log.Fatal("{@KolayCARPostPaymentResponseObject}", postPaymentResult.Body.POST_PAYMENTResult);

                    var postPaymentResponseObject = JsonConvert.DeserializeObject<KolayCARResponseBase>(postPaymentResult.Body.POST_PAYMENTResult);

                    return new ServiceResponseBase
                    {
                        Success = postPaymentResponseObject.RETURNCODE == 0,
                        Message = postPaymentResponseObject.MESSAGE,
                        ServiceMessage = postPaymentResponseObject.MESSAGE,
                        ServiceCode = postPaymentResponseObject.RETURNCODE.ToStringNullSafe(),
                        Data = new PostPaymentResponse
                        {
                            Message = postPaymentResponseObject.MESSAGE,
                            Code = postPaymentResponseObject.RETURNCODE.ToStringNullSafe(),
                            PaymentResult = postPaymentResponseObject.PAYMENT != null && postPaymentResponseObject.PAYMENT.Count > 0 ? postPaymentResponseObject.PAYMENT[0].Map() : null,
                        }
                    };

                }
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Eksik parametre!"
            };
        }

        public async Task<ServiceResponseBase> PostThreeDSecureControl(PostThreeDSecureControlRequest postThreeDSecureControlRequest)
        {
            if (postThreeDSecureControlRequest != null)
            {
                var configurations = await _configurationService.GetConfigurations();

                SetLocalConfigurations(configurations);

                if (configurations != null)
                {
                    var baseUrl = !string.IsNullOrEmpty(_configuration?.GetSectionValueString("PaymentTokenUrl"))
                                                                    ? $"{_configuration.GetSectionValueString("PaymentTokenUrl")}"
                                                                    : "https://trngate.kolaycar.com/";

                    var _httpManager = new HttpManager(baseUrl);

                    DateTime date = DateTime.Now;
                    string secretKey = GenerateSharedSecret(date.Year, date.Month);

                    var token = await _httpManager.PostAsyncWithModel<KolayCarAuthRequest, KolayCarAuthResponse>(
                        requestPath: "/api/auth/token",
                        entity: new KolayCarAuthRequest
                        {
                            ApiKey = configurations.KolayCARPaymentAPIKey,
                            Password = configurations.KolayCARPaymentAPIPassword,
                            Payload = new Payload
                            {
                                IssuedAt = date,
                                SharedSecret = secretKey
                            },
                            source = "Broker"
                        }
                    );

                    if (string.IsNullOrEmpty(token?.accessToken))
                    {
                        return new ServiceResponseBase
                        {
                            Success = false,
                            Message = "KolayCar payment token alınamadı!"
                        };
                    }


                    var postThreeDSecureControlResult = await _kolayCARService.POST_3D_SECURE_CONTROLAsync(
                        configurations.KolayCARPaymentAPIKey,
                        configurations.KolayCARPaymentAPIPassword,
                        ((LanguageTypes)(postThreeDSecureControlRequest.LanguageId - 1)).ToString(),
                        ((CurrencyTypes)(postThreeDSecureControlRequest.CurrencyId - 1)).ToString(),
                        configurations.KolayCARPaymentAPIVendorId.ToStringNullSafe(),
                        string.Empty,
                        postThreeDSecureControlRequest.BankId.ToString(),
                        postThreeDSecureControlRequest.BankVendorId.ToString(),
                        postThreeDSecureControlRequest.CustomerMailAddress,
                        postThreeDSecureControlRequest.CreditCardHolder,
                        postThreeDSecureControlRequest.CreditCardNumber.ToString(),
                        postThreeDSecureControlRequest.CreditCardExpiredYear.ToString(),
                        postThreeDSecureControlRequest.CreditCardExpiredMonth.ToString(),
                        postThreeDSecureControlRequest.SecurityCode,
                        postThreeDSecureControlRequest.InstallmentCount.ToString(),
                        postThreeDSecureControlRequest.PaymentAmount.ToString(),
                        postThreeDSecureControlRequest.OrderNo,
                        postThreeDSecureControlRequest.IpAddress,
                        postThreeDSecureControlRequest.CallbackUrl,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty);

                    Serilog.Log.Fatal("{@KolayCARPostThreeDSecureControlResponseObject}", postThreeDSecureControlResult.Body.POST_3D_SECURE_CONTROLResult);

                    var postThreeDSecureControlResponseObject = JsonConvert.DeserializeObject<KolayCARResponseBase>(postThreeDSecureControlResult.Body.POST_3D_SECURE_CONTROLResult);

                    return new ServiceResponseBase
                    {
                        Success = postThreeDSecureControlResponseObject.RETURNCODE == 0,
                        Message = postThreeDSecureControlResponseObject.MESSAGE,
                        ServiceMessage = postThreeDSecureControlResponseObject.MESSAGE,
                        ServiceCode = postThreeDSecureControlResponseObject.RETURNCODE.ToStringNullSafe(),
                        Data = new PostThreeDSecureControlResponse
                        {
                            Message = postThreeDSecureControlResponseObject.MESSAGE,
                            Code = postThreeDSecureControlResponseObject.RETURNCODE.ToStringNullSafe(),
                            Content = postThreeDSecureControlResponseObject.THREEDSHTMLKEY,
                            APIKey = configurations.KolayCARPaymentAPIKey,
                            EncryptedAPIPassword = EncryptionHelper.Encrypt(configurations.KolayCARPaymentAPIPassword),
                            EncryptedVendorId = EncryptionHelper.Encrypt(configurations.KolayCARPaymentAPIVendorId.ToString()),
                            EncryptedType = EncryptionHelper.Encrypt("t1"),
                            Token = token.accessToken
                        }
                    };
                }
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Eksik parametre!"
            };
        }

        public async Task<ServiceResponseBase> PostPaymentRefund(PostPaymentRefundRequest postPaymentRefundRequest)
        {
            if (postPaymentRefundRequest != null)
            {
                var configurations = await _configurationService.GetConfigurations();

                SetLocalConfigurations(configurations);

                if (configurations != null && configurations.PaymentRefundActive)
                {
                    var postPaymentRequestObject = new KolayCARRequest.PostPaymentRequest
                    {
                        //APIKEY = "Ss4A3CYBHY4mUobwftsqpm8FJ4VsaYBlcW2zwQmBBD+aJWUaFcEA/WHRc2E6rYnF", //payment servisinde broker için bu API static olarak tanımlandı
                        APIKEY = configurations.KolayCARPaymentAPIKey,
                        APIPASSWORD = configurations.KolayCARPaymentAPIPassword,
                        //APIPASSWORD = string.Empty,
                        LANGISOCODE = postPaymentRefundRequest.LanguageCode,
                        CURRENCYISOCODE = postPaymentRefundRequest.CurrencyCode,
                        VENDORID = configurations.KolayCARPaymentAPIVendorId.ToStringNullSafe(),
                        DOMAINID = configurations.KolayCARPaymentAPIDomainId.ToStringNullSafe(),
                        BANKVENDORID = configurations.KolayCARBankVendorId.ToStringNullSafe(),
                        CUSTOMERMAILADDRESS = string.Empty,
                        CREDITCARDHOLDER = string.Empty,
                        CREDITCARDNUMBER = string.Empty,
                        CREDITCARDEXPIREDYEAR = string.Empty,
                        CREDITCARDEXPIREDMONTH = string.Empty,
                        SECURITYCODE = string.Empty,
                        INSTALLMENTCOUNT = "0",
                        PAYMENTAMOUNT = postPaymentRefundRequest.RefundAmount.ToString(),
                        ORDERNO = postPaymentRefundRequest.OrderNumber.ToStringNullSafe(),
                        IPADRESS = postPaymentRefundRequest.IpAddress,
                        THREEDPAYMENTACTIVE = false,
                        PARAM1VALUE = string.Empty,
                        PARAM2VALUE = string.Empty,
                        PARAM3VALUE = string.Empty,
                        PARAM4VALUE = string.Empty,
                        PARAM5VALUE = string.Empty,
                        PARAM6VALUE = string.Empty,
                        PARAM7VALUE = string.Empty,
                        PARAM8VALUE = string.Empty,
                        PARAM9VALUE = string.Empty,
                        PARAM10VALUE = ((int)postPaymentRefundRequest.PaymentRefundType).ToString(),
                        PARAM11VALUE = EncryptionHelper.Encrypt($"{configurations.KolayCARPaymentAPIVendorId.ToStringNullSafe()}{configurations.KolayCARPaymentAPIDomainId.ToStringNullSafe()}{configurations.KolayCARBankVendorId.ToStringNullSafe()}{postPaymentRefundRequest.OrderNumber.ToStringNullSafe()}"),
                        PARAM12VALUE = string.Empty,
                        PARAM13VALUE = string.Empty,
                        PARAM14VALUE = string.Empty
                    };
                    postPaymentRequestObject.IPADRESS = "37.130.115.31";
                    Serilog.Log.Fatal("{@KolayCARPostPaymentRefundRequestObject}", postPaymentRequestObject);
                    //string s = JsonConvert.SerializeObject(postPaymentRequestObject);
                    var postPaymentRefundResult = await _kolayCARService.POST_PAYMENTAsync(
                        postPaymentRequestObject.APIKEY,
                        postPaymentRequestObject.APIPASSWORD,
                        postPaymentRequestObject.LANGISOCODE,
                        postPaymentRequestObject.CURRENCYISOCODE,
                        postPaymentRequestObject.VENDORID,
                        postPaymentRequestObject.DOMAINID,
                        postPaymentRequestObject.BANKVENDORID,
                        postPaymentRequestObject.CUSTOMERMAILADDRESS,
                        postPaymentRequestObject.CREDITCARDHOLDER,
                        postPaymentRequestObject.CREDITCARDNUMBER.Replace("-", ""),
                        postPaymentRequestObject.CREDITCARDEXPIREDYEAR,
                        postPaymentRequestObject.CREDITCARDEXPIREDMONTH,
                        postPaymentRequestObject.SECURITYCODE,
                        postPaymentRequestObject.INSTALLMENTCOUNT,
                        postPaymentRequestObject.PAYMENTAMOUNT,
                        postPaymentRequestObject.ORDERNO,
                        postPaymentRequestObject.IPADRESS,
                        postPaymentRequestObject.THREEDPAYMENTACTIVE,
                        postPaymentRequestObject.PARAM1VALUE,
                        postPaymentRequestObject.PARAM2VALUE,
                        postPaymentRequestObject.PARAM3VALUE,
                        postPaymentRequestObject.PARAM4VALUE,
                        postPaymentRequestObject.PARAM5VALUE,
                        postPaymentRequestObject.PARAM6VALUE,
                        postPaymentRequestObject.PARAM7VALUE,
                        postPaymentRequestObject.PARAM8VALUE,
                        postPaymentRequestObject.PARAM9VALUE,
                        postPaymentRequestObject.PARAM10VALUE,
                        postPaymentRequestObject.PARAM11VALUE,
                        postPaymentRequestObject.PARAM12VALUE,
                        postPaymentRequestObject.PARAM13VALUE,
                        postPaymentRequestObject.PARAM14VALUE);

                    Serilog.Log.Fatal("{@KolayCARPostPaymentRefundResponseObject}", postPaymentRefundResult.Body.POST_PAYMENTResult);

                    var postPaymentRefundResponseObject = JsonConvert.DeserializeObject<KolayCARResponseBase>(postPaymentRefundResult.Body.POST_PAYMENTResult);

                    return new ServiceResponseBase
                    {
                        Success = postPaymentRefundResponseObject.RETURNCODE == 0,
                        Message = postPaymentRefundResponseObject.MESSAGE,
                        ServiceMessage = postPaymentRefundResponseObject.MESSAGE,
                        ServiceCode = postPaymentRefundResponseObject.RETURNCODE.ToStringNullSafe(),
                        Data = new PostPaymentResponse
                        {
                            Message = postPaymentRefundResponseObject.MESSAGE,
                            Code = postPaymentRefundResponseObject.RETURNCODE.ToStringNullSafe(),
                            PaymentResult = postPaymentRefundResponseObject.PAYMENT != null && postPaymentRefundResponseObject.PAYMENT.Count > 0 ? postPaymentRefundResponseObject.PAYMENT[0].Map() : null
                        }
                    };
                }
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Eksik parametre!"
            };
        }

        private void SetLocalConfigurations(Configurations configurations)
        {
            if (_env.IsDevelopment())
            {
                //configurations.KolayCARPaymentAPIKey = "dNg8MAogFARJqAdFb482pg==";
                //configurations.KolayCARPaymentAPIPassword = ".test";
                //configurations.KolayCARPaymentAPIVendorId = 68;
                //configurations.KolayCARPaymentAPIDomainId = 8;
                //configurations.KolayCARBankVendorId = 685;
                //configurations.KolayCARRentwsAPIKey = "qrRt7nuM5egyQqdI2JYQWg==";
                //configurations.KolayCARRentwsAPIPassword = "y5QPQ95ay9CMhPHT3qCZdzcqD1BWFDkbtmoXC9vrB/Q=";
                //configurations.PaymentRefundActive = false;
            }
        }

        public async Task UpdateReservationRefundAmount(PostPaymentRefundRequest postPaymentRefundRequest)
        {
            decimal oldValue = 0;
            decimal newValue = 0;
            try
            {
                var res = await _context.Rez.Where(x => x.Rezno == postPaymentRefundRequest.ReservationNumber).FirstOrDefaultAsync();

                if (res != null)
                {
                    oldValue = res.RefundAmount.ToDecimalNullSafe();
                    newValue = oldValue + postPaymentRefundRequest.RefundAmount.ToDecimalNullSafe();

                    Serilog.Log.Error("{@UpdateReservationRefundValues}", $"{postPaymentRefundRequest.ReservationNumber} - Old value : {oldValue} - New value : {newValue}");

                    res.RefundAmount = newValue;
                    await _context.SaveChangesAsync();
                }

                Serilog.Log.Error("{@UpdateReservationRefundAmount}", postPaymentRefundRequest);
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error("{@UpdateReservationRefundAmountError}", ex.Message);
                Serilog.Log.Error("{@UpdateReservationRefundAmountValues}", $"Old value : {oldValue} - New value : {newValue}");
            }
        }
        string GenerateSharedSecret(int year, int month)
        {
            try
            {
                var baseKey = "P.vEpUG0+ab?I2_q9o02L8Y2D$n";

                if (string.IsNullOrEmpty(baseKey))
                {
                    return string.Empty;
                }

                string monthString = new DateTime(year, month, 1).ToString("yyyy-MM");
                string combined = $"{baseKey}-{monthString}";

                using (var sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                    string hashedSecret = Convert.ToBase64String(hashBytes);

                    return hashedSecret;
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error generating shared secret for {Year}-{Month:D2}", year, month);
                return string.Empty;
            }
        }

    }
}
