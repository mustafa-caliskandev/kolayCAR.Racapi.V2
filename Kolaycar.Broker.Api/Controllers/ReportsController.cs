using KolayCAR.Broker.API.Attributes;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Controllers
{
    [BrokerAuthorize(UserRoles.Admin)]
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost("sendMonthly")]
        public async Task<string> SendAllMonthlyReport()
        {
            return await _reportService.SendAllMonthlyReport();
        }

        [HttpPost("sendMonthly/{vendorId}")]
        public async Task<bool> SendVendorMonthlyReport(int vendorId)
        {
            if (vendorId == null || vendorId <= 0)
            {
                return false;
            }

            return await _reportService.SendVendorMonthlyReport(vendorId);
        }

        [HttpPost("sendWeekly")]
        public async Task<string> SendAllWeeklyReport()
        {
            return await _reportService.SendAllWeeklyReport();
        }

        [HttpPost("sendWeekly/{vendorId}")]
        public async Task<bool> SendAllWeeklyReport(int vendorId)
        {
            if (vendorId == null || vendorId <= 0)
            {
                return false;
            }

            return await _reportService.SendVendorWeeklyReport(vendorId);
        }

    }
}
