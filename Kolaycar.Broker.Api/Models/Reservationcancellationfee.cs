namespace KolayCAR.Broker.API.Models
{
    public partial class Reservationcancellationfee
    {
        public int Id { get; set; }
        public int Vendorid { get; set; }
        public int Minrange { get; set; }
        public int Maxrange { get; set; }
        public int Pricepercentage { get; set; }
    }
}
