

using DocumentFormat.OpenXml.Drawing.Diagrams;
using KolayCAR.Broker.API.Models;
using System.Collections.Generic;
using System.Linq;
using KolayCAR.Broker.Infrastructure.Extensions;

namespace KolayCAR.Broker.API.Mappers
{
    public static class ProfitMarkupMapper
    {

        public static Domain.Models.ProfitMarkup Map (this ProfitMarkup profitMarkup) =>
            profitMarkup != null ? new Domain.Models.ProfitMarkup
            {
                Id = profitMarkup.Id,
                ProfitMarkupVendors = profitMarkup.ProfitMarkupVendors.Select(v => new Domain.Models.ProfitMarkupVendor
                {
            
                    VendorId = v.VendorId
                    
                }).ToList(),
                ProfitMarkupAgencies = profitMarkup.ProfitMarkupAgencies.Select(a => new Domain.Models.ProfitMarkupAgency
                {
                    AgencyId = a.AgencyId,
                }).ToList(),
                ProfitMarkupLocations = profitMarkup.ProfitMarkupLocations.Select(l => new Domain.Models.ProfitMarkupLocation
                {
                    LocationId = l.LocationId,
                }).ToList(),
                CurrencyId = profitMarkup.CurrencyId.ToIntNullSafe(),
                AmountCurrencyId = profitMarkup.AmountCurrencyId.ToIntNullSafe(),
                EditDate = profitMarkup.EditDate.ToDateTimeNullSafe(),
                LastByUpdateUserId = profitMarkup.LastByUpdateUserId.ToIntNullSafe(),
                MarkupType = profitMarkup.MarkupType.ToIntNullSafe(),
                MarkupValue = profitMarkup.MarkupValue.ToFloatNullSafe(),
                MaximumAmount = profitMarkup.MaximumAmount.ToDecimalNullSafe(),
                MinimumAmount = profitMarkup.MinimumAmount.ToDecimalNullSafe(),
                MaximumDay = profitMarkup.MaximumDay.ToIntNullSafe(),
                MinimumDay = profitMarkup.MinimumDay.ToIntNullSafe(),
                Name = profitMarkup.Name.ToStringNullSafe(),
                PickupEndDate = profitMarkup.PickupEndDate.ToDateTimeNullSafe(),
                PickupStartDate = profitMarkup.PickupStartDate.ToDateTimeNullSafe(),
                Priority = profitMarkup.Priority.ToIntNullSafe(),
                ReservationEndDate = profitMarkup.ReservationEndDate.ToDateTimeNullSafe(),
                ReservationStartDate = profitMarkup.ReservationStartDate.ToDateTimeNullSafe(),
                Type = profitMarkup.Type.ToIntNullSafe()

            } : null;


        public static List<Domain.Models.ProfitMarkup> Map(this List<ProfitMarkup> profitMarkups)
        {
            var _profitMarkups = new List<Domain.Models.ProfitMarkup>();

            if (profitMarkups?.Count > 0)
                foreach (var profitMarkup in profitMarkups)
                    _profitMarkups.Add(profitMarkup.Map());

            return _profitMarkups;
            
        }
    }
}
