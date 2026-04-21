using System;

namespace KolayCAR.Broker.Domain.Models
{
    public class PaymentDto
    {
        public int RowId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string ContractNumber { get; set; }
        public long ResId { get; set; }
        public string ReservationNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerSurname { get; set; }
        public string CustomerNumber { get; set; }
        public decimal? Amount { get; set; }
        public decimal? RentAmount { get; set; }
        public decimal? RentAmountWithoutTax { get; set; }
        public decimal? DailyRentPrice { get; set; }
        public decimal? TaxRate { get; set; }
        public decimal? DailyRentPriceWithoutTax { get; set; }
        public decimal? CancellationRefundAmount { get; set; }
        public decimal? ExtraAmount { get; set; }
        public decimal? ExtraAmountWithoutTax { get; set; }
        public decimal? DropAmount { get; set; }
        public decimal? DropAmountWithoutTax { get; set; }
        public decimal? CouponDiscount { get; set; }
        public int? Currency { get; set; }
        public string PaymentType { get; set; }
        public string Bank { get; set; }
        public string BankAccountCode { get; set; }
        public string BankAccountingCode { get; set; }
        public string CreditCard { get; set; }
        public int? InstallmentCount { get; set; }
        public decimal? InstallmentCommissionAmount { get; set; }
        public string ReceiptNumber { get; set; }
        public long? PaymentID { get; set; }
        public string PaymentRefNo { get; set; }
        public string TCNumber { get; set; }
        public string CustomerTitle { get; set; }
        public string TaxNumber { get; set; }
        public string TaxOffice { get; set; }
        public string CashRegisterCode { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string CustomerAddress { get; set; }
        public decimal? ExchangeRate { get; set; }
        public decimal? TLAmount { get; set; }
        public string Mail { get; set; }
        public string Branch { get; set; }
        public string MemberBusinessBranch { get; set; }
        public string MemberBusinessNumber { get; set; }
        public string Description { get; set; }
        public int? VendorId { get; set; }
        public string PickupLocation { get; set; }
        public int? PickupLocationId { get; set; }
        public string ReturnLocation { get; set; }
        public int? ReturnLocationId { get; set; }
        public int? RentalDuration { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public DateTime? CancelDate { get; set; }
        public int? ResStatusId { get; set; }
        public string CreditCardBank { get; set; }
        public int? RentalWorkingType { get; set; }
        public decimal? ProfitMarkupRental { get; set; }
        public int? AdditionalWorkingType { get; set; }
        public decimal? ProfitMatkupAdditionalProducts { get; set; }
        public int? OneWayWorkingType { get; set; }
        public decimal? ProfitMatkupOneWay { get; set; }
        public int? CurrentResStatus { get; set; }
        public decimal? ServiceCharge { get; set; }
        public bool? ExtraPricePayOnDelivery { get; set; }
        public bool? OneWayFeePayOnDelivery { get; set; }
        public bool? VendorCommissionInvoice { get; set; }
    }
}