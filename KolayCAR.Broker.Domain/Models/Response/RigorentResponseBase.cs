using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class RigorentResponseBase
    {
        public MyCarRent MyCarRent { get; set; }
        public Mycarturassist Mycarturassist { get; set; }
        public RigorentRezSonuc Rez_Sonuc { get; set; }
    }

    public class RigorentLocation
    {
        public string Kayit_No { get; set; }
        public string Bolge { get; set; }
        public string Phone { get; set; }
        public string Openning { get; set; }
        public string Closing { get; set; }
        public string Nokta { get; set; }
        public string IATA { get; set; }
        public string Ulke { get; set; }
        public string Google_Konum { get; set; }
        [JsonProperty("@id")]
        public string _id { get; set; }
    }

    public class RigorentVehicle
    {
        public string Rez_ID { get; set; }
        public string Arac_Kayit_No { get; set; }
        public string Arac_Marka { get; set; }
        public string Arac_Tipi { get; set; }
        public string Arac_Yakit { get; set; }
        public string Arac_Vites { get; set; }
        public string Arac_Koltuk { get; set; }
        public string Arac_Kapi { get; set; }
        public string Arac_K_Bagaj { get; set; }
        public string Arac_B_Bagaj { get; set; }
        public string Arac_Resim { get; set; }
        public string Gunluk_Kur { get; set; }
        public string Doviz { get; set; }
        public string Gunluk_Kira { get; set; }
        public string Toplam_Kira_Bedeli { get; set; }
        public string Bebek_Koltugu_Bedeli { get; set; }
        public string Navigasyon_Bedeli { get; set; }
        public string Ozel_Sofor_Bedeli { get; set; }
        public string Ek_Sofor_Bedeli { get; set; }
        public string Drop_Bedeli { get; set; }
        public string Genel_Toplam { get; set; }
        public string Tedarikci { get; set; }
        public string Tedarikci_Isim { get; set; }
        public string LCF_Bedeli { get; set; }
        public string SCDW_Bedeli { get; set; }
        public string Muaifiyetsiz_Bedeli { get; set; }
        public string Arac_Kayit_No_Takip { get; set; }
        public string Arac_Grup_Adi { get; set; }
        public string Adres { get; set; }
        public string Ilce_Semt { get; set; }
        public string Sehir { get; set; }
        public string Konum { get; set; }
        public string Telefon { get; set; }
        public string KM_Limiti { get; set; }
        public string Sinirsiz_KM_Bedeli { get; set; }
        public string Kira_Gun { get; set; }
        public string Araclar_Kayit_No { get; set; }
        public string Sahis_3_Bedeli { get; set; }
        public string Adres_Tes { get; set; }
        public string Km_250_Bedeli { get; set; }
        public string Km_500_Bedeli { get; set; }
        public string Km_1000_Bedeli { get; set; }
        public string Ust_Fiyat { get; set; }
        public string SK_Oran { get; set; }
        public string SIPP { get; set; }
        public string Frm_Mail { get; set; }
        public string Tablo_T { get; set; }
        public string Takip_T { get; set; }
        public string Liste_Ust { get; set; }
        public string Ust_Goster { get; set; }
        public string Iptal_Sigortasi { get; set; }
        public string Indirim_Toplam { get; set; }
        public string Grup_Tur { get; set; }
        public string I_O_Fiyat { get; set; }
        public string A { get; set; }
        public string T { get; set; }
        public string Yas { get; set; }
        public string Ehliyet { get; set; }
        public string Fiyat_ID { get; set; }
        public string Maliyet_ID { get; set; }
        public string Rakip { get; set; }
        public string Rez_Arac_Kayit_No { get; set; }
        public string Rez_Arac_Kayit_Deger { get; set; }
        public string Dvz_Kisaltma { get; set; }
        public string Lst_Gunluk_M { get; set; }
        public string Maliyet_Ilave { get; set; }
        public string Dinamik_Oncesi { get; set; }
        public string Piyasa_Oran { get; set; }
        public string Iskonto_Anlasmali { get; set; }
        public string TA_Maliyet_Oran { get; set; }
        public string Piyasa_Orani_Eklenen { get; set; }
        public string Ek_Oran_Eklenen { get; set; }
        public string Rakip_Analizi_Eklenen { get; set; }
        public string Ek_Oran { get; set; }
        public string Anlasmali_Indirim_Toplami { get; set; }
        public string Fiyat_Artir { get; set; }
        public string Listeli_Arac { get; set; }
        public string Path { get; set; }
        [JsonProperty("@id")]
        public string _id { get; set; }
    }

    public class RigorentVehicleListItem
    {
        public string Kayit_No { get; set; }
        public string Marka { get; set; }
        public string Tipi { get; set; }
        public string Yakit_Turu { get; set; }
        public string Vites { get; set; }
        public string Grup_Adi { get; set; }
        public string Koltuk { get; set; }
        public string Kapi { get; set; }
        public string Buyuk_Bagaj { get; set; }
        public string Kucuk_Bagaj { get; set; }
        public string Resim { get; set; }
        public string SIPP { get; set; }
        [JsonProperty("@id")]
        public string _id { get; set; }
    }

    public class RigorentRezervasyon
    {
        public bool Sonuc { get; set; }
        public string _id { get; set; }
    }

    public class RigorentRezSonuc
    {
        public RigorentRezervasyon Rezervasyon { get; set; }
    }

    public class Mycarturassist
    {
        public List<RigorentVehicle> Arac { get; set; }
    }

    public class MyCarRent
    {
        public List<RigorentLocation> Sube { get; set; }
        public List<RigorentVehicleListItem> Arac { get; set; }
    }
}
