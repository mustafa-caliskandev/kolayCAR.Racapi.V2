namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class KolayCARRequest
    {
        public class PostKolayCARReservationRequest
        {
            public string APIKEY { get; set; }
            public string APIPASSWORD { get; set; }
            public string LANGISOCODE { get; set; }
            public string CURRENCYISOCODE { get; set; }
            public string PICKUPLOCATIONID { get; set; }
            public string RETURNLOCATIONID { get; set; }
            public string PICKUPDATE { get; set; }
            public string RETURNDATE { get; set; }
            public string PICKUPTIME { get; set; }
            public string RETURNTIME { get; set; }
            public string VENDORID { get; set; }
            public string VEHICLEID { get; set; }
            public string EXTRAIDLIST { get; set; }
            public string CUSTOMERINSTUTIONTYPENO { get; set; }
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
            public string PAIDAMOUNT { get; set; }
            public string SENDMAIL { get; set; }
            public string UPDATERESNO { get; set; }
            public bool CREDITCARDPAYMENTTYPEACTIVE { get; set; }
            public bool ADVANCEPAYMENTTYPEACTIVE { get; set; }
            public string BANKID { get; set; }
            public string BANKVENDORID { get; set; }
            public string CREDITCARDHOLDER { get; set; }
            public string CREDITCARDNO { get; set; }
            public string EXPIREDYEAR { get; set; }
            public string EXPIREDMONTH { get; set; }
            public string SECURITYCODE { get; set; }
            public string INSTALLMENTCOUNT { get; set; }
            public string THREEDPAYMENTACTIVE { get; set; }
            public string PARAM1VALUE { get; set; }
            public string PARAM2VALUE { get; set; }
            public string PARAM3VALUE { get; set; }
            public string PARAM4VALUE { get; set; }
            public string PARAM5VALUE { get; set; }
            public string PARAM6VALUE { get; set; }
            public string PARAM7VALUE { get; set; }
            public string PARAM8VALUE { get; set; }
            public string PARAM9VALUE { get; set; }
            public string PARAM10VALUE { get; set; }
            public string PARAM11VALUE { get; set; }
            public string PARAM12VALUE { get; set; }
            public string PARAM13VALUE { get; set; }
            public string PARAM14VALUE { get; set; }
        }

        public class PostPaymentRequest
        {
            public string APIKEY { get; set; }
            public string APIPASSWORD { get; set; }
            public string LANGISOCODE { get; set; }
            public string CURRENCYISOCODE { get; set; }
            public string VENDORID { get; set; }
            public string DOMAINID { get; set; }
            public string BANKVENDORID { get; set; }
            public string CUSTOMERMAILADDRESS { get; set; }
            public string CREDITCARDHOLDER { get; set; }
            public string CREDITCARDNUMBER { get; set; }
            public string CREDITCARDEXPIREDYEAR { get; set; }
            public string CREDITCARDEXPIREDMONTH { get; set; }
            public string SECURITYCODE { get; set; }
            public string INSTALLMENTCOUNT { get; set; }
            public string PAYMENTAMOUNT { get; set; }
            public string ORDERNO { get; set; }
            public string IPADRESS { get; set; }
            public bool THREEDPAYMENTACTIVE { get; set; }
            public string PARAM1VALUE { get; set; }
            public string PARAM2VALUE { get; set; }
            public string PARAM3VALUE { get; set; }
            public string PARAM4VALUE { get; set; }
            public string PARAM5VALUE { get; set; }
            public string PARAM6VALUE { get; set; }
            public string PARAM7VALUE { get; set; }
            public string PARAM8VALUE { get; set; }
            public string PARAM9VALUE { get; set; }
            public string PARAM10VALUE { get; set; }
            public string PARAM11VALUE { get; set; }
            public string PARAM12VALUE { get; set; }
            public string PARAM13VALUE { get; set; }
            public string PARAM14VALUE { get; set; }
        }

        public class PostCancelKolayCARReservationRequest
        {
            public string APIKEY { get; set; }
            public string APIPASSWORD { get; set; }
            public string LANGISOCODE { get; set; }
            public string RESERVATIONNO { get; set; }
            public string COMMENT { get; set; }
            public string TOKEN { get; set; }
            public string PARAM1 { get; set; }
            public string PARAM2 { get; set; }
            public string PARAM3 { get; set; }
            public string PARAM4 { get; set; }
            public string PARAM5 { get; set; }
            public string PARAM6 { get; set; }
        }
    }
}
