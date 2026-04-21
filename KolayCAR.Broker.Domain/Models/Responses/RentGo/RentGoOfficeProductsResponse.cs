using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Responses.RentGo
{
    public class RentGoOfficeProductsResponse
    {
        [JsonProperty("additionalProducts")]
        public List<RentGoOfficeAdditionalProduct> AdditionalProducts { get; set; }

        [JsonProperty("additionalPackages")]
        public List<RentGoOfficeAdditionalPackage> AdditionalPackages { get; set; }
    }

    public class RentGoOfficeAdditionalProduct
    {
        [JsonProperty("additionalProductId")]
        public string AdditionalProductId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("translations")]
        public RentGoTranslations Translations { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("maximumPieces")]
        public int MaximumPieces { get; set; }

        [JsonProperty("priceCalculationType")]
        public string PriceCalculationType { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("maxPrice")]
        public decimal MaxPrice { get; set; }

        [JsonProperty("status")]
        public bool Status { get; set; }
    }

    public class RentGoOfficeAdditionalPackage
    {
        [JsonProperty("packageId")]
        public string PackageId { get; set; }

        [JsonProperty("packageName")]
        public string PackageName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("additionalProducts")]
        public List<RentGoOfficeAdditionalProduct> AdditionalProducts { get; set; }

        [JsonProperty("status")]
        public bool Status { get; set; }
    }

    public class RentGoTranslations
    {
        [JsonProperty("name")]
        public Dictionary<string, string> Name { get; set; }

        [JsonProperty("description")]
        public Dictionary<string, string> Description { get; set; }
    }
}
