namespace KolayCAR.Broker.API.Models
{
    public partial class Vehiclecategory
    {
        public int Categoryid { get; set; }
        public bool Active { get; set; }
        public int? Order { get; set; }
        public string IconPath { get; set; }
    }
}
