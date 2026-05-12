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
            try
            {
                if (CacheSettings.UseCache)
                    return await _cacheService.GetOrCreateAsync($"Label-{languageId}", () => GetSafeLabelQuery().Where(x => x.Dilid == languageId).ToListAsync());

                return await GetSafeLabelQuery().Where(x => x.Dilid == languageId).ToListAsync();
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error("{@LabelServiceGetAllLabelsByLanguageIdError}", ex.Message);
                return new List<Label>();
            }
        }
        public async Task<Label> GetLabelByCodeAndLanguageId(string labelCode, int languageId)
        {
            try
            {
                if (CacheSettings.UseCache)
                    return await _cacheService.GetOrCreateAsync($"Label-{labelCode}-{languageId}", () => GetSafeLabelQuery().Where(x => x.LabelKodu == labelCode && x.Dilid == languageId).FirstOrDefaultAsync());

                return await GetSafeLabelQuery().Where(x => x.LabelKodu == labelCode && x.Dilid == languageId).FirstOrDefaultAsync();
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error("{@LabelServiceGetLabelByCodeAndLanguageIdError}", ex.Message);
                return null;
            }
        }

        private IQueryable<Label> GetSafeLabelQuery()
        {
            return _context.Label
                .AsNoTracking()
                .Select(x => new Label
                {
                    Labelid = x.Labelid,
                    Dilid = x.Dilid,
                    Labeladi = x.Labeladi,
                    LabelKodu = x.LabelKodu,
                    TypeId = x.TypeId
                });
        }
    }
}
