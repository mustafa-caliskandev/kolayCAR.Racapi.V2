using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Bulten
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public DateTime Tarih { get; set; }
        public string Ip { get; set; }
        public int? Acenteid { get; set; }
        public bool? ContactPermission { get; set; }
    }
}
