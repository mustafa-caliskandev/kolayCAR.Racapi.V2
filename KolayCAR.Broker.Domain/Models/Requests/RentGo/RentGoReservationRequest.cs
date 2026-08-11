using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Requests.RentGo
{
    public class RentGoReservationRequest
    {
        [JsonProperty("resType")]
        public int ResType { get; set; } = 1;

        [JsonProperty("listId")]
        public string ListId { get; set; }

        [JsonProperty("versionId")]
        public string VersionId { get; set; }

        [JsonProperty("createdAt")]
        public long CreatedAt { get; set; }

        [JsonProperty("additionalProducts")]
        public List<string> AdditionalProducts { get; set; }
        [JsonProperty("additionalPackages")]
        public List<string> AdditionalPackages { get; set; }
        [JsonProperty("customerInfo")]
        public RentGoCustomerInfo CustomerInfo { get; set; }

        [JsonProperty("invoiceInfo")]
        public RentGoInvoiceInfo InvoiceInfo { get; set; }

        //[JsonProperty("items")]
        //public List<RentGoReservationItem> Items { get; set; }

        //[JsonProperty("total")]
        //public decimal Total { get; set; }

        //[JsonProperty("totalDiscount")]
        //public decimal TotalDiscount { get; set; }

        //[JsonProperty("paymentType")]
        //public int PaymentType { get; set; }
    }

    public class RentGoCustomerInfo
    {
        [JsonProperty("governmentId")]
        public string GovernmentId { get; set; }

        [JsonProperty("passportNumber")]
        public string PassportNumber { get; set; }

        [JsonProperty("passportExpiryDate")]
        public string PassportExpiryDate { get; set; }

        [JsonProperty("customerType")]
        public int CustomerType { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; }

        [JsonProperty("gender")]
        public int Gender { get; set; }

        [JsonProperty("dialCode")]
        public string DialCode { get; set; }

        [JsonProperty("isTurkish")]
        public bool IsTurkish { get; set; }

        [JsonProperty("countryId")]
        public string CountryId { get; set; }

        [JsonProperty("birthDate")]
        public string BirthDate { get; set; }

        [JsonProperty("licenseNumber")]
        public string LicenseNumber { get; set; }

        [JsonProperty("licenseIssuedPlace")]
        public string LicenseIssuedPlace { get; set; }

        [JsonProperty("licenseIssueDate")]
        public string LicenseIssueDate { get; set; }

        [JsonProperty("licenseExpiredDate")]
        public string LicenseExpiredDate { get; set; }

        [JsonProperty("licenseClass")]
        public int LicenseClass { get; set; }
    }

    public class RentGoInvoiceInfo
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("surname")]
        public string Surname { get; set; }

        [JsonProperty("governmentId")]
        public string GovernmentId { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("taxNo")]
        public string TaxNo { get; set; }

        [JsonProperty("taxOffice")]
        public string TaxOffice { get; set; }

        [JsonProperty("addressLine")]
        public string AddressLine { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("district")]
        public string District { get; set; }
    }


    public class RentGoCancelReservationRequest
    {
        [JsonProperty("description")]
        public string Description { get; set; }
    }
    public class RentGoReservationItem
    {
        [JsonProperty("category")]
        public int Category { get; set; }

        [JsonProperty("refId")]
        public string RefId { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("day")]
        public int Day { get; set; }

        [JsonProperty("unitPrice")]
        public decimal UnitPrice { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("maxQuantity")]
        public int MaxQuantity { get; set; }

        [JsonProperty("maxPrice")]
        public decimal MaxPrice { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("invoiceId")]
        public string InvoiceId { get; set; }

        [JsonProperty("paymentId")]
        public string PaymentId { get; set; }

        [JsonProperty("refundId")]
        public string RefundId { get; set; }

        [JsonProperty("isForced")]
        public bool IsForced { get; set; }

        [JsonProperty("blockage")]
        public bool Blockage { get; set; }
    }
}

