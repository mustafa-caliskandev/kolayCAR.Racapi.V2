using Newtonsoft.Json;
using System;

namespace kolayCAR.Broker.AWS.Models.AwsModels.Kinesis
{
    /// <summary>
    /// 
    /// </summary>
    public class ListingModel : IKinesisModel
    {
        /// <summary>
        /// Login olan kullanıcılara ait unique kullanıcı id'si
        /// </summary>
        [JsonProperty("user")]
        public string User { get; set; }

        /// <summary>
        /// Müşteri siteyi yada app'i açtığı zaman oluşan yaklaşık yarım saat boyunca yaptığı her işlemde her tabloya gönderilen, o yarım saate unique 11 haneli bir kod. Kullanıcının site içi hareketlerini tablolardaki session bilgisi üzerinden birbirine bağlayarak takip etmeyi mümkün kılıyor.
        /// </summary>
        [JsonProperty("session")]
        public string Session { get; set; }

        /// <summary>
        /// İşlemin yapıldığu cihaza ait unique kod
        /// </summary>
        [JsonProperty("device")]
        public string Device { get; set; }

        /// <summary>
        /// Browser/MobileBrowser/Android/iOS
        /// </summary>
        [JsonProperty("device_type")]
        public string DeviceType { get; set; }

        /// <summary>
        /// İlgili Tabloya kaydın atıldığı date bilgisi
        /// </summary>
        [JsonProperty("date")]
        public DateTime Date { get; set; }

        /// <summary>
        /// İşlemin yapıldığı Environment (Local/Stage/Production)
        /// </summary>
        [JsonProperty("application_environment")]
        public string ApplicationEnv { get; set; }

        /// <summary>
        /// Araç Alış Günü
        /// </summary>
        [JsonProperty("pickup_date")]
        public DateTime PickupDate { get; set; }

        /// <summary>
        /// Araç Bırakış Günü
        /// </summary>
        [JsonProperty("dropoff_date")]
        public DateTime DropDate { get; set; }

        /// <summary>
        /// Pick-up Point Id değeri
        /// </summary>
        [JsonProperty("pickup_point_id")]
        public int PickupPointId { get; set; }

        /// <summary>
        /// Pick-up Point
        /// </summary>
        [JsonProperty("pickup_point")]
        public string PickupPoint { get; set; }

        /// <summary>
        /// Araç Alış yeri
        /// </summary>
        [JsonProperty("drop_off_id")]
        public int DropPointId { get; set; }

        /// <summary>
        /// Araç Alış yeri
        /// </summary>
        [JsonProperty("dropoff_point")]
        public string DropPoint { get; set; }

        /// <summary>
        /// Araç Alış Yeri Havalimanı mı?
        /// </summary>
        [JsonProperty("is_pickup_airport")]
        public bool IsPickupAirport { get; set; }

        /// <summary>
        /// Araç Bırakış Yeri Havalimanı mı?
        /// </summary>
        [JsonProperty("is_dropoff_airport")]
        public bool IsDropAirport { get; set; }

        /// <summary>
        /// Araç Alış ve Bırakış yeri aynı mı?
        /// </summary>
        [JsonProperty("is_pickup_dropoff_same")]
        public bool IsPickupDropSame { get; set; }

        /// <summary>
        /// Araç Kiralanan Toplam Gün Sayısı
        /// </summary>
        [JsonProperty("rental_period")]
        public int RentalPeriod { get; set; }

        /// <summary>
        /// Boolean, True / False
        /// </summary>
        [JsonProperty("is_filtered")]
        public bool IsFiltered { get; set; }

        /// <summary>
        /// "Eğer filtre yapıldıysa filterede seçilen alanlar: Struct şeklinde[Rental Firms:, min_price: 1000 max_price:5000, min_km: 6000 max_price:10000, car_features = 'Benzin/Dizel'] vs gibi"
        /// "{Rental Firms -&gt; {Garenta,Easygo}, min_price: 1000, max_price ->; 10000 min_km ->; 6000 max_km ->; 15000, ....}"
        /// </summary>
        [JsonProperty("filters")]
        public FilterModel Filters { get; set; }

        /// <summary>
        /// Array her bir elemanı tek bir Araç
        /// { Marka: Fiat, Model: Egea ,Supplier: Garenta, Car Group: Ekonomik, Fuel Type:Benzin, Gear Type: Manuel,Cancellation Policy: Free Cancel, People &amp; Luggage:{people:5, luggage:3},Car Type:Sedan, Deposit (TL):1500, Total Km:1000, Minimum Driver's Age:24,Minumum Driving Licence (Years):3,Daily Price (TL): 700,Total Price (TL):1600, Drop Price (TL):200, Office Service Fee (TL):300 }
        /// </summary>
        [JsonProperty("listed_cars")]
        public CarModel[] ListedCars { get; set; }
    }
}
