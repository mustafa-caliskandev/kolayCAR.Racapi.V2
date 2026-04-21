namespace KolayCAR.Broker.API.Models
{
    public partial class Dil
    {
        public int Dilid { get; set; }
        public string Dilkod { get; set; }
        public string Dilad { get; set; }
        public string Gorunurad { get; set; }
        public string Bayrak { get; set; }
        public bool? Aktif { get; set; }
        public string Culture { get; set; }
        public bool? IsLtr { get; set; }
    }
}
