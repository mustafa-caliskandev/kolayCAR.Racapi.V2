using KolayCAR.Broker.Domain.Models;
using static KolayCAR.Broker.Domain.Models.Response.AvisResponseBase;

namespace KolayCAR.Broker.API.Helpers.Avis
{
    public static class ObjectValidationHelper
    {
        public static bool CheckResponseObject(HttpResult<AvisLocationResponse> result)
        {
            if (result != null)
            {
                if (result.Data != null && result.Success)
                {
                    if (result.Data.Data.Count > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }
        public static bool CheckResponseObject(HttpResult<AvisVehicleListResponse> result)
        {
            if (result != null)
            {
                if (result.Data != null && result.Success)
                {
                    if (result.Data.Data.Count > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }
        public static bool CheckResponseObject(HttpResult<AvisAvailableVehicleResponse> result)
        {
            if (result != null)
            {
                if (result.Data != null && result.Success)
                {
                    if (result.Data?.Data?.vehicles.Count > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }
    }
}
