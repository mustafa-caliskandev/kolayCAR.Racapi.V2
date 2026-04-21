using System;

namespace KolayCAR.Broker.Domain.Models.Logo
{
    public class LogoReservation
    {
        public int AgencyId { get; set; }

        public DateTime ReservationDate { get; set; }
        public int ReservationStatusId { get; set; }
        public long ReservationId { get; set; }
        public string ReservationNo { get; set; }
        public decimal DailyPrice { get; set; }
        public int RentalDuration { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalPaymentPrice { get; set; }
        public string PickupLocation { get; set; }
        public int PickupLocationId { get; set; }
        public string ReturnLocation { get; set; }
        public int ReturnLocationId { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public decimal? CancellationRefund { get; set; }
        public decimal? ExtraTotal { get; set; }
        public decimal? DropAmount { get; set; }

        public int? CouponDiscountType { get; set; }
        public decimal? CouponDiscountValue { get; set; }
        public decimal? CouponVendorDiscountValue { get; set; }
        public decimal? CouponDiscountAmount { get; set; }

        public int Currency { get; set; }
        public string PaymentType { get; set; }
        public string Bank { get; set; }
        public string BankAccountCode { get; set; }
        public string BankAccountingCode { get; set; }
        public string CreditCard { get; set; }
        public string CreditCardBank { get; set; }
        public int? InstallmentCount { get; set; }
        public decimal? InstallmentCommission { get; set; }
        public decimal? InstallmentCommissionAmount { get; set; }

        public string IdentityNumber { get; set; }
        public string CustomerTitle { get; set; }
        public string CustomerName { get; set; }
        public string CustomerSurname { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string TaxNumber { get; set; }
        public string TaxOffice { get; set; }
        public string CustomerNote { get; set; }
        public string CustomerAddress { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public int VendorId { get; set; }
        public int RentalWorkingType { get; set; }
        public int AdditionalWorkingType { get; set; }
        public int OneWayWorkingType { get; set; }
        public decimal? ProfitMarkupRental { get; set; }
        public decimal? ProfitMarkupAdditionalProducts { get; set; }
        public decimal? ProfitMarkupOneWay { get; set; }
        public decimal? ServiceCharge { get; set; }
        public bool? ExtraPricePayOnDelivery { get; set; }
        public bool? OneWayFeePayOnDelivery { get; set; }
        public bool? VendorCommissionInvoice { get; set; }
        public decimal? InstallmentFee { get; set; }
    }
}
