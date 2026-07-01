using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services;

public interface IAdditionalProductService
{
    Task<List<Additionalproduct>> GetAdditionalProductsByIdList(IEnumerable<int> idList, int langId);
    Task<List<Extra>> GetPremiumPackets(IEnumerable<SpecialRequest> specialRequests, ReservationToken token, int rentalDuration, int langId);
}
public class AdditionalProductService : IAdditionalProductService
{
    private readonly IAdditionalProductRepository _additionalProductRepository;
    private readonly ICurrencyService _currencyService;
    public AdditionalProductService(IAdditionalProductRepository additionalProductRepository, ICurrencyService currencyService)
    {
        _additionalProductRepository = additionalProductRepository;
        _currencyService = currencyService;
    }
    public async Task<List<Additionalproduct>> GetAdditionalProductsByIdList(IEnumerable<int> idList, int langId)
    {
        return await _additionalProductRepository.GetAdditionalProductsByIdList(idList, langId + 1);
    }
    public async Task<List<Extra>> GetPremiumPackets(IEnumerable<SpecialRequest> specialRequests, ReservationToken token, int rentalDuration, int langId)
    {
        if (specialRequests == null || !specialRequests.Any())
            return new List<Extra>();

        var productIds = specialRequests
               .Select(e => e.AdditionalProductId)
               .Distinct()
               .ToList();

        var extras = await GetAdditionalProductsByIdList(productIds, langId);

        extras = extras.Where(e => e.VendorExtraExists != true).ToList();

        if (!extras.Any())
            return new List<Extra>();

        var curencies = await _currencyService.GetAllCurrencies();
        var premiumExtras = new List<Extra>();

        foreach (var specialRequest in specialRequests)
        {
            var extra = extras.Where(e => e.Productid == specialRequest.AdditionalProductId).FirstOrDefault();

            if (extra != null)
            {
                if (!extra.ShowOnlyFullCreditVehicles || token.FullCredit)
                {
                    var rentalType = (ExtraRentalTypes)specialRequest.SpecialRequestTariff.RentalTypeId;

                    var currency = curencies.FirstOrDefault(c => c.Currencyid == extra.CurrencyId);

                    var extraDailyPrice = (float)specialRequest.SpecialRequestTariff.Amount;

                    if (rentalType == ExtraRentalTypes.Daily)
                    {
                        var maxAmount = specialRequest.SpecialRequestTariff?.MaxAmount;

                        if (maxAmount.HasValue)
                        {
                            var totalPrice = extraDailyPrice * rentalDuration;

                            if (totalPrice > maxAmount.Value)
                                extraDailyPrice = TruncateToTwoDecimalPlaces(maxAmount.Value / rentalDuration);
                        }
                    }

                    premiumExtras.Add(new Extra
                    {
                        ExtraId = extra.Productid,
                        ExtraCode = extra.Productcode,
                        ExtraName = extra.Productname,
                        ExtraDescription = extra.Productdescription,
                        ExtraRentalType = rentalType,
                        ExtraType = AdditionalProductTypes.Premium,
                        ExtraQuantityIncreasable = false,
                        Price = extraDailyPrice,
                        Icon = extra.Iconpath,
                        CurrencyCode = currency?.Currencyisocode,
                        VendorId = token.VendorId,
                        ShowDayCountStart = extra.Showdaycountstart,
                        ShowDayCountEnd = extra.Showdaycountend,
                        Sequence = extra.Sequence ?? 1,
                        IsRequired = false,
                        VendorExtraExists = extra.VendorExtraExists ?? false,
                        DefaultPrice = (float)extra.Defaultprice,
                        ShowInVehicleList = extra.Showinvehiclelist ?? false
                    });
                }
            }
        }
        return premiumExtras;
    }

    private static float TruncateToTwoDecimalPlaces(float value)
    {
        return (float)(Math.Truncate((decimal)value * 100) / 100);
    }
}
