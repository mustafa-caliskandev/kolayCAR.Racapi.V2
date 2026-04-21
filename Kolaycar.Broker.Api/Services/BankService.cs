using KolayCAR.Broker.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IBankService
    {
        Task<Bank> AddAsync(Bank bank);
        Task<Bank> GetBankByVendorIdAsync(int bankVendorId);
    }
    public class BankService : IBankService
    {
        private readonly BrokerContext _context;
        public BankService(BrokerContext context)
        {
            _context = context;
        }

        public async Task<Bank> AddAsync(Bank bank)
        {
            try
            {
                await _context.Banks.AddAsync(bank);
                await _context.SaveChangesAsync();
                return bank;
            }
            catch (Exception)
            {
                return Activator.CreateInstance<Bank>();
                throw;
            }
        }

        public async Task<Bank> GetBankByVendorIdAsync(int bankVendorId)
        {
            var result = await _context.Banks.FirstOrDefaultAsync(b => b.BankVendorId == bankVendorId);
            if (result == null)
            {
                return Activator.CreateInstance<Bank>();
            }

            return result;
        }
    }
}
