namespace KolayCAR.Broker.API.Models
{
    public partial class Vehicletype
    {
        public int Typeid { get; set; }
        public bool Active { get; set; }
        public int Order { get; set; }
        public string IconPath { get; set; }
    }
}
