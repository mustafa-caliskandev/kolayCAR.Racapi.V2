namespace KolayCAR.Broker.API.Models.PaymentDto
{
    public class InstallmentsRequestDto
    {
        public string LanguageCode { get; set; }
        public string CardNumber { get; set; }
        public string ReservationToken { get; set; }
        public bool AdvancePaymentActive { get; set; }
        //public string PaymentAmount { get; set; }
        public string CouponCode { get; set; }
        public int PickupLocationId { get; set; } = -1;
        public int VendorId { get; set; } = -1;
        public string CustomerInfo { get; set; } = "";
    }
}
