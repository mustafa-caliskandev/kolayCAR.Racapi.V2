using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Models.PaymentDto;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IPaymentResultService
    {
        Task<bool> AddAsync(PaymentResult entity);
        Task<PaymentResult> GetWithResTokenAndPaymentCodeAsync(CheckPaymentResultRequestDto paymentResultRequestDto);
    }
    public class PaymentResultService : IPaymentResultService
    {
        private readonly BrokerContext _context;
        public PaymentResultService(BrokerContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(PaymentResult entity)
        {
            try
            {
                await _context.PaymentResults.AddAsync(entity);
                _context.SaveChanges();
                return true;
            }
            catch (System.Exception)
            {
                return false;
                throw;
            }
        }

        public async Task<PaymentResult> GetWithResTokenAndPaymentCodeAsync(CheckPaymentResultRequestDto paymentResultRequestDto)
        {
            var entity = await _context.PaymentResults.FirstOrDefaultAsync(p => p.ResToken == paymentResultRequestDto.ReservationToken && p.PaymentCode == paymentResultRequestDto.PaymentCode);

            return entity;
        }
    }
}
