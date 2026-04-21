namespace KolayCAR.Broker.API.Models.Dtos
{
    public class CouponDetailDto
    {
        public int Result { get; set; }
        public string Message { get; set; }

        public int? CouponId { get; set; } //* Kupon Id'si
        public int? CouponDiscountType { get; set; } //* Kupon Tipi (Yüzde & Tutar)
        public string CouponCode { get; set; } // Kupon Kodu
        public string CouponName { get; set; } // Kupon Adı
        public string CouponDescription { get; set; } // Kupon Açıklaması

        public decimal? TotalAmount { get; set; } // Toplam Tutar
        public decimal? TotalDiscount { get; set; } // Toplam İndirim Tutarı
        public int? DiscountPercent { get; set; } // İndirim Yüzdesi
        public decimal? DiscountAmount { get; set; } // Kupon İndirimi

        public int? PaymentType { get; set; } // Ödeme Tipi (Kullanılmıyor)
    }
}
