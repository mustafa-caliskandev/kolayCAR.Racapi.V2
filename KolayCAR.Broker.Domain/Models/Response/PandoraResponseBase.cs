using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class PandoraResponseBase
    {
        public class Auth
        {
            public string grant_type { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public string client_id { get; set; }
            public string signature { get; set; }
            public string salt { get; set; }
        }

        public class Token
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
            public string client_id { get; set; }
            public string host { get; set; }
            [JsonProperty(".issued")]
            public string issued { get; set; }
            [JsonProperty(".expires")]
            public string expires { get; set; }
        }

        public class Office
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
            public string CodeNumber { get; set; }
            public string CountryCode { get; set; }
            public string PostalCode { get; set; }
            public string Town { get; set; }
            public string Address { get; set; }
            public string AddressHouseNumber { get; set; }
            public string Tel { get; set; }
            public string Fax { get; set; }
            public string Email { get; set; }
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
            public List<object> MeetAndGreetLocations { get; set; }
        }

        public class PandoraAddition
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
            public string Group { get; set; }
            public string Type { get; set; }
            public int TypeId { get; set; }
            public object InsuranceFranchise { get; set; }
        }

        public class CarCategories
        {
            public CarTransmissionType CarTransmissionType { get; set; }
            public int Id { get; set; }
            public string SIPP { get; set; }
            public object Title { get; set; }
            public string CarModel { get; set; }
            public bool IsModelGuaranteed { get; set; }
            public string CarModelImageURL { get; set; }
            public int BigBagsCapacity { get; set; }
            public int SmallBagsCapacity { get; set; }
            public int PassengerCapacity { get; set; }
            public bool AirConditioning { get; set; }
            public int NumberOfDoors { get; set; }
            public bool ShowInList { get; set; }
            public double SortOrder { get; set; }
            public List<Group> Groups { get; set; }
            public List<FuelType> FuelTypes { get; set; }
            public float? DepositAmount { get; set; }
        }

        public class CarTransmissionType
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Group
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string TypeId { get; set; }
            public string TypeName { get; set; }
        }

        public class FuelType
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class AvailableVehicle
        {
            public int CarCategoryId { get; set; }
            public string CarCategoryGroup { get; set; }
            public string SIPP { get; set; }
            public string ModelName { get; set; }
            public int ServiceId { get; set; }
            public double Amount { get; set; }
            public double OriginalAmount { get; set; }
            public double OriginalNetAmount { get; set; }
            public int DaysForPayment { get; set; }
            public double DiscountPercentage { get; set; }
            public double VatAmount { get; set; }
            public double NetAmount { get; set; }
            public string Currency { get; set; }
            public int CurrencyId { get; set; }
            public int PricelistId { get; set; }
            public string PricelistCode { get; set; }
            public string PricelistName { get; set; }
            public DateTime PriceDate { get; set; }
            public List<IncludedService> IncludedServices { get; set; }
            public List<AvailableService> AvailableServices { get; set; }
        }

        public class AvailableService
        {
            public int ServiceId { get; set; }
            public int ServiceGroupId { get; set; }
            public string ServiceGroupName { get; set; }
            public int ServiceTypeId { get; set; }
            public string ServiceTypeName { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int? MaximumQuantity { get; set; }
            public bool PayOnArrival { get; set; }
            public bool FreeOfCharge { get; set; }
            public bool IncludedInPriceUnlimited { get; set; }
            public bool IncludedInPriceLimited { get; set; }
            public int QuantityIncluded { get; set; }
            public bool IncludedPerDurationMeasuringUnit { get; set; }
            public bool IncludedPerContract { get; set; }
            public bool IsOneTimePayment { get; set; }
            public int CurrencyId { get; set; }
            public int? MaxDurationMeasuringUnitForPayment { get; set; }
            public int DurationMeasuringUnitId { get; set; }
            public double Amount { get; set; }
            public double VatAmount { get; set; }
        }

        public class IncludedService
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int ServiceTypeId { get; set; }
            public string ServiceTypeName { get; set; }
            public int Quantity { get; set; }
            public double AmountTotal { get; set; }
            public double VatTotal { get; set; }
            public bool PayOnArrival { get; set; }
        }

        public class Total
        {
            public int BookingItemPriceTypeId { get; set; }
            public double TotalRent { get; set; }
            public double TotalDiscount { get; set; }
            public double TotalAddition { get; set; }
            public double TotalInsurance { get; set; }
            public double TotalVat { get; set; }
            [JsonProperty("Total")]
            public double TotalAmount { get; set; }
        }

        public class OfficeOut
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
            public string CodeNumber { get; set; }
            public string CountryCode { get; set; }
            public string PostalCode { get; set; }
            public string Town { get; set; }
            public string Address { get; set; }
            public string AddressHouseNumber { get; set; }
            public string Tel { get; set; }
            public object Fax { get; set; }
            public string Email { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public List<object> MeetAndGreetLocations { get; set; }
        }

        public class OfficeIn
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
            public string CodeNumber { get; set; }
            public string CountryCode { get; set; }
            public string PostalCode { get; set; }
            public string Town { get; set; }
            public string Address { get; set; }
            public string AddressHouseNumber { get; set; }
            public string Tel { get; set; }
            public object Fax { get; set; }
            public string Email { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public List<object> MeetAndGreetLocations { get; set; }
        }

        public class Pricelist
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
            public string Currency { get; set; }
            public bool IsNett { get; set; }
            public DateTime ActiveFrom { get; set; }
            public DateTime ActiveTo { get; set; }
            public DateTime BookingFrom { get; set; }
            public DateTime BookingTo { get; set; }
        }

        public class ServicePrice
        {
            public bool FreeOfCharge { get; set; }
            public bool IncludedInPriceUnlimited { get; set; }
            public bool IncludedInPriceLimited { get; set; }
            public int QuantityIncluded { get; set; }
            public bool IncludedPerDurationMeasuringUnit { get; set; }
            public bool IncludedPerContract { get; set; }
            public bool IsOneTimePayment { get; set; }
            public bool IsMainRent { get; set; }
            public string Currency { get; set; }
            public int CurrencyId { get; set; }
            public int? MaxDurationMeasuringUnitForPayment { get; set; }
            public int DurationMeasuringUnitId { get; set; }
            public double Amount { get; set; }
            public double AmountTotal { get; set; }
            public double VatTotal { get; set; }
        }

        public class Service
        {
            public int ServiceId { get; set; }
            public int ServiceGroupId { get; set; }
            public string ServiceGroupName { get; set; }
            public int ServiceTypeId { get; set; }
            public string ServiceTypeName { get; set; }
            public string Name { get; set; }
            public object Description { get; set; }
            public bool IsRent { get; set; }
            public bool IsSelected { get; set; }
            public int Quantity { get; set; }
            public bool IsMandatory { get; set; }
            public bool IsQuantityEditable { get; set; }
            public int? MaximumQuantity { get; set; }
            public bool PayOnArrival { get; set; }
            public double? InsuranceFranchiseAmount { get; set; }
            public double? InsuranceFranchiseAmountNet { get; set; }
            public double DiscountPercentage { get; set; }
            public double DiscountAmount { get; set; }
            public bool IsDiscountPercentage { get; set; }
            public ServicePrice ServicePrice { get; set; }
        }

        public class BookingsCreate
        {
            public DateTime ExpirationDate { get; set; }
            public double TotalRent { get; set; }
            public double TotalDiscount { get; set; }
            public double TotalAddition { get; set; }
            public double TotalInsurance { get; set; }
            public double TotalVat { get; set; }
            public List<Total> Totals { get; set; }
            public OfficeOut OfficeOut { get; set; }
            public OfficeIn OfficeIn { get; set; }
            public Pricelist Pricelist { get; set; }
            public List<object> Booking_Drivers { get; set; }
            public object Client { get; set; }
            public List<Service> Services { get; set; }
            public object DropOffLocation { get; set; }
            public object Remark { get; set; }
            public object ExternalUserId { get; set; }
            public int DaysForPayment { get; set; }
            public int HoursForPayment { get; set; }
            public object PartnerWebCode { get; set; }
            public int Id { get; set; }
            public object RowVersion { get; set; }
            public object Number { get; set; }
            public int BookingTypeId { get; set; }
            public bool IsCancelled { get; set; }
            public bool IsProcessed { get; set; }
            public bool IsOnRequest { get; set; }
            public object CancellationDate { get; set; }
            public bool BookAsCommissioner { get; set; }
            public string Currency { get; set; }
            public int CarCategoryId { get; set; }
            public object ClientName { get; set; }
            public object ClientEmail { get; set; }
            public object ClientPhone { get; set; }
            public DateTime DateOut { get; set; }
            public DateTime DateIn { get; set; }
            public double Total { get; set; }
            public string VoucherNumber { get; set; }
            public object FlightNumber { get; set; }
            public int OfficeOutId { get; set; }
            public int OfficeInId { get; set; }
            public string CarCategorySIPP { get; set; }
            public string OfficeOutCode { get; set; }
            public string OfficeOutName { get; set; }
            public string OfficeInCode { get; set; }
            public string OfficeInName { get; set; }
            public DateTime InitialBookingDate { get; set; }
            public object UserOpenName { get; set; }
            public object ModificationTime { get; set; }
            public object UserModifiedName { get; set; }
            public int PricelistId { get; set; }
            public string PricelistCode { get; set; }
            public bool HasDelivery { get; set; }
            public string DeliveryLocation { get; set; }
            public bool HasCollection { get; set; }
            public string CollectionLocation { get; set; }
            public object MeetAndGreetLocationOutId { get; set; }
            public object MeetAndGreetLocationInId { get; set; }
            public object MeetAndGreetLocationOutName { get; set; }
            public object MeetAndGreetLocationInName { get; set; }
            public object DropOffLocationId { get; set; }
            public DateTime PriceDate { get; set; }
        }

        public class BookingDriver
        {
            public int Id { get; set; }
            public string RowVersion { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Phone { get; set; }
            public int DriverAge { get; set; }
            public string Email { get; set; }
        }

        public class Client
        {
            public int Id { get; set; }
            public string RowVersion { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public object VatId { get; set; }
            public object DateOfBirth { get; set; }
            public string Gender { get; set; }
            public object Street { get; set; }
            public object PostalCode { get; set; }
            public object TownName { get; set; }
            public string CountryCode { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public object DriversLicenseNumber { get; set; }
            public object DriversLicenseIssuedIn { get; set; }
            public object DriversLicenseIssueDate { get; set; }
            public object DriversLicenseValidUntil { get; set; }
            public object PassportNumber { get; set; }
            public object PassportIssuedIn { get; set; }
            public object PassportIssueDate { get; set; }
            public object PassportValidUntil { get; set; }
            public object IdentityCardNumber { get; set; }
            public object IdentityCardIssuedIn { get; set; }
            public object IdentityCardIssueDate { get; set; }
            public object IdentityCardValidUntil { get; set; }
        }

        public class PandoraPostBookingSaveResponse
        {
            public DateTime ExpirationDate { get; set; }
            public double TotalRent { get; set; }
            public double TotalDiscount { get; set; }
            public double TotalAddition { get; set; }
            public double TotalInsurance { get; set; }
            public double TotalVat { get; set; }
            public List<Total> Totals { get; set; }
            public OfficeOut OfficeOut { get; set; }
            public OfficeIn OfficeIn { get; set; }
            public Pricelist Pricelist { get; set; }
            public List<BookingDriver> Booking_Drivers { get; set; }
            public Client Client { get; set; }
            public List<Service> Services { get; set; }
            public object DropOffLocation { get; set; }
            public object Remark { get; set; }
            public object ExternalUserId { get; set; }
            public int DaysForPayment { get; set; }
            public int HoursForPayment { get; set; }
            public string PartnerWebCode { get; set; }
            public int Id { get; set; }
            public string RowVersion { get; set; }
            public string Number { get; set; }
            public int BookingTypeId { get; set; }
            public bool IsCancelled { get; set; }
            public bool IsProcessed { get; set; }
            public bool IsOnRequest { get; set; }
            public object CancellationDate { get; set; }
            public bool BookAsCommissioner { get; set; }
            public string Currency { get; set; }
            public int CarCategoryId { get; set; }
            public string ClientName { get; set; }
            public string ClientEmail { get; set; }
            public string ClientPhone { get; set; }
            public DateTime DateOut { get; set; }
            public DateTime DateIn { get; set; }
            public double Total { get; set; }
            public string VoucherNumber { get; set; }
            public object FlightNumber { get; set; }
            public int OfficeOutId { get; set; }
            public int OfficeInId { get; set; }
            public string CarCategorySIPP { get; set; }
            public string OfficeOutCode { get; set; }
            public string OfficeOutName { get; set; }
            public string OfficeInCode { get; set; }
            public string OfficeInName { get; set; }
            public DateTime InitialBookingDate { get; set; }
            public object UserOpenName { get; set; }
            public DateTime ModificationTime { get; set; }
            public object UserModifiedName { get; set; }
            public int PricelistId { get; set; }
            public string PricelistCode { get; set; }
            public bool HasDelivery { get; set; }
            public string DeliveryLocation { get; set; }
            public bool HasCollection { get; set; }
            public string CollectionLocation { get; set; }
            public object MeetAndGreetLocationOutId { get; set; }
            public object MeetAndGreetLocationInId { get; set; }
            public object MeetAndGreetLocationOutName { get; set; }
            public object MeetAndGreetLocationInName { get; set; }
            public object DropOffLocationId { get; set; }
            public DateTime PriceDate { get; set; }
        }
    }
}
