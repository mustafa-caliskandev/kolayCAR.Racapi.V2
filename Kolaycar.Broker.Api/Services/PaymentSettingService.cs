using KolayCAR.Broker.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IPaymentSettingService
    {
        Task<PaymentSetting> AddAsync(PaymentSetting paymentSetting);
        Task<PaymentSetting> GetByTokenWithDetails(string reservationToken);
        Task<bool> UpdateAsync(int Id, PaymentSetting paymentSetting);
    }
    public class PaymentSettingService : IPaymentSettingService
    {
        private readonly BrokerContext _context;
        public PaymentSettingService(BrokerContext context)
        {
            _context = context;
        }

        public async Task<PaymentSetting> AddAsync(PaymentSetting paymentSetting)
        {
            try
            {
                await _context.PaymentSettings.AddAsync(paymentSetting);
                await _context.SaveChangesAsync();
                return paymentSetting;
            }
            catch (Exception)
            {
                return Activator.CreateInstance<PaymentSetting>();
                throw;
            }
        }

        public async Task<PaymentSetting> GetByTokenWithDetails(string reservationToken)
        {
            try
            {
                var result = await _context.PaymentSettings
                .Where(ps => ps.ReservationToken == reservationToken)
                .Include(ps => ps.Bank)
                .Include(ps => ps.Installments)
                .OrderByDescending(ps => ps.Id)
                .FirstOrDefaultAsync();
                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<bool> UpdateAsync(int Id, PaymentSetting paymentSetting)
        {
            if (paymentSetting == null || Id <= 0)
            {
                return false;
            }

            paymentSetting.Id = Id;

            try
            {
                _context.Entry(paymentSetting).State = EntityState.Modified;
                _context.SaveChanges();
                return true;
            }
            catch (System.Exception)
            {
                return false;
                throw;
            }
        }
    }
}
