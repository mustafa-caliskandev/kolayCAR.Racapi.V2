using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Services.Abstract;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IExchangeRateService
    {
        Task<IEnumerable<Exchangerates>> GetAllExchangeRates();
    }
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly ICacheService _cacheService;
        private readonly IExchangeRateRepository _exchangeRateRepository;
        public ExchangeRateService(ICacheService cacheService, IExchangeRateRepository exchangeRateRepository)
        {
            _cacheService = cacheService;
            _exchangeRateRepository = exchangeRateRepository;
        }
        public async Task<IEnumerable<Exchangerates>> GetAllExchangeRates()
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync("ExchangeRate", _exchangeRateRepository.GetAllAsync);

            return await _exchangeRateRepository.GetAllAsync();
        }
    }
}
