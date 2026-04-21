using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IRentalConditionService
    {
        Task<ServiceResponseBase> GetRentalConditions(int vendorId, LanguageTypes languageType);
    }
    public class RentalConditionService : IRentalConditionService
    {
        private readonly IRentalConditionRepository _rentalConditionRepository;
        private readonly ICacheService _cacheService;
        public RentalConditionService(IRentalConditionRepository rentalConditionRepository, ICacheService cacheService)
        {
            _rentalConditionRepository = rentalConditionRepository;
            _cacheService = cacheService;
        }

        public async Task<ServiceResponseBase> GetRentalConditions(int vendorId, LanguageTypes languageType)
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync($"RentalCondition-{vendorId}-{languageType.ToString()}", () => _rentalConditionRepository.GetRentalConditions(vendorId, languageType));

            return await _rentalConditionRepository.GetRentalConditions(vendorId, languageType);
        }
    }
}
