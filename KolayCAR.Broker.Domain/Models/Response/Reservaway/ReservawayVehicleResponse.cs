using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response.Reservaway;

public class ReservawaySearchResponse : ReservawayResponseBase
{
    public List<JToken> vehicles { get; set; }
}

public class ReservawayFilteredVehicleResponse : ReservawayResponseBase
{
    public List<ReservawayFilteredVehicleItem> vehicles { get; set; }
    public int expiresInMinutes { get; set; }
}

public class ReservawayVehicleSearchResponse : ReservawayResponseBase
{
    public List<ReservawayVehicleDetail> vehicles { get; set; }
}

public class ReservawayVehicleResponse : ReservawayResponseBase
{
    public List<ReservawayVehicleDetail> vehicle { get; set; }
    public int expiresInMinutes { get; set; }
}

public class ReservawayTypeResponse : ReservawayResponseBase
{
    public List<ReservawayTypeItem> types { get; set; }
}

public class ReservawayTypeItem
{
    public int id { get; set; }
    public string key { get; set; }
    public string description { get; set; }
}

public class ReservawayFilteredVehicleItem
{
    public string id { get; set; }
    public bool isHighlighted { get; set; }
    public string currency { get; set; }
    public List<string> filterTags { get; set; }
    public string name { get; set; }
    public string imgUrl { get; set; }
    public int seats { get; set; }
    public string fuelType { get; set; }
    public string transmission { get; set; }
    public ReservawayVendorSummary vendor { get; set; }
    public ReservawayLocationSummary pickupLocation { get; set; }
    public ReservawayVehicleOptions options { get; set; }
    public ReservawayPlan basePlan { get; set; }
    public ReservawayPlan fullyInclusivePlan { get; set; }
    public ReservawayPrices prices { get; set; }
    public int? luggageCapacity { get; set; }
}

public class ReservawayVehicleDetail
{
    public string id { get; set; }
    public string search_hash { get; set; }
    public int vendor_id { get; set; }
    public string vehicle_code { get; set; }
    public List<ReservawayPaymentType> active_payment_types { get; set; }
    public string daily_k_m_limit { get; set; }
    public string vehicle_model_name { get; set; }
    public string vehicle_brand_name { get; set; }
    public string pickup_date_time { get; set; }
    public string return_date_time { get; set; }
    public bool is_available { get; set; }
    public string reservation_token { get; set; }
    public bool is_office { get; set; }
    public bool is_airport { get; set; }
    public string total_k_m_limit { get; set; }
    public string vehicle_name { get; set; }
    public string sipp_code { get; set; }
    public List<ReservawayVehicleImage> vehicle_images { get; set; }
    public int kolay_id { get; set; }
    public string provider { get; set; }
    public string visitor_id { get; set; }
    public int? vendor_minimum_driver_age { get; set; }
    public int? driver_age { get; set; }
    public ReservawayPrices prices { get; set; }
    public List<ReservawayVehicleExtra> extras { get; set; }
    public int fuel_type_id { get; set; }
    public int passenger_quantity_type_id { get; set; }
    public int? baggage_quantity_type_id { get; set; }
    public int vehicle_category_id { get; set; }
    public int? transmission_type_id { get; set; }
    public int vehicle_type_id { get; set; }
    public bool has_air_condition { get; set; }
    public int? pickup_location_id { get; set; }
    public int? dropoff_location_id { get; set; }
    public int? delivery_type_id { get; set; }
    public string cheapest_price { get; set; }
    public string min_price_bst { get; set; }
    public string min_price_std { get; set; }
    public string base_currency { get; set; }
    public string rate { get; set; }
    public int? country_of_residence_id { get; set; }
    public int rental_duration { get; set; }
    public string fuel_type { get; set; }
    public string passenger_quantity_type { get; set; }
    public string baggage_quantity_type { get; set; }
    public string vehicle_category_type { get; set; }
    public string transmission_type { get; set; }
    public string vehicle_type { get; set; }
    public string delivery_type { get; set; }
    public string pickup_location { get; set; }
    public string dropoff_location { get; set; }
    public ReservawayVendorDetail vendor { get; set; }
    public ReservawayCountry country_of_residence { get; set; }
    public int seats { get; set; }
    public List<string> available_product_types { get; set; }
    public List<string> available_payment_types { get; set; }
    public ReservawayTotals extras_prices { get; set; }
    public string deep_link { get; set; }
    public string deep_link_vehicle_id { get; set; }
    public int expires_in_minutes { get; set; }
}

public class ReservawayVendorSummary
{
    public string id { get; set; }
    public string name { get; set; }
    public string logoUrl { get; set; }
    public float rating { get; set; }
}

public class ReservawayVendorDetail
{
    public int id { get; set; }
    public string name { get; set; }
    public string kolay_id { get; set; }
    public string kolay_vendor_type { get; set; }
    public string logo { get; set; }
    public JToken terms_and_conditions { get; set; }
    public string full_name { get; set; }
    public string checkin_url { get; set; }
    public bool is_debit_card_accepted { get; set; }
    public string default_currency { get; set; }
}

public class ReservawayLocationSummary
{
    public string name { get; set; }
}

public class ReservawayVehicleOptions
{
    public string fuelPolicy { get; set; }
    public float? oneWayFee { get; set; }
    public float? youngDriverFee { get; set; }
    public string debitCardAccepted { get; set; }
    public string pickupType { get; set; }
    public ReservawayMileageLimit mileageLimit { get; set; }
}

public class ReservawayMileageLimit
{
    public string type { get; set; }
    public int? daily { get; set; }
    public int? total { get; set; }
    public string unit { get; set; }
}

public class ReservawayPlan
{
    public string rateCode { get; set; }
    public float price { get; set; }
    public float retailPrice { get; set; }
    public float deposit { get; set; }
    public float liability { get; set; }
    public List<string> inclusions { get; set; }
}

public class ReservawayPrices
{
    public ReservawayPriceDefinition price_definition { get; set; }
    public Dictionary<string, Dictionary<string, ReservawayRatePrice>> rate_codes { get; set; }
}

public class ReservawayPriceDefinition
{
    public ReservawayPlanDefinition cheapest_base_plan { get; set; }
    public ReservawayPlanDefinition cheapest_inclusive_plan { get; set; }
    public string currency { get; set; }
    public float extra_price { get; set; }
    public float one_way_fee { get; set; }
    public float deposit_price { get; set; }
    public float original_price { get; set; }
    public bool? is_one_way_fee_poa { get; set; }
    public float daily_price_pay_now { get; set; }
    public float total_price_pay_now { get; set; }
    public bool? is_additional_product_price_poa { get; set; }
    public ReservawayCheapestDefinition cheapest { get; set; }
}

public class ReservawayCheapestDefinition
{
    public float daily_price { get; set; }
    public string rate_code { get; set; }
    public string payment_type { get; set; }
    public float retail_price { get; set; }
    public float multiplication { get; set; }
}

public class ReservawayPlanDefinition
{
    public string product_type_name { get; set; }
    public string payment_type_name { get; set; }
    public int product_type_id { get; set; }
    public int payment_type_id { get; set; }
}

public class ReservawayRatePrice
{
    public string rate_code { get; set; }
    public float daily_cost { get; set; }
    public float excess_fee { get; set; }
    public float daily_price { get; set; }
    public float total_price { get; set; }
    public float retail_price { get; set; }
    public float deposit_price { get; set; }
    public float payable_today { get; set; }
    public float pay_on_arrival { get; set; }
    public float discount_amount { get; set; }
    public string reservation_token { get; set; }
    public float discount_percentage { get; set; }
    public string rate_code_payment_type { get; set; }
    public bool is_cheapest { get; set; }
    public float daily_price_addition { get; set; }
    public float multiplication { get; set; }
}

public class ReservawayPaymentType
{
    public int id { get; set; }
    public string key { get; set; }
}

public class ReservawayVehicleImage
{
    public string url { get; set; }
}

public class ReservawayVehicleExtra
{
    public int extra_id { get; set; }
    public string extra_code { get; set; }
    public string extra_name { get; set; }
    public string extra_description { get; set; }
    public int extra_rental_type { get; set; }
    public int extra_type { get; set; }
    public bool extra_quantity_increasable { get; set; }
    public float price { get; set; }
    public string currency_code { get; set; }
    public string label { get; set; }
    public string damage_insurance_category { get; set; }
}

public class ReservawayTotals
{
    public float total { get; set; }
    public float payable_today { get; set; }
    public float pay_on_arrival { get; set; }
}

public class ReservawayCountry
{
    public int id { get; set; }
    public int kolay_id { get; set; }
    public string name { get; set; }
    public string code { get; set; }
    public string currency { get; set; }
}
