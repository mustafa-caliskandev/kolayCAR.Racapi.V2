using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class AvisResponseBase
    {
        public class AvisAuthResponse
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
        }

        public class Location
        {
            public int OfficeNo { get; set; }
            public string OfficeCode { get; set; }
            public string OfficeName { get; set; }
            public string WebOfficeTr { get; set; }
            public string WebOfficeEng { get; set; }
            public bool IsFranchisee { get; set; }
            public string PhoneNumber { get; set; }
            public string Latitude { get; set; }
            public string Longitude { get; set; }
            public string DeliveryLatitude { get; set; }
            public string DeliveryLongitude { get; set; }
            public string PostAdress1 { get; set; }
            public string PostAdress2 { get; set; }
            public string City { get; set; }
            public int CityId { get; set; }
            public string District { get; set; }
            public int DistrictId { get; set; }
            public bool IsAirport { get; set; }
            public string DeliveryDescription { get; set; }
            public string MondayStart { get; set; }
            public string MondayEnd { get; set; }
            public string TuesdayStart { get; set; }
            public string TuesdayEnd { get; set; }
            public string WednesdayStart { get; set; }
            public string WednesdayEnd { get; set; }
            public string ThursdayStart { get; set; }
            public string ThursdayEnd { get; set; }
            public string FridayStart { get; set; }
            public string FridayEnd { get; set; }
            public string SaturdayStart { get; set; }
            public string SaturdayEnd { get; set; }
            public string SundayStart { get; set; }
            public string SundayEnd { get; set; }
            public bool IsHub { get; set; }
            public bool IsMeet { get; set; }
            public bool DelColAllDistricts { get; set; }
            public string CountryCode { get; set; }
            public string CountryName { get; set; }
            public string CountryNameEng { get; set; }
            public bool IsMotorcycle { get; set; }
            public string OfisMail { get; set; }
            public bool IsShuttleService { get; set; }
            public string Description { get; set; }
            public string DescriptionENG { get; set; }
            public string DescriptionRES { get; set; }
            public string PostAdressENG { get; set; }
        }
        public class Vehicle
        {
            public string GroupName { get; set; }
            public int GroupNo { get; set; }
            public int FuelType { get; set; }
            public string FuelTypeName { get; set; }
            public string FuelTypeNameEng { get; set; }
            public string VehicleName { get; set; }
            public string EquivalentVehicle { get; set; }
            public int ClassNo { get; set; }
            public string ClassName { get; set; }
            public int TransmissionNo { get; set; }
            public string TransmissionName { get; set; }
            public string TransmissionNameEng { get; set; }
            public string UpsellGroupName { get; set; }
            public int MinAge { get; set; }
            public int MinDriverLicense { get; set; }
            public int MinCreditCard { get; set; }
            public int DoorCount { get; set; }
            public int SeatsCount { get; set; }
            public int LuggageCount { get; set; }
            public double Deposit { get; set; }
            public bool Refrigerator { get; set; }
            public bool DoubleCooker { get; set; }
            public bool SolarPanel { get; set; }
            public bool Awning { get; set; }
            public bool AirConditioning { get; set; }
            public bool RearViewCamera { get; set; }
            public bool SolarEnergySystem { get; set; }
            public bool GasCooker { get; set; }
            public bool CassetteWc { get; set; }
            public bool Refrigerator12Volt { get; set; }
            public string MotorCC { get; set; }
            public string LicenseType { get; set; }
            public bool IsMotorcycle { get; set; }
            public int Bed { get; set; }
            public bool ShowerBath { get; set; }
            public bool Heater { get; set; }
            public bool Television { get; set; }
            public bool Cooker { get; set; }
            public bool Swatter { get; set; }
            public bool SwivelFrontSeat { get; set; }
            public bool Mirror { get; set; }
            public bool PetFriendly { get; set; }
            public bool WashingMachine { get; set; }
            public bool Dishwasher { get; set; }
            public bool Freezer { get; set; }
            public bool GasOven { get; set; }
            public bool BikeCarrier { get; set; }
            public bool OutdoorShower { get; set; }
            public bool Stairs { get; set; }
            public bool MotorcycleCarrier { get; set; }
            public bool CleanWaterTank { get; set; }
            public bool WasteWaterTank { get; set; }
            public int Segment { get; set; }
            public string Location { get; set; }
            public string KaravanTip { get; set; }
            public string KaravanTipOzelligi { get; set; }
            public double DailyPrice { get; set; }
        }
        public class AvisLocationResponse : AvisBaseResponse
        {
            public List<Location> Data { get; set; }
        }
        public class AvisVehicleListResponse : AvisBaseResponse
        {
            public List<Vehicle> Data { get; set; }
        }
        public class AvisPostReservationResponse : AvisBaseResponse
        {
            public PostReservationResponse Data { get; set; }
        }
        public class AvisCancelReservationResponse : AvisBaseResponse
        {
            public CancelReservationResponse Data { get; set; }
        }
        public class CancelReservationResponse
        {
            public int ProcessStatus { get; set; }
            public int ReservationId { get; set; }
            public string CCOrderId { get; set; }
            public int SposOrderId { get; set; }
            public string ErrorMessage { get; set; }
            public string ReserVationCnf { get; set; }
        }
        public class PostReservationResponse
        {
            public Transaction transaction { get; set; }
            public object product { get; set; }
            public ReservationResponse reservation { get; set; }
            public Payment payment { get; set; }
        }
        public class ReservationResponse
        {
            public Confirmation confirmation { get; set; }
            public object distance { get; set; }
            public object pickup_location { get; set; }
            public object dropoff_location { get; set; }
            public object rate_totals { get; set; }
            public object vehicle { get; set; }
            public object insurance { get; set; }
            public object extras { get; set; }
            public object terms { get; set; }
            public object disclaimers { get; set; }
        }
        public class Confirmation
        {
            public string number { get; set; }
            public DateTime pickup_date { get; set; }
            public DateTime dropoff_date { get; set; }
        }
        public class Payment
        {
            public int ProcessStatus { get; set; }
            public int ReservationId { get; set; }
            public object CCOrderId { get; set; }
            public int SposOrderId { get; set; }
            public object ErrorMessage { get; set; }
            public string ReserVationCnf { get; set; }
        }
        public class FindeksPhoneIdListResponse : AvisBaseResponse
        {
            public PhoneData Data { get; set; }
        }
        public class FindeksPinConfirmResponse : AvisBaseResponse
        {
            public object Data { get; set; }
        }

        public class FindeksPinRenewResponse : AvisBaseResponse
        {
            public object Data { get; set; }
        }
        public class FindeksReportResponse : AvisBaseResponse
        {
            public int Data { get; set; }
        }
        public class IsActiveFindeksReportExistsResponse : AvisBaseResponse
        {
            public FindeksReportExist Data { get; set; }
        }
        public class IsVehicleSuitableForCustomerResponse
        {
            public bool IsSuitable { get; set; }
            public bool IsRequiredYoungDriverPacked { get; set; }
        }
        public class FindeksReportExist
        {
            public bool CompanyDecision { get; set; }
            public DateTime ReportDate { get; set; }
            public int CompanyScore { get; set; }
            public string CompanyNote { get; set; }
            public int CustomerAge { get; set; }
            public int CustomerLicenseAge { get; set; }
            public string CompanyDecisionDesc { get; set; }
            public List<CompanySegmentList> CompanySegmentList { get; set; }
            public List<CarGroupList> CarGroupList { get; set; }
        }
        public class CarGroupList
        {
            public string CarGroupCode { get; set; }
            public bool YoungDriverPacked { get; set; }
        }

        public class CompanySegmentList
        {
            public int CompanySegmentId { get; set; }
            public string CompanySegment { get; set; }
        }
        public class PhoneData
        {
            public List<Phone> PhoneList { get; set; }
        }
        public class Phone
        {
            public long PhoneId { get; set; }
            public string PhoneNo { get; set; }
        }
        public class AvisBaseResponse
        {
            public bool Result { get; set; }
            public string MessageTR { get; set; }
            public string MessageEN { get; set; }

        }
        public class AvisAvailableVehicleResponse : AvisBaseResponse
        {
            public Data Data { get; set; }
        }

        public class Address
        {
            public string address_line_1 { get; set; }
            public string address_line_2 { get; set; }
            public object address_line_3 { get; set; }
            public string city { get; set; }
            public string state_name { get; set; }
            public string postal_code { get; set; }
            public string country_code { get; set; }
            public string lat { get; set; }
            public string _long { get; set; }
        }

        public class Capacity
        {
            public string doors { get; set; }
            public string seats { get; set; }
            public LuggageCapacity luggage_capacity { get; set; }
        }

        public class Category
        {
            public string name { get; set; }
            public string make { get; set; }
            public string model { get; set; }
            public string vehicle_class_code { get; set; }
            public string vehicle_class_name { get; set; }
            public string vehicle_transmission { get; set; }
            public string mpg { get; set; }
            public string image_url { get; set; }
            public string vehicle_class_size { get; set; }
            public string vehicle_class_category { get; set; }
            public string upsell_group { get; set; }
            public string kmLimit { get; set; }
            public string RateCodeId { get; set; }
        }

        public class Coupon
        {
            public object code { get; set; }
            public object type { get; set; }
            public object description { get; set; }
            public int quantity { get; set; }
            public object coupon_extras { get; set; }
        }

        public class Data
        {
            public Transaction transaction { get; set; }
            public Product product { get; set; }
            public List<AvailableVehicle> vehicles { get; set; }
            public Discount discount { get; set; }
            public Coupon coupon { get; set; }
            public Reservation reservation { get; set; }
        }

        public class Discount
        {
            public string code { get; set; }
        }

        public class DropoffLocation
        {
            public AvailableLocation location { get; set; }
            public Address address { get; set; }
        }

        public class Extras
        {
            public double pai { get; set; }
            public double li { get; set; }
            public double sli { get; set; }
            public double mi { get; set; }
            public double smi { get; set; }
            public double scdw { get; set; }
            public double scdwplus { get; set; }
            public double tandg { get; set; }
            public double bordercrossingfee { get; set; }
            public double pai_discounted { get; set; }
            public double li_discounted { get; set; }
            public double sli_discounted { get; set; }
            public double mi_discounted { get; set; }
            public double smi_discounted { get; set; }
            public double additinoal_driver { get; set; }
            public double navigation { get; set; }
            public double baby_seat { get; set; }
            public double young_driver { get; set; }
            public bool is_delivery { get; set; }
            public bool is_collection { get; set; }
            public double Bicycle { get; set; }
            public double DubleConfortSet { get; set; }
            public double BasicConfortSet { get; set; }
            public double CampTableAndChair { get; set; }
            public double TravelRoutePlan { get; set; }
            public double SafetyBox { get; set; }
            public double Blanket { get; set; }
            public double Chair { get; set; }
            public double CancelFee { get; set; }
            public double FullDayService { get; set; }
            public double lcfa { get; set; }
            public double lcfa_discounted { get; set; }
        }

        public class Features
        {
            public bool bluetooth_equipped { get; set; }
            public bool smoke_free { get; set; }
            public bool air_conditioned { get; set; }
            public bool connected_car { get; set; }
        }

        public class AvailableLocation
        {
            public string code { get; set; }
            public string name { get; set; }
            public string telephone { get; set; }
            public string hours { get; set; }
            public bool airport_location { get; set; }
            public int additional_product_is_franchise { get; set; }
        }

        public class LuggageCapacity
        {
            public string large_suitcase { get; set; }
        }

        public class PayLater
        {
            public double vehicle_total { get; set; }
            public double reservation_total { get; set; }
            public double original_vehicle_total { get; set; }
            public double original_reservation_total { get; set; }
            public double one_way_fee { get; set; }
        }

        public class PayNow
        {
            public float vehicle_total { get; set; }
            public float reservation_total { get; set; }
            public float original_vehicle_total { get; set; }
            public float original_reservation_total { get; set; }
            public float one_way_fee { get; set; }
        }

        public class PickupLocation
        {
            public Location location { get; set; }
            public Address address { get; set; }
        }

        public class Product
        {
            public string brand { get; set; }
        }

        public class Rate
        {
            public int rentdaycount { get; set; }
            public string currency { get; set; }
            public string rate_code { get; set; }
            public bool coupon_applied { get; set; }
            public double coupon_discount_amount { get; set; }
            public string discount_code { get; set; }
            public string prepay_type { get; set; }
            public double eur { get; set; }
            public double usd { get; set; }
        }

        public class RateTotals
        {
            public Rate rate { get; set; }
            public PayLater pay_later { get; set; }
            public PayNow pay_now { get; set; }
            public Extras extras { get; set; }
        }

        public class Reservation
        {
            public PickupLocation pickup_location { get; set; }
            public DropoffLocation dropoff_location { get; set; }
            public object terms { get; set; }
        }



        public class Transaction
        {
            public string transaction_id { get; set; }
        }

        public class AvailableVehicle
        {
            public Category category { get; set; }
            public Features features { get; set; }
            public Capacity capacity { get; set; }
            public RateTotals rate_totals { get; set; }
        }



    }
}
