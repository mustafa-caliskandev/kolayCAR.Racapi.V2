using KolayCAR.Broker.API.Models;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Abstract
{
    public interface ICurrencyRepository : IRepository<Currency>
    {
        Task<bool> IsActiveCurrency(string currencyCode);
        Task<Currency> GetCurrencyByCode(string currencyCode);
    }
}
