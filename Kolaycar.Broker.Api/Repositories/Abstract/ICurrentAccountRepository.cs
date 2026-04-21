using KolayCAR.Broker.API.Models;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Abstract
{
    public interface ICurrentAccountRepository : IRepository<Currentaccount>
    {
        Task DeleteByReservationNumber(string reservationNumber);
    }
}
