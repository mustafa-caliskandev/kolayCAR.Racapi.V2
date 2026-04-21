using KolayCAR.Broker.API.Repositories.Abstract;

namespace KolayCAR.Broker.API.Services
{
    public interface ISpecialRequestLocationService
    {

    }
    public class SpecialRequestLocationService : ISpecialRequestLocationService
    {
        private readonly ISpecialRequestLocationRepository _specialRequestLocationRepository;
        public SpecialRequestLocationService(ISpecialRequestLocationRepository specialRequestLocationRepository)
        {            
            _specialRequestLocationRepository = specialRequestLocationRepository;
        }
    }
}
