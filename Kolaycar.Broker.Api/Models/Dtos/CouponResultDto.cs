using System;

namespace KolayCAR.Broker.API.Models.Dtos
{
    public class CouponResultDto
    {
        public int? CouponDiscountType { get; set; } //* Kupon Tipi (Yüzde & Tutar)
        public string CouponCode { get; set; } // Kupon Kodu
        public string CouponName { get; set; } // Kupon Adı
        public string CouponDescription { get; set; } // Kupon Açıklaması
        public decimal? CouponDiscountAmount { get; set; } // İndirim Tutarı
        public int? CouponDiscountPercent { get; set; } // İndirim Yüzdesi
        public bool MultipleUsage { get; set; } // Çoklu Kullanım
        public int CurrencyId { get; set; } // Para Birimi Id'si
        public DateTime StartDate { get; set; } // Kupon başlangıç tarihi
        public DateTime EndDate { get; set; } // Kupon bitiş tarihi
    }
}
