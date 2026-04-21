using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Log
    {
        public int Logid { get; set; }
        public DateTime? Tarih { get; set; }
        public string Islem { get; set; }
        public int? Kullaniciid { get; set; }
        public int? Uyeid { get; set; }
        public int? Agencyid { get; set; }
        public string Kullaniciadi { get; set; }
        public string Uyeadi { get; set; }
        public string Ip { get; set; }
        public string Browser { get; set; }
        public string Referer { get; set; }
        public string Pathinfo { get; set; }
        public int? Logtip { get; set; }
    }
}
