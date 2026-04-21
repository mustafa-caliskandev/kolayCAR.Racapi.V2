using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Responses.RentGo
{
    public class RentGoListingResponse
    {
        [JsonProperty("listId")]
        public string ListId { get; set; }

        [JsonProperty("expireTime")]
        public long ExpireTime { get; set; }

        [JsonProperty("currency")]
        public int Currency { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("day")]
        public int Day { get; set; }

        [JsonProperty("hour")]
        public int Hour { get; set; }

        [JsonProperty("campaignInfo")]
        public object CampaignInfo { get; set; }

        [JsonProperty("oneWay")]
        public float OneWay { get; set; }

        [JsonProperty("oneWayId")]
        public string OneWayId { get; set; }

        [JsonProperty("exchange")]
        public RentGoExchangeInfo Exchange { get; set; }

        [JsonProperty("list")]
        public List<RentGoVehicleListingItem> List { get; set; }
    }

    public class RentGoExchangeInfo
    {
        [JsonProperty("dolar")]
        public decimal Dolar { get; set; }

        [JsonProperty("euro")]
        public decimal Euro { get; set; }
    }

    public class RentGoVehicleListingItem
    {
        [JsonProperty("photo")]
        public string Photo { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("vehicleGroup")]
        public RentGoVehicleGroup VehicleGroup { get; set; }

        [JsonProperty("segment")]
        public RentGoSegment Segment { get; set; }

        [JsonProperty("version")]
        public RentGoVersionInfo Version { get; set; }

        [JsonProperty("payOffice")]
        public decimal PayOffice { get; set; }

        [JsonProperty("payNow")]
        public decimal PayNow { get; set; }

        [JsonProperty("commissionTable")]
        public List<RentGoCommissionItem> CommissionTable { get; set; }

        [JsonProperty("vehicle")]
        public RentGoVehicleDetail Vehicle { get; set; }

        [JsonProperty("upgradable")]
        public List<string> Upgradable { get; set; }
    }

    public class RentGoBasicInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class RentGoSegment
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public class RentGoVehicleGroup
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("minAge")]
        public int MinAge { get; set; }

        [JsonProperty("minExp")]
        public int MinExp { get; set; }

        [JsonProperty("maxDailyKm")]
        public int MaxDailyKm { get; set; }

        [JsonProperty("deposit")]
        public decimal Deposit { get; set; }

        [JsonProperty("payNow")]
        public decimal PayNow { get; set; }

        [JsonProperty("overkm")]
        public decimal Overkm { get; set; }

        [JsonProperty("isUpgradable")]
        public bool IsUpgradable { get; set; }
    }

    public class RentGoVersionInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("sipp")]
        public string Sipp { get; set; }

        [JsonProperty("transmission")]
        public string Transmission { get; set; }

        [JsonProperty("bodyType")]
        public string BodyType { get; set; }

        [JsonProperty("fuel")]
        public string Fuel { get; set; }

        [JsonProperty("seat")]
        public string Seat { get; set; }

        [JsonProperty("luggage")]
        public int Luggage { get; set; }

        [JsonProperty("door")]
        public string Door { get; set; }

        [JsonProperty("engine")]
        public decimal Engine { get; set; }

        [JsonProperty("consumption")]
        public decimal Consumption { get; set; }

        [JsonProperty("emission")]
        public decimal Emission { get; set; }

        [JsonProperty("maxFuelBars")]
        public int MaxFuelBars { get; set; }

        [JsonProperty("brand")]
        public RentGoBrand Brand { get; set; }

        [JsonProperty("model")]
        public RentGoModel Model { get; set; }
    }



    public class RentGoCommissionItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("ratio")]
        public decimal Ratio { get; set; }
    }

    public class RentGoVehicleDetail
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("plate")]
        public string Plate { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("km")]
        public int Km { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("vehicleGroupId")]
        public string VehicleGroupId { get; set; }

        [JsonProperty("officeId")]
        public string OfficeId { get; set; }

        [JsonProperty("currentOfficeId")]
        public string CurrentOfficeId { get; set; }

        [JsonProperty("refId")]
        public string RefId { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("oldPlate")]
        public string OldPlate { get; set; }

        [JsonProperty("sasi")]
        public string Sasi { get; set; }

        [JsonProperty("engineNo")]
        public string EngineNo { get; set; }

        [JsonProperty("registrationNo")]
        public string RegistrationNo { get; set; }

        [JsonProperty("lastInspectionAt")]
        public string LastInspectionAt { get; set; }

        [JsonProperty("vehicleStatus")]
        public string VehicleStatus { get; set; }

        [JsonProperty("status")]
        public bool Status { get; set; }

        [JsonProperty("tireType")]
        public string TireType { get; set; }

        [JsonProperty("fuelBarCount")]
        public int FuelBarCount { get; set; }

        [JsonProperty("firstRegistrationAt")]
        public string FirstRegistrationAt { get; set; }

        [JsonProperty("tireSize")]
        public string TireSize { get; set; }

        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public string UpdatedAt { get; set; }
    }
}
