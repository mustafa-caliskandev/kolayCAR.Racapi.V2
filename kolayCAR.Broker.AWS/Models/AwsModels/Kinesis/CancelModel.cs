using Newtonsoft.Json;
using System;

namespace kolayCAR.Broker.AWS.Models.AwsModels.Kinesis
{
    /// <summary>
    /// 
    /// </summary>
    public class CancelModel : IKinesisModel
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
        /// Reservation Token alanı
        /// </summary>
        [JsonProperty("vehicle_token")]
        public string ReservationToken { get; set; }

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
        /// Winter Tire / Child Seat / Baby Seat / Additional Driver
        /// </summary>
        [JsonProperty("additional_products")]
        public string[] AdditionalProducts { get; set; }

        /// <summary>
        /// "Checkout states: 1/6/8
        ///1 ->; Kullanıcı ödeme sayfasını açtığı zaman state kolonu = 1 olarak bir satır gönderilir.
        ///6 ->; Kullanıcı ödeme yap butonuna bastığı zaman, ödeme adımına yönlendirildiği durumda state = 6 olarak bir satır gönderilir.
        ///8 ->; Ödeme adımı başarılı olduğu zaman state = 8 olarak bir satır gönderilir."
        /// </summary>
        [JsonProperty("state")]
        public int State { get; set; }

        /// <summary>
        /// Her order için unique olan code. State6 esnasında oluşur ve ödeme başarılı olursa State8'de de gönderilir
        /// 219D7539A29
        /// </summary>
        [JsonProperty("order_code")]
        public string OrderCode { get; set; }

        /// <summary>
        /// Array her bir elemanı tek bir Araç
        /// { Marka: Fiat, Model: Egea ,Supplier: Garenta, Car Group: Ekonomik, Fuel Type:Benzin, Gear Type: Manuel,Cancellation Policy: Free Cancel, People &amp; Luggage:{people:5, luggage:3},Car Type:Sedan, Deposit (TL):1500, Total Km:1000, Minimum Driver's Age:24,Minumum Driving Licence (Years):3,Daily Price (TL): 700,Total Price (TL):1600, Drop Price (TL):200, Office Service Fee (TL):300 }
        /// </summary>
        [JsonProperty("car")]
        public CarModel Car { get; set; }

        /// <summary>
        /// Kulanıcı bilgilerinin yer aldığı kolon. State1'de henüz elimizde kullanıcı bilgileri olmadığı için boş gelir. State6 ve state8'de dolu gelir.
        /// "{mail=lutfullah.cinar@obilet.com, phone=905312821982, payment_method=Masterpass Ödeme Sistemi, payment_card=Garanti Bankasi Paracard - Alt DC, payment_type=cash, late_charge=0.000000000000000000, has_newsletter_subscription=false}"
        /// </summary>
        [JsonProperty("user_detail")]
        public UserDetailModel UserDetail { get; set; }

        /// <summary>
        /// Kupon bilgileri
        /// [{coupon_code=NFCEWU49FV, campaign_name= Çalışan Kuponu - Otobüs, discount_amount=80.000000000000000000, payment_type=ObiletNonRefundable}]
        /// </summary>
        [JsonProperty("coupons")]
        public CouponModel[] Coupons { get; set; }

        /// <summary>
        /// Supplier commission
        /// </summary>
        [JsonProperty("api_price")]
        public decimal VendorCommission { get; set; }

        /// <summary>
        /// OB Commission
        /// </summary>
        [JsonProperty("sale_price")]
        public decimal ObCommission { get; set; }

        /// <summary>
        /// geri ödenecek tutar
        /// </summary>
        [JsonProperty("refunded_amount")]
        public decimal RefundedAmount { get; set; }

        /// <summary>
        /// iptal nasıl gerçekleşti? Online/System
        /// </summary>
        [JsonProperty("channel")]
        public string Channel { get; set; }

        /// <summary>
        /// İptal Sebebi
        /// </summary>
        [JsonProperty("cancel_reason")]
        public string CancelReason { get; set; }
    }
}
