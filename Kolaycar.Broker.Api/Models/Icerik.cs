using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models
{
    public partial class Icerik
    {
        public int Icerikid { get; set; }
        public int Tipid { get; set; }
        public int Grupid { get; set; }
        public int? Parent { get; set; }
        public bool? Aktif { get; set; }
        public int? Sira { get; set; }
        public bool? Gosterim { get; set; }
        public int? Lokasyonid { get; set; }
        public int? Sinifid { get; set; }
        public DateTime? Baslangic { get; set; }
        public DateTime? Bitis { get; set; }
        public int? Modulid { get; set; }
        public bool? Kirasozlesmeaktif { get; set; }
        public bool? Kirakosulaktif { get; set; }
        public bool? Kvkkmetinaktif { get; set; }
        public DateTime? Createdate { get; set; }
        public int? Vendorid { get; set; }
        public int? Vehicleclassid { get; set; }

        public virtual IEnumerable<Icerikdil> IcerikDiller { get; set; }
    }
}
