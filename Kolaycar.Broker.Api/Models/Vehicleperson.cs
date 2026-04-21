namespace KolayCAR.Broker.API.Models
{
    public partial class Vehicleperson
    {
        public int Personid { get; set; }
        public bool Active { get; set; }
        public int? Order { get; set; }
        public string IconPath { get; set; }
    }
}
