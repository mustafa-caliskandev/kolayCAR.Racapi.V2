using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models
{
    public class SpecialRequest
    {
        public int Id { get; set; }
        public bool Active { get; set; }
        public int Priority { get; set; }
        public int SpecialRequestTariffId { get; set; }
        public int AdditionalProductId { get; set; }
        public int? LastUpdateByUserId { get; set; }
        public string Name { get; set; }
        public DateTime? ReservationStartDate { get; set; }
        public DateTime? ReservationEndDate { get; set; }
        public DateTime? PickupStartDate { get; set; }
        public DateTime? PickupEndDate { get; set; }
        public DateTime? EditDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? CurrencyId { get; set; }
        public float? MaximumAmount { get; set; }
        public int? MaximumDay { get; set; }
        public float? MinimumAmount { get; set; }
        public int? MinimumDay { get; set; }
        public virtual SpecialRequestTariff SpecialRequestTariff { get; set; }
        public virtual ICollection<SpecialRequestVendor> Vendors { get; set; }
        public virtual ICollection<SpecialRequestAgency> Agencies { get; set; }
        public virtual ICollection<SpecialRequestLocation> Locations { get; set; }
        public virtual ICollection<SpecialRequestCategory> Categories { get; set; }
    }
}
