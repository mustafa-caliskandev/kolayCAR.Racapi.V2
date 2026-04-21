using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Portalacente
    {
        public int Portalacenteid { get; set; }
        public int? Kcportalacenteid { get; set; }
        public bool Aktif { get; set; }
        public string Acenteadi { get; set; }
        public string Logo { get; set; }
        public string Telefon { get; set; }
        public string Eposta { get; set; }
        public string Adres { get; set; }
        public string Gsm { get; set; }
        public string Yetkili { get; set; }
        public string Apikey { get; set; }
        public string Sifre { get; set; }
        public DateTime? Tarih { get; set; }
        public string Acentekodu { get; set; }
        public int Parent { get; set; }
        public double Acentekomisyon { get; set; }
    }
}
