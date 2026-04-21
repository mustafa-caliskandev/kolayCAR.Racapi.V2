using KolayCAR.Broker.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IBankBinCodeService
    {
        Task<bool> IsInstallmentSupported(long binNumber);
    }
    public class BankBinCodeService : IBankBinCodeService
    {
        private readonly BrokerContext _context;
        public BankBinCodeService(BrokerContext brokerContext)
        {
            _context = brokerContext;
        }

        public async Task<bool> IsInstallmentSupported(long binNo)
        {
            var binCode = $"{binNo}0000000000000"[..13];
            if (!long.TryParse(binCode, out var bin))
            {
                bin = 0;
            }

            var result = await _context.BankBinCodes.FirstOrDefaultAsync(b => b.BinStart <= bin && b.BinEnd >= bin);
            return result?.InstallmentSupported ?? true;
        }
    }
}
