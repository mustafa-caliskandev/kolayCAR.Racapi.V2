using KolayCAR.Broker.API.Models;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Abstract
{
    public interface ICityRepository : IRepository<City>
    {
        Task<City> GetCityByIdAsync(int cityId);
    }
}
