using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class GreenMotionRequest
    {
        [JsonProperty("@type")]
        public string _type { get; set; }
    }

    public class GreenMotionHeader
    {
        public GreenMotionRequest request { get; set; }
    }

    public class GreenMotionTotal
    {
        [JsonProperty("@currency")]
        public string _currency { get; set; }
        [JsonProperty("#text")]
        public string __text { get; set; }
    }

    public class GreenMotionFullCredit
    {
        [JsonProperty("@currency")]
        public string _currency { get; set; }
        [JsonProperty("#text")]
        public string __text { get; set; }
    }

    public class GreenMotionOption
    {
        public string optionID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Damage_excess { get; set; }
        public string Deposit { get; set; }
        public string Daily_rate { get; set; }
        public string Total_for_this_booking { get; set; }
        public string Prepay_available { get; set; }
        public string Prepay_daily_rate { get; set; }
        public string Prepay_total_for_this_booking { get; set; }
        public string Prepay2_available { get; set; }
    }

    public class GreenMotionInsuranceOptions
    {
        public List<GreenMotionOption> option { get; set; }
    }

    public class GreenMotionVehicle
    {
        public GreenMotionTotal total { get; set; }
        public GreenMotionFullCredit fullcredit { get; set; }
        public string groupName { get; set; }
        public string adults { get; set; }
        public string children { get; set; }
        public string luggageSmall { get; set; }
        public string luggageMed { get; set; }
        public string luggageLarge { get; set; }
        public string smallImage { get; set; }
        public string largeImage { get; set; }
        public string fuel { get; set; }
        public string mpg { get; set; }
        public string acriss { get; set; }
        public string co2 { get; set; }
        public string mileage { get; set; }
        public string excess { get; set; }
        public string deposit { get; set; }
        public string carorvan { get; set; }
        public string airConditioning { get; set; }
        public string transmission { get; set; }
        public string paymentURL { get; set; }
        public string driveandgo { get; set; }
        [JsonProperty("@name")]
        public string _name { get; set; }
        [JsonProperty("@id")]
        public string _id { get; set; }
        [JsonProperty("@image")]
        public string _image { get; set; }
        public GreenMotionInsuranceOptions insurance_options { get; set; }
    }

    public class GreenMotionVehicles
    {
        [JsonConverter(typeof(CustomArrayConverter<GreenMotionVehicle>))]
        public List<GreenMotionVehicle> vehicle { get; set; }
    }

    public class GreenMotionDailyRate
    {
        [JsonProperty("@image")]
        public string _currency { get; set; }
        [JsonProperty("#text")]
        public string __text { get; set; }
    }

    public class GreenMotionTotalForThisBooking
    {
        [JsonProperty("@image")]
        public string _currency { get; set; }
        [JsonProperty("#text")]
        public string __text { get; set; }
    }

    public class GreenMotionExtra
    {
        public string optionID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public GreenMotionDailyRate Daily_rate { get; set; }
        public string Category { get; set; }
        public GreenMotionTotalForThisBooking Total_for_this_booking { get; set; }
        public string Choices { get; set; }
        public string code { get; set; }
    }

    public class GreenMotionOptionalextras
    {

        //[JsonConverter(typeof(SingleOrArrayConverter<Extra>))]
        //public List<GreenMotionExtra> extra { get; set; }

        [JsonProperty("extra")]
        public JToken ExtraRaw { get; set; }

        [JsonIgnore]
        public List<GreenMotionExtra> Extras
        {
            get
            {
                if (ExtraRaw == null)
                    return new List<GreenMotionExtra>();

                if (ExtraRaw.Type == JTokenType.Array)
                    return ExtraRaw.ToObject<List<GreenMotionExtra>>();

                if (ExtraRaw.Type == JTokenType.Object)
                    return new List<GreenMotionExtra> { ExtraRaw.ToObject<GreenMotionExtra>() };

                return new List<GreenMotionExtra>();
            }
        }
    }

    public class GreenMotionDay
    {
        [JsonProperty("@name")]
        public string _name { get; set; }
        [JsonProperty("@is24hrs")]
        public string _is24hrs { get; set; }
        [JsonProperty("@open")]
        public string _open { get; set; }
        [JsonProperty("@close")]
        public string _close { get; set; }
    }

    public class GreenMotionOpeningHours
    {
        public List<GreenMotionDay> day { get; set; }
    }

    public class GreenMotionOfficeOpeningHours
    {
        public List<GreenMotionDay> day { get; set; }
    }

    public class GreenMotionOneway
    {
        public List<string> location_id { get; set; }
    }

    public class GreenMotionLocationInfo
    {
        public string location_name { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string address_3 { get; set; }
        public string address_city { get; set; }
        public string address_county { get; set; }
        public string address_postcode { get; set; }
        public string telephone { get; set; }
        public string fax { get; set; }
        public string email { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string iata { get; set; }
        public string wifi_ssid { get; set; }
        public string wifi_pw { get; set; }
        public string wifi_type { get; set; }
        public string four_products { get; set; }
        public GreenMotionOpeningHours opening_hours { get; set; }
        public GreenMotionOfficeOpeningHours office_opening_hours { get; set; }
        public string collectiondetails { get; set; }
        public string googleurl { get; set; }
        public string driveandgo { get; set; }
        public GreenMotionOneway oneway { get; set; }
    }

    public class GreenMotionServicearea
    {
        public string locationID { get; set; }
        public string name { get; set; }
    }

    public class GreenMotionResponse
    {
        [JsonConverter(typeof(CustomArrayConverter<GreenMotionServicearea>))]
        public List<GreenMotionServicearea> servicearea { get; set; }
        public GreenMotionLocationInfo location_info { get; set; }
        public string location { get; set; }
        public string days { get; set; }
        public GreenMotionVehicles vehicles { get; set; }
        public GreenMotionOptionalextras optionalextras { get; set; }
        public string quoteid { get; set; }
        public float oneway_fee { get; set; }
        public string booking_ref { get; set; }
        public string booking_notes { get; set; }
        [JsonProperty("@type")]
        public string _type { get; set; }
        public string full_credit { get; set; }
    }

    public class GreenMotionGetVehiclesResponse
    {
        public string location { get; set; }
        public string days { get; set; }
        public GreenMotionVehicles vehicles { get; set; }
        public GreenMotionOptionalextras optionalextras { get; set; }
        public string quoteid { get; set; }
        [JsonProperty("@type")]
        public string _type { get; set; }
    }

    public class GreenMotionGmWebservice
    {
        public GreenMotionHeader header { get; set; }
        public GreenMotionResponse response { get; set; }
    }

    public class GreenMotionResponseBase
    {
        public GreenMotionGmWebservice gm_webservice { get; set; }
    }
}
