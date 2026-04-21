using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class CityRepository : Repository<City>, ICityRepository
    {
        public CityRepository(BrokerContext context) : base(context)
        {
        }
        public async Task<City> GetCityByIdAsync(int cityId) 
        {
            return await _dbSet.Where(e => e.Cityid == cityId).FirstOrDefaultAsync();
        }
    }
}
