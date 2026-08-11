using KolayCAR.Broker.Domain.Models.Requests.RentGo;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Responses.RentGo
{
    public class RentGoReservationResponse
    {
        [JsonProperty("resType")]
        public int ResType { get; set; }

        [JsonProperty("listId")]
        public string ListId { get; set; }

        [JsonProperty("versionId")]
        public string VersionId { get; set; }

        [JsonProperty("additionalProducts")]
        public List<string> AdditionalProducts { get; set; }

        [JsonProperty("additionalPackages")]
        public List<string> AdditionalPackages { get; set; }

        [JsonProperty("customerInfo")]
        public RentGoCustomerInfo CustomerInfo { get; set; }

        [JsonProperty("invoiceInfo")]
        public RentGoInvoiceInfo InvoiceInfo { get; set; }

        [JsonProperty("total")]
        public decimal? Total { get; set; }

        [JsonProperty("resId")]
        public string ReservationId { get; set; }

        [JsonProperty("pnr")]
        public string Pnr { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }
    }
}
