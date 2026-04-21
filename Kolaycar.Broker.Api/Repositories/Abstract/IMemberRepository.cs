using KolayCAR.Broker.API.Models;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Abstract;

public interface IMemberRepository : IRepository<Uye>
{
    public Task<Uye> GetFirstMemberByAgencyId(int agencyId);
}
