using KolayCAR.Broker.API.Helpers.Netresys;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public class NetResysService
    {
        HttpManager _httpManager;
        NetresysHelper _netresysHelper;
        IConfiguration _configuration;

        public NetResysService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpManager = new HttpManager(_configuration["AppSettings:ApiBaseUrl"]);
            _netresysHelper = new NetresysHelper();
        }
        public async Task<ServiceResponseBase> SendNetresysRequestAsync(Dictionary<string, object> header, Reservation reservation, Domain.Models.Agency agency)
        {
            var request = _netresysHelper.GetEntity(reservation, agency);
            var result = await _httpManager.PostAsyncWithModel<object, ServiceResponseBase>(
                requestPath: "netresys",
                parameters: _netresysHelper.CreateNetresysRequestModel(request),
                headers: header);
            return result;
        }

        public async Task<ServiceResponseBase> SendNetresysCancelRequestAsync(Dictionary<string, object> header, Reservation reservation)
        {
            var request = _netresysHelper.GetCancelEntity(reservation);
            var result = await _httpManager.PostAsyncWithModel<object, ServiceResponseBase>(
                requestPath: "netresys/cancel",
                parameters: _netresysHelper.CreateNetresysCancelRequestModel(request),
                headers: header);
            return result;
        }
    }
}
