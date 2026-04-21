using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Hata
    {
        public int Id { get; set; }
        public int? Kullaniciid { get; set; }
        public int? Uyeid { get; set; }
        public int? Rezid { get; set; }
        public DateTime? Tarih { get; set; }
        public string Bilgi { get; set; }
        public string Hata1 { get; set; }
        public string Sorgu { get; set; }
        public string Ip { get; set; }
        public string Browser { get; set; }
        public string Referer { get; set; }
        public string Pathinfo { get; set; }
    }
}
