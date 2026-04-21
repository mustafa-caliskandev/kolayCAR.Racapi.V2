using KolayCAR.Broker.API.Repositories.Abstract;

namespace KolayCAR.Broker.API.Services
{
    public interface ISpecialRequestVendorService 
    {
    }
    public class SpecialRequestVendorService : ISpecialRequestVendorService
    {
        private readonly ISpecialRequestVendorRepository _specialRequestVendorRepository;
        public SpecialRequestVendorService(ISpecialRequestVendorRepository specialRequestVendorRepository)
        {
            _specialRequestVendorRepository = specialRequestVendorRepository;
        }
    }
}
