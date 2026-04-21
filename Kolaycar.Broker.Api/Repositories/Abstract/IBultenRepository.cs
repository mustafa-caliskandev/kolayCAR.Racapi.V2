using KolayCAR.Broker.API.Models;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Abstract
{
    public interface IBultenRepository : IRepository<Bulten>
    {
        public Task<Bulten> GetByEmail(string customerEmail);
    }
}
