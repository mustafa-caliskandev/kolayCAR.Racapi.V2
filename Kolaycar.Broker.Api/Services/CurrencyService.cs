using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Services.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface ICurrencyService
    {
        Task<IEnumerable<Currency>> GetAllCurrencies();
        Task<Currency> Get(string currencyCode);
        Task<Currency> GetCurrencyByCode(string currencyCode);
        Task<bool> IsActiveCurrency(string currencyCode);
    }
    public class CurrencyService : ICurrencyService
    {
        private readonly BrokerContext _context;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly ICacheService _cacheService;
        public CurrencyService(ICurrencyRepository currencyRepository, BrokerContext context, ICacheService cacheService)
        {
            _currencyRepository = currencyRepository;
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<Currency> Get(string currencyCode)
        {
            return await _context.Currency.FirstOrDefaultAsync(c => c.Active == true && c.Currencyisocode == currencyCode);
        }

        public async Task<IEnumerable<Currency>> GetAllCurrencies()
        {
            return await _context.Currency.Where(c => c.Active == true).ToListAsync();
        }
        public async Task<Currency> GetCurrencyByCode(string currencyCode)
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync($"Currency-{currencyCode}", () => _currencyRepository.GetCurrencyByCode(currencyCode));

            return await _currencyRepository.GetCurrencyByCode(currencyCode);
        }

        public async Task<bool> IsActiveCurrency(string currencyCode) => await _currencyRepository.IsActiveCurrency(currencyCode);
    }
}
