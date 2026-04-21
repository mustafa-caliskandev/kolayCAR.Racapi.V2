using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Icerikdil
    {
        public int Icerikid { get; set; }
        public int Dilid { get; set; }
        public string Baslik { get; set; }
        public string Ozet { get; set; }
        public string Editor { get; set; }
        public string Link { get; set; }
        public string Resim { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Keyword { get; set; }
        public string Canonical { get; set; }
        public string Resimalt { get; set; }
        public string Contenturl { get; set; }
        public int? Syncicerikid { get; set; }
        public bool? Aktifmi { get; set; }
        public DateTime? BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }

        public virtual Icerik Icerik { get; set; }
    }
    public enum IcerikGrup : int
    {
        SMS = 15
    }
}
