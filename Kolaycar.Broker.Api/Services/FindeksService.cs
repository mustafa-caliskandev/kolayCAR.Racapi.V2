using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.FindeksRequestBase;
using static KolayCAR.Broker.Domain.Models.Response.AvisResponseBase;
using AvisProvider = KolayCAR.Broker.API.Providers.Avis;

namespace KolayCAR.Broker.API.Services
{
    public interface IFindeksService
    {
        public Task<ServiceResponseBase> GetIsActiveFindeksReportExists(GetIsActiveFindeksReportRequest getActiveFindeksReportRequest);
        public Task<ServiceResponseBase> GetFindeksPhoneIdList(GetPhoneIdListRequest getPhoneIdListRequest);
        public Task<ServiceResponseBase> GetFindeksReport(GetFindeksReportRequest getFindeksReportRequest);
        public Task<ServiceResponseBase> FindeksPinRenew(CreatePinRenewRequest createPinRenewRequest);
        public Task<ServiceResponseBase> FindeksPinConfirm(ConfirmPinRequest confirmPinRequest);
        public Task<ServiceResponseBase> IsVehicleSuitableForCustomer(IsVehicleSuitableForCustomer isVehicleSuitableForCustomer);
        public Task SaveLog(string tckn, string reservationToken, string stepName, bool? isSuitable, string request, string response);
    }
    public class FindeksService : IFindeksService
    {
        private IVendorService _vendorService;
        private IFindeksProvider _findeksProvider;
        private IResTokenService _resTokenService;
        private readonly BrokerContext _context;
        public FindeksService(IVendorService vendorService, BrokerContext context, IResTokenService resTokenService)
        {
            _vendorService = vendorService;
            _context = context;
            _resTokenService = resTokenService;
        }
        public async Task<ServiceResponseBase> GetFindeksPhoneIdList(GetPhoneIdListRequest getPhoneIdListRequest)
        {
            var vendor = await _vendorService.GetVendorById(getPhoneIdListRequest.VendorId);
            if (vendor != null)
            {
                switch (vendor.VendorType)
                {
                    default:
                        return new ServiceResponseBase(null, false, "No vendor matching the vendorType parameter was found!");
                    case VendorTypes.Avis:
                        {
                            _findeksProvider = new AvisProvider.FindeksProvider(vendor.APIBaseUrl);
                            break;
                        }
                }
            }
            var result = await _findeksProvider.FindeksPhoneIdListRequest(getPhoneIdListRequest, vendor);

            if (result is null)
                return new ServiceResponseBase { Success = false, Data = null };

            return result;
        }

        public async Task<ServiceResponseBase> FindeksPinConfirm(ConfirmPinRequest confirmPinRequest)
        {
            var vendor = await _vendorService.GetVendorById(confirmPinRequest.VendorId);
            if (vendor != null)
            {
                switch (vendor.VendorType)
                {
                    default:
                        return new ServiceResponseBase(null, false, "No vendor matching the vendorType parameter was found!");
                    case VendorTypes.Avis:
                        {
                            _findeksProvider = new AvisProvider.FindeksProvider(vendor.APIBaseUrl);
                            break;
                        }
                }
            }
            var result = await _findeksProvider.FindeksPinConfirm(confirmPinRequest, vendor);

            if (result is null)
                return new ServiceResponseBase { Success = false, Data = null };

            return result;

        }

        public async Task<ServiceResponseBase> FindeksPinRenew(CreatePinRenewRequest createPinRenewRequest)
        {
            var vendor = await _vendorService.GetVendorById(createPinRenewRequest.VendorId);
            if (vendor != null)
            {
                switch (vendor.VendorType)
                {
                    default:
                        return new ServiceResponseBase(null, false, "No vendor matching the vendorType parameter was found!");
                    case VendorTypes.Avis:
                        {
                            _findeksProvider = new AvisProvider.FindeksProvider(vendor.APIBaseUrl);
                            break;
                        }
                }
            }
            var result = await _findeksProvider.FindeksPinRenew(createPinRenewRequest, vendor);

            if (result is null)
                return new ServiceResponseBase { Success = false, Data = null };

            return result;
        }

        public async Task<ServiceResponseBase> GetFindeksReport(GetFindeksReportRequest getFindeksReportRequest)
        {
            var vendor = await _vendorService.GetVendorById(getFindeksReportRequest.VendorId);
            if (vendor != null)
            {
                switch (vendor.VendorType)
                {
                    default:
                        return new ServiceResponseBase(null, false, "No vendor matching the vendorType parameter was found!");
                    case VendorTypes.Avis:
                        {
                            _findeksProvider = new AvisProvider.FindeksProvider(vendor.APIBaseUrl);
                            break;
                        }
                }
            }
            var result = await _findeksProvider.FindeksReportRequest(getFindeksReportRequest, vendor);

            if (result is null)
                return new ServiceResponseBase { Success = false, Data = null };

            return result;
        }

        public async Task<ServiceResponseBase> GetIsActiveFindeksReportExists(GetIsActiveFindeksReportRequest getActiveFindeksReportRequest)
        {
            var vendor = await _vendorService.GetVendorById(getActiveFindeksReportRequest.VendorId);
            if (vendor != null)
            {
                switch (vendor.VendorType)
                {
                    default:
                        return new ServiceResponseBase(null, false, "No vendor matching the vendorType parameter was found!");
                    case VendorTypes.Avis:
                        {
                            _findeksProvider = new AvisProvider.FindeksProvider(vendor.APIBaseUrl);
                            break;
                        }
                }
            }
            var result = await _findeksProvider.IsActiveFindeksReportExists(getActiveFindeksReportRequest, vendor);

            if (result is null)
            {
                await SaveLog(getActiveFindeksReportRequest.Tckn, getActiveFindeksReportRequest.ReservationToken, "{@IsActiveFindeksReportExists}", null, getActiveFindeksReportRequest.ToJson(), null);
                return new ServiceResponseBase { Success = false, Data = null };
            }
            await SaveLog(getActiveFindeksReportRequest.Tckn, getActiveFindeksReportRequest.ReservationToken, "{@IsActiveFindeksReportExists}", null, getActiveFindeksReportRequest.ToJson(), result.Data.ToJson());
            return result;

        }

        public async Task<ServiceResponseBase> IsVehicleSuitableForCustomer(IsVehicleSuitableForCustomer isVehicleSuitableForCustomer)
        {
            var token = await _resTokenService.GetReservationTokenByUniqueId(isVehicleSuitableForCustomer.ReservationToken);
            if (token != null)
            {
                var isVehicleSuitable = new IsVehicleSuitableForCustomerResponse();

                isVehicleSuitable.IsSuitable = isVehicleSuitableForCustomer.FindeksReportExist.CompanySegmentList.Any(x => x.CompanySegmentId == token.VehicleClassNo);

                if (isVehicleSuitable.IsSuitable)
                {
                    isVehicleSuitable.IsRequiredYoungDriverPacked = isVehicleSuitableForCustomer.FindeksReportExist.CarGroupList.Where(x => x.CarGroupCode == token.VehicleGroupName).Select(x => x.YoungDriverPacked).FirstOrDefault();
                    await SaveLog(isVehicleSuitableForCustomer.Tckn, isVehicleSuitableForCustomer.ReservationToken, "{@IsVehicleSuitableForCustomer}", true, isVehicleSuitableForCustomer.ToJson(), "Araç müşteriye verilebilir!");
                    return new ServiceResponseBase(isVehicleSuitable, true, "Araç müşteriye verilebilir!");
                }
                await SaveLog(isVehicleSuitableForCustomer.Tckn, isVehicleSuitableForCustomer.ReservationToken, "{@IsVehicleSuitableForCustomer}", true, isVehicleSuitableForCustomer.ToJson(), "Araç müşteri için uygun değildir!");
                return new ServiceResponseBase(isVehicleSuitable, false, "Araç müşteri için uygun değildir!");
            }
            await SaveLog(isVehicleSuitableForCustomer.Tckn, isVehicleSuitableForCustomer.ReservationToken, "{@IsVehicleSuitableForCustomer}", true, isVehicleSuitableForCustomer.ToJson(), "ReservationToken hatalı!");
            return new ServiceResponseBase(null, false, "ReservationToken hatalı!");
        }
  
        public async Task SaveLog(string tckn, string reservationToken, string stepName, bool? isSuitable, string request, string response)
        {
            try
            {
                var findeksLog = new FindeksLog
                {
                    LogDate = DateTime.Now,
                    ReservationToken = reservationToken,
                    StepName = stepName,
                    Request = request,
                    Response = response,
                    Tckn = tckn,
                    IsSuitable = isSuitable
                };

                await _context.FindeksLogs.AddAsync(findeksLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Serilog.Log.Error("{@FindeksSaveLogError}", exception);
            }
        }
    }
}
