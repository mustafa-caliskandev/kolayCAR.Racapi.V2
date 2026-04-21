using KolayCAR.Broker.API.Attributes;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Controllers
{
    [BrokerAuthorize(UserRoles.Admin, UserRoles.Agency, UserRoles.User, UserRoles.MobileAPP)]
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet("{id}")]
        public async Task<ReservationInvoice> GetInvoice(int id)
        {
            return await _invoiceService.GetReservationInvoice(id);
        }

        [HttpGet("list")]
        public async Task<List<ReservationInvoice>> GetInvoiceList(bool isOnlyActiveInvoice = false)
        {
            return await _invoiceService.GetReservationInvoiceList(isOnlyActiveInvoice);
        }

        [HttpPost]
        public async Task<HttpResult<ReservationInvoice>> PostInvoice(string reservationNumber, string customerEmail)
        {
            var result = await _invoiceService.CreateReservationInvoice(reservationNumber, customerEmail);

            return new HttpResult<ReservationInvoice>
            {
                Success = result.Success,
                Message = result.Success ? "Invoice created successfully!" : result.Message,
                Data = result.Success ? result.Data as ReservationInvoice : null,
                HttpStatusCode = System.Net.HttpStatusCode.OK
            };
        }

        [HttpPost("cancel")]
        public async Task<HttpResult<ReservationInvoice>> PostCancelInvoice(string reservationNumber, string customerEmail)
        {
            var result = await _invoiceService.CancelReservationInvoice(reservationNumber, customerEmail);

            return new HttpResult<ReservationInvoice>
            {
                Success = result.Success,
                Message = result.Success ? "Invoice has been successfully canceled!" : result.Message,
                Data = result.Success ? result.Data as ReservationInvoice : null,
                HttpStatusCode = System.Net.HttpStatusCode.OK
            };
        }
    }
}
