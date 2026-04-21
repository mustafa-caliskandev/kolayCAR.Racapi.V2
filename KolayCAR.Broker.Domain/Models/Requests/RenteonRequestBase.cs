using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class RenteonRequestBase
    {
        public class Provider
        {
            public string Code { get; set; }
            public List<string> PricelistCodes { get; set; }
            public List<int> PickupOfficeIds { get; set; }
            public List<int> DropOffOfficeIds { get; set; }
        }

        public class RenteonVehicleRequestBase
        {
            public bool Prepaid { get; set; }
            public bool IncludeOnRequest { get; set; }
            public List<Provider> Providers { get; set; }
            public List<string> CarCategories { get; set; }
            public string PickupLocation { get; set; }
            public string DropOffLocation { get; set; }
            public string PickupDate { get; set; }
            public string DropOffDate { get; set; }
            public string Currency { get; set; }
            public bool HasDelivery { get; set; }
            public bool HasCollection { get; set; }
        }

        public class RenteonReservationCancelRequestBase
        {
            public int ConnectorId { get; set; }
            public string Number { get; set; }
        }

        public class RenteonReservationPostRequestBase
        {
            public DateTime? ExpirationDate { get; set; }
            public string TotalRent { get; set; }
            public string TotalDiscount { get; set; }
            public string TotalAddition { get; set; }
            public string TotalInsurance { get; set; }
            public string TotalVat { get; set; }
            public List<Fees> Totals { get; set; }
            public float? Excess { get; set; }
            public float? Deposit { get; set; }
            public string DepositCurrency { get; set; }
            public PickupOffice PickupOffice { get; set; }
            public DropOffOffice DropOffOffice { get; set; }
            public List<object> Drivers { get; set; }
            public Client Client { get; set; }
            public List<Service> Services { get; set; }
            public string Remark { get; set; }
            public string ExternalUserId { get; set; }
            public int? DaysForPayment { get; set; }
            public int? HoursForPayment { get; set; }
            public string PartnerWebCode { get; set; }
            public int? YoungDriverAgeFrom { get; set; }
            public int? YoungDriverAgeTo { get; set; }
            public int? SeniorDriverAgeFrom { get; set; }
            public int? SeniorDriverAgeTo { get; set; }
            public int? MinimumDriverAge { get; set; }
            public int? MaximumDriverAge { get; set; }
            public string Id { get; set; }
            public string RowVersion { get; set; }
            public string ConnectorId { get; set; }
            public string Provider { get; set; }
            public string Number { get; set; }
            public bool IsCancelled { get; set; }
            public bool IsProcessed { get; set; }
            public bool IsOnRequest { get; set; }
            public DateTime? CancellationDate { get; set; }
            public bool Prepaid { get; set; }
            public string Currency { get; set; }
            public string CarCategory { get; set; }
            public string ClientName { get; set; }
            public string ClientEmail { get; set; }
            public string ClientPhone { get; set; }
            public DateTime? PickupDate { get; set; }
            public DateTime? DropOffDate { get; set; }
            public string Total { get; set; }
            public string VoucherNumber { get; set; }
            public string FlightNumber { get; set; }
            public string PickupOfficeId { get; set; }
            public string DropOffOfficeId { get; set; }
            public DateTime? InitialBookingDate { get; set; }
            public string UserOpenName { get; set; }
            public DateTime? ModificationTime { get; set; }
            public string UserModifiedName { get; set; }
            public int? PricelistId { get; set; }
            public string PricelistCode { get; set; }
            public string PricelistName { get; set; }
            public bool HasDelivery { get; set; }
            public string DeliveryLocation { get; set; }
            public bool HasCollection { get; set; }
            public string CollectionLocation { get; set; }
            public DateTime? PriceDate { get; set; }
            public string ConnectorCarCategoryId { get; set; }
            public string PromoCode { get; set; }
        }

        public class Client
        {
            public string Id { get; set; }
            public string RowVersion { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public string VatId { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public string Gender { get; set; }
            public string Street { get; set; }
            public string PostalCode { get; set; }
            public string TownName { get; set; }
            public string CountryCode { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string DriversLicenseNumber { get; set; }
            public string DriversLicenseIssuedIn { get; set; }
            public string DriversLicenseIssueDate { get; set; }
            public string DriversLicenseValidUntil { get; set; }
            public string PassportNumber { get; set; }
            public string PassportIssuedIn { get; set; }
            public string PassportIssueDate { get; set; }
            public string PassportValidUntil { get; set; }
            public string IdentityCardNumber { get; set; }
            public string IdentityCardIssuedIn { get; set; }
            public string IdentityCardIssueDate { get; set; }
            public string IdentityCardValidUntil { get; set; }
        }

        public class DropOffOffice
        {
            public string OfficeCode { get; set; }
            public string OfficeId { get; set; }
            public string LocationCode { get; set; }
            public string Address { get; set; }
            public string Town { get; set; }
            public string PostalCode { get; set; }
            public string Latitude { get; set; }
            public string Longitude { get; set; }
            public string ConnectorId { get; set; }
            public string IsPickupOffice { get; set; }
            public string IsDropOffOffice { get; set; }
            public string IsMeetAndGreetLocation { get; set; }
            public string IsDropOffLocation { get; set; }
            public string OfficeDropOffInstructions { get; set; }
            public string OfficePickupInstructions { get; set; }
            public string ConnectorOfficeId { get; set; }
            public string ConnectorMeetAndGreetLocationId { get; set; }
            public string ConnectorDropOffLocationId { get; set; }
            public string IsShuttle { get; set; }
            public string LocationType { get; set; }
        }

        public class PickupOffice
        {
            public string OfficeCode { get; set; }
            public string OfficeId { get; set; }
            public string LocationCode { get; set; }
            public string Address { get; set; }
            public string Town { get; set; }
            public string PostalCode { get; set; }
            public string Latitude { get; set; }
            public string Longitude { get; set; }
            public string ConnectorId { get; set; }
            public string IsPickupOffice { get; set; }
            public string IsDropOffOffice { get; set; }
            public string IsMeetAndGreetLocation { get; set; }
            public string IsDropOffLocation { get; set; }
            public string OfficeDropOffInstructions { get; set; }
            public string OfficePickupInstructions { get; set; }
            public string ConnectorOfficeId { get; set; }
            public string ConnectorMeetAndGreetLocationId { get; set; }
            public string ConnectorDropOffLocationId { get; set; }
            public string IsShuttle { get; set; }
            public string LocationType { get; set; }
        }

        public class Service
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public int? ServiceId { get; set; }
            public string AdditionalName { get; set; }
            public string AdditionalCode { get; set; }
            public int? ServiceGroupId { get; set; }
            public string ServiceGroupName { get; set; }
            public int? ServiceTypeId { get; set; }
            public string ServiceTypeName { get; set; }
            public string Description { get; set; }
            public bool IsRent { get; set; }
            public bool IsSelected { get; set; }
            public string Quantity { get; set; }
            public bool IsMandatory { get; set; }
            public bool IsQuantityEditable { get; set; }
            public string MaximumQuantity { get; set; }
            public bool Prepaid { get; set; }
            public float? InsuranceFranchiseAmount { get; set; }
            public float? InsuranceFranchiseAmountNet { get; set; }
            public float? InsuranceDepositAmount { get; set; }
            public float? InsuranceDepositAmountNet { get; set; }
            public float? FranchiseTheftAmount { get; set; }
            public float? FranchiseTheftAmountNet { get; set; }
            public string DiscountPercentage { get; set; }
            public string DiscountAmount { get; set; }
            public bool IsDiscountPercentage { get; set; }
            public ServicePrice ServicePrice { get; set; }
        }

        public class ServicePrice
        {
            public bool FreeOfCharge { get; set; }
            public bool IncludedInPriceUnlimited { get; set; }
            public bool IncludedInPriceLimited { get; set; }
            public string QuantityIncluded { get; set; }
            public bool IncludedPerDurationMeasuringUnit { get; set; }
            public bool IncludedPerContract { get; set; }
            public bool IsOneTimePayment { get; set; }
            public bool IsMainRent { get; set; }
            public string Currency { get; set; }
            public int? CurrencyId { get; set; }
            public int? MaxDurationMeasuringUnitForPayment { get; set; }
            public int? DurationMeasuringUnitId { get; set; }
            public string Amount { get; set; }
            public string AmountTotal { get; set; }
            public string VatTotal { get; set; }
        }

        public class Fees
        {
            public bool Prepaid { get; set; }
            public string TotalRent { get; set; }
            public string TotalDiscount { get; set; }
            public string TotalAddition { get; set; }
            public string TotalInsurance { get; set; }
            public string TotalVat { get; set; }
            public string Total { get; set; }
        }
    }
}