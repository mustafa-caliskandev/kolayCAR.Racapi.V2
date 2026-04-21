using KolayCAR.Broker.API.Helpers.Avis;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.AvisRequestBase;
using static KolayCAR.Broker.Domain.Models.Requests.FindeksRequestBase;
using static KolayCAR.Broker.Domain.Models.Response.AvisResponseBase;

namespace KolayCAR.Broker.API.Providers.Avis
{
    public class FindeksProvider : IFindeksProvider
    {
        private readonly AuthProvider _authProvider;
        private readonly HttpManager _httpManager;
        public FindeksProvider(string apiBaseUrl)
        {
            _httpManager = new HttpManager(apiBaseUrl);
            _authProvider = new AuthProvider(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> FindeksPhoneIdListRequest(GetPhoneIdListRequest getPhoneIdListRequest, Vendor vendor)
        {
            var token = await _authProvider.GetTokenAsync(vendor);
            if (!(token is null))
            {
                var result = await _httpManager.PostAsync2<FindeksPhoneIdListRequest, FindeksPhoneIdListResponse>(
                        requestPath: "/STFindeksService/FindeksPhoneIdListRequest",
                        headers: QueryHelper<FindeksPhoneIdListRequest>.GetHeaders(token, GetEntity(getPhoneIdListRequest, vendor), vendor.SecretKey),
                        entity: GetEntity(getPhoneIdListRequest, vendor)
                    );
                if (result?.Data?.Data?.PhoneList?.Count > 0)
                    return new ServiceResponseBase(result.Data.Data, result.Data.Result, result.Data.MessageTR);

                return new ServiceResponseBase(null, false, $"{vendor.VendorName} servisine ulaşılamadı!");
            }
            return new ServiceResponseBase(null, false, $"{vendor.VendorName} servisine ulaşılamadı - Token hatası!");
        }

        public async Task<ServiceResponseBase> FindeksPinConfirm(ConfirmPinRequest confirmPinRequest, Vendor vendor)
        {
            var token = await _authProvider.GetTokenAsync(vendor);
            if (!(token is null))
            {
                var result = await _httpManager.PostAsync2<FindeksPinConfirmRequest, FindeksPinConfirmResponse>(
                        requestPath: "/STFindeksService/FindeksPinConfirm",
                        headers: QueryHelper<FindeksPinConfirmRequest>.GetHeaders(token, GetEntity(confirmPinRequest, vendor), vendor.SecretKey),
                        entity: GetEntity(confirmPinRequest, vendor)
                    );
                if (result?.Data?.Data != null)
                    return new ServiceResponseBase(result.Data.Data, result.Data.Result, result.Data.MessageTR);

                return new ServiceResponseBase(null, false, $"{vendor.VendorName} servisine ulaşılamadı!");
            }
            return new ServiceResponseBase(null, false, $"{vendor.VendorName} servisine ulaşılamadı - Token hatası!");
        }

        public async Task<ServiceResponseBase> FindeksPinRenew(CreatePinRenewRequest createPinRenewRequest, Vendor vendor)
        {
            var token = await _authProvider.GetTokenAsync(vendor);
            if (!(token is null))
            {
                var result = await _httpManager.PostAsync2<FindeksPinRenewRequest, FindeksPinConfirmResponse>(
                      requestPath: "/STFindeksService/FindeksPinRenew",
                      headers: QueryHelper<FindeksPinRenewRequest>.GetHeaders(token, GetEntity(createPinRenewRequest, vendor), vendor.SecretKey),
                      entity: GetEntity(createPinRenewRequest, vendor)
                );
                if (result?.Data?.Data != null)
                    return new ServiceResponseBase(result.Data.Data, result.Data.Result, result.Data.MessageTR);

                return new ServiceResponseBase(null, false, $"{vendor.VendorName} servisine ulaşılamadı!");
            }
            return new ServiceResponseBase(null, false, $"{vendor.VendorName} servisine ulaşılamadı - Token hatası!");
        }

        public async Task<ServiceResponseBase> FindeksReportRequest(GetFindeksReportRequest getFindeksReportRequest, Vendor vendor)
        {
            var token = await _authProvider.GetTokenAsync(vendor);
            if (!(token is null))
            {
                var result = await _httpManager.PostAsync2<FindeksReportRequest, FindeksReportResponse>(
                        requestPath: "/STFindeksService/FindeksReportRequest",
                        headers: QueryHelper<FindeksReportRequest>.GetHeaders(token, GetEntity(getFindeksReportRequest, vendor), vendor.SecretKey),
                        entity: GetEntity(getFindeksReportRequest, vendor)
                    );
                if (result?.Data?.Data != null)
                    return new ServiceResponseBase(result.Data.Data, result.Data.Result, result.Data.MessageTR);

                return new ServiceResponseBase(null, false, $"{vendor.VendorName} servisine ulaşılamadı!");
            }
            return new ServiceResponseBase(null, false, $"{vendor.VendorName} servisine ulaşılamadı - Token hatası!");
        }

        public async Task<ServiceResponseBase> IsActiveFindeksReportExists(GetIsActiveFindeksReportRequest getActiveFindeksReportRequest, Vendor vendor)
        {
            var token = await _authProvider.GetTokenAsync(vendor);
            if (!(token is null))
            {
                var result = await _httpManager.PostAsync2<IsActiveFindeksReportExists, IsActiveFindeksReportExistsResponse>(
                      requestPath: "/STFindeksService/IsActiveFindeksReportExists",
                      headers: QueryHelper<IsActiveFindeksReportExists>.GetHeaders(token, GetEntity(getActiveFindeksReportRequest, vendor), vendor.SecretKey),
                      entity: GetEntity(getActiveFindeksReportRequest, vendor)
                    );
                if (result?.Data?.Data != null && result?.Data?.Result == true)
                    return new ServiceResponseBase(result.Data.Data, result.Data.Result, result.Data.MessageTR);
                if (result?.Data?.Data == null && result?.Data?.Result == true)
                    return new ServiceResponseBase(null, false, "Kayıtlı rapor bulunamadı!");
                if (result == null)
                    return new ServiceResponseBase(null, false, $"{vendor.VendorName} servisine ulaşılamadı");
            }
            return new ServiceResponseBase(null, false, $"{vendor.VendorName} servisine ulaşılamadı - Token hatası!");
        }
        private FindeksPhoneIdListRequest GetEntity(GetPhoneIdListRequest getPhoneIdListRequest, Vendor vendor)
        {
            return new FindeksPhoneIdListRequest() { Tckn = getPhoneIdListRequest.Tckn, LicenseNo = vendor.ApiClientId.Contains("-") ? vendor.ApiClientId.Split("-")[2].ToIntNullSafe() : 1 };
        }
        private FindeksPinConfirmRequest GetEntity(ConfirmPinRequest confirmPinRequest, Vendor vendor)
        {
            return new FindeksPinConfirmRequest() { RequestId = confirmPinRequest.RequestId, BirthYear = confirmPinRequest.BirthYear, PinCode = confirmPinRequest.PinCode, LicenseNo = vendor.ApiClientId.Contains("-") ? vendor.ApiClientId.Split("-")[2].ToIntNullSafe() : 1 };
        }
        private FindeksPinRenewRequest GetEntity(CreatePinRenewRequest createPinRenewRequest, Vendor vendor)
        {
            return new FindeksPinRenewRequest() { RequestId = createPinRenewRequest.RequestId, LicenseNo = vendor.ApiClientId.Contains("-") ? vendor.ApiClientId.Split("-")[2].ToIntNullSafe() : 1 };
        }
        private FindeksReportRequest GetEntity(GetFindeksReportRequest getFindeksReportRequest, Vendor vendor)
        {
            return new FindeksReportRequest() { Tckn = getFindeksReportRequest.Tckn, BirthDate = getFindeksReportRequest.BirthDate, DriverLicenseDate = getFindeksReportRequest.DriverLicenseDate, PhoneId = getFindeksReportRequest.PhoneId, PhoneNo = getFindeksReportRequest.PhoneNo, LicenseNo = vendor.ApiClientId.Contains("-") ? vendor.ApiClientId.Split("-")[2].ToIntNullSafe() : 1 };
        }
        private IsActiveFindeksReportExists GetEntity(GetIsActiveFindeksReportRequest getActiveFindeksReportRequest, Vendor vendor)
        {
            return new IsActiveFindeksReportExists() { Tckn = getActiveFindeksReportRequest.Tckn, LicenseNo = vendor.ApiClientId.Contains("-") ? vendor.ApiClientId.Split("-")[2].ToIntNullSafe() : 1 };
        }
    }
}
