using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Concrete;

public class MemberRepository : Repository<Uye>, IMemberRepository
{
    public MemberRepository(BrokerContext context) : base(context)
    {
    }
    public async Task<Uye> GetFirstMemberByAgencyId(int agencyId) =>
         await _dbSet.Where(u => u.Agencyid == agencyId).OrderBy(u => u.Uyeid).FirstOrDefaultAsync();
}
