using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models.StoredPorcedureModels;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IReportService
    {
        Task<string> SendAllMonthlyReport();

        Task<bool> SendVendorMonthlyReport(int vendorId, List<ReservationReportModel> reservation = null);

        Task<string> SendAllWeeklyReport();
        Task<bool> SendVendorWeeklyReport(int vendorId, List<ReservationReportModel> reservation = null);
    }
    public class ReportService : IReportService
    {
        private readonly string _reportProcName = "GETVENDORRESERVATIONREPORT";
        private readonly BrokerContext _context;
        private readonly IEmailService _emailService;

        public ReportService(BrokerContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<string> SendAllMonthlyReport()
        {
            var vendors = await _context.Vendor.Where(x => x.Active == true).ToListAsync();

            if (vendors?.Count == 0) return null;

            //var tasks = new List<Task>();
            var responses = new Dictionary<object, object>();

            var tasks = vendors.Select(async vendorItem =>
            {
                var now = DateTime.Now;
                var endDate = new DateTime(now.Year, now.Month, 1);
                var beginDate = endDate.AddMonths(-1);

                var query =
                    $"EXEC {_reportProcName} @beginDate = '{beginDate.ToString("yyyy-MM-dd")}', @endDate = '{endDate.ToString("yyyy-MM-dd")}', @type='Monthly'";
                var result = GetReportsByQuery(query);

                if (result != null)
                {
                    var reservations = result.Where(x => x.VendorName == vendorItem.Vendorname).ToList();

                    if (reservations?.Count > 0)
                    {
                        var response = await SendVendorMonthlyReport(vendorItem.Vendorid, reservations);

                        responses.Add(vendorItem.Vendorid, response);
                    }
                }
            });

            await Task.WhenAll(tasks);

            return JsonConvert.SerializeObject(responses);
        }

        public async Task<bool> SendVendorMonthlyReport(int vendorId, List<ReservationReportModel> reservation = null)
        {
            var queryDate = ReporDateHelper.GetDateMonthly();

            var query =
                $"EXEC {_reportProcName} @vendorId = {vendorId}, @beginDate = '{queryDate.beginDate.ToString("yyyy-MM-dd")}', @endDate = '{queryDate.endDate.ToString("yyyy-MM-dd")}', @type='Monthly'";
            var procResult = reservation ?? GetReportsByQuery(query);

            if (procResult?.Count == 0) return false;

            var path = $"Docs/Reports/Monthly/vendor-{vendorId}/{DateTime.Now.ToString("dd-MM-yyyy HH-mm")} aylik_rapor.xlsx";

            ReportHelper.ExportToExcel(procResult, path);

            var attachment = new MimePart("application", "vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                Content = new MimeContent(File.OpenRead(path), ContentEncoding.Default),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = Path.GetFileName(path)
            };


            // ToDo: Bu alanlar dinamik hale getirilecek
            return await _emailService.PostEmail("SendVendorMonthlyReportMail", "title", "mail address", "subject", "body", "");
        }

        public async Task<string> SendAllWeeklyReport()
        {
            var vendors = await _context.Vendor.Where(x => x.Active == true).ToListAsync();

            if (vendors?.Count == 0) return null;

            //var tasks = new List<Task>();
            var responses = new Dictionary<object, object>();

            var queryDate = ReporDateHelper.GetDateWeekly();

            var query =
                $"EXEC {_reportProcName} @beginDate = '{queryDate.beginDate.ToString("yyyy-MM-dd HH:mm")}', @endDate = '{queryDate.endDate.ToString("yyyy-MM-dd HH:mm")}', @type='Weekly'";
            var result = GetReportsByQuery(query);

            var tasks = vendors.Select(async vendorItem =>
            {
                if (result != null)
                {
                    var reservations = result.Where(x => x.VendorName == vendorItem.Vendorname).ToList();

                    if (reservations?.Count > 0)
                    {
                        var response = await SendVendorWeeklyReport(vendorItem.Vendorid, reservations);

                        if (!response)
                        {
                            response = await SendVendorWeeklyReport(vendorItem.Vendorid, reservations);
                        }

                        responses.Add(vendorItem.Vendorid, response);
                    }
                }
            });

            await Task.WhenAll(tasks);

            return JsonConvert.SerializeObject(responses);
        }

        public async Task<bool> SendVendorWeeklyReport(int vendorId, List<ReservationReportModel> reservation = null)
        {
            var queryDate = ReporDateHelper.GetDateWeekly();

            var query =
                $"EXEC {_reportProcName} @vendorId = {vendorId}, @beginDate = '{queryDate.beginDate.ToString("yyyy-MM-dd HH:mm")}', @endDate = '{queryDate.endDate.ToString("yyyy-MM-dd HH:mm")}', @type='Weekly'";

            var procResult = reservation ?? GetReportsByQuery(query);

            if (procResult?.Count == 0) return false;

            var path = $"Docs/Reports/Weekly/vendor-{vendorId}/{DateTime.Now.ToString("dd-MM-yyyy HH-mm")} haftalik_rapor.xlsx";

            ReportHelper.ExportToExcel(procResult, path);

            var attachment = new MimePart("application", "vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                Content = new MimeContent(File.OpenRead(path), ContentEncoding.Default),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = Path.GetFileName(path)
            };

            // ToDo: Bu alanlar dinamik hale getirilecek
            return await _emailService.PostEmail("SendVendorWeeklyReportMail", "title", "mail address", "subject", "body", "");
        }

        private List<ReservationReportModel> GetReportsByQuery(string query)
        {
            return _context.ReservationReportModel.FromSqlRaw(query).ToListAsync().Result;
        }
    }
}
