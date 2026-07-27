using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class GarentaResponseBase
    {
        public EXPORTDATA EXPORT { get; set; }

        public class ISSUCCESS
        {
            public string SUCCESS { get; set; }
        }

        public class HOUROFWORK
        {
            public string OFFICE_CODE { get; set; }
            public string DAY { get; set; }
            public string DAY_DESC { get; set; }
            public string BEGTIME { get; set; }
            public string ENDTIME { get; set; }
        }

        public class LOCATION
        {
            public string OFFICE_CODE { get; set; }
            public string OFFICE_DESC { get; set; }
            public string XCOORDINATE { get; set; }
            public string YCOORDINATE { get; set; }
            public string ADDRESS { get; set; }
            public string TEL { get; set; }
            public string EMAIL { get; set; }
            public List<HOUROFWORK> HOUR_OF_WORK { get; set; }
        }

        public class ESOUTPUT
        {
            public ISSUCCESS SUCCESS { get; set; }
            public List<RETURN_MESSAGE> MESSAGE { get; set; }
            public List<LOCATION> LOCATION { get; set; }
            public List<VEHICLE> RETURN { get; set; }
            public List<ADD_PROD> ADD_PROD { get; set; }
            public List<EXTRA> EXTRAS { get; set; }
            public RESERVINFO RESERV_INFO { get; set; }
        }

        public class ADD_PROD
        {
            public string PRODUCT_ID { get; set; }
            public string PRODUCT_DESC { get; set; }
            public double PRICE { get; set; }
            public string CURRENCY { get; set; }
            public int PRICE2 { get; set; }
            public string CURRENCY2 { get; set; }
        }

        public class RETURN_MESSAGE
        {
            public string TYPE { get; set; }
            public string MESSAGE { get; set; }
        }

        public class VEHICLE
        {
            public string PICKUP_OFFICE { get; set; }
            public string SIPP_CODE { get; set; }
            public double DISCOUNT_AMOUNT { get; set; }
            public double NET_AMOUNT { get; set; }
            public string CURRENCY { get; set; }
            public float? DEPOSIT_AMOUNT { get; set; }
            public string IS_CAMPAIGN { get; set; } = null;
            public string SEARCH_REFERENCE { get; set; } = null;
            public string MIN_AGE { get; set; }
            public string MIN_LICENSE_AGE { get; set; }
            public string MIN_YOUNG_LICENSE_AGE { get; set; }
            public string MAX_KM { get; set; }
            public string MAX_MONTHLY_KM { get; set; }
            public string EXCESS_KM_PRICE { get; set; }
            public string FLEET_INFORMATION { get; set; }
            public META_DATA META_DATA { get; set; }
        }

        public class META_DATA
        {
            public string MIN_AGE { get; set; }
            public string MIN_LICENSE_AGE { get; set; }
            public string MIN_YOUNG_AGE { get; set; }
            public string MIN_YOUNG_LICENSE_AGE { get; set; }
            public string MAX_KM { get; set; }
            public string MAX_MONTHLY_KM { get; set; }
            public string EXCESS_KM_PRICE { get; set; }
            public string FLEET_INFORMATION { get; set; }
        }

        public class STATIC_VEHICLE
        {
            public string CarGroup { get; set; }
            public string NewSipp { get; set; }
            public string Brand { get; set; }
            public string Model { get; set; }
            public string Type { get; set; }
            public string Fuel { get; set; }
            public string Transmission { get; set; }
            public string Doors { get; set; }
            public string Seats { get; set; }
            public string Deposit { get; set; }
            public string MinDriverAge { get; set; }
            public string YoungDriverAge { get; set; }
            public string MinDriverLicenseAge { get; set; }
            public string DailyKmLimit { get; set; }
            public string TotalKmLimit { get; set; }
        }

        public class EXTRA
        {
            public string PRODUCT_ID { get; set; }
            public string PROD_DESC_LARGE { get; set; }
            public string PROD_DESC_SHORT { get; set; }
            public string MANDATORY { get; set; }
            public string PRICE { get; set; }
            public string DISC_PRICE { get; set; }
            public string DISCOUNT_AMOUNT { get; set; }
            public string NET_AMOUNT { get; set; }
            public string CURRENCY { get; set; }
            public string SIPP_CODE { get; set; }
            public string MAX_COUNT { get; set; }
            public string MAX_DAY { get; set; }
            public string INFO_TEXT { get; set; }
        }

        public class RESERVINFO
        {
            public string PNR_CODE { get; set; }
        }

        public class RETURN
        {
            public string ID { get; set; }
            public string NUMBER { get; set; }
            public string MESSAGE { get; set; }
        }

        public class EXPORTDATA
        {
            public ESOUTPUT ES_OUTPUT { get; set; }
        }
    }
}
