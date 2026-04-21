using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.AvisResponseBase;

namespace KolayCAR.Broker.API.Helpers.Avis
{
    public static class QueryHelper<T>
    {
        public static IDictionary<string, object> GetHeaders(AvisAuthResponse avisAuthResponse, T entity, string secretKey)
        {
            var time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
            string bodySignature = AvisEncryptHelper.HashHMACHex(JsonConvert.SerializeObject(entity), secretKey);
            string originalSignature = AvisEncryptHelper.HashHMACHex(string.Concat(bodySignature, time), secretKey);

            return new Dictionary<string, object>()
            {
                { "Authorization", $"Bearer {avisAuthResponse.access_token}"},

                { "Signature", originalSignature},
                { "Time", time }

            };
        }
    }
}
