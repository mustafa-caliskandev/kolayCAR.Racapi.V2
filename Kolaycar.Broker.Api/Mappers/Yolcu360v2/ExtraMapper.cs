using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response.Yolcu360v2;
using System;
using System.Collections.Generic;
using System.Linq;
using static KolayCAR.Broker.Domain.Models.Response.Yolcu360v2.Yolcu360v2ResponseBaseDeleted;

namespace KolayCAR.Broker.API.Mappers.Yolcu360v2
{
    public static class ExtraMapper
    {
        public static List<Extra> Map(this List<Yolcu360v2ExtrasResponse> extras, int rentalDuration = 0)
        {
            return extras.Select(extra =>
            {
                bool isInsurance = extra.type == "insurance";
                float basePrice = extra.pricing.net.amount / 100f;
                float price = isInsurance && rentalDuration > 0 ? basePrice / rentalDuration : basePrice;

                return new Extra
                {
                    ExtraCode = isInsurance ? $"{GetInitials(extra.name)}-{extra.type}"
                                            : extra.type,
                    ExtraName = extra.name,
                    ExtraRentalType = isInsurance ? ExtraRentalTypes.Daily
                                                  : ExtraRentalTypes.PerRental,
                    Price = price,
                    ApiExtraCode = extra.code
                };
            }).ToList();
        }
        public static List<Extra> Map(this List<Yolcu360v2ExtraResponseBase.Root> extras, int rentalDuration = 0)
        {
            return extras?.Select(extra =>
            {
                bool isInsurance = extra.type == "insurance";
                float basePrice = extra.pricing.net.amount / 100f;
                float price = isInsurance && rentalDuration > 0 ? basePrice / rentalDuration : basePrice;

                return new Extra
                {
                    ExtraCode = isInsurance ? $"{GetInitials(extra.name)}-{extra.type}"
                                           : extra.type,
                    ExtraName = extra.name,
                    ExtraRentalType = isInsurance ? ExtraRentalTypes.Daily
                                                  : ExtraRentalTypes.PerRental,
                    Price = price,
                    ApiExtraCode = extra.code
                };
            }).ToList() ?? new List<Extra>();


        }
        //public static List<Extra> Map(this List<Yolcu360v2ExtrasResponse> extras, int rentalDuration = 0)
        //{
        //    int insuranceIndex = 0;

        //    return extras.Select(extra =>
        //    {
        //        bool isInsurance = extra.type == "insurance";
        //        float basePrice = extra.pricing.net.amount / 100f;
        //        float price = isInsurance && rentalDuration > 0 ? basePrice / rentalDuration : basePrice;

        //        ExtraRentalTypes rentalType = ExtraRentalTypes.PerRental;

        //        if (isInsurance)
        //        {
        //            insuranceIndex++;
        //            if (insuranceIndex == 1)
        //                rentalType = ExtraRentalTypes.Daily; // İlk insurance ürünü günlük
        //        }

        //        return new Extra
        //        {
        //            ExtraCode = isInsurance ? $"{GetInitials(extra.name)}-{extra.type}"
        //                                    : extra.type,
        //            ExtraName = extra.name,
        //            ExtraRentalType = rentalType,
        //            Price = price,
        //            apiExtraCode = extra.code
        //        };
        //    }).ToList();
        //}
        public static string GetInitials(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return string.Concat(text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(word => char.ToUpper(word[0])));
        }
    }
}