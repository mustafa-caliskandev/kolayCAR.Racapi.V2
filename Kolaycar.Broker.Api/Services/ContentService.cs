using KolayCAR.Broker.API.Extensions;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Models.MobileAppModels.Enums;
using KolayCAR.Broker.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace KolayCAR.Broker.API.Services
{
    public interface IContentService
    {
        public Task<IEnumerable<Icerikdil>> GetSmsContent(LanguageTypes languageType);
        public Task<IEnumerable<Icerikdil>> GetRentalContracts(int languageId);
        public Task<IEnumerable<Icerik>> GetAllActiveContents(int tipId);
        public Task<IEnumerable<Icerikdil>> GetAllContentLanguagesIncludeContent(int tipId, int languageId);
        public Task<string> GetContractUrlByVendorId(LanguageTypes languageType, int vendorId);
        public (string typeName, string groupName) GetCvcViewSettings();
        public (string typeName, string groupName) GetClarificationTextViewSettings();
        public (string typeName, string groupName) GetInformationViewSettings();
        public (string typeName, string groupName) GetKvkkViewSettings();
        public (string typeName, string groupName) GetRentalConditionViewSettings();
        public (string typeName, string groupName) GetSalesContractViewSettings();
        public (string typeName, string groupName) GetPopupViewSettings();
    }
    public class ContentService : IContentService
    {
        private readonly BrokerContext _context;
        private readonly IConfiguration _configuration;
        public ContentService(BrokerContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IEnumerable<Icerikdil>> GetSmsContent(LanguageTypes languageType)
        {
            //modelin içerisinde diğer tablolardan referans yoktu o yüzden include kullanamadım :/

            var groupList = await _context.Icerik.Where(x => x.Tipid == (int)IcerikGrup.SMS).Select(x => x.Icerikid).ToListAsync();
            var smsList = await _context.Icerikdil.Where(x => groupList.Contains(x.Icerikid)
                                                                        && x.Aktifmi == true
                                                                        && x.Dilid == (int)languageType + 1
                                                                        && ((x.BaslangicTarihi != null && x.BitisTarihi != null)
                                                                         ? (x.BaslangicTarihi <= DateTime.Now && x.BitisTarihi >= DateTime.Now)
                                                                         : true)).ToListAsync();
            return smsList;
        }
        public async Task<string> GetContractUrlByVendorId(LanguageTypes languageType, int vendorId)
        {
            var icerikIdList = await _context.Icerik.Where(x => x.Tipid == 10 && x.Vendorid == vendorId).Select(x => x.Icerikid).ToListAsync();
            var value = await _context.Icerikdil.Where(x => icerikIdList.Contains(x.Icerikid) && x.Dilid == (int)languageType + 1
                                                                                              && x.Aktifmi == true).Select(x => x.Contenturl).FirstOrDefaultAsync();
            return value;
        }

        public (string typeName, string groupName) GetCvcViewSettings()
        {
            return (_configuration.GetSectionValueString("CvcView", "typeName"),
                _configuration.GetSectionValueString("CvcView", "groupName"));
        }

        public (string typeName, string groupName) GetPopupViewSettings()
        {
            return (_configuration.GetSectionValueString("VehiclelistPopup", "typeName"),
                _configuration.GetSectionValueString("VehiclelistPopup", "groupName"));
        }

        public (string typeName, string groupName) GetInformationViewSettings()
        {
            return (_configuration.GetSectionValueString("InformationView", "typeName"),
                _configuration.GetSectionValueString("InformationView", "groupName"));
        }

        public (string typeName, string groupName) GetRentalConditionViewSettings()
        {
            return (_configuration.GetSectionValueString("RentalConditionView", "typeName"),
                _configuration.GetSectionValueString("RentalConditionView", "groupName"));
        }

        public (string typeName, string groupName) GetSalesContractViewSettings()
        {
            return (_configuration.GetSectionValueString("SalesContractView", "typeName"),
                _configuration.GetSectionValueString("SalesContractView", "groupName"));
        }

        public (string typeName, string groupName) GetKvkkViewSettings()
        {
            return (_configuration.GetSectionValueString("KvkkView", "typeName"),
                _configuration.GetSectionValueString("KvkkView", "groupName"));
        }

        public (string typeName, string groupName) GetClarificationTextViewSettings()
        {
            return (_configuration.GetSectionValueString("ClarificationTextView", "typeName"),
                _configuration.GetSectionValueString("ClarificationTextView", "groupName"));
        }

        public async Task<IEnumerable<Icerik>> GetAllActiveContents(int tipId)
        {
            return await _context.Icerik.Where(i => i.Tipid == tipId && (i.Aktif ?? true)).ToListAsync();
        }

        public async Task<IEnumerable<Icerikdil>> GetRentalContracts(int languageId)
        {
            return await _context.Icerikdil.Include(i => i.Icerik).Where(i => i.Dilid == languageId && i.Icerik.Tipid == (int)ContentTypes.TedarikciSozlesmesi).ToListAsync();
        }

        public async Task<IEnumerable<Icerikdil>> GetAllContentLanguagesIncludeContent(int tipId, int languageId)
        {
            return await _context.Icerikdil.Where(i => i.Dilid == languageId && i.Icerik.Tipid == tipId).Include(i => i.Icerik).ToListAsync();
        }
    }
}
