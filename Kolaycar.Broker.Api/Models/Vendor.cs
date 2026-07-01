using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace KolayCAR.Broker.API.Models
{
    public partial class Vendor
    {
        public int Vendorid { get; set; }
        public int Vendortype { get; set; }
        public string Vendorname { get; set; }
        public string Apikey { get; set; }
        public string Apipassword { get; set; }
        public bool? Active { get; set; }
        public bool? Reslistactive { get; set; }
        public decimal? Profitmarkup { get; set; }
        public int Priceroundingtype { get; set; }
        public int Currencyid { get; set; }
        public string Apibaseurl { get; set; }
        public decimal Servicecharge { get; set; }
        public int Servicechargecurrencyid { get; set; }
        public bool? Profitmarkupdailypriceactive { get; set; }
        public bool? Profitmarkupextraactive { get; set; }
        public bool? Profitmarkuponewayfeeactive { get; set; }
        public string Logo { get; set; }
        public bool? Depositcreditcardrequired { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public decimal? Profitmarkupadditionalproducts { get; set; }
        public decimal? Profitmarkuponewayfee { get; set; }
        public bool? Vehiclemappingactive { get; set; }
        public bool? Extramappingactive { get; set; }
        public string Apiclientid { get; set; }
        public string Secretkey { get; set; }
        public bool? Apiphoneactive { get; set; }
        public int? Apitimeout { get; set; }
        public string Companytitle { get; set; }
        public bool? Disabledeposit { get; set; }
        public bool? Personelnumberrequired { get; set; }
        public bool? Sendreservationmailtovendor { get; set; }
        public string Availablecurrencies { get; set; }
        public bool? Useonlydefaultcurrency { get; set; }
        public bool Resagencynamesending { get; set; }
        public bool? Sellingbelowcostforcouponcode { get; set; }
        public bool? Usebrokerconfigurations { get; set; }
        public int Rentalworkingtype { get; set; }
        public int Additionalproductworkingtype { get; set; }
        public int Onewayfeeworkingtype { get; set; }
        public bool? Couponcodeactive { get; set; }
        public int? Freecancellationhour { get; set; }
        public bool? Showcustomernotearea { get; set; }
        public bool? Showflightnumberarea { get; set; }
        public bool Documentrequiredshow { get; set; }
        public string Apidescription { get; set; }
        public bool? PRIORITYFEE { get; set; }
        public short? CreditType { get; set; }

        public string BankName { get; set; }
        public string BankBranchCode { get; set; }
        public string IBAN { get; set; }
        public string AccountNumber { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Address { get; set; }
        public string PersonalNumber { get; set; }
        public string TaxNumber { get; set; }
        public string TaxOffice { get; set; }
        public int? InvoiceOwner { get; set; }
        public string Dbs { get; set; }
        public bool? VendorComissionInvoice { get; set; }
        public bool? IsPopular { get; set; }
        public int? VendorOrder { get; set; }
        public string FoundationYear { get; set; }
        public bool? ShowSubVendorLogo { get; set; }
        public decimal? AppearingProfitMarkup { get; set; }
        public bool? BirthdayRequired { get; set; }
        public bool? FindeksRequired { get; set; }
        public bool? SendEmailToBranch { get; set; }
        public bool? UseLocalDeposit { get; set; }
        public int? ToleranceTime { get; set; }
        public bool? FlightNumberRequired { get; set; }
        public int? EarliestResTime { get; set; }
        public int CountryId { get; set; }
        public bool? SendAvailabilityRequest { get; set; }
        public bool? ExtraDescriptionFromVendor { get; set; }
        public bool? SendDefaultMailAddress { get; set; }
        public bool? DeliveryTypeFromVendor { get; set; }
        [NotMapped]
        public bool HideLocationAddressOnPayment { get; set; }
        public virtual ICollection<ProfitMarkupVendor> ProfitMarkupVendors { get; set; }
    }
}
