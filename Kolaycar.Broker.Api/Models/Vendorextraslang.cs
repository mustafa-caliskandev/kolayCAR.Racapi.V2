namespace KolayCAR.Broker.API.Models
{
    public partial class Vendorextraslang
    {
        public int Id { get; set; }
        public int Extraid { get; set; }
        public int Vendorid { get; set; }
        public int Langid { get; set; }
        public string Extraname { get; set; }
        public string Extradescription { get; set; }
    }
}
