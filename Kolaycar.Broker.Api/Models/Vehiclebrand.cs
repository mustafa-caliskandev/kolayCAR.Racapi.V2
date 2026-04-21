using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models
{
    public partial class Vehiclebrand
    {
        public int Brandid { get; set; }
        public string Brandname { get; set; }
        public string Logo { get; set; }

        public virtual ICollection<Vehicleclass> VehicleClasses { get; set; }
    }
}
