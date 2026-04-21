using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models
{
    public partial class Vehiclemodel
    {
        public int Modelid { get; set; }
        public int Brandid { get; set; }
        public string Modelname { get; set; }

        public virtual ICollection<Vehicleclass> VehicleClasses { get; set; }
    }
}
