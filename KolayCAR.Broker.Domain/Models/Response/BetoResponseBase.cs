using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class BetoResponseBase
    {
        public class AuthResponseBase
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
        }

        public class LocationResponse
        {
            public string KODU { get; set; }
            public string YER_TR { get; set; }
            public string YER_EN { get; set; }
            public string LOCATION_NAME { get; set; }
            public string ADRES_TR { get; set; }
            public string ADRES_EN { get; set; }
            public string TEL { get; set; }
            public string EMAIL { get; set; }
        }

        public class ExtraResponse
        {
            public string MASRAFKODU { get; set; }
            public string EKMASRAF_TR { get; set; }
            public string ACIKLAMA_TR { get; set; }
            public string ACIKLAMA_EN { get; set; }
            public string DROPSAYISI { get; set; }
            public string GUNLUK { get; set; }
            public string EKMASRAF_EN { get; set; }
            public string RESIM { get; set; }
            public string MOBILERESIM { get; set; }
            public string DEGISKEN_FIYAT { get; set; }
            public string BIRIM { get; set; }
            public string FIYAT { get; set; }
            public string SONFIYAT { get; set; }
        }

        public class AvaibilityVehicleResponse
        {
            public string SINIF { get; set; }
            public string ARACNO { get; set; }
            public string ARACADI { get; set; }
            public string VITES_TR { get; set; }
            public string VITES_EN { get; set; }
            public string KLIMA_TR { get; set; }
            public string KLIMA_EN { get; set; }
            public string YAKIT_TR { get; set; }
            public string YAKIT_EN { get; set; }
            public string GOVDETIPI { get; set; }
            public string RESIM { get; set; }
            public string KISI { get; set; }
            public string BAGAJ { get; set; }
            public string KAPI { get; set; }
            public string YASACIKLAMA_TR { get; set; }
            public string YASACIKLAMA_EN { get; set; }
            public string YASSINIRI { get; set; }
            public string EHLIYET_YIL { get; set; }
            public string SINIFBASLIK_TR { get; set; }
            public string SINIFBASLIK_EN { get; set; }
            public string ABSS { get; set; }
            public string HAVAYASTIGI { get; set; }
            public string CD { get; set; }
            public string DERI { get; set; }
            public string HIDROLIK { get; set; }
            public string SUNROOF { get; set; }
            public string MODEL { get; set; }
            public string MOBILERESIM { get; set; }
            public string KAR_LASTIK { get; set; }
            public string DAHIL1_TR { get; set; }
            public string DAHIL1_EN { get; set; }
            public string DAHIL2_TR { get; set; }
            public string DAHIL2_EN { get; set; }
            public string DAHIL3_TR { get; set; }
            public string DAHIL3_EN { get; set; }
            public string DAHIL4_TR { get; set; }
            public string DAHIL4_EN { get; set; }
            public string DAHIL5_TR { get; set; }
            public string DAHIL5_EN { get; set; }
            public string DAHIL6_TR { get; set; }
            public string DAHIL6_EN { get; set; }
            public string KM_LIMIT { get; set; }
            public string SUITABLE { get; set; }
            public string GUN { get; set; }
            public string OFISTUTAR { get; set; }
            public string HEMENTUTAR { get; set; }
            public string DROPUCRET { get; set; }
            public string KAZANC { get; set; }
            public string TUTAR { get; set; }
            public string MARKA { get; set; }
            public string DEPOZIT { get; set; }
        }

        public class ReservationResponse
        {
            public string TARIH { get; set; }
            public string REZERVNO { get; set; }
            public string SINIFI { get; set; }
            public string ADISOYADI { get; set; }
            public string ULKE { get; set; }
            public string TCKIMLIK { get; set; }
            public string PASSPORTNO { get; set; }
            public string GSM { get; set; }
            public string ALISYERI { get; set; }
            public string ALISYERI_ADI { get; set; }
            public string REZERVBASTARIH { get; set; }
            public string BASSAAT { get; set; }
            public string IADEYERI { get; set; }
            public string IADEYERI_ADI { get; set; }
            public string REZERVBITTARIH { get; set; }
            public string BITSAAT { get; set; }
            public string GUN { get; set; }
            public string FIYAT { get; set; }
            public string GUNLUKFIYAT { get; set; }
            public string EPOSTA { get; set; }
            public string DURUM { get; set; }
            public string BROKER_NAME { get; set; }
            public string PARA_BIRIMI { get; set; }
            public string TAHSIL_EDILEN { get; set; }
            public List<object> EkHizmetler { get; set; }
        }
    }
}
