using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class OtorentoResponseBase
    {

        // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
        // <remarks/>
        //[System.SerializableAttribute()]
        //[System.ComponentModel.DesignerCategoryAttribute("code")]
        //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2003/05/soap-envelope")]
        //[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.w3.org/2003/05/soap-envelope", IsNullable = false)]
        //public partial class Envelope
        //{

        //    private EnvelopeBody bodyField;

        //    /// <remarks/>
        //    public EnvelopeBody Body
        //    {
        //        get
        //        {
        //            return this.bodyField;
        //        }
        //        set
        //        {
        //            this.bodyField = value;
        //        }
        //    }
        //}

        ///// <remarks/>
        //[System.SerializableAttribute()]
        //[System.ComponentModel.DesignerCategoryAttribute("code")]
        //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2003/05/soap-envelope")]
        //public partial class EnvelopeBody
        //{

        //    private GetTicketResponse getTicketResponseField;

        //    /// <remarks/>
        //    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://tempuri.org/")]
        //    public GetTicketResponse GetTicketResponse
        //    {
        //        get
        //        {
        //            return this.getTicketResponseField;
        //        }
        //        set
        //        {
        //            this.getTicketResponseField = value;
        //        }
        //    }
        //}

        ///// <remarks/>
        //[System.SerializableAttribute()]
        //[System.ComponentModel.DesignerCategoryAttribute("code")]
        //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://tempuri.org/")]
        //[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://tempuri.org/", IsNullable = false)]
        //public partial class GetTicketResponse
        //{

        //    private string getTicketResultField;

        //    /// <remarks/>
        //    public string GetTicketResult
        //    {
        //        get
        //        {
        //            return this.getTicketResultField;
        //        }
        //        set
        //        {
        //            this.getTicketResultField = value;
        //        }
        //    }
        //}



        public class GetTicketResponseBase
        {
            [JsonProperty("soap:Envelope")]
            public Envelope _Envelope { get; set; }

            [JsonProperty("?xml")]
            public Xml xml { get; set; }

        }
        public class GetTicketResponse
        {
            [JsonProperty("GetTicketResult")]
            public string GetTicketResult { get; set; }

        }


        public class Body
        {
            [JsonProperty("GetTicketResponse")]
            public GetTicketResponse GetTicketResponse { get; set; }
            [JsonProperty("SearchLocationsResponse")]
            public SearchLocationsResponse SearchLocationsResponse { get; set; }
            //[JsonProperty("SearchResponse")]
            //public SearchResponse SearchResponse { get; set; }
            [JsonProperty("SaveResponse")]
            public SaveResponse SaveResponse { get; set; }
            public CancelServiceResponse CancelServiceResponse { get; set; }
        }


        public class Envelope
        {
            [JsonProperty("soap:Body")]
            public Body Body { get; set; }
        }


        public class SearchLocations
        {
            [JsonProperty("Id")]

            public int Id { get; set; }
            [JsonProperty("LocationId")]

            public int LocationId { get; set; }
            [JsonProperty("LocationTextTr")]

            public string LocationTextTr { get; set; }
            [JsonProperty("LocationTextEn")]

            public string LocationTextEn { get; set; }
            [JsonProperty("LocationTextDe")]

            public string LocationTextDe { get; set; }
            [JsonProperty("TypeImage")]

            public string TypeImage { get; set; }
            [JsonProperty("Popularity")]

            public int Popularity { get; set; }
            [JsonProperty("City")]

            public string City { get; set; }
            [JsonProperty("CityEn")]

            public string CityEn { get; set; }
            [JsonProperty("CityDe")]

            public string CityDe { get; set; }
            [JsonProperty("Country")]

            public string Country { get; set; }
            [JsonProperty("CountryEn")]

            public string CountryEn { get; set; }
            [JsonProperty("CountryDe")]

            public string CountryDe { get; set; }
        }


        public class SearchLocationsResult
        {

            [JsonProperty("SearchLocations")]
            public List<SearchLocations> SearchLocations { get; set; }
        }

        [XmlRoot(ElementName = "SearchLocationsResponse")]
        public class SearchLocationsResponse
        {
            [JsonProperty("SearchLocationsResult")]

            public SearchLocationsResult SearchLocationsResult { get; set; }

        }

        //public class Supplier
        //{
        //    public int Id { get; set; }
        //    public string CompanyName { get; set; }
        //    public string BrandName { get; set; }
        //    public string RepFullName { get; set; }
        //    public string RepPhone { get; set; }
        //    public object RepEmail { get; set; }
        //    public bool InAirPort { get; set; }
        //    public double DeliverToAddressPrice { get; set; }
        //    public double ReturnFromAddressPrice { get; set; }
        //}

        //public class VehicleModel
        //{
        //    public string Id { get; set; }
        //    public int ProviderId { get; set; }
        //    public int DriverAge { get; set; }
        //    public bool IsOffice { get; set; }
        //    public int DriverLicenceYear { get; set; }
        //    public int Deposit { get; set; }
        //    public double KMExceedCharge { get; set; }
        //    public bool Findex { get; set; }
        //    public Supplier Supplier { get; set; }
        //    public string ImagePath { get; set; }
        //    public string Brand { get; set; }
        //    public string Model { get; set; }
        //    public string Serial { get; set; }
        //    public int Year { get; set; }
        //    public string Class { get; set; }
        //    public int NumberOfSeats { get; set; }
        //    public int NumberOfDoors { get; set; }
        //    public string Fuel { get; set; }
        //    public string Transmission { get; set; }
        //    public string FuelPolicy { get; set; }
        //    public string MileageLimit { get; set; }
        //    public string ImgS { get; set; }
        //    public string ImgB { get; set; }
        //    public int Price { get; set; }
        //    public int IntegralDailyPrice { get; set; }
        //    public int ScaleDailyPrice { get; set; }
        //    public string Sipp { get; set; }
        //    public int GroupId { get; set; }
        //    public int DropPrice { get; set; }
        //    public int BaggageLimit { get; set; }
        //    public int NumberOfDays { get; set; }
        //}

        //public class SearchResult
        //{
        //    [JsonProperty("VehicleModel")]
        //    public List<VehicleModel> VehicleModel { get; set; }
        //}

        //public class SearchResponse
        //{
        //    [JsonProperty("SearchResult")]
        //    public SearchResult SearchResult { get; set; }
        //    [JsonProperty("@xmlns")]
        //    public string Xmlns { get; set; }

        //}




        //-----------------------deneme------------------------------------------------------

        public class Root
        {
            [JsonProperty("?xml")]
            public Xml xml { get; set; }

            [JsonProperty("soap:Envelope")]
            public SoapEnvelope soapEnvelope { get; set; }
        }

        public class SearchResponse
        {
            [JsonProperty("@xmlns")]
            public string xmlns { get; set; }
            public SearchResult SearchResult { get; set; }
        }

        public class SearchResult
        {
            public List<VehicleModel> VehicleModel { get; set; }
        }

        public class SoapBody
        {
            public SearchResponse SearchResponse { get; set; }

            public SaveResponse SaveResponse { get; set; }

            public CancelServiceResponse CancelServiceResponse { get; set; }

            public StartChargeResponse StartChargeResponse { get; set; }
        }

        public class SoapEnvelope
        {
            [JsonProperty("@xmlns:soap")]
            public string xmlnssoap { get; set; }

            [JsonProperty("@xmlns:xsi")]
            public string xmlnsxsi { get; set; }

            [JsonProperty("@xmlns:xsd")]
            public string xmlnsxsd { get; set; }

            [JsonProperty("soap:Body")]
            public SoapBody soapBody { get; set; }
        }

        public class Supplier
        {
            public string Id { get; set; }
            public string CompanyName { get; set; }
            public string BrandName { get; set; }
            public string RepFullName { get; set; }
            public string RepPhone { get; set; }
            public string RepEmail { get; set; }
            public string InAirPort { get; set; }
            public string DeliverToAddressPrice { get; set; }
            public string ReturnFromAddressPrice { get; set; }
        }

        public class VehicleModel
        {
            public string Id { get; set; }
            public string ProviderId { get; set; }
            public string DriverAge { get; set; }
            public string isOffice { get; set; }
            public string DriverLicenceYear { get; set; }
            public string Deposit { get; set; }
            public string KMExceedCharge { get; set; }
            public string Findex { get; set; }
            public Supplier Supplier { get; set; }
            public string ImagePath { get; set; }
            public string Brand { get; set; }
            public string Model { get; set; }
            public string Serial { get; set; }
            public string Year { get; set; }
            public string Class { get; set; }
            public string NumberOfSeats { get; set; }
            public string NumberOfDoors { get; set; }
            public string Fuel { get; set; }
            public string Transmission { get; set; }
            public string FuelPolicy { get; set; }
            public string MileageLimit { get; set; }
            public string ImgS { get; set; }
            public string ImgB { get; set; }
            public string Price { get; set; }
            public string IntegralDailyPrice { get; set; }
            public string ScaleDailyPrice { get; set; }
            public string Sipp { get; set; }
            public string GroupId { get; set; }
            public string DropPrice { get; set; }
            public string BaggageLimit { get; set; }
            public string NumberOfDays { get; set; }
        }

        public class Xml
        {
            [JsonProperty("@version")]
            public string version { get; set; }

            [JsonProperty("@encoding")]
            public string encoding { get; set; }
        }


        public class SaveResponse
        {
            [JsonProperty("@xmlns")]
            public string xmlns { get; set; }
            public SaveResult SaveResult { get; set; }
        }

        public class SaveResult
        {
            public string Id { get; set; }
            public string No { get; set; }
            public string Email { get; set; }
            public string VehicleId { get; set; }
            public string ForeignCurrency { get; set; }
            public string Price { get; set; }
            public string DeliveryLocationId { get; set; }
            public string ReturnLocationId { get; set; }
            public string DeliveryDateTime { get; set; }
            public string ReturnDateTime { get; set; }
            public string NumberOfDays { get; set; }
            public object Extras { get; set; }
            public string DeliverToAddress { get; set; }
            public string ReturnFromAddress { get; set; }
            public string BillToCompany { get; set; }
            public string ProviderId { get; set; }
        }

        public class CancelServiceResult
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
        }


        public class CancelServiceResponse
        {
            public CancelServiceResult CancelServiceResult { get; set; }
            public string xmlns { get; set; }
            public string text { get; set; }
        }

        public class StartChargeResult
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
        }

        public class StartChargeResponse
        {
            public StartChargeResult StartChargeResult { get; set; }
            public string xmlns { get; set; }
            public string text { get; set; }
        }


    }
}
