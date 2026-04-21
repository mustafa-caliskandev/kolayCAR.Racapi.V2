using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class BetoRequestBase
    {
        public class AuthTokenRequest
        {
            public string username { get; set; }
            public string password { get; set; }
        }

        public class ExtraRequest
        {
            public int days { get; set; }
            public string currency { get; set; }
        }


        public class AvaibilitiyVehicleRequest
        {
            public string pickup_date { get; set; }
            public string pickup_time { get; set; }
            public string dropoff_date { get; set; }
            public string dropoff_time { get; set; }
            public string pickup_location { get; set; }
            public string Drop_location { get; set; }
            public string currency { get; set; }
        }


        public class PostReservationRequest
        {

            public string TARIH { get; set; }
            public string BROKER_TYPE { get; set; }
            public string BROKER_NAME { get; set; }
            public string ODEME_TURU { get; set; }
            public string REZERVNO { get; set; }
            public string ULKE { get; set; }
            public string ADISOYADI { get; set; }
            public string EPOSTA { get; set; }
            public string PASSPORTNO { get; set; }
            public string TCKIMLIK { get; set; }
            public string GSM { get; set; }
            public string SINIFI { get; set; }
            public string ARACNO { get; set; }
            public string ALISYERI { get; set; }
            public string REZERVBASTARIH { get; set; }
            public string BASSAAT { get; set; }
            public string IADEYERI { get; set; }
            public string REZERVBITTARIH { get; set; }
            public string BITSAAT { get; set; }
            public string GUN { get; set; }
            public string GUNLUKFIYAT { get; set; }
            public string FIYAT { get; set; }
            public string TAHSIL_EDILEN { get; set; }
            public string DROPUCRET { get; set; }
            public string PARA_BIRIMI { get; set; }
            public string tolerance_hours { get; set; }
            public string fiyat_kontrol { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public List<EkHizmetler> EkHizmetler { get; set; }
            //public string BROKER_HK { get; set; }

        }
        public class EkHizmetler
        {
            public string REZERVNO { get; set; }
            public string MASRAFKODU { get; set; }
            public string MASRAFADI { get; set; }
            public string MIKTAR { get; set; }
            public string BIRIMTUTAR { get; set; }
            public string TUTAR { get; set; }
        }

        public class PostCancelReservationRequest
        {
            public string RESERVNO { get; set; }
            public string TARIH { get; set; }
            public string ACIKLAMA { get; set; }
        }
    }
}
