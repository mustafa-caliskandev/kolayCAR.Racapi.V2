using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Services.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface ILabelService
    {
        Task<List<Label>> GetAllLabelsByLanguageId(int languageId);
        Task<Label> GetLabelByCodeAndLanguageId(string labelCode, int languageId);
    }
    public class LabelService : ILabelService
    {
        private readonly BrokerContext _context;
        private readonly ICacheService _cacheService;
        public LabelService(BrokerContext context, ICacheService cacheService)
        {
            _context = context;
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _cacheService = cacheService;
        }
        public async Task<List<Label>> GetAllLabelsByLanguageId(int languageId)
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync($"Label-{languageId}", () => _context.Label.Where(x => x.Dilid == languageId).ToListAsync());

            return await _context.Label.Where(x => x.Dilid == languageId).ToListAsync();
        }
        public async Task<Label> GetLabelByCodeAndLanguageId(string labelCode, int languageId)
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync($"Label-{labelCode}-{languageId}", () => _context.Label.Where(x => x.LabelKodu == labelCode && x.Dilid == languageId).FirstOrDefaultAsync());

            return await _context.Label.Where(x => x.LabelKodu == labelCode && x.Dilid == languageId).FirstOrDefaultAsync();
        }
    }
}
