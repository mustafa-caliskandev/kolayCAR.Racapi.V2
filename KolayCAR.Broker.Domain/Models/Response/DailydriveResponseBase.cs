using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json;

namespace KolayCAR.Broker.Domain.Models.Response.Dailydrive
{
    #region Location
    public class Ns2Locations
    {
        [JsonProperty("ns2:locNo")]
        public string Ns2LocNo { get; set; }

        [JsonProperty("ns2:locCode")]
        public string Ns2LocCode { get; set; }

        [JsonProperty("ns2:locName")]
        public string Ns2LocName { get; set; }

        [JsonProperty("ns2:locAddress")]
        public string Ns2LocAddress { get; set; }

        [JsonProperty("ns2:locPhone")]
        public string Ns2LocPhone { get; set; }

        [JsonProperty("ns2:locMobilePhone")]
        public string Ns2LocMobilePhone { get; set; }

        [JsonProperty("ns2:locFax")]
        public string Ns2LocFax { get; set; }

        [JsonProperty("ns2:locEmail")]
        public string Ns2LocEmail { get; set; }

        [JsonProperty("ns2:countryCode")]
        public string Ns2CountryCode { get; set; }

        [JsonProperty("ns2:countryName")]
        public string Ns2CountryName { get; set; }

        [JsonProperty("ns2:cityName")]
        public string Ns2CityName { get; set; }

        [JsonProperty("ns2:districtName")]
        public string Ns2DistrictName { get; set; }

        [JsonProperty("ns2:locIata")]
        public string Ns2LocIata { get; set; }

        [JsonProperty("ns2:openingTimes")]
        public List<string> Ns2OpeningTimes { get; set; }

        [JsonProperty("ns2:closingTimes")]
        public List<string> Ns2ClosingTimes { get; set; }
    }

    public class Ns2LocationsResponse
    {
        [JsonProperty("@xmlns:ns2")]
        public string XmlnsNs2 { get; set; }

        [JsonProperty("ns2:locations")]
        public List<Ns2Locations> Ns2Locations { get; set; }
    }

    public class SOAPENVBody
    {
        [JsonProperty("ns2:LocationsResponse")]
        public Ns2LocationsResponse Ns2LocationsResponse { get; set; }

        [JsonProperty("ns2:CancelReservationResponse")]
        public Ns2CancelReservationResponse Ns2CancelReservationResponse { get; set; }

        [JsonProperty("ns2:SendBankTransactionResponse")]
        public Ns2SendBankTransactionResponse Ns2SendBankTransactionResponse { get; set; }
    }

    public class SOAPENVEnvelope
    {
        [JsonProperty("@xmlns:SOAP-ENV")]
        public string XmlnsSOAPENV { get; set; }

        [JsonProperty("SOAP-ENV:Header")]
        public object SOAPENVHeader { get; set; }

        [JsonProperty("SOAP-ENV:Body")]
        public SOAPENVBody SOAPENVBody { get; set; }
    }

    public class DailydriveLocationResponseBase
    {
        [JsonProperty("SOAP-ENV:Envelope")]
        public SOAPENVEnvelope SOAPENVEnvelope { get; set; }
    }
    #endregion

    #region Araç
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Ns2VehicleClasses
    {
        [JsonProperty("ns2:classNo")]
        public string Ns2ClassNo { get; set; }

        [JsonProperty("ns2:classCode")]
        public string Ns2ClassCode { get; set; }

        [JsonProperty("ns2:className")]
        public string Ns2ClassName { get; set; }

        [JsonProperty("ns2:classOrder")]
        public string Ns2ClassOrder { get; set; }

        [JsonProperty("ns2:classCodeAcriss")]
        public string Ns2ClassCodeAcriss { get; set; }

        [JsonProperty("ns2:minDriverAge")]
        public string Ns2MinDriverAge { get; set; }

        [JsonProperty("ns2:minDlicensePeriod")]
        public string Ns2MinDlicensePeriod { get; set; }

        [JsonProperty("ns2:minYdriverAge")]
        public string Ns2MinYdriverAge { get; set; }

        [JsonProperty("ns2:minYdlicensePeriod")]
        public string Ns2MinYdlicensePeriod { get; set; }
        [JsonProperty("ns2:vehicleTypeNos")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public List<string> VehicleTypeNos { get; set; }
        
    }

    public class Ns2VehicleClassesResponse
    {
        [JsonProperty("@xmlns:ns2")]
        public string XmlnsNs2 { get; set; }

        [JsonProperty("ns2:vehicleClasses")]
        public List<Ns2VehicleClasses> Ns2VehicleClasses { get; set; }
    }
    public class Ns2VehicleType
    {
        [JsonProperty("ns2:typeNo")]
        public string ns2typeNo { get; set; }

        [JsonProperty("ns2:typeFullname")]
        public string ns2typeFullname { get; set; }

        [JsonProperty("ns2:mark")]
        public string ns2mark { get; set; }

        [JsonProperty("ns2:model")]
        public string ns2model { get; set; }

        [JsonProperty("ns2:classNo")]
        public string ns2classNo { get; set; }

        [JsonProperty("ns2:classCode")]
        public string ns2classCode { get; set; }

        [JsonProperty("ns2:className")]
        public string ns2className { get; set; }

        [JsonProperty("ns2:body")]
        public string ns2body { get; set; }

        [JsonProperty("ns2:transmission")]
        public string ns2transmission { get; set; }

        [JsonProperty("ns2:ccm")]
        public string ns2ccm { get; set; }

        [JsonProperty("ns2:engineType")]
        public string ns2engineType { get; set; }

        [JsonProperty("ns2:hp")]
        public string ns2hp { get; set; }

        [JsonProperty("ns2:fuelType")]
        public string ns2fuelType { get; set; }

        [JsonProperty("ns2:fuelCapacity")]
        public string ns2fuelCapacity { get; set; }

        [JsonProperty("ns2:co2Emission")]
        public string ns2co2Emission { get; set; }

        [JsonProperty("ns2:doors")]
        public string ns2doors { get; set; }

        [JsonProperty("ns2:seats")]
        public string ns2seats { get; set; }

        [JsonProperty("ns2:luggageCapacity")]
        public string ns2luggageCapacity { get; set; }

        [JsonProperty("ns2:img1")]
        public string ns2img1 { get; set; }

        [JsonProperty("ns2:img2")]
        public string ns2img2 { get; set; }

        [JsonProperty("ns2:img3")]
        public object ns2img3 { get; set; }

        [JsonProperty("ns2:img4")]
        public object ns2img4 { get; set; }

        [JsonProperty("ns2:img5")]
        public string ns2img5 { get; set; }

        [JsonProperty("ns2:description")]
        public object ns2description { get; set; }

        [JsonProperty("ns2:note")]
        public object ns2note { get; set; }
    }
    public class Ns2VehicleTypesResponse
    {
        [JsonProperty("@xmlns:ns2")]
        public string XmlnsNs2 { get; set; }

        [JsonProperty("ns2:vehicleTypes")]
        public List<Ns2VehicleType> ns2vehicleTypes { get; set; }
    }
    public class SOAPENVBody_Vehicle
    {
        [JsonProperty("ns2:VehicleClassesResponse")]
        public Ns2VehicleClassesResponse Ns2VehicleClassesResponse { get; set; }
    }
    public class SOAPENVBody_VehicleList
    {
        [JsonProperty("ns2:VehicleTypesResponse")]
        public Ns2VehicleTypesResponse Ns2VehicleTypesResponse { get; set; }
    }

    public class SOAPENVEnvelope__Vehicle
    {
        [JsonProperty("@xmlns:SOAP-ENV")]
        public string XmlnsSOAPENV { get; set; }

        [JsonProperty("SOAP-ENV:Header")]
        public object SOAPENVHeader { get; set; }

        [JsonProperty("SOAP-ENV:Body")]
        public SOAPENVBody_Vehicle SOAPENVBody { get; set; }
    }

    public class SOAPENVEnvelope__VehicleList
    {
        [JsonProperty("@xmlns:SOAP-ENV")]
        public string XmlnsSOAPENV { get; set; }

        [JsonProperty("SOAP-ENV:Header")]
        public object SOAPENVHeader { get; set; }

        [JsonProperty("SOAP-ENV:Body")]
        public SOAPENVBody_VehicleList SOAPENVBody { get; set; }
    }
    public class DailydriveVehicleResponseBase
    {
        [JsonProperty("SOAP-ENV:Envelope")]
        public SOAPENVEnvelope__Vehicle SOAPENVEnvelope { get; set; }
    }
    public class DailydriveVehicleListResponseBase
    {
        [JsonProperty("SOAP-ENV:Envelope")]
        public SOAPENVEnvelope__VehicleList SOAPENVEnvelope { get; set; }
    }
    #endregion

    #region Rezervasyona Göre Araçlar

    public class DailydriveCapacitiesResponseBase
    {
        [JsonProperty("SOAP-ENV:Envelope")]
        public SOAPENVEnvelope__Capacities SOAPENVEnvelope { get; set; }
    }

    public class SOAPENVEnvelope__Capacities
    {
        [JsonProperty("@xmlns:SOAP-ENV")]
        public string XmlnsSOAPENV { get; set; }

        [JsonProperty("SOAP-ENV:Header")]
        public object SOAPENVHeader { get; set; }

        [JsonProperty("SOAP-ENV:Body")]
        public SOAPENVBody_Capacities SOAPENVBody { get; set; }
    }

    public class SOAPENVBody_Capacities
    {
        [JsonProperty("ns2:CapacitiesResponse")]
        public N2CapacitiesResponse N2CapacitiesResponse { get; set; }
    }

    public class N2CapacitiesResponse
    {
        [JsonProperty("@xmlns:ns2")]
        public string XmlnsNs2 { get; set; }

        [JsonProperty("ns2:capacities")]
        public List<Ns2CapacitiesClasses> Ns2CapacitiesClasses { get; set; }
    }

    public class Ns2CapacitiesClasses
    {
        [JsonProperty("ns2:available")]
        public bool Ns2Available { get; set; }

        [JsonProperty("ns2:pickupLocNo")]
        public int Ns2PickupLocNo { get; set; }

        [JsonProperty("ns2:pickupLocName")]
        public string Ns2PickupLocName { get; set; }

        [JsonProperty("ns2:returnLocNo")]
        public int Ns2ReturnLocNo { get; set; }

        [JsonProperty("ns2:returnLocName")]
        public string Ns2ReturnLocName { get; set; }

        [JsonProperty("ns2:classNo")]
        public int Ns2ClassNo { get; set; }

        [JsonProperty("ns2:classCode")]
        public string Ns2ClassCode { get; set; }

        [JsonProperty("ns2:className")]
        public string ns2ClassName { get; set; }

        [JsonProperty("ns2:classCodeAcriss")]
        public string Ns2ClassCodeAcriss { get; set; }

        [JsonProperty("ns2:minDriverAge")]
        public int Ns2MinDriverAge { get; set; }

        [JsonProperty("ns2:minDlicencePeriod")]
        public int Ns2MinDlicencePeriod { get; set; }

        [JsonProperty("ns2:minYdriverAge")]
        public int Ns2MinYdriverAge { get; set; }

        [JsonProperty("ns2:minYdlicencePeriod")]
        public int Ns2MinYdlicencePeriod { get; set; }

        [JsonProperty("ns2:pickupDate")]
        public string Ns2PickupDate { get; set; }

        [JsonProperty("ns2:returnDate")]
        public string Ns2ReturnDate { get; set; }

        [JsonProperty("ns2:pickupInsideOfficeHours")]
        public bool Ns2PickupInsideOfficeHours { get; set; }

        [JsonProperty("ns2:returnInsideOfficeHours")]
        public bool Ns2ReturnInsideOfficeHours { get; set; }

        [JsonProperty("ns2:rentalDays")]
        public int Ns2RentalDays { get; set; }
        [JsonProperty("ns2:kmLimitType")]
        public string Ns2KmLimitType { get; set; }


        [JsonProperty("ns2:kmLimit")]
        public int Ns2KmLimit { get; set; }

        [JsonProperty("ns2:extraHours")]
        public int Ns2ExtraHours { get; set; }

        [JsonProperty("ns2:campaignNo")]
        public int Ns2CampaignNo { get; set; }

        [JsonProperty("ns2:vehicleTypes")]
        public List<Ns2VehicleTypes> Ns2VehicleTypes { get; set; }

        [JsonProperty("ns2:tariffs")]
        public List<Ns2Tariffs> Ns2Tariffs { get; set; }

        [JsonProperty("ns2:onewayPrice")]
        public string Ns2OnewayPrice { get; set; }

        [JsonProperty("ns2:onewayCurrency")]
        public string Ns2OnewayCurrency { get; set; }
        [JsonProperty("ns2:provision")]
        public float Ns2Provision { get; set; }

    }

    public class Ns2VehicleTypes
    {

        [JsonProperty("ns2:typeNo")]
        public int Ns2TypeNo { get; set; }

        [JsonProperty("ns2:typeFullname")]
        public string Ns2TypeFullname { get; set; }

        [JsonProperty("ns2:mark")]
        public string Ns2Mark { get; set; }

        [JsonProperty("ns2:model")]
        public string Ns2Model { get; set; }

        [JsonProperty("ns2:body")]
        public string Ns2Body { get; set; }

        [JsonProperty("ns2:transmission")]
        public string Ns2Transmission { get; set; }

        [JsonProperty("ns2:description")]
        public string Ns2Description { get; set; }

        [JsonProperty("ns2:note")]
        public string Ns2Note { get; set; }

        [JsonProperty("ns2:img1")]
        public string Img1 { get; set; }

    }


    public class Ns2Tariffs
    {
        [JsonProperty("ns2:tariffNo")]
        public int Ns2TariffNo { get; set; }

        [JsonProperty("ns2:tariffCode")]
        public string Ns2TariffCode { get; set; }

        [JsonProperty("ns2:tariffName")]
        public string Ns2TariffName { get; set; }

        [JsonProperty("ns2:totalRentalPrice")]
        public decimal Ns2TotalRentalPrice { get; set; }

        [JsonProperty("ns2:hourLimitExceedPrice")]
        public decimal? Ns2HourLimitExceedPrice { get; set; }

        [JsonProperty("ns2:rentalPriceCurrency")]
        public string Ns2RentalPriceCurrency { get; set; }
    }


    #endregion

    #region Ekstra
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Ns2ExtraProducts
    {
        [JsonProperty("ns2:productNo")]
        public string Ns2ProductNo { get; set; }

        [JsonProperty("ns2:productName")]
        public string Ns2ProductName { get; set; }

        [JsonProperty("ns2:productDescription")]
        public string Ns2ProductDescription { get; set; }

        [JsonProperty("ns2:tariffNo")]
        public string Ns2TariffNo { get; set; }

        [JsonProperty("ns2:salesType")]
        public object Ns2SalesType { get; set; }

        [JsonProperty("ns2:unitPrice")]
        public string Ns2UnitPrice { get; set; }

        [JsonProperty("ns2:totalPrice")]
        public string Ns2TotalPrice { get; set; }

        [JsonProperty("ns2:currency")]
        public object Ns2Currency { get; set; }

        [JsonProperty("ns2:included")]
        public string Ns2Included { get; set; }

        [JsonProperty("ns2:selected")]
        public string Ns2Selected { get; set; }

        [JsonProperty("ns2:incremental")]
        public string Ns2Incremental { get; set; }

        [JsonProperty("ns2:calculateTax")]
        public string Ns2CalculateTax { get; set; }

        [JsonProperty("ns2:insurance")]
        public string Ns2Insurance { get; set; }

        [JsonProperty("ns2:order")]
        public string Ns2Order { get; set; }
    }

    public class Ns2ExtraProductsResponse
    {
        [JsonProperty("@xmlns:ns2")]
        public string XmlnsNs2 { get; set; }

        [JsonProperty("ns2:extraProducts")]
        public List<Ns2ExtraProducts> Ns2ExtraProducts { get; set; }
    }

    public class SOAPENVBody_Ekstra
    {
        [JsonProperty("ns2:ExtraProductsResponse")]
        public Ns2ExtraProductsResponse Ns2ExtraProductsResponse { get; set; }
    }

    public class SOAPENVEnvelope__Ekstra
    {
        [JsonProperty("@xmlns:SOAP-ENV")]
        public string XmlnsSOAPENV { get; set; }

        [JsonProperty("SOAP-ENV:Header")]
        public object SOAPENVHeader { get; set; }

        [JsonProperty("SOAP-ENV:Body")]
        public SOAPENVBody_Ekstra SOAPENVBody { get; set; }
    }

    public class DailydriveExtraResponseBase
    {
        [JsonProperty("SOAP-ENV:Envelope")]
        public SOAPENVEnvelope__Ekstra SOAPENVEnvelope { get; set; }
    }


    #endregion

    #region Rezervasyon Post 
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Ns2OperationResult
    {
        [JsonProperty("ns2:success")]
        public string Ns2Success { get; set; }

        [JsonProperty("ns2:resStatus")]
        public object Ns2ResStatus { get; set; }

        [JsonProperty("ns2:resNo")]
        public string Ns2ResNo { get; set; }

        [JsonProperty("ns2:resCorpNo")]
        public string Ns2ResCorpNo { get; set; }

        [JsonProperty("ns2:resAdNo")]
        public string Ns2ResAdNo { get; set; }

        [JsonProperty("ns2:message")]
        public string Ns2Message { get; set; }
    }

    public class Ns2InsertReservationResponse
    {
        [JsonProperty("@xmlns:ns2")]
        public string XmlnsNs2 { get; set; }

        [JsonProperty("ns2:operationResult")]
        public Ns2OperationResult Ns2OperationResult { get; set; }
    }

    public class ResPostSOAPENVBody
    {
        [JsonProperty("ns2:InsertReservationResponse")]
        public Ns2InsertReservationResponse Ns2InsertReservationResponse { get; set; }
    }

    public class ResPostSOAPENVEnvelope
    {
        [JsonProperty("@xmlns:SOAP-ENV")]
        public string XmlnsSOAPENV { get; set; }

        [JsonProperty("SOAP-ENV:Header")]
        public object SOAPENVHeader { get; set; }

        [JsonProperty("SOAP-ENV:Body")]
        public ResPostSOAPENVBody SOAPENVBody { get; set; }
    }

    public class ReservationResponseBody
    {
        [JsonProperty("SOAP-ENV:Envelope")]
        public ResPostSOAPENVEnvelope SOAPENVEnvelope { get; set; }
    }
    #endregion

    //#region Rezervasyon İptal
    //// using System.Xml.Serialization;
    //// XmlSerializer serializer = new XmlSerializer(typeof(Envelope));
    //// using (StringReader reader = new StringReader(xml))
    //// {
    ////    var test = (Envelope)serializer.Deserialize(reader);
    //// }

    //[XmlRoot(ElementName = "operationResult")]
    //public class OperationResult
    //{

    //    [XmlElement(ElementName = "success")]
    //    public bool Success { get; set; }

    //    [XmlElement(ElementName = "resStatus")]
    //    public object ResStatus { get; set; }

    //    [XmlElement(ElementName = "resNo")]
    //    public string ResNo { get; set; }

    //    [XmlElement(ElementName = "resCorpNo")]
    //    public string ResCorpNo { get; set; }

    //    [XmlElement(ElementName = "resAdNo")]
    //    public int ResAdNo { get; set; }

    //    [XmlElement(ElementName = "message")]
    //    public string Message { get; set; }
    //}

    //[XmlRoot(ElementName = "CancelReservationResponse")]
    //public class CancelReservationResponse
    //{

    //    [XmlElement(ElementName = "operationResult")]
    //    public OperationResult OperationResult { get; set; }

    //    [XmlAttribute(AttributeName = "ns2")]
    //    public string Ns2 { get; set; }

    //    [XmlText]
    //    public string Text { get; set; }
    //}

    //[XmlRoot(ElementName = "Body")]
    //public class Body
    //{

    //    [XmlElement(ElementName = "CancelReservationResponse")]
    //    public CancelReservationResponse CancelReservationResponse { get; set; }
    //}

    //[XmlRoot(ElementName = "Envelope")]
    //public class ReservationCancelResponseBody
    //{

    //    [XmlElement(ElementName = "Header")]
    //    public object Header { get; set; }

    //    [XmlElement(ElementName = "Body")]
    //    public Body Body { get; set; }

    //    [XmlAttribute(AttributeName = "SOAP-ENV")]
    //    public string SOAPENV { get; set; }

    //    [XmlText]
    //    public string Text { get; set; }
    //}
    //#endregion

    #region Reservation Cancel Response New
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class SOAPENVHeader
    {
        [JsonProperty("-self-closing")]
        public string SelfClosing { get; set; }
    }

    //public class Ns2OperationResult
    //{
    //    [JsonProperty("ns2:success")]
    //    public string Ns2Success { get; set; }

    //    [JsonProperty("ns2:resStatus")]
    //    public string Ns2ResStatus { get; set; }

    //    [JsonProperty("ns2:resNo")]
    //    public string Ns2ResNo { get; set; }

    //    [JsonProperty("ns2:resCorpNo")]
    //    public string Ns2ResCorpNo { get; set; }

    //    [JsonProperty("ns2:resAdNo")]
    //    public string Ns2ResAdNo { get; set; }

    //    [JsonProperty("ns2:message")]
    //    public string Ns2Message { get; set; }
    //}

    public class Ns2CancelReservationResponse
    {
        [JsonProperty("-xmlns:ns2")]
        public string XmlnsNs2 { get; set; }

        [JsonProperty("ns2:operationResult")]
        public Ns2OperationResult Ns2OperationResult { get; set; }
    }

    //public class SOAPENVBody
    //{
    //    [JsonProperty("ns2:CancelReservationResponse")]
    //    public Ns2CancelReservationResponse Ns2CancelReservationResponse { get; set; }
    //}

    //public class SOAPENVEnvelope
    //{
    //    [JsonProperty("-xmlns:SOAP-ENV")]
    //    public string XmlnsSOAPENV { get; set; }

    //    [JsonProperty("SOAP-ENV:Header")]
    //    public SOAPENVHeader SOAPENVHeader { get; set; }

    //    [JsonProperty("SOAP-ENV:Body")]
    //    public SOAPENVBody SOAPENVBody { get; set; }
    //}

    public class ReservationCancelResponseBody
    {
        [JsonProperty("SOAP-ENV:Envelope")]
        public SOAPENVEnvelope SOAPENVEnvelope { get; set; }

        [JsonProperty("#omit-xml-declaration")]
        public string OmitXmlDeclaration { get; set; }
    }


    #endregion

    public class Ns2SendBankTransactionResponse
    {
        [JsonProperty("@xmlns:ns2")]
        public string XmlnsNs2 { get; set; }

        [JsonProperty("ns2:operationResult")]
        public Ns2OperationResult Ns2OperationResult { get; set; }
    }

    public class SendBankTransactionResponse
    {
        [JsonProperty("SOAP-ENV:Envelope")]
        public SOAPENVEnvelope SOAPENVEnvelope { get; set; }
    }

}
