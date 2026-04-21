using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class CurrencyRepository : Repository<Currency>, ICurrencyRepository
    {
        public CurrencyRepository(BrokerContext context) : base(context)
        {
        }
        public async Task<Currency> GetCurrencyByCode(string currencyCode) => await _dbSet.FirstOrDefaultAsync(e => e.Currencyisocode == currencyCode);
        public async Task<bool> IsActiveCurrency(string currencyCode) => await _dbSet.AnyAsync(e => e.Currencyisocode == currencyCode && e.Active == true);
    }
}
