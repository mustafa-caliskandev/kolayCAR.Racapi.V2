using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Helpers.GreenMotion
{
    public class RequestHelper
    {
        public static Dictionary<string, object> GetGreenMotionRequestHeader() =>
            new Dictionary<string, object>()
            {
                { "Content-Type", "application/xml"}
            };
        public static GreenMotionRequestBase GetGreenMotionRequestObject(Vendor vendor, object requestPayload) =>
            new GreenMotionRequestBase
            {
                gm_webservice = new GreenMotionRequestBase.GreenMotionRequestBaseScope
                {
                    header = new GreenMotionRequestBase.GreenMotionRequestHeader
                    {
                        username = vendor.ApiKey,
                        password = vendor.ApiPassword,
                        version = vendor.ApiClientId
                    },
                    request = requestPayload
                }
            };
    }
}
