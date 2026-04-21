using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class OtocarResponseBase
    {
        public class LocationResponse
        {
            public ArrayOfLocationNode ArrayOfLocation { get; set; }
        }
        public class VehicleResponse
        {
            public ArrayOfFleetNode ArrayOfFleet { get; set; }
        }
        public class AvaibilityVehicleResponse
        {
            public ArrayOfAraclarNode ArrayOfAraclar { get; set; }
        }
        public class ReservationResponse
        {
            public ArrayOfRezervasyonNode ArrayOfRezervasyon { get; set; }
        }
        public class ReservationCancelResponse
        {
            public ArrayOfRezervasyonCancelNode ArrayOfRezervasyon { get; set; }
        }
        public class ArrayOfLocationNode
        {
            public List<Location> Location { get; set; }
        }

        public class ArrayOfFleetNode
        {
            public List<Fleet> Fleet { get; set; }
        }

        public class ArrayOfAraclarNode
        {
            public List<araclar> araclar { get; set; }
        }
        public class ArrayOfRezervasyonNode
        {
            public rezervasyon rezervasyon { get; set; }
        }

        public class ArrayOfRezervasyonCancelNode
        {
            public rezervasyonCancel rezervasyon { get; set; }
        }
        public class Location
        {
            public int Id { get; set; }
            public string branch { get; set; }
            public string adres { get; set; }
            public string telefon { get; set; }
            public string mail { get; set; }
            public string calismasaatleri { get; set; }
            public string delivery_type { get; set; }
            public string koordinat { get; set; }
        }

        public class Fleet
        {
            public int Id { get; set; }
            public string brand { get; set; }
            public string model { get; set; }
            public string gear { get; set; }
            public string klima { get; set; }
            public string yolcu { get; set; }
            public string bagaj { get; set; }
            public string fuel { get; set; }
            public string carClass { get; set; }
            public int yas { get; set; }
            public int ehliyet { get; set; }
            public float gunluk_km { get; set; }
            public float provizyon { get; set; }
        }

        public class araclar : Fleet
        {
            public int arac_Id { get; set; }
            public int gun { get; set; }
            public string segment { get; set; }
            public float fiyat3 { get; set; }
            public float fiyat4_7 { get; set; }
            public float fiyat8_15 { get; set; }
            public float fiyat16_29 { get; set; }
            public float fiyat30 { get; set; }
            public float drop_ucret { get; set; }
            public string hizmet { get; set; }
            public float ek_sofor { get; set; }
            public float navigasyon { get; set; }
            public float bebek_koltuk { get; set; }
            public float lcf_sigorta { get; set; }
            public float superkasko { get; set; }
            public float daily_price_turkish_lira { get; set; }
            public string vehicle_picture { get; set; }
            public float total_price_turkish_lira { get; set; }
        }

        public class rezervasyon
        {
            public string rezervasyon_no { get; set; }
        }

        public class rezervasyonCancel
        {
            public string durum { get; set; }
        }





    }
}
