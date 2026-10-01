using AutoMapper;
using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers;
using KolayCAR.Broker.API.Mappers.KolayCAR;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Models.Dtos;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CommonModels = KolayCAR.Broker.Domain.Models;
using KolayCARBrokerProvider = KolayCAR.Broker.API.Providers.KolayCARBroker;


namespace KolayCAR.Broker.API.Services
{
    public interface IAgencyService
    {
        Task<ServiceResponseBase> GetAgencies(GetAgenciesRequest getAgenciesRequest);
        Task<CommonModels.Agency> GetAgency(long agencyId);
        Task<IEnumerable<Models.Agency>> GetActiveAgencyList();
        //Task<string> GetAgencyType(long agencyId);
        Task<string> GetCurrentAgencyType();
        Task<CommonModels.Agency> SetAgencyPaymentOptions(CommonModels.Agency agency, CommonModels.Vendor vendor);
        UserRoles GetCurrentUserRole();
        int GetCurrentAgencyId();
        object ChechAgencyRestricted<T>(object data) where T : class;
        object ChechAgencyRestrictedList<Tsource, Tdestination>(IList<Tsource> data)
            where Tsource : class
            where Tdestination : class;
        string GetCurrentBearerToken();
        Vehicle SetVehiclePaymentOptions(Vehicle vehicle, CommonModels.Agency agency, bool getPaymentSettingsFromBroker = false);
        Task<IEnumerable<AgencyVendorDto>> GetAgencyVendors(int agencyId);
        Task<IEnumerable<AgencyVendorDto>> GetVendorsByAgencyAndLocationId(int agencyId, int locationId);
        Task<Domain.Models.Agency> GetAgencyByUsernamePassword(string username, string password);
        Task<Domain.Models.Agency> GetAgencyByCode(string agencyCode);
    }

    public class AgencyService : IAgencyService
    {
        private readonly BrokerContext _context;
        private readonly IVendorService _vendorService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private UserRoles userRole;
        private int currenctAgencyId;
        private string bearerToken;
        private readonly ICacheService _cacheService;
        private readonly string AgenciesCacheKey = "ActiveAgencyCacheKey";
        private readonly IAgencyRepository _agencyRepository;
        private readonly IResTokenService _resTokenService;

        public AgencyService(BrokerContext context, IConfigurationService configurationService, IMapper mapper, IHttpContextAccessor httpContextAccessor, IVendorService vendorService, ICacheService cacheService, IAgencyRepository agencyRepository)
        {
            _context = context;
            _vendorService = vendorService;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            var claims = _httpContextAccessor.HttpContext?.User?.Claims;
            userRole = claims?.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value.ToEnum<UserRoles>() ?? UserRoles.External;
            currenctAgencyId = claims?.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value.ToIntNullSafe() ?? 0;

            var authorizationHeader = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToStringNullSafe();
            bearerToken = !string.IsNullOrWhiteSpace(authorizationHeader) && authorizationHeader.Contains(' ')
                ? authorizationHeader.Split(' ')[1]
                : string.Empty;

            _cacheService = cacheService;
            _agencyRepository = agencyRepository;
        }

        public async Task<CommonModels.Agency> GetAgency(long agencyId)
        {
            if (CacheSettings.UseCache)
            {
                var list = await GetActiveAgencyList();
                return list.FirstOrDefault(x => x.Agencyid == agencyId)?.Map();
            }

            return (await _agencyRepository.GetByIdAsync((int)agencyId))?.Map();
        }
        public async Task<string> GetAgencyType(long agencyId)
        {
            var agency = await GetAgency(agencyId);
            return agency.AgencyName;
        }

        public async Task<string> GetCurrentAgencyType()
        {
            var agency = await GetAgency(GetCurrentAgencyId());
            return agency.AgencyName;
        }

        public async Task<CommonModels.Agency> SetAgencyPaymentOptions(CommonModels.Agency agency, CommonModels.Vendor vendor)
        {
            if (agency != null && vendor != null)
            {
                var agenycPaymentOptions = await _context.Agencyvendorpaymentoption.Where(x => x.Agencyid == agency.AgencyId && x.Vendorid == vendor.VendorId).FirstOrDefaultAsync();
                if (agenycPaymentOptions != null)
                {
                    agency.CreditCardPaymentActive = agenycPaymentOptions.Creditcardpaymentactive;
                    agency.PayAllActive = agenycPaymentOptions.Payallactive;
                    agency.AdvancePaymentActive = agenycPaymentOptions.Advancepaymentactive;
                    agency.CommissionFreePaymentActive = agenycPaymentOptions.Commissionfreepaymentactive;
                    agency.PayDeliveryActive = agenycPaymentOptions.Paydeliveryactive;
                    agency.PayAgencyActive = agenycPaymentOptions.Payagencyactive;
                    agency.OptionalRentalAdvancePaymentPercent = vendor.RentalWorkingType == VendorWorkingTypes.Commission ? vendor.ProfitMarkupDailyPrice : (float?)null;
                    agency.OptionalAdditionalProductAdvancePaymentPercent = vendor.RentalWorkingType == VendorWorkingTypes.Commission ? vendor.ProfitMarkupAdditionalProducts : (float?)null;
                    agency.OptionalOneWayFeeAdvancePaymentPercent = vendor.RentalWorkingType == VendorWorkingTypes.Commission ? vendor.ProfitMarkupOneWayFee : (float?)null;
                    agency.OneWayAmountDeliveryPayment = agenycPaymentOptions.Onewayamountdeliverypayment ?? false;
                    agency.AdditionalProductAmountDeliveryPayment = agenycPaymentOptions.Additionalproductamountdeliverypayment ?? false;
                }
            }

            return agency;
        }

        public async Task<ServiceResponseBase> GetAgencies(GetAgenciesRequest getAgenciesRequest)
        {
            try
            {
                var reservationToken = await _resTokenService.GetReservationTokenByUniqueId(getAgenciesRequest.ReservationToken);
                if (reservationToken != null)
                {
                    var agency = await GetAgency(getAgenciesRequest.AgencyId);
                    if (agency != null)
                    {
                        if (await _vendorService.GetVendorById(reservationToken.VendorId, agency, subVendorId: reservationToken.APIVendorId) is CommonModels.Vendor vendor && vendor != null)
                        {
                            await SetAgencyPaymentOptions(agency, vendor);
                            IAgencyProvider agencyProvider;

                            getAgenciesRequest.ApiKey = vendor.ApiKey;
                            getAgenciesRequest.ApiPassword = vendor.ApiPassword;
                            getAgenciesRequest.VendorType = vendor.VendorType;

                            switch (getAgenciesRequest.VendorType)
                            {
                                default:
                                    return new ServiceResponseBase
                                    {
                                        Success = false,
                                        Message = "vendorType parametresine uygun tedarikçi bulunamadı!",
                                        ServiceCode = null,
                                        ServiceMessage = null
                                    };
                                case VendorTypes.KolayCARBroker:
                                    {
                                        agencyProvider = new KolayCARBrokerProvider.AgencyProvider(vendor.APIBaseUrl);
                                        break;
                                    }
                            }

                            return await agencyProvider.GetAgency(getAgenciesRequest, vendor, reservationToken, agency);
                        }
                    }

                    return new ServiceResponseBase
                    {
                        Success = false,
                        Message = "Tedarikçi bilgisine ulaşılamadı!",
                        ServiceCode = null,
                        ServiceMessage = null
                    };
                }
                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "reservationToken hatalı!",
                    ServiceCode = null,
                    ServiceMessage = null
                };
            }

            catch (Exception ex)
            {
                Serilog.Log
                    .ForContext("GetAgenciesRequest", getAgenciesRequest)
                    .Fatal("{@GetAgenciesError}", ex.Message);

                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "Hata!",
                    ServiceCode = null,
                    ServiceMessage = null
                };
            }
        }

        public UserRoles GetCurrentUserRole() => userRole;
        public int GetCurrentAgencyId() => currenctAgencyId;

        public object ChechAgencyRestricted<T>(object data) where T : class
        {
            if (data != null)
                return userRole == UserRoles.External ? _mapper.Map<T>(data) : data;

            return data;
        }

        public object ChechAgencyRestrictedList<Tsource, Tdestination>(IList<Tsource> data)
            where Tsource : class
            where Tdestination : class
        {
            if (data != null && data.Count > 0)
                return userRole == UserRoles.External ? (object)data.Select(x => ChechAgencyRestricted<Tdestination>(x)) : (object)data;

            return data;
        }

        public string GetCurrentBearerToken() => bearerToken;

        public Vehicle SetVehiclePaymentOptions(Vehicle vehicle, CommonModels.Agency agency, bool getPaymentSettingsFromBroker)
        {
            if (agency != null && vehicle != null)
            {
                if (!getPaymentSettingsFromBroker)
                {
                    vehicle.ActivePaymentTypes = new List<PaymentTypes>();
                    if (agency.PayAllActive)
                        vehicle.ActivePaymentTypes.Add(PaymentTypes.PayAll);
                    if (agency.AdvancePaymentActive)
                        vehicle.ActivePaymentTypes.Add(PaymentTypes.AdvancePayment);
                    if (agency.CommissionFreePaymentActive)
                        vehicle.ActivePaymentTypes.Add(PaymentTypes.CommissionFree);
                    if (agency.PayDeliveryActive)
                        vehicle.ActivePaymentTypes.Add(PaymentTypes.PayOnDelivery);
                    if (agency.PayAgencyActive)
                        vehicle.ActivePaymentTypes.Add(PaymentTypes.PayToAgency);

                    if (agency.OptionalRentalAdvancePaymentPercent != null && agency.OptionalRentalAdvancePaymentPercent != 0)
                        vehicle.OptionalRentalAdvancePaymentPercent = agency.OptionalRentalAdvancePaymentPercent;
                    if (agency.OptionalAdditionalProductAdvancePaymentPercent != null && agency.OptionalAdditionalProductAdvancePaymentPercent != 0)
                        vehicle.OptionalAdditionalProductAdvancePaymentPercent = agency.OptionalAdditionalProductAdvancePaymentPercent;
                    if (agency.OptionalOneWayFeeAdvancePaymentPercent != null && agency.OptionalOneWayFeeAdvancePaymentPercent != 0)
                        vehicle.OptionalOneWayFeeAdvancePaymentPercent = agency.OptionalOneWayFeeAdvancePaymentPercent;

                    vehicle.IsAdditionalProductPricePOA = agency.AdditionalProductAmountDeliveryPayment;
                    vehicle.IsOneWayFeePOA = agency.OneWayAmountDeliveryPayment;
                }

                return vehicle;
            }

            return vehicle;
        }

        public async Task<IEnumerable<AgencyVendorDto>> GetAgencyVendors(int agencyId)
        {
            return await _context.AgencyVendorDtos.FromSqlRaw($"EXEC GETAGENCYVENDORS @agencyId = {agencyId}").ToListAsync();
        }

        public async Task<IEnumerable<Models.Agency>> GetActiveAgencyList()
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync($"{CacheSettings.AgencyKey}-ActiveAgencyList", _agencyRepository.GetActiveAgencyList);

            return await _agencyRepository.GetActiveAgencyList();
        }

        public async Task<IEnumerable<AgencyVendorDto>> GetVendorsByAgencyAndLocationId(int agencyId, int locationId)
        {
            var vendors = new List<AgencyVendorDto>();

            if (CacheSettings.UseCache)
            {
                vendors = await _cacheService.GetOrCreateAsync($"AgencyVendors-{agencyId}", () => _context.AgencyVendorDtos.FromSqlRaw($"EXEC GETAGENCYVENDORS @agencyId = {agencyId}").ToListAsync());
            }
            else
            {
                vendors = await _context.AgencyVendorDtos.FromSqlRaw($"EXEC GETAGENCYVENDORS @agencyId = {agencyId}").ToListAsync();
            }

            return vendors.Where(e => e.LocationId == locationId).ToList();
        }
        public async Task<Domain.Models.Agency> GetAgencyByUsernamePassword(string username, string password)
        {
            var agencyList = await GetActiveAgencyList();
            var agency = agencyList.Where(e => e.Agencyapikey == username && e.Agencyapipassword == password).FirstOrDefault();

            if (agency == null) return null;

            return agency.Map();
        }

        public async Task<Domain.Models.Agency> GetAgencyByCode(string agencyCode)
        {
            if (string.IsNullOrWhiteSpace(agencyCode))
                return null;

            var agencyList = await GetActiveAgencyList();
            var agency = agencyList.FirstOrDefault(x =>
                string.Equals(x.Agencycode?.Trim(), agencyCode.Trim(), StringComparison.OrdinalIgnoreCase));

            return agency?.Map();
        }
    }
}
