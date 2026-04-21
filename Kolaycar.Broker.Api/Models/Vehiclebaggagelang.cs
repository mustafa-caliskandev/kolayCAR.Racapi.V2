namespace KolayCAR.Broker.API.Models
{
    public partial class Vehiclebaggagelang
    {
        public int Baggageid { get; set; }
        public int Langid { get; set; }
        public string Baggagename { get; set; }

        public virtual Vehiclebaggage Baggage { get; set; }
    }
}
