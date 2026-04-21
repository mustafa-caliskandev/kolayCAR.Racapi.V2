using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class AyesResponseBase
    {
        public List<AyesLocation> AyesLocations { get; set; }
        public List<AyesExtra> AyesExtras { get; set; }
        public AyesPrice fiyat { get; set; }
        public AyesBranchContactInformation sube_iletisim_bilgileri { get; set; }
        public string rezervasyon_numarasi { get; set; }
        public bool islem_durumu { get; set; }
    }

    public class AyesVehicleResponse
    {
        public List<AyesVehicle> araclar { get; set; }
        public AyesReservationInfo rezervasyon_bilgileri { get; set; }
    }

    public class AyesReservationResponse
    {
        public AyesReservationInfo rezervasyon_bilgileri { get; set; }
        public AyesVehicle arac { get; set; }
        public AyesPrice fiyat { get; set; }
        public AyesBranchContactInformation sube_iletisim_bilgileri { get; set; }
        public string rezervasyon_numarasi { get; set; }
        public bool islem_durumu { get; set; }
        public string durum { get; set; }
        public string hata_kodu { get; set; }
        public string mesaj { get; set; }
    }

    public class AyesLocation
    {
        public string id { get; set; }
        public string baslik { get; set; }
    }

    public class AyesExtra
    {
        public string id { get; set; }
        public string baslik { get; set; }
        public string bilgi { get; set; }
        public string ucret { get; set; }
        public string tek_odeme { get; set; }
        public string zorunlu { get; set; }
    }

    public class AyesVehicle : VehicleItems
    {
        public string filo_id { get; set; }
        //public string baslik { get; set; }
        //public string gorsel { get; set; }
        //public string yakit { get; set; }
        //public string vites { get; set; }
        //public string klima { get; set; }
        //public string canta { get; set; }
        public int km_limit { get; set; }
        public int drop_goster { get; set; }
        public string drop_ucreti { get; set; }
        public string toplam_ucret { get; set; }
        public string gunluk_ucret { get; set; }
        public string arac_id { get; set; }
        //public string kisi_sayisi { get; set; }
        //public string kapi_sayisi { get; set; }
        public AyesVehicleGroup grup { get; set; }
        //public string surucu_yasi { get; set; }
        //public string ehliyet_yasi { get; set; }
    }

    public class AyesVehicleGroup
    {
        public int id { get; set; }
        public string isim { get; set; }
    }

    public class AyesReservationInfo
    {
        public int toplam_gun { get; set; }
        public string alis_tarihi_saati { get; set; }
        public string teslim_tarihi_saati { get; set; }
        public string alis_yeri { get; set; }
        public string alis_yeri_isim { get; set; }
        public string teslim_yeri { get; set; }
        public string teslim_yeri_isim { get; set; }
    }

    public class AyesPrice
    {
        public int drop_goster { get; set; }
        public string toplam_ucret { get; set; }
        public string gunluk_ucret { get; set; }
        public bool hgs { get; set; }
        public string hgs_ucret { get; set; }
        public string ekstralar_id { get; set; }
        public string ekstra_tutar { get; set; }
        public string genel_toplam { get; set; }
    }

    public class AyesBranchContactInformation
    {
        public string yetkili { get; set; }
        public string yetkili_telefon { get; set; }
        public string sube_telefon { get; set; }
        public string email { get; set; }
        public string lokasyon { get; set; }
        public string sube { get; set; }
        public string adres { get; set; }
    }

    public class Grup
    {
        public int id { get; set; }
        public string isim { get; set; }
    }

    public class VehicleList : VehicleItems
    {
        public string id { get; set; }
        //public string baslik { get; set; }
        public string sipp_kodu { get; set; }
        public Grup grup { get; set; }
        //public string gorsel { get; set; }
        //public string yakit { get; set; }
        //public string vites { get; set; }
        //public string klima { get; set; }
        //public string kisi_sayisi { get; set; }
        //public string kapi_sayisi { get; set; }
        //public string canta { get; set; }
        //public string surucu_yasi { get; set; }
        //public string ehliyet_yasi { get; set; }
    }

    public class VehicleItems
    {
        public string baslik { get; set; }
        public string gorsel { get; set; }
        public string yakit { get; set; }
        public string vites { get; set; }
        public string klima { get; set; }
        public string kisi_sayisi { get; set; }
        public string kapi_sayisi { get; set; }
        public string canta { get; set; }
        public string surucu_yasi { get; set; }
        public string ehliyet_yasi { get; set; }
        public float provizyon { get; set; }
    }
}
