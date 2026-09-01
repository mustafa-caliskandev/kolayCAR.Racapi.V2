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
    private readonly IExchangeRateService _exchangeRateService;
    public AdditionalProductService(IAdditionalProductRepository additionalProductRepository, IExchangeRateService exchangeRateService)
    {
        _additionalProductRepository = additionalProductRepository;
        _exchangeRateService = exchangeRateService;
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

        var exchangeRates = await _exchangeRateService.GetAllExchangeRates();
        var premiumExtras = new List<Extra>();

        foreach (var specialRequest in specialRequests)
        {
            var extra = extras.Where(e => e.Productid == specialRequest.AdditionalProductId).FirstOrDefault();

            if (extra != null)
            {
                if (!extra.ShowOnlyFullCreditVehicles || (token.CreditType == CreditType.FullCredit || token.CreditType == CreditType.LimitedCredit))
                {
                    var tariff = specialRequest.SpecialRequestTariff;
                    if (tariff?.Amount == null)
                        continue;

                    var rentalType = (ExtraRentalTypes)tariff.RentalTypeId;
                    var sourceCurrencyType = (CurrencyTypes)(tariff.CurrencyId - 1);
                    var targetCurrencyType = token.CurrencyType;

                    var extraDailyPrice = tariff.Amount.Value;

                    if (rentalType == ExtraRentalTypes.Daily)
                    {
                        var maxAmount = tariff.MaxAmount;

                        if (maxAmount.HasValue)
                        {
                            var totalPrice = extraDailyPrice * rentalDuration;

                            if (totalPrice > maxAmount.Value)
                                extraDailyPrice = TruncateToTwoDecimalPlaces(maxAmount.Value / rentalDuration);
                        }
                    }

                    extraDailyPrice = ConvertCurrency(extraDailyPrice, sourceCurrencyType, targetCurrencyType, exchangeRates);

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
                        CurrencyCode = targetCurrencyType.ToString(),
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

    private static float ConvertCurrency(float amount, CurrencyTypes sourceCurrency, CurrencyTypes targetCurrency, IEnumerable<Exchangerates> exchangeRates)
    {
        if (sourceCurrency == targetCurrency)
            return TruncateToTwoDecimalPlaces(amount);

        var sourceRate = exchangeRates?.FirstOrDefault(e => e.Currencyid == (int)sourceCurrency + 1);
        var targetRate = exchangeRates?.FirstOrDefault(e => e.Currencyid == (int)targetCurrency + 1);

        if (sourceRate?.Exchangerate == null || targetRate?.Exchangerate == null || targetRate.Exchangerate == 0)
            return TruncateToTwoDecimalPlaces(amount);

        return TruncateToTwoDecimalPlaces((float)((decimal)amount * sourceRate.Exchangerate.Value / targetRate.Exchangerate.Value));
    }
}
