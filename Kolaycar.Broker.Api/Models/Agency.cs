using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models
{
    public partial class Agency
    {
        public int Agencyid { get; set; }
        public bool? Active { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Postalcode { get; set; }
        public string Pobox { get; set; }
        public string Place { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Tktue { get; set; }
        public DateTime? Lastedit { get; set; }
        public string Paymentmethod { get; set; }
        public string Taxnumber1 { get; set; }
        public string Taxnumber2 { get; set; }
        public string Branch { get; set; }
        public DateTime? Barrier { get; set; }
        public int? Limit { get; set; }
        public DateTime? Creationdate { get; set; }
        public int? Countryid { get; set; }
        public int? Cityid { get; set; }
        public string Taxoffice { get; set; }
        public string Ownername { get; set; }
        public string Ownersurname { get; set; }
        public bool? Agencyapplication { get; set; }
        public decimal? Commission { get; set; }
        public string Notes { get; set; }
        public string Notes2 { get; set; }
        public string Countryname { get; set; }
        public string Cityname { get; set; }
        public string Agencyapikey { get; set; }
        public string Agencyapipassword { get; set; }
        public string Logo { get; set; }
        public int? Roleid { get; set; }
        public int? Agencycommissiontype { get; set; }
        public string Agencycode { get; set; }
        public decimal? Profitmarkupsharingratedailyprice { get; set; }
        public decimal? Profitmarkupsharingrateadditionalproducts { get; set; }
        public decimal? Profitmarkupsharingrateonewayfee { get; set; }
        public bool? Loginenable { get; set; }
        public bool Creditcardpaymentactive { get; set; }
        public bool Payallactive { get; set; }
        public bool Advancepaymentactive { get; set; }
        public bool Commissionfreepaymentactive { get; set; }
        public bool Paydeliveryactive { get; set; }
        public bool Payagencyactive { get; set; }
        public int? Creditcarddiscountpercent { get; set; }
        public bool? Freepriceshowactive { get; set; }
        public bool? Freepriceactive { get; set; }
        public bool Advancepaymentamountbyagencycommissionactive { get; set; }
        public bool? Sendreservationmailtocustomeractive { get; set; }
        public bool Isrestrictedapi { get; set; }
        public bool? Rentalamountdeliverypayment { get; set; }
        public bool? Onewayamountdeliverypayment { get; set; }
        public bool? Additionalproductamountdeliverypayment { get; set; }
        public bool? Sendreservationmailtoagency { get; set; }
        public int Currencyid { get; set; }
        public bool? Showsubagency { get; set; }
        public bool? Reservationsourceselectactive { get; set; }
        public decimal? Agencyrentalprofitmarkup { get; set; }
        public int? Surveyposttypeid { get; set; }
        public bool? Cancellationpenaltyactive { get; set; }
        public bool? SpecialParameters { get; set; }
        public short? CreditType { get; set; }
        public bool? IsActiveSendCheapestCar { get; set; }

        public virtual ICollection<ProfitMarkupAgency> ProfitMarkupAgencies { get; set; }
    }
}
