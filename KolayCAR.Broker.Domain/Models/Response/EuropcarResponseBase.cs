using KolayCAR.Broker.Domain.Models.Response;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class option
    {
        [JsonProperty("@code")]
        public string code { get; set; }
        [JsonProperty("@name")]
        public string name { get; set; }
        [JsonProperty("@quant")]
        public string quant { get; set; }
        [JsonProperty("@rate")]
        public string rate { get; set; }
        [JsonProperty("@firstfree")]
        public string firstfree { get; set; }
    }
    public class optionList
    {
        [JsonProperty("code")]
        public string code { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("@quant")]
        public string quant { get; set; }
        [JsonProperty("@rate")]
        public string rate { get; set; }
        [JsonProperty("@firstfree")]
        public string firstfree { get; set; }
    }
    public class options
    {
        public List<option> option { get; set; }
        //public option option { get; set; }
    }

    public class category
    {
        public options options { get; set; }
        [JsonProperty("@availability")]
        public string availability { get; set; }
        [JsonProperty("@code")]
        public string code { get; set; }
        [JsonProperty("@model")]
        public string model { get; set; }
        [JsonProperty("@totalrate")]
        public string totalrate { get; set; }
        [JsonProperty("@baserate")]
        public string baserate { get; set; }
        [JsonProperty("@onewaycharge")]
        public string onewaycharge { get; set; }
        [JsonProperty("@outofhours")]
        public string outofhours { get; set; }
        [JsonProperty("@outofoffice")]
        public string outofoffice { get; set; }
        [JsonProperty("@excess")]
        public string excess { get; set; }
        [JsonProperty("@excessapplies")]
        public string excessapplies { get; set; }
        [JsonProperty("@klmincluded")]
        public string klmincluded { get; set; }
        [JsonProperty("@klmTotal")]
        public string klmTotal { get; set; }
        [JsonProperty("@unlimited")]
        public string unlimited { get; set; }
        [JsonProperty("@addklmrate")]
        public string addklmrate { get; set; }
        [JsonProperty("@provision")]
        public string provision { get; set; }
    }

    public class rates
    {
        public List<category> category { get; set; }
    }

    public class pricequote
    {
        public rates rates { get; set; }
        [JsonProperty("@id")]
        public string id { get; set; }
        [JsonProperty("@validto")]
        public string validto { get; set; }
        [JsonProperty("@duration")]
        public string duration { get; set; }
        [JsonProperty("@taxinclusive")]
        public string taxinclusive { get; set; }
        [JsonProperty("@currency")]
        public string currency { get; set; }
        [JsonProperty("@timetaken")]
        public string timetaken { get; set; }
        [JsonProperty("@graceperiod")]
        public string graceperiod { get; set; }
        public List<errors> errors { get; set; }
    }

    public class errors
    {
        public error error { get; set; }
    }

    public class error
    {
        [JsonProperty("@code")]
        public string code { get; set; }
        public string text { get; set; }
    }

    public class station
    {
        //[JsonProperty("@code")]
        //public string code { get; set; }
        //[JsonProperty("@name")]
        //public string name { get; set; }
        [JsonProperty("code")]
        public string code { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("mail")]
        public string mail { get; set; }
        [JsonProperty("phone")]
        public string phone { get; set; }
        [JsonProperty("lat")]
        public string lat { get; set; }
        [JsonProperty("long")]
        public string longLatitude { get; set; }

    }
}

public class reservation
{
    [JsonProperty("@irn")]
    public string irn { get; set; }
    [JsonProperty("@status")]
    public string status { get; set; }
    [JsonProperty("@refno")]
    public string refno { get; set; }
}

public class cargroup
{
    //[JsonProperty("@code")]
    //public string code { get; set; }
    //[JsonProperty("@model")]
    //public string model { get; set; }
    [JsonProperty("cargroup")]
    public string code { get; set; }
    [JsonProperty("model")]
    public string model { get; set; }
    [JsonProperty("gear")]
    public string gear { get; set; }
    [JsonProperty("driverMinYear")]
    public int driverMinYear { get; set; }
    [JsonProperty("licenseMinYear")]
    public int licenseMinYear { get; set; }
    [JsonProperty("category")]
    public string category { get; set; }
}

public class response
{
    public List<station> stations { get; set; }
    public List<optionList> options { get; set; }
    public reservation reservation { get; set; }
    public List<cargroup> groups { get; set; }
}

public class EuropcarResponseBase
{
    public pricequote pricequote { get; set; }
    public response response { get; set; }
}
public class EuropcarLocation
{
    public string requestStationCode { get; set; }
    public string stationName { get; set; }
    public string countryCode { get; set; }
    public string stationCountry { get; set; }
    public string stationCity { get; set; }
    public string stationTown { get; set; }
    public string stationStreet { get; set; }
    public string stationType { get; set; }
    public string longitude { get; set; }
    public string latitude { get; set; }
    public string stationEmail { get; set; }
    public string stationPhone { get; set; }
    public int minReservationTime { get; set; }
    public int deliveryTime { get; set; }
    public List<ShiftDayAndHour> shiftDayAndHours { get; set; }
}

public class EuropcarLocationResponse
{
    public string status { get; set; }
    public List<EuropcarLocation> data { get; set; }
}

public class ShiftDayAndHour
{
    public string dayOfWeek { get; set; }
    public string startHour { get; set; }
    public string endHour { get; set; }
    public bool isClosed { get; set; }
    public string localTime { get; set; }
    public bool is24Hours { get; set; }
    public string timeFormat { get; set; }
}
public class EuropcarExtra
{
    public string serviceCode { get; set; }
    public string serviceName { get; set; }
    public bool isRentableMoreThanOne { get; set; }
    public string limitType { get; set; }
    public string calculationType { get; set; }
}
public class EuropcarExtraResponse
{
    public string status { get; set; }
    public List<EuropcarExtra> data { get; set; }
}
public class EuropcarVehicle
{
    public string groupCode { get; set; }
    public string acrissCode { get; set; }
    public string groupName { get; set; }
    public string fuelType { get; set; }
    public string gearType { get; set; }
    public int minimumDriverAge { get; set; }
    public int maximumDriverAge { get; set; }
    public int minimumLicenseAge { get; set; }
    public int baggageCapacity { get; set; }
    public int numberOfDoors { get; set; }
    public string euroProvisionAmount { get; set; }
    public string tryProvisionAmount { get; set; }
}

public class EuropcarVehicleListResponse
{
    public string status { get; set; }
    public List<EuropcarVehicle> data { get; set; }
}
