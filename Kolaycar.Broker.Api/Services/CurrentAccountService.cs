using KolayCAR.Broker.API.Repositories.Abstract;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface ICurrentAccountService
    {
        public Task DeleteCurrentAccountByReservationNumber(string reservationNumber);
    }
    public class CurrentAccountService : ICurrentAccountService
    {
        private readonly ICurrentAccountRepository _currentAccountRepository;
        public CurrentAccountService(ICurrentAccountRepository currentAccountRepository)
        {
            _currentAccountRepository = currentAccountRepository;
        }

        public async Task DeleteCurrentAccountByReservationNumber(string reservationNumber) =>
           await _currentAccountRepository.DeleteByReservationNumber(reservationNumber);
    }
}
