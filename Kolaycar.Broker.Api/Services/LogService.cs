using KolayCAR.Broker.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface ILogService
    {
        Task SaveLogAsync(string template, string message, string requestId);
        Task SaveEmailLog(EmailLog log);
    }

    public class LogService : ILogService
    {
        private readonly IWebHostEnvironment _env;

        private readonly BrokerContext _context;

        public LogService(
            IWebHostEnvironment env,
            BrokerContext context)
        {
            _env = env;
            _context = context;
        }

        public async Task SaveEmailLog(EmailLog log)
        {

            try
            {
                if (!_env.IsDevelopment())
                {
                    await _context.EmailLogs.AddAsync(log);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {

            }

        }

        public async Task SaveLogAsync(string template, string message, string requestId)
        {
            try
            {
                await _context.Apilog.AddAsync(new Apilog
                {
                    Message = message,
                    MessageTemplate = template,
                    TimeStamp = DateTime.Now,
                    RequestId = requestId
                });
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
        }
    }
}
