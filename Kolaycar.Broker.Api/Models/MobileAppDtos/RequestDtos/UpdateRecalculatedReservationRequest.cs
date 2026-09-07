using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.RequestDtos
{
    [DataContract]
    public class UpdateRecalculatedReservationRequest
    {
        /// <summary>
        /// Gets or sets ReservationCode — güncellenecek rezervasyonun numarası
        /// </summary>
        [DataMember(Order = 1, Name = "reservationCode"), JsonProperty(PropertyName = "reservationCode")]
        public string ReservationCode { get; set; }

        /// <summary>
        /// Gets or sets NewReturnDate — yeni bırakış tarihi
        /// </summary>
        [DataMember(Order = 2, Name = "newReturnDate"), JsonProperty(PropertyName = "newReturnDate")]
        public DateTime NewReturnDate { get; set; }

        /// <summary>
        /// Gets or sets RentalDayCount — yeniden hesaplanan kiralama gün sayısı
        /// </summary>
        [DataMember(Order = 3, Name = "rentalDayCount"), JsonProperty(PropertyName = "rentalDayCount")]
        public int RentalDayCount { get; set; }

        /// <summary>
        /// Gets or sets TotalInvoiceAmount — yeniden hesaplanan toplam tutar
        /// </summary>
        [DataMember(Order = 4, Name = "totalInvoiceAmount"), JsonProperty(PropertyName = "totalInvoiceAmount")]
        public decimal TotalInvoiceAmount { get; set; }

        /// <summary>
        /// Gets or sets VendorPaidAmount — tedarikçiye ödenen tutar
        /// </summary>
        [DataMember(Order = 5, Name = "vendorPaidAmount"), JsonProperty(PropertyName = "vendorPaidAmount")]
        public decimal VendorPaidAmount { get; set; }

        /// <summary>
        /// Gets or sets AgencyPaidAmount — acenteye ödenen tutar
        /// </summary>
        [DataMember(Order = 6, Name = "agencyPaidAmount"), JsonProperty(PropertyName = "agencyPaidAmount")]
        public decimal AgencyPaidAmount { get; set; }

        /// <summary>
        /// Gets or sets InstallmentDelta — yeniden hesaplanan vade farkı
        /// </summary>
        [DataMember(Order = 7, Name = "installmentDelta"), JsonProperty(PropertyName = "installmentDelta")]
        public decimal InstallmentDelta { get; set; }

        /// <summary>
        /// Gets or sets CouponDiscountAmount — yeniden hesaplanan kupon indirim tutarı
        /// </summary>
        [DataMember(Order = 8, Name = "couponDiscountAmount"), JsonProperty(PropertyName = "couponDiscountAmount")]
        public decimal CouponDiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets RefundAmount
        /// </summary>
        [DataMember(Order = 9, Name = "refundAmount"), JsonProperty(PropertyName = "refundAmount")]
        public decimal RefundAmount { get; set; }

        /// <summary>
        /// Gets or sets PaidAmount
        /// </summary>
        [DataMember(Order = 10, Name = "paidAmount"), JsonProperty(PropertyName = "paidAmount")]
        public decimal PaidAmount { get; set; }
    }
}
