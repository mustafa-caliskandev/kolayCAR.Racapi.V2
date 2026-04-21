using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class Yolcu360RequestBase
    {
        public class EmailLoginParameters
        {
            public string email { get; set; }
            public string password { get; set; }
        }
        public class CarListingAgencyRequestBody
        {
            public int pickupLocationId { get; set; }
            public string pickupLocationSlug { get; set; }
            public int dropoffLocationId { get; set; }
            public string dropoffLocationSlug { get; set; }
            public string pickupDatetime { get; set; }
            public string dropoffDatetime { get; set; }
            public int requestedCommissionType { get; set; }
            public int requestedCommissionAmount { get; set; }
        }
        public class ReservationRequestBody
        {
            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
            public class User
            {
                public string firstName { get; set; }
                public string lastName { get; set; }
                public string email { get; set; }
                public string identityNumber { get; set; }
                public string passportNo { get; set; }
                public string birthDate { get; set; }
                public int purchaseCount { get; set; }
            }

            public class Phone
            {
                public string number { get; set; }
                public string country { get; set; }
            }

            public class Address
            {
                public string line { get; set; }
                public string city { get; set; }
                public string country { get; set; }
                public int zip { get; set; }
            }

            public class ContactInformation
            {
                public Phone phone { get; set; }
                public Address address { get; set; }
            }

            public class Renter
            {
                public User user { get; set; }
                public ContactInformation contactInformation { get; set; }
            }

            public class Product
            {
                public string productType { get; set; }
                public string label { get; set; }
                public int price { get; set; }
                public int maxAmount { get; set; }
                //public string damageInsuranceType { get; set; }
                //public string damageInsuranceCategory { get; set; }
                public int extraRangeAmount { get; set; }
            }

            public class OrderedProduct
            {
                public int count { get; set; }
                public Product product { get; set; }
            }

            public class CreditCard
            {
                public string number { get; set; }
                public int expirationMonth { get; set; }
                public int expirationYear { get; set; }
                public string holderName { get; set; }
                public int cvc { get; set; }
            }

            public class PaymentCard
            {
                public string cardOption { get; set; }
                public string savedCardLabel { get; set; }
                public CreditCard creditCard { get; set; }
            }

            [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
            public class Payment
            {
                public bool useThreeDS { get; set; }
                public string threedsCompletionScheme { get; set; }
                public int installmentCount { get; set; }
                public PaymentCard paymentCard { get; set; }
            }

            public class OrderRequest
            {
                public string carRentalSearchId { get; set; }
                public string listingId { get; set; }
                public string referenceID { get; set; }
                public Renter renter { get; set; }
                public List<OrderedProduct> orderedProducts { get; set; }
                public bool isFullCredit { get; set; }
                [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
                public Payment payment { get; set; }
            }
        }
        public class ReservationCancelRequestBody
        {
            public class RentalOrderParameters
            {
                public string carRentalOrderId { get; set; }
                public string email { get; set; }
                public string key { get; set; }
            }
        }
    }
}
