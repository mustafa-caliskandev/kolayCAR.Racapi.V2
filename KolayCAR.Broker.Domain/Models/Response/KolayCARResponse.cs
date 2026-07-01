using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class KolayCARResponseBase
    {
        public int RETURNCODE { get; set; }
        public string MESSAGE { get; set; }
        public List<LOCATION> LOCATIONS { get; set; }
        public List<VEHICLE> VEHICLES { get; set; }
        public List<EXTRA> EXTRAS { get; set; }
        public List<AGENCY> AGENCIES { get; set; }
        public bool ISACTIVE { get; set; }
        public string AGENTNAME { get; set; }
        public string AGENTMAILADDRESS { get; set; }
        public string PARENTMAILADDRESS { get; set; }
        public string AGENTADDRESS { get; set; }
        public string AGENTTELEPHONE { get; set; }
        public string AGENTGSM { get; set; }
        public string AGENTLOGO { get; set; }
        public string AGENTAUTHORIZED { get; set; }
        public bool AGENTSERVICEACTIVE { get; set; }
        public int DEFAULTVENDORID { get; set; }
        public int DEFAULTDOMAINID { get; set; }
        public string DEFAULTVENDORLOGO { get; set; }
        public string DEFAULTVENDORWHATSAPPTELEPHONE { get; set; }
        public string DEFAULTVENDORRENTCONTRACT { get; set; }
        public string DEFAULTVENDORSPECIALAREA1 { get; set; }
        public string DEFAULTVENDORSPECIALAREA2 { get; set; }
        public string DEFAULTVENDORSPECIALAREA3 { get; set; }
        public bool AGENTDETAILLOGACTIVE { get; set; }
        public int AGENCYCOMMISSIONTYPE { get; set; }
        public string AGENCYRESEMAIL { get; set; }
        public List<SETTINGS_LANGUAGE> LANGUAGES { get; set; }
        public List<SETTINGS_CURRENCY> CURRENCY { get; set; }
        public int AGENCYRENTALFEETYPE { get; set; }
        public int AGENCYEXTRASFEETYPE { get; set; }
        public int AGENCYONEWAYFEETYPE { get; set; }
        public int SQLSERVERTIMEOUT { get; set; }
        public string LOADBALANCESERVER { get; set; }
        public string APICOUNTERVALUE { get; set; }
        public string CUSTOMERMAILSENDING { get; set; }
        public string CUSTOMERSMSSENDING { get; set; }
        public List<RESSOURCETYPES> RESSOURCETYPE { get; set; }
        public string RESERVATIONNO { get; set; }
        public List<RESERVATION> RESERVATION { get; set; }
        public List<RESERVATIONINFO> RESERVATIONINFO { get; set; }
        public List<VENDOR> VENDOR { get; set; }
        public List<INSTALLMENTS> INSTALLMENTS { get; set; }
        public List<BANK> BANK { get; set; }
        public string THREEDSHTMLKEY { get; set; }
        public List<PAYMENT> PAYMENT { get; set; }
    }

    public class LOCATION
    {
        public int COUNTRYID { get; set; }
        public string COUNTRYNAME { get; set; }
        public int CITYID { get; set; }
        public string CITYNAME { get; set; }
        public int LOCATIONID { get; set; }
        public string LOCATIONNAME { get; set; }
        public List<LOCATIONTYPES> LOCATIONTYPES { get; set; }
        public string IATACODE { get; set; }
        public bool ISAIRPORT { get; set; }
        public bool ISPICKUP { get; set; }
        public List<LOCATIONGEOS> LOCATIONGEOS { get; set; }
    }

    public class LOCATIONTYPES
    //LOCATIONTYPEID => 1 => Ofis 
    //LOCATIONTYPEID => 2 => Karşılama 
    {
        public int LOCATIONTYPEID { get; set; }
        public string LOCATIONTYPENAME { get; set; }
    }

    public class LOCATIONGEOS
    {
        public string LOCATIONGEO { get; set; }
    }

    public class VENDOR
    {
        public int VENDORID { get; set; }
        public string VENDORNAME { get; set; }
        public string VENDORPHONE { get; set; }
        public string VENDORADDRESS { get; set; }
        public int VENDORTYPE { get; set; }
        public string APIKEY { get; set; }
        public string APIPASSWORD { get; set; }
        public bool ACTIVE { get; set; }
        public bool RESLISTACTIVE { get; set; }
        public string VENDORPICKUPLOCATIONADDRESS { get; set; }
        public string VENDORRETURNLOCATIONADDRESS { get; set; }
        public string VENDORPICKUPLOCATIONPHONE { get; set; }
        public string VENDORRETURNLOCATIONPHONE { get; set; }
    }

    public class RESSOURCETYPES
    {
        public int RESSOURCEID { get; set; }
        public string RESSOURCE { get; set; }
    }

    public class SETTINGS_LANGUAGE
    {
        public int LANGUAGEID { get; set; }
        public string LANGUAGEISOCODE { get; set; }
        public bool DEFAULTLANGUAGE { get; set; }
    }

    public class SETTINGS_CURRENCY
    {
        public int CURRENCYID { get; set; }
        public string CURRENCYISOCODE { get; set; }
        public bool DEFAULTCURRENCY { get; set; }
    }

    public class VEHICLE
    {
        public int VENDORID { get; set; }
        public string VENDORNAME { get; set; }
        public string VENDORVEHICLENAME { get; set; }
        public string VENDORLOGO { get; set; }
        public string VEHICLENAME { get; set; }
        public string VEHICLEDESCRIPTION { get; set; }
        public string SIPPCODE { get; set; }
        public int VEHICLEID { get; set; }
        public string PICKUPLOCATIONNAME { get; set; }
        public int PICKUPLOCATIONID { get; set; }
        public string RETURNLOCATIONNAME { get; set; }
        public int RETURNLOCATIONID { get; set; }
        public int VENDORPICKUPLOCATIONID { get; set; }
        public string VENDORPICKUPLOCATIONNAME { get; set; }
        public int VENDORRETURNLOCATIONID { get; set; }
        public string VENDORRETURNLOCATIONNAME { get; set; }
        public DateTime PICKUPDATETIME { get; set; }
        public DateTime RETURNDATETIME { get; set; }
        public int MINIMUMRENTALDURATIONDAYS { get; set; }
        public int RENTALDURATION { get; set; }
        public float DAILYPRICE { get; set; }
        public float ONEWAYFEE { get; set; }
        public float EXTRAPRICE { get; set; }
        public float TOTALPRICE { get; set; }
        public int ISAVAILABLE { get; set; }
        public List<RENTALCONDITIONS> RENTALCONDITIONS { get; set; }
        public List<VEHICLEIMAGES> VEHICLEIMAGES { get; set; }
        public string VEHICLETYPE { get; set; }
        public int VEHICLETYPEID { get; set; }
        public string TRANSMISSIONTYPE { get; set; }
        public int TRANSMISSIONTYPEID { get; set; }
        public string VEHICLECATEGORY { get; set; }
        public int VEHICLECATEGORYID { get; set; }
        public string PASSENGERQUANTITY { get; set; }
        public int PASSENGERQUANTITYID { get; set; }
        public string FUELTYPE { get; set; }
        public int FUELTYPEID { get; set; }
        public string BAGGAGEQUANTITY { get; set; }
        public int BAGGAGEQUANTITYID { get; set; }
        public bool ISAIRCONDITION { get; set; }
        public int VENDORMINDRIVERAGE { get; set; }
        public int VENDORMINDRIVINGLICENSEAGE { get; set; }
        public bool DELIVERYPAYMENTACTIVE { get; set; }
        public int CREDITCARDPAYMENTTYPE { get; set; }
        public int ADVANCEPAYMENTTYPE { get; set; }
        public float DAILYPRICEPAYNOW { get; set; }
        public float TOTALPRICEPAYNOW { get; set; }
        public string DISCOUNTTYPE { get; set; }
        public float DISCOUNTPERCENT { get; set; }
        public float DISCOUNTPRICE { get; set; }
        public float DISCOUNTTOTALPRICE { get; set; }
        public float DISCOUNTTOTALPRICEPAYNOW { get; set; }
        public float DISCOUNTPERCENTEPAYNOW { get; set; }
        public float DEPOSITPRICE { get; set; }
        public string COMMISSIONFREEPAYMENTTYPE { get; set; }
        public bool FREEDAILYPRICE { get; set; }
        public bool FREEEXTRAPRICE { get; set; }
        public bool FREEONEWAYFEE { get; set; }
        public int VENDORPRICECALCTYPE { get; set; }
        public string TOTALKMLIMIT { get; set; }
    }

    public class RENTALCONDITIONS
    {
        public string RENTALCONDITIONNAME { get; set; }
    }
    public class VEHICLEIMAGES
    {
        public string VEHICLEIMAGE { get; set; }
    }

    public class EXTRA
    {
        public int VENDORID { get; set; }
        public string VENDORNAME { get; set; }
        public int EXTRAID { get; set; }
        public string EXTRANAME { get; set; }
        public string EXTRADESCRIPTION { get; set; }
        public string EXTRATYPE { get; set; }
        public int EXTRAPERDAY { get; set; }
        public string EXTRAQUANTITYINCREASABLE { get; set; }
        public float PRICE { get; set; }
    }

    public class AGENCY
    {
        public int PORTALAGENCYID { get; set; }
        public bool ACTIVE { get; set; }
        public string AGENCYNAME { get; set; }
        public string TELEPHONE { get; set; }
        public string EMAIL { get; set; }
        public string ADDRESS { get; set; }
        public string GSM { get; set; }
        public string AUTHORIZED { get; set; }
        public string APIKEY { get; set; }
        public string PASSWORD { get; set; }
        public DateTime DATE { get; set; }
        public string AGENCYCODE { get; set; }
        public float AGENCYCOMMISSION { get; set; }
        public int PARENT { get; set; }
        public int AGENCYRENTALFEETYPE { get; set; }
        public int AGENCYEXTRASFEETYPE { get; set; }
        public int AGENCYONEWAYFEETYPE { get; set; }
        public int AGENCYCOMMISSIONTYPE { get; set; }
    }

    public class RESERVATION
    {
        public string RESERVATIONNO { get; set; }
        public bool CUSTOMERMAILFORWARD { get; set; }
        public bool ADMINMAILFORWARD { get; set; }
        public string PDF { get; set; }
        public string SMSFORWARD { get; set; }
    }

    public class RESERVATIONINFO
    {
        public string RESERVATIONNO { get; set; }
        public DateTime RESERVATIONDATETIME { get; set; }
        public int RESERVATIONSTATUSID { get; set; }
        public string RESERVATIONSTATUS { get; set; }
        public string RESERVATIONEXTRA { get; set; }
        public int RESERVATIONSOURCETYPE { get; set; }
        public int HOMEAGENCYID { get; set; }
        public string HOMEAGENCYNAME { get; set; }
        public string HOMEAGENCYPASS { get; set; }
        public int SUBAGENCYID { get; set; }
        public string SUBAGENCYNAME { get; set; }
        public int VENDORID { get; set; }
        public int VEHICLEID { get; set; }
        public string VEHICLENAME { get; set; }
        public int VENDORPICKUPLOCATIONID { get; set; }
        public string VENDORPICKUPLOCATIONNAME { get; set; }
        public int VENDORRETURNLOCATIONID { get; set; }
        public string VENDORRETURNLOCATIONNAME { get; set; }
        public DateTime PICKUPDATETIME { get; set; }
        public DateTime RETURNDATETIME { get; set; }
        public int RENTALDURATION { get; set; }
        public int CUSTOMERINSTUTIONTYPENO { get; set; }
        public string CUSTOMERNAME { get; set; }
        public string CUSTOMERSURNAME { get; set; }
        public string CUSTOMERTELEPHONE { get; set; }
        public string CUSTOMEREMAIL { get; set; }
        public string CUSTOMERPERSONALNUMBER { get; set; }
        public string CUSTOMERNOTE { get; set; }
        public string CUSTOMEREXPLANATION { get; set; }
        public string COMPANYTITLE { get; set; }
        public string COMPANYADDRESS { get; set; }
        public string COMPANYTAXOFFICE { get; set; }
        public string COMPANYTAXNO { get; set; }
        public string FLIGHTNOARRIVAL { get; set; }
        public string FLIGHTNODEPARTURE { get; set; }
        public string CUSTOMERIP { get; set; }
        public float DAILYPRICE { get; set; }
        public float ONEWAYFEE { get; set; }
        public float EXTRAPRICE { get; set; }
        public float TOTALPRICE { get; set; }
        public float PAIDAMOUNT { get; set; }
        public float RATE { get; set; }
        public string CURRENCYISOCODE { get; set; }
        public string LANGUAGEISOCODE { get; set; }
        public string PDFURL { get; set; }
    }

    public class INSTALLMENTS
    {
        public string INSTALLMENT { get; set; }
        public string PERCENT { get; set; }
        public string INSTALLMENTTYPE { get; set; }
        public string BINNO { get; set; }
        public float INSTALLMENTTOTALAMOUNT { get; set; }
        public float INSTALLMENTONLYPRICE { get; set; }
        public string COMMENT { get; set; }
    }

    public class BANK
    {
        public int BANKID { get; set; }
        public string BANKNAME { get; set; }
        public int BANKVENDORID { get; set; }
        public string BANKLOGOPOSITION { get; set; }
        public string LOGOPOSITION { get; set; }
        public string PRODTHREEDHOST { get; set; }
        public string PRODPROVISIONHOST { get; set; }
        public string TESTTHREEDHOST { get; set; }
        public string TESTPROVISIONHOST { get; set; }
        public bool DEFAULT { get; set; }
        public string BANKDEFINITION { get; set; }
        public bool INSTALLMENTACTIVE { get; set; }
        public bool THREEDACTIVE { get; set; }
        public int THREEDSTATUS { get; set; }
        public int VIRTUALPOSENVIRONMENT { get; set; }
        public string BANKBRANCHCODE { get; set; }
        public string BANKACCOUNTNO { get; set; }
        public string IBAN { get; set; }
        public string PARAM1VALUE { get; set; }
        public string PARAM2VALUE { get; set; }
        public string PARAM3VALUE { get; set; }
        public string PARAM4VALUE { get; set; }
        public string PARAM5VALUE { get; set; }
        public string BANKVIRTUALPOSTYPE { get; set; }
        public bool BANKAMEXCARDACTIVE { get; set; }
    }

    public class PAYMENT
    {
        public string BANKID { get; set; }
        public string PROVISIONNO { get; set; }
        public double PAYMENTAMOUNT { get; set; }
        public string CURRENCYISOCODE { get; set; }
        public string ALERTERRORCODE { get; set; }
        public string LOGRESULTNO { get; set; }
        public string LOGERRORCODE { get; set; }
        public string ORDERNO { get; set; }
    }

    public class POST_SMS_RESPONSE_ROOT
    {
        public POST_SMS_RESPONSE ROOT { get; set; }
    }

    public class POST_SMS_RESPONSE
    {
        public string RETURNCODE { get; set; }
        public string MESSAGE { get; set; }
        public bool SMSGITTI { get; set; }
        public string SMSGITTIMESAJ { get; set; }
    }
    public class LANGUAGE
    {
        public int LANGUAGEID { get; set; }
        public string LANGUAGEISOCODE { get; set; }
        public bool DEFAULTLANGUAGE { get; set; }
    }

    public class CURRENCY
    {
        public int CURRENCYID { get; set; }
        public string CURRENCYISOCODE { get; set; }
        public bool DEFAULTCURRENCY { get; set; }
    }

    public class KOLAYCARSETTINGS
    {
        public string RETURNCODE { get; set; }
        public string MESSAGE { get; set; }
        public bool ISACTIVE { get; set; }
        public string AGENTNAME { get; set; }
        public string AGENTMAILADDRESS { get; set; }
        public string PARENTMAILADDRESS { get; set; }
        public string AGENTADDRESS { get; set; }
        public string AGENTTELEPHONE { get; set; }
        public string AGENTGSM { get; set; }
        public string AGENTLOGO { get; set; }
        public string AGENTAUTHORIZED { get; set; }
        public bool AGENTSERVICEACTIVE { get; set; }
        public string DEFAULTVENDORID { get; set; }
        public string DEFAULTDOMAINID { get; set; }
        public string DEFAULTVENDORLOGO { get; set; }
        public string DEFAULTVENDORWHATSAPPTELEPHONE { get; set; }
        public string DEFAULTVENDORSPECIALAREA1 { get; set; }
        public string DEFAULTVENDORSPECIALAREA2 { get; set; }
        public string DEFAULTVENDORSPECIALAREA3 { get; set; }
        public bool AGENTDETAILLOGACTIVE { get; set; }
        public string AGENCYCOMMISSIONTYPE { get; set; }
        public string AGENCYRESEMAIL { get; set; }
        public List<LANGUAGE> LANGUAGES { get; set; }
        public List<CURRENCY> CURRENCY { get; set; }
        public string AGENCYRENTALFEETYPE { get; set; }
        public string AGENCYEXTRASFEETYPE { get; set; }
        public string AGENCYONEWAYFEETYPE { get; set; }
        public bool FULLCREDITACTIVE { get; set; }
        public bool FULLCREDITLIMITEDACTIVE { get; set; }
        public string RESQUERYTRANSFORMATION { get; set; }
        public List<object> RESSOURCETYPE { get; set; }
        public string UPDATERESERVATIONTYPE { get; set; }
    }
}
