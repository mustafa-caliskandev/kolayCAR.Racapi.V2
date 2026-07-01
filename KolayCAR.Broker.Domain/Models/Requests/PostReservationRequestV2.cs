using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class PostReservationRequestV2
    {
        public Customer Customer { get; set; }
        public Payment Payment { get; set; }
        public Company Company { get; set; }
        public Agency Agency { get; set; }
        public Pricing Pricing { get; set; }
        public string VehicleName { get; set; }
        public string VehicleImageURL { get; set; }
        public string FlightNumberArrival { get; set; }
        public string FlightNumberDeparture { get; set; }   
        public string UpdateReservationNumber { get; set; }  
        public bool SendReservationMail { get; set; }
        public string DepartureInfo { get; set; }
        public string AgencyReservationReference { get; set; }      
        public bool IsSpecialWebSiteAgency { get; set; }
        public string SkyscannerRedirectID { get; set; }
        public bool CommercialAllowance { get; set; }
        public int? ReservationSourceId { get; set; }    
        public bool HighAmountDiscountActive { get; set; }
        public bool? FullCredit { get; set; }
        [JsonConverter(typeof(CreditTypeCodeJsonConverter))]
        [Newtonsoft.Json.JsonConverter(typeof(CreditTypeNewtonsoftJsonConverter))]
        public CreditType CreditType { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string OrderNumber { get; set; }
        public bool IsRetry { get; set; }
        public bool? ContactPermission { get; set; }    
        public string CountryCode { get; set; }     
        public string RequestId { get; set; }
        public string ExternalCreditCardInfo { get; set; }
        public bool SendAgencyReservationNumber { get; set; }       
        public string CurrencyCode { get; set; }
        public int PickupLocationId { get; set; }
        public int ReturnLocationId { get; set; }
        public string PickupDate { get; set; }
        public string ReturnDate { get; set; }
        public string PickupTime { get; set; }
        public string ReturnTime { get; set; }
        public string UserToken { get; set; }
        public string CouponCode { get; set; }
        public int? MemberId { get; set; }
        public bool IsReservationRequest { get; set; }
        public string SessionCode { get; set; }
        public string ApiLocationCode { get; set; }
        public string LanguageCode { get; set; }
        [JsonConverter(typeof(PostReservationExtraListJsonConverter))]
        public List<Extra> Extras { get; set; }
        private string reservationToken;
        public string ReservationToken { get => reservationToken; set => reservationToken = value?.Replace(" ", "+"); }
    }
}
