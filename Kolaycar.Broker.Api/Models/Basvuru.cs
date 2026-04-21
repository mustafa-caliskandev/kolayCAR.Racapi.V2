using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Basvuru
    {
        public int Basvuruid { get; set; }
        public DateTime? Tarih { get; set; }
        public string Firmaadi { get; set; }
        public string Firmasahibiadi { get; set; }
        public string Firmasahibisoyadi { get; set; }
        public string Telefon { get; set; }
        public string Email { get; set; }
        public string Vergidairesi { get; set; }
        public string Verginumarasi { get; set; }
        public string Adres { get; set; }
        public string Sehir { get; set; }
        public int? Aracsayisi { get; set; }
        public int? Dizelmanuel { get; set; }
        public int? Dizelotomatik { get; set; }
        public int? Benzinmanuel { get; set; }
        public int? Benzinotomatik { get; set; }
        public string Aciklama { get; set; }
        public string Ip { get; set; }
        public bool? Goruldu { get; set; }
    }
}
