using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class RecordGoResponseBase
    {
        public class ClosingHours
        {
            [JsonProperty("WeekDays")]
            public string WeekDays { get; set; }

            [JsonProperty("Holidays")]
            public string Holidays { get; set; }
        }

        public class Country
        {
            [JsonProperty("CountryCode")]
            public string CountryCode { get; set; }

            [JsonProperty("Locations")]
            public List<Location> Locations { get; set; }
        }

        public class Location
        {
            [JsonProperty("Name")]
            public string Name { get; set; }

            [JsonProperty("Coordinates")]
            public string Coordinates { get; set; }

            [JsonProperty("Address")]
            public string Address { get; set; }

            [JsonProperty("Counter")]
            public string Counter { get; set; }

            [JsonProperty("Parking")]
            public string Parking { get; set; }

            [JsonProperty("Email")]
            public string Email { get; set; }

            [JsonProperty("PhoneNumber")]
            public List<string> PhoneNumber { get; set; }

            [JsonProperty("OpeningHours")]
            public OpeningHours OpeningHours { get; set; }

            [JsonProperty("ClosingHours")]
            public ClosingHours ClosingHours { get; set; }

            [JsonProperty("XMLCode")]
            public string XMLCode { get; set; }

            [JsonProperty("Key Box")]
            public string KeyBox { get; set; }

            [JsonProperty("ExtrasList")]
            public List<string> ExtrasList { get; set; }

            [JsonProperty("PhoneNumbers")]
            public List<string> PhoneNumbers { get; set; }
        }

        public class OpeningHours
        {
            [JsonProperty("WeekDays")]
            public string WeekDays { get; set; }

            [JsonProperty("Christmas Eve")]
            public string ChristmasEve { get; set; }

            [JsonProperty("Christmas")]
            public string Christmas { get; set; }

            [JsonProperty("New Years Eve")]
            public string NewYearsEve { get; set; }

            [JsonProperty("New Years")]
            public string NewYears { get; set; }
        }

        public class Countries
        {
            [JsonProperty("CountryList")]
            public List<Country> CountryList { get; set; }
        }


        //-- VEHICLE GROUPS --//
        public class VehicleGroups
        {
            [JsonProperty("VehicleGroupsList")]
            public List<VehicleGroupsList> VehicleGroupsList { get; set; }
        }

        public class VehicleGroup
        {
            [JsonProperty("Acriss")]
            public string Acriss { get; set; }

            [JsonProperty("GroupID")]
            public string GroupID { get; set; }

            [JsonProperty("Doors")]
            public int Doors { get; set; }

            [JsonProperty("Passengers")]
            public int Passengers { get; set; }

            [JsonProperty("Display Model")]
            public string DisplayModel { get; set; }

            [JsonProperty("Guaranteed Model")]
            public bool GuaranteedModel { get; set; }
        }

        public class VehicleGroupsList
        {
            [JsonProperty("CountryCode")]
            public string CountryCode { get; set; }

            [JsonProperty("VehicleGroups")]
            public List<VehicleGroup> VehicleGroups { get; set; }
        }

        //-- AUTHENTIICATION --//

        public class AuthResponse
        {
            [JsonProperty("access_token")]
            public string access_token { get; set; }

            [JsonProperty("expires_in")]
            public int expires_in { get; set; }

            [JsonProperty("token_type")]
            public string token_type { get; set; }

            [JsonProperty("scope")]
            public string scope { get; set; }
        }

        //-- AVAILABILTY RESPONSE --//
        public class Acriss
        {
            [JsonProperty("acrissId")]
            public int acrissId { get; set; }

            [JsonProperty("acrissCode")]
            public string acrissCode { get; set; }

            [JsonProperty("available")]
            public bool available { get; set; }

            [JsonProperty("salesGroup")]
            public List<string> salesGroup { get; set; }

            [JsonProperty("imagesArray")]
            public List<ImagesArray> imagesArray { get; set; }

            [JsonProperty("acrissSeats")]
            public int acrissSeats { get; set; }

            [JsonProperty("gearboxType")]
            public string gearboxType { get; set; }

            [JsonProperty("acrissSuitcase")]
            public int acrissSuitcase { get; set; }

            [JsonProperty("acrissDoors")]
            public int acrissDoors { get; set; }

            [JsonProperty("fromHeight")]
            public int fromHeight { get; set; }

            [JsonProperty("toHeight")]
            public int toHeight { get; set; }

            [JsonProperty("fromWidth")]
            public int fromWidth { get; set; }

            [JsonProperty("toWidth")]
            public int toWidth { get; set; }

            [JsonProperty("fromLength")]
            public int fromLength { get; set; }

            [JsonProperty("toLength")]
            public int toLength { get; set; }

            [JsonProperty("fromTareWeight")]
            public int fromTareWeight { get; set; }

            [JsonProperty("toTareWeight")]
            public int toTareWeight { get; set; }

            [JsonProperty("fromMMA")]
            public int fromMMA { get; set; }

            [JsonProperty("toMMA")]
            public int toMMA { get; set; }

            [JsonProperty("acrissGreenGroup")]
            public string acrissGreenGroup { get; set; }

            [JsonProperty("products")]
            public List<Product> products { get; set; }
        }

        public class ImagesArray
        {
            [JsonProperty("acrissImgUrl")]
            public string acrissImgUrl { get; set; }

            [JsonProperty("acrissDisplayName")]
            public string acrissDisplayName { get; set; }

            [JsonProperty("isDefault")]
            public bool isDefault { get; set; }
        }

        public class KmPolicyComercial
        {
            [JsonProperty("kmPolicyVer")]
            public int kmPolicyVer { get; set; }

            [JsonProperty("kmPolicyName")]
            public string kmPolicyName { get; set; }

            [JsonProperty("kmLimited")]
            public int kmLimited { get; set; }

            [JsonProperty("kmMaxDaily")]
            public int kmMaxDaily { get; set; }

            [JsonProperty("kmPolicyTTCC")]
            public string kmPolicyTTCC { get; set; }

            [JsonProperty("kmReview")]
            public int kmReview { get; set; }
        }

        public class PreauthExcess
        {
            [JsonProperty("type")]
            public string type { get; set; }

            [JsonProperty("value")]
            public int value { get; set; }
        }

        public class Product
        {
            [JsonProperty("product")]
            public Product2 product { get; set; }

            [JsonProperty("available")]
            public bool available { get; set; }

            [JsonProperty("rateProdVer")]
            public string rateProdVer { get; set; }

            [JsonProperty("priceTaxIncDay")]
            public double priceTaxIncDay { get; set; }

            [JsonProperty("priceTaxIncBooking")]
            public double priceTaxIncBooking { get; set; }

            [JsonProperty("clubRecordPromo")]
            public int clubRecordPromo { get; set; }

            [JsonProperty("valueDto")]
            public int valueDto { get; set; }

            [JsonProperty("applicationDiscount")]
            public int applicationDiscount { get; set; }

            [JsonProperty("priceTaxIncDayDiscount")]
            public double priceTaxIncDayDiscount { get; set; }

            [JsonProperty("priceTaxIncBookingDiscount")]
            public double priceTaxIncBookingDiscount { get; set; }

            [JsonProperty("commissionMin")]
            public int commissionMin { get; set; }

            [JsonProperty("commissionMax")]
            public int commissionMax { get; set; }
        }

        public class Product2
        {
            [JsonProperty("productId")]
            public string productId { get; set; }

            [JsonProperty("productVer")]
            public int productVer { get; set; }

            [JsonProperty("productName")]
            public string productName { get; set; }

            [JsonProperty("productDescription")]
            public string productDescription { get; set; }

            [JsonProperty("productSubtitle")]
            public string productSubtitle { get; set; }

            [JsonProperty("productTTCC")]
            public string productTTCC { get; set; }

            [JsonProperty("minDriverLicense")]
            public int minDriverLicense { get; set; }

            [JsonProperty("minAgeProduct")]
            public int minAgeProduct { get; set; }

            [JsonProperty("maxAgeProduct")]
            public int maxAgeProduct { get; set; }

            [JsonProperty("refuelPolicyComercial")]
            public RefuelPolicyComercial refuelPolicyComercial { get; set; }

            [JsonProperty("kmPolicyComercial")]
            public KmPolicyComercial kmPolicyComercial { get; set; }

            [JsonProperty("productComplementsIncluded")]
            public List<ProductComplementsIncluded> productComplementsIncluded { get; set; }
        }

        public class ProductComplementsIncluded
        {
            [JsonProperty("complementId")]
            public int complementId { get; set; }

            [JsonProperty("complementVer")]
            public int complementVer { get; set; }

            [JsonProperty("complementName")]
            public string complementName { get; set; }

            [JsonProperty("complementDescription")]
            public string complementDescription { get; set; }

            [JsonProperty("complementCategory")]
            public string complementCategory { get; set; }

            [JsonProperty("complementGroup")]
            public string complementGroup { get; set; }

            [JsonProperty("complementTTCC")]
            public string complementTTCC { get; set; }

            [JsonProperty("complementUnits")]
            public int complementUnits { get; set; }

            [JsonProperty("preauth&Excess")]
            public List<PreauthExcess> preauthExcess { get; set; }

            [JsonProperty("amountAdditionalKm")]
            public int amountAdditionalKm { get; set; }

            [JsonProperty("rateComplVer")]
            public string rateComplVer { get; set; }

            [JsonProperty("taxApplied")]
            public int taxApplied { get; set; }

            [JsonProperty("priceTaxIncDay")]
            public double priceTaxIncDay { get; set; }

            [JsonProperty("priceTaxIncComplement")]
            public double priceTaxIncComplement { get; set; }

            [JsonProperty("package")]
            public object package { get; set; }
        }

        public class RefuelPolicyComercial
        {
            [JsonProperty("refuelPolicyVer")]
            public int refuelPolicyVer { get; set; }

            [JsonProperty("refuelPolicyName")]
            public string refuelPolicyName { get; set; }

            [JsonProperty("refuelPolicyTTCC")]
            public string refuelPolicyTTCC { get; set; }
        }

        public class VehicleAvailabilityResponse
        {
            [JsonProperty("status")]
            public Status status { get; set; }

            [JsonProperty("sellCodeVer")]
            public string sellCodeVer { get; set; }

            [JsonProperty("acriss")]
            public List<Acriss> acriss { get; set; }

            [JsonProperty("pickUpbranchTTCC")]
            public string pickUpbranchTTCC { get; set; }

            [JsonProperty("dropOffbranchTTCC")]
            public string dropOffbranchTTCC { get; set; }
        }

        public class Status
        {
            [JsonProperty("idStatus")]
            public int idStatus { get; set; }

            [JsonProperty("detailedStatus")]
            public string detailedStatus { get; set; }
        }

        //-- RESERVATION RESPONSE --//
        public class ReservationResponse
        {
            [JsonProperty("status")]
            public Status status { get; set; }

            [JsonProperty("numVoucher")]
            public string numVoucher { get; set; }

            public class Status
            {
                [JsonProperty("idStatus")]
                public int idStatus { get; set; }

                [JsonProperty("detailedStatus")]
                public string detailedStatus { get; set; }
            }


        }

        //-- RESERVATION CANCEL RESPONSE --//
        public class ReservationCancelResponse
        {

            [JsonProperty("status")]
            public Status status { get; set; }

            [JsonProperty("cancelComplement")]
            public TComplement cancelComplement { get; set; }
            public class Status
            {
                [JsonProperty("idStatus")]
                public int idStatus { get; set; }

                [JsonProperty("detailedStatus")]
                public string detailedStatus { get; set; }
            }

            public class TComplement
            {
                public int complementId { get; set; }
                public int complementVer { get; set; }
                public string typeComplUnits { get; set; }
                public int complementUnits { get; set; }
                public int maxUnits { get; set; }
                public int rateComplVer { get; set; }
                public float taxApplied { get; set; }
                public float priceTaxIncDay { get; set; }
                public float priceTaxIncComplement { get; set; }
                public float valueDto { get; set; }
                public string typeDto { get; set; }
                public string applicationDiscount { get; set; }
                public float priceTaxIncDayDiscount { get; set; }
                public float priceTaxIncComplementDiscount { get; set; }
                public float commissionMin { get; set; }
                public float commissionMax { get; set; }
                public List<TComplement> package { get; set; }
            }
        }

        //-- EXTRAS RESPONSE --//
        public class RecordGoExtraResponse
        {
            public class ExtraListsbyCountry
            {
                [JsonProperty("CountryCode")]
                public string CountryCode { get; set; }

                [JsonProperty("ExtrasList")]
                public List<string> ExtrasList { get; set; }
            }

            public class Root
            {
                [JsonProperty("ExtraListsbyCountry")]
                public List<ExtraListsbyCountry> ExtraListsbyCountry { get; set; }
            }
        }
        //-- AVAILABLE EXTRAS RESPONSE --//
        public class AvailableExtrasResponse
        {
            [JsonProperty("status")]
            public Status status { get; set; }

            [JsonProperty("productAssociatedComplements")]
            public List<ProductAssociatedComplement> productAssociatedComplements { get; set; }

            [JsonProperty("productAutomaticComplements")]
            public List<ProductAutomaticComplement> productAutomaticComplements { get; set; }
            public class ProductAssociatedComplement
            {
                [JsonProperty("complementId")]
                public int complementId { get; set; }

                [JsonProperty("complementVer")]
                public int complementVer { get; set; }

                [JsonProperty("complementName")]
                public string complementName { get; set; }

                [JsonProperty("complementDescription")]
                public string complementDescription { get; set; }

                [JsonProperty("complementSubtitle")]
                public string complementSubtitle { get; set; }

                [JsonProperty("complementCategory")]
                public string complementCategory { get; set; }

                [JsonProperty("complementGroup")]
                public string complementGroup { get; set; }

                [JsonProperty("complementTTCC")]
                public string complementTTCC { get; set; }

                [JsonProperty("typeComplUnits")]
                public string typeComplUnits { get; set; }

                [JsonProperty("complementUnits")]
                public int complementUnits { get; set; }

                [JsonProperty("maxUnits")]
                public int maxUnits { get; set; }

                [JsonProperty("rateComplVer")]
                public int rateComplVer { get; set; }

                [JsonProperty("taxApplied")]
                public int taxApplied { get; set; }

                [JsonProperty("priceTaxIncDay")]
                public double priceTaxIncDay { get; set; }

                [JsonProperty("priceTaxIncComplement")]
                public double priceTaxIncComplement { get; set; }

                [JsonProperty("valueDto")]
                public int valueDto { get; set; }

                [JsonProperty("applicationDiscount")]
                public int applicationDiscount { get; set; }

                [JsonProperty("priceTaxIncDayDiscount")]
                public int priceTaxIncDayDiscount { get; set; }

                [JsonProperty("priceTaxIncComplementDiscount")]
                public int priceTaxIncComplementDiscount { get; set; }

                [JsonProperty("clubRecordPromo")]
                public int clubRecordPromo { get; set; }

                [JsonProperty("commissionMin")]
                public int commissionMin { get; set; }

                [JsonProperty("commissionMax")]
                public int commissionMax { get; set; }

                [JsonProperty("preAuth&Excess")]
                public object preAuthExcess { get; set; }

                [JsonProperty("complementBaseUpgrade")]
                public object complementBaseUpgrade { get; set; }

                [JsonProperty("amountAdditionalKm")]
                public int amountAdditionalKm { get; set; }

                [JsonProperty("package")]
                public object package { get; set; }
            }

            public class ProductAutomaticComplement
            {
                [JsonProperty("complementId")]
                public int complementId { get; set; }

                [JsonProperty("complementVer")]
                public int complementVer { get; set; }

                [JsonProperty("complementName")]
                public string complementName { get; set; }

                [JsonProperty("complementDescription")]
                public string complementDescription { get; set; }

                [JsonProperty("complementSubtitle")]
                public string complementSubtitle { get; set; }

                [JsonProperty("complementCategory")]
                public string complementCategory { get; set; }

                [JsonProperty("complementGroup")]
                public string complementGroup { get; set; }

                [JsonProperty("complementTTCC")]
                public string complementTTCC { get; set; }

                [JsonProperty("typeComplUnits")]
                public string typeComplUnits { get; set; }

                [JsonProperty("complementUnits")]
                public int complementUnits { get; set; }

                [JsonProperty("maxUnits")]
                public int maxUnits { get; set; }

                [JsonProperty("rateComplVer")]
                public int rateComplVer { get; set; }

                [JsonProperty("taxApplied")]
                public int taxApplied { get; set; }

                [JsonProperty("priceTaxIncDay")]
                public double priceTaxIncDay { get; set; }

                [JsonProperty("priceTaxIncComplement")]
                public double priceTaxIncComplement { get; set; }

                [JsonProperty("valueDto")]
                public int valueDto { get; set; }

                [JsonProperty("applicationDiscount")]
                public int applicationDiscount { get; set; }

                [JsonProperty("priceTaxIncDayDiscount")]
                public int priceTaxIncDayDiscount { get; set; }

                [JsonProperty("priceTaxIncComplementDiscount")]
                public int priceTaxIncComplementDiscount { get; set; }

                [JsonProperty("clubRecordPromo")]
                public int clubRecordPromo { get; set; }

                [JsonProperty("commissionMin")]
                public int commissionMin { get; set; }

                [JsonProperty("commissionMax")]
                public int commissionMax { get; set; }

                [JsonProperty("preAuth&Excess")]
                public object preAuthExcess { get; set; }

                [JsonProperty("complementBaseUpgrade")]
                public object complementBaseUpgrade { get; set; }

                [JsonProperty("amountAdditionalKm")]
                public int amountAdditionalKm { get; set; }

                [JsonProperty("package")]
                public object package { get; set; }
            }

            public class Status
            {
                [JsonProperty("idStatus")]
                public int idStatus { get; set; }

                [JsonProperty("detailedStatus")]
                public string detailedStatus { get; set; }
            }

        }
    }
}