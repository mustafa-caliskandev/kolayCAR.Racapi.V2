using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class GarentaRequestBase
    {
        public SapProps sap_props { get; set; }
        public Import import { get; set; }

        public enum ServiceTypes
        {
            GET_LOCATIONS,
            SEARCH_AVAILABILITY,
            GET_EXTRAS,
            SEARCH_EXTRAS,
            CREATE_BROKER_RESERV,
            CANCEL_BROKER_RESERV
        }

        public enum LanguageTypes
        {
            T
        }

        public class SapProps
        {
            public string SERVICE_NAME { get; set; }
            public string USER_NAME { get; set; }
            public string PASSWORD { get; set; }
        }

        public class ISINPUT_BASE
        {
            public string BROKER_CODE { get; set; }
            public string LANGU { get; set; }
        }

        public class ISINPUT_SEARCH : ISINPUT_BASE
        {
            public SEARCH SEARCH { get; set; }
        }

        public class ISINPUT_SEARCH_EXTRAS : ISINPUT_BASE
        {
            public SEARCH_EXTRAS SEARCH { get; set; }
        }

        public class ISINPUT_RESERVATION : ISINPUT_BASE
        {
            public RESERVATION RESERVATION { get; set; }
            public List<EXTRA> EXTRA { get; set; }
        }

        public class ISINPUT_CANCEL_RESERVATION : ISINPUT_BASE
        {
            public string PNR_NO { get; set; }
        }

        public class SEARCH
        {
            public string SIPP_CODE { get; set; }
            public string PICKUP_DATE { get; set; }
            public string PICKUP_TIME { get; set; }
            public string DROPOFF_DATE { get; set; }
            public string DROPOFF_TIME { get; set; }
            public string PICKUP_OFFICE { get; set; }
            public string RETURN_OFFICE { get; set; }
        }

        public class SEARCH_EXTRAS : SEARCH
        {
            public List<EXTRA> EXTRAS { get; set; }
        }

        public class RESERVATION : SEARCH
        {
            public string NAME { get; set; }
            public string SURNAME { get; set; }
            public string TEL_NO { get; set; }
            public string EMAIL { get; set; }
            public string NATIO { get; set; }
            public string BROKER_RESERV_NO { get; set; }
            public string CAMPAIGN_ID { get; set; }
            public string SEARCH_REFERENCE { get; set; }
        }

        public class EXTRA
        {
            public string PRODUCT_ID { get; set; }
            public string COUNT { get; set; }
        }

        public class Import
        {
            public dynamic IS_INPUT { get; set; }
        }
    }
}
