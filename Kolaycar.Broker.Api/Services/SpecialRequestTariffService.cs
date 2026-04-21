using KolayCAR.Broker.API.Repositories.Abstract;

namespace KolayCAR.Broker.API.Services
{
    public interface ISpecialRequestTariffService
    {
    }
    public class SpecialRequestTariffService : ISpecialRequestTariffService
    {
        private readonly ISpecialRequestTariffRepository _specialRequestTariffRepository;
        public SpecialRequestTariffService(ISpecialRequestTariffRepository specialRequestTariffRepository)
        {
            _specialRequestTariffRepository = specialRequestTariffRepository;
        }
    }
}
