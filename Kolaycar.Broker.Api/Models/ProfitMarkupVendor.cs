using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Models
{
    public class ProfitMarkupVendor
    {
        public int Id { get; set; }
        public int ProfitMarkupId { get; set; }
        public int VendorId { get; set; }

        public virtual ProfitMarkup ProfitMarkup { get; set; }

        public virtual Vendor Vendor { get; set; }
        
    }
}
