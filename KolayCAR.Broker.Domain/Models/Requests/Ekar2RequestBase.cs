namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class Ekar2RequestBase
    {
        public class PostReservationRequestBody
        {
            public string currency { get; set; }
            public string customerEmail { get; set; }
            public string customerGSM { get; set; }
            public string customerName { get; set; }
            public string customerNote { get; set; }
            public string customerPersonalNumber { get; set; }
            public string customerSurname { get; set; }
            public string extras { get; set; }
            public string flightNo { get; set; }
            public bool isDomestic { get; set; }
            public PaymentType paymentType { get; set; }
            public string pickupDate { get; set; }
            public int pickupLocationId { get; set; }
            public string pickupTime { get; set; }
            public string reservationNotes { get; set; }
            public string reservationUniqueId { get; set; }
            public string returnDate { get; set; }
            public int returnLocationId { get; set; }
            public string returnTime { get; set; }
            public int vehicleGroupId { get; set; }
        }
        public class PostCancelReservationRequestBody
        {
            public int reservationId { get; set; }
            public string cancelDescription { get; set; }
        }
        public enum PaymentType
        {
            COMMISSION_RECEIVED,
            RENT_FEE_RECEIVED,
            ALL_FEE_RECEIVED,
            PAYMENT_NOT_RECEIVED
        }
    }
}
