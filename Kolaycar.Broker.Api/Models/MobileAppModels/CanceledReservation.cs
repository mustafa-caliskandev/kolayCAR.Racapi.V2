using KolayCAR.Broker.Domain.Models.Requests;
using System;

namespace KolayCAR.Broker.API.Models.MobileAppModels
{
    public class CanceledReservation
    {
        public long Rezid { get; set; }
        public string Rezno { get; set; }       
        public bool Servisegonderildi { get; set; }       
        public int Aracid { get; set; }
        public DateTime Tarih { get; set; }
        public DateTime Alistarihi { get; set; }
        public DateTime Birakistarihi { get; set; }
        public int Alisyerid { get; set; }
        public int Birakisyerid { get; set; }
        public int Dovizid { get; set; }
        public int Kiralamasuresi { get; set; }
        public decimal Gunlukfiyat { get; set; }
        public decimal Extratutar { get; set; }
        public decimal Tekyontutar { get; set; }
        public decimal Toplamtutar { get; set; }
        public decimal Odenentutar { get; set; }
        public string Odemekartsahibi { get; set; }
        public string Odemekartno { get; set; }
        public string Extralar { get; set; }
        public byte? Musteritip { get; set; }
        public string Musteriad { get; set; }
        public string Musterisoyad { get; set; }
        public string Musteritelefon { get; set; }
        public string Musterieposta { get; set; }
        public string Musteritcpasaport { get; set; }
        public string Musteriadres { get; set; }
        public string Musteriunvan { get; set; }
        public string Musterivergidaire { get; set; }
        public string Musterivergino { get; set; }
        public string Musterigelisucusno { get; set; }
        public string Musteridonusucusno { get; set; }
        public string Musterinot { get; set; }
        public string Aracadi { get; set; }
        public string Alisyeri { get; set; }
        public string Birakisyeri { get; set; }
        public string Extraidlist { get; set; }
        public int Vendorid { get; set; }
        public int? Resstatusid { get; set; }
        public decimal? Servicecharge { get; set; }
        public decimal? Depositprice { get; set; }
        public DateTime? Customerbirthday { get; set; }
        public bool? Depositcreditcardrequired { get; set; }
        public decimal? Paymentamount { get; set; }
        public string Couponcode { get; set; }
        public decimal? Coupondiscountamount { get; set; }
        public decimal? Coupondiscountvalue { get; set; }
        public int? Coupondiscounttype { get; set; }
        public DateTime? Updatedate { get; set; }
        public int? Installmentcount { get; set; }
        public string Reservationtokentext { get; set; }
        public decimal? InstallmentCommissionAmount { get; set; }
    }
}
