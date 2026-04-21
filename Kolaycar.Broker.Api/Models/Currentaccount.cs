using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Currentaccount
    {
        public long RecId { get; set; }
        public int? VendorId { get; set; }
        public int? AgencyId { get; set; }
        public string? DocumentNumber { get; set; }
        public byte? Type { get; set; }
        public DateTime? RegisteredDate { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string? Note { get; set; }
        public decimal? Amount { get; set; }
        public int? CurrencyId { get; set; }
        public DateTime? ResPickUpDate { get; set; }
        public DateTime? ResReturnDate { get; set; }
        public byte? TransactionType { get; set; }
        public long? ResId { get; set; }
        public bool IsPaid { get; set; }
        public int CurrentType { get; set; }
        public decimal PenaltyAmount { get; set; }
    }
}
