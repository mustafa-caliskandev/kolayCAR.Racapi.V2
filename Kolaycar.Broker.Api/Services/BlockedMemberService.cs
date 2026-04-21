using KolayCAR.Broker.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IBlockedMemberService
    {
        Task<bool> IsBlocked(string memberIdentity);
    }
    public class BlockedMemberService : IBlockedMemberService
    {
        private readonly BrokerContext _context;
        public BlockedMemberService(BrokerContext context)
        {
            _context = context;
        }

        public async Task<bool> IsBlocked(string bannedMemberIdentity)
        {
            var memberIds = await _context.Uye
                .Where(m => m.Tcpasaport == bannedMemberIdentity)
                .Select(m => m.Uyeid)
                .ToListAsync();
            var result = await _context.BlockedMembers.Where(bm => memberIds.Contains(bm.MemberId)).AnyAsync();

            if (!result)
            {
                result = await _context.BlockedDatas.Where(d => d.PassportNumber == bannedMemberIdentity || d.OtherInfos.Contains(bannedMemberIdentity)).AnyAsync();
            }

            return result;
        }
    }
}
