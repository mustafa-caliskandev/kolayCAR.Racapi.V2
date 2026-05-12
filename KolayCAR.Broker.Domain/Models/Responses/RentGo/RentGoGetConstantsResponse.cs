using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Responses.RentGo
{
    public class RentGoGetConstantsResponse
    {
        [JsonProperty("offices")]
        public List<RentGoOffice> Offices { get; set; }

        [JsonProperty("additionalPackages")]
        public List<RentGoPackage> AdditionalPackages { get; set; }

        [JsonProperty("additionalProducts")]
        public List<RentGoProduct> AdditionalProducts { get; set; }

        [JsonProperty("brands")]
        public List<RentGoBrand> Brands { get; set; }

        [JsonProperty("models")]
        public List<RentGoModel> Models { get; set; }

        [JsonProperty("versions")]
        public List<RentGoVersion> Versions { get; set; }
    }

    public class RentGoOffice
    {
        [JsonProperty("branchId")]
        public string BranchId { get; set; }

        [JsonProperty("branchName")]
        public string BranchName { get; set; }

        [JsonProperty("cityName")]
        public string CityName { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("location")]
        public RentGoLocation Location { get; set; }
    }

    public class RentGoLocation
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("coordinates")]
        public List<double> Coordinates { get; set; }
    }

    public class RentGoPackage
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("packageName")]
        public string PackageName { get; set; }

        [JsonProperty("info")]
        public string Info { get; set; }

        [JsonProperty("scope")]
        public List<string> Scope { get; set; }

        [JsonProperty("subPackages")]
        public List<RentGoSubPackage> SubPackages { get; set; }
    }

    public class RentGoSubPackage
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("price")]
        public decimal? Price { get; set; }

        [JsonProperty("maxPrice")]
        public decimal? MaxPrice { get; set; }

        [JsonProperty("declarationLimit")]
        public decimal? DeclarationLimit { get; set; }
    }

    public class RentGoProduct
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("info")]
        public string Info { get; set; }

        [JsonProperty("maxQty")]
        public int MaxQty { get; set; }

        [JsonProperty("price")]
        public decimal? Price { get; set; }

        [JsonProperty("maxPrice")]
        public decimal? MaxPrice { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }
    }

    public class RentGoBrand
    {
        [JsonProperty("brandId")]
        public string BrandId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class RentGoModel
    {
        [JsonProperty("modelId")]
        public string ModelId { get; set; }

        [JsonProperty("brandId")]
        public string BrandId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }
    }
    public class RentGoVersion
    {
        [JsonProperty("versionId")]
        public string VersionId { get; set; }

        [JsonProperty("versionName")]
        public string VersionName { get; set; }

        [JsonProperty("modelId")]
        public string ModelId { get; set; }

        [JsonProperty("modelName")]
        public string ModelName { get; set; }

        [JsonProperty("brandId")]
        public string BrandId { get; set; }

        [JsonProperty("brandName")]
        public string BrandName { get; set; }

        [JsonProperty("transmissionType")]
        public string TransmissionType { get; set; }

        [JsonProperty("fuelType")]
        public string FuelType { get; set; }

        [JsonProperty("sipp")]
        public string Sipp { get; set; }
    }
}
