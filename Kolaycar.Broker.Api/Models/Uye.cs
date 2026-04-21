namespace KolayCAR.Broker.API.Models
{
    public partial class Uye
    {
        public int Uyeid { get; set; }
        public bool? Aktif { get; set; }
        public string Eposta { get; set; }
        public string Pwd { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string Telefon { get; set; }
        public string Tcpasaport { get; set; }
        public string Vergidaire { get; set; }
        public string Vergino { get; set; }
        public string Unvan { get; set; }
        public string Adres { get; set; }
        public byte? Uyetip { get; set; }
        public byte? Membertype { get; set; }
        public int? Agencyid { get; set; }
        public bool? Defaultmember { get; set; }
        public bool? Subagencyuser { get; set; }
        public string Uyetipmembertype { get; set; }
        public bool? Commercialallowance { get; set; }
        public string Ip { get; set; }
        public string Birthday { get; set; }
    }
}
