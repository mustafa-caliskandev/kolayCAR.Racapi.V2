namespace KolayCAR.Broker.API.Models.MobileAppDtos.RequestDtos
{
    public class GetCouponDetailsDto
    {
        public string ReservationToken { get; set; }
        public string CouponCode { get; set; }
        public string LanguageCode { get; set; }
        public string CurrencyCode { get; set; }
    }
}
