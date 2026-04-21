using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class EkarResponseBase
    {
        [JsonProperty("Sistemrent")]
        public Sistemrent EkarSistemrent { get; set; }

        public class Sube
        {
            public string Sube_Kodu { get; set; }
            public string Sube_Ismi { get; set; }
            public string Kayit_ID { get; set; }
            public string Enlem { get; set; }
            public string Boylam { get; set; }
            public string Adres { get; set; }
            public string Ilce { get; set; }
            public string Sehir { get; set; }
            public string Mail_Adresi { get; set; }
            public string Telefon { get; set; }
            public string _id { get; set; }
        }

        public class Arac
        {
            public string Grup_Adi { get; set; }
            public string Marka { get; set; }
            public string Tipi { get; set; }
            public string Yakit_Turu { get; set; }
            public string Vites { get; set; }
            public string Grup_Kodu { get; set; }
            public string Ehliyet_Yil { get; set; }
            public string Surucu_Yas { get; set; }
            public string Grup_Aciklama { get; set; }
            public string SIPP { get; set; }
            public string AracResmi { get; set; }
            public string Provizyon_Ucreti { get; set; }
            public string _id { get; set; }
        }

        public class Musaitlik
        {
            public string Rez_ID { get; set; }
            public string Grup_Adi { get; set; }
            public string SIPP { get; set; }
            public string Ehliyet_Yil { get; set; }
            public string Surucu_Yas { get; set; }
            public string G_Fiyat { get; set; }
            public string Kira_Gun { get; set; }
            public string T_Fiyat { get; set; }
            public string Buyuk_Bagaj { get; set; }
            public string Kucuk_Bagaj { get; set; }
            public string Koltuk_Sayisi { get; set; }
            public string Kaynak_Adi { get; set; }
            public string Kaynak_ID { get; set; }
            public string Kayit_No { get; set; }
            public string Provizyon_Ucreti { get; set; }
            public string KM_Siniri { get; set; }
            public string Drop_Mesafe { get; set; }
            public string Hemen_Ode_Indirim { get; set; }
            public string Arac_Resmi { get; set; }
            public string Lst_Fiyat { get; set; }
            public string Rent_Path { get; set; }
            public float CDW_Hesap_Fiyat { get; set; }
            public string CDW_Hesap_Baslik { get; set; }
            public string CDW_Hesap_Aciklama { get; set; }
            public float SCDW_Hesap_Fiyat { get; set; }
            public string SCDW_Hesap_Baslik { get; set; }
            public string SCDW_Hesap_Aciklama { get; set; }
            public float LCF_Hesap_Fiyat { get; set; }
            public string LCF_Hesap_Baslik { get; set; }
            public string LCF_Hesap_Aciklama { get; set; }
            public float PAI_Hesap_Fiyat { get; set; }
            public string PAI_Hesap_Baslik { get; set; }
            public string PAI_Hesap_Aciklama { get; set; }
            public float Bebek_Koltuk_Hesap_Fiyat { get; set; }
            public string Bebek_Koltuk_Hesap_Baslik { get; set; }
            public string Bebek_Koltuk_Hesap_Aciklama { get; set; }
            public float Navigasyon_Hesap_Fiyat { get; set; }
            public string Navigasyon_Hesap_Baslik { get; set; }
            public string Navigasyon_Hesap_Aciklama { get; set; }
            public float Ek_Surucu_Hesap_Fiyat { get; set; }
            public string Ek_Surucu_Hesap_Baslik { get; set; }
            public string Ek_Surucu_Hesap_Aciklama { get; set; }
            public string LCF_Dahil { get; set; }
            public string SCDW_Dahil { get; set; }
            public string CDW_Dahil { get; set; }
            public string PAI_Dahil { get; set; }
            public string Doviz { get; set; }
            public string Kur { get; set; }
            public string Symbol { get; set; }
            public string On_Odeme_Tutari { get; set; }
            public string _id { get; set; }
        }

        public class Rezervasyon
        {
            public string Kayit_No { get; set; }
            public string Key { get; set; }
            public string Doluluk { get; set; }
            public string Durum { get; set; }
            public string _id { get; set; }
        }

        public class Rezervation
        {
            public string ID { get; set; }
            public string Status { get; set; }
            public string _id { get; set; }
        }

        public class Sistemrent
        {
            public List<Arac> Arac { get; set; }
            public List<Sube> Sube { get; set; }
            public List<Musaitlik> Musaitlik { get; set; }
            public Rezervasyon Rezervasyon { get; set; }
            public Rezervation Rezervation { get; set; }
        }
    }
}
