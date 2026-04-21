using KolayCAR.Broker.API.Repositories.Abstract;

namespace KolayCAR.Broker.API.Services
{
    public interface ISpecialRequestAgencyService 
    {
    }
    public class SpecialRequestAgencyService : ISpecialRequestAgencyService
    {
        private readonly ISpecialRequestAgencyRepository _specialRequestAgencyRepository;
        public SpecialRequestAgencyService(ISpecialRequestAgencyRepository specialRequestAgencyRepository)
        {
            _specialRequestAgencyRepository = specialRequestAgencyRepository;
        }
    }
}
