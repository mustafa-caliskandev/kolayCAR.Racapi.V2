namespace KolayCAR.Broker.API.Models
{
    public partial class Vendorcontactinformation
    {
        public int Id { get; set; }
        public int Vendorid { get; set; }
        public int Locationid { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Phonenumber { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string GoogleLink { get; set; }
        public string ShortAddress { get; set; }
    }
}
