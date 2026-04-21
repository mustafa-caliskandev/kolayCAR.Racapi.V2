using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models
{
    public class SpecialRequestTariff
    {
        public int Id { get; set; }
        public int CurrencyId { get; set; }
        public int RentalTypeId { get; set; }
        public bool Active { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public float? Amount { get; set; }
        public float? MaxAmount { get; set; }
        public int LastUpdateByUser { get; set; }
        public DateTime? EditDate { get; set; }
        public virtual ICollection<SpecialRequest> SpecialRequests { get; set; }

    }
}
