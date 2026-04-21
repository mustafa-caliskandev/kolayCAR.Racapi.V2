using System;

namespace KolayCAR.Broker.Domain.Models
{
    public class Addition
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AdditionTypeId { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? LastStatusChangeDate { get; set; }
        public string ReservationNumber { get; set; }
        public string AdditionName { get; set; }
        public string Description { get; set; }
        public float? VendorAmount { get; set; }
        public int? VendorCurrencyId { get; set; }
        public float? AgencyAmount { get; set; }
        public int? AgencyCurrencyId { get; set; }
        public ReservationAdditionStatusTypes AdditionStatusType { get; set; }
        public int? LastStatusChangeUserId { get; set; }
    }

    public enum ReservationAdditionStatusTypes
    {
        Added,
        Canceled
    }
}
