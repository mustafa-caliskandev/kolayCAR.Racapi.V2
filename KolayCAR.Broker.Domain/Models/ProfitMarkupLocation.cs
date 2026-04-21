using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KolayCAR.Broker.Domain.Models
{
    public class ProfitMarkupLocation
    {

        public int Id { get; set; }
        public int ProfitMarkupId { get; set; }
        public int LocationId { get; set; }

        public virtual ProfitMarkup ProfitMarkup { get; set; }
        public virtual Location Location { get; set; }
    }
}
