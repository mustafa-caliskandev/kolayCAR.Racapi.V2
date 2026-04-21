using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models
{
    public class Vendor
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public string VendorPhone { get; set; }
        public string VendorEmail { get; set; }
        public VendorTypes VendorType { get; set; }
        public string ApiKey { get; set; }
        public string ApiPassword { get; set; }
        public string ApiClientId { get; set; }
        public string SecretKey { get; set; }
        public bool Active { get; set; }
        public bool ReservationListActive { get; set; }
        public float ProfitMarkupDailyPrice { get; set; }
        public float ProfitMarkupAdditionalProducts { get; set; }
        public float ProfitMarkupOneWayFee { get; set; }
        public PriceRoundingTypes PriceRoundingType { get; set; }
        public CurrencyTypes CurrencyType { get; set; }
        public List<CurrencyTypes> AvailableCurrencies { get; set; }
        public string APIBaseUrl { get; set; }
        public float ServiceCharge { get; set; }
        public CurrencyTypes ServiceChargeCurrencyType { get; set; }
        public bool ProfitMarkupDailyPriceActive { get; set; }
        public bool ProfitMarkupAdditionalProductsActive { get; set; }
        public bool ProfitMarkupOneWayFeeActive { get; set; }
        public string Logo { get; set; }
        public bool DepositCreditCardRequired { get; set; }
        public bool VehicleMappingActive { get; set; }
        public bool APIPhoneActive { get; set; }
        public int APITimeout { get; set; }
        public string CompanyTitle { get; set; }
        public bool DisableDeposit { get; set; }
        public bool PersonelNumberRequired { get; set; }
        public bool SendReservationMailToVendor { get; set; }
        public bool UseOnlyDefaultCurrency { get; set; }
        public bool ResAgencyNameSending { get; set; }
        public bool SellingBelowCostForCouponCode { get; set; }
        public bool UseBrokerConfigurations { get; set; }
        public VendorWorkingTypes RentalWorkingType { get; set; }
        public VendorWorkingTypes AdditionalProductWorkingType { get; set; }
        public VendorWorkingTypes OneWayFeeWorkingType { get; set; }
        public bool CouponCodeActive { get; set; }
        public int FreeCancellationHour { get; set; }
        public int? VendorOrder { get; set; }
        public bool ShowCustomerNoteArea { get; set; }
        public bool ShowFlightNumberArea { get; set; }
        public bool PRIORITYFEE { get; set; }
        public CreditType CreditType { get; set; }
        public List<VendorVendor> VendorVendors { get; set; }
        public List<VendorVendor> NewVendors { get; set; } // Yeni kayıt ekleme işlemi için tanımlanmış bir alan. Ek veri tabanına işlem yapılmaması için bu yöntem kullanıldı.
        public string FoundationYear { get; set; }
        public bool? ShowSubVendorLogo { get; set; }
        public decimal AppearingProfitMarkup { get; set; }
        public bool? BirthdayRequired { get; set; }
        public bool? FindeksRequired { get; set; }
        public bool? SendEmailToBranch { get; set; }
        public bool? UseLocalDeposit { get; set; }
        public int? ToleranceTime { get; set; }
        public bool? FlightNumberRequired { get; set; }
        public int? EarliestResTime { get; set; }
        public int CountryId { get; set; }
        public bool SendAvailabilityRequest { get; set; }
        public bool ExtraDescriptionFromVendor { get; set; }
        public bool SendDefaultMailAddress { get; set; }
        public bool DeliveryTypeFromVendor { get; set; }
        public bool HideLocationAddressOnPayment { get; set; }
    }

    public enum VendorTypes
    {
        KolayCAR = 1,
        KolayCARBroker,
        Avec,
        Pandora,
        Aytu,
        Ayes,
        Europcar,
        Central,
        Nissa,
        Rigorent,
        GreenMotion,
        Ekar,
        Hara,
        FiloNova,
        Elitcar,
        Cizgi,
        Avec2,
        Wishcar,
        Otocar,
        Garenta,
        Assist,
        CredyCar,
        Erboycar,
        Circular,
        Turmobil,
        Akkor,
        Dailydrive,
        Elibol,
        YDZ,
        Enterprise,
        Yolcu360,
        Central2,
        BesS,
        Beto,
        Turevrac,
        Sixt,
        Renticar,
        Nissa2,
        Avec3,
        Ekar2,
        Circular2,
        AutoHome,
        Ototur,
        Otorento,
        Goldcar,
        Avis,
        Turevrac2,
        Renteon,
        OKMobility,
        Jimpisoft,
        Wheelsys,
        Centauro,
        Wiber,
        Simply,
        MyRent,
        RecordGo,
        ClickRent,
        KlassWagen,
        EnterpriseGlobal,
        Thermeon,
        AmericaCROTA,
        RCM,
        Easirent,
        GoRentals,
        Locauto,
        RecordGoES,
        Surprice,
        Europcar2,
        Rently,
        EverythingFleet,
        EnUygun,
        GreenmotionGLobal,
        Flexways,
        ABG,
        MEX,
        Eganis,
        Karve,
        Hertz,
        Drivalia,
        RightCars,
        FirstCar,
        DriveCarRental,
        Jucy,
        Zezgo,
        Free2Move,
        Yesaway,
        FOCO,
        GobyCar,
        Localiza,
        RentHub,
        Motus,
        Economy,
        Bluu,
        ACE,
        TreasureCaribe,
        NPAutoGroup,
        Sicily,
        ABGCanaries,
        Elephant,
        Sixt2,
        ZiraatFilo,
        Movida,
        Surprice2,
        Routes,
        Garajlar,
        Unidas,
        AutoRentals,
        Yolcu360v2,
        ArnoldClark,
        YesOto,
        Eren,
        RentGo
    }

    public enum PriceRoundingTypes
    {
        RoundUp,
        RoundDown,
        DoNotRounding
    }

    public enum VendorWorkingTypes
    {
        ProfitMarkup,
        Commission
    }

    public enum CreditType
    {
        Non,
        FullCredit
    }
}
