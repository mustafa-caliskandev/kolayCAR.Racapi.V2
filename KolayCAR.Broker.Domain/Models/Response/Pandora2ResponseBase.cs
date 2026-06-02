using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class Pandora2ResponseBase
    {
        public class Token
        {
            public string token_type { get; set; }
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
        }

        public class Error
        {
            public int error_code { get; set; }
            public string error_message { get; set; }
        }

        public class Country
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
        }

        public class Location
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public int CountryId { get; set; }
            public string CountryCode { get; set; }
            public string Name { get; set; }
            public string LocationSource { get; set; }
        }

        public class Supplier
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Logo { get; set; }
            public string Phone { get; set; }
            public string Address { get; set; }
            public string SpecialInstructions { get; set; }
        }

        public class Fee
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public int Quantity { get; set; }
            public string NetAmount { get; set; }
            public string AmountTotal { get; set; }
            public bool? Mandatory { get; set; }
            public bool? IncludedInVehiclePrice { get; set; }
            public string RentalPrice { get; set; }
            public string ExcessAmount { get; set; }
        }

        public class Service
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int Quantity { get; set; }
            public string NetAmount { get; set; }
            public string AmountTotal { get; set; }
        }

        public class ExtraGroup
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Extra
        {
            public ExtraGroup Group { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int? MaxQuantity { get; set; }
            public string ImgSrc { get; set; }
            public string Currency { get; set; }
            public string NetAmount { get; set; } //Price without commissions
            public string TotalAmount { get; set; } //Price including commissions
            public string PaymentType { get; set; } //Enum: Now, Local
            public string ExcessAmount { get; set; }
        }

        public class Lookup
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class FuelLookup : Lookup
        {
            public string FuelPolicy { get; set; }
        }

        public class MileagePolicy
        {
            public string MileageIncluded { get; set; }
            public string DistanceUnit { get; set; }
            public string ExcessMileageRate { get; set; }
        }

        public class AvailableVehicle
        {
            public string Id { get; set; }
            public string SIPP { get; set; }
            public string ModelName { get; set; }
            public string ModelImageURL { get; set; }
            public string ModelThumbImageURL { get; set; }
            public Supplier Supplier { get; set; }
            public string Currency { get; set; }
            public int DaysForPayment { get; set; }
            public string NetDailyRentAmount { get; set; } //Günlük araç kiralama Ücreti (komisyonsuz)
            public string NetTotalRentAmount { get; set; } //Toplam araç kiralama Ücreti (komisyonsuz)
            public string DailyRentAmount { get; set; } //Günlük araç kiralama Ücreti (komisyon dahil)
            public string TotalRentAmount { get; set; }  //Toplam araç kiralama Ücreti (komisyon dahil)
            public string CdwExcess { get; set; }
            public string Deposit { get; set; }
            public Fee DropOffFee { get; set; }
            public Fee ServiceFee { get; set; }
            public List<Fee> OtherFees { get; set; }
            public List<Service> IncludedServices { get; set; }
            public List<Extra> Extras { get; set; }
            public List<Extra> IncludedExtras { get; set; }
            public string NetTotalAmount { get; set; } //Toplam Ücret (komisyonsuz)
            public string TotalAmount { get; set; } //Toplam ücret (komisyon dahil)
            public Lookup CarTransmissionType { get; set; }
            public FuelLookup FuelType { get; set; }
            public int? MinLicenseAge { get; set; }
            public int? MinDriverAge { get; set; }
            public int? MaxDriverAge { get; set; }
            public int? SmallBagsCapacity { get; set; }
            public int? BigBagsCapacity { get; set; }
            public int? PassengerCapacity { get; set; }
            public bool AirConditioning { get; set; }
            public string DoorCount { get; set; }
            public bool? BuiltInGps { get; set; }
            public MileagePolicy MileagePolicy { get; set; }
        }

        public class PriceResponse
        {
            public string Currency { get; set; }
            public string NetTotalRent { get; set; }
            public string TotalRent { get; set; }
            public string NetTotalAddition { get; set; }
            public string TotalAddition { get; set; }
            public Fee DropOffFee { get; set; }
            public Fee ServiceFee { get; set; }
            public string AgencyCommission { get; set; }
            public string NetTotal { get; set; }
            public string Total { get; set; }
        }

        public class BookingCreateResponse : PriceResponse
        {
            public int Id { get; set; }
            public BookingFiles Files { get; set; }
        }

        public class BookingFiles
        {
            [JsonProperty("ReservationForm")]
            public string ReservationForm { get; set; }
            public string Contract { get; set; }
            public string Bill { get; set; }
        }

        public class CancelResponse
        {
            public int Id { get; set; }
            public string Status { get; set; }
            public List<string> CancellationInvoices { get; set; }
        }
    }
}
