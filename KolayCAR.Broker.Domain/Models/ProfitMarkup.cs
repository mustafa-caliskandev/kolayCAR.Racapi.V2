using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KolayCAR.Broker.Domain.Models
{
    public class ProfitMarkup
    {


        public int Id { get; set; }
        public int Type { get; set; } // Increase & Decrease
        public int? Priority { get; set; }
        public int MarkupType { get; set; }
        public int? MinimumDay { get; set; }
        public int? MaximumDay { get; set; }
        public int? LastByUpdateUserId { get; set; }
        public int? CurrencyId { get; set; }
        public int? AmountCurrencyId { get; set; }


        public DateTime? ReservationStartDate { get; set; }
        public DateTime? ReservationEndDate { get; set; }
        public DateTime? PickupStartDate { get; set; }
        public DateTime? PickupEndDate { get; set; }
        public DateTime? EditDate { get; set; }

        public decimal? MinimumAmount { get; set; }
        public decimal? MaximumAmount { get; set; }
        public float? MarkupValue { get; set; }

        public string Name { get; set; }

        public virtual ICollection<ProfitMarkupAgency> ProfitMarkupAgencies { get; set; }
        public virtual ICollection<ProfitMarkupLocation> ProfitMarkupLocations { get; set; }
        public virtual ICollection<ProfitMarkupVendor> ProfitMarkupVendors { get; set; }
    }
}
