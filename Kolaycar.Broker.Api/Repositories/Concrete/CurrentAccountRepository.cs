using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class CurrentAccountRepository : Repository<Currentaccount>, ICurrentAccountRepository
    {
        public CurrentAccountRepository(BrokerContext context) : base(context)
        {
        }

        public async Task DeleteByReservationNumber(string reservationNumber)
        {
            var entities = _dbSet.Where(e=> e.DocumentNumber == reservationNumber).ToList();

            if (entities.Any())
            {
                _dbSet.RemoveRange(entities);
                await _context.SaveChangesAsync();
            }
        }
    }
}
