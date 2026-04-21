using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;

namespace KolayCAR.Broker.API.Helpers.Garenta
{
    public class RequestHelper
    {
        public static GarentaRequestBase.SapProps GetSapProps(GarentaRequestBase.ServiceTypes serviceType, string apiKey, string password) =>
            new GarentaRequestBase.SapProps
            {
                SERVICE_NAME = serviceType.ToString(),
                USER_NAME = apiKey,
                PASSWORD = password
            };

        public static string GetLanguageType(LanguageTypes languageType) =>
            languageType switch
            {
                _ => GarentaRequestBase.LanguageTypes.T.ToString(),
            };
    }
}
