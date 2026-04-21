using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Responses.RentGo;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Kolaycar.Broker.Api.Mappers.RentGo
{
    public static class ExtraMapper
    {
        public static List<Extra> MapProducts(this List<RentGoOfficeAdditionalProduct> products)
        {
            if (products == null) return new List<Extra>();

            return products.Select(p => new Extra
            {
                ExtraCode = p.AdditionalProductId,
                ExtraName = (p.Translations?.Name != null && p.Translations.Name.ContainsKey("tr")) ? p.Translations.Name["tr"] : p.Name,
                ExtraDescription = (p.Translations?.Description != null && p.Translations.Description.ContainsKey("tr")) ? p.Translations.Description["tr"] : p.Description,
                Price = (float)p.Price,
                ExtraRentalType = p.PriceCalculationType == "DEPENDED_ON_DURATION" ? ExtraRentalTypes.Daily : ExtraRentalTypes.PerRental
            }).ToList();
        }

        public static List<Extra> MapPackages(this List<RentGoOfficeAdditionalPackage> packages)
        {
            if (packages == null) return new List<Extra>();

            var extras = new List<Extra>();
            foreach (var package in packages)
            {
                if (package.AdditionalProducts != null)
                {
                    extras.AddRange(package.AdditionalProducts.Select(sp => new Extra
                    {
                        ExtraCode = sp.AdditionalProductId, // İstenirse PackageId eklenebilir ancak model bazlı ürün Id tutuluyordu
                        ExtraName = $"{(string.IsNullOrWhiteSpace(package.PackageName) ? "Paket" : package.PackageName)} - {((sp.Translations?.Name != null && sp.Translations.Name.ContainsKey("tr")) ? sp.Translations.Name["tr"] : sp.Name)}",
                        ExtraDescription = (sp.Translations?.Description != null && sp.Translations.Description.ContainsKey("tr")) ? sp.Translations.Description["tr"] : sp.Description,
                        Price = (float)sp.Price,
                        ExtraRentalType = sp.PriceCalculationType == "DEPENDED_ON_DURATION" ? ExtraRentalTypes.Daily : ExtraRentalTypes.PerRental
                    }));
                }
            }
            return extras;
        }
        public static List<Extra> MapProducts(this List<RentGoProduct> products)
        {
            if (products == null) return new List<Extra>();

            return products.Select(p => new Extra
            {
                ExtraCode = p.Id,
                ExtraName = p.Name,
                ExtraDescription = p.Info,
                Price = p.Price.ToFloatNullSafe(),
                ExtraRentalType = p.Type == 1 ? ExtraRentalTypes.PerRental : ExtraRentalTypes.Daily
            }).ToList();
        }

        public static List<Extra> MapPackages(this List<RentGoPackage> packages)
        {
            if (packages == null) return new List<Extra>();

            var extras = new List<Extra>();
            foreach (var package in packages)
            {
                if (package.SubPackages != null)
                {
                    extras.AddRange(package.SubPackages.Select(sp => new Extra
                    {
                        ExtraCode = sp.Id,
                        ExtraName = $"{package.PackageName} - {sp.Name}",
                        ExtraDescription = sp.Content,
                        Price = sp.Price.ToFloatNullSafe(),
                        ExtraRentalType = ExtraRentalTypes.Daily
                    }));
                }
            }
            return extras;
        }
    }
}
