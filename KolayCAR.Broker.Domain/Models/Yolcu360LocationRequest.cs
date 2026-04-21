using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KolayCAR.Broker.Domain.Models
{
    public class Yolcu360LocationRequest
    {
        public string LocationName { get; set; }
        public int VendorId { get; set; }
        public int LanguageId { get; set; }
    }
}
