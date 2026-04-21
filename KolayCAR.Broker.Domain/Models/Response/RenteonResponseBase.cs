using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class RenteonResponseBase
    {
        public class RenteonLocationResponseBase
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public List<string> Countries { get; set; }
            public object LogoUrl { get; set; }
            public List<Service> Services { get; set; }
            public List<Location> Locations { get; set; }
            public List<Office> Offices { get; set; }
            public List<CarCategory> CarCategories { get; set; }
            public List<Pricelist> Pricelists { get; set; }
            public List<Connector> Connectors { get; set; }
            public object TermsAndConditions { get; set; }
        }

        public class CarCategory
        {
            public string Code { get; set; }
            public double? SortOrder { get; set; }
            public string SIPP { get; set; }
            public string Title { get; set; }
            public string CarModel { get; set; }
            public bool IsModelGuaranteed { get; set; }
            public string CarModelImageURL { get; set; }
            public int? BigBagsCapacity { get; set; }
            public int? SmallBagsCapacity { get; set; }
            public int? PassengerCapacity { get; set; }
            public bool AirConditioning { get; set; }
            public int? NumberOfDoors { get; set; }
            public bool ShowInList { get; set; }
            public decimal? FranchiseAmount { get; set; }
            public string FranchiseCurrency { get; set; }
            public decimal? DepositAmount { get; set; }
            public string DepositCurrency { get; set; }
        }

        public class Connector
        {
            public string Name { get; set; }
            public int? Id { get; set; }
            public string Url { get; set; }
            public List<CarCategory> CarCategories { get; set; }
            public List<string> CountryCodes { get; set; }
        }

        public class HolidayWorkingTime
        {
            public DateTime Date { get; set; }
            public DateTime DateLocal { get; set; }
            public string Name { get; set; }
            public int? HourFrom { get; set; }
            public int? HourTo { get; set; }
            public int? MinuteFrom { get; set; }
            public int? MinuteTo { get; set; }
            public int? HourFrom2 { get; set; }
            public int? HourTo2 { get; set; }
            public int? MinuteFrom2 { get; set; }
            public int? MinuteTo2 { get; set; }
            public bool IsWorking { get; set; }
            public bool IsWorkingNonStop { get; set; }
        }

        public class Location
        {
            public string Code { get; set; }
            public string CountryCode { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public object Category { get; set; }
            public string Path { get; set; }
        }

        public class Office
        {
            public List<RegularWorkingTime> RegularWorkingTimes { get; set; }
            public List<HolidayWorkingTime> HolidayWorkingTimes { get; set; }
            public List<object> SpecialWorkingTimes { get; set; }
            public string OfficeCode { get; set; }
            public int? OfficeId { get; set; }
            public string LocationCode { get; set; }
            public string Address { get; set; }
            public string Town { get; set; }
            public object PostalCode { get; set; }
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
            public int? ConnectorId { get; set; }
            public bool IsPickupOffice { get; set; }
            public bool IsDropOffOffice { get; set; }
            public bool IsMeetAndGreetLocation { get; set; }
            public bool IsDropOffLocation { get; set; }
            public object OfficeDropOffInstructions { get; set; }
            public object OfficePickupInstructions { get; set; }
            public int? ConnectorOfficeId { get; set; }
            public object ConnectorMeetAndGreetLocationId { get; set; }
            public int? ConnectorDropOffLocationId { get; set; }
            public object IsShuttle { get; set; }
            public string LocationType { get; set; }
        }

        public class Pricelist
        {
            public string Name { get; set; }
            public string Code { get; set; }
            public bool IsPrepaid { get; set; }
        }

        public class RegularWorkingTime
        {
            public int? DayOfWeekIndex { get; set; }
            public int? HourFrom { get; set; }
            public int? HourTo { get; set; }
            public int? MinuteFrom { get; set; }
            public int? MinuteTo { get; set; }
            public int? HourFrom2 { get; set; }
            public int? HourTo2 { get; set; }
            public int? MinuteFrom2 { get; set; }
            public int? MinuteTo2 { get; set; }
            public bool IsWorking { get; set; }
            public bool? IsWorkingNonStop { get; set; }
            public string DayOfWeekName { get; set; }
        }

        public class Service
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
        }


        // Vehicle

        public class RenteonVehicleResponseBase
        {
            public string Provider { get; set; }
            public int? ConnectorId { get; set; }
            public string CarCategory { get; set; }
            public bool Prepaid { get; set; }
            public string ModelName { get; set; }
            public decimal Amount { get; set; }
            public decimal? OriginalAmount { get; set; }
            public decimal? OriginalNetAmount { get; set; }
            public int? DaysForPayment { get; set; }
            public int? HoursForPayment { get; set; }
            public decimal? DiscountPercentage { get; set; }
            public decimal? VatAmount { get; set; }
            public decimal? NetAmount { get; set; }
            public string Currency { get; set; }
            public int? PricelistId { get; set; }
            public string PricelistCode { get; set; }
            public DateTime PriceDate { get; set; }
            public List<object> IncludedServices { get; set; }
            public List<AvailableService> AvailableServices { get; set; }
            public bool IsOnRequest { get; set; }
            public int? PickupOfficeId { get; set; }
            public PickupOffice PickupOffice { get; set; }
            public int? DropOffOfficeId { get; set; }
            public DropOffOffice DropOffOffice { get; set; }
            public DateTime PickupDate { get; set; }
            public DateTime DropOffDate { get; set; }
            public string CarModelImageURL { get; set; }
            public int? BigBagsCapacity { get; set; }
            public int? SmallBagsCapacity { get; set; }
            public int? PassengerCapacity { get; set; }
            public int? NumberOfDoors { get; set; }
            public int? YoungDriverAgeFrom { get; set; }
            public int? YoungDriverAgeTo { get; set; }
            public int? SeniorDriverAgeFrom { get; set; }
            public int? SeniorDriverAgeTo { get; set; }
            public int? ConnectorCarCategoryId { get; set; }
            public int? MinimumDriverAge { get; set; }
            public int? MaximumDriverAge { get; set; }
            public string PromoCode { get; set; }
        }
        public class AvailableService
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
            public int? MaximumQuantity { get; set; }
            public bool Prepaid { get; set; }
            public bool FreeOfCharge { get; set; }
            public bool IncludedInPriceUnlimited { get; set; }
            public bool IncludedInPriceLimited { get; set; }
            public int? QuantityIncluded { get; set; }
            public bool IncludedPerDurationMeasuringUnit { get; set; }
            public bool IncludedPerContract { get; set; }
            public bool IsOneTimePayment { get; set; }
            public object MaxDurationMeasuringUnitForPayment { get; set; }
            public int? DurationMeasuringUnitId { get; set; }
            public decimal? Amount { get; set; }
            public decimal? VatAmount { get; set; }
        }

        public class DropOffOffice
        {
            public string OfficeCode { get; set; }
            public int? OfficeId { get; set; }
            public string LocationCode { get; set; }
            public string Address { get; set; }
            public string Town { get; set; }
            public object PostalCode { get; set; }
            public object Latitude { get; set; }
            public object Longitude { get; set; }
            public int? ConnectorId { get; set; }
            public bool IsPickupOffice { get; set; }
            public bool IsDropOffOffice { get; set; }
            public bool IsMeetAndGreetLocation { get; set; }
            public bool IsDropOffLocation { get; set; }
            public object OfficeDropOffInstructions { get; set; }
            public object OfficePickupInstructions { get; set; }
            public int? ConnectorOfficeId { get; set; }
            public object ConnectorMeetAndGreetLocationId { get; set; }
            public object ConnectorDropOffLocationId { get; set; }
            public object IsShuttle { get; set; }
            public string LocationType { get; set; }
        }

        public class PickupOffice
        {
            public string OfficeCode { get; set; }
            public int? OfficeId { get; set; }
            public string LocationCode { get; set; }
            public string Address { get; set; }
            public string Town { get; set; }
            public object PostalCode { get; set; }
            public object Latitude { get; set; }
            public object Longitude { get; set; }
            public int? ConnectorId { get; set; }
            public bool IsPickupOffice { get; set; }
            public bool IsDropOffOffice { get; set; }
            public bool IsMeetAndGreetLocation { get; set; }
            public bool IsDropOffLocation { get; set; }
            public object OfficeDropOffInstructions { get; set; }
            public object OfficePickupInstructions { get; set; }
            public int? ConnectorOfficeId { get; set; }
            public object ConnectorMeetAndGreetLocationId { get; set; }
            public object ConnectorDropOffLocationId { get; set; }
            public object IsShuttle { get; set; }
            public string LocationType { get; set; }
        }

    }
}
