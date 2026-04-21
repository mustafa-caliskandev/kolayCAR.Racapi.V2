using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IReservationTokenService
    {
        Task<string> GetReservationToken(string uniqueId);
        Task<string> GetReservationTokenSessionId(string uniqueId);
        Task<bool> GetReservationTokenExists(string reservationToken);
    }
    public class ReservationTokenService : IReservationTokenService
    {
        private readonly BrokerContext _context;
        public ReservationTokenService(BrokerContext context)
        {
            _context = context;
        }
        public async Task<string> GetReservationToken(string uniqueId)
        {
            var resToken = await _context.Restoken.FirstOrDefaultAsync(r => r.Uniqueid == uniqueId);
            return resToken != null && resToken.Id > 0 ? ObjectHelper.DecompressToString(resToken.Token) : "";
        }

        public async Task<string> GetReservationTokenSessionId(string uniqueId)
        {
            var resToken = await _context.Restoken.FirstOrDefaultAsync(r => r.Uniqueid == uniqueId);
            return resToken != null && resToken.Id > 0 ? resToken.SessionId : "";
        }

        public async Task<bool> GetReservationTokenExists(string reservationToken)
        {
            bool reservationTokenExists = !await _context.Rez.AnyAsync(r => r.Reservationtoken == reservationToken);

            if (reservationTokenExists)
            {
                reservationTokenExists = await _context.Restoken.AnyAsync(r => r.Uniqueid == reservationToken);
            }

            return reservationTokenExists;
        }
    }
}
