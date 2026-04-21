using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Models.PaymentDto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IReservationPaymentDetailService
    {
        Task<bool> ResultUpdate(PaymentResultUpdateDto paymentResultUpdateDto);
        Task<ReservationPaymentDetail> GetByReservationDetailId(int reservationDetailId);
    }
    public class ReservationPaymentDetailService : IReservationPaymentDetailService
    {
        private readonly IReservationDetailService _reservationDetailService;
        private readonly BrokerContext _context;
        public ReservationPaymentDetailService(
            IReservationDetailService reservationDetailService, BrokerContext context)
        {
            _reservationDetailService = reservationDetailService;
            _context = context;
        }

        public async Task<bool> ResultUpdate(PaymentResultUpdateDto paymentResultUpdateDto)
        {
            if (string.IsNullOrEmpty(paymentResultUpdateDto.ReservationToken))
            {
                return false;
            }

            try
            {
                var reservationDetail =
                    await _reservationDetailService.GetByReservationToken(paymentResultUpdateDto.ReservationToken);

                if (reservationDetail != null && reservationDetail.Id > 0)
                {

                    var reservationPaymentDetail =
                        await GetByReservationDetailId(reservationDetail.Id);

                    if (reservationPaymentDetail != null && reservationPaymentDetail.Id > 0)
                    {
                        reservationPaymentDetail.PaymentResultCode = paymentResultUpdateDto.PaymentResultCode;
                        reservationPaymentDetail.PaymentResultMessage = paymentResultUpdateDto.PaymentResultMessage;
                        reservationPaymentDetail.BankResultMessage = paymentResultUpdateDto.BankResultMessage;
                        reservationPaymentDetail.ProvisionNumber = paymentResultUpdateDto.ProvisionNumber;
                        _context.Entry(reservationPaymentDetail).State = EntityState.Modified;
                        _context.SaveChanges();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO : Loglama yapılacak
            }
            return false;
        }

        public async Task<ReservationPaymentDetail> GetByReservationDetailId(int reservationDetailId)
        {
            return await _context.ReservationPaymentDetails
                .AsNoTracking()
                .OrderByDescending(r => r.Id)
                .FirstOrDefaultAsync(r => r.ReservationDetailId == reservationDetailId) ?? null;
        }
    }
}
