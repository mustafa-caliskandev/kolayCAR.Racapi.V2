using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;

namespace KolayCAR.Broker.API.Helpers
{
    public static class VendorReservationResponseHelper
    {
        public static string GetRawSupplierResponse<T>(HttpResult<T> result) where T : class
        {
            return !string.IsNullOrWhiteSpace(result?.ServiceMessage)
                ? result.ServiceMessage
                : result?.RawContent;
        }

        public static string GetSupplierMessage<T>(HttpResult<T> result, string fallbackMessage = "") where T : class
        {
            if (!string.IsNullOrWhiteSpace(result?.Message))
                return result.Message;

            return GetRawSupplierResponse(result) ?? fallbackMessage;
        }

        public static ServiceResponseBase CreateErrorResponse<T>(Reservation localReservation, Vendor vendor, HttpResult<T> result, string fallbackMessage) where T : class
        {
            var rawSupplierResponse = GetRawSupplierResponse(result);
            var supplierMessage = GetSupplierMessage(result);

            if (!string.IsNullOrWhiteSpace(supplierMessage))
                localReservation.APIMessage = supplierMessage;

            return new ServiceResponseBase(
                localReservation,
                false,
                !string.IsNullOrWhiteSpace(supplierMessage) ? $"{vendor.VendorName} - {supplierMessage}" : $"{vendor.VendorName} {fallbackMessage}",
                serviceMessage: rawSupplierResponse ?? supplierMessage ?? string.Empty,
                serviceCode: result != null ? ((int)result.HttpStatusCode).ToString() : string.Empty);
        }
    }
}
