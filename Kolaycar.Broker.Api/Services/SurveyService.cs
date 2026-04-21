using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface ISurveyService
    {
        Task<IEnumerable<CommentDto>> GetCommentsByLanguageIdLocationId(int languageId, int location);
        Task<IEnumerable<ExternalComment>> GetExternalCommentsByLanguageIdLocationId(int languageId, int location, int vendorId);
    }
    public class SurveyService : ISurveyService
    {
        private readonly BrokerContext _context;
        public SurveyService(BrokerContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByLanguageIdLocationId(int languageId, int location)
        {
            return await _context.CommentDtos.FromSqlRaw($"EXEC GETCOMMENTSBYLANGIDLOCATIONID {languageId},{location}").ToListAsync();
        }

        public async Task<IEnumerable<ExternalComment>> GetExternalCommentsByLanguageIdLocationId(int languageId, int location, int vendorId)
        {
            var externalComments = await _context.ExternalComments
                                                            .Where(ex => //ex.LanguageId == languageId &&
                                                                        ex.LocationId == location &&
                                                                        ex.VendorId == vendorId &&
                                                                        ex.ShowOnWebsite == true)
                                                            .ToListAsync();
            return externalComments;
        }
    }
}
