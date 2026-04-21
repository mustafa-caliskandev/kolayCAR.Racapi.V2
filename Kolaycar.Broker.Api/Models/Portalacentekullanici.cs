using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Portalacentekullanici
    {
        public int Portalacentekullaniciid { get; set; }
        public int? Kcportalacentekullaniciid { get; set; }
        public int Portalacenteid { get; set; }
        public string Kullanicieposta { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string Kullaniciadi { get; set; }
        public string Sifre { get; set; }
        public DateTime? Kayittarihi { get; set; }
        public bool? Aktif { get; set; }
    }
}
