namespace KolayCAR.Broker.API.Models
{
    public partial class Staticlokasyon
    {
        public int Id { get; set; }
        public int Dilid { get; set; }
        public string Bolge { get; set; }
        public string Iata { get; set; }
        public bool? Havalimani { get; set; }
        public int? Mycarid { get; set; }
        public bool? Mycaraktif { get; set; }
        public int? V3id { get; set; }
        public bool? V3aktif { get; set; }
        public int? Ekarid { get; set; }
        public bool? Ekaraktif { get; set; }
    }
}
