namespace KolayCAR.Broker.Domain.Models
{
    public enum BrokerLogTypes
    {
        ReservationRequest,
        ReservationResponse,
        ReservationVendorAPIRequest,
        ReservationVendorAPIResponse,
        ReservationCancelRequest,
        ReservationCancelResponse,
        ReservationCancelVendorAPIRequest,
        ReservationCancelVendorAPIResponse,
        ReservationPaymentVendorAPIRequest,
        ReservationPaymentVendorAPIResponse
    }
}
