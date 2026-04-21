using KolayCAR.Broker.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface ILanguageService
    {
        Task<IEnumerable<Dil>> GetActiveLanguages();
        Task<IEnumerable<Dil>> GetAllLanguages();
        Task<Dil> Get(string languageCode);
        Task<Dil> GetById(int languageId);
        Task<Dil> GetByLanguageCode(string languageCode);
    }
    public class LanguageService : ILanguageService
    {
        private readonly BrokerContext _context;
        public LanguageService(BrokerContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Dil>> GetActiveLanguages()
        {
            return await _context.Dil.Where(d => d.Aktif == true).ToListAsync();
        }

        public async Task<IEnumerable<Dil>> GetAllLanguages()
        {
            return await _context.Dil.ToListAsync();
        }

        public async Task<Dil> Get(string languageCode)
        {
            var result = await _context.Dil.FirstOrDefaultAsync(d => d.Aktif == true && d.Dilkod == languageCode);
            return result;
        }

        public async Task<Dil> GetById(int languageId)
        {
            return await _context.Dil.FirstOrDefaultAsync(d => d.Aktif == true && d.Dilid == languageId);
        }

        public async Task<Dil> GetByLanguageCode(string languageCode)
        {
            return await _context.Dil.FirstOrDefaultAsync(d => d.Dilkod == languageCode);
        }
    }
}
