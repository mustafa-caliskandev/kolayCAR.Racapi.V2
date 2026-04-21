using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models
{
    public partial class Vehiclebaggage
    {
        public Vehiclebaggage()
        {
            Vehiclebaggagelang = new HashSet<Vehiclebaggagelang>();
        }

        public int Baggageid { get; set; }
        public bool Active { get; set; }
        public int? Order { get; set; }
        public string Image { get; set; }

        public virtual ICollection<Vehiclebaggagelang> Vehiclebaggagelang { get; set; }
    }
}
