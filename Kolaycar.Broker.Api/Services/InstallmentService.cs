using KolayCAR.Broker.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IInstallmentService
    {
        Task AddRangeAsync(List<Installment> installmentList);
    }
    public class InstallmentService : IInstallmentService
    {
        private readonly BrokerContext _context;
        public InstallmentService(BrokerContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(List<Installment> installmentList)
        {
            try
            {
                await _context.Installments.AddRangeAsync(installmentList);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
