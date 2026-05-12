using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Rentws;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Xml;
using static KolayCAR.Rentws.ServiceSoapClient;

namespace KolayCAR.Broker.API.Services
{
    public interface ISmsService
    {
        Task<bool> PostSms(PostSmsRequest postSmsRequest);
    }

    public class SmsService : ISmsService
    {
        private readonly ServiceSoapClient _kolayCARService;
        private readonly IConfigurationService _configurationService;
        public SmsService(IConfigurationService configurationService)
        {
            _kolayCARService = new ServiceSoapClient(EndpointConfiguration.ServiceSoap);
            _configurationService = configurationService;
        }

        public async Task<bool> PostSms(PostSmsRequest postSmsRequest)
        {
            try
            {
                if (postSmsRequest != null)
                {
                    if (!string.IsNullOrEmpty(postSmsRequest.Content) &&
                        !string.IsNullOrEmpty(postSmsRequest.PhoneNumber))
                    {
                        var configurations = await _configurationService.GetConfigurations();

                        var postSmsRequestBody = new POST_SMSRequestBody
                        {
                            APIKEY = EncryptionHelper.Encrypt($"Brk.r%+76!{DateTime.Now.ToString("dd.MM.yyyy")}"),
                            APIPASSWORD = "brkr.API1122!",
                            LANGISOCODE = postSmsRequest.LanguageType.ToString(),
                            RESERVATIONNO = string.Empty,
                            RESTYPE = "2",
                            PROCESSTYPE = string.Empty,
                            PHONENUMBER = postSmsRequest.PhoneNumber,
                            SUBJECT = string.Empty,
                            CONTENT = postSmsRequest.Content,
                            SMSBULKID = string.Empty,
                            TASKDATE = string.Empty,
                            PARAM1 = configurations.KolayCARPaymentAPIVendorId.ToString(),
                            PARAM2 = string.Empty,
                            PARAM3 = string.Empty,
                            PARAM4 = string.Empty,
                            PARAM5 = string.Empty,
                            PARAM6 = string.Empty
                        };

                        Serilog.Log.Error("{@KolayCARPostSmsRequestBody}", JsonConvert.SerializeObject(postSmsRequestBody));

                        var postSmsResult = await _kolayCARService.POST_SMSAsync(new POST_SMSRequest(postSmsRequestBody));

                        Serilog.Log.Error("{@KolayCARPostSmsResponseBody}", postSmsResult.Body.POST_SMSResult.InnerXml);

                        var xmldoc = new XmlDocument();
                        xmldoc.LoadXml($"<ROOT>{postSmsResult.Body.POST_SMSResult.InnerXml}</ROOT>");
                        var fromXml = JsonConvert.SerializeXmlNode(xmldoc);

                        var result = JsonConvert.DeserializeObject<POST_SMS_RESPONSE_ROOT>(fromXml);

                        return result != null && result.ROOT != null && result.ROOT.SMSGITTI;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@KolayCARPostSmsError}", ex.ToJson());
                return false;
            }
        }
    }
}
