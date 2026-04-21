namespace KolayCAR.Broker.API.Models.Dtos
{
    public class CouponDto
    {
        public int? CouponId { get; set; } //* Kupon Id'si
        public int? CouponDiscountType { get; set; } //* Kupon Tipi (Yüzde & Tutar)
        public string CouponCode { get; set; } // Kupon Kodu
        public string CouponName { get; set; } // Kupon Adı
        public string CouponDescription { get; set; } // Kupon Açıklaması
        public decimal? CouponDiscountAmount { get; set; } // Toplam İndirim Tutarı
        public int? CouponDiscountPercent { get; set; } // İndirim Yüzdesi
    }
}
