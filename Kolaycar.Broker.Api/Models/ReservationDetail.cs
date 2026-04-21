using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models
{
    public class ReservationDetail
    {
        public ReservationDetail()
        {
            ReservationDriverInfos = new HashSet<ReservationDriverInfo>();
            ReservationInvoiceAddresses = new HashSet<ReservationInvoiceAddress>();
            ReservationPaymentDetails = new HashSet<ReservationPaymentDetail>();
            ReservationSelectedExtras = new HashSet<ReservationSelectedExtra>();
        }
        public int Id { get; set; }
        public int PaymentType { get; set; }

        public DateTime? LogDate { get; set; }

        public string ReservationToken { get; set; }
        public string CouponCode { get; set; }
        public string SelectedExtrasJson { get; set; }
        public string ReservationNote { get; set; }
        public string IpAddress { get; set; }
        public int? MemberId { get; set; }
        public int? CurrencyId { get; set; }
        public string SkyScannerRedirectId { get; set; }

        public string PickupLocation { get; set; }
        public string PickupDate { get; set; }
        public string ReturnLocation { get; set; }
        public string ReturnDate { get; set; }
        public string VendorName { get; set; }

        public bool? ConfirmConditions { get; set; }
        public bool? InvoiceToDifferentAddress { get; set; }
        public bool? FullCredit { get; set; }

        public decimal? CouponDiscountAmount { get; set; }
        public decimal? ServiceCharge { get; set; }

        public virtual ICollection<ReservationDriverInfo> ReservationDriverInfos { get; set; }
        public virtual ICollection<ReservationInvoiceAddress> ReservationInvoiceAddresses { get; set; }
        public virtual ICollection<ReservationPaymentDetail> ReservationPaymentDetails { get; set; }
        public virtual ICollection<ReservationSelectedExtra> ReservationSelectedExtras { get; set; }
        public virtual ICollection<ReservationVehicleInfo> ReservationVehicleInfos { get; set; }
    }
}
