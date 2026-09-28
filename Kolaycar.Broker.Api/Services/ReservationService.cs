using KolayCAR.Broker.API.Extensions;
using KolayCAR.Broker.API.Factories.Abstract;
using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers;
using KolayCAR.Broker.API.Mappers.KolayCAR;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Models.Dtos;
using KolayCAR.Broker.API.Models.MobileAppDtos.RequestDtos;
using KolayCAR.Broker.API.Models.MobileAppDtos.ReservationDtos;
using KolayCAR.Broker.API.Models.MobileAppDtos.ResponseDtos;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BrokerReservationHelper = KolayCAR.Broker.API.Helpers.ReservationHelper;
using CommonModels = KolayCAR.Broker.Domain.Models;
using KolayCARBrokerProvider = KolayCAR.Broker.API.Providers.KolayCARBroker;
using UsingCouponCode = KolayCAR.Broker.Domain.Models.UsingCouponCode;

namespace KolayCAR.Broker.API.Services
{
    public interface IReservationService
    {
        Task<Reservation> GetReservation(GetReservationsRequest getReservationsRequest, bool isBrokerReservation = false);
        Task<Reservation> GetReservationByAgencyReference(string agencyReservationReference, string customerSurname, LanguageTypes? languageType = null);
        Task<Reservation> GetReservationByReservationNumberAndAgencyReference(string reservationNumber, string agencyReservationReference, LanguageTypes? languageType = null);
        Task<Reservation> GetReservationByReservationNumberAndSurname(string reservationNumber, string customerSurname, LanguageTypes? languageType = null);
        Task<Rez> GetReservationByRezNo(string rezNo);
        Task<List<Rez>> GetUpdatedReservations(DateTime startDate, DateTime endDate);
        Task<List<Reservation>> GetReservations(GetReservationsRequest getReservationsRequest);
        Task<List<Rez>> GetReservationsByEmail(string email);
        Task<List<Reservation>> GetReservations(string customerName, string customerSurname, string customerPhone, string customerEmail);
        Task<IEnumerable<BrokerLocationVehicleDetailDto>> GetAllPopularVehicles();
        Task<IEnumerable<BrokerLocationVehicleDetailDto>> GetAllPopularVehiclesByLocation(int languageId, int locationId);
        Task<ServiceResponseBase> CancelReservationLocal(PostCancelReservationRequest postCancelReservationRequest);
        Task<ServiceResponseBase> CancelReservationLocalV2(PostCancelReservationRequest postCancelReservationRequest, Reservation reservation);
        Task<ServiceResponseBase> CancelReservationService(PostCancelReservationRequest postCancelReservationRequest);
        Task<ServiceResponseBase> CancelReservationServiceV2(PostCancelReservationRequest postCancelReservationRequest, Reservation reservation);
        Task<ServiceResponseBase> PostReservationLocal(PostReservationRequest postReservationRequest, long reservationId, GetExtrasResponse getExtrasResponse, PostPaymentResponse postPaymentResponse, Vehicle vehicle);
        Task<ServiceResponseBase> PostReservationLocalV2(Domain.Models.Requests.PostReservationRequestV2 postReservationRequest, Domain.Models.Agency agency, ReservationToken reservationToken, Domain.Models.Vendor vendor, long reservationId, List<Extra> extras = null);
        Task<ServiceResponseBase> PostReservationLocalV3(PostReservationRequest postReservationRequest, long reservationId, ReservationToken reservationTokenObj, List<Extra> apiExtras);
        Task<ServiceResponseBase> PostReservationToVendorAPI(PostReservationRequest postReservationRequest, long reservationId, List<Extra> Extras);
        Task<ServiceResponseBase> PostReservationToVendorAPI(Domain.Models.Requests.PostReservationRequestV2 postReservationRequest, ReservationToken reservationToken, Domain.Models.Agency agency, Domain.Models.Vendor vendor, long reservationId, List<Extra> apiExtras);
        Reservation UpdateReservationWhenPostReservationToServiceSuccessfully(Reservation reservation);
        Reservation UpdateReservationWhenCancelReservationToServiceSuccessfully(Reservation reservation);
        Reservation UpdateReservationWhenPaymentRefundSuccessfully(Reservation reservation);
        Task PostReservationMail(Reservation reservation, bool resend = false, string toMailAddress = null);
        Task SetVendorLocalContactInformations(Reservation reservation);
        bool CheckReservationRefundStatus(Reservation reservation);
        float GetDailyPriceOfInstallmentByAgencySettings(CommonModels.Agency agency, PostReservationRequest postReservationRequest, ReservationToken reservationToken);
        Task SetVendorAddress(Reservation reservation);
        Task<ReservateNowDtoMobile> Recalculate(ReservateNowDtoMobile reservateNow);
        Task<bool> CheckReservationTokenExists(string reservationToken);
        Task<List<Reservationupdate>> GetReservationUpdatesByReservationNumber(string reservationNumber);
        Task<List<Rez>> GetCanceledReservations(DateTime startDate, DateTime endDate);
        Rez GetCanceledReservationsByRezToken(string rezToken);
        Task<Reservation> GetMappedReservationByRezNo(string rezNo);
        Task<bool> LocalCancel(CancelLocalRequest request);
        Task<bool> UpdateRecalculatedReservation(UpdateRecalculatedReservationRequest request);
    }

    public class ReservationService : IReservationService
    {
        #region Constructor & Definations
        private readonly AppSettings _appSettings;
        private readonly BrokerContext _context;
        private readonly IConfigurationService _configurationService;
        private readonly IVendorService _vendorService;
        private readonly IReservationStepsService _reservationStepsService;
        private readonly IVehicleService _vehicleService;
        private readonly IAgencyService _agencyService;
        private readonly ICouponService _couponService;
        private readonly IExtraService _extraService;
        private readonly ILocationService _locationService;
        private readonly IWebHostEnvironment _env;
        private readonly IMemoryCache _memoryCache;
        private readonly IEmailService _emailService;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ISmsService _smsService;
        private readonly IContentService _contentService;
        private readonly IConfiguration _configuration;
        private readonly IReservationTokenService _reservationTokenService;
        private readonly ICacheService _cacheService;
        private readonly IReservationProviderFactory _reservationProviderFactory;
        private readonly IBultenService _bultenService;
        private readonly IResTokenService _resTokenService;
        private readonly IParameterService _parameterService;
        private readonly IVendorOfficeService _vendorOfficeService;
        private readonly IMemberService _memberService;
        private readonly IVendorContactInformationService _vendorContactInformationService;
        private UserRoles _userRole;
        private int _currentAgencyId;
        private List<Reservationextra> ReservationExtras { get; set; }
        private List<Resstatushistory> ReservationStatusHistories { get; set; }
        private List<Resstatuslang> ReservationStatusLabels { get; set; }
        private List<Additions> ReservationAdditions { get; set; }

        public ReservationService(IOptions<AppSettings> appSettings, BrokerContext context, IConfigurationService configurationService, IAgencyService agencyService, ICouponService couponService, IExtraService extraService, ILocationService locationService, IVendorService vendorService, IReservationStepsService reservationStepsService, IVehicleService vehicleService, IContentService contentService, IWebHostEnvironment env, IMemoryCache memoryCache, IEmailService emailService, ISmsService smsService, IConfiguration configuration, IReservationTokenService reservationTokenService, ICacheService cacheService, IReservationProviderFactory reservationProviderFactory, IBultenService bultenService, IResTokenService resTokenService, IParameterService parameterService, IVendorOfficeService vendorOfficeService, IExchangeRateService exchangeRateService, IMemberService memberService, IVendorContactInformationService vendorContactInformationService)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _configurationService = configurationService;
            _vendorService = vendorService;
            _reservationStepsService = reservationStepsService;
            _agencyService = agencyService;
            _couponService = couponService;
            _extraService = extraService;
            _locationService = locationService;
            _vehicleService = vehicleService;
            _contentService = contentService;
            _userRole = _agencyService.GetCurrentUserRole();
            _currentAgencyId = _agencyService.GetCurrentAgencyId();
            _env = env;
            _memoryCache = memoryCache;
            _emailService = emailService;
            _smsService = smsService;
            _configuration = configuration;
            _reservationTokenService = reservationTokenService;
            _cacheService = cacheService;
            _reservationProviderFactory = reservationProviderFactory;
            _bultenService = bultenService;
            _resTokenService = resTokenService;
            _parameterService = parameterService;
            _vendorOfficeService = vendorOfficeService;
            _exchangeRateService = exchangeRateService;
            _memberService = memberService;
            _vendorContactInformationService = vendorContactInformationService;
        }
        #endregion

        private async Task<string> GenerateFriendlyReservationNumber()
        {
            const int maxAttempts = 20;

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                var friendlyReservationNumber = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
                var exists = await _context.Rez
                    .AsNoTracking()
                    .AnyAsync(x => x.FriendlyReservationNumber == friendlyReservationNumber);

                if (!exists)
                    return friendlyReservationNumber;
            }

            return "";
        }

        public async Task<IEnumerable<BrokerLocationVehicleDetailDto>> GetAllPopularVehicles()
        {
            return await _context.LocationVehicleDetailDtos.FromSqlRaw("EXEC GETALLPOPULARVEHICLESBYLOCATION").ToListAsync();
        }

        public async Task<IEnumerable<BrokerLocationVehicleDetailDto>> GetAllPopularVehiclesByLocation(int languageId, int locationId)
        {
            var result = await _context.LocationVehicleDetailDtos.FromSqlRaw("EXEC GETALLPOPULARVEHICLESBYLOCATION").ToListAsync();
            return result.Where(r => r.LanguageId == languageId && r.LocationId == locationId);
        }

        public async Task<List<ReservationExtra>> GetReservationExtras(string reservationNumber, int languageId, bool checkLanguage)
        {
            if (ReservationExtras == null)
                ReservationExtras = await _context.Reservationextra.ToListAsync();

            var result = ReservationExtras.Where(x => x.Resno == reservationNumber).ToList().Map();
            if (result?.Count > 0 && checkLanguage)
            {
                foreach (var item in result)
                {
                    var productId = item.ExtraId;
                    item.ExtraDescription = await _context.Additionalproduct.Where(e => e.Productid == productId && e.Langid == languageId + 1).Select(e => e.Productdescription).FirstOrDefaultAsync();
                }
            }
            return result;
        }

        public async Task<List<ReservationExtra>> GetReservationExtras(string reservationNumber)
        {
            if (ReservationExtras == null)
                ReservationExtras = await _context.Reservationextra.ToListAsync();

            return ReservationExtras.Where(x => x.Resno == reservationNumber).ToList().Map();
        }
        public async Task<List<ReservationStatusHistory>> GetReservationStatusNew(IEnumerable<Resstatushistory> reservationStatusHistories, IEnumerable<Resstatuslang> reservationStatusLabels)
        {
            ReservationStatusHistories = reservationStatusHistories.ToList();
            return ReservationStatusHistories.Map(ReservationStatusLabels);
        }

        public async Task<List<ReservationStatusHistory>> GetReservationStatus(string reservationNumber, Domain.Models.LanguageTypes languageType)
        {
            ReservationStatusHistories = await _context.Resstatushistory.Where(x => x.Resno == reservationNumber).ToListAsync();
            ReservationStatusLabels = await _context.Resstatuslang.Where(x => x.Langid == (int)languageType + 1).ToListAsync();
            return ReservationStatusHistories.Map(ReservationStatusLabels);
        }

        public async Task<List<Addition>> GetReservationAdditions(string reservationNumber)
        {
            ReservationAdditions = await _context.Additions.Where(x => x.Reservationnumber == reservationNumber && x.Additionstatustype == (int)ReservationAdditionStatusTypes.Added).ToListAsync();
            return ReservationAdditions.Map();
        }

        public async Task GetReservationUpdates(Reservation reservation)
        {
            var updates = await _context.Reservationupdate.Where(x => x.Reservationid == reservation.ReservationId).ToListAsync();
            if (updates != null && updates.Count > 0)
            {
                reservation.UpdateCount = updates.GroupBy(x => x.Uniqueid).Count();
                reservation.LastUpdateDate = updates.OrderByDescending(x => x.Id).ToList()[0].Updatedate;
            }
            else
                reservation.UpdateCount = 0;
        }

        public async Task<List<Reservationupdate>> GetReservationUpdatesByReservationNumber(string reservationNumber)
        {
            return await _context.Reservationupdate.Where(x => x.Reservatinonumber == reservationNumber).ToListAsync();
        }

        public async Task<Reservation> GetReservation(GetReservationsRequest getReservationsRequest, bool isBrokerReservation = false)
        {
            if (!string.IsNullOrWhiteSpace(getReservationsRequest.CustomerEmail))
                getReservationsRequest.CustomerEmail = getReservationsRequest.CustomerEmail.Trim().ToLowerInvariant();

            var sqlParameters = SqlParameterHelper.GetReservationSqlParameters(getReservationsRequest, _userRole, _currentAgencyId, isBrokerReservation);

            var reservation = await _context.Rez.FromSqlRaw("EXECUTE SP_AGENCY_OPERATIONS " +
                "@OPERATIONID = @OPERATIONID," +
                "@AGENCYID = @AGENCYID," +
                "@RESERVATIONNO = @RESERVATIONNO," +
                "@USEREMAIL = @USEREMAIL," +
                "@BROKERCANCELRESERVATION = @BROKERCANCELRESERVATION", sqlParameters).ToListAsync();

            if (reservation.Count > 0)
            {
                var mappedReservation = reservation[0].Map();
                LanguageTypes requestLanguageType = getReservationsRequest.LanguageType != null ? (LanguageTypes)getReservationsRequest.LanguageType : mappedReservation.LanguageType;
                var checkLanguage = getReservationsRequest.LanguageType != null;
                var reservationExtras = await GetReservationExtras(mappedReservation.ReservationNumber, (int)requestLanguageType, checkLanguage);
                var reservationStatus = await GetReservationStatus(mappedReservation.ReservationNumber, requestLanguageType);
                var reservationAddition = await GetReservationAdditions(mappedReservation.ReservationNumber);
                await GetReservationUpdates(mappedReservation);

                string fuelTypeName = await _vehicleService.GetFuelName(requestLanguageType, mappedReservation.FuelType);
                string transmissionTypeName = await _vehicleService.GetTransmissionName(requestLanguageType, mappedReservation.TransmissionType);
                string vehicleTypeName = await _vehicleService.GetVehicleTypeName(requestLanguageType, mappedReservation.VehicleType);
                string vehicleCategoryTypeName = await _vehicleService.GetVehicleCategoryTypeName(requestLanguageType, mappedReservation.VehicleCategoryType);
                string passangerQuantityTypeName = await _vehicleService.GetPassangerQuantityTypeName(requestLanguageType, mappedReservation.PassangerQuantityType);
                string baggageQuantityTypeName = await _vehicleService.GetBaggageQuantityTypeName(requestLanguageType, mappedReservation.BaggageQuantityType);
                string reservationSourceName = await GetReservationSourceName(mappedReservation.ReservationSourceId);
                mappedReservation.VehicleDescription
                    = await _vehicleService.GetVehicleDescription(mappedReservation.VehicleCode, requestLanguageType);
                var vendorScoreDto = await GetReservationVendorSurveyStatics(mappedReservation.VendorId, mappedReservation.PickupLocationId, (int)mappedReservation.LanguageType);
                var vendorScore = GetReservationVendorScore(vendorScoreDto);
                var commentCount = GetReservationVendorCommentCount(vendorScoreDto);

                var dataLayer = await _context.DataLayers.FromSqlRaw($"EXEC GETRESERVATIONDATALAYERINFO @RESERVATIONNUMBER = '{mappedReservation.ReservationNumber.ToString()}'").ToListAsync();

                mappedReservation.ReservationExtras = reservationExtras;
                mappedReservation.ReservationStatusHistories = reservationStatus;
                mappedReservation.Additions = reservationAddition;
                mappedReservation.FuelTypeName = fuelTypeName;
                mappedReservation.TransmissionTypeName = transmissionTypeName;
                mappedReservation.VehicleTypeName = vehicleTypeName;
                mappedReservation.VehicleCategoryTypeName = vehicleCategoryTypeName;
                mappedReservation.PassangerQuantityTypeName = passangerQuantityTypeName;
                mappedReservation.BaggageQuantityTypeName = baggageQuantityTypeName;
                mappedReservation.ReservationSourceName = reservationSourceName;
                mappedReservation.VendorScore = vendorScore;
                mappedReservation.CommentCount = commentCount;
                mappedReservation.IsAirport = mappedReservation.ReservationToken.IsAirport.ToBoolNullSafe();

                if (dataLayer != null && dataLayer.Count > 0 && dataLayer[0] != null)
                {
                    mappedReservation.CityOfPickupLocation = dataLayer[0].CityOfPickupLocation;
                    mappedReservation.CityOfReturnLocation = dataLayer[0].CityOfReturnLocation;
                    mappedReservation.VehicleBrandName = dataLayer[0].VehicleBrandName;
                    mappedReservation.VehicleModelName = dataLayer[0].VehicleModelName;
                }
                return mappedReservation;
            }
            return null;
        }

        public async Task<Reservation> GetReservationByAgencyReference(
            string agencyReservationReference,
            string customerSurname,
            LanguageTypes? languageType = null)
        {
            if (string.IsNullOrWhiteSpace(agencyReservationReference) || string.IsNullOrWhiteSpace(customerSurname))
                return null;

            var normalizedReference = agencyReservationReference.Trim();
            var normalizedCustomerSurname = customerSurname.Trim();
            var reservationQuery = _context.Rez
                .AsNoTracking()
                .Where(x => EF.Functions.Collate(x.Agencyreservationreference, "Latin1_General_100_CI_AS") == normalizedReference &&
                            EF.Functions.Collate(x.Musterisoyad, "Latin1_General_100_CI_AS") == normalizedCustomerSurname);

            if (_userRole != UserRoles.Admin && _userRole != UserRoles.MobileAPP)
                reservationQuery = reservationQuery.Where(x => x.Agencyid == _currentAgencyId);

            var reservation = await reservationQuery
                .OrderByDescending(x => x.Tarih)
                .FirstOrDefaultAsync();

            if (reservation == null)
                return null;

            return await GetReservation(
                new GetReservationsRequest
                {
                    ReservationNumber = reservation.Rezno,
                    CustomerEmail = reservation.Musterieposta,
                    LanguageType = languageType
                },
                isBrokerReservation: false);
        }

        public async Task<Reservation> GetReservationByReservationNumberAndSurname(
            string reservationNumber,
            string customerSurname,
            LanguageTypes? languageType = null)
        {
            if (string.IsNullOrWhiteSpace(reservationNumber) || string.IsNullOrWhiteSpace(customerSurname))
                return null;

            var normalizedReservationNumber = reservationNumber.Trim();
            var normalizedCustomerSurname = customerSurname.Trim();
            var reservationQuery = _context.Rez
                .AsNoTracking()
                .Where(x => EF.Functions.Collate(x.Rezno, "Latin1_General_100_CI_AS") == normalizedReservationNumber &&
                            EF.Functions.Collate(x.Musterisoyad, "Latin1_General_100_CI_AS") == normalizedCustomerSurname);

            if (_userRole != UserRoles.Admin && _userRole != UserRoles.MobileAPP)
                reservationQuery = reservationQuery.Where(x => x.Agencyid == _currentAgencyId);

            var reservation = await reservationQuery.FirstOrDefaultAsync();

            if (reservation == null)
                return null;

            return await GetReservation(
                new GetReservationsRequest
                {
                    ReservationNumber = reservation.Rezno,
                    CustomerEmail = reservation.Musterieposta,
                    LanguageType = languageType
                },
                isBrokerReservation: false);
        }

        public async Task<Reservation> GetReservationByReservationNumberAndAgencyReference(
            string reservationNumber,
            string agencyReservationReference,
            LanguageTypes? languageType = null)
        {
            if (string.IsNullOrWhiteSpace(reservationNumber) || string.IsNullOrWhiteSpace(agencyReservationReference))
                return null;

            var normalizedReservationNumber = reservationNumber.Trim();
            var normalizedAgencyReference = agencyReservationReference.Trim();
            var reservationQuery = _context.Rez
                .AsNoTracking()
                .Where(x => EF.Functions.Collate(x.Rezno, "Latin1_General_100_CI_AS") == normalizedReservationNumber &&
                            EF.Functions.Collate(x.Agencyreservationreference, "Latin1_General_100_CI_AS") == normalizedAgencyReference);

            reservationQuery = reservationQuery.Where(x => x.Agencyid == _currentAgencyId);

            var reservation = await reservationQuery.FirstOrDefaultAsync();
            if (reservation == null)
                return null;

            return await GetReservation(
                new GetReservationsRequest
                {
                    ReservationNumber = reservation.Rezno,
                    CustomerEmail = reservation.Musterieposta,
                    LanguageType = languageType
                },
                isBrokerReservation: false);
        }

        public async Task<Rez> GetReservationByRezNo(string rezNo)
        {
            return await _context.Rez.FirstOrDefaultAsync(x => x.Rezno == rezNo);
        }

        public async Task<List<Rez>> GetUpdatedReservations(DateTime startDate, DateTime endDate)
        {
            var updateHistory = _context.Reservationupdate
                                     .Where(u => u.Updatedate >= startDate && u.Updatedate <= endDate)
                                     .Select(u => u.Reservatinonumber);
            var stateHistory = _context.Resstatushistory
                                     .Where(u => u.Inserteddate >= startDate && u.Inserteddate <= endDate && u.Resstatusid == -4 && u.Userid != null && u.Userid == 123518)
                                     .Select(u => u.Resno);

            return await _context.Rez
                                 .Where(r => updateHistory.Contains(r.Rezno) || stateHistory.Contains(r.Rezno))
                                 .ToListAsync();
        }

        public async Task<Reservation> GetMappedReservationByRezNo(string rezNo)
        {
            var reservation = await _context.Rez.FirstOrDefaultAsync(x => x.Rezno == rezNo);
            var reservationExtras = await _context.Reservationextra.Where(e => e.Resno == rezNo).ToListAsync();
            return reservation.Map(reservationExtras.Map());
        }
        public async Task<List<Reservation>> GetReservations(GetReservationsRequest getReservationsRequest)
        {
            var sqlParameters = SqlParameterHelper.GetReservationsSqlParameters(getReservationsRequest, _userRole, _currentAgencyId);

            var reservations = await _context.Rez.FromSqlRaw("EXECUTE GETRESERVATIONS " +
                "@AGENCYID = @AGENCYID, " +
                "@MEMBERID = @MEMBERID," +
                "@RESERVATIONSTARTDATE = @RESERVATIONSTARTDATE, " +
                "@RESERVATIONENDDATE = @RESERVATIONENDDATE, " +
                "@RESERVATIONRETURNSTARTDATE = @RESERVATIONRETURNSTARTDATE, " +
                "@RESERVATIONRETURNENDDATE = @RESERVATIONRETURNENDDATE, " +
                "@RESERVATIONDATEFIRST = @RESERVATIONDATEFIRST, " +
                "@RESERVATIONDATESECOND = @RESERVATIONDATESECOND," +
                "@RESERVATIONNO = @RESERVATIONNO," +
                "@APIRESERVATIONNO = @APIRESERVATIONNO," +
                "@RESERVATIONINFO = @RESERVATIONINFO," +
                "@AGENCYNAMEFORSEARCH = @AGENCYNAMEFORSEARCH", sqlParameters).ToListAsync();

            if (reservations.Count > 0)
            {
                var mappedReservations = reservations.Map();
                var vehicleFuelTypes = await _context.Vehiclefuellang.ToListAsync();
                var vehicleTransmissionTypes = await _context.Vehicletransmissionlang.ToListAsync();
                var vehicleTypes = await _context.Vehicletypelang.ToListAsync();
                var vehicleCategoryTypes = await _context.Vehiclecategorylang.ToListAsync();
                var passangerQuantityTypes = await _context.Vehiclepersonlang.ToListAsync();
                var baggageQuantityTypes = await _context.Vehiclebaggagelang.ToListAsync();

                //var reznos = mappedReservations.Select(e => e.ReservationNumber);

                //var ReservationStatusHistories = await _context.Resstatushistory.Where(e=> reznos.Contains(e.Resno)).ToListAsync();
                //var ReservationStatusLabels = await _context.Resstatuslang.ToListAsync();
                foreach (var reservation in mappedReservations)
                {
                    LanguageTypes requestLanguageType = getReservationsRequest.LanguageType != null ? (LanguageTypes)getReservationsRequest.LanguageType : reservation.LanguageType;
                    var checkLanguage = getReservationsRequest.LanguageType != null;
                    var reservationExtras = await GetReservationExtras(reservation.ReservationNumber, (int)requestLanguageType, checkLanguage);
                    var reservationStatus = await GetReservationStatus(reservation.ReservationNumber, requestLanguageType);
                    //var reservationStatus = await GetReservationStatusNew(ReservationStatusHistories.Where(e=> e.Resno == reservation.ReservationNumber), ReservationStatusLabels.Where(e=> e.Langid == (int)requestLanguageType + 1));
                    string reservationSourceName = await GetReservationSourceName(reservation.ReservationSourceId);
                    var detailPageContentUrl = await GetReservationDetailPageContentUrl((int)reservation.LanguageType + 1);

                    reservation.DetailPageContentUrl = detailPageContentUrl;
                    reservation.ReservationExtras = reservationExtras;
                    reservation.ReservationStatusHistories = reservationStatus;
                    await GetReservationUpdates(reservation);

                    reservation.FuelTypeName = _vehicleService.GetFuelName(vehicleFuelTypes, requestLanguageType, reservation.FuelType);
                    reservation.TransmissionTypeName = _vehicleService.GetTransmissionName(vehicleTransmissionTypes, requestLanguageType, reservation.TransmissionType);
                    reservation.VehicleTypeName = _vehicleService.GetVehicleTypeName(vehicleTypes, requestLanguageType, reservation.VehicleType);
                    reservation.VehicleCategoryTypeName = _vehicleService.GetVehicleCategoryTypeName(vehicleCategoryTypes, requestLanguageType, reservation.VehicleCategoryType);
                    reservation.PassangerQuantityTypeName = _vehicleService.GetPassangerQuantityTypeName(passangerQuantityTypes, requestLanguageType, reservation.PassangerQuantityType);
                    reservation.BaggageQuantityTypeName = _vehicleService.GetBaggageQuantityTypeName(baggageQuantityTypes, requestLanguageType, reservation.BaggageQuantityType);
                    reservation.ReservationSourceName = reservationSourceName;
                }
                return mappedReservations;
            }
            return null;
        }

        public async Task<List<Reservation>> GetReservations(string customerName, string customerSurname, string customerPhone, string customerEmail)
        {
            var sqlParameters = SqlParameterHelper.GetReservationsNewFilterSqlParameters(customerName, customerSurname, customerPhone, customerEmail);
            try
            {
                var reservations = await _context.Rez.FromSqlRaw("EXECUTE GETRESERVATIONSNEWFILTER " +
                    "@customerName = @customerName, " +
                    "@customerSurname = @customerSurname," +
                    "@customerPhone = @customerPhone, " +
                    "@customerEmail = @customerEmail ", sqlParameters).ToListAsync();

                if (reservations.Count > 0)
                {
                    var mappedReservations = reservations.Map();
                    var vehicleFuelTypes = await _context.Vehiclefuellang.ToListAsync();
                    var vehicleTransmissionTypes = await _context.Vehicletransmissionlang.ToListAsync();
                    var vehicleTypes = await _context.Vehicletypelang.ToListAsync();
                    var vehicleCategoryTypes = await _context.Vehiclecategorylang.ToListAsync();
                    var passangerQuantityTypes = await _context.Vehiclepersonlang.ToListAsync();
                    var baggageQuantityTypes = await _context.Vehiclebaggagelang.ToListAsync();

                    foreach (var reservation in mappedReservations)
                    {
                        var reservationExtras = await GetReservationExtras(reservation.ReservationNumber);
                        var reservationStatus = await GetReservationStatus(reservation.ReservationNumber, LanguageTypes.TR);
                        string reservationSourceName = await GetReservationSourceName(reservation.ReservationSourceId);
                        var detailPageContentUrl = await GetReservationDetailPageContentUrl((int)reservation.LanguageType + 1);

                        reservation.DetailPageContentUrl = detailPageContentUrl;
                        reservation.ReservationExtras = reservationExtras;
                        reservation.ReservationStatusHistories = reservationStatus;
                        await GetReservationUpdates(reservation);

                        reservation.FuelTypeName = _vehicleService.GetFuelName(vehicleFuelTypes, LanguageTypes.TR, reservation.FuelType);
                        reservation.TransmissionTypeName = _vehicleService.GetTransmissionName(vehicleTransmissionTypes, LanguageTypes.TR, reservation.TransmissionType);
                        reservation.VehicleTypeName = _vehicleService.GetVehicleTypeName(vehicleTypes, LanguageTypes.TR, reservation.VehicleType);
                        reservation.VehicleCategoryTypeName = _vehicleService.GetVehicleCategoryTypeName(vehicleCategoryTypes, LanguageTypes.TR, reservation.VehicleCategoryType);
                        reservation.PassangerQuantityTypeName = _vehicleService.GetPassangerQuantityTypeName(passangerQuantityTypes, LanguageTypes.TR, reservation.PassangerQuantityType);
                        reservation.BaggageQuantityTypeName = _vehicleService.GetBaggageQuantityTypeName(baggageQuantityTypes, LanguageTypes.TR, reservation.BaggageQuantityType);
                        reservation.ReservationSourceName = reservationSourceName;
                    }
                    return mappedReservations;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@GetReservationNewFilter}", ex.Message);
                return null;
            }
            return null;
        }
        public async Task<ServiceResponseBase> CancelReservationLocalV2(PostCancelReservationRequest postCancelReservationRequest, Reservation reservation)
        {
            try
            {
                var agency = await _agencyService.GetAgency(reservation.AgencyId.ToLongNullSafe());
                var reservationCancelletionPenalty = await GetReservationCancelletionPenalty(reservation, agency, postCancelReservationRequest.PenaltyStatus);
                var mobilCanCancelVoucher = await _configurationService.GetConfigurationValueByFieldName<bool>("MobileCanCancelVoucher");
                Serilog.Log.Error("{@PostCancelReservationRequest}", postCancelReservationRequest);

                if (reservation != null)
                {
                    if (_userRole == UserRoles.Admin || _currentAgencyId == reservation.AgencyId || postCancelReservationRequest.IsKpanelAdmin
                        || (mobilCanCancelVoucher && (_userRole == UserRoles.MobileAPP || _userRole == UserRoles.User)))
                    {
                        var sqlParameters = SqlParameterHelper.CancelLocalReservationSqlParameters(postCancelReservationRequest, reservationCancelletionPenalty);
                        var localCancelReservationResult = await _context.Database.ExecuteSqlRawAsync("EXECUTE SP_AGENCY_OPERATIONS " +
                            "@OPERATIONID = @OPERATIONID," +
                            "@RESERVATIONNO = @RESERVATIONNO," +
                            "@USEREMAIL = @USEREMAIL," +
                            "@RESSTATUSNOTE = @RESSTATUSNOTE," +
                            "@BROKERCANCELRESERVATION = @BROKERCANCELRESERVATION," +
                            "@CANCELLATIONPENALTYRATE = @CANCELLATIONPENALTYRATE," +
                            "@CANCELLATIONREFUNDEDAMOUNT = @CANCELLATIONREFUNDEDAMOUNT," +
                            "@CANCELLATIONPENALTYHOUR = @CANCELLATIONPENALTYHOUR," +
                            "@PenaltyStatus = @PenaltyStatus," +
                            "@PENALTYAMOUNT = @PENALTYAMOUNT," +
                            "@CancelReasonId = @CancelReasonId," +
                            "@UserId = @UserId", sqlParameters);

                        return new ServiceResponseBase(localCancelReservationResult > 0 ? reservation : null, localCancelReservationResult > 0, localCancelReservationResult > 0 ? "Reservation canceled!" : "An error occurred during the request!");
                    }
                }
                Serilog.Log.Error("No reservation found!");
                return new ServiceResponseBase(null, false, "No reservation found!");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@CancelReservationLocalError}", ex.ToJson());
                return new ServiceResponseBase(null, false, "The reservation could not be cancelled!");
            }

        }

        public async Task<ServiceResponseBase> CancelReservationLocal(PostCancelReservationRequest postCancelReservationRequest)
        {
            try
            {
                var getReservationsRequest = new GetReservationsRequest
                {
                    ReservationNumber = postCancelReservationRequest.ReservationNumber,
                    CustomerEmail = postCancelReservationRequest.CustomerEmail
                };

                var reservation = await GetReservation(getReservationsRequest, postCancelReservationRequest.IsBrokerReservation);
                var agency = await _agencyService.GetAgency(reservation.AgencyId.ToLongNullSafe());
                var reservationCancelletionPenalty = await GetReservationCancelletionPenalty(reservation, agency, postCancelReservationRequest.PenaltyStatus);
                var mobilCanCancelVoucher = await _configurationService.GetConfigurationValueByFieldName<bool>("MobileCanCancelVoucher");
                Serilog.Log.Error("{@PostCancelReservationRequest}", postCancelReservationRequest);

                if (reservation != null)
                {
                    if (_userRole == UserRoles.Admin || _currentAgencyId == reservation.AgencyId || postCancelReservationRequest.IsKpanelAdmin
                        || (mobilCanCancelVoucher && (_userRole == UserRoles.MobileAPP || _userRole == UserRoles.User)))
                    {
                        var sqlParameters = SqlParameterHelper.CancelLocalReservationSqlParameters(postCancelReservationRequest, reservationCancelletionPenalty);
                        var localCancelReservationResult = await _context.Database.ExecuteSqlRawAsync("EXECUTE SP_AGENCY_OPERATIONS " +
                            "@OPERATIONID = @OPERATIONID," +
                            "@RESERVATIONNO = @RESERVATIONNO," +
                            "@USEREMAIL = @USEREMAIL," +
                            "@RESSTATUSNOTE = @RESSTATUSNOTE," +
                            "@BROKERCANCELRESERVATION = @BROKERCANCELRESERVATION," +
                            "@CANCELLATIONPENALTYRATE = @CANCELLATIONPENALTYRATE," +
                            "@CANCELLATIONREFUNDEDAMOUNT = @CANCELLATIONREFUNDEDAMOUNT," +
                            "@CANCELLATIONPENALTYHOUR = @CANCELLATIONPENALTYHOUR," +
                            "@PenaltyStatus = @PenaltyStatus," +
                            "@PENALTYAMOUNT = @PENALTYAMOUNT," +
                            "@CancelReasonId = @CancelReasonId," +
                            "@UserId = @UserId", sqlParameters);

                        reservation = await GetReservation(getReservationsRequest, postCancelReservationRequest.IsBrokerReservation);

                        return new ServiceResponseBase(localCancelReservationResult > 0 ? reservation : null, localCancelReservationResult > 0, localCancelReservationResult > 0 ? "Reservation canceled!" : "An error occurred during the request!");
                    }
                }
                Serilog.Log.Error("No reservation found!");
                return new ServiceResponseBase(null, false, "No reservation found!");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@CancelReservationLocalError}", ex.ToJson());
                return new ServiceResponseBase(null, false, "The reservation could not be cancelled!");
            }
        }
        public async Task<ServiceResponseBase> CancelReservationServiceV2(PostCancelReservationRequest postCancelReservationRequest, Reservation reservation)
        {
            var vendor = await _context.Vendor.FindAsync(reservation.VendorId);
            var vendorType = await _context.Vendortype.SingleOrDefaultAsync(x => x.Vendortypeid == vendor.Vendortype);

            var mappedVendor = vendor.Map(encrypt: false);

            if (vendorType.Iscancelable)
            {
                var reservationProvider = _reservationProviderFactory.CreateReservationProvider(mappedVendor, _configurationService, _configuration, _memoryCache, _cacheService);

                if (reservationProvider is null)
                    return new ServiceResponseBase(null, false, "Tedarikçi tipi bulunamadı!");

                var cancelResponse = await reservationProvider.PostCancelReservation(postCancelReservationRequest, mappedVendor, reservation);

                await UpdateCancelAttempt(reservation.ReservationNumber);

                return cancelResponse;
            }
            return new ServiceResponseBase(null, false, "Bu rezervasyon iptal edilemez!");
        }

        public async Task<ServiceResponseBase> CancelReservationService(PostCancelReservationRequest postCancelReservationRequest)
        {
            var getReservationsRequest = new GetReservationsRequest
            {
                ReservationNumber = postCancelReservationRequest.ReservationNumber,
                CustomerEmail = postCancelReservationRequest.CustomerEmail
            };

            var reservation = await GetReservation(getReservationsRequest, postCancelReservationRequest.IsBrokerReservation);
            var vendor = await _context.Vendor.FindAsync(reservation.VendorId);
            var vendorType = await _context.Vendortype.SingleOrDefaultAsync(x => x.Vendortypeid == vendor.Vendortype);

            var mappedVendor = vendor.Map(encrypt: false);

            if (vendorType.Iscancelable)
            {
                var reservationProvider = _reservationProviderFactory.CreateReservationProvider(mappedVendor, _configurationService, _configuration, _memoryCache, _cacheService);

                if (reservationProvider is null)
                    return new ServiceResponseBase(null, false, "Tedarikçi tipi bulunamadı!");

                var cancelResponse = await reservationProvider.PostCancelReservation(postCancelReservationRequest, mappedVendor, reservation);

                await UpdateCancelAttempt(reservation.ReservationNumber);

                return cancelResponse;
            }
            return new ServiceResponseBase(null, false, "Bu rezervasyon iptal edilemez!");
        }

        public async Task<ServiceResponseBase> PostReservationLocal(PostReservationRequest postReservationRequest, long reservationId, GetExtrasResponse getExtrasResponse, PostPaymentResponse postPaymentResponse, Vehicle vehicle = null)
        {
            var reservationToken = await _resTokenService.GetReservationTokenByUniqueId(postReservationRequest.ReservationToken);
            if (reservationToken != null)
            {
                var agency = await _agencyService.GetAgency(reservationToken.AgencyId.ToLongNullSafe());
                if (agency != null)
                {
                    Serilog.Log.Error("{@ReservationLocalAgency}", agency);

                    var vendor = await _vendorService.GetVendorById(reservationToken.VendorId, agency, subVendorId: reservationToken.APIVendorId);
                    if (vendor != null)
                    {
                        ReservationHelper.FillPostReservationRequest(postReservationRequest, reservationToken, agency);

                        await _agencyService.SetAgencyPaymentOptions(agency, vendor);
                        Serilog.Log.Error("{@ReservationLocalVendor}", vendor);

                        var sendAvailabilityRequest = vendor.SendAvailabilityRequest || (agency.UserRole == UserRoles.External && !(agency.AgencyName.Contains("Airtuerk") || agency.AgencyName.Contains("Tatil")));

                        string originalExtraList = postReservationRequest.ExtraList;

                        var getExtrasRequest = new GetExtrasRequest
                        {
                            ReservationToken = postReservationRequest.ReservationToken.ToString(),
                            LanguageCode = postReservationRequest.LanguageCode,
                            CurrencyCode = reservationToken.CurrencyType.ToString()
                        };

                        //Seçilen ek ürünlerin satış fiyatları ve bilgilerini okumak için servis çağrılıyor
                        var extraServiceResponse = (!string.IsNullOrEmpty(postReservationRequest.ExtraList) && sendAvailabilityRequest) ?
                           await _extraService.GetExtras(getExtrasRequest, isReservationStep: true) :
                           new ServiceResponseBase(new GetExtrasResponse { Extras = new List<Extra>() }, true);

                        var getExtrasResponseTemp = extraServiceResponse.Data as GetExtrasResponse;

                        var selectedReservationExtras = ReservationHelper.GetSelectedReservationExtras(getExtrasResponseTemp.Extras, postReservationRequest.ExtraList);
                        postReservationRequest.ExtraList = ReservationHelper.ReservationExtraToStringList(selectedReservationExtras);
                        postReservationRequest.ExtraAmount = ReservationHelper.GetTotalExtraAmount(selectedReservationExtras, reservationToken.RentalDuration);
                        float packetPricePremium = 0;
                        string tempExtraList = postReservationRequest.ExtraList;


                        var extraIdList = (List<int>)(sendAvailabilityRequest ? getExtrasResponseTemp.Extras.Select(e => e.ExtraId).ToList()
                            : originalExtraList
                                    .Split('|', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(e =>
                                    {
                                        var parts = e.Split('~');
                                        return parts.Length > 0 ? parts[0].ToInt() : 0;
                                    })
                                    .Where(id => id > 0) // geçersizleri ele
                                    .ToList());

                        var premiumPackets = new List<Extra>();
                        if (!string.IsNullOrEmpty(postReservationRequest.ExtraList) || !string.IsNullOrEmpty(originalExtraList))
                            premiumPackets = await _extraService.GetPremiumPackets(reservationToken, extraIdList);

                        if (!string.IsNullOrEmpty(postReservationRequest.ExtraList) &&
                                (vendor.VendorType != VendorTypes.KolayCARBroker || !vendor.UseBrokerConfigurations) &&
                                sendAvailabilityRequest)
                        {
                            var vendorExtras = await _context.Additionalproductvendor.Where(e => e.Vendorid == vendor.VendorId && e.Active == true).ToListAsync();

                            Serilog.Log.Error("{@ReservationLocalVendorExtras}", vendorExtras);

                            var extraListItems = postReservationRequest.ExtraList.Split('|', StringSplitOptions.RemoveEmptyEntries);

                            var apiExtraListMapped = new List<string>();

                            foreach (var item in extraListItems)
                            {
                                var parts = item.Split('~');
                                var extraId = parts[0].ToIntNullSafe();
                                var quantity = parts[1];
                                var price = parts[2];
                                var other = parts[3];
                                var extraCode = parts.Length > 4 ? parts[4] : string.Empty;

                                var dbExtra = vendorExtras.FirstOrDefault(x => x.Apiproductcode == extraCode);
                                string newApiExtra = string.Empty;

                                // Premium paket kontrolü
                                var premiumPacket = premiumPackets.FirstOrDefault(e => e.ExtraId == extraId);
                                if (premiumPacket != null)
                                {
                                    var packetPrice = (float)premiumPacket.Price;
                                    var agencyAmount = agency.AgencyCommissionAmount > 0
                                        ? packetPrice - (packetPrice * agency.AgencyCommissionAmount / 100)
                                        : packetPrice;

                                    newApiExtra = $"{(dbExtra?.Apiproductcode ?? premiumPacket.ExtraCode)}~{quantity}~{agencyAmount}~{other}~{premiumPacket.ExtraId}~{packetPrice}";

                                    packetPricePremium += packetPrice;
                                }
                                else if (dbExtra != null)
                                {
                                    // Normal paket
                                    newApiExtra = $"{dbExtra.Apiproductcode}~{quantity}~{price}~{other}~{dbExtra.Productid}~{price}";
                                }

                                if (!string.IsNullOrEmpty(newApiExtra))
                                    apiExtraListMapped.Add(newApiExtra);
                            }

                            postReservationRequest.ExtraList = string.Join("|", apiExtraListMapped);
                        }

                        if (!string.IsNullOrEmpty(tempExtraList) && sendAvailabilityRequest)
                        {
                            var localExtras = tempExtraList.Split('|');
                            for (int i = 0; i < localExtras.Length; i++)
                            {
                                foreach (var extra in getExtrasResponse.Extras)
                                {
                                    var extraCode = localExtras[i].Split('~')[4];
                                    var extraId = localExtras[i].Split('~')[0].ToIntNullSafe();
                                    var premiumPrice = localExtras[i].Split('~')[2].ToFloatNullSafe();
                                    if (extraCode == extra.ExtraCode)
                                    {
                                        var isPremiumPacket = premiumPackets.Any(e => e.ExtraId == extraId);
                                        extra.ExtraType = isPremiumPacket ? AdditionalProductTypes.Premium : getExtrasResponse.Extras.Where(e => e.ExtraCode == extraCode).FirstOrDefault()?.ExtraType ?? AdditionalProductTypes.Extra;
                                        extra.Price = isPremiumPacket ? premiumPrice : getExtrasResponse.Extras.Where(e => e.ExtraCode == extraCode).FirstOrDefault()?.Price ?? 0;
                                        if (extra.ExtraType == AdditionalProductTypes.Premium)
                                        {
                                            //acente komisyonuna göre işlem
                                            var extraProps = localExtras[i].Split('~');

                                            var agencyAmount = (agency.AgencyCommissionAmount > 0
                                                    ? extra.Price - extra.Price * agency.AgencyCommissionAmount / 100
                                                    : extra.Price).ToString().Replace(',', '.');
                                            extraProps[2] = extra.Price.ToString().Replace(',', '.');

                                            localExtras[i] = string.Join('~', extraProps);

                                            //extra.Price = 0;
                                            localExtras[i] = $"{localExtras[i]}~{extra.ApiPrice.ToString().Replace(",", ".")}~{agencyAmount}";
                                        }
                                        else if (vendor.AdditionalProductWorkingType == VendorWorkingTypes.Commission)
                                        {
                                            float extraPrice = extra.Price;
                                            extra.Price = CalculationHelper.ExtractCommission(vendor.ProfitMarkupAdditionalProducts, vendor.PriceRoundingType, extra.Price);
                                            localExtras[i] = $"{localExtras[i]}~{extra.Price.ToString().Replace(",", ".")}~{extraPrice.ToString().Replace(",", ".")}";
                                        }
                                        else
                                            localExtras[i] = $"{localExtras[i]}~{extra.Price.ToString().Replace(",", ".")}~{localExtras[i].Split("~")[2].Replace(",", ".")}";
                                    }
                                }
                            }
                            tempExtraList = string.Join('|', localExtras);
                        }
                        string extraListesi = "";
                        float apiExtraAmount2 = 0;
                        float totalExtraPriceAmount = 0;
                        if (!string.IsNullOrEmpty(originalExtraList) && !sendAvailabilityRequest)
                        {
                            //var premiumPack = await _context.Additionalproduct.FirstOrDefaultAsync(ap => ap.Producttype == 5);
                            var extraItems = originalExtraList.Split('|', StringSplitOptions.RemoveEmptyEntries);
                            var extraListBuilder = new List<string>();

                            foreach (var item in extraItems)
                            {
                                var parts = item.Split('~');

                                if (parts.Length < 8)
                                    continue;

                                var (extraID, extraPiece, rawExtraPrice, extraName, extraCode, extraRentalTypeStr, extraApiPriceStr, extraDescription) =
                                      (parts[0], parts[1], parts[2], parts[3], parts[4], parts[5], parts[6], parts[7]);

                                float extraPrice = rawExtraPrice.ToFloatNullSafe();
                                float extraApiPrice = extraApiPriceStr.ToFloatNullSafe();
                                int extraRentalType = extraRentalTypeStr.ToIntNullSafe();
                                float agencyAmount = 0;
                                foreach (var premiumPack in premiumPackets)
                                {
                                    if (extraID.ToInt() == premiumPack?.ExtraId)
                                    //if (extraCode == premiumPack?.ExtraCode)
                                    {
                                        float defaultPrice = (float)(premiumPack.Price);
                                        agencyAmount = agency.AgencyCommissionAmount > 0
                                                                ? defaultPrice - (defaultPrice * agency.AgencyCommissionAmount / 100)
                                                                : defaultPrice;
                                        packetPricePremium = packetPricePremium + defaultPrice;
                                    }
                                }
                                agencyAmount = agencyAmount == 0 ? extraPrice : agencyAmount;
                                extraListBuilder.Add(string.Join("~", new[]
                                  {
                                            extraID,
                                            extraPiece,
                                            rawExtraPrice.Replace(',', '.'),
                                            extraName,
                                            extraCode,
                                            extraRentalTypeStr,
                                            extraApiPriceStr.Replace(',','.'),
                                            agencyAmount.ToString().Replace(',','.'),
                                            extraDescription
                                      }));

                                float durationMultiplier = extraRentalType == 1 ? reservationToken.RentalDuration : 1;
                                apiExtraAmount2 += extraApiPrice * durationMultiplier;
                                totalExtraPriceAmount += extraPrice * durationMultiplier;
                            }
                            extraListesi = string.Join("|", extraListBuilder);
                            postReservationRequest.ExtraAmount = totalExtraPriceAmount;
                        }

                        if (postReservationRequest.InstallmentCount != 0 && postReservationRequest.PaymentType == PaymentTypes.PayAll)
                        {
                            //float dailyPriceOfInstallment = (postReservationRequest.PaidAmount - reservationToken.OneWayFee - postReservationRequest.ExtraAmount) / reservationToken.RentalDuration;
                            //reservationToken.DailyPricePayNow = dailyPriceOfInstallment;
                            //float dailyPriceOfInstallment;
                            //if (postReservationRequest.CouponCode != null)
                            //{
                            //    dailyPriceOfInstallment = GetDailyPriceOfInstallmentByAgencySettings(agency,postReservationRequest,reservationToken);
                            //        dailyPriceOfInstallment = (postReservationRequest.PaidAmountAfterUsingCouponCode - reservationToken.OneWayFee - postReservationRequest.ExtraAmount) / reservationToken.RentalDuration;
                            //}
                            ////else 
                            ////if (postReservationRequest.PaidAmount != reservationToken.APITotalPrice) {
                            ////    dailyPriceOfInstallment = (float)(reservationToken.APITotalPrice - reservationToken.OneWayFee - postReservationRequest.ExtraAmount) / reservationToken.RentalDuration;
                            ////}
                            //else
                            //{
                            //        dailyPriceOfInstallment = (postReservationRequest.PaidAmount - reservationToken.OneWayFee - postReservationRequest.ExtraAmount) / reservationToken.RentalDuration;
                            //}
                            reservationToken.DailyPricePayNow = GetDailyPriceOfInstallmentByAgencySettings(agency, postReservationRequest, reservationToken);
                        }
                        var exchangeRates = await _context.Exchangerates.ToListAsync();
                        var mappedExchangeRates = exchangeRates.Map();
                        var reservationCurrencyType = postReservationRequest.CurrencyCode.ToEnum<CurrencyTypes>();
                        Serilog.Log.Error("{@ReservationLocalExchangeRates}", mappedExchangeRates);

                        if (vendor.VendorType == VendorTypes.KolayCARBroker && vendor.UseBrokerConfigurations && sendAvailabilityRequest)
                            postReservationRequest.ExtraList = ReservationHelper.ChangeExtraCodeAndExtraId(postReservationRequest.ExtraList);

                        var configurations = await _configurationService.GetConfigurations();

                        Serilog.Log.Error("{@Configurations}", configurations);

                        var vendorLogoUrl = "";
                        if (vendor.VendorType == (VendorTypes.Yolcu360 | VendorTypes.Yolcu360v2))
                            vendorLogoUrl = reservationToken.APIVendorLogo ?? string.Empty;
                        else
                            vendorLogoUrl = vendor.VendorType != VendorTypes.KolayCARBroker || (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations) ? $"{configurations.PortalOwnerDomain}{reservationToken.APIVendorLogo}" : reservationToken.APIVendorLogo ?? string.Empty;

                        var dailyPrice =
                            postReservationRequest.SpecialDailyPrice < 0
                                ? (postReservationRequest.PaymentType != PaymentTypes.PayAll
                                    ? reservationToken.DailyPrice
                                    : reservationToken.DailyPricePayNow)
                                : postReservationRequest.SpecialDailyPrice;

                        var totalAmount =
                            CalculationHelper.CalculateTotalPrice(
                                vendor.PriceRoundingType,
                                reservationToken.RentalDuration,
                                dailyPrice,
                                sendAvailabilityRequest ? postReservationRequest.ExtraAmount : totalExtraPriceAmount,
                                postReservationRequest.SpecialOneWayFee == -1 ? reservationToken.OneWayFee : postReservationRequest.SpecialOneWayFee);

                        var serviceCharge =
                            _userRole != UserRoles.External ? reservationToken.CurrencyType != vendor.ServiceChargeCurrencyType
                                ? CalculationHelper.CurrencyExchange(mappedExchangeRates, vendor, vendor.ServiceCharge, vendor.ServiceChargeCurrencyType, postReservationRequest.CurrencyCode.ToEnum<CurrencyTypes>())
                                : vendor.ServiceCharge : 0;

                        float discountAmount = 0;
                        float? discountValue = null;
                        int? couponId = null;

                        if (!string.IsNullOrEmpty(postReservationRequest.CouponCode) && !configurations.CheckCouponActive)
                        {
                            discountAmount = postReservationRequest.CouponDiscountAmount.ToFloatNullSafe();
                            discountValue = postReservationRequest.CouponDiscountValue.ToFloatNullSafe();
                        }

                        if (!string.IsNullOrEmpty(postReservationRequest.CouponCode) && configurations.CheckCouponActive)
                        {
                            //bool checkCouponIsUsable = await _couponService.CheckCouponIsUsable(postReservationRequest.CouponCode, postReservationRequest.MemberId ?? 0, totalAmount, reservationCurrencyType, postReservationRequest.PickupDate.ToDateTimeNullSafe(), postReservationRequest.HighAmountDiscountActive);
                            var checkCouponIsUsable = await _couponService.CheckCouponIsUsable(postReservationRequest.CouponCode, postReservationRequest.MemberId ?? 0, totalAmount, reservationCurrencyType, postReservationRequest.PickupDate.ToDateTimeNullSafe(), postReservationRequest.ReturnDate.ToDateTimeNullSafe(), reservationToken.DailyPrice.ToDecimalNullSafe(), reservationToken.RentalDuration, highAmountDiscountActive: true);

                            if (checkCouponIsUsable)
                            {
                                UsingCouponCode usingCouponCode = null;

                                if (!string.IsNullOrEmpty(postReservationRequest.CouponCode))
                                {
                                    // TODO: paidAmount Null olmaktan çıkarıldı yerine 0 değeri verildi (gkursad)
                                    usingCouponCode = await _couponService.GetUsingCouponCode(
                                        postReservationRequest.MemberId ?? 0,
                                        postReservationRequest.CouponCode,
                                        totalAmount: reservationToken.DailyPrice * reservationToken.RentalDuration,
                                        vendor,
                                        reservationCurrencyType,
                                        postReservationRequest.PickupDate.ToDateTimeNullSafe(),
                                        postReservationRequest.ReturnDate.ToDateTimeNullSafe(),
                                        reservationToken.DailyPrice.ToDecimalNullSafe(),
                                        reservationToken.RentalDuration,
                                        paidAmount:
                                        Math.Abs(postReservationRequest.PaidAmount -
                                                 postReservationRequest.PaidAmountAfterUsingCouponCode) > 0 &&
                                        postReservationRequest.PaidAmount == 0
                                            ? postReservationRequest.PaidAmountAfterUsingCouponCode
                                            : (float?)0,
                                        highAmountDiscountActive: postReservationRequest.HighAmountDiscountActive);
                                    //  highAmountDiscountActive: true);
                                }
                                Serilog.Log.Error("{@UsingCouponCode}", usingCouponCode);

                                if (usingCouponCode != null && usingCouponCode.Success)
                                {
                                    discountAmount = usingCouponCode.DiscountAmount.ToFloatNullSafe();
                                    discountValue = usingCouponCode.DiscountValue;
                                    couponId = usingCouponCode.CouponId;
                                    bool useCouponCodeResult = await _couponService.UseCouponCode(usingCouponCode.CouponId);
                                    Serilog.Log.Error("{UseCouponCodeResult}", useCouponCodeResult);
                                }
                            }
                            else
                                return new ServiceResponseBase(null, false, await _configurationService.GetLabel(113, postReservationRequest.LanguageCode.ToEnum<LanguageTypes>()));
                        }

                        totalAmount -= discountAmount;

                        float apiDailyPrice = BrokerReservationHelper.GetAPIDailyPrice(reservationToken.APIDailyPrice, vendor);

                        var apiExtraList = getExtrasResponse.Extras;
                        //extraID~extraPiece~extraPrice~extraName~productCode~extraRentalType~apiExtraPrice~agencyAmount~extraDescription

                        float apiExtraAmount = (!string.IsNullOrWhiteSpace(postReservationRequest.ExtraList) || !string.IsNullOrWhiteSpace(tempExtraList)) ? ReservationHelper.GetTotalExtraAmount(apiExtraList, postReservationRequest.ExtraList, reservationToken.RentalDuration) : 0;

                        apiExtraAmount = sendAvailabilityRequest ? apiExtraAmount : apiExtraAmount2;
                        tempExtraList = sendAvailabilityRequest ? tempExtraList : extraListesi;

                        float apiTotalAmount = BrokerReservationHelper.GetAPITotalAmount(reservationToken, vendor, apiExtraAmount);
                        //apiExtraAmount = BrokerReservationHelper.GetAPIAdditionalPrice(apiExtraAmount, vendor); //TODO: bu satır bazı durumlarda toplam ek ürün tutarını bozuyor. Kontrol edilecek !!!!!!!!!!!!!

                        var locationVendor = await _context.Locationvendor.Where(x =>
                        x.Active == true &&
                        x.Vendorid == vendor.VendorId &&
                        x.Locallocationid == postReservationRequest.PickupLocationId).FirstOrDefaultAsync();

                        bool isOffice = vendor.VendorType == (VendorTypes.Yolcu360 | VendorTypes.Yolcu360v2) ? reservationToken.IsOffice : vendor.VendorType != VendorTypes.KolayCARBroker || (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations) ? locationVendor.Isoffice ?? false : reservationToken.IsOffice; //KolayCARBroker dışındaki VehicleProvider'larda ReservationToken'a isOffice bilgisi tutulmuyor. Onlar kendi local isOffice bilgisini yazıyor. ++ (ekleme) Yolcu360 için reztokendan çekildi

                        await _bultenService.CheckContactPermission(postReservationRequest);

                        //var friendlyReservationNumber = await GenerateFriendlyReservationNumber();
                        var sqlParameters = SqlParameterHelper.PostReservationLocalSqlParameters(reservationId, postReservationRequest, reservationToken, vendor, dailyPrice, totalAmount, serviceCharge, tempExtraList, apiDailyPrice, apiExtraAmount, apiTotalAmount, vendorLogoUrl, isOffice, postPaymentResponse, discountAmount, agency, packetPricePremium, configurations, discountValue, couponId, vehicle);

                        Serilog.Log.Error("{@PostReservationLocalSqlParameters}", sqlParameters.Select(e => new { e.ParameterName, Type = e.SqlDbType, Value = e.Value }));

                        if (!sendAvailabilityRequest)
                            postReservationRequest.ExtraList = originalExtraList;

                        try
                        {
                            int postReservationLocalResult = await _context.Database.ExecuteSqlRawAsync("EXECUTE SP_ADD_RESERVATION " + SqlParameterHelper.SqlParamList(sqlParameters), sqlParameters);

                            Serilog.Log.Error("{@PostReservationLocalProcedureResult}", postReservationLocalResult);
                            return new ServiceResponseBase(postReservationRequest, true, postReservationLocalResult > 0 ? "Reservation received successfully!" : "An error occurred during the request!");
                        }
                        catch (Exception ex)
                        {
                            Serilog.Log.Error("{@PostReservationLocalProcedureErrorResult}", ex.ToJson());
                            try
                            {
                                var previousCommandTimeout = _context.Database.GetCommandTimeout();
                                int postReservationLocalResult = 0;
                                try
                                {
                                    _context.Database.SetCommandTimeout(15);
                                    postReservationLocalResult = await _context.Database.ExecuteSqlRawAsync("EXECUTE SP_ADD_RESERVATION " + SqlParameterHelper.SqlParamList(sqlParameters), sqlParameters);
                                }
                                catch (Exception ex2)
                                {
                                    Serilog.Log.Error("{@PostReservationLocalProcedureErrorResult2}", ex2.ToJson());
                                    return new ServiceResponseBase(postReservationRequest, false, $"An error occurred during the request! - {ex2.Message}");
                                }
                                finally
                                {
                                    _context.Database.SetCommandTimeout(previousCommandTimeout);

                                }
                                return new ServiceResponseBase(postReservationRequest, true, postReservationLocalResult > 0 ? "Reservation received successfully!" : "An error occurred during the request!");
                            }
                            catch (Exception retryEx)
                            {
                                Serilog.Log.Error("{@PostReservationLocalProcedureErrorResult2}", retryEx.ToJson());
                                return new ServiceResponseBase(postReservationRequest, false, $"An error occurred during the request! - {retryEx.Message}");
                            }
                        }
                    }
                    else
                    {
                        Serilog.Log.Error("Tedarikçi bilgisine ulaşılamadı!");
                        return new ServiceResponseBase(null, false, "Vendor information not available!");
                    }
                }
                else
                {
                    Serilog.Log.Error("Acente bilgisine ulaşılamadı!");
                    return new ServiceResponseBase(null, false, "Agency information not available!");
                }
            }
            return new ServiceResponseBase(null, false, "Check your reservationToken!");
        }

        public async Task<ServiceResponseBase> PostReservationLocalV3(PostReservationRequest postReservationRequest, long reservationId, ReservationToken reservationToken, List<Extra> apiExtras)
        {
            var agency = await _agencyService.GetAgency(reservationToken.AgencyId.ToLongNullSafe());
            if (agency != null)
            {
                Serilog.Log.Error("{@ReservationLocalAgency}", agency);

                var vendor = await _vendorService.GetVendorById(reservationToken.VendorId, agency, subVendorId: reservationToken.APIVendorId);
                if (vendor != null)
                {
                    ReservationHelper.FillPostReservationRequest(postReservationRequest, reservationToken, agency);

                    await _agencyService.SetAgencyPaymentOptions(agency, vendor);
                    Serilog.Log.Error("{@ReservationLocalVendor}", vendor);

                    var sendAvailabilityRequest = vendor.SendAvailabilityRequest;
                    string originalExtraList = postReservationRequest.ExtraList;

                    var selectedReservationExtras = ReservationHelper.GetSelectedReservationExtras(apiExtras, postReservationRequest.ExtraList);
                    postReservationRequest.ExtraList = ReservationHelper.ReservationExtraToStringList(selectedReservationExtras);
                    postReservationRequest.ExtraAmount = ReservationHelper.GetTotalExtraAmount(selectedReservationExtras, reservationToken.RentalDuration);
                    float packetPricePremium = 0;
                    string tempExtraList = postReservationRequest.ExtraList;


                    var extraIdList = apiExtras.Select(e => e.ExtraId).ToList();

                    if (!string.IsNullOrEmpty(postReservationRequest.ExtraList) &&
                            (vendor.VendorType != VendorTypes.KolayCARBroker || !vendor.UseBrokerConfigurations) &&
                            sendAvailabilityRequest)
                    {
                        var vendorExtras = await _context.Additionalproductvendor.Where(e => e.Vendorid == vendor.VendorId && e.Active == true).ToListAsync();

                        Serilog.Log.Error("{@ReservationLocalVendorExtras}", vendorExtras);

                        var extraListItems = postReservationRequest.ExtraList.Split('|', StringSplitOptions.RemoveEmptyEntries);

                        var apiExtraListMapped = new List<string>();

                        foreach (var item in extraListItems)
                        {
                            var parts = item.Split('~');
                            var extraId = parts[0].ToIntNullSafe();
                            var quantity = parts[1];
                            var price = parts[2];
                            var other = parts[3];
                            var extraCode = parts.Length > 4 ? parts[4] : string.Empty;

                            var dbExtra = vendorExtras.FirstOrDefault(x => x.Apiproductcode == extraCode);
                            string newApiExtra = string.Empty;

                            if (dbExtra != null)
                            {
                                newApiExtra = $"{dbExtra.Apiproductcode}~{quantity}~{price}~{other}~{dbExtra.Productid}~{price}";
                            }

                            if (!string.IsNullOrEmpty(newApiExtra))
                                apiExtraListMapped.Add(newApiExtra);
                        }

                        postReservationRequest.ExtraList = string.Join("|", apiExtraListMapped);
                    }

                    if (!string.IsNullOrEmpty(tempExtraList) && sendAvailabilityRequest)
                    {
                        var localExtras = tempExtraList.Split('|');
                        for (int i = 0; i < localExtras.Length; i++)
                        {
                            foreach (var extra in apiExtras)
                            {
                                var extraCode = localExtras[i].Split('~')[4];
                                var extraId = localExtras[i].Split('~')[0].ToIntNullSafe();

                                if (extraCode == extra.ExtraCode)
                                {
                                    extra.ExtraType = apiExtras.Where(e => e.ExtraCode == extraCode).FirstOrDefault()?.ExtraType ?? AdditionalProductTypes.Extra;
                                    extra.Price = apiExtras.Where(e => e.ExtraCode == extraCode).FirstOrDefault()?.Price ?? 0;
                                    if (vendor.AdditionalProductWorkingType == VendorWorkingTypes.Commission)
                                    {
                                        float extraPrice = extra.Price;
                                        extra.Price = CalculationHelper.ExtractCommission(vendor.ProfitMarkupAdditionalProducts, vendor.PriceRoundingType, extra.Price);
                                        localExtras[i] = $"{localExtras[i]}~{extra.Price.ToString().Replace(",", ".")}~{extraPrice.ToString().Replace(",", ".")}";
                                    }
                                    else
                                        localExtras[i] = $"{localExtras[i]}~{extra.Price.ToString().Replace(",", ".")}~{localExtras[i].Split("~")[2].Replace(",", ".")}";
                                }
                            }
                        }
                        tempExtraList = string.Join('|', localExtras);
                    }

                    if (postReservationRequest.InstallmentCount != 0 && postReservationRequest.PaymentType == PaymentTypes.PayAll)
                    {
                        reservationToken.DailyPricePayNow = GetDailyPriceOfInstallmentByAgencySettings(agency, postReservationRequest, reservationToken);
                    }
                    var exchangeRates = await _context.Exchangerates.ToListAsync();
                    var mappedExchangeRates = exchangeRates.Map();
                    var reservationCurrencyType = postReservationRequest.CurrencyCode.ToEnum<CurrencyTypes>();
                    Serilog.Log.Error("{@ReservationLocalExchangeRates}", mappedExchangeRates);

                    if (vendor.VendorType == VendorTypes.KolayCARBroker && vendor.UseBrokerConfigurations && sendAvailabilityRequest)
                        postReservationRequest.ExtraList = ReservationHelper.ChangeExtraCodeAndExtraId(postReservationRequest.ExtraList);

                    var configurations = await _configurationService.GetConfigurations();

                    Serilog.Log.Error("{@Configurations}", configurations);

                    var vendorLogoUrl = vendor.VendorType != VendorTypes.KolayCARBroker || (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations) ? $"{configurations.PortalOwnerDomain}{reservationToken.APIVendorLogo}" : reservationToken.APIVendorLogo ?? string.Empty;

                    var dailyPrice =
                        postReservationRequest.SpecialDailyPrice < 0
                            ? (postReservationRequest.PaymentType != PaymentTypes.PayAll
                                ? reservationToken.DailyPrice
                                : reservationToken.DailyPricePayNow)
                            : postReservationRequest.SpecialDailyPrice;

                    var totalAmount =
                        CalculationHelper.CalculateTotalPrice(
                            vendor.PriceRoundingType,
                            reservationToken.RentalDuration,
                            dailyPrice,
                            postReservationRequest.ExtraAmount,
                            postReservationRequest.SpecialOneWayFee == -1 ? reservationToken.OneWayFee : postReservationRequest.SpecialOneWayFee);

                    var serviceCharge =
                        _userRole != UserRoles.External ? reservationToken.CurrencyType != vendor.ServiceChargeCurrencyType
                            ? CalculationHelper.CurrencyExchange(mappedExchangeRates, vendor, vendor.ServiceCharge, vendor.ServiceChargeCurrencyType, postReservationRequest.CurrencyCode.ToEnum<CurrencyTypes>())
                            : vendor.ServiceCharge : 0;

                    float discountAmount = 0;
                    float? discountValue = null;
                    int? couponId = null;

                    if (!string.IsNullOrEmpty(postReservationRequest.CouponCode) && !configurations.CheckCouponActive)
                    {
                        discountAmount = postReservationRequest.CouponDiscountAmount.ToFloatNullSafe();
                        discountValue = postReservationRequest.CouponDiscountValue.ToFloatNullSafe();
                    }

                    totalAmount -= discountAmount;

                    float apiDailyPrice = BrokerReservationHelper.GetAPIDailyPrice(reservationToken.APIDailyPrice, vendor);

                    float apiExtraAmount = (!string.IsNullOrWhiteSpace(postReservationRequest.ExtraList) || !string.IsNullOrWhiteSpace(tempExtraList)) ? ReservationHelper.GetTotalExtraAmount(apiExtras, postReservationRequest.ExtraList, reservationToken.RentalDuration) : 0;

                    float apiTotalAmount = BrokerReservationHelper.GetAPITotalAmount(reservationToken, vendor, apiExtraAmount);

                    var locationVendor = await _context.Locationvendor.Where(x =>
                    x.Active == true &&
                    x.Vendorid == vendor.VendorId &&
                    x.Locallocationid == postReservationRequest.PickupLocationId).FirstOrDefaultAsync();

                    bool isOffice = vendor.VendorType != VendorTypes.KolayCARBroker || (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations) ? locationVendor.Isoffice ?? false : reservationToken.IsOffice;

                    var sqlParameters = SqlParameterHelper.PostReservationLocalSqlParameters(reservationId, postReservationRequest, reservationToken, vendor, dailyPrice, totalAmount, serviceCharge, tempExtraList, apiDailyPrice, apiExtraAmount, apiTotalAmount, vendorLogoUrl, isOffice, null, discountAmount, agency, packetPricePremium, configurations, discountValue, couponId, null);

                    Serilog.Log.Error("{@PostReservationLocalSqlParameters}", sqlParameters.Select(e => new { e.ParameterName, Type = e.SqlDbType, Value = e.Value }));

                    try
                    {
                        int postReservationLocalResult = await _context.Database.ExecuteSqlRawAsync("EXECUTE SP_ADD_RESERVATION " + SqlParameterHelper.SqlParamList(sqlParameters), sqlParameters);

                        Serilog.Log.Error("{@PostReservationLocalProcedureResult}", postReservationLocalResult);
                        return new ServiceResponseBase(postReservationRequest, true, postReservationLocalResult > 0 ? "Reservation received successfully!" : "An error occurred during the request!");
                    }
                    catch (Exception ex)
                    {
                        Serilog.Log.Error("{@PostReservationLocalProcedureErrorResult}", ex.ToJson());
                        try
                        {
                            var previousCommandTimeout = _context.Database.GetCommandTimeout();
                            int postReservationLocalResult = 0;
                            try
                            {
                                _context.Database.SetCommandTimeout(15);
                                postReservationLocalResult = await _context.Database.ExecuteSqlRawAsync("EXECUTE SP_ADD_RESERVATION " + SqlParameterHelper.SqlParamList(sqlParameters), sqlParameters);
                            }
                            catch (Exception ex2)
                            {
                                Serilog.Log.Error("{@PostReservationLocalProcedureErrorResult2}", ex2.ToJson());
                                return new ServiceResponseBase(postReservationRequest, false, $"An error occurred during the request! - {ex2.Message}");
                            }
                            finally
                            {
                                _context.Database.SetCommandTimeout(previousCommandTimeout);

                            }
                            return new ServiceResponseBase(postReservationRequest, true, postReservationLocalResult > 0 ? "Reservation received successfully!" : "An error occurred during the request!");
                        }
                        catch (Exception retryEx)
                        {
                            Serilog.Log.Error("{@PostReservationLocalProcedureErrorResult2}", retryEx.ToJson());
                            return new ServiceResponseBase(postReservationRequest, false, $"An error occurred during the request! - {retryEx.Message}");
                        }
                    }
                }
                else
                {
                    Serilog.Log.Error("Tedarikçi bilgisine ulaşılamadı!");
                    return new ServiceResponseBase(null, false, "Vendor information not available!");
                }
            }
            else
            {
                Serilog.Log.Error("Acente bilgisine ulaşılamadı!");
                return new ServiceResponseBase(null, false, "Agency information not available!");
            }
        }

        public float GetDailyPriceOfInstallmentByAgencySettings(CommonModels.Agency agency, PostReservationRequest postReservationRequest, ReservationToken reservationToken)
        {
            float dailyPriceOfInstallment;
            bool onewaydelivery = agency.OneWayAmountDeliveryPayment;
            bool additionalproduct = agency.AdditionalProductAmountDeliveryPayment;
            if (postReservationRequest.CouponCode != null)
            {
                switch (onewaydelivery)
                {
                    case true when additionalproduct == true:
                        dailyPriceOfInstallment = (postReservationRequest.PaidAmountAfterUsingCouponCode) / reservationToken.RentalDuration;
                        break;
                    case true when additionalproduct == false:
                        dailyPriceOfInstallment = (postReservationRequest.PaidAmountAfterUsingCouponCode - postReservationRequest.ExtraAmount) / reservationToken.RentalDuration;
                        break;
                    case false when additionalproduct == true:
                        dailyPriceOfInstallment = (postReservationRequest.PaidAmountAfterUsingCouponCode - reservationToken.OneWayFee) / reservationToken.RentalDuration;
                        break;
                    case false when additionalproduct == false:
                        dailyPriceOfInstallment = (postReservationRequest.PaidAmountAfterUsingCouponCode - reservationToken.OneWayFee - postReservationRequest.ExtraAmount) / reservationToken.RentalDuration;
                        break;
                    default:
                        dailyPriceOfInstallment = (postReservationRequest.PaidAmountAfterUsingCouponCode - reservationToken.OneWayFee - postReservationRequest.ExtraAmount) / reservationToken.RentalDuration;
                        break;
                }
            }
            else
            {
                switch (onewaydelivery)
                {
                    case true when additionalproduct == true:
                        dailyPriceOfInstallment = (postReservationRequest.PaidAmount) / reservationToken.RentalDuration;
                        break;
                    case true when additionalproduct == false:
                        dailyPriceOfInstallment = (postReservationRequest.PaidAmount - postReservationRequest.ExtraAmount) / reservationToken.RentalDuration;
                        break;
                    case false when additionalproduct == true:
                        dailyPriceOfInstallment = (postReservationRequest.PaidAmount - reservationToken.OneWayFee) / reservationToken.RentalDuration;
                        break;
                    case false when additionalproduct == false:
                        dailyPriceOfInstallment = (postReservationRequest.PaidAmount - reservationToken.OneWayFee - postReservationRequest.ExtraAmount) / reservationToken.RentalDuration;
                        break;
                    default:
                        dailyPriceOfInstallment = (postReservationRequest.PaidAmount - reservationToken.OneWayFee - postReservationRequest.ExtraAmount) / reservationToken.RentalDuration;
                        break;
                }
            }
            return dailyPriceOfInstallment;
        }
        public float GetDailyPriceOfInstallmentByAgencySettings(CommonModels.Agency agency, Domain.Models.Requests.PostReservationRequestV2 postReservationRequest, ReservationToken reservationToken)
        {
            float dailyPriceOfInstallment;
            bool onewaydelivery = agency.OneWayAmountDeliveryPayment;
            bool additionalproduct = agency.AdditionalProductAmountDeliveryPayment;
            if (postReservationRequest.CouponCode != null)
            {
                switch (onewaydelivery)
                {
                    case true when additionalproduct == true:
                        dailyPriceOfInstallment = (postReservationRequest.Pricing.PaidAmountAfterUsingCouponCode) / reservationToken.RentalDuration;
                        break;
                    case true when additionalproduct == false:
                        dailyPriceOfInstallment = (postReservationRequest.Pricing.PaidAmountAfterUsingCouponCode - postReservationRequest.Pricing.ExtraAmount) / reservationToken.RentalDuration;
                        break;
                    case false when additionalproduct == true:
                        dailyPriceOfInstallment = (postReservationRequest.Pricing.PaidAmountAfterUsingCouponCode - reservationToken.OneWayFee) / reservationToken.RentalDuration;
                        break;
                    case false when additionalproduct == false:
                        dailyPriceOfInstallment = (postReservationRequest.Pricing.PaidAmountAfterUsingCouponCode - reservationToken.OneWayFee - postReservationRequest.Pricing.ExtraAmount) / reservationToken.RentalDuration;
                        break;
                    default:
                        dailyPriceOfInstallment = (postReservationRequest.Pricing.PaidAmountAfterUsingCouponCode - reservationToken.OneWayFee - postReservationRequest.Pricing.ExtraAmount) / reservationToken.RentalDuration;
                        break;
                }
            }
            else
            {
                switch (onewaydelivery)
                {
                    case true when additionalproduct == true:
                        dailyPriceOfInstallment = (postReservationRequest.Pricing.PaidAmount) / reservationToken.RentalDuration;
                        break;
                    case true when additionalproduct == false:
                        dailyPriceOfInstallment = (postReservationRequest.Pricing.PaidAmount - postReservationRequest.Pricing.ExtraAmount) / reservationToken.RentalDuration;
                        break;
                    case false when additionalproduct == true:
                        dailyPriceOfInstallment = (postReservationRequest.Pricing.PaidAmount - reservationToken.OneWayFee) / reservationToken.RentalDuration;
                        break;
                    case false when additionalproduct == false:
                        dailyPriceOfInstallment = (postReservationRequest.Pricing.PaidAmount - reservationToken.OneWayFee - postReservationRequest.Pricing.ExtraAmount) / reservationToken.RentalDuration;
                        break;
                    default:
                        dailyPriceOfInstallment = (postReservationRequest.Pricing.PaidAmount - reservationToken.OneWayFee - postReservationRequest.Pricing.ExtraAmount) / reservationToken.RentalDuration;
                        break;
                }
            }
            return dailyPriceOfInstallment;
        }

        public async Task<ServiceResponseBase> PostReservationToVendorAPI(PostReservationRequest postReservationRequest, long reservationId, List<Extra> apiExtras)
        {
            var reservationToken = await _resTokenService.GetReservationTokenByUniqueId(postReservationRequest.ReservationToken);
            if (reservationToken != null)
            {
                var agency = await _agencyService.GetAgency(reservationToken.AgencyId.ToLongNullSafe());
                Serilog.Log.Error("{@ReservationAgency}", JsonConvert.SerializeObject(agency) + $"-{reservationToken.AgencyId}");
                if (agency != null)
                {
                    var vendor = await _vendorService.GetVendorById(reservationToken.VendorId, agency);
                    if (vendor != null)
                    {
                        ReservationHelper.FillPostReservationRequest(postReservationRequest, reservationToken, agency);
                        await _agencyService.SetAgencyPaymentOptions(agency, vendor);
                        if (await _reservationStepsService.GetAdditionalInformation(
                            vendor,
                            agency,
                            postReservationRequest.LanguageCode,
                            postReservationRequest.CurrencyCode,
                            vendor.CurrencyType,
                            postReservationRequest.PickupLocationId,
                            postReservationRequest.ReturnLocationId,
                            postReservationRequest.PickupDate,
                            postReservationRequest.ReturnDate,
                            postReservationRequest.PickupTime,
                            postReservationRequest.ReturnTime,
                            vehicleId: reservationToken.VehicleId,
                            apiVendorId: reservationToken.APIVendorId,
                            rentalDuration: reservationToken.RentalDuration,
                            apiReferenceCode: reservationToken.APIReferenceCode,
                            reservationToken: reservationToken,
                            apiLocationCode: reservationToken.APIPickupLocationCode) is ResponseReservationStepsAdditionalInformation additionalInformation && additionalInformation != null)
                        {
                            Serilog.Log.Error("{@ReservationAdditionalInformation}", JsonConvert.SerializeObject(additionalInformation));

                            var exchangeRates = await _context.Exchangerates.ToListAsync();
                            var mappedExchangeRates = exchangeRates.Map();
                            var configurations = await _configurationService.GetConfigurations();

                            Serilog.Log.Error("{@ReservationExchangeRates}", JsonConvert.SerializeObject(mappedExchangeRates));

                            var reservationNumber = ReservationHelper.GenerateReservationNumber(reservationId);

                            var localReservation = await GetReservation(new GetReservationsRequest
                            {
                                ReservationNumber = reservationNumber,
                                CustomerEmail = postReservationRequest.CustomerEmail
                            });

                            //Local rezervasyon yazıldıktan sonra komisyon çıkarılmış ekstra tutarları tedarikçi servisinden gelen ilk fiyatları ilke değiştirilir. Bu işlem tedarikçi fiyatının hesaplanması içindir local rezervasyonda güncelleme olmaz.
                            if (vendor.AdditionalProductWorkingType == VendorWorkingTypes.Commission)
                            {
                                foreach (var apiExtra in apiExtras)
                                {
                                    var reservationExtra = localReservation.ReservationExtras.Where(x => x.ExtraCode == apiExtra.ExtraCode).FirstOrDefault();
                                    if (reservationExtra != null)
                                        apiExtra.Price = reservationExtra.Price;
                                }
                            }

                            localReservation.APIPaidAmount = await CalculateAPIPaidAmount(agency, vendor, reservationToken, localReservation, postReservationRequest, apiExtras);

                            if (vendor.SendDefaultMailAddress && !string.IsNullOrEmpty(configurations.DefaultCustomerMailAddress) && (vendor.VendorType != VendorTypes.KolayCARBroker || (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations)))
                                postReservationRequest.CustomerEmail = configurations.DefaultCustomerMailAddress;

                            #region Özel ek ürünlerin tedarikçi apilerine gitmeme işlemi
                            bool haveSpecialExtras = false;
                            var specialExtrasList = new List<ReservationExtra>();
                            var ExtrasString = new List<string>();
                            var extraList = "";
                            //if (configurations.SpecialExtrasIsActive && localReservation.ReservationExtras.Count > 0 && postReservationRequest.ExtraList.Contains("BrokerSpecial"))
                            //{
                            //    extraList = postReservationRequest.ExtraList;
                            //    haveSpecialExtras = true;

                            //    specialExtrasList = localReservation.ReservationExtras.Where(x => x.ExtraType == AdditionalProductTypes.Compulsory || x.ExtraCode.Contains("BrokerSpecial")).ToList();
                            //    localReservation.ReservationExtras.RemoveAll(x => x.ExtraType == AdditionalProductTypes.Compulsory || x.ExtraCode.Contains("BrokerSpecial"));

                            //    ExtrasString = postReservationRequest.ExtraList.Split('|').ToList();
                            //    ExtrasString.RemoveAll(x => x.Contains("BrokerSpecial"));
                            //    var specialExtrasString = postReservationRequest.ExtraList.Split('|').Where(x => x.Contains("BrokerSpecial")).ToList();
                            //    postReservationRequest.ExtraList = "";

                            //    for (int i = 0; i < ExtrasString.Count; i++)
                            //        postReservationRequest.ExtraList += i == ExtrasString.Count - 1 ? ExtrasString[i].ToString() : ExtrasString[i].ToString() + "|";
                            //}
                            if (localReservation.ReservationExtras.Where(x => x.ExtraType == AdditionalProductTypes.Premium).Any())
                            {
                                extraList = postReservationRequest.ExtraList;
                                haveSpecialExtras = true;

                                var premiumPack = await _context.Additionalproduct.FirstOrDefaultAsync(ap => ap.Producttype == 5);
                                localReservation.ReservationExtras.RemoveAll(x => x.ExtraCode == "PRMPKT-1");

                                ExtrasString = postReservationRequest.ExtraList.Split('|').ToList();
                                postReservationRequest.ExtraList = "";

                                foreach (var extra in ExtrasString)
                                {
                                    var extraProps = extra.Split('~').ToList();

                                    if (!(premiumPack != null && extraProps[0] == premiumPack.Productcode)) // zorunlu olmayanları ekle //== premiumPack.Productid
                                        postReservationRequest.ExtraList += extra + "|";
                                }
                                if (postReservationRequest.ExtraList.EndsWith("|"))
                                    postReservationRequest.ExtraList = postReservationRequest.ExtraList.TrimEnd('|');
                            }
                            #endregion
                            var reservationProvider = _reservationProviderFactory.CreateReservationProvider(vendor, _configurationService, _configuration, _memoryCache, _cacheService);

                            if (reservationProvider is null)
                                return new ServiceResponseBase(null, false, "Tedarikçi tipi bulunamadı!");

                            var result = await reservationProvider.PostReservation(postReservationRequest, vendor, additionalInformation, reservationNumber, reservationToken, mappedExchangeRates, localReservation, apiExtras);

                            Serilog.Log.Error("{@PostReservationVendorResponse}", JsonConvert.SerializeObject(result));

                            try
                            {
                                var retryPostReservation = _parameterService.GetParameterValue("RetryPostReservationToVendor").ToBoolNullSafe();
                                var reservation = result.Data as Reservation;
                                if (reservation == null)
                                {
                                    Serilog.Log.Error("{@PostReservationError}", "result.Data is null or not a Reservation object");
                                    return result;
                                }

                                if (retryPostReservation && vendor.VendorType != VendorTypes.Vonarent)
                                {
                                    if (string.IsNullOrEmpty(reservation.APIReservationNumber))
                                    {
                                        result = await reservationProvider.PostReservation(postReservationRequest, vendor, additionalInformation, reservationNumber, reservationToken, mappedExchangeRates, localReservation, apiExtras);

                                        Serilog.Log.Error("{@PostReservationVendorResponseRetry}", JsonConvert.SerializeObject(result));
                                    }
                                }
                                if (haveSpecialExtras)//tedarikçi servisine gitmemesi için zorunlu zorunlu ek ürün varsa tekrar ekleme yapar.
                                {
                                    localReservation.ReservationExtras.AddRange(specialExtrasList);
                                    postReservationRequest.ExtraList = extraList;
                                }
                                var officeLabel = await _configurationService.GetLabel(2075, postReservationRequest.LanguageCode.ToEnum<LanguageTypes>());

                                if (string.IsNullOrEmpty(reservation.PickupOfficeWorkingHours))
                                {
                                    var pickupOffice = await _vendorOfficeService.GetVendorOffice(reservation.VendorId, reservation.PickupLocationId);
                                    reservation.PickupOfficeWorkingHours = pickupOffice != null
                                        ? $"{pickupOffice.OpeningTime.ToDateTimeNullSafe().ToString("HH:mm")} - {pickupOffice.ClosingTime.ToDateTimeNullSafe().ToString("HH:mm")}"
                                        : officeLabel;
                                }

                                if (string.IsNullOrEmpty(reservation.ReturnOfficeWorkingHours))
                                {
                                    var returnOffice = await _vendorOfficeService.GetVendorOffice(reservation.VendorId, reservation.ReturnLocationId);
                                    reservation.ReturnOfficeWorkingHours = returnOffice != null
                                        ? $"{returnOffice.OpeningTime.ToDateTimeNullSafe().ToString("HH:mm")} - {returnOffice.ClosingTime.ToDateTimeNullSafe().ToString("HH:mm")}"
                                        : officeLabel;
                                }

                                if (string.IsNullOrEmpty(reservation.PickupOfficeWorkingHours) || string.IsNullOrEmpty(reservation.ReturnOfficeWorkingHours))
                                    Serilog.Log.Error("{@OfficeWorkingHours}", $"{officeLabel} - {reservation.PickupOfficeWorkingHours} - {reservation.ReturnOfficeWorkingHours}");
                                result.Data = reservation;
                                return result;

                            }
                            catch (Exception)
                            {
                                try
                                {
                                    var reservation = result.Data as Reservation;
                                    result.Data = reservation;
                                    return result;
                                }
                                catch (Exception)
                                {
                                    return result;
                                }
                            }
                        }
                    }
                    else
                        return new ServiceResponseBase(null, false, "Tedarikçi bilgisine ulaşılamadı!");
                }
                else
                    return new ServiceResponseBase(null, false, "Acente bilgisine ulaşılamadı!");

                return new ServiceResponseBase(null, false, "Bilgileri kontrol edip tekrar deneyin!");
            }
            return new ServiceResponseBase(null, false, "reservationToken hatalı!");
        }

        public Reservation UpdateReservationWhenPostReservationToServiceSuccessfully(Reservation reservation)
        {
            try
            {
                if (reservation != null)
                {
                    var mappedReservation = reservation.Map();

                    _context.Attach(mappedReservation);
                    _context.Entry(mappedReservation).State = EntityState.Unchanged;
                    _context.Entry(mappedReservation).Property(nameof(mappedReservation.Servisegonderildi)).IsModified = true;
                    _context.Entry(mappedReservation).Property(nameof(mappedReservation.Apireservationsuccessfully)).IsModified = true;
                    _context.Entry(mappedReservation).Property(nameof(mappedReservation.Pdf)).IsModified = true;
                    _context.Entry(mappedReservation).Property(nameof(mappedReservation.Apireservationnumber)).IsModified = true;
                    _context.Entry(mappedReservation).Property(nameof(mappedReservation.Apivendorname)).IsModified = true;
                    _context.Entry(mappedReservation).Property(nameof(mappedReservation.Apivendoraddress)).IsModified = true;
                    _context.Entry(mappedReservation).Property(nameof(mappedReservation.Apivendorreturnaddress)).IsModified = true;
                    _context.Entry(mappedReservation).Property(nameof(mappedReservation.Apivendorphone)).IsModified = true;
                    _context.Entry(mappedReservation).Property(nameof(mappedReservation.Apivendorreturnphone)).IsModified = true;
                    _context.Entry(mappedReservation).Property(nameof(mappedReservation.Apimessage)).IsModified = true;
                    _context.Entry(mappedReservation).Property(nameof(mappedReservation.Apiphoneactive)).IsModified = true;
                    _context.Entry(mappedReservation).Property(nameof(mappedReservation.Apipaidamount)).IsModified = true;
                    _context.Entry(mappedReservation).Property(nameof(mappedReservation.Apireferencecode)).IsModified = true;
                    _context.Entry(mappedReservation).Property(nameof(mappedReservation.Apireferencecode2)).IsModified = true;
                    _context.Entry(mappedReservation).Property(nameof(mappedReservation.Apireferencecode3)).IsModified = true;
                    _context.Entry(mappedReservation).Property(nameof(mappedReservation.PickupOfficeWorkingHours)).IsModified = true;
                    _context.Entry(mappedReservation).Property(nameof(mappedReservation.ReturnOfficeWorkingHours)).IsModified = true;
                    _context.SaveChanges();
                }
                return reservation;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@UpdateReservationWhenPostReservationToServiceSuccessfully}", ex.Message);
                return reservation;
            }
        }

        public Reservation UpdateReservationWhenCancelReservationToServiceSuccessfully(Reservation reservation)
        {
            if (reservation != null)
            {
                var mappedReservation = reservation.Map();

                _context.Attach(mappedReservation);
                _context.Entry(mappedReservation).State = EntityState.Unchanged;

                _context.Entry(mappedReservation).Property(nameof(mappedReservation.Apireservationcancel)).IsModified = true;

                _context.SaveChanges();
                _context.Entry(mappedReservation).State = EntityState.Detached;
            }

            return reservation;
        }

        public Reservation UpdateReservationWhenPaymentRefundSuccessfully(Reservation reservation)
        {
            if (reservation != null)
            {
                var mappedReservation = reservation.Map();

                _context.Attach(mappedReservation);
                _context.Entry(mappedReservation).State = EntityState.Unchanged;
                _context.Entry(mappedReservation).Property(nameof(mappedReservation.Paymentrefundsuccess)).IsModified = true;
                _context.SaveChanges();
                _context.Entry(mappedReservation).State = EntityState.Detached;
            }
            return reservation;
        }

        public async Task PostReservationMail(Reservation reservation, bool resend = false, string toMailAddress = null)
        {
            if (reservation != null)
            {
                var configurations = await _configurationService.GetConfigurations();
                var baseAgencies = await _configurationService.GetBaseAgencies();
                var coupon = await _couponService.GetCouponCodeByReservastionNumber(reservation.ReservationId);
                var couponTemplate = await _configurationService.GetLabel(193, reservation.LanguageType);
                var couponsmstemplate = new TemplateHelper(couponTemplate);

                var agency = await _agencyService.GetAgency(reservation.AgencyId.ToLongNullSafe());
                var reservationMailTemplate = (reservation.CouponId != null && reservation.CouponId != 0 && !await _couponService.CheckCouponIsShowPrice((int)reservation.CouponId) && !configurations.NoPriceVoucherSending)
                                            ? await _configurationService.GetFormContent(-42, reservation.LanguageType)
                                            : await _configurationService.GetFormContent(configurations.ReservationMailTemplateId, reservation.LanguageType);

                if (agency.AgencyId == 857 || agency.AgencyId == 1484)
                    reservationMailTemplate = await _configurationService.GetFormContent(9, reservation.LanguageType);

                var reservationSmsTemplate = await _configurationService.GetLabel(141, reservation.LanguageType);
                var reservationSmsTemplateTest = await _configurationService.GetLabel(Convert.ToInt32(configurations.RezIlaveSMS), reservation.LanguageType);
                var reservationCancelSmsTemplate = await _configurationService.GetLabel(142, reservation.LanguageType);
                var reservationPenaltyInformationEmailTemplate = await _configurationService.GetFormContent(-31, reservation.LanguageType);
                var failedReservationEmailTempalte = await _configurationService.GetFormContent(-32, reservation.LanguageType);

                var reservationStatusName = await _configurationService.GetLabel(reservation.ReservationStatusType == ReservationStatusTypes.Cancelled ? 115 : configurations.ReservationStatusNameLabelId, reservation.LanguageType);
                var reservationStatusMessage = await _configurationService.GetLabel(configurations.ReservationStatusMessageLabelId, reservation.LanguageType);
                var isOfficeTrueLabel = await _configurationService.GetLabel(140, reservation.LanguageType);
                var isOfficeFalseLabel = await _configurationService.GetLabel(139, reservation.LanguageType);
                var depositMessage = await _configurationService.GetLabel(144, reservation.LanguageType);
                var documentTitle = await _configurationService.GetLabel(reservation.ReservationStatusType == ReservationStatusTypes.Cancelled ? 138 : 137, reservation.LanguageType);
                var agencyMessage = await _configurationService.GetLabel(143, reservation.LanguageType);
                var cancellationPenaltyInformationEmailTitle = await _configurationService.GetLabel(145, reservation.LanguageType);
                var failedReservationEmailTitle = await _configurationService.GetLabel(146, reservation.LanguageType);

                var mailTemplateHelper = new TemplateHelper(reservationMailTemplate);
                var smsTemplateHelper = new TemplateHelper(reservation.ReservationStatusType != ReservationStatusTypes.Cancelled ? reservationSmsTemplate : reservationCancelSmsTemplate);
                var smsTemplateHelperTest = new TemplateHelper(reservation.ReservationStatusType != ReservationStatusTypes.Cancelled ? reservationSmsTemplateTest : reservationCancelSmsTemplate);
                var penaltyInformationEmailTemplateHelper = new TemplateHelper(reservationPenaltyInformationEmailTemplate);
                var failedReservationEmailTemplateHelper = new TemplateHelper(failedReservationEmailTempalte);


                var vendor = await _vendorService.GetVendorById(reservation.VendorId, agency, subVendorId: reservation.APIVendorId);
                await _agencyService.SetAgencyPaymentOptions(agency, vendor);
                string fuelName = await _vehicleService.GetFuelName(reservation.LanguageType, reservation.FuelType);
                string transmissionName = await _vehicleService.GetTransmissionName(reservation.LanguageType, reservation.TransmissionType);
                var additionsTitleLabel = await _configurationService.GetLabel(147, reservation.LanguageType);
                var pickupLocation = await _locationService.GetLocation(reservation.PickupLocationId, (int)reservation.LanguageType + 1);
                var returnLocation = await _locationService.GetLocation(reservation.ReturnLocationId, (int)reservation.LanguageType + 1);
                var vendorContactInformation = await _vendorContactInformationService.GetVendorContactInformation(reservation.PickupLocationId, vendor.VendorId);
                var reservationDetailPageContentUrl = await _configurationService.GetContentUrl(-8, reservation.LanguageType);
                var reservationDetailContentUrl = await _configurationService.GetContentUrl(-18, reservation.LanguageType);
                var vendorLocationFee = await _vendorService.GetVendorLocationFeeByVendorIdAndLocationId(reservation.VendorId, reservation.PickupLocationId);
                var vendorShowLogoBool = await _configurationService.GetConfigurationByDegisken("TedarikciLogoGosterimi");
                var vendorShowLogo = vendorShowLogoBool.ToBoolNullSafe();
                //var sendInvoiceActiveBool = await _configurationService.GetConfigurationByDegisken("SendInvoiceActive");
                //var sendInvoiceActive = sendInvoiceActiveBool.ToBoolNullSafe();
                //var agencySendInvoiceActiveBool = await _configurationService.GetConfigurationByDegisken("SendMailInvoice");
                //var agencySendInvoiceActive = agencySendInvoiceActiveBool.ToBoolNullSafe();
                var customerInvoice = await _configurationService.GetLabel(415, reservation.LanguageType);

                bool disableSsl = _configuration["AppSettings:DisableSsl"].ToBoolNullSafe();
                var reservationTemplateFields = new ReservationMailTemplate
                {
                    Reservation = reservation,
                    Agency = agency,
                    Vendor = vendor,
                    ReservationStatusName = reservationStatusName,
                    ReservationStatusMessage = reservationStatusMessage,
                    ReservationDetailUrlTemplate = configurations.ReservationDetailUrlTemplate,
                    PortalOwnerTitle = configurations.PortalOwnerTitle,
                    PortalOwnerAddress = configurations.PortalOwnerAddress,
                    PortalOwnerPhone = configurations.PortalOwnerPhone,
                    PortalOwnerFax = configurations.PortalOwnerFax,
                    PortalOwnerEmail = configurations.PortalOwnerEmail,
                    PortalOwnerDomain = configurations.PortalOwnerDomain,
                    PortalOwnerLogo = configurations.PortalOwnerLogoPath,
                    IsOfficeTrueLabel = isOfficeTrueLabel,
                    IsOfficeFalseLabel = isOfficeFalseLabel,
                    EmergencyPhone = configurations.EmergencyPhone,
                    DocumentTitle = documentTitle,
                    FuelName = fuelName,
                    TransmissionName = transmissionName,
                    AgencyMessage = agencyMessage,
                    DepositMessage = depositMessage,
                    AdditionsTitleLabel = additionsTitleLabel,
                    PickupLocation = pickupLocation,
                    ReturnLocation = returnLocation,
                    ReservationDetailPageContentUrl = reservationDetailPageContentUrl,
                    CancellationPenaltyInformationEmailTitle = cancellationPenaltyInformationEmailTitle,
                    ReservationDetailContentUrl = reservationDetailContentUrl,
                    CouponCode = coupon,
                    ReservationVendorContractUrl = await GetContractUrl(reservation.LanguageType, vendor.VendorId),
                    VendorLocationFee = vendorLocationFee,
                    VendorShowLogo = vendorShowLogo,
                    DefaultLanguageType = configurations.DefaultLanguageType,
                };

                var mailTitleTemplateHelper = new TemplateHelper(reservationTemplateFields.ReservationStatusName);
                reservationTemplateFields.ReservationStatusName = mailTitleTemplateHelper.GetParsedReservationTemplate(reservationTemplateFields);
                var authProvider = new KolayCARBrokerProvider.AuthProvider(_appSettings.ApiBaseUrl);
                var auth = await authProvider.GetJWT(baseAgencies.Admin.AgencyApiKey, baseAgencies.Admin.AgencyApiPassword, EncryptionHelper.Encrypt(baseAgencies.Admin.AgencyApiPassword));
                var smsList = await _contentService.GetSmsContent(reservation.LanguageType);
                var customerMailAddress = string.IsNullOrWhiteSpace(toMailAddress)
                    ? reservation.CustomerMail
                    : toMailAddress.TrimNullSafe().ToLower();

                var agencyMails = agency.Email
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToList();

                if (auth is { Success: true } && !_env.IsDevelopment())
                {
                    var user = auth.Data as User;
                    RestManager restManager = new RestManager(_appSettings.ApiBaseUrl);

                    #region Mail Gönderme
                    //Müşteri mail gönderimi
                    if (!string.IsNullOrWhiteSpace(customerMailAddress) && reservation.SendReservationMail &&
                        agency is { SendReservationMailToCustomerActive: true } && !reservation.IsSpecialWebSiteAgency)
                    {
                        reservationTemplateFields.ReservationMailSendToType = ReservationMailSendToTypes.ToCustomer;

                        #region Yorumda - gkursad
                        //await restManager.PostAsync<PostEmailRequest, object>(
                        //    requestPath: "emails/longContent",
                        //    entity: CreateBrokerPostEmailRequestParameters(
                        //        typeName: "CustomerMail",
                        //        fromTitle: configurations.PortalOwnerTitle,
                        //        toMailAddress: reservation.CustomerMail,
                        //        subject: $"{reservationTemplateFields.ReservationStatusName} - {reservation.ReservationNumber}",
                        //        body: EmailHelper.PrepareReservationEmailHtml(mailTemplateHelper.GetParsedReservationTemplate(reservationTemplateFields, true), reservation.ReservationStatusType),
                        //        isCancelMail : reservation.ReservationStatusType == ReservationStatusTypes.Cancelled ? true : false),
                        //    headers: CreateBrokerAuthRequestHeader(user.Token));
                        #endregion

                        var postEmailRequest = CreateBrokerPostEmailRequestParameters(
                            typeName: $"CustomerMail{(reservation.ReservationStatusType == ReservationStatusTypes.Cancelled ? " - CANCEL" : "")}",
                            fromTitle: configurations.PortalOwnerTitle,
                            toMailAddress: customerMailAddress,
                            subject:
                            $"{(resend ? "YENİDEN YÖNLENDİRME " : "")}{reservationTemplateFields.ReservationStatusName} - {reservation.ReservationNumber}",
                            body: EmailHelper.PrepareReservationEmailHtml(
                                mailTemplateHelper.GetParsedReservationTemplate(reservationTemplateFields, true),
                                reservation.ReservationStatusType),
                            isCancelMail: reservation.ReservationStatusType == ReservationStatusTypes.Cancelled);

                        await _emailService.PostEmail(
                            postEmailRequest.TypeName,
                            postEmailRequest.FromTitle,
                            postEmailRequest.ToMailAddress,
                            postEmailRequest.Subject,
                            postEmailRequest.Body,
                            postEmailRequest.ReplyTo,
                            postEmailRequest.IsCancelMail, disableSsl: disableSsl).ConfigureAwait(false);
                    }

                    //Acente mail gönderimi
                    if (agency != null && !string.IsNullOrWhiteSpace(agency.Email) && agency.SendReservationMailToAgency && !reservation.IsSpecialWebSiteAgency &&
                        !string.IsNullOrEmpty(reservation.APIReservationNumber))
                    {
                        reservationTemplateFields.ReservationMailSendToType = ReservationMailSendToTypes.ToAgency;

                        #region Yorumda - gkursad
                        //restManager.PostAsync<PostEmailRequest, object>(
                        //    requestPath: "emails/longContent",
                        //    entity: CreateBrokerPostEmailRequestParameters(
                        //        typeName: "AgencyMail",
                        //        fromTitle: configurations.PortalOwnerTitle,
                        //        toMailAddress: agency.Email,
                        //        subject: $"{reservationTemplateFields.ReservationStatusName} - {reservation.ReservationNumber}",
                        //        body: EmailHelper.PrepareReservationEmailHtml(mailTemplateHelper.GetParsedReservationTemplate(reservationTemplateFields, mask: true), reservation.ReservationStatusType), 
                        //        isCancelMail: reservation.ReservationStatusType == ReservationStatusTypes.Cancelled ? true : false),
                        //    headers: CreateBrokerAuthRequestHeader(user.Token));
                        #endregion

                        foreach (var agencyMail in agencyMails)
                        {
                            var postEmailRequest = CreateBrokerPostEmailRequestParameters(
                           typeName: $"AgencyMail{(reservation.ReservationStatusType == ReservationStatusTypes.Cancelled ? " - CANCEL" : "")}",
                           fromTitle: configurations.PortalOwnerTitle,
                           toMailAddress: agencyMail,
                           subject:
                           $"{(resend ? "YENİDEN YÖNLENDİRME " : "")}{reservationTemplateFields.ReservationStatusName} - {reservation.ReservationNumber}",
                           body: EmailHelper.PrepareReservationEmailHtml(
                               mailTemplateHelper.GetParsedReservationTemplate(reservationTemplateFields, mask: true),
                               reservation.ReservationStatusType),
                           isCancelMail: reservation.ReservationStatusType == ReservationStatusTypes.Cancelled);

                            await _emailService.PostEmail(
                                  postEmailRequest.TypeName,
                                  postEmailRequest.FromTitle,
                                  postEmailRequest.ToMailAddress,
                                  postEmailRequest.Subject,
                                  postEmailRequest.Body,
                                  postEmailRequest.ReplyTo,
                                  postEmailRequest.IsCancelMail, disableSsl: disableSsl);
                        }

                    }

                    //Admin mail gönderimi
                    if (configurations.AdministratorEmailSending && !string.IsNullOrWhiteSpace(configurations.AdministratorEmail))
                    {
                        reservationTemplateFields.ReservationMailSendToType = ReservationMailSendToTypes.ToAdministrator;

                        #region Yorumda - gkursad
                        //restManager.PostAsync<PostEmailRequest, object>(
                        //    requestPath: "emails/longContent",
                        //    entity: CreateBrokerPostEmailRequestParameters(
                        //        typeName: "AdminMail",
                        //        fromTitle: configurations.PortalOwnerTitle,
                        //        toMailAddress: configurations.AdministratorEmail,
                        //        subject: $"{reservationTemplateFields.ReservationStatusName} - {reservation.ReservationNumber}",
                        //        body: EmailHelper.PrepareReservationEmailHtml(mailTemplateHelper.GetParsedReservationTemplate(reservationTemplateFields, mask: true), reservation.ReservationStatusType),
                        //        isCancelMail: reservation.ReservationStatusType == ReservationStatusTypes.Cancelled ? true : false),
                        //    headers: CreateBrokerAuthRequestHeader(user.Token));
                        #endregion

                        var postEmailRequest = CreateBrokerPostEmailRequestParameters(
                            typeName: $"AdminMail{(reservation.ReservationStatusType == ReservationStatusTypes.Cancelled ? " - CANCEL" : "")}",
                            fromTitle: configurations.PortalOwnerTitle,
                            toMailAddress: configurations.AdministratorEmail,
                            subject:
                            $"{(resend ? "YENİDEN YÖNLENDİRME " : "")}{reservationTemplateFields.ReservationStatusName} - {reservation.ReservationNumber}",
                            body: EmailHelper.PrepareReservationEmailHtml(
                                mailTemplateHelper.GetParsedReservationTemplate(reservationTemplateFields, mask: true),
                                reservation.ReservationStatusType),
                            isCancelMail: reservation.ReservationStatusType == ReservationStatusTypes.Cancelled
                                ? true
                                : false);

                        _emailService.PostEmail(
                             postEmailRequest.TypeName,
                             postEmailRequest.FromTitle,
                             postEmailRequest.ToMailAddress,
                             postEmailRequest.Subject,
                             postEmailRequest.Body,
                             postEmailRequest.ReplyTo, postEmailRequest.IsCancelMail, disableSsl: disableSsl);
                    }

                    //Başarısız Rezervasyon Admin Mail Bildirimi
                    if (configurations.AdministratorEmailSending && !string.IsNullOrWhiteSpace(configurations.AdministratorEmail) && reservation.ReservationStatusType != ReservationStatusTypes.Cancelled && string.IsNullOrEmpty(reservation.APIReservationNumber))
                    {
                        reservationTemplateFields.ReservationMailSendToType = ReservationMailSendToTypes.ToAdministrator;

                        #region Yorumda - gkursad
                        //restManager.PostAsync<PostEmailRequest, object>(
                        //    requestPath: "emails/longContent",
                        //    entity: CreateBrokerPostEmailRequestParameters(
                        //        typeName: "AdminUnsuccessfulMail",
                        //        fromTitle: configurations.PortalOwnerTitle,
                        //        toMailAddress: configurations.AdministratorEmail,
                        //        subject: $"{failedReservationEmailTitle} - {reservation.ReservationNumber}",
                        //        body: EmailHelper.PrepareReservationEmailHtml(failedReservationEmailTemplateHelper.GetParsedReservationTemplate(reservationTemplateFields, mask: true), reservation.ReservationStatusType)),
                        //    headers: CreateBrokerAuthRequestHeader(user.Token));
                        #endregion

                        var postEmailRequest = CreateBrokerPostEmailRequestParameters(
                            typeName: $"AdminUnsuccessfulMail{(reservation.ReservationStatusType == ReservationStatusTypes.Cancelled ? " - CANCEL" : "")}",
                            fromTitle: configurations.PortalOwnerTitle,
                            toMailAddress: configurations.AdministratorEmail,
                            subject: $"{(resend ? "YENİDEN YÖNLENDİRME " : "")}{failedReservationEmailTitle} - {reservation.ReservationNumber}",
                            body: EmailHelper.PrepareReservationEmailHtml(
                                failedReservationEmailTemplateHelper.GetParsedReservationTemplate(
                                    reservationTemplateFields, mask: true), reservation.ReservationStatusType));

                        _emailService.PostEmail(
                        postEmailRequest.TypeName,
                        postEmailRequest.FromTitle,
                        postEmailRequest.ToMailAddress,
                        postEmailRequest.Subject,
                        postEmailRequest.Body,
                        postEmailRequest.ReplyTo, postEmailRequest.IsCancelMail, disableSsl: disableSsl);
                    }

                    //Tedarikçi mail gönderimi
                    if (vendor != null && !string.IsNullOrEmpty(vendor.VendorEmail) && vendor.SendReservationMailToVendor)
                    {
                        reservationTemplateFields.ReservationMailSendToType = ReservationMailSendToTypes.ToVendor;

                        #region Yorumda - gkursad
                        //restManager.PostAsync<PostEmailRequest, object>(
                        //    requestPath: "emails/longContent",
                        //    entity: CreateBrokerPostEmailRequestParameters(
                        //        typeName: "VendorMail",
                        //        fromTitle: configurations.PortalOwnerTitle,
                        //        toMailAddress: vendor.VendorEmail,
                        //        subject: $"{reservationTemplateFields.ReservationStatusName} - {reservation.ReservationNumber}",
                        //        body: EmailHelper.PrepareReservationEmailHtml(mailTemplateHelper.GetParsedReservationTemplate(reservationTemplateFields, mask: true), reservation.ReservationStatusType),
                        //        isCancelMail: reservation.ReservationStatusType == ReservationStatusTypes.Cancelled ? true : false),
                        //    headers: CreateBrokerAuthRequestHeader(user.Token));
                        #endregion

                        var postEmailRequest = CreateBrokerPostEmailRequestParameters(
                            typeName: $"VendorMail{(reservation.ReservationStatusType == ReservationStatusTypes.Cancelled ? " - CANCEL" : "")}",
                            fromTitle: configurations.PortalOwnerTitle,
                            toMailAddress: vendor.VendorEmail,
                            subject:
                            $"{(resend ? "YENİDEN YÖNLENDİRME " : "")}{reservationTemplateFields.ReservationStatusName} - {reservation.ReservationNumber}",
                            body: EmailHelper.PrepareReservationEmailHtml(
                                mailTemplateHelper.GetParsedReservationTemplate(reservationTemplateFields, mask: true),
                                reservation.ReservationStatusType),
                            isCancelMail: reservation.ReservationStatusType == ReservationStatusTypes.Cancelled
                                ? true
                                : false);

                        await _emailService.PostEmail(
                           postEmailRequest.TypeName,
                           postEmailRequest.FromTitle,
                           postEmailRequest.ToMailAddress,
                           postEmailRequest.Subject,
                           postEmailRequest.Body,
                           postEmailRequest.ReplyTo, postEmailRequest.IsCancelMail, disableSsl: disableSsl);
                    }


                    if (configurations.LocationEmailSending && vendorContactInformation?.Email != "")
                    {
                        #region Yorumda - gkursad
                        //restManager.PostAsync<PostEmailRequest, object>(
                        //      requestPath: "emails/longContent",
                        //      entity: CreateBrokerPostEmailRequestParameters(
                        //          typeName: "LocationBranchMail",
                        //          fromTitle: configurations.PortalOwnerTitle,
                        //          toMailAddress: vendorContactInformation?.Email,
                        //          subject: $"{reservationTemplateFields.ReservationStatusName} - {reservation.ReservationNumber}",
                        //          body: EmailHelper.PrepareReservationEmailHtml(mailTemplateHelper.GetParsedReservationTemplate(reservationTemplateFields, mask: true), reservation.ReservationStatusType),
                        //          isCancelMail: reservation.ReservationStatusType == ReservationStatusTypes.Cancelled ? true : false),
                        //  headers: CreateBrokerAuthRequestHeader(user.Token)); 
                        #endregion

                        var postEmailRequest = CreateBrokerPostEmailRequestParameters(
                            typeName: $"LocationBranchMail{(reservation.ReservationStatusType == ReservationStatusTypes.Cancelled ? " - CANCEL" : "")}",
                            fromTitle: configurations.PortalOwnerTitle,
                            toMailAddress: vendorContactInformation?.Email,
                            subject:
                            $"{(resend ? "YENİDEN YÖNLENDİRME " : "")}{reservationTemplateFields.ReservationStatusName} - {reservation.ReservationNumber}",
                            body: EmailHelper.PrepareReservationEmailHtml(
                                mailTemplateHelper.GetParsedReservationTemplate(reservationTemplateFields, mask: true),
                                reservation.ReservationStatusType),
                            isCancelMail: reservation.ReservationStatusType == ReservationStatusTypes.Cancelled
                                ? true
                                : false);

                        await _emailService.PostEmail(
                         postEmailRequest.TypeName,
                         postEmailRequest.FromTitle,
                         postEmailRequest.ToMailAddress,
                         postEmailRequest.Subject,
                         postEmailRequest.Body,
                         postEmailRequest.ReplyTo, postEmailRequest.IsCancelMail, disableSsl: disableSsl);
                    }

                    //Lokasyon mail gönderimi

                    if (configurations.LocationEmailSending && pickupLocation != null && !string.IsNullOrEmpty(pickupLocation.MailAddress))
                    {
                        reservationTemplateFields.ReservationMailSendToType = ReservationMailSendToTypes.ToLocation;

                        #region Yorumda - gkursad
                        //restManager.PostAsync<PostEmailRequest, object>(
                        //    requestPath: "emails/longContent",
                        //    entity: CreateBrokerPostEmailRequestParameters(
                        //        typeName: "LocationMail",
                        //        fromTitle: configurations.PortalOwnerTitle,
                        //        toMailAddress: pickupLocation.MailAddress,
                        //        subject: $"{reservationTemplateFields.ReservationStatusName} - {reservation.ReservationNumber}",
                        //        body: EmailHelper.PrepareReservationEmailHtml(mailTemplateHelper.GetParsedReservationTemplate(reservationTemplateFields, mask: true), reservation.ReservationStatusType),
                        //         isCancelMail: reservation.ReservationStatusType == ReservationStatusTypes.Cancelled ? true : false),
                        //    headers: CreateBrokerAuthRequestHeader(user.Token));
                        #endregion

                        var postEmailRequest = CreateBrokerPostEmailRequestParameters(
                            typeName: $"LocationMail{(reservation.ReservationStatusType == ReservationStatusTypes.Cancelled ? " - CANCEL" : "")}",
                            fromTitle: configurations.PortalOwnerTitle,
                            toMailAddress: pickupLocation.MailAddress,
                            subject:
                            $"{(resend ? "YENİDEN YÖNLENDİRME " : "")}{reservationTemplateFields.ReservationStatusName} - {reservation.ReservationNumber}",
                            body: EmailHelper.PrepareReservationEmailHtml(
                                mailTemplateHelper.GetParsedReservationTemplate(reservationTemplateFields, mask: true),
                                reservation.ReservationStatusType),
                            isCancelMail: reservation.ReservationStatusType == ReservationStatusTypes.Cancelled
                                ? true
                                : false);

                        _emailService.PostEmail(
                         postEmailRequest.TypeName,
                         postEmailRequest.FromTitle,
                         postEmailRequest.ToMailAddress,
                         postEmailRequest.Subject,
                         postEmailRequest.Body,
                         postEmailRequest.ReplyTo, postEmailRequest.IsCancelMail, disableSsl: disableSsl);
                    }

                    //İptal ceza acente bilgilendirme maili
                    if (agency != null &&
                        !string.IsNullOrWhiteSpace(agency.Email) &&
                        configurations.PenaltyInformationEmailActive &&
                        (reservation.CancellationPenaltyAmount > 0 || reservation.PenaltyStatus == _penaltyStatus.NotApplied) &&
                        reservation.ReservationStatusType == ReservationStatusTypes.Cancelled)
                    {
                        reservationTemplateFields.ReservationMailSendToType = ReservationMailSendToTypes.ToAgency;

                        #region Yorumda - gkursad
                        //restManager.PostAsync<PostEmailRequest, object>(
                        //    requestPath: "emails/longContent",
                        //    entity: CreateBrokerPostEmailRequestParameters(
                        //        typeName: "CancelationFeeMail",
                        //        fromTitle: configurations.PortalOwnerTitle,
                        //        toMailAddress: agency.Email,
                        //        subject: $"{reservationTemplateFields.CancellationPenaltyInformationEmailTitle} - {reservation.ReservationNumber}",
                        //        body: EmailHelper.PrepareReservationEmailHtml(penaltyInformationEmailTemplateHelper.GetParsedReservationTemplate(reservationTemplateFields, mask: true), reservation.ReservationStatusType),
                        //        isCancelMail: reservation.ReservationStatusType == ReservationStatusTypes.Cancelled ? true : false),
                        //    headers: CreateBrokerAuthRequestHeader(user.Token));
                        #endregion

                        var postEmailRequest = CreateBrokerPostEmailRequestParameters(
                            typeName: $"CancelationFeeMail{(reservation.ReservationStatusType == ReservationStatusTypes.Cancelled ? " - CANCEL" : "")}",
                            fromTitle: configurations.PortalOwnerTitle,
                            toMailAddress: agency.Email,
                            subject:
                            $"{(resend ? "YENİDEN YÖNLENDİRME " : "")}{reservationTemplateFields.CancellationPenaltyInformationEmailTitle} - {reservation.ReservationNumber}",
                            body: EmailHelper.PrepareReservationEmailHtml(
                                penaltyInformationEmailTemplateHelper.GetParsedReservationTemplate(
                                    reservationTemplateFields, mask: true), reservation.ReservationStatusType),
                            isCancelMail: reservation.ReservationStatusType == ReservationStatusTypes.Cancelled
                                ? true
                                : false);

                        _emailService.PostEmail(
                         postEmailRequest.TypeName,
                         postEmailRequest.FromTitle,
                         postEmailRequest.ToMailAddress,
                         postEmailRequest.Subject,
                         postEmailRequest.Body,
                         postEmailRequest.ReplyTo,
                         postEmailRequest.IsCancelMail, disableSsl: disableSsl);
                    }
                    #endregion

                    #region SMS Gönderme
                    // İlk sms gönderimi / iptal sms gönderimi
                    if (configurations.SendSmsToCustomer && !string.IsNullOrEmpty(reservation.CustomerPhone))
                    {
                        var smsTemplate = new TemplateHelper(reservation.ReservationStatusType != ReservationStatusTypes.Cancelled ? reservationSmsTemplate : reservationCancelSmsTemplate);
                        #region Yorumda - gkursad
                        //restManager.PostAsync<PostSmsRequest, object>(
                        //requestPath: "sms",
                        //entity: new PostSmsRequest
                        //{
                        //    Content = smsTemplate.GetParsedReservationTemplate(reservationTemplateFields),
                        //    LanguageType = reservation.LanguageType,
                        //    PhoneNumber = reservation.CustomerPhone.Replace(" ", String.Empty)
                        //},
                        //headers: CreateBrokerAuthRequestHeader(user.Token)); 
                        #endregion

                        await _smsService.PostSms(new PostSmsRequest
                        {
                            Content = smsTemplate.GetParsedReservationTemplate(reservationTemplateFields),
                            LanguageType = reservation.LanguageType,
                            PhoneNumber = reservation.CustomerPhone.Replace(" ", String.Empty)
                        })
                           .ConfigureAwait(false);
                    }

                    //Müşteri SMS gönderimi
                    if (configurations.SendSmsToCustomer && !string.IsNullOrEmpty(reservation.CustomerPhone))
                    {
                        foreach (var sms in smsList)
                        {
                            var smsTemplate = new TemplateHelper(reservation.ReservationStatusType != ReservationStatusTypes.Cancelled ? sms.Editor : reservationCancelSmsTemplate);

                            #region Yorumda - gkursad
                            //restManager.PostAsync<PostSmsRequest, object>(
                            //requestPath: "sms",
                            //entity: new PostSmsRequest
                            //{
                            //    Content = smsTemplate.GetParsedReservationTemplate(reservationTemplateFields),
                            //    LanguageType = reservation.LanguageType,
                            //    PhoneNumber = reservation.CustomerPhone.Replace(" ", String.Empty)
                            //},
                            //headers: CreateBrokerAuthRequestHeader(user.Token)); 
                            #endregion

                            await _smsService.PostSms(new PostSmsRequest
                            {
                                Content = smsTemplate.GetParsedReservationTemplate(reservationTemplateFields),
                                LanguageType = reservation.LanguageType,
                                PhoneNumber = reservation.CustomerPhone.Replace(" ", String.Empty)
                            })
                                .ConfigureAwait(false);
                        }

                    }

                    //Müşteri SMS (Kampanya) gönderimi
                    if (configurations.SendSmsToCustomer && !string.IsNullOrEmpty(reservation.CustomerPhone) && configurations.RezIlaveSMSAktif)
                    {
                        #region Yorumda - gkursad
                        //restManager.PostAsync<PostSmsRequest, object>(
                        //    requestPath: "sms",
                        //    entity: new PostSmsRequest
                        //    {
                        //        Content = smsTemplateHelperTest.GetParsedReservationTemplate(reservationTemplateFields),
                        //        LanguageType = reservation.LanguageType,
                        //        PhoneNumber = reservation.CustomerPhone.Replace(" ", String.Empty)
                        //    },
                        //    headers: CreateBrokerAuthRequestHeader(user.Token)); 
                        #endregion

                        _smsService.PostSms(new PostSmsRequest
                        {
                            Content = smsTemplateHelperTest.GetParsedReservationTemplate(reservationTemplateFields),
                            LanguageType = reservation.LanguageType,
                            PhoneNumber = reservation.CustomerPhone.Replace(" ", String.Empty)
                        })
                           .ConfigureAwait(false);
                    }

                    //Kazan Kazan sms gönderimi
                    if (configurations.SendSmsToCustomer && !string.IsNullOrEmpty(reservation.CustomerPhone) && configurations.KazanKazanSmsAktif && coupon != null)
                    {
                        #region Yorumda - gkursad
                        //restManager.PostAsync<PostSmsRequest, object>(
                        //    requestPath: "sms",
                        //    entity: new PostSmsRequest
                        //    {
                        //        Content = couponsmstemplate.GetParsedReservationTemplate(reservationTemplateFields),
                        //        LanguageType = reservation.LanguageType,
                        //        PhoneNumber = reservation.CustomerPhone.Replace(" ", String.Empty)
                        //    },
                        //    headers: CreateBrokerAuthRequestHeader(user.Token)); 
                        #endregion

                        _smsService.PostSms(new PostSmsRequest
                        {
                            Content = couponsmstemplate.GetParsedReservationTemplate(reservationTemplateFields),
                            LanguageType = reservation.LanguageType,
                            PhoneNumber = reservation.CustomerPhone.Replace(" ", String.Empty)
                        })
                           .ConfigureAwait(false);
                    }
                    #endregion
                }
                else
                    Serilog.Log.Error("{EmailAuthenticationFailed}", "EmailAuthenticationFailed");
            }
        }

        public async Task<ReservateNowDtoMobile> Recalculate(ReservateNowDtoMobile reservateNow)
        {
            if (reservateNow.SpecialDailyPrice == "-1" || string.IsNullOrEmpty(reservateNow.SpecialDailyPrice))
            {
                #region Reservation detaylarını tekrar al
                var reservationCrytedData =
                    await _reservationTokenService.GetReservationToken(reservateNow.ReservationToken);
                var reservationData = reservationCrytedData.DecryptAES256();
                var reservation =
                    reservationData.ValidateJson() ?
                        JsonConvert.DeserializeObject<ReservationToken>(reservationData) :
                        Activator.CreateInstance<ReservationToken>();

                var couponResult = Activator.CreateInstance<CouponDetailDto>();
                var selectedExtras = Enumerable.Empty<ReservationDetailSelectedExtra>();
                #endregion

                #region Kupon detayları alınıyor
                if (!string.IsNullOrEmpty(reservateNow.CouponCode))
                {
                    var couponProcedureParameters = new GetCouponDetailsResponseDto()
                    {
                        CouponCode = reservateNow.CouponCode,
                        CustomerMailAddress = reservateNow.Email,
                        LanguageId = reservateNow.LanguageId,
                        CurrencyId = reservateNow.CurrencyId,
                        VendorId = reservation.VendorId,
                        TotalPrice = reservateNow.TotalPrice.ToString(),
                        PickupDate = reservation.PickupDateTime.ToString(),
                        ReturnDate = reservation.ReturnDateTime.ToString(),
                        PickupLocationId = reservation.PickupLocationId,
                        ReturnLocationId = reservation.ReturnLocationId,
                        RentalDuration = reservateNow.RentalDuration.ToInt(),
                        AgencyId = (int)reservation.AgencyId
                    };
                    couponResult = await _couponService.GetCouponResults(couponProcedureParameters);
                }
                #endregion

                #region Extralar
                if (!string.IsNullOrEmpty(reservateNow.SelectedExtrasJson) && reservateNow.SelectedExtrasJson.ValidateJson())
                    selectedExtras = JsonConvert.DeserializeObject<IEnumerable<ReservationDetailSelectedExtra>>(reservateNow.SelectedExtrasJson);
                #endregion

                #region Hesaplamaları tekrar yapılıyor
                var totalPrice = (decimal)reservation.DailyPrice * reservation.RentalDuration; // Toplam Tutar
                var couponDiscountAmount = (couponResult?.DiscountAmount ?? 0) > totalPrice ? totalPrice : (couponResult?.DiscountAmount ?? 0); // Kupon İndirimi
                var totalPriceAfterCoupon = totalPrice - couponDiscountAmount; // Kupon İndirimi sonrası toplam tutar
                // Vade Farkı
                var installmentCommissionAmount = (reservateNow.InstallmentPercent ?? 0) > 0
                    ? Math.Round((totalPriceAfterCoupon * ((reservateNow.InstallmentPercent ?? 0) / 100)), 2)
                    : 0;

                // Kupn tutarı düşülmüş tutara vade farkı ekleniyor
                var totalPriceAfterInstallmentCommission = totalPriceAfterCoupon + installmentCommissionAmount;
                //Servis Ücreti Ekleniyor
                //totalPriceAfterInstallmentCommission += reservateNow.ServiceCharge ?? 0;

                //Extraların toplam tutarı
                var extraAmount = selectedExtras != null
                    ? selectedExtras.Sum(e => e.ExtraRentalType == 0 ? ((decimal)e.Price) : (decimal)(e.RentalDuration * e.Price))
                    : 0;

                //Ek ürün ve Tek Yön Acente Teslimat Ödemesi Bilgisine Göre KK tutarına ekleme yapılıyor.
                if (!reservateNow.AdditionalProductPricePoa.ToBool())
                    totalPriceAfterInstallmentCommission += extraAmount;
                if (!reservateNow.OneWayFeePoa.ToBool())
                    totalPriceAfterInstallmentCommission += reservateNow.OneWayFee.ToDecimal();
                // Ödenecek tutar
                var paidAmount = reservateNow.PaymentType switch
                {
                    PaymentTypes.PayAll => (totalPriceAfterInstallmentCommission),
                    PaymentTypes.AdvancePayment => reservateNow.PaidAmount,
                    PaymentTypes.CommissionFree => (totalPriceAfterInstallmentCommission - couponDiscountAmount) + extraAmount,
                    PaymentTypes.PayOnDelivery => 0,
                    PaymentTypes.PayToAgency => (totalPriceAfterInstallmentCommission - couponDiscountAmount) + extraAmount,
                    _ => (totalPriceAfterInstallmentCommission - couponDiscountAmount) + extraAmount,
                };
                #endregion

                reservateNow.FullCredit = CreditHelper.ResolveTokenCreditType(reservation) == CreditType.FullCredit;

                reservateNow.TotalPrice = totalPrice; // Günlük ücret x Kiralama süresi
                reservateNow.InstallmentCommissionAmount = installmentCommissionAmount; // Vade Farkı
                reservateNow.PaidAmount = paidAmount; // Ödenecek tutar
                reservateNow.CouponDiscountAmount = couponDiscountAmount; // Kupon indirimi
            }
            else if (reservateNow.SpecialDailyPrice != "-1" && !string.IsNullOrEmpty(reservateNow.SpecialDailyPrice))
            {
                if (reservateNow.FullCredit != null && !(bool)reservateNow.FullCredit && !reservateNow.AdditionalProductPricePoa.ToBool())
                {
                    reservateNow.PaidAmount = reservateNow.PaidAmount + reservateNow.ExtraAmount.ToDecimal();
                    reservateNow.TotalPrice = reservateNow.TotalPrice + reservateNow.ExtraAmount.ToDecimal();
                }
            }
            return reservateNow;
        }

        public async Task<bool> CheckReservationTokenExists(string reservationToken)
        {
            return await _context.Rez.AnyAsync(r => r.Reservationtokentext == reservationToken);
        }

        private PostEmailRequest CreateBrokerPostEmailRequestParameters(string typeName, string fromTitle, string toMailAddress, string subject, string body, bool isCancelMail = false)
        {
            return new PostEmailRequest
            {
                TypeName = typeName,
                FromTitle = fromTitle,
                ToMailAddress = toMailAddress,
                Subject = subject,
                Body = body,
                IsCancelMail = isCancelMail
            };
        }

        public static Dictionary<string, object> CreateBrokerAuthRequestHeader(string bearer)
        {
            return new Dictionary<string, object>()
            {
                { "Authorization", $"Bearer {bearer}" }
            };
        }

        private async Task<float> CalculateAPIPaidAmount(CommonModels.Agency agency, CommonModels.Vendor vendor, ReservationToken reservationToken, Reservation reservation, PostReservationRequest postReservationRequest, List<Extra> apiExtras)
        {
            float dailyPrice, oneWayFee, extraPrice;

            postReservationRequest.ExtraAmount = await _reservationStepsService.CurrencyExchange(vendor, postReservationRequest.ExtraAmount, reservationToken.CurrencyType, reservationToken.BaseVendorRequestCurrencyType);
            postReservationRequest.PaidAmount = await _reservationStepsService.CurrencyExchange(vendor, postReservationRequest.PaidAmount, reservationToken.CurrencyType, reservationToken.BaseVendorRequestCurrencyType);

            if (postReservationRequest.SpecialDailyPrice != -1)
                postReservationRequest.SpecialDailyPrice = await _reservationStepsService.CurrencyExchange(vendor, postReservationRequest.SpecialDailyPrice, reservationToken.CurrencyType, reservationToken.BaseVendorRequestCurrencyType);

            if (postReservationRequest.SpecialOneWayFee != -1)
                postReservationRequest.SpecialOneWayFee = await _reservationStepsService.CurrencyExchange(vendor, postReservationRequest.SpecialOneWayFee, reservationToken.CurrencyType, reservationToken.BaseVendorRequestCurrencyType);

            reservationToken.DailyPrice = await _reservationStepsService.CurrencyExchange(vendor, reservation.DailyPrice, reservationToken.CurrencyType, reservationToken.BaseVendorRequestCurrencyType);
            reservationToken.OneWayFee = await _reservationStepsService.CurrencyExchange(vendor, reservation.OneWayFee, reservationToken.CurrencyType, reservationToken.BaseVendorRequestCurrencyType);

            if (agency.FreePriceShowActive || postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery)
            {
                dailyPrice = CalculationHelper.GetAPIDailyPrice(agency, reservationToken, postReservationRequest, reservation);
                oneWayFee = CalculationHelper.GetAPIOneWayFee(agency, reservationToken, postReservationRequest, reservation);
                extraPrice = CalculationHelper.GetAPIExtraPrice(agency, reservationToken, postReservationRequest, reservation);
            }
            else
            {
                if (reservationToken.SpecialProfitApplied != null && reservationToken.SpecialProfitApplied != "")
                    //kar marjı geliştirmesi için apiDailyPrice değişmeyecek bu yüzden bu kısmı sabit tutmak gerekiyor.
                    dailyPrice = reservationToken.APIDailyPrice;
                else
                {
                    if (reservationToken.DailyPrice > reservationToken.APIDailyPrice)
                        dailyPrice = reservationToken.APIDailyPrice;
                    else
                        dailyPrice = reservationToken.DailyPrice;
                }

                if (postReservationRequest.OneWayFeePayToDelivery)
                    oneWayFee = 0;
                else if (vendor.OneWayFeeWorkingType == VendorWorkingTypes.Commission)
                    oneWayFee = reservationToken.OneWayFee;
                else
                    oneWayFee = reservationToken.APIOneWayFee;

                if (postReservationRequest.ExtraPricePayToDelivery)
                    extraPrice = 0;
                else if (vendor.AdditionalProductWorkingType == VendorWorkingTypes.Commission)
                    extraPrice = reservation.ExtraPrice;
                else
                    extraPrice = reservation.APIExtraAmount;
            }

            switch (postReservationRequest.PaymentType)
            {
                default:
                case PaymentTypes.PayToAgency:
                case PaymentTypes.PayAll:
                case PaymentTypes.CommissionFree:
                    {
                        return CalculationHelper.CalculateTotalPrice(vendor.PriceRoundingType, reservationToken.RentalDuration, dailyPrice, extraPrice, oneWayFee);
                    }
                case PaymentTypes.PayOnDelivery:
                    {
                        return !postReservationRequest.AdvancedPaymentWithoutPayment ? 0 : postReservationRequest.PaidAmount;
                    }
                case PaymentTypes.AdvancePayment:
                    {
                        var isSpecialExtraPriceUse = BrokerReservationHelper.IsSpecialExtraPriceUse(postReservationRequest.ExtraList, apiExtras, vendor.ProfitMarkupAdditionalProducts, postReservationRequest.PaymentType, postReservationRequest, agency, vendor);

                        if ((agency.FreePriceShowActive && postReservationRequest.SpecialDailyPrice != -1) ||
                            (agency.FreePriceShowActive && postReservationRequest.SpecialOneWayFee != -1) ||
                            agency.AdvancePaymentAmountByAgencyCommissionActive ||
                            isSpecialExtraPriceUse ||
                            vendor.RentalWorkingType == VendorWorkingTypes.Commission ||
                            vendor.AdditionalProductWorkingType == VendorWorkingTypes.Commission ||
                            vendor.OneWayFeeWorkingType == VendorWorkingTypes.Commission)
                        {
                            if ((agency.FreePriceShowActive && postReservationRequest.SpecialDailyPrice != -1) ||
                            (agency.FreePriceShowActive && postReservationRequest.SpecialOneWayFee != -1) ||
                            agency.AdvancePaymentAmountByAgencyCommissionActive ||
                            isSpecialExtraPriceUse)
                            {
                                return postReservationRequest.PaidAmount;
                            }
                            else
                            {
                                float commissionPrice = 0;
                                if (vendor.RentalWorkingType == VendorWorkingTypes.Commission)
                                    commissionPrice += CalculationHelper.GetCommissionPrice(vendor.ProfitMarkupDailyPrice, vendor.PriceRoundingType, reservationToken.APIDailyPrice * reservationToken.RentalDuration);
                                if (vendor.AdditionalProductWorkingType == VendorWorkingTypes.Commission)
                                    commissionPrice += CalculationHelper.GetCommissionPrice(vendor.ProfitMarkupAdditionalProducts, vendor.PriceRoundingType, extraPrice);
                                if (vendor.OneWayFeeWorkingType == VendorWorkingTypes.Commission)
                                    commissionPrice += CalculationHelper.GetCommissionPrice(vendor.ProfitMarkupOneWayFee, vendor.PriceRoundingType, oneWayFee);

                                return commissionPrice;
                            }
                        }
                        else
                            return postReservationRequest.PaidAmount; // TODO: test ediliyor. eski degeri: 0
                    }
            }
        }
        private async Task<float> CalculateAPIPaidAmount(CommonModels.Agency agency, CommonModels.Vendor vendor, ReservationToken reservationToken, Reservation reservation, Domain.Models.Requests.PostReservationRequestV2 postReservationRequest, List<Extra> apiExtras)
        {
            float dailyPrice, oneWayFee, extraPrice;

            postReservationRequest.Pricing.ExtraAmount = await _reservationStepsService.CurrencyExchange(vendor, postReservationRequest.Pricing.ExtraAmount, reservationToken.CurrencyType, reservationToken.BaseVendorRequestCurrencyType);
            postReservationRequest.Pricing.PaidAmount = await _reservationStepsService.CurrencyExchange(vendor, postReservationRequest.Pricing.PaidAmount, reservationToken.CurrencyType, reservationToken.BaseVendorRequestCurrencyType);

            if (postReservationRequest.Pricing.SpecialDailyPrice != -1)
                postReservationRequest.Pricing.SpecialDailyPrice = await _reservationStepsService.CurrencyExchange(vendor, postReservationRequest.Pricing.SpecialDailyPrice, reservationToken.CurrencyType, reservationToken.BaseVendorRequestCurrencyType);

            if (postReservationRequest.Pricing.SpecialOneWayFee != -1)
                postReservationRequest.Pricing.SpecialOneWayFee = await _reservationStepsService.CurrencyExchange(vendor, postReservationRequest.Pricing.SpecialOneWayFee, reservationToken.CurrencyType, reservationToken.BaseVendorRequestCurrencyType);

            reservationToken.DailyPrice = await _reservationStepsService.CurrencyExchange(vendor, reservation.DailyPrice, reservationToken.CurrencyType, reservationToken.BaseVendorRequestCurrencyType);
            reservationToken.OneWayFee = await _reservationStepsService.CurrencyExchange(vendor, reservation.OneWayFee, reservationToken.CurrencyType, reservationToken.BaseVendorRequestCurrencyType);

            if (agency.FreePriceShowActive || postReservationRequest.Payment.PaymentType == PaymentTypes.PayOnDelivery)
            {
                dailyPrice = CalculationHelper.GetAPIDailyPrice(agency, reservationToken, postReservationRequest, reservation);
                oneWayFee = CalculationHelper.GetAPIOneWayFee(agency, reservationToken, postReservationRequest, reservation);
                extraPrice = CalculationHelper.GetAPIExtraPrice(agency, reservationToken, postReservationRequest, reservation);
            }
            else
            {
                if (reservationToken.SpecialProfitApplied != null && reservationToken.SpecialProfitApplied != "")
                    //kar marjı geliştirmesi için apiDailyPrice değişmeyecek bu yüzden bu kısmı sabit tutmak gerekiyor.
                    dailyPrice = reservationToken.APIDailyPrice;
                else
                {
                    if (reservationToken.DailyPrice > reservationToken.APIDailyPrice)
                        dailyPrice = reservationToken.APIDailyPrice;
                    else
                        dailyPrice = reservationToken.DailyPrice;
                }

                if (postReservationRequest.Payment.OneWayFeePayToDelivery)
                    oneWayFee = 0;
                else if (vendor.OneWayFeeWorkingType == VendorWorkingTypes.Commission)
                    oneWayFee = reservationToken.OneWayFee;
                else
                    oneWayFee = reservationToken.APIOneWayFee;

                if (postReservationRequest.Payment.ExtraPricePayToDelivery)
                    extraPrice = 0;
                else if (vendor.AdditionalProductWorkingType == VendorWorkingTypes.Commission)
                    extraPrice = reservation.ExtraPrice;
                else
                    extraPrice = reservation.APIExtraAmount;
            }

            switch (postReservationRequest.Payment.PaymentType)
            {
                default:
                case PaymentTypes.PayToAgency:
                case PaymentTypes.PayAll:
                case PaymentTypes.CommissionFree:
                    {
                        return CalculationHelper.CalculateTotalPrice(vendor.PriceRoundingType, reservationToken.RentalDuration, dailyPrice, extraPrice, oneWayFee);
                    }
                case PaymentTypes.PayOnDelivery:
                    {
                        return !postReservationRequest.Payment.AdvancedPaymentWithoutPayment ? 0 : postReservationRequest.Pricing.PaidAmount;
                    }
                case PaymentTypes.AdvancePayment:
                    {
                        var isSpecialExtraPriceUse = true; // 20.10.2025 kontrol edilecek
                                                           // var isSpecialExtraPriceUse = BrokerReservationHelper.IsSpecialExtraPriceUse(postReservationRequest.ExtraList, apiExtras, vendor.ProfitMarkupAdditionalProducts, postReservationRequest.Payment.PaymentType, postReservationRequest, agency, vendor);

                        if ((agency.FreePriceShowActive && postReservationRequest.Pricing.SpecialDailyPrice != -1) ||
                            (agency.FreePriceShowActive && postReservationRequest.Pricing.SpecialOneWayFee != -1) ||
                            agency.AdvancePaymentAmountByAgencyCommissionActive ||
                            isSpecialExtraPriceUse ||
                            vendor.RentalWorkingType == VendorWorkingTypes.Commission ||
                            vendor.AdditionalProductWorkingType == VendorWorkingTypes.Commission ||
                            vendor.OneWayFeeWorkingType == VendorWorkingTypes.Commission)
                        {
                            if ((agency.FreePriceShowActive && postReservationRequest.Pricing.SpecialDailyPrice != -1) ||
                            (agency.FreePriceShowActive && postReservationRequest.Pricing.SpecialOneWayFee != -1) ||
                            agency.AdvancePaymentAmountByAgencyCommissionActive ||
                            isSpecialExtraPriceUse)
                            {
                                return postReservationRequest.Pricing.PaidAmount;
                            }
                            else
                            {
                                float commissionPrice = 0;
                                if (vendor.RentalWorkingType == VendorWorkingTypes.Commission)
                                    commissionPrice += CalculationHelper.GetCommissionPrice(vendor.ProfitMarkupDailyPrice, vendor.PriceRoundingType, reservationToken.APIDailyPrice * reservationToken.RentalDuration);
                                if (vendor.AdditionalProductWorkingType == VendorWorkingTypes.Commission)
                                    commissionPrice += CalculationHelper.GetCommissionPrice(vendor.ProfitMarkupAdditionalProducts, vendor.PriceRoundingType, extraPrice);
                                if (vendor.OneWayFeeWorkingType == VendorWorkingTypes.Commission)
                                    commissionPrice += CalculationHelper.GetCommissionPrice(vendor.ProfitMarkupOneWayFee, vendor.PriceRoundingType, oneWayFee);

                                return commissionPrice;
                            }
                        }
                        else
                            return postReservationRequest.Pricing.PaidAmount; // TODO: test ediliyor. eski degeri: 0
                    }
            }
        }
        private async Task<string> GetReservationSourceName(int? reservationSourceId)
        {
            if (reservationSourceId != null)
            {
                var source = await _context.Ressource.Where(x => x.Id == reservationSourceId).FirstOrDefaultAsync();
                return source != null ? source.Sourcename : string.Empty;
            }
            else
                return string.Empty;
        }
        private async Task<string> GetReservationDetailPageContentUrl(int? langId)
        {
            if (langId != null)
                return await _context.Icerikdil.Where(x => x.Icerikid == -18 && x.Dilid == langId).Select(x => x.Contenturl).FirstOrDefaultAsync() ?? "";
            return "";
        }
        private float GetReservationVendorScore(VendorScoreDto vendorScoreDto)
        {
            return vendorScoreDto.CurrentScore.ToFloatNullSafe() != 0 && vendorScoreDto.CurrentScore != -1 ? vendorScoreDto.CurrentScore.ToFloatNullSafe() : vendorScoreDto.Score != 0 ? vendorScoreDto.Score.ToFloatNullSafe() : 4;
        }
        private int GetReservationVendorCommentCount(VendorScoreDto vendorScoreDto)
        {
            return vendorScoreDto.CommentCount;
        }
        private async Task<VendorScoreDto> GetReservationVendorSurveyStatics(int vendorId, int pickupLocationId, int langId)
        {
            var vendorScoreResult = await _context.VendorScoresDto.FromSqlRaw("EXEC GETVENDORSCOREBYLOCATIONID @languageId = {0}, @vendorId = {1}, @locationId = {2}", langId, vendorId, pickupLocationId).ToListAsync();
            var vendorScoreDto = vendorScoreResult.FirstOrDefault();
            return vendorScoreDto != null ? vendorScoreDto : new VendorScoreDto
            {
                CommentCount = 0,
                Score = 4,
                CurrentScore = -1
            };
        }

        private async Task<ReservationPenalty> GetReservationCancelletionPenalty(Reservation reservation, CommonModels.Agency agency, _penaltyStatus penaltyStatus)
        {
            bool ApplyPenalty = false;
            bool ServicePricePenalty = (await _context.Parametre.Where(x => x.Degisken == "AddServicePriceToPenalty").FirstAsync()).Deger.ToBoolNullSafe();
            bool NoShow = reservation.PickupDate <= DateTime.Now && (await _context.Parametre.Where(x => x.Degisken == "NoShowPenalty").FirstAsync()).Deger.ToBoolNullSafe();

            if (penaltyStatus == _penaltyStatus.None || penaltyStatus == _penaltyStatus.Applied)
                ApplyPenalty = true;
            if (reservation == null || agency == null) new ReservationPenalty();

            if (ApplyPenalty && reservation.PaidAmount > 0 && reservation.ReservationStatusType != ReservationStatusTypes.Cancelled)
            {
                if (agency.CancellationPenaltyActive)
                {
                    if (true/*reservation.PaymentType == PaymentTypes.PayAll || reservation.PaymentType == PaymentTypes.AdvancePayment || reservation.PaymentType == PaymentTypes.PayToAgency*/)
                    {
                        if (await _vendorService.GetVendorById(reservation.VendorId) is CommonModels.Vendor vendor && vendor != null)
                        {
                            int vendorFreeCancellationHour = vendor.FreeCancellationHour;
                            var reservationCancellationFeeList = await _context.Reservationcancellationfee.Where(x => x.Vendorid == reservation.VendorId).ToListAsync();

                            if (DateTime.Now > reservation.ReservationDate.AddHours(vendorFreeCancellationHour) || vendor.PRIORITYFEE.ToBoolNullSafe())
                            {
                                if (reservationCancellationFeeList != null && reservationCancellationFeeList.Count > 0)
                                {
                                    foreach (var reservationCancellationFee in reservationCancellationFeeList)
                                    {
                                        if (reservation.PickupDate > DateTime.Now || NoShow)
                                        {
                                            TimeSpan difference = NoShow ? TimeSpan.FromHours(1) : reservation.PickupDate - DateTime.Now;
                                            int differenceHour = difference.Days * 24 + difference.Hours + (difference.Minutes > 0 ? 1 : 0);

                                            if (differenceHour != 0)
                                            {
                                                int minRange = reservationCancellationFee.Minrange;
                                                int maxRange = reservationCancellationFee.Maxrange;
                                                int percent = reservationCancellationFee.Pricepercentage;

                                                if (percent != 0)
                                                {
                                                    if (minRange <= differenceHour && differenceHour <= maxRange)
                                                    {
                                                        var servicePrice = ServicePricePenalty ? 0 : reservation.ServiceCharge;
                                                        var refundAmountBase = reservation.PaidAmount;
                                                        var agencyCommission = ReservationHelper.GetAgencyCommissionWithAgencyProfitMarkup(1, reservation.AgencyRentalProfitMarkup, reservation.AgencyCommission);
                                                        var agencyPrice = reservation.DailyPrice * reservation.RentalDuration * agencyCommission / 100;

                                                        if (reservation.AgencyCommission > 0 || reservation.AgencyRentalProfitMarkup > 0)
                                                        {
                                                            refundAmountBase = reservation.PaymentType != PaymentTypes.AdvancePayment ?
                                                                reservation.DailyPrice * reservation.RentalDuration * (100 - agencyCommission) / 100 + reservation.ExtraPrice + reservation.OneWayFee - reservation.CouponDiscountAmount :
                                                                reservation.PaidAmount;
                                                        }
                                                        else if (reservation.ServiceCharge != 0)
                                                            refundAmountBase -= servicePrice - reservation.CouponDiscountAmount;

                                                        var refundAmount = reservation.PaymentType != PaymentTypes.AdvancePayment ?
                                                            refundAmountBase * (100 - percent) / 100 + agencyPrice + servicePrice :
                                                            reservation.TotalPrice - reservation.PaidAmount + refundAmountBase * (100 - percent) / 100;

                                                        var penaltyAmount = reservation.TotalPrice - refundAmount;

                                                        return new ReservationPenalty
                                                        {
                                                            DifferenceHour = differenceHour,
                                                            PenaltyRate = percent,
                                                            PenaltyAmount = penaltyAmount,
                                                            RefundedAmount = refundAmount
                                                        };
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return new ReservationPenalty
            {
                RefundedAmount = reservation.PaidAmount
            };
        }

        public async Task SetVendorLocalContactInformations(Reservation reservation)
        {
            if (reservation != null)
            {
                var vendorPickupContactInformation = await _context.Vendorcontactinformation.FirstOrDefaultAsync(x =>
                                x.Vendorid == reservation.VendorId && x.Locationid == reservation.PickupLocationId);
                var vendorReturnContactInformation = await _context.Vendorcontactinformation.FirstOrDefaultAsync(x =>
                    x.Vendorid == reservation.VendorId && x.Locationid == reservation.ReturnLocationId);

                if (vendorPickupContactInformation != null)
                {
                    if (!string.IsNullOrEmpty(vendorPickupContactInformation.Address))
                        reservation.APIVendorPickupAddress = vendorPickupContactInformation.Address;

                    if (!string.IsNullOrEmpty(vendorPickupContactInformation.Phonenumber))
                        reservation.APIVendorPickupPhone = vendorPickupContactInformation.Phonenumber;
                }

                if (vendorReturnContactInformation != null)
                {
                    if (!string.IsNullOrEmpty(vendorReturnContactInformation.Address))
                        reservation.APIVendorReturnAddress = vendorReturnContactInformation.Address;

                    if (!string.IsNullOrEmpty(vendorReturnContactInformation.Phonenumber))
                        reservation.APIVendorReturnPhone = vendorReturnContactInformation.Phonenumber;
                }
            }
        }

        public bool CheckReservationRefundStatus(Reservation reservation)
        {
            try
            {
                var res = _context.Rez.Where(x => x.Rezno == reservation.ReservationNumber && x.Musterieposta == reservation.CustomerMail).FirstOrDefault();
                return res != null ? (!res.Paymentrefundsuccess ?? true) : true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@CheckReservationRefundStatus}", ex.Message);
                return true;
            }
        }
        public async Task<string> GetContractUrl(LanguageTypes languageTypes, int vendorId)
        {
            // TODO : SELECT (SELECT DEGER FROM PARAMETRE WHERE DEGISKEN = 'PODOMAIN') + '/' + CONTENTURL FROM ICERIKDIL WHERE ICERIKID = (SELECT ICERIKID FROM ICERIK WHERE TIPID = 10 AND VENDORID = 16) AND DILID = 1
            string domain = await _configurationService.GetConfigurationValueByFieldName<string>("PODOMAIN");
            string vendorContractUrl = await _contentService.GetContractUrlByVendorId(languageTypes, vendorId);

            if (vendorContractUrl == null || vendorContractUrl == "")
                vendorContractUrl = "kiralama-kosullari";

            return domain + "/" + vendorContractUrl;
        }
        public async Task SetVendorAddress(Reservation reservation)
        {
            try
            {
                var agency = await _context.Agency.Where(x => x.Agencyid == _currentAgencyId).FirstOrDefaultAsync();

                if (agency != null && agency.SpecialParameters.ToBoolNullSafe())
                {
                    reservation.Vendor = reservation.VendorName;
                    reservation.PickupAddress = reservation.APIVendorPickupAddress;
                    reservation.ReturnAddress = reservation.APIVendorReturnAddress;
                    reservation.VendorPickupPhone = reservation.APIVendorPickupPhone;
                    reservation.VendorReturnPhone = reservation.APIVendorReturnPhone;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@SetVendorAddressError}", ex.Message);
            }
        }

        public async Task<List<Rez>> GetReservationsByEmail(string email)
        {
            return await _context.Rez.Where(r => r.Musterieposta == email).ToListAsync();
        }

        public async Task<List<Rez>> GetCanceledReservations(DateTime startDate, DateTime endDate)
        {
            return await _context.Resstatushistory
                                             .Where(r =>
                                                    r.Resstatusid == -4 &&
                                                    r.Inserteddate >= startDate &&
                                                    r.Inserteddate <= endDate)
                                             .Select(r => r.Rez)
                                             .ToListAsync();
        }

        public Rez GetCanceledReservationsByRezToken(string rezToken)
        {
            return _context.Rez.FirstOrDefault(r => r.Reservationtokentext == rezToken);
        }

        public async Task<ServiceResponseBase> PostReservationLocalV2(Domain.Models.Requests.PostReservationRequestV2 postReservationRequestV2, Domain.Models.Agency agency, ReservationToken reservationToken, Domain.Models.Vendor vendor, long reservationId, List<Extra> extras = null)
        {
            if (reservationToken == null)
                return new(null, false, "Check your reservationToken!");

            if (agency == null)
            {
                Serilog.Log.Error("Acente bilgisine ulaşılamadı!");
                return new(null, false, "Agency information not available!");
            }

            Serilog.Log.Error("{@ReservationLocalAgency}", agency);

            if (vendor == null)
            {
                Serilog.Log.Error("Tedarikçi bilgisine ulaşılamadı!");
                return new(null, false, "Vendor information not available!");
            }

            ReservationHelper.FillPostReservationRequest(postReservationRequestV2, reservationToken, agency);

            await _agencyService.SetAgencyPaymentOptions(agency, vendor);
            Serilog.Log.Error("{@ReservationLocalVendor}", vendor);

            float apiExtraAmount = extras.Sum(e => e.ExtraRentalType == ExtraRentalTypes.PerRental ? e.ApiPrice : e.ApiPrice * reservationToken.RentalDuration);
            var totalExtraAmount = extras.Sum(e => e.ExtraRentalType == ExtraRentalTypes.PerRental ? e.Price : e.Price * reservationToken.RentalDuration);
            var premiumPackets = extras.Where(e => e.ExtraType == AdditionalProductTypes.Premium).ToList();
            var premiumPacketsPrice = premiumPackets.Sum(item => item.Price * (item.ExtraRentalType == ExtraRentalTypes.PerRental ? 1 : reservationToken.RentalDuration));
            var localExtras = vendor.ExtraMappingActive
                ? await _extraService.GetActiveLocalExtras(postReservationRequestV2.LanguageCode.ToEnum<LanguageTypes>())
                : new List<Extra>();
            postReservationRequestV2.Pricing.ExtraAmount = totalExtraAmount;

            string extraListString = GetExtraListString(extras, agency, localExtras, vendor.ExtraMappingActive).Replace(",", ".");

            if (postReservationRequestV2.Payment.InstallmentCount != 0 && postReservationRequestV2.Payment.PaymentType == PaymentTypes.PayAll)
            {
                reservationToken.DailyPricePayNow = GetDailyPriceOfInstallmentByAgencySettings(agency, postReservationRequestV2, reservationToken);
            }
            var exchangeRates = await _exchangeRateService.GetAllExchangeRates();
            var mappedExchangeRates = exchangeRates.Map();
            Serilog.Log.Error("{@ReservationLocalExchangeRates}", mappedExchangeRates);

            var configurations = await _configurationService.GetConfigurations();

            Serilog.Log.Error("{@Configurations}", configurations);

            var vendorLogoUrl = "";
            if (vendor.VendorType == VendorTypes.Yolcu360 || vendor.VendorType == VendorTypes.Yolcu360v2)
                vendorLogoUrl = reservationToken.APIVendorLogo ?? string.Empty;
            else
                vendorLogoUrl = vendor.VendorType != VendorTypes.KolayCARBroker || (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations) ? $"{configurations.PortalOwnerDomain}{reservationToken.APIVendorLogo}" : reservationToken.APIVendorLogo ?? string.Empty;

            if (postReservationRequestV2.MemberId == null)
            {
                postReservationRequestV2.MemberId = await _memberService.GetFirstMemberIdByAgencyId(postReservationRequestV2.Agency.AgencyId.ToIntNullSafe());
            }

            //var dailyPrice = postReservationRequestV2.Pricing.SpecialDailyPrice <= 0
            //        ? (postReservationRequestV2.Payment.PaymentType != PaymentTypes.PayAll
            //            ? reservationToken.DailyPrice
            //            : reservationToken.DailyPricePayNow)
            //        : postReservationRequestV2.Pricing.SpecialDailyPrice;

            var dailyPrice = postReservationRequestV2.Pricing.SpecialDailyPrice > 0 ? postReservationRequestV2.Pricing.SpecialDailyPrice : reservationToken.DailyPrice;

            var totalAmount =
                CalculationHelper.CalculateTotalPrice(
                    vendor.PriceRoundingType,
                    reservationToken.RentalDuration,
                    dailyPrice,
                    totalExtraAmount,
                    postReservationRequestV2.Pricing.SpecialOneWayFee > 0 ? postReservationRequestV2.Pricing.SpecialOneWayFee : reservationToken.OneWayFee);

            var serviceCharge =
                _userRole != UserRoles.External ? reservationToken.CurrencyType != vendor.ServiceChargeCurrencyType
                    ? CalculationHelper.CurrencyExchange(mappedExchangeRates, vendor, vendor.ServiceCharge, vendor.ServiceChargeCurrencyType, postReservationRequestV2.CurrencyCode.ToEnum<CurrencyTypes>())
                    : vendor.ServiceCharge : 0;

            float discountAmount = 0;
            float? discountValue = null;
            int? couponId = null;

            if (!string.IsNullOrEmpty(postReservationRequestV2.CouponCode))
            {
                var checkCouponIsUsable = await _couponService.CheckCouponIsUsable(postReservationRequestV2.CouponCode, postReservationRequestV2.MemberId ?? 0, totalAmount, postReservationRequestV2.CurrencyCode.ToEnum<CurrencyTypes>(), postReservationRequestV2.PickupDate.ToDateTimeNullSafe(), postReservationRequestV2.ReturnDate.ToDateTimeNullSafe(), reservationToken.DailyPrice.ToDecimalNullSafe(), reservationToken.RentalDuration, highAmountDiscountActive: true);

                if (checkCouponIsUsable)
                {
                    UsingCouponCode usingCouponCode = null;

                    if (!string.IsNullOrEmpty(postReservationRequestV2.CouponCode))
                    {
                        // TODO: paidAmount Null olmaktan çıkarıldı yerine 0 değeri verildi (gkursad)
                        usingCouponCode = await _couponService.GetUsingCouponCode(
                            postReservationRequestV2.MemberId ?? 0,
                            postReservationRequestV2.CouponCode,
                            totalAmount: reservationToken.DailyPrice * reservationToken.RentalDuration,
                            vendor,
                            postReservationRequestV2.CurrencyCode.ToEnum<CurrencyTypes>(),
                            postReservationRequestV2.PickupDate.ToDateTimeNullSafe(),
                            postReservationRequestV2.ReturnDate.ToDateTimeNullSafe(),
                            reservationToken.DailyPrice.ToDecimalNullSafe(),
                            reservationToken.RentalDuration,
                            paidAmount:
                            Math.Abs(postReservationRequestV2.Pricing.PaidAmount -
                                     postReservationRequestV2.Pricing.PaidAmountAfterUsingCouponCode) > 0 &&
                            postReservationRequestV2.Pricing.PaidAmount == 0
                                ? postReservationRequestV2.Pricing.PaidAmountAfterUsingCouponCode
                                : (float?)0,
                            highAmountDiscountActive: postReservationRequestV2.HighAmountDiscountActive);
                        //  highAmountDiscountActive: true);
                    }
                    Serilog.Log.Error("{@UsingCouponCode}", usingCouponCode);

                    if (usingCouponCode != null && usingCouponCode.Success)
                    {
                        discountAmount = usingCouponCode.DiscountAmount.ToFloatNullSafe();
                        discountValue = usingCouponCode.DiscountValue;
                        couponId = usingCouponCode.CouponId;
                        bool useCouponCodeResult = await _couponService.UseCouponCode(usingCouponCode.CouponId);
                        Serilog.Log.Error("{UseCouponCodeResult}", useCouponCodeResult);
                    }
                    totalAmount -= discountAmount;
                }
                else
                    return new ServiceResponseBase(null, false, await _configurationService.GetLabel(113, postReservationRequestV2.LanguageCode.ToEnum<LanguageTypes>()));
            }

            float apiDailyPrice = BrokerReservationHelper.GetAPIDailyPrice(reservationToken.APIDailyPrice, vendor);

            float apiTotalAmount = BrokerReservationHelper.GetAPITotalAmount(reservationToken, vendor, apiExtraAmount);
            //apiExtraAmount = BrokerReservationHelper.GetAPIAdditionalPrice(apiExtraAmount, vendor); //TODO: bu satır bazı durumlarda toplam ek ürün tutarını bozuyor. Kontrol edilecek !!!!!!!!!!!!!
            if (vendor.AdditionalProductWorkingType == VendorWorkingTypes.Commission && vendor.ProfitMarkupAdditionalProducts > 0)
                apiExtraAmount = apiExtraAmount * (100f - vendor.ProfitMarkupAdditionalProducts) / 100f;

            var locationVendor = await _context.Locationvendor.Where(x =>
            x.Active == true &&
            x.Vendorid == vendor.VendorId &&
            x.Locallocationid == postReservationRequestV2.PickupLocationId).FirstOrDefaultAsync();

            bool isOffice = ((vendor.VendorType == VendorTypes.Yolcu360) || vendor.VendorType == VendorTypes.Yolcu360v2) ? reservationToken.IsOffice : vendor.VendorType != VendorTypes.KolayCARBroker || (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations) ? locationVendor.Isoffice ?? false : reservationToken.IsOffice; //KolayCARBroker dışındaki VehicleProvider'larda ReservationToken'a isOffice bilgisi tutulmuyor. Onlar kendi local isOffice bilgisini yazıyor. ++ (ekleme) Yolcu360 için reztokendan çekildi

            await _bultenService.CheckContactPermission(postReservationRequestV2);

            var sqlParameters = SqlParameterHelper.PostReservationLocalSqlParameters(reservationId, postReservationRequestV2, reservationToken, vendor, dailyPrice, totalAmount, serviceCharge, extraListString, apiDailyPrice, apiExtraAmount, apiTotalAmount, vendorLogoUrl, isOffice, new PostPaymentResponse(), discountAmount, agency, premiumPacketsPrice, configurations, discountValue, couponId);

            Serilog.Log.Error("{@PostReservationLocalSqlParameters}", sqlParameters.Select(e => new { e.ParameterName, Type = e.SqlDbType, Value = e.Value }));

            try
            {
                var postReservationLocalResult = await _context.Database.ExecuteSqlRawAsync("EXECUTE AddReservation " + SqlParameterHelper.SqlParamList(sqlParameters), sqlParameters);

                Serilog.Log.Error("{@PostReservationLocalProcedureResult}", postReservationLocalResult);

                return new(postReservationRequestV2, true, postReservationLocalResult > 0 ? "Reservation received successfully!" : "An error occurred during the request!");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@PostReservationLocalProcedureErrorResult}", ex.Message);
                return new(postReservationRequestV2, false, $"An error occurred during the request! - {ex.Message}");
            }
        }

        private string GetExtraListString(List<Extra> extras, Domain.Models.Agency agency, List<Extra> localExtras, bool extraMappingActive)
        {
            return string.Join("|", extras.Select(e =>
            {
                var localExtra = extraMappingActive
                    ? localExtras.FirstOrDefault(l => l.ExtraId == e.ExtraId)
                    : null;

                var extraName = localExtra?.ExtraName ?? e.ExtraName;
                var extraDescription = string.IsNullOrEmpty(e.ExtraDescription)
                    ? localExtra?.ExtraDescription ?? string.Empty
                    : e.ExtraDescription;

                return $"{e.ExtraId}~{e.Piece}~{e.Price}~{extraName}~{e.ExtraCode}~{(int)e.ExtraRentalType}~{e.ApiPrice}~{(e.ExtraType == AdditionalProductTypes.Premium ? (e.Price * (100 - agency.AgencyCommissionAmount) / 100f) : e.Price)}~{(int)e.ExtraType}~{extraDescription}";
            }));
        }

        public async Task<ServiceResponseBase> PostReservationToVendorAPI(Domain.Models.Requests.PostReservationRequestV2 postReservationRequest, ReservationToken reservationToken, Domain.Models.Agency agency, Domain.Models.Vendor vendor, long reservationId, List<Extra> apiExtras)
        {
            if (reservationToken == null)
                return new ServiceResponseBase(null, false, "reservationToken hatalı!");

            Serilog.Log.Error("{@ReservationAgency}", JsonConvert.SerializeObject(agency) + $"-{reservationToken.AgencyId}");
            if (agency == null)
                return new ServiceResponseBase(null, false, "Acente bilgisine ulaşılamadı!");

            if (vendor == null)
                return new ServiceResponseBase(null, false, "Tedarikçi bilgisine ulaşılamadı!");

            ReservationHelper.FillPostReservationRequest(postReservationRequest, reservationToken, agency);
            await _agencyService.SetAgencyPaymentOptions(agency, vendor);
            if (await _reservationStepsService.GetAdditionalInformation(
                vendor,
                agency,
                postReservationRequest.LanguageCode,
                postReservationRequest.CurrencyCode,
                vendor.CurrencyType,
                postReservationRequest.PickupLocationId,
                postReservationRequest.ReturnLocationId,
                postReservationRequest.PickupDate,
                postReservationRequest.ReturnDate,
                postReservationRequest.PickupTime,
                postReservationRequest.ReturnTime,
                vehicleId: reservationToken.VehicleId,
                apiVendorId: reservationToken.APIVendorId,
                rentalDuration: reservationToken.RentalDuration,
                apiReferenceCode: reservationToken.APIReferenceCode,
                reservationToken: reservationToken,
                apiLocationCode: reservationToken.APIPickupLocationCode) is ResponseReservationStepsAdditionalInformation additionalInformation && additionalInformation != null)
            {
                Serilog.Log.Error("{@ReservationAdditionalInformation}", JsonConvert.SerializeObject(additionalInformation));

                var exchangeRates = (await _exchangeRateService.GetAllExchangeRates()).Map();
                var configurations = await _configurationService.GetConfigurations();

                Serilog.Log.Error("{@ReservationExchangeRates}", JsonConvert.SerializeObject(exchangeRates));

                var reservationNumber = ReservationHelper.GenerateReservationNumber(reservationId);

                var localReservation = await GetReservation(new GetReservationsRequest
                {
                    ReservationNumber = reservationNumber,
                    CustomerEmail = postReservationRequest.Customer.Email
                });

                //Local rezervasyon yazıldıktan sonra komisyon çıkarılmış ekstra tutarları tedarikçi servisinden gelen ilk fiyatları ilke değiştirilir. Bu işlem tedarikçi fiyatının hesaplanması içindir local rezervasyonda güncelleme olmaz.
                if (vendor.AdditionalProductWorkingType == VendorWorkingTypes.Commission)
                {
                    foreach (var apiExtra in apiExtras)
                    {
                        var reservationExtra = localReservation.ReservationExtras.Where(x => x.ExtraCode == apiExtra.ExtraCode).FirstOrDefault();
                        if (reservationExtra != null)
                            apiExtra.Price = reservationExtra.Price;
                    }
                }

                localReservation.APIPaidAmount = await CalculateAPIPaidAmount(agency, vendor, reservationToken, localReservation, postReservationRequest, apiExtras);

                if (vendor.SendDefaultMailAddress && !string.IsNullOrEmpty(configurations.DefaultCustomerMailAddress) && (vendor.VendorType != VendorTypes.KolayCARBroker || (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations)))
                    postReservationRequest.Customer.Email = configurations.DefaultCustomerMailAddress;

                #region Özel ek ürünlerin tedarikçi apilerine gitmeme işlemi
                bool haveSpecialExtras = false;
                var specialExtrasList = new List<ReservationExtra>();
                //var ExtrasString = new List<string>();
                //var extraList = "";
                //if (configurations.SpecialExtrasIsActive && localReservation.ReservationExtras.Count > 0 && postReservationRequest.ExtraList.Contains("BrokerSpecial"))
                //{
                //    extraList = postReservationRequest.ExtraList;
                //    haveSpecialExtras = true;

                //    specialExtrasList = localReservation.ReservationExtras.Where(x => x.ExtraType == AdditionalProductTypes.Compulsory || x.ExtraCode.Contains("BrokerSpecial")).ToList();
                //    localReservation.ReservationExtras.RemoveAll(x => x.ExtraType == AdditionalProductTypes.Compulsory || x.ExtraCode.Contains("BrokerSpecial"));

                //    ExtrasString = postReservationRequest.ExtraList.Split('|').ToList();
                //    ExtrasString.RemoveAll(x => x.Contains("BrokerSpecial"));
                //    var specialExtrasString = postReservationRequest.ExtraList.Split('|').Where(x => x.Contains("BrokerSpecial")).ToList();
                //    postReservationRequest.ExtraList = "";

                //    for (int i = 0; i < ExtrasString.Count; i++)
                //        postReservationRequest.ExtraList += i == ExtrasString.Count - 1 ? ExtrasString[i].ToString() : ExtrasString[i].ToString() + "|";
                //}

                if (localReservation.ReservationExtras.Where(e => e.ExtraType == AdditionalProductTypes.Premium).Any())
                {
                    localReservation.ReservationExtras.ForEach(e =>
                    {
                        e.VendorExtraExists = apiExtras.FirstOrDefault(ex => ex.ExtraId == e.ExtraId)?.VendorExtraExists ?? false;
                    });
                    specialExtrasList = localReservation.ReservationExtras.Where(e => e.ExtraType == AdditionalProductTypes.Premium && !e.VendorExtraExists).ToList();
                    localReservation.ReservationExtras.RemoveAll(e => e.ExtraType == AdditionalProductTypes.Premium && !e.VendorExtraExists);
                    haveSpecialExtras = true;
                }

                #endregion
                var reservationProvider = _reservationProviderFactory.CreateReservationProvider(vendor, _configurationService, _configuration, _memoryCache, _cacheService);

                if (reservationProvider is null)
                    return new ServiceResponseBase(null, false, "Tedarikçi tipi bulunamadı!");

                var result = await reservationProvider.PostReservation(postReservationRequest.Map(), vendor, additionalInformation, reservationNumber, reservationToken, exchangeRates, localReservation, apiExtras);

                Serilog.Log.Error("{@PostReservationVendorResponse}", JsonConvert.SerializeObject(result));

                var retryPostReservation = Boolean.Parse(_parameterService.GetParameterValue("RetryPostReservationToVendor") ?? "false");

                var reservation = result.Data as Reservation;

                if (retryPostReservation && vendor.VendorType != VendorTypes.Vonarent)
                {
                    if (string.IsNullOrEmpty(reservation.APIReservationNumber))
                    {
                        result = await reservationProvider.PostReservation(postReservationRequest.Map(), vendor, additionalInformation, reservationNumber, reservationToken, exchangeRates, localReservation, apiExtras);

                        Serilog.Log.Error("{@PostReservationVendorResponseRetry}", JsonConvert.SerializeObject(result));
                    }
                }

                if (haveSpecialExtras)//tedarikçi servisine gitmemesi için zorunlu zorunlu ek ürün varsa tekrar ekleme yapar.
                {
                    localReservation.ReservationExtras.AddRange(specialExtrasList);
                }

                var officeLabel = await _configurationService.GetLabel(2075, postReservationRequest.LanguageCode.ToEnum<LanguageTypes>());

                if (string.IsNullOrEmpty(reservation.PickupOfficeWorkingHours))
                {
                    var pickupOffice = await _vendorOfficeService.GetVendorOffice(reservation.VendorId, reservation.PickupLocationId);
                    reservation.PickupOfficeWorkingHours = pickupOffice != null
                        ? $"{pickupOffice.OpeningTime.ToDateTimeNullSafe().ToString("HH:mm")} - {pickupOffice.ClosingTime.ToDateTimeNullSafe().ToString("HH:mm")}"
                        : officeLabel;
                }

                if (string.IsNullOrEmpty(reservation.ReturnOfficeWorkingHours))
                {
                    var returnOffice = await _vendorOfficeService.GetVendorOffice(reservation.VendorId, reservation.ReturnLocationId);
                    reservation.ReturnOfficeWorkingHours = returnOffice != null
                        ? $"{returnOffice.OpeningTime.ToDateTimeNullSafe().ToString("HH:mm")} - {returnOffice.ClosingTime.ToDateTimeNullSafe().ToString("HH:mm")}"
                        : officeLabel;
                }

                if (string.IsNullOrEmpty(reservation.PickupOfficeWorkingHours) || string.IsNullOrEmpty(reservation.ReturnOfficeWorkingHours))
                    Serilog.Log.Error("{@OfficeWorkingHours}", $"{officeLabel} - {reservation.PickupOfficeWorkingHours} - {reservation.ReturnOfficeWorkingHours}");

                result.Data = reservation;
                return result;
            }
            return new ServiceResponseBase(null, false, "Tedarikçi bilgisine ulaşılamadı!");
        }

        public async Task UpdateCancelAttempt(string rezNo)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(@"UPDATE REZ SET APICANCELREQUESTATTEMPTED = 1 where REZNO = @REZNO", new SqlParameter("@REZNO", rezNo));
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@UpdateCancelAttempt}", ex.ToJson());
            }
        }

        public async Task<bool> LocalCancel(CancelLocalRequest request)
        {
            try
            {
                var affectedRows = await _context.Database.ExecuteSqlRawAsync("EXEC CANCELRESERVATIONWITHOUTVENDORSERVICE @RESERVATIONNUMBER = {0}, @CUSTOMEREMAIL = {1}, @CANCELNOTE = {2}, @KPANELUSERID = {3}",
                request.ReservationNumber,
                request.CustomerEmail,
                request.CancelNote,
                request.UserId);

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@LocalCancel}", ex.ToJson());

                return false;
            }
        }

        public async Task<bool> UpdateRecalculatedReservation(UpdateRecalculatedReservationRequest request)
        {
            try
            {
                var reservation = await _context.Rez.FirstOrDefaultAsync(r => r.Rezno == request.ReservationCode);

                if (reservation == null)
                    return false;

                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE REZ SET LASTUPDATEBYUSER = {18} WHERE REZID = {reservation.Rezid}");

                reservation.Birakistarihi = request.NewReturnDate;
                reservation.Kiralamasuresi = request.RentalDayCount;
                reservation.Toplamtutar = request.AgencyPaidAmount + request.CouponDiscountAmount;
                reservation.Apipaidamount = request.VendorPaidAmount;
                reservation.Odenentutar = request.PaidAmount;
                reservation.InstallmentCommissionAmount = request.InstallmentDelta;
                reservation.Coupondiscountamount = request.CouponDiscountAmount;
                reservation.Updatedate = DateTime.Now;
                reservation.Apidailyprice = request.VendorPaidAmount / request.RentalDayCount;
                reservation.Gunlukfiyat = (request.AgencyPaidAmount + request.CouponDiscountAmount) / request.RentalDayCount;

                var entry = _context.Entry(reservation);
                entry.State = EntityState.Unchanged;
                entry.Property(nameof(reservation.Birakistarihi)).IsModified = true;
                entry.Property(nameof(reservation.Kiralamasuresi)).IsModified = true;
                entry.Property(nameof(reservation.Toplamtutar)).IsModified = true;
                entry.Property(nameof(reservation.Apipaidamount)).IsModified = true;
                entry.Property(nameof(reservation.Odenentutar)).IsModified = true;
                entry.Property(nameof(reservation.InstallmentCommissionAmount)).IsModified = true;
                entry.Property(nameof(reservation.Coupondiscountamount)).IsModified = true;
                entry.Property(nameof(reservation.Updatedate)).IsModified = true;
                entry.Property(nameof(reservation.Apidailyprice)).IsModified = true;
                entry.Property(nameof(reservation.Gunlukfiyat)).IsModified = true;

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@UpdateRecalculatedReservation}", ex.ToJson());

                return false;
            }
        }
    }
}


