using KolayCAR.Broker.API.Data.Maps.Financial;
using KolayCAR.Broker.API.Data.Maps.Location;
using KolayCAR.Broker.API.Data.Maps.Logs;
using KolayCAR.Broker.API.Data.Maps.MobileAppModels;
using KolayCAR.Broker.API.Data.Maps.Payment;
using KolayCAR.Broker.API.Data.Maps.Reservation;
using KolayCAR.Broker.API.Data.Maps.Users;
using KolayCAR.Broker.API.Data.Maps.Vendor;
using KolayCAR.Broker.API.Models.Dtos;
using KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels;
using KolayCAR.Broker.API.Models.MobileAppModels;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Logo;
using KolayCAR.Broker.Domain.Models.StoredPorcedureModels;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace KolayCAR.Broker.API.Models
{
    public partial class BrokerContext : DbContext
    {
        //public BrokerContext()
        //{
        //}

        public BrokerContext(DbContextOptions<BrokerContext> options)
            : base(options)
        {
            this.Database.SetCommandTimeout(75);
        }

        public virtual DbSet<Additionalproduct> Additionalproduct { get; set; }
        public virtual DbSet<Additionalproductprice> Additionalproductprice { get; set; }
        public virtual DbSet<Additionalproducttype> Additionalproducttype { get; set; }
        public virtual DbSet<Additionalproductvendor> Additionalproductvendor { get; set; }
        public virtual DbSet<Additionalproductvendordays> Additionalproductvendordays { get; set; }
        public virtual DbSet<Additions> Additions { get; set; }
        public virtual DbSet<Agency> Agency { get; set; }
        public virtual DbSet<Agencylocation> Agencylocation { get; set; }
        public virtual DbSet<Agencyvendor> Agencyvendor { get; set; }
        public virtual DbSet<Agencyvendorpaymentoption> Agencyvendorpaymentoption { get; set; }
        public virtual DbSet<Agencyvendorprofitmarkup> Agencyvendorprofitmarkup { get; set; }
        public virtual DbSet<Apilog> Apilog { get; set; }
        public virtual DbSet<ApilogDev> ApilogDev { get; set; }
        public virtual DbSet<Ayar> Ayar { get; set; }
        public virtual DbSet<Basvuru> Basvuru { get; set; }
        public virtual DbSet<Bank> Banks { get; set; }
        public virtual DbSet<BankBinCode> BankBinCodes { get; set; }
        public virtual DbSet<Brokerlog> Brokerlog { get; set; }
        public virtual DbSet<Bulten> Bulten { get; set; }
        public virtual DbSet<BlockedMember> BlockedMembers { get; set; }
        public virtual DbSet<BlockedData> BlockedDatas { get; set; }
        public virtual DbSet<City> City { get; set; }
        public virtual DbSet<Commercialallowance> Commercialallowance { get; set; }
        public virtual DbSet<Companyinvoice> Companyinvoice { get; set; }
        public virtual DbSet<Country> Country { get; set; }
        public virtual DbSet<Countrylang> Countrylang { get; set; }
        public virtual DbSet<Coupon> Coupon { get; set; }
        public virtual DbSet<CouponVendor> CouponVendor { get; set; }
        public virtual DbSet<Currency> Currency { get; set; }
        public virtual DbSet<Currentaccount> Currentaccount { get; set; }
        public virtual DbSet<Currentaccounthistory> Currentaccounthistory { get; set; }
        public virtual DbSet<Currentaccountvendor> Currentaccountvendor { get; set; }
        public virtual DbSet<Currentaccountvendorhistory> Currentaccountvendorhistory { get; set; }
        public virtual DbSet<Dil> Dil { get; set; }
        public virtual DbSet<Exchangecopy> Exchangecopy { get; set; }
        public virtual DbSet<Exchangeratecenter> Exchangeratecenter { get; set; }
        public virtual DbSet<Exchangerates> Exchangerates { get; set; }
        public virtual DbSet<Exchangerateshistory> Exchangerateshistory { get; set; }
        public virtual DbSet<FindeksLog> FindeksLogs { get; set; }
        public virtual DbSet<Hata> Hata { get; set; }
        public virtual DbSet<Icerik> Icerik { get; set; }
        public virtual DbSet<Icerikdil> Icerikdil { get; set; }
        public virtual DbSet<Icerikgrup> Icerikgrup { get; set; }
        public virtual DbSet<Iceriktip> Iceriktip { get; set; }
        public virtual DbSet<Invoice> Invoice { get; set; }
        public virtual DbSet<Installment> Installments { get; set; }
        public virtual DbSet<Kullanici> Kullanici { get; set; }
        public virtual DbSet<Label> Label { get; set; }
        public virtual DbSet<Location> Location { get; set; }
        public virtual DbSet<Locationvendor> Locationvendor { get; set; }
        public virtual DbSet<Locationvendorcloseddate> Locationvendorcloseddate { get; set; }
        public virtual DbSet<LocationVendorInstallment> LocationVendorInstallments { get; set; }
        public virtual DbSet<Log> Log { get; set; }
        public virtual DbSet<Parametre> Parametre { get; set; }
        public virtual DbSet<PaymentSetting> PaymentSettings { get; set; }
        public virtual DbSet<Payment3dSecure> Payment3DSecure { get; set; }
        public virtual DbSet<PaymentResult> PaymentResults { get; set; }
        public virtual DbSet<Popularvehicle> Popularvehicle { get; set; }
        public virtual DbSet<Portalacente> Portalacente { get; set; }
        public virtual DbSet<Portalacentekullanici> Portalacentekullanici { get; set; }
        public virtual DbSet<ProfitMarkup> ProfitMarkups { get; set; }
        public virtual DbSet<ProfitMarkupAgency> ProfitMarkupAgencies { get; set; }
        public virtual DbSet<ProfitMarkupLocation> ProfitMarkupLocations { get; set; }
        public virtual DbSet<ProfitMarkupVendor> ProfitMarkupVendors { get; set; }
        public virtual DbSet<Reservationcancellationfee> Reservationcancellationfee { get; set; }
        public virtual DbSet<Reservationextra> Reservationextra { get; set; }
        public virtual DbSet<ReservationDetail> ReservationDetails { get; set; }
        public virtual DbSet<ReservationDriverInfo> ReservationDriverInfos { get; set; }
        public virtual DbSet<ReservationInvoiceAddress> ReservationInvoiceAddresses { get; set; }
        public virtual DbSet<ReservationPaymentDetail> ReservationPaymentDetails { get; set; }
        public virtual DbSet<ReservationSelectedExtra> ReservationSelectedExtras { get; set; }
        public virtual DbSet<ReservationVehicleInfo> ReservationVehicleInfos { get; set; }
        public virtual DbSet<Reservationupdate> Reservationupdate { get; set; }
        public virtual DbSet<Reservationupdateproperty> Reservationupdateproperty { get; set; }
        public virtual DbSet<Ressource> Ressource { get; set; }
        public virtual DbSet<Resstatushistory> Resstatushistory { get; set; }
        public virtual DbSet<Resstatuslang> Resstatuslang { get; set; }
        public virtual DbSet<Restoken> Restoken { get; set; }
        public virtual DbSet<Reswslog> Reswslog { get; set; }
        public virtual DbSet<Rez> Rez { get; set; }
        public virtual DbSet<Scoreusagehistory> Scoreusagehistory { get; set; }
        public virtual DbSet<Smartvehiclesequence> Smartvehiclesequence { get; set; }
        public virtual DbSet<SpecialProductPrice> SpecialProductPrices { get; set; }
        public virtual DbSet<Staticlokasyon> Staticlokasyon { get; set; }
        public virtual DbSet<Subvendor> Subvendor { get; set; }
        public virtual DbSet<Survey> Survey { get; set; }
        public virtual DbSet<Surveyanswer> Surveyanswer { get; set; }
        public virtual DbSet<Surveyquestion> Surveyquestion { get; set; }
        public virtual DbSet<Surveystatus> Surveystatus { get; set; }
        public virtual DbSet<Surveystatustype> Surveystatustype { get; set; }
        public virtual DbSet<Uye> Uye { get; set; }
        public virtual DbSet<Vehiclebaggage> Vehiclebaggage { get; set; }
        public virtual DbSet<Vehiclebaggagelang> Vehiclebaggagelang { get; set; }
        public virtual DbSet<Vehiclebrand> Vehiclebrand { get; set; }
        public virtual DbSet<Vehiclecategory> Vehiclecategory { get; set; }
        public virtual DbSet<Vehiclecategorylang> Vehiclecategorylang { get; set; }
        public virtual DbSet<Vehicleclass> Vehicleclass { get; set; }
        public virtual DbSet<Vehicleclasslang> Vehicleclasslang { get; set; }
        public virtual DbSet<Vehicleclassvendor> Vehicleclassvendor { get; set; }
        public virtual DbSet<Vehiclefuel> Vehiclefuel { get; set; }
        public virtual DbSet<VehicleFilterMobile> VehicleFilter { get; set; }
        public virtual DbSet<MobileAppVehicleBadge> MobileAppVehicleBadge { get; set; }
        public virtual DbSet<Vehiclefuellang> Vehiclefuellang { get; set; }
        public virtual DbSet<Vehiclemodel> Vehiclemodel { get; set; }
        public virtual DbSet<Vehicleperson> Vehicleperson { get; set; }
        public virtual DbSet<Vehiclepersonlang> Vehiclepersonlang { get; set; }
        public virtual DbSet<Vehicleresultstatistic> Vehicleresultstatistic { get; set; }
        public virtual DbSet<Vehicleseries> Vehicleseries { get; set; }
        public virtual DbSet<Vehicletransmission> Vehicletransmission { get; set; }
        public virtual DbSet<Vehicletransmissionlang> Vehicletransmissionlang { get; set; }
        public virtual DbSet<Vehicletype> Vehicletype { get; set; }
        public virtual DbSet<Vehicletypelang> Vehicletypelang { get; set; }
        public virtual DbSet<Vendor> Vendor { get; set; }
        public virtual DbSet<Vendorcontactinformation> Vendorcontactinformation { get; set; }
        public virtual DbSet<Vendortype> Vendortype { get; set; }
        public virtual DbSet<VendorVendor> VendorVendors { get; set; }
        public virtual DbSet<Yonlendirme> Yonlendirme { get; set; }
        public virtual DbSet<VendorScore> VendorScores { get; set; }
        public virtual DbSet<VendorLocationFee> VendorLocationFees { get; set; }
        public virtual DbSet<VendorLocationCondition> VendorLocationConditions { get; set; }
        public virtual DbSet<VendorLocationDeliveryType> VendorLocationDeliveryTypes { get; set; }
        public virtual DbSet<VendorOffice> VendorOffices { get; set; }
        public virtual DbSet<EmailLog> EmailLogs { get; set; }
        public virtual DbSet<ExternalComment> ExternalComments { get; set; }
        public virtual DbSet<MobileAppSetting> MobileAppSettings { get; set; }
        public virtual DbSet<DeliveryTypeLanguage> DeliveryTypeLanguages { get; set; }
        public virtual DbSet<CountryTaxRate> CountryTaxRates { get; set; }
        public virtual DbSet<SpecialRequest> SpecialRequests { get; set; }
        public virtual DbSet<SpecialRequestAgency> SpecialRequestAgencies { get; set; }
        public virtual DbSet<SpecialRequestVendor> SpecialRequestVendors { get; set; }
        public virtual DbSet<SpecialRequestLocation> SpecialRequestLocations { get; set; }
        public virtual DbSet<SpecialRequestTariff> SpecialRequestTariffs { get; set; }

        #region Mobile App Models
        public DbSet<MobileSortingParameter> MobileSortingParameters { get; set; }
        public DbSet<MobileVehicleBadge> MobileVehicleBadges { get; set; }
        public DbSet<MobileVehicleDetail> MobileVehicleDetails { get; set; }
        public DbSet<MobileVehicleFeature> MobileVehicleFeatures { get; set; }
        public DbSet<MobileVehicleListFastFilter> MobileVehicleListFastFilters { get; set; }
        public DbSet<MobileVehicleListPopularFilter> MobileVehicleListPopularFilters { get; set; }
        public DbSet<MobileVehicleListFilter> MobileVehicleListFilters { get; set; }
        public DbSet<MobileVehiclePromotion> MobileVehiclePromotions { get; set; }
        public DbSet<MobileVehicleSortingOption> MobileVehicleSortingOptions { get; set; }
        public DbSet<MobileFindeksUrl> MobileFindeksUrl { get; set; }
        public DbSet<ReservationFindeksDetail> ReservationFindeksDetail { get; set; }
        public DbSet<BultenLog> BultenLogs { get; set; }
        #endregion

        [NotMapped]
        public DbSet<SearchLocationDto> SearchLocationDtos { get; set; }
        [NotMapped]
        public DbSet<CouponDetailDto> CouponDetailDtos { get; set; }
        [NotMapped]
        public DbSet<BrokerLocationVehicleDetailDto> LocationVehicleDetailDtos { get; set; }
        [NotMapped]
        public DbSet<PopupContentDto> PopupContentDtos { get; set; }
        [NotMapped]
        public DbSet<AgencyVendorDto> AgencyVendorDtos { get; set; }

        #region Stored Procedures Models
        [NotMapped]
        public DbSet<VendorScoreDto> VendorScoresDto { get; set; }

        [NotMapped]
        public DbSet<Domain.Models.PaymentDto> Payments { get; set; }

        [NotMapped]
        public DbSet<DataLayer> DataLayers { get; set; }

        [NotMapped]
        public DbSet<ReservationReportModel> ReservationReportModel { get; set; }

        [NotMapped]
        public DbSet<LogoReservation> LogoReservations { get; set; }

        [NotMapped]
        public DbSet<CommentDto> CommentDtos { get; set; }
        #endregion

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Data Source=broker.kolaycar.com,2727;Initial Catalog=miniyoldb_broker;User ID=miniyoluser;Password=Qwqt593&");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region OnModelCreating
            modelBuilder.Entity<VendorScore>(entity =>
            {
                entity.ToTable("VENDORSCORE");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.VendorId).HasColumnName("VENDORID");
                entity.Property(e => e.VendorName).HasColumnName("VENDORNAME");
                entity.Property(e => e.ApiVendorId).HasColumnName("APIVENDORID");
                entity.Property(e => e.ApiVendorName).HasColumnName("APIVENDORNAME");
                entity.Property(e => e.LocationId).HasColumnName("LOCATIONID");
                entity.Property(e => e.LocationName).HasColumnName("LOCATIONNAME");
                entity.Property(e => e.Score).HasColumnName("SCORE").HasColumnType("decimal(2, 1)");
                entity.Property(e => e.CurrentScore).HasColumnName("CURRENTSCORE").HasColumnType("decimal(2, 1)");
            });

            modelBuilder.Entity<Additionalproduct>(entity =>
                {
                    entity.HasKey(e => new { e.Productid, e.Langid, e.Producttype })
                        .HasName("PK_EXTRAS");

                    entity.ToTable("ADDITIONALPRODUCT");

                    entity.Property(e => e.Productid).HasColumnName("PRODUCTID");

                    entity.Property(e => e.Langid).HasColumnName("LANGID");

                    entity.Property(e => e.Producttype).HasColumnName("PRODUCTTYPE");

                    entity.Property(e => e.Active).HasColumnName("ACTIVE");

                    entity.Property(e => e.Defaultprice)
                        .HasColumnName("DEFAULTPRICE")
                        .HasColumnType("decimal(18, 2)")
                        .HasDefaultValueSql("((0))");

                    entity.Property(e => e.Iconpath).HasColumnName("ICONPATH");

                    entity.Property(e => e.Productcode)
                        .HasColumnName("PRODUCTCODE")
                        .HasMaxLength(50);

                    entity.Property(e => e.Productdescription).HasColumnName("PRODUCTDESCRIPTION");

                    entity.Property(e => e.Productname)
                        .IsRequired()
                        .HasColumnName("PRODUCTNAME");

                    entity.Property(e => e.Quantityincreasable)
                        .HasColumnName("QUANTITYINCREASABLE")
                        .HasDefaultValueSql("((0))");

                    entity.Property(e => e.Rentaltype)
                        .HasColumnName("RENTALTYPE")
                        .HasDefaultValueSql("((0))");

                    entity.Property(e => e.Sequence)
                        .HasColumnName("SEQUENCE")
                        .HasDefaultValueSql("((1))");

                    entity.Property(e => e.Showdaycountend).HasColumnName("SHOWDAYCOUNTEND");

                    entity.Property(e => e.Showdaycountstart).HasColumnName("SHOWDAYCOUNTSTART");

                    entity.Property(e => e.Showinvehiclelist)
                        .HasColumnName("SHOWINVEHICLELIST")
                        .HasDefaultValueSql("((0))");

                    entity.Property(e => e.VendorExtraExists)
                      .HasColumnName("VENDOREXTRAEXISTS");

                    entity.Property(e => e.ShowOnlyFullCreditVehicles)
                      .HasColumnName("SHOWONLYFULLCREDITVEHICLES");
                });

            modelBuilder.Entity<Additionalproductprice>(entity =>
            {
                entity.HasKey(e => new { e.Additionalproductvendorid, e.Productid, e.Vendorid, e.Startday, e.Endday });

                entity.ToTable("ADDITIONALPRODUCTPRICE");

                entity.Property(e => e.Additionalproductvendorid).HasColumnName("ADDITIONALPRODUCTVENDORID");

                entity.Property(e => e.Productid).HasColumnName("PRODUCTID");

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");

                entity.Property(e => e.Startday).HasColumnName("STARTDAY");

                entity.Property(e => e.Endday).HasColumnName("ENDDAY");

                entity.Property(e => e.Currencyid).HasColumnName("CURRENCYID");

                entity.Property(e => e.Price)
                    .HasColumnName("PRICE")
                    .HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<Additionalproducttype>(entity =>
            {
                entity.HasKey(e => new { e.Producttype, e.Langid });

                entity.ToTable("ADDITIONALPRODUCTTYPE");

                entity.Property(e => e.Producttype).HasColumnName("PRODUCTTYPE");

                entity.Property(e => e.Langid).HasColumnName("LANGID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("NAME");
            });

            modelBuilder.Entity<Additionalproductvendor>(entity =>
            {
                entity.ToTable("ADDITIONALPRODUCTVENDOR");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active)
                    .HasColumnName("ACTIVE")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Apiproductcode).HasColumnName("APIPRODUCTCODE");

                entity.Property(e => e.Apiproductid).HasColumnName("APIPRODUCTID");

                entity.Property(e => e.Apiproductname).HasColumnName("APIPRODUCTNAME");

                entity.Property(e => e.Apiproductprice)
                    .HasColumnName("APIPRODUCTPRICE")
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Currencyid).HasColumnName("CURRENCYID");

                entity.Property(e => e.Defaultprice)
                    .HasColumnName("DEFAULTPRICE")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Productid).HasColumnName("PRODUCTID");

                entity.Property(e => e.Quantityincreasable)
                    .HasColumnName("QUANTITYINCREASABLE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Recorddate)
                    .HasColumnName("RECORDDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Rentaltype)
                    .HasColumnName("RENTALTYPE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");
            });

            modelBuilder.Entity<Additionalproductvendordays>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.Vendorid });

                entity.ToTable("ADDITIONALPRODUCTVENDORDAYS");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");

                entity.Property(e => e.Endday).HasColumnName("ENDDAY");

                entity.Property(e => e.Startday).HasColumnName("STARTDAY");
            });

            modelBuilder.Entity<Additions>(entity =>
            {
                entity.ToTable("ADDITIONS");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Additionname)
                    .IsRequired()
                    .HasColumnName("ADDITIONNAME");

                entity.Property(e => e.Additionstatustype).HasColumnName("ADDITIONSTATUSTYPE");

                entity.Property(e => e.Additiontypeid).HasColumnName("ADDITIONTYPEID");

                entity.Property(e => e.Agencyamount)
                    .HasColumnName("AGENCYAMOUNT")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Agencycurrencyid).HasColumnName("AGENCYCURRENCYID");

                entity.Property(e => e.Createdate)
                    .HasColumnName("CREATEDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasColumnName("DESCRIPTION");

                entity.Property(e => e.Laststatuschangedate)
                    .HasColumnName("LASTSTATUSCHANGEDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Laststatuschangeuserid).HasColumnName("LASTSTATUSCHANGEUSERID");

                entity.Property(e => e.Reservationnumber)
                    .IsRequired()
                    .HasColumnName("RESERVATIONNUMBER");

                entity.Property(e => e.Userid).HasColumnName("USERID");

                entity.Property(e => e.Vendoramount)
                    .HasColumnName("VENDORAMOUNT")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Vendorcurrencyid).HasColumnName("VENDORCURRENCYID");
            });

            modelBuilder.Entity<Agency>(entity =>
            {
                entity.ToTable("AGENCY");

                entity.Property(e => e.Agencyid)
                    .HasColumnName("AGENCYID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Active)
                    .HasColumnName("ACTIVE")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Additionalproductamountdeliverypayment)
                    .HasColumnName("ADDITIONALPRODUCTAMOUNTDELIVERYPAYMENT")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Address)
                    .HasColumnName("ADDRESS")
                    .HasMaxLength(250);

                entity.Property(e => e.Advancepaymentactive).HasColumnName("ADVANCEPAYMENTACTIVE");

                entity.Property(e => e.Advancepaymentamountbyagencycommissionactive).HasColumnName("ADVANCEPAYMENTAMOUNTBYAGENCYCOMMISSIONACTIVE");

                entity.Property(e => e.Agencyapikey)
                    .HasColumnName("AGENCYAPIKEY")
                    .HasMaxLength(250);

                entity.Property(e => e.Agencyapipassword)
                    .HasColumnName("AGENCYAPIPASSWORD")
                    .HasMaxLength(250);

                entity.Property(e => e.Agencyapplication).HasColumnName("AGENCYAPPLICATION");

                entity.Property(e => e.Agencycode)
                    .HasColumnName("AGENCYCODE")
                    .HasMaxLength(150);

                entity.Property(e => e.Agencycommissiontype)
                    .HasColumnName("AGENCYCOMMISSIONTYPE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Agencyrentalprofitmarkup)
                    .HasColumnName("AGENCYRENTALPROFITMARKUP")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Barrier)
                    .HasColumnName("BARRIER")
                    .HasColumnType("datetime");

                entity.Property(e => e.Branch)
                    .HasColumnName("BRANCH")
                    .HasMaxLength(50);

                entity.Property(e => e.Cancellationpenaltyactive)
                    .HasColumnName("CANCELLATIONPENALTYACTIVE")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Cityid).HasColumnName("CITYID");

                entity.Property(e => e.Cityname)
                    .HasColumnName("CITYNAME")
                    .HasMaxLength(250);

                entity.Property(e => e.Commission)
                    .HasColumnName("COMMISSION")
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Commissionfreepaymentactive).HasColumnName("COMMISSIONFREEPAYMENTACTIVE");

                entity.Property(e => e.Country)
                    .HasColumnName("COUNTRY")
                    .HasMaxLength(100);

                entity.Property(e => e.Countryid).HasColumnName("COUNTRYID");

                entity.Property(e => e.Countryname)
                    .HasColumnName("COUNTRYNAME")
                    .HasMaxLength(250);

                entity.Property(e => e.Creationdate)
                    .HasColumnName("CREATIONDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Creditcarddiscountpercent).HasColumnName("CREDITCARDDISCOUNTPERCENT");

                entity.Property(e => e.Creditcardpaymentactive).HasColumnName("CREDITCARDPAYMENTACTIVE");

                entity.Property(e => e.Currencyid).HasColumnName("CURRENCYID");

                entity.Property(e => e.Email)
                    .HasColumnName("EMAIL")
                    .HasMaxLength(100);

                entity.Property(e => e.Fax)
                    .HasColumnName("FAX")
                    .HasMaxLength(50);

                entity.Property(e => e.Freepriceactive).HasColumnName("FREEPRICEACTIVE");

                entity.Property(e => e.Freepriceshowactive).HasColumnName("FREEPRICESHOWACTIVE");

                entity.Property(e => e.Isrestrictedapi).HasColumnName("ISRESTRICTEDAPI");

                entity.Property(e => e.Lastedit)
                    .HasColumnName("LASTEDIT")
                    .HasColumnType("datetime");

                entity.Property(e => e.Limit).HasColumnName("LIMIT");

                entity.Property(e => e.Loginenable)
                    .HasColumnName("LOGINENABLE")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Logo).HasColumnName("LOGO");

                entity.Property(e => e.Mobile)
                    .HasColumnName("MOBILE")
                    .HasMaxLength(50);

                entity.Property(e => e.Name)
                    .HasColumnName("NAME")
                    .HasMaxLength(150);

                entity.Property(e => e.Notes)
                    .HasColumnName("NOTES")
                    .HasMaxLength(500);

                entity.Property(e => e.Notes2)
                    .HasColumnName("NOTES2")
                    .HasMaxLength(500);

                entity.Property(e => e.Onewayamountdeliverypayment)
                    .HasColumnName("ONEWAYAMOUNTDELIVERYPAYMENT")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Ownername)
                    .HasColumnName("OWNERNAME")
                    .HasMaxLength(150);

                entity.Property(e => e.Ownersurname)
                    .HasColumnName("OWNERSURNAME")
                    .HasMaxLength(150);

                entity.Property(e => e.Payagencyactive).HasColumnName("PAYAGENCYACTIVE");

                entity.Property(e => e.Payallactive).HasColumnName("PAYALLACTIVE");

                entity.Property(e => e.Paydeliveryactive).HasColumnName("PAYDELIVERYACTIVE");

                entity.Property(e => e.Paymentmethod)
                    .HasColumnName("PAYMENTMETHOD")
                    .HasMaxLength(150);

                entity.Property(e => e.Phone)
                    .HasColumnName("PHONE")
                    .HasMaxLength(50);

                entity.Property(e => e.Place)
                    .HasColumnName("PLACE")
                    .HasMaxLength(150);

                entity.Property(e => e.Pobox)
                    .HasColumnName("POBOX")
                    .HasMaxLength(150);

                entity.Property(e => e.Postalcode)
                    .HasColumnName("POSTALCODE")
                    .HasMaxLength(50);

                entity.Property(e => e.Profitmarkupsharingrateadditionalproducts)
                    .HasColumnName("PROFITMARKUPSHARINGRATEADDITIONALPRODUCTS")
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Profitmarkupsharingratedailyprice)
                    .HasColumnName("PROFITMARKUPSHARINGRATEDAILYPRICE")
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Profitmarkupsharingrateonewayfee)
                    .HasColumnName("PROFITMARKUPSHARINGRATEONEWAYFEE")
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Rentalamountdeliverypayment)
                    .HasColumnName("RENTALAMOUNTDELIVERYPAYMENT")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Reservationsourceselectactive)
                    .HasColumnName("RESERVATIONSOURCESELECTACTIVE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Roleid)
                    .HasColumnName("ROLEID")
                    .HasDefaultValueSql("((4))");

                entity.Property(e => e.Sendreservationmailtoagency)
                    .HasColumnName("SENDRESERVATIONMAILTOAGENCY")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Sendreservationmailtocustomeractive)
                    .IsRequired()
                    .HasColumnName("SENDRESERVATIONMAILTOCUSTOMERACTIVE")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Showsubagency)
                    .HasColumnName("SHOWSUBAGENCY")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Surveyposttypeid)
                    .HasColumnName("SURVEYPOSTTYPEID")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Taxnumber1)
                    .HasColumnName("TAXNUMBER1")
                    .HasMaxLength(50);

                entity.Property(e => e.Taxnumber2)
                    .HasColumnName("TAXNUMBER2")
                    .HasMaxLength(100);

                entity.Property(e => e.Taxoffice)
                    .HasColumnName("TAXOFFICE")
                    .HasMaxLength(150);

                entity.Property(e => e.Tktue)
                    .HasColumnName("TKTUE")
                    .HasMaxLength(20);

                entity.Property(e => e.SpecialParameters)
                    .HasColumnName("SPECIALPARAMETERS")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.CreditType)
                    .HasColumnName("CREDITTYPE")
                    .HasColumnType("smallint")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.IsActiveSendCheapestCar)
                    .HasColumnName("ISACTIVESENDCHEAPESTCAR")
                    .HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<Agencylocation>(entity =>
            {
                entity.ToTable("AGENCYLOCATION");
                //10.12.2024 ID alanı tabloda olmadığı için kaldırıldı
                //entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Agencyid).HasColumnName("AGENCYID");

                entity.Property(e => e.Locationid).HasColumnName("LOCATIONID");
                entity.HasNoKey();
            });

            modelBuilder.Entity<Agencyvendor>(entity =>
            {
                entity.ToTable("AGENCYVENDOR");

                entity.HasIndex(e => new { e.Agencyid, e.Vendorid })
                    .HasName("AGENCYVENDOR_UN")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Agencyid).HasColumnName("AGENCYID");

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");
            });

            modelBuilder.Entity<Agencyvendorpaymentoption>(entity =>
            {
                entity.ToTable("AGENCYVENDORPAYMENTOPTION");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Additionalproductamountdeliverypayment)
                    .HasColumnName("ADDITIONALPRODUCTAMOUNTDELIVERYPAYMENT")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Advancepaymentactive).HasColumnName("ADVANCEPAYMENTACTIVE");

                entity.Property(e => e.Agencyid).HasColumnName("AGENCYID");

                entity.Property(e => e.Commissionfreepaymentactive).HasColumnName("COMMISSIONFREEPAYMENTACTIVE");

                entity.Property(e => e.Creditcardpaymentactive).HasColumnName("CREDITCARDPAYMENTACTIVE");

                entity.Property(e => e.Onewayamountdeliverypayment)
                    .HasColumnName("ONEWAYAMOUNTDELIVERYPAYMENT")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Payagencyactive).HasColumnName("PAYAGENCYACTIVE");

                entity.Property(e => e.Payallactive).HasColumnName("PAYALLACTIVE");

                entity.Property(e => e.Paydeliveryactive).HasColumnName("PAYDELIVERYACTIVE");

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");
            });

            modelBuilder.Entity<Agencyvendorprofitmarkup>(entity =>
            {
                entity.ToTable("AGENCYVENDORPROFITMARKUP");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("ACTIVE")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Agencyid).HasColumnName("AGENCYID");

                entity.Property(e => e.Profitmarkup)
                    .HasColumnName("PROFITMARKUP")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Profitmarkupadditionalproducts)
                    .HasColumnName("PROFITMARKUPADDITIONALPRODUCTS")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Profitmarkuponewayfee)
                    .HasColumnName("PROFITMARKUPONEWAYFEE")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");
            });

            modelBuilder.Entity<Apilog>(entity =>
            {
                entity.ToTable("APILOG");

                entity.HasIndex(e => e.TimeStamp)
                    .HasName("missing_index_103");

                entity.Property(e => e.ClientIp).HasColumnName("ClientIP");

                entity.Property(e => e.TimeStamp).HasColumnType("datetime");
            });

            modelBuilder.Entity<ApilogDev>(entity =>
            {
                entity.ToTable("APILOG_DEV");

                entity.Property(e => e.ClientIp).HasColumnName("ClientIP");

                entity.Property(e => e.TimeStamp).HasColumnType("datetime");
            });

            modelBuilder.Entity<Ayar>(entity =>
            {
                entity.ToTable("AYAR");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Autorateactive).HasColumnName("AUTORATEACTIVE");

                entity.Property(e => e.Googleanalytics)
                    .HasColumnName("GOOGLEANALYTICS")
                    .HasColumnType("ntext");

                entity.Property(e => e.Googleconversion)
                    .HasColumnName("GOOGLECONVERSION")
                    .HasColumnType("ntext");

                entity.Property(e => e.Googlediger)
                    .HasColumnName("GOOGLEDIGER")
                    .HasColumnType("ntext");

                entity.Property(e => e.Googledogrulama)
                    .HasColumnName("GOOGLEDOGRULAMA")
                    .HasColumnType("ntext");

                entity.Property(e => e.Googleremarketing)
                    .HasColumnName("GOOGLEREMARKETING")
                    .HasColumnType("ntext");

                entity.Property(e => e.Smtpmail)
                    .HasColumnName("SMTPMAIL")
                    .HasMaxLength(500);

                entity.Property(e => e.Smtpmailalici)
                    .HasColumnName("SMTPMAILALICI")
                    .HasMaxLength(500);

                entity.Property(e => e.Smtpport)
                    .HasColumnName("SMTPPORT")
                    .HasMaxLength(50);

                entity.Property(e => e.Smtppwd)
                    .HasColumnName("SMTPPWD")
                    .HasMaxLength(500);

                entity.Property(e => e.Smtpssl).HasColumnName("SMTPSSL");

                entity.Property(e => e.Smtpsunucu)
                    .HasColumnName("SMTPSUNUCU")
                    .HasMaxLength(500);

                entity.Property(e => e.Sslaktif)
                    .HasColumnName("SSLAKTIF")
                    .HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<Basvuru>(entity =>
            {
                entity.ToTable("BASVURU");

                entity.Property(e => e.Basvuruid).HasColumnName("BASVURUID");

                entity.Property(e => e.Aciklama).HasColumnName("ACIKLAMA");

                entity.Property(e => e.Adres).HasColumnName("ADRES");

                entity.Property(e => e.Aracsayisi).HasColumnName("ARACSAYISI");

                entity.Property(e => e.Benzinmanuel)
                    .HasColumnName("BENZINMANUEL")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Benzinotomatik)
                    .HasColumnName("BENZINOTOMATIK")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Dizelmanuel)
                    .HasColumnName("DIZELMANUEL")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Dizelotomatik)
                    .HasColumnName("DIZELOTOMATIK")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Email).HasColumnName("EMAIL");

                entity.Property(e => e.Firmaadi).HasColumnName("FIRMAADI");

                entity.Property(e => e.Firmasahibiadi).HasColumnName("FIRMASAHIBIADI");

                entity.Property(e => e.Firmasahibisoyadi).HasColumnName("FIRMASAHIBISOYADI");

                entity.Property(e => e.Goruldu)
                    .HasColumnName("GORULDU")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Ip).HasColumnName("IP");

                entity.Property(e => e.Sehir).HasColumnName("SEHIR");

                entity.Property(e => e.Tarih)
                    .HasColumnName("TARIH")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Telefon).HasColumnName("TELEFON");

                entity.Property(e => e.Vergidairesi).HasColumnName("VERGIDAIRESI");

                entity.Property(e => e.Verginumarasi).HasColumnName("VERGINUMARASI");
            });

            modelBuilder.Entity<Brokerlog>(entity =>
            {
                entity.ToTable("BROKERLOG");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Content).HasColumnName("CONTENT");

                entity.Property(e => e.Logdate)
                    .HasColumnName("LOGDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Logkey).HasColumnName("LOGKEY");

                entity.Property(e => e.Logtype).HasColumnName("LOGTYPE");

                entity.Property(e => e.Logtypekey)
                    .IsRequired()
                    .HasColumnName("LOGTYPEKEY");
            });

            modelBuilder.Entity<Bulten>(entity =>
            {
                entity.ToTable("BULTEN");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Acenteid).HasColumnName("ACENTEID");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("EMAIL")
                    .HasMaxLength(50);

                entity.Property(e => e.Ip)
                    .HasColumnName("IP")
                    .HasMaxLength(45);

                entity.Property(e => e.Tarih)
                    .HasColumnName("TARIH")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ContactPermission)
                .HasColumnName("CONTACTPERMISSION");
            });

            modelBuilder.Entity<City>(entity =>
            {
                entity.HasKey(e => new { e.Cityid, e.Countryid })
                    .HasName("PK_SEHIR_1");

                entity.ToTable("CITY");

                entity.Property(e => e.Cityid)
                    .HasColumnName("CITYID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Countryid).HasColumnName("COUNTRYID");

                entity.Property(e => e.Cityname)
                    .HasColumnName("CITYNAME")
                    .HasMaxLength(100);

                entity.Property(e => e.Parent).HasColumnName("PARENT");
            });

            modelBuilder.Entity<Commercialallowance>(entity =>
            {
                entity.ToTable("COMMERCIALALLOWANCE");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasColumnName("ACTIVE");

                entity.Property(e => e.Record).HasColumnName("RECORD");

                entity.Property(e => e.Recorddate)
                    .HasColumnName("RECORDDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Recordtypeid).HasColumnName("RECORDTYPEID");
            });

            modelBuilder.Entity<Companyinvoice>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("COMPANYINVOICE");

                entity.Property(e => e.City)
                    .HasColumnName("CITY")
                    .HasMaxLength(50);

                entity.Property(e => e.Companyaddress)
                    .HasColumnName("COMPANYADDRESS")
                    .HasMaxLength(500);

                entity.Property(e => e.Companyemail)
                    .HasColumnName("COMPANYEMAIL")
                    .HasMaxLength(100);

                entity.Property(e => e.Companyfax)
                    .HasColumnName("COMPANYFAX")
                    .HasMaxLength(100);

                entity.Property(e => e.Companyphone)
                    .HasColumnName("COMPANYPHONE")
                    .HasMaxLength(50);

                entity.Property(e => e.Companypostbox)
                    .HasColumnName("COMPANYPOSTBOX")
                    .HasMaxLength(50);

                entity.Property(e => e.Companytaxno)
                    .HasColumnName("COMPANYTAXNO")
                    .HasMaxLength(50);

                entity.Property(e => e.Companytaxoffice)
                    .HasColumnName("COMPANYTAXOFFICE")
                    .HasMaxLength(50);

                entity.Property(e => e.Companytitle)
                    .HasColumnName("COMPANYTITLE")
                    .HasMaxLength(250);

                entity.Property(e => e.Companywebsite)
                    .IsRequired()
                    .HasColumnName("COMPANYWEBSITE")
                    .HasMaxLength(100);

                entity.Property(e => e.County)
                    .HasColumnName("COUNTY")
                    .HasMaxLength(50);

                entity.Property(e => e.Invoicehtml)
                    .HasColumnName("INVOICEHTML")
                    .HasColumnType("ntext");

                entity.Property(e => e.Recid)
                    .HasColumnName("RECID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Taxrate).HasColumnName("TAXRATE");
            });

            modelBuilder.Entity<Country>(entity =>
            {
                entity.ToTable("COUNTRY");

                entity.Property(e => e.Countryid).HasColumnName("COUNTRYID");

                entity.Property(e => e.Countrycode2)
                    .HasColumnName("COUNTRYCODE2")
                    .HasMaxLength(10);

                entity.Property(e => e.Countrycode3)
                    .HasColumnName("COUNTRYCODE3")
                    .HasMaxLength(10);

                entity.Property(e => e.Defaultcountry).HasColumnName("DEFAULTCOUNTRY");

                entity.Property(e => e.Numbercode).HasColumnName("NUMBERCODE");

                entity.Property(e => e.Phonecode).HasColumnName("PHONECODE");
            });

            modelBuilder.Entity<Countrylang>(entity =>
            {
                entity.ToTable("COUNTRYLANG");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Countryid).HasColumnName("COUNTRYID");

                entity.Property(e => e.Countryname)
                    .HasColumnName("COUNTRYNAME")
                    .HasMaxLength(100);

                entity.Property(e => e.Lang).HasColumnName("LANG");
            });

            modelBuilder.Entity<Coupon>(entity =>
            {
                entity.ToTable("COUPON", tb => tb.UseSqlOutputClause(false));

                #region gkursad - 08.04.2024
                /*entity.Property(e => e.Id).HasColumnName("ID");
                        entity.Property(e => e.AdminId).HasColumnName("ADMINID");
                        entity.Property(e => e.Active).HasColumnName("ACTIVE");

                        entity.Property(e => e.AdminId).HasColumnName("ADMINID");

                        entity.Property(e => e.AgencyId).HasColumnName("AGENCYID");

                        entity.Property(e => e.Code)
                            .IsRequired()
                            .HasColumnName("CODE");

                        entity.Property(e => e.CreationDate)
                            .HasColumnName("CREATIONDATE")
                            .HasColumnType("datetime")
                            .HasDefaultValueSql("(getdate())");

                        entity.Property(e => e.CreationType).HasColumnName("CREATIONTYPE");

                        entity.Property(e => e.CurrencyId).HasColumnName("CURRENCYID");

                        entity.Property(e => e.Description).HasColumnName("DESCRIPTION");

                        entity.Property(e => e.DiscountType).HasColumnName("DISCOUNTTYPE");

                        entity.Property(e => e.DiscountValue)
                            .HasColumnName("DISCOUNTVALUE")
                            .HasColumnType("decimal(18, 2)");

                        entity.Property(e => e.EndDate)
                            .HasColumnName("ENDDATE")
                            .HasColumnType("datetime");

                        entity.Property(e => e.MemberId).HasColumnName("MEMBERID");

                        entity.Property(e => e.MultipleUsage).HasColumnName("MULTIPLEUSAGE");

                        entity.Property(e => e.Name)
                            .IsRequired()
                            .HasColumnName("NAME");

                        entity.Property(e => e.StartDate)
                            .HasColumnName("STARTDATE")
                            .HasColumnType("datetime");
                        entity.Property(e => e.VendorId).HasColumnName("VENDORID");
                        entity.Property(e => e.UsageCount).HasColumnName("USAGECOUNT");
                        entity.Property(e => e.ShowPrice).HasColumnName("SHOWPRICE");*/
                #endregion

                entity.Property(c => c.Id).HasColumnName("ID").IsRequired().UseIdentityColumn();
                entity.Property(c => c.AdminId).HasColumnName("ADMINID");
                entity.Property(c => c.AgencyId).HasColumnName("AGENCYID");
                entity.Property(c => c.MemberId).HasColumnName("MEMBERID");
                entity.Property(c => c.Active).HasColumnName("ACTIVE").HasDefaultValueSql("((0))");
                entity.Property(c => c.Code).HasColumnName("CODE").IsRequired();
                entity.Property(c => c.Name).HasColumnName("NAME").IsRequired();
                entity.Property(c => c.Description).HasColumnName("DESCRIPTION");
                entity.Property(c => c.MultipleUsage).HasColumnName("MULTIPLEUSAGE").HasDefaultValueSql("((0))");
                entity.Property(c => c.UsageCount).HasColumnName("USAGECOUNT").IsRequired().HasDefaultValueSql("((0))");
                entity.Property(c => c.CreationType).HasColumnName("CREATIONTYPE").IsRequired();
                entity.Property(c => c.CreationDate).HasColumnName("CREATIONDATE").IsRequired().HasDefaultValueSql("(getdate())");
                entity.Property(c => c.DiscountType).HasColumnName("DISCOUNTTYPE").IsRequired();
                entity.Property(c => c.DiscountValue).HasColumnName("DISCOUNTVALUE").HasColumnType("DECIMAL(18, 2)").IsRequired().HasDefaultValueSql("((0))");
                entity.Property(c => c.CurrencyId).HasColumnName("CURRENCYID");
                entity.Property(c => c.StartDate).HasColumnName("STARTDATE");
                entity.Property(c => c.EndDate).HasColumnName("ENDDATE");
                entity.Property(c => c.CouponStartDate).HasColumnName("COUPONSTARTDATE");
                entity.Property(c => c.CouponEndDate).HasColumnName("COUPONENDDATE");
                entity.Property(c => c.MinimumDay).HasColumnName("MINIMUMDAY");
                entity.Property(c => c.MaximumDay).HasColumnName("MAXIMUMDAY");
                entity.Property(c => c.MinimumAmount).HasColumnName("MINIMUMAMOUNT").HasColumnType("decimal(18, 2)").HasDefaultValueSql("((0))");
                entity.Property(c => c.MaximumAmount).HasColumnName("MAXIMUMAMOUNT").HasColumnType("decimal(18, 2)").HasDefaultValueSql("((0))");
                entity.Property(c => c.PickupLocation).HasColumnName("PICKUPLOCATIONID");
                entity.Property(c => c.ReturnLocation).HasColumnName("RETURNLOCATIONID");
                entity.Property(c => c.VendorId).HasColumnName("VENDORID");
                entity.Property(c => c.MaxUsageCount).HasColumnName("MAXUSAGECOUNT");
                entity.Property(c => c.ConnectedReservationId).HasColumnName("CONNECTEDRESERVATION");
                entity.Property(c => c.ShowPrice).HasColumnName("SHOWPRICE").HasDefaultValueSql("((0))");
                entity.Property(c => c.CustomerMailAddress).HasColumnName("CUSTOMERMAILADDRESS");
                entity.Property(c => c.VendorDiscountValue).HasColumnName("VENDORDISCOUNTVALUE").HasDefaultValueSql("(0)");
                entity.Property(c => c.AnyoneCanUse).HasColumnName("ANYONECANUSE").HasDefaultValueSql("(0)");
                entity.Property(c => c.UseOncePerEMail).HasColumnName("USEONCEPEREMAIL").HasDefaultValueSql("(0)");// TODO : Bu alan gereksiz oldu kaldırılacak
                entity.Property(c => c.LimitedUsage).HasColumnName("LIMITEDUSAGE").HasDefaultValueSql("(0)");
                entity.Property(c => c.LimitedUsageField).HasColumnName("LIMITEDUSAGEFIELD");
                entity.Property(c => c.LimitedUsageCount).HasColumnName("LIMITEDUSAGECOUNT").HasDefaultValueSql("(0)");
                entity.Property(c => c.ShowInVehicleList).HasColumnName("SHOWINVEHICLELIST").HasDefaultValueSql("(0)");
            });

            modelBuilder.Entity<CouponVendor>(entity =>
            {
                entity.ToTable("COUPONVENDORS");

                entity.HasKey(c => c.Id);

                entity.HasIndex(c => new { c.CouponId, c.VendorId });

                entity.Property(c => c.Id).HasColumnName("ID").IsRequired().UseIdentityColumn();
                entity.Property(c => c.CouponId).HasColumnName("CouponId").IsRequired();
                entity.Property(c => c.VendorId).HasColumnName("VendorId").IsRequired();
                entity.Property(c => c.VendorRate).HasColumnName("VendorRate").HasColumnType("DECIMAL(18, 2)").HasDefaultValue(0M);
            });

            modelBuilder.Entity<Currency>(entity =>
            {
                entity.ToTable("CURRENCY");

                entity.Property(e => e.Currencyid)
                    .HasColumnName("CURRENCYID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Active).HasColumnName("ACTIVE");

                entity.Property(e => e.Currencyisocode)
                    .HasColumnName("CURRENCYISOCODE")
                    .HasMaxLength(10);

                entity.Property(c => c.Symbol)
                     .HasColumnName("CURRENCYSYMBOL");

                entity.Property(e => e.Currencyname)
                    .HasColumnName("CURRENCYNAME")
                    .HasMaxLength(20);

                entity.Property(e => e.InternationalActive)
                 .HasColumnName("INTERNATIONALACTIVE")
                 .HasMaxLength(20);
            });

            modelBuilder.Entity<Currentaccount>(entity =>
            {
                entity.HasKey(e => e.RecId);

                entity.ToTable("CURRENTACCOUNT");

                entity.Property(e => e.RecId).HasColumnName("RECID");

                entity.Property(e => e.AgencyId).HasColumnName("AGENCYID");

                entity.Property(e => e.Amount)
                    .HasColumnName("AMOUNT")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CurrencyId).HasColumnName("CURRENCYID");

                entity.Property(e => e.DocumentNumber)
                    .HasColumnName("DOCUMENTNUMBER")
                    .HasMaxLength(50);

                entity.Property(e => e.Note)
                    .HasColumnName("NOTE")
                    .HasMaxLength(500);

                entity.Property(e => e.RegisteredDate)
                    .HasColumnName("REGISTEREDDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ResId).HasColumnName("RESID");

                entity.Property(e => e.ResPickUpDate)
                    .HasColumnName("RESPICKUPDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.ResReturnDate)
                    .HasColumnName("RESRETURNDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.TransactionDate)
                    .HasColumnName("TRANSACTIONDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.TransactionType)
                    .HasColumnName("TRANSACTIONTYPE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Type).HasColumnName("TYPE");

                entity.Property(e => e.VendorId).HasColumnName("VENDORID");
                entity.Property(e => e.IsPaid).HasColumnName("ISPAID");
                entity.Property(e => e.CurrentType).HasColumnName("CURRENTTYPE");
                entity.Property(e => e.PenaltyAmount).HasColumnName("PENALTYAMOUNT ");


            });

            modelBuilder.Entity<Currentaccounthistory>(entity =>
            {
                entity.HasKey(e => e.Recid);

                entity.ToTable("CURRENTACCOUNTHISTORY");

                entity.Property(e => e.Recid)
                    .HasColumnName("RECID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Agencyid).HasColumnName("AGENCYID");

                entity.Property(e => e.Amount)
                    .HasColumnName("AMOUNT")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Currencyid).HasColumnName("CURRENCYID");

                entity.Property(e => e.Documentnumber)
                    .HasColumnName("DOCUMENTNUMBER")
                    .HasMaxLength(50);

                entity.Property(e => e.Note)
                    .HasColumnName("NOTE")
                    .HasMaxLength(500);

                entity.Property(e => e.Registereddate)
                    .HasColumnName("REGISTEREDDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Resid).HasColumnName("RESID");

                entity.Property(e => e.Respickupdate)
                    .HasColumnName("RESPICKUPDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Resreturndate)
                    .HasColumnName("RESRETURNDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Transactiondate)
                    .HasColumnName("TRANSACTIONDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Transactiontype)
                    .HasColumnName("TRANSACTIONTYPE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Type).HasColumnName("TYPE");

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");
            });

            modelBuilder.Entity<Currentaccountvendor>(entity =>
            {
                entity.HasKey(e => e.Recid);

                entity.ToTable("CURRENTACCOUNTVENDOR");

                entity.Property(e => e.Recid).HasColumnName("RECID");

                entity.Property(e => e.Agencyid).HasColumnName("AGENCYID");

                entity.Property(e => e.Amount)
                    .HasColumnName("AMOUNT")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Currencyid).HasColumnName("CURRENCYID");

                entity.Property(e => e.Documentnumber)
                    .HasColumnName("DOCUMENTNUMBER")
                    .HasMaxLength(50);

                entity.Property(e => e.Note)
                    .HasColumnName("NOTE")
                    .HasMaxLength(500);

                entity.Property(e => e.Registereddate)
                    .HasColumnName("REGISTEREDDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Resid).HasColumnName("RESID");

                entity.Property(e => e.Respickupdate)
                    .HasColumnName("RESPICKUPDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Resreturndate)
                    .HasColumnName("RESRETURNDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Transactiondate)
                    .HasColumnName("TRANSACTIONDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Transactiontype)
                    .HasColumnName("TRANSACTIONTYPE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Type).HasColumnName("TYPE");

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");
            });

            modelBuilder.Entity<Currentaccountvendorhistory>(entity =>
            {
                entity.HasKey(e => e.Recid);

                entity.ToTable("CURRENTACCOUNTVENDORHISTORY");

                entity.Property(e => e.Recid)
                    .HasColumnName("RECID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Agencyid).HasColumnName("AGENCYID");

                entity.Property(e => e.Amount)
                    .HasColumnName("AMOUNT")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Currencyid).HasColumnName("CURRENCYID");

                entity.Property(e => e.Documentnumber)
                    .HasColumnName("DOCUMENTNUMBER")
                    .HasMaxLength(50);

                entity.Property(e => e.Note)
                    .HasColumnName("NOTE")
                    .HasMaxLength(500);

                entity.Property(e => e.Registereddate)
                    .HasColumnName("REGISTEREDDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Resid).HasColumnName("RESID");

                entity.Property(e => e.Respickupdate)
                    .HasColumnName("RESPICKUPDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Resreturndate)
                    .HasColumnName("RESRETURNDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Transactiondate)
                    .HasColumnName("TRANSACTIONDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Transactiontype)
                    .HasColumnName("TRANSACTIONTYPE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Type).HasColumnName("TYPE");

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");
            });

            modelBuilder.Entity<Dil>(entity =>
            {
                entity.HasKey(e => new { e.Dilid, e.Dilkod });

                entity.ToTable("DIL");

                entity.Property(e => e.Dilid).HasColumnName("DILID");

                entity.Property(e => e.Dilkod)
                    .HasColumnName("DILKOD")
                    .HasMaxLength(2);

                entity.Property(e => e.Aktif).HasColumnName("AKTIF");

                entity.Property(e => e.Bayrak)
                    .HasColumnName("BAYRAK")
                    .HasMaxLength(50);

                entity.Property(e => e.Dilad)
                    .HasColumnName("DILAD")
                    .HasMaxLength(50);

                entity.Property(e => e.Gorunurad)
                    .HasColumnName("GORUNURAD")
                    .HasMaxLength(50);

                entity.Property(l => l.Culture)
                    .HasColumnName("KULTUR")
                    .HasMaxLength(50);

                entity.Property(l => l.IsLtr)
                    .HasColumnName("LTR")
                    .HasDefaultValue(false);
            });

            modelBuilder.Entity<Exchangecopy>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("EXCHANGECOPY");

                entity.Property(e => e.Currencyid).HasColumnName("CURRENCYID");

                entity.Property(e => e.Date)
                    .HasColumnName("DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Exchangerate)
                    .HasColumnName("EXCHANGERATE")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Recid)
                    .HasColumnName("RECID")
                    .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Exchangeratecenter>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("EXCHANGERATECENTER");

                entity.Property(e => e.Currencycode)
                    .HasColumnName("CURRENCYCODE")
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.Property(e => e.Date)
                    .HasColumnName("DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Exchangerate)
                    .HasColumnName("EXCHANGERATE")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Piece).HasColumnName("PIECE");

                entity.Property(e => e.Recid)
                    .HasColumnName("RECID")
                    .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Exchangerates>(entity =>
            {
                entity.HasKey(e => new { e.Recid, e.Currencyid });

                entity.ToTable("EXCHANGERATES");

                entity.Property(e => e.Recid)
                    .HasColumnName("RECID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Currencyid).HasColumnName("CURRENCYID");

                entity.Property(e => e.Date)
                    .HasColumnName("DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Exchangerate)
                    .HasColumnName("EXCHANGERATE")
                    .HasColumnType("decimal(18, 4)");
            });

            modelBuilder.Entity<Exchangerateshistory>(entity =>
            {
                entity.HasKey(e => new { e.Recid, e.Currencyid });

                entity.ToTable("EXCHANGERATESHISTORY");

                entity.Property(e => e.Recid)
                    .HasColumnName("RECID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Currencyid).HasColumnName("CURRENCYID");

                entity.Property(e => e.Date)
                    .HasColumnName("DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Exchangerate)
                    .HasColumnName("EXCHANGERATE")
                    .HasColumnType("decimal(18, 4)");
            });

            modelBuilder.Entity<Hata>(entity =>
            {
                entity.ToTable("HATA");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Bilgi).HasColumnName("BILGI");

                entity.Property(e => e.Browser).HasColumnName("BROWSER");

                entity.Property(e => e.Hata1)
                    .HasColumnName("HATA")
                    .HasColumnType("ntext");

                entity.Property(e => e.Ip)
                    .HasColumnName("IP")
                    .HasMaxLength(50);

                entity.Property(e => e.Kullaniciid).HasColumnName("KULLANICIID");

                entity.Property(e => e.Pathinfo).HasColumnName("PATHINFO");

                entity.Property(e => e.Referer).HasColumnName("REFERER");

                entity.Property(e => e.Rezid).HasColumnName("REZID");

                entity.Property(e => e.Sorgu)
                    .HasColumnName("SORGU")
                    .HasColumnType("ntext");

                entity.Property(e => e.Tarih)
                    .HasColumnName("TARIH")
                    .HasColumnType("datetime");

                entity.Property(e => e.Uyeid).HasColumnName("UYEID");
            });

            modelBuilder.Entity<Icerik>(entity =>
            {
                entity.HasKey(e => new { e.Icerikid, e.Tipid, e.Grupid });

                entity.ToTable("ICERIK");

                entity.Property(e => e.Icerikid).HasColumnName("ICERIKID");

                entity.Property(e => e.Tipid).HasColumnName("TIPID");

                entity.Property(e => e.Grupid).HasColumnName("GRUPID");

                entity.Property(e => e.Aktif).HasColumnName("AKTIF");

                entity.Property(e => e.Baslangic)
                    .HasColumnName("BASLANGIC")
                    .HasColumnType("datetime");

                entity.Property(e => e.Bitis)
                    .HasColumnName("BITIS")
                    .HasColumnType("datetime");

                entity.Property(e => e.Createdate)
                    .HasColumnName("CREATEDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Gosterim).HasColumnName("GOSTERIM");

                entity.Property(e => e.Kirakosulaktif)
                    .HasColumnName("KIRAKOSULAKTIF")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Kirasozlesmeaktif)
                    .HasColumnName("KIRASOZLESMEAKTIF")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Kvkkmetinaktif)
                    .HasColumnName("KVKKMETINAKTIF")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Lokasyonid).HasColumnName("LOKASYONID");

                entity.Property(e => e.Modulid)
                    .HasColumnName("MODULID")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Parent).HasColumnName("PARENT");

                entity.Property(e => e.Sinifid).HasColumnName("SINIFID");

                entity.Property(e => e.Sira).HasColumnName("SIRA");

                entity.Property(e => e.Vehicleclassid).HasColumnName("VEHICLECLASSID");

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");

            });

            modelBuilder.Entity<Icerikdil>(entity =>
            {
                entity.HasKey(e => new { e.Icerikid, e.Dilid });

                entity.ToTable("ICERIKDIL");

                entity.HasIndex(e => new { e.Dilid, e.Contenturl })
                    .HasName("UI_ICERIKDIL_CONTENTURL")
                    .IsUnique();

                entity.HasIndex(e => new { e.Dilid, e.Baslik, e.Icerikid })
                    .HasName("missing_index_211");

                entity.Property(e => e.Icerikid).HasColumnName("ICERIKID");

                entity.Property(e => e.Dilid).HasColumnName("DILID");

                entity.Property(e => e.Aktifmi).HasColumnName("AKTIFMI");

                entity.Property(e => e.Baslik)
                    .HasColumnName("BASLIK")
                    .HasMaxLength(250);

                entity.Property(e => e.Canonical)
                    .HasColumnName("CANONICAL")
                    .HasMaxLength(500);

                entity.Property(e => e.Contenturl)
                    .HasColumnName("CONTENTURL")
                    .HasMaxLength(500);

                entity.Property(e => e.Description)
                    .HasColumnName("DESCRIPTION")
                    .HasMaxLength(500);

                entity.Property(e => e.Editor).HasColumnName("EDITOR");

                entity.Property(e => e.Keyword)
                    .HasColumnName("KEYWORD")
                    .HasMaxLength(500);

                entity.Property(e => e.Link)
                    .HasColumnName("LINK")
                    .HasMaxLength(500);

                entity.Property(e => e.Ozet).HasColumnName("OZET");

                entity.Property(e => e.Resim)
                    .HasColumnName("RESIM")
                    .HasMaxLength(500);

                entity.Property(e => e.Resimalt)
                    .HasColumnName("RESIMALT")
                    .HasMaxLength(500);

                entity.Property(e => e.Syncicerikid).HasColumnName("SYNCICERIKID");

                entity.Property(e => e.Title)
                    .HasColumnName("TITLE")
                    .HasMaxLength(250);
                entity.Property(e => e.BaslangicTarihi).HasColumnName("BASLANGICTARIHI");
                entity.Property(e => e.BitisTarihi).HasColumnName("BITISTARIHI");

                entity
                .HasOne(c => c.Icerik)
                .WithMany(con => con.IcerikDiller)
                .HasForeignKey(c => c.Icerikid)
                .HasPrincipalKey(con => con.Icerikid)
                .OnDelete(DeleteBehavior.ClientSetNull);

            });

            modelBuilder.Entity<Icerikgrup>(entity =>
            {
                entity.HasKey(e => new { e.Grupid, e.Tipid, e.Dilid });

                entity.ToTable("ICERIKGRUP");

                entity.Property(e => e.Grupid).HasColumnName("GRUPID");

                entity.Property(e => e.Tipid).HasColumnName("TIPID");

                entity.Property(e => e.Dilid).HasColumnName("DILID");

                entity.Property(e => e.Aktif)
                    .HasColumnName("AKTIF")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Grupadi)
                    .HasColumnName("GRUPADI")
                    .HasMaxLength(250);

                entity.Property(e => e.Link).HasColumnName("LINK");

                entity.Property(e => e.Sira)
                    .HasColumnName("SIRA")
                    .HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<Iceriktip>(entity =>
            {
                entity.HasKey(e => e.Tipid);

                entity.ToTable("ICERIKTIP");

                entity.Property(e => e.Tipid)
                    .HasColumnName("TIPID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Sira).HasColumnName("SIRA");

                entity.Property(e => e.Tipadi)
                    .HasColumnName("TIPADI")
                    .HasMaxLength(250);
            });

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.ToTable("INVOICE");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Accountinginvoicenumber)
                    .HasColumnName("ACCOUNTINGINVOICENUMBER")
                    .HasMaxLength(50);

                entity.Property(e => e.Address).HasColumnName("ADDRESS");

                entity.Property(e => e.Canceldate)
                    .HasColumnName("CANCELDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Cancelinvoiceserviceresponse).HasColumnName("CANCELINVOICESERVICERESPONSE");

                entity.Property(e => e.Cityid).HasColumnName("CITYID");

                entity.Property(e => e.Companytitle)
                    .HasColumnName("COMPANYTITLE")
                    .HasMaxLength(250);

                entity.Property(e => e.Countryid).HasColumnName("COUNTRYID");

                entity.Property(e => e.Createinvoiceserviceresponse).HasColumnName("CREATEINVOICESERVICERESPONSE");

                entity.Property(e => e.Currencyid).HasColumnName("CURRENCYID");

                entity.Property(e => e.Customeremail)
                    .IsRequired()
                    .HasColumnName("CUSTOMEREMAIL")
                    .HasMaxLength(250);

                entity.Property(e => e.Customeridentitynumber)
                    .HasColumnName("CUSTOMERIDENTITYNUMBER")
                    .HasMaxLength(100);

                entity.Property(e => e.Customername)
                    .HasColumnName("CUSTOMERNAME")
                    .HasMaxLength(250);

                entity.Property(e => e.Customerphonenumber)
                    .IsRequired()
                    .HasColumnName("CUSTOMERPHONENUMBER")
                    .HasMaxLength(250);

                entity.Property(e => e.Customersurname)
                    .HasColumnName("CUSTOMERSURNAME")
                    .HasMaxLength(250);

                entity.Property(e => e.Customertype).HasColumnName("CUSTOMERTYPE");

                entity.Property(e => e.Districtid).HasColumnName("DISTRICTID");

                entity.Property(e => e.Exchangerate)
                    .HasColumnName("EXCHANGERATE")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Invoicedate)
                    .HasColumnName("INVOICEDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Invoicetype).HasColumnName("INVOICETYPE");

                entity.Property(e => e.Piece)
                    .HasColumnName("PIECE")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Priceexcludingvat)
                    .HasColumnName("PRICEEXCLUDINGVAT")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Reservationnumber)
                    .IsRequired()
                    .HasColumnName("RESERVATIONNUMBER")
                    .HasMaxLength(100);

                entity.Property(e => e.Servicename)
                    .HasColumnName("SERVICENAME")
                    .HasMaxLength(100);

                entity.Property(e => e.Status).HasColumnName("STATUS");

                entity.Property(e => e.Taxnumber)
                    .HasColumnName("TAXNUMBER")
                    .HasMaxLength(100);

                entity.Property(e => e.Taxoffice)
                    .HasColumnName("TAXOFFICE")
                    .HasMaxLength(100);

                entity.Property(e => e.Totalpriceincludingvat)
                    .HasColumnName("TOTALPRICEINCLUDINGVAT")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Totalvat)
                    .HasColumnName("TOTALVAT")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Unitpriceexcludingvat)
                    .HasColumnName("UNITPRICEEXCLUDINGVAT")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Vatrate).HasColumnName("VATRATE");
            });

            modelBuilder.Entity<Kullanici>(entity =>
            {
                entity.ToTable("KULLANICI");

                entity.Property(e => e.Kullaniciid)
                    .HasColumnName("KULLANICIID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Ad)
                    .HasColumnName("AD")
                    .HasMaxLength(250);

                entity.Property(e => e.Aktif).HasColumnName("AKTIF");

                entity.Property(e => e.Eposta)
                    .HasColumnName("EPOSTA")
                    .HasMaxLength(250);

                entity.Property(e => e.Pwd)
                    .HasColumnName("PWD")
                    .HasMaxLength(250);

                entity.Property(e => e.Roleid).HasColumnName("ROLEID");

                entity.Property(e => e.Soyad)
                    .HasColumnName("SOYAD")
                    .HasMaxLength(250);

                entity.Property(e => e.Telefon)
                    .HasColumnName("TELEFON")
                    .HasMaxLength(250);
            });

            modelBuilder.Entity<Label>(entity =>
            {
                entity.HasKey(e => new { e.Labelid, e.Dilid });

                entity.ToTable("LABEL");

                entity.Property(e => e.Labelid).HasColumnName("LABELID");

                entity.Property(e => e.Dilid).HasColumnName("DILID");

                entity.Property(e => e.Labeladi).HasColumnName("LABELADI");

                entity.Property(e => e.LabelKodu).HasColumnName("LABELKODU");

                entity.Property(e => e.TypeId).HasColumnName("TYPEID");
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.Langid });

                entity.ToTable("LOCATION");

                entity.HasIndex(e => new { e.Active, e.Locationname, e.Langid })
                    .HasName("missing_index_46");

                entity.HasIndex(e => new { e.Locationname, e.Active, e.Langid })
                    .HasName("missing_index_44");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Langid).HasColumnName("LANGID");

                entity.Property(e => e.Active).HasColumnName("ACTIVE");

                entity.Property(e => e.Address).HasColumnName("ADDRESS");

                entity.Property(e => e.Airport).HasColumnName("AIRPORT");

                entity.Property(e => e.Cityid).HasColumnName("CITYID");

                entity.Property(e => e.Coordinatelatitude)
                    .HasColumnName("COORDINATELATITUDE")
                    .HasMaxLength(100);

                entity.Property(e => e.Coordinatelongitude)
                    .HasColumnName("COORDINATELONGITUDE")
                    .HasMaxLength(100);

                entity.Property(e => e.Countryid).HasColumnName("COUNTRYID");

                entity.Property(e => e.Iata)
                    .HasColumnName("IATA")
                    .HasMaxLength(50);

                entity.Property(e => e.Imagepath).HasColumnName("IMAGEPATH");

                entity.Property(e => e.Ispickup).HasColumnName("ISPICKUP");

                entity.Property(e => e.Ispopular)
                    .HasColumnName("ISPOPULAR")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Locationname)
                    .HasColumnName("LOCATIONNAME")
                    .HasMaxLength(200);

                entity.Property(e => e.Mailaddress).HasColumnName("MAILADDRESS");

                entity.Property(e => e.Phonenumber).HasColumnName("PHONENUMBER");
            });

            modelBuilder.Entity<Locationvendor>(entity =>
            {
                entity.ToTable("LOCATIONVENDOR");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasColumnName("ACTIVE");

                entity.Property(e => e.Apilocationname)
                    .HasColumnName("APILOCATIONNAME")
                    .HasMaxLength(250);

                entity.Property(e => e.Isoffice)
                    .HasColumnName("ISOFFICE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Ispickup).HasColumnName("ISPICKUP");

                entity.Property(e => e.Locallocationid).HasColumnName("LOCALLOCATIONID");

                entity.Property(e => e.Locationcode)
                    .HasColumnName("LOCATIONCODE")
                    .HasMaxLength(150);

                entity.Property(e => e.Locationid).HasColumnName("LOCATIONID");

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");


                entity.Property(e => e.DistrictCode).HasColumnName("DISTRICTCODE");

                entity.Property(e => e.CityCode).HasColumnName("CITYCODE");

                entity.Property(e => e.FlightCardMandatory).HasColumnName("FLIGHTCARDMANDATORY");
                entity.Property(e => e.EarliestResTime).HasColumnName("EARLIESTRESTIME");
                entity.Property(e => e.RateCode).HasColumnName("RateCode");
            });

            modelBuilder.Entity<Locationvendorcloseddate>(entity =>
            {
                entity.ToTable("LOCATIONVENDORCLOSEDDATE");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Enddate)
                    .HasColumnName("ENDDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Locationid).HasColumnName("LOCATIONID");

                entity.Property(e => e.Startdate)
                    .HasColumnName("STARTDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.ToTable("LOG");

                entity.Property(e => e.Logid).HasColumnName("LOGID");

                entity.Property(e => e.Agencyid).HasColumnName("AGENCYID");

                entity.Property(e => e.Browser)
                    .HasColumnName("BROWSER")
                    .HasMaxLength(500);

                entity.Property(e => e.Ip)
                    .HasColumnName("IP")
                    .HasMaxLength(50);

                entity.Property(e => e.Islem)
                    .HasColumnName("ISLEM")
                    .HasMaxLength(500);

                entity.Property(e => e.Kullaniciadi)
                    .HasColumnName("KULLANICIADI")
                    .HasMaxLength(500);

                entity.Property(e => e.Kullaniciid).HasColumnName("KULLANICIID");

                entity.Property(e => e.Logtip)
                    .HasColumnName("LOGTIP")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Pathinfo)
                    .HasColumnName("PATHINFO")
                    .HasMaxLength(500);

                entity.Property(e => e.Referer)
                    .HasColumnName("REFERER")
                    .HasMaxLength(500);

                entity.Property(e => e.Tarih)
                    .HasColumnName("TARIH")
                    .HasColumnType("datetime");

                entity.Property(e => e.Uyeadi)
                    .HasColumnName("UYEADI")
                    .HasMaxLength(500);

                entity.Property(e => e.Uyeid).HasColumnName("UYEID");
            });

            modelBuilder.Entity<Parametre>(entity =>
            {
                entity.ToTable("PARAMETRE");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Aciklama).HasColumnName("ACIKLAMA");

                entity.Property(e => e.Deger).HasColumnName("DEGER");

                entity.Property(e => e.Degisken).HasColumnName("DEGISKEN");
            });

            modelBuilder.Entity<Popularvehicle>(entity =>
            {
                entity.ToTable("POPULARVEHICLE");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasColumnName("ACTIVE");

                entity.Property(e => e.Islocalvehicle).HasColumnName("ISLOCALVEHICLE");

                entity.Property(e => e.Recorddate)
                    .HasColumnName("RECORDDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Vehiclecode)
                    .IsRequired()
                    .HasColumnName("VEHICLECODE");

                entity.Property(e => e.Vehiclename)
                    .IsRequired()
                    .HasColumnName("VEHICLENAME");

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");
            });

            modelBuilder.Entity<Portalacente>(entity =>
            {
                entity.ToTable("PORTALACENTE");

                entity.Property(e => e.Portalacenteid)
                    .HasColumnName("PORTALACENTEID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Acenteadi)
                    .IsRequired()
                    .HasColumnName("ACENTEADI")
                    .HasMaxLength(250);

                entity.Property(e => e.Acentekodu)
                    .HasColumnName("ACENTEKODU")
                    .HasMaxLength(50);

                entity.Property(e => e.Acentekomisyon).HasColumnName("ACENTEKOMISYON");

                entity.Property(e => e.Adres)
                    .HasColumnName("ADRES")
                    .HasMaxLength(250);

                entity.Property(e => e.Aktif).HasColumnName("AKTIF");

                entity.Property(e => e.Apikey)
                    .HasColumnName("APIKEY")
                    .HasMaxLength(250);

                entity.Property(e => e.Eposta)
                    .HasColumnName("EPOSTA")
                    .HasMaxLength(100);

                entity.Property(e => e.Gsm)
                    .HasColumnName("GSM")
                    .HasMaxLength(100);

                entity.Property(e => e.Kcportalacenteid).HasColumnName("KCPORTALACENTEID");

                entity.Property(e => e.Logo)
                    .HasColumnName("LOGO")
                    .HasMaxLength(500);

                entity.Property(e => e.Parent).HasColumnName("PARENT");

                entity.Property(e => e.Sifre)
                    .HasColumnName("SIFRE")
                    .HasMaxLength(250);

                entity.Property(e => e.Tarih)
                    .HasColumnName("TARIH")
                    .HasColumnType("datetime");

                entity.Property(e => e.Telefon)
                    .HasColumnName("TELEFON")
                    .HasMaxLength(100);

                entity.Property(e => e.Yetkili)
                    .HasColumnName("YETKILI")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Portalacentekullanici>(entity =>
            {
                entity.HasKey(e => new { e.Portalacentekullaniciid, e.Portalacenteid });

                entity.ToTable("PORTALACENTEKULLANICI");

                entity.Property(e => e.Portalacentekullaniciid)
                    .HasColumnName("PORTALACENTEKULLANICIID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Portalacenteid).HasColumnName("PORTALACENTEID");

                entity.Property(e => e.Ad)
                    .HasColumnName("AD")
                    .HasMaxLength(250);

                entity.Property(e => e.Aktif).HasColumnName("AKTIF");

                entity.Property(e => e.Kayittarihi)
                    .HasColumnName("KAYITTARIHI")
                    .HasColumnType("datetime");

                entity.Property(e => e.Kcportalacentekullaniciid).HasColumnName("KCPORTALACENTEKULLANICIID");

                entity.Property(e => e.Kullaniciadi)
                    .HasColumnName("KULLANICIADI")
                    .HasMaxLength(250);

                entity.Property(e => e.Kullanicieposta)
                    .HasColumnName("KULLANICIEPOSTA")
                    .HasMaxLength(250);

                entity.Property(e => e.Sifre)
                    .HasColumnName("SIFRE")
                    .HasMaxLength(250);

                entity.Property(e => e.Soyad)
                    .HasColumnName("SOYAD")
                    .HasMaxLength(250);
            });

            modelBuilder.Entity<ProfitMarkup>(entity =>
            {
                entity.ToTable("PROFITMARKUPS", "dbo");

                entity.HasKey(p => p.Id);

                entity.Property(p => p.Id).HasColumnName("ID").IsRequired().UseIdentityColumn();
                entity.Property(p => p.Type).HasColumnName("TYPE").IsRequired();
                entity.Property(p => p.Priority).HasColumnName("PRIORITY");
                entity.Property(p => p.MarkupType).HasColumnName("MARKUPTYPE");
                entity.Property(p => p.MinimumDay).HasColumnName("MINIMUMDAY");
                entity.Property(p => p.MaximumDay).HasColumnName("MAXIMUMDAY");
                entity.Property(p => p.LastByUpdateUserId).HasColumnName("LASTUPDATEBYUSER");
                entity.Property(p => p.CurrencyId).HasColumnName("CURRENCYID");
                entity.Property(p => p.AmountCurrencyId).HasColumnName("AMOUNTCURRENCYID");

                entity.Property(p => p.ReservationStartDate).HasColumnName("RESERVATIONSTARTDATE");
                entity.Property(p => p.ReservationEndDate).HasColumnName("RESERVATIONENDDATE");
                entity.Property(p => p.PickupStartDate).HasColumnName("PICKUPSTARTDATE");
                entity.Property(p => p.PickupEndDate).HasColumnName("PICKUPENDDATE");
                entity.Property(p => p.EditDate).HasColumnName("EDITDATE");

                entity.Property(p => p.MinimumAmount).HasColumnName("MINIMUMAMOUNT").HasColumnType("DECIMAL(10,2)");
                entity.Property(p => p.MaximumAmount).HasColumnName("MAXIMUMAMOUNT").HasColumnType("DECIMAL(10,2)");
                entity.Property(p => p.MarkupValue).HasColumnName("MARKUPVALUE").HasColumnType("DECIMAL(10,2)");

                entity.Property(p => p.Name).HasColumnName("NAME");
            });

            modelBuilder.Entity<ProfitMarkupAgency>(entity =>
            {
                entity.ToTable("PROFITMARKUPAGENCIES");

                entity.HasKey(p => p.Id);

                entity.Property(p => p.Id).HasColumnName("ID").IsRequired().UseIdentityColumn();
                entity.Property(p => p.ProfitMarkupId).HasColumnName("PROFITMARKUPID").IsRequired();
                entity.Property(p => p.AgencyId).HasColumnName("AGENCYID").IsRequired();

                entity.HasOne(p => p.ProfitMarkup).WithMany(pm => pm.ProfitMarkupAgencies).HasForeignKey(p => p.ProfitMarkupId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(p => p.Agency).WithMany(a => a.ProfitMarkupAgencies).HasForeignKey(p => p.AgencyId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<ProfitMarkupLocation>(entity =>
            {
                entity.ToTable("PROFITMARKUPLOCATIONS");

                entity.HasKey(p => p.Id);

                entity.Property(p => p.Id).HasColumnName("ID").IsRequired().UseIdentityColumn();
                entity.Property(p => p.ProfitMarkupId).HasColumnName("PROFITMARKUPID").IsRequired();
                entity.Property(p => p.LocationId).HasColumnName("LOCATIONID").IsRequired();

                entity.HasOne(p => p.ProfitMarkup)
                      .WithMany(pm => pm.ProfitMarkupLocations)
                      .HasForeignKey(p => p.ProfitMarkupId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(p => p.Location)
                      .WithMany(l => l.ProfitMarkupLocations)
                      .HasForeignKey(p => p.LocationId)  // Composite key ilişkilendirme
                      .HasPrincipalKey(l => l.Id)  // Principal key ilişkilendirme
                      .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<ProfitMarkupVendor>(entity =>
            {
                entity.ToTable("PROFITMARKUPVENDORS", "dbo");

                entity.HasKey(p => p.Id);

                entity.Property(p => p.Id).HasColumnName("ID").IsRequired().UseIdentityColumn();
                entity.Property(p => p.ProfitMarkupId).HasColumnName("PROFITMARKUPID").IsRequired();
                entity.Property(p => p.VendorId).HasColumnName("VENDORID").IsRequired();

                entity.HasOne(p => p.ProfitMarkup).WithMany(pm => pm.ProfitMarkupVendors).HasForeignKey(p => p.ProfitMarkupId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(p => p.Vendor).WithMany(v => v.ProfitMarkupVendors).HasForeignKey(p => p.VendorId)
                    .OnDelete(DeleteBehavior.NoAction);
            });



            modelBuilder.Entity<Reservationcancellationfee>(entity =>
            {
                entity.ToTable("RESERVATIONCANCELLATIONFEE");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Maxrange).HasColumnName("MAXRANGE");

                entity.Property(e => e.Minrange).HasColumnName("MINRANGE");

                entity.Property(e => e.Pricepercentage).HasColumnName("PRICEPERCENTAGE");

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");
            });

            modelBuilder.Entity<Reservationextra>(entity =>
            {
                entity.ToTable("RESERVATIONEXTRA");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Amount)
                    .HasColumnName("AMOUNT")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Apiprice)
                    .HasColumnName("APIPRICE")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Date)
                    .HasColumnName("DATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Extraid).HasColumnName("EXTRAID");

                entity.Property(e => e.Extraname)
                    .HasColumnName("EXTRANAME")
                    .HasMaxLength(100);

                entity.Property(e => e.Extrarentaltype).HasColumnName("EXTRARENTALTYPE");

                entity.Property(e => e.Piece).HasColumnName("PIECE");

                entity.Property(e => e.Productcode)
                    .HasColumnName("PRODUCTCODE")
                    .HasMaxLength(50);

                entity.Property(e => e.Resid).HasColumnName("RESID");

                entity.Property(e => e.Resno)
                    .IsRequired()
                    .HasColumnName("RESNO")
                    .HasMaxLength(50);

                entity.Property(e => e.ExtraDescription).HasColumnName("EXTRADESCRIPTION");
            });

            modelBuilder.Entity<Reservationupdate>(entity =>
            {
                entity.ToTable("RESERVATIONUPDATE");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Additionalproductid).HasColumnName("ADDITIONALPRODUCTID");

                entity.Property(e => e.Newvalue).HasColumnName("NEWVALUE");

                entity.Property(e => e.Oldvalue).HasColumnName("OLDVALUE");

                entity.Property(e => e.Propertyid).HasColumnName("PROPERTYID");

                entity.Property(e => e.Reservatinonumber)
                    .IsRequired()
                    .HasColumnName("RESERVATINONUMBER")
                    .HasMaxLength(100);

                entity.Property(e => e.Reservationid).HasColumnName("RESERVATIONID");

                entity.Property(e => e.Uniqueid).HasColumnName("UNIQUEID");

                entity.Property(e => e.Updatedate)
                    .HasColumnName("UPDATEDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Userid).HasColumnName("USERID");

                entity.Property(e => e.Usernamesurname)
                    .IsRequired()
                    .HasColumnName("USERNAMESURNAME");
            });

            modelBuilder.Entity<Reservationupdateproperty>(entity =>
            {
                entity.ToTable("RESERVATIONUPDATEPROPERTY");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Property)
                    .IsRequired()
                    .HasColumnName("PROPERTY");

                entity.Property(e => e.Propertyname)
                    .IsRequired()
                    .HasColumnName("PROPERTYNAME");

                entity.Property(e => e.Propertytype).HasColumnName("PROPERTYTYPE");

                entity.Property(e => e.Sqlproperty)
                    .IsRequired()
                    .HasColumnName("SQLPROPERTY");
            });

            modelBuilder.Entity<Ressource>(entity =>
            {
                entity.ToTable("RESSOURCE");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AddDate)
                    .HasColumnName("ADD_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnName("IS_ACTIVE");

                entity.Property(e => e.Orders).HasColumnName("ORDERS");

                entity.Property(e => e.Sourcename)
                    .HasColumnName("SOURCENAME")
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Resstatushistory>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.Resno });

                entity.ToTable("RESSTATUSHISTORY");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Resno)
                    .HasColumnName("RESNO")
                    .HasMaxLength(50);

                entity.Property(e => e.Agencyid).HasColumnName("AGENCYID");

                entity.Property(e => e.Agencyname)
                    .HasColumnName("AGENCYNAME")
                    .HasMaxLength(150);

                entity.Property(e => e.Inserteddate)
                    .HasColumnName("INSERTEDDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Resid).HasColumnName("RESID");

                entity.Property(e => e.Resstatusid).HasColumnName("RESSTATUSID");

                entity.Property(e => e.Resstatusname)
                    .HasColumnName("RESSTATUSNAME")
                    .HasMaxLength(50);

                entity.Property(e => e.Resstatusnote)
                    .HasColumnName("RESSTATUSNOTE")
                    .HasMaxLength(500);

                entity.Property(e => e.Userid).HasColumnName("USERID");

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");

                entity.HasOne(p => p.Rez).WithMany(v => v.RezStatusHistories).HasForeignKey(p => p.Resid)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Resstatuslang>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("RESSTATUSLANG");

                entity.Property(e => e.Langid).HasColumnName("LANGID");

                entity.Property(e => e.Resstatustypeid).HasColumnName("RESSTATUSTYPEID");

                entity.Property(e => e.Statusname)
                    .HasColumnName("STATUSNAME")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Restoken>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.Uniqueid })
                    .HasName("PK_RESTOKEN_1");

                entity.ToTable("RESTOKEN");

                entity.HasIndex(e => e.Uniqueid)
                    .HasName("missing_index_76");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.SessionId)
                    .HasColumnName("SESSIONID");

                entity.Property(e => e.Uniqueid)
                    .HasColumnName("UNIQUEID")
                    .HasMaxLength(60)
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Recorddate)
                    .HasColumnName("RECORDDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasColumnName("TOKEN");
            });

            modelBuilder.Entity<Reswslog>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("RESWSLOG");

                entity.Property(e => e.Detay)
                    .HasColumnName("DETAY")
                    .HasColumnType("text");

                entity.Property(e => e.Metot)
                    .HasColumnName("METOT")
                    .HasColumnType("text");

                entity.Property(e => e.Tarih)
                    .HasColumnName("TARIH")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<Rez>(entity =>
            {
                entity.ToTable("REZ");

                entity.HasIndex(e => e.Rezno)
                    .HasName("IDX_REZ_REZNO");

                entity.Property(e => e.Rezid)
                    .HasColumnName("REZID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Acenteid).HasColumnName("ACENTEID");

                entity.Property(e => e.Additionalproductworkingtype)
                    .HasColumnName("ADDITIONALPRODUCTWORKINGTYPE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Advancedpaymentwithoutpayment)
                    .HasColumnName("ADVANCEDPAYMENTWITHOUTPAYMENT")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Agencycode).HasColumnName("AGENCYCODE");

                entity.Property(e => e.Agencycommission)
                    .HasColumnName("AGENCYCOMMISSION")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Agencyid).HasColumnName("AGENCYID");

                entity.Property(e => e.Agencyname)
                    .HasColumnName("AGENCYNAME")
                    .HasMaxLength(250);

                entity.Property(e => e.Agencyrentalprofitmarkup)
                    .HasColumnName("AGENCYRENTALPROFITMARKUP")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Agencyreservationreference).HasColumnName("AGENCYRESERVATIONREFERENCE");

                entity.Property(e => e.Alerterrorcode).HasColumnName("ALERTERRORCODE");

                entity.Property(e => e.Alistarihi)
                    .HasColumnName("ALISTARIHI")
                    .HasColumnType("datetime");

                entity.Property(e => e.Alisyeri)
                    .HasColumnName("ALISYERI")
                    .HasMaxLength(250);

                entity.Property(e => e.Alisyerid).HasColumnName("ALISYERID");

                entity.Property(e => e.Apicurrencyid).HasColumnName("APICURRENCYID");

                entity.Property(e => e.Apidailyprice)
                    .HasColumnName("APIDAILYPRICE")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Apiexchangerate)
                    .HasColumnName("APIEXCHANGERATE")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Apiextraamount)
                    .HasColumnName("APIEXTRAAMOUNT")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Apimessage).HasColumnName("APIMESSAGE");

                entity.Property(e => e.Apionewayfee)
                    .HasColumnName("APIONEWAYFEE")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Apipaidamount)
                    .HasColumnName("APIPAIDAMOUNT")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Apiphoneactive)
                    .HasColumnName("APIPHONEACTIVE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Apireferencecode).HasColumnName("APIREFERENCECODE");

                entity.Property(e => e.Apireferencecode2).HasColumnName("APIREFERENCECODE2");

                entity.Property(e => e.Apireferencecode3).HasColumnName("APIREFERENCECODE3");

                entity.Property(e => e.Apireservationcancel)
                    .HasColumnName("APIRESERVATIONCANCEL")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Apireservationnumber).HasColumnName("APIRESERVATIONNUMBER");

                entity.Property(e => e.Apireservationsuccessfully).HasColumnName("APIRESERVATIONSUCCESSFULLY");

                entity.Property(e => e.Apitotalamount)
                    .HasColumnName("APITOTALAMOUNT")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Apivendoraddress).HasColumnName("APIVENDORADDRESS");

                entity.Property(e => e.Apivendorname)
                    .HasColumnName("APIVENDORNAME")
                    .HasMaxLength(150);

                entity.Property(e => e.Apivendorphone).HasColumnName("APIVENDORPHONE");

                entity.Property(e => e.Apivendorreturnaddress).HasColumnName("APIVENDORRETURNADDRESS");

                entity.Property(e => e.Apivendorreturnphone).HasColumnName("APIVENDORRETURNPHONE");

                entity.Property(e => e.Aracadi)
                    .HasColumnName("ARACADI")
                    .HasMaxLength(250);

                entity.Property(e => e.Aracid).HasColumnName("ARACID");

                entity.Property(e => e.Baggagequantitytype).HasColumnName("BAGGAGEQUANTITYTYPE");

                entity.Property(e => e.Bankid).HasColumnName("BANKID");

                entity.Property(e => e.Baserequestcurrencyid).HasColumnName("BASEREQUESTCURRENCYID");

                entity.Property(e => e.Birakistarihi)
                    .HasColumnName("BIRAKISTARIHI")
                    .HasColumnType("datetime");

                entity.Property(e => e.Birakisyeri)
                    .HasColumnName("BIRAKISYERI")
                    .HasMaxLength(250);

                entity.Property(e => e.Birakisyerid).HasColumnName("BIRAKISYERID");

                entity.Property(e => e.Cancellationpenaltyhour)
                    .HasColumnName("CANCELLATIONPENALTYHOUR")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Cancellationpenaltyrate)
                    .HasColumnName("CANCELLATIONPENALTYRATE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Cancellationrefundedamount)
                    .HasColumnName("CANCELLATIONREFUNDEDAMOUNT")
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Commercialallowance).HasColumnName("COMMERCIALALLOWANCE");

                entity.Property(e => e.Couponcode).HasColumnName("COUPONCODE");

                entity.Property(e => e.Couponcurrencyid).HasColumnName("COUPONCURRENCYID");

                entity.Property(e => e.Coupondiscountamount)
                    .HasColumnName("COUPONDISCOUNTAMOUNT")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Coupondiscounttype).HasColumnName("COUPONDISCOUNTTYPE");

                entity.Property(e => e.Coupondiscountvalue)
                    .HasColumnName("COUPONDISCOUNTVALUE")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Couponid).HasColumnName("COUPONID");

                entity.Property(e => e.Creditcarddiscountpercent)
                    .HasColumnName("CREDITCARDDISCOUNTPERCENT")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Customerbirthday)
                    .HasColumnName("CUSTOMERBIRTHDAY")
                    .HasColumnType("datetime");

                entity.Property(e => e.Defaultcustomermailaddress).HasColumnName("DEFAULTCUSTOMERMAILADDRESS");

                entity.Property(e => e.Departureinfo)
                    .HasColumnName("DEPARTUREINFO")
                    .HasMaxLength(500);

                entity.Property(e => e.Depositcreditcardrequired)
                    .HasColumnName("DEPOSITCREDITCARDREQUIRED")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Depositprice)
                    .HasColumnName("DEPOSITPRICE")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Dilid).HasColumnName("DILID");

                entity.Property(e => e.Dovizid).HasColumnName("DOVIZID");

                entity.Property(e => e.Entegrasyonid).HasColumnName("ENTEGRASYONID");

                entity.Property(e => e.Exchangerate)
                    .HasColumnName("EXCHANGERATE")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Extraidlist)
                    .HasColumnName("EXTRAIDLIST")
                    .HasMaxLength(250);

                entity.Property(e => e.Extralar)
                    .HasColumnName("EXTRALAR")
                    .HasMaxLength(500);

                entity.Property(e => e.Extrapricepaytodelivery).HasColumnName("EXTRAPRICEPAYTODELIVERY");

                entity.Property(e => e.Extratutar)
                    .HasColumnName("EXTRATUTAR")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Fueltype).HasColumnName("FUELTYPE");

                entity.Property(e => e.Gunlukfiyat)
                    .HasColumnName("GUNLUKFIYAT")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Installmentcount)
                    .HasColumnName("INSTALLMENTCOUNT")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Ip)
                    .HasColumnName("IP")
                    .HasMaxLength(50);

                entity.Property(e => e.Iscancelable).HasColumnName("ISCANCELABLE");

                entity.Property(e => e.Isoffice)
                    .HasColumnName("ISOFFICE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Isspecialwebsiteagency)
                    .HasColumnName("ISSPECIALWEBSITEAGENCY")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Kaynak)
                    .HasColumnName("KAYNAK")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Kiralamasuresi).HasColumnName("KIRALAMASURESI");

                entity.Property(e => e.Kuponkod).HasColumnName("KUPONKOD");

                entity.Property(e => e.Logerrorcode).HasColumnName("LOGERRORCODE");

                entity.Property(e => e.Logresultnumber).HasColumnName("LOGRESULTNUMBER");

                entity.Property(e => e.Membername).HasColumnName("MEMBERNAME");

                entity.Property(e => e.Moneyforreservationscore).HasColumnName("MONEYFORRESERVATIONSCORE");

                entity.Property(e => e.Musteriaciklama)
                    .HasColumnName("MUSTERIACIKLAMA")
                    .HasMaxLength(500);

                entity.Property(e => e.Musteriad)
                    .HasColumnName("MUSTERIAD")
                    .HasMaxLength(250);

                entity.Property(e => e.Musteriadres)
                    .HasColumnName("MUSTERIADRES")
                    .HasMaxLength(500);

                entity.Property(e => e.Musteridonusucusno)
                    .HasColumnName("MUSTERIDONUSUCUSNO")
                    .HasMaxLength(50);

                entity.Property(e => e.Musteriebulten)
                    .IsRequired()
                    .HasColumnName("MUSTERIEBULTEN")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Musterieposta)
                    .HasColumnName("MUSTERIEPOSTA")
                    .HasMaxLength(250);

                entity.Property(e => e.Musterigelisucusno)
                    .HasColumnName("MUSTERIGELISUCUSNO")
                    .HasMaxLength(50);

                entity.Property(e => e.Musterinot)
                    .HasColumnName("MUSTERINOT")
                    .HasMaxLength(500);

                entity.Property(e => e.Musterisoyad)
                    .HasColumnName("MUSTERISOYAD")
                    .HasMaxLength(250);

                entity.Property(e => e.Musteritcpasaport)
                    .HasColumnName("MUSTERITCPASAPORT")
                    .HasMaxLength(100);

                entity.Property(e => e.Musteritelefon)
                    .HasColumnName("MUSTERITELEFON")
                    .HasMaxLength(250);

                entity.Property(e => e.Musteritip).HasColumnName("MUSTERITIP");

                entity.Property(e => e.Musteriunvan)
                    .HasColumnName("MUSTERIUNVAN")
                    .HasMaxLength(250);

                entity.Property(e => e.Musterivergidaire)
                    .HasColumnName("MUSTERIVERGIDAIRE")
                    .HasMaxLength(100);

                entity.Property(e => e.Musterivergino)
                    .HasColumnName("MUSTERIVERGINO")
                    .HasMaxLength(100);

                entity.Property(e => e.Odemebasarili).HasColumnName("ODEMEBASARILI");

                entity.Property(e => e.Odemehatamesaji)
                    .HasColumnName("ODEMEHATAMESAJI")
                    .HasColumnType("ntext");

                entity.Property(e => e.Odemekartno)
                    .HasColumnName("ODEMEKARTNO")
                    .HasMaxLength(50);

                entity.Property(e => e.Odemekartsahibi)
                    .HasColumnName("ODEMEKARTSAHIBI")
                    .HasMaxLength(250);

                entity.Property(e => e.Odenentutar)
                    .HasColumnName("ODENENTUTAR")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Onewayfeepaytodelivery).HasColumnName("ONEWAYFEEPAYTODELIVERY");

                entity.Property(e => e.Onewayfeeworkingtype)
                    .HasColumnName("ONEWAYFEEWORKINGTYPE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Parakod)
                    .HasColumnName("PARAKOD")
                    .HasMaxLength(3);

                entity.Property(e => e.Passangerquantitytype).HasColumnName("PASSANGERQUANTITYTYPE");

                entity.Property(e => e.Paymentamount)
                    .HasColumnName("PAYMENTAMOUNT")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Paymentrefundsuccess).HasColumnName("PAYMENTREFUNDSUCCESS");

                entity.Property(e => e.Paymentresultcode).HasColumnName("PAYMENTRESULTCODE");

                entity.Property(e => e.Paymentresultmessage).HasColumnName("PAYMENTRESULTMESSAGE");

                entity.Property(e => e.Paymenttype).HasColumnName("PAYMENTTYPE");

                entity.Property(e => e.Pdf)
                    .HasColumnName("PDF")
                    .HasMaxLength(500);

                entity.Property(e => e.Penaltyamount)
                    .HasColumnName("PENALTYAMOUNT")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Pesinataktif).HasColumnName("PESINATAKTIF");

                entity.Property(e => e.Profitmarkup)
                    .HasColumnName("PROFITMARKUP")
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Profitmarkupadditionalproducts)
                    .HasColumnName("PROFITMARKUPADDITIONALPRODUCTS")
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Profitmarkuponewayfee)
                    .HasColumnName("PROFITMARKUPONEWAYFEE")
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Provisionnumber).HasColumnName("PROVISIONNUMBER");

                entity.Property(e => e.Provizyonno)
                    .HasColumnName("PROVIZYONNO")
                    .HasMaxLength(250);

                entity.Property(e => e.Rentalworkingtype)
                    .HasColumnName("RENTALWORKINGTYPE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Reservationemailsendtoadminfromsourceapi).HasColumnName("RESERVATIONEMAILSENDTOADMINFROMSOURCEAPI");

                entity.Property(e => e.Reservationemailsendtocustomerfromsourceapi).HasColumnName("RESERVATIONEMAILSENDTOCUSTOMERFROMSOURCEAPI");

                entity.Property(e => e.Reservationscoreformoney)
                    .HasColumnName("RESERVATIONSCOREFORMONEY")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Reservationsmssendtocustomerfromsourceapi).HasColumnName("RESERVATIONSMSSENDTOCUSTOMERFROMSOURCEAPI");

                entity.Property(e => e.Reservationtoken).HasColumnName("RESERVATIONTOKEN");

                entity.Property(e => e.Reservationtokentext).HasColumnName("RESERVATIONTOKENTEXT");

                entity.Property(e => e.Resstatusid)
                    .HasColumnName("RESSTATUSID")
                    .HasDefaultValueSql("((-1))");

                entity.Property(e => e.Resstatusnote)
                    .HasColumnName("RESSTATUSNOTE")
                    .HasMaxLength(1000);

                entity.Property(e => e.Rezno)
                    .HasColumnName("REZNO")
                    .HasMaxLength(100);

                entity.Property(e => e.Score).HasColumnName("SCORE");

                entity.Property(e => e.Sendreservationmail)
                    .HasColumnName("SENDRESERVATIONMAIL")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Servicecharge)
                    .HasColumnName("SERVICECHARGE")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Servicechargecurrencyid).HasColumnName("SERVICECHARGECURRENCYID");

                entity.Property(e => e.Servisegonderildi).HasColumnName("SERVISEGONDERILDI");

                entity.Property(e => e.Sifreliveri)
                    .HasColumnName("SIFRELIVERI")
                    .HasMaxLength(250);

                entity.Property(e => e.Sippcode)
                    .HasColumnName("SIPPCODE")
                    .HasMaxLength(4);

                entity.Property(e => e.Skyscannerredirectid).HasColumnName("SKYSCANNERREDIRECTID");

                entity.Property(e => e.Sourceid).HasColumnName("SOURCEID");

                entity.Property(e => e.Specialdailyprice)
                    .HasColumnName("SPECIALDAILYPRICE")
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((-1))");

                entity.Property(e => e.Specialonewayfee)
                    .HasColumnName("SPECIALONEWAYFEE")
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((-1))");

                entity.Property(e => e.Statusname)
                    .HasColumnName("STATUSNAME")
                    .HasMaxLength(2500);

                entity.Property(e => e.Tarih)
                    .HasColumnName("TARIH")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Tedarikciid).HasColumnName("TEDARIKCIID");

                entity.Property(e => e.Tekyontutar)
                    .HasColumnName("TEKYONTUTAR")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Toplamtutar)
                    .HasColumnName("TOPLAMTUTAR")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Totalkmlimit).HasColumnName("TOTALKMLIMIT");

                entity.Property(e => e.Transmissiontype).HasColumnName("TRANSMISSIONTYPE");

                entity.Property(e => e.Updatedate)
                    .HasColumnName("UPDATEDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Useonlydefaultcurrency)
                    .HasColumnName("USEONLYDEFAULTCURRENCY")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Uyeid).HasColumnName("UYEID");

                entity.Property(e => e.Vehiclecategorytype).HasColumnName("VEHICLECATEGORYTYPE");

                entity.Property(e => e.Vehiclecode).HasColumnName("VEHICLECODE");

                entity.Property(e => e.Vehicleimageurl)
                    .HasColumnName("VEHICLEIMAGEURL")
                    .HasMaxLength(500);

                entity.Property(e => e.Vehicletype).HasColumnName("VEHICLETYPE");

                entity.Property(e => e.Vendoremail).HasColumnName("VENDOREMAIL");

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");

                entity.Property(e => e.Vendorlogo).HasColumnName("VENDORLOGO");

                entity.Property(e => e.Vendormindriverage).HasColumnName("VENDORMINDRIVERAGE");

                entity.Property(e => e.Vendormindrivinglicenseage).HasColumnName("VENDORMINDRIVINGLICENSEAGE");

                entity.Property(e => e.Vendorname)
                    .HasColumnName("VENDORNAME")
                    .HasMaxLength(150);

                entity.Property(e => e.Vendorphone).HasColumnName("VENDORPHONE");

                entity.Property(e => e.Vendortype).HasColumnName("VENDORTYPE");

                entity.Property(e => e.RefundAmount)
                    .HasColumnName("REFUNDAMOUNT")
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.FriendlyReservationNumber).HasColumnName("FRIENDLYRESERVATIONNUMBER");
                entity.Property(e => e.VendorDiscountValue).HasColumnName("VENDORDISCOUNTVALUE").HasColumnType("decimal(18,2)");


                entity.Property(e => e.ReturnInvoiceNumber).HasColumnName("RETURNINVOICENUMBER");
                entity.Property(e => e.AgencyReturnInvoiceNumber).HasColumnName("AGENCYRETURNINVOICENUMBER");

                entity.Property(e => e.CreditCardNumber).HasColumnName("CREDITCARDNUMBER");

                entity.Property(e => e.InstallmentCommissionAmount)
                                .HasColumnName("INSTALLMENTCOMMISIONAMOUNT")
                                .HasColumnType("decimal(18,2)")
                                .HasDefaultValueSql("((0))");

                entity.Property(e => e.CreditType)
                    .HasColumnName("CREDITTYPE")
                    .HasColumnType("smallint")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.CreditCardBank).HasColumnName("CREDITCARDBANK");
                entity.Property(e => e.CouponName).HasColumnName("COUPONNAME");


                entity.Property(e => e.PremiumExtraAmount).HasColumnName("PREMIUMEXTRAAMOUNT")
                                                          .HasColumnType("decimal(18,2)")
                                                          .HasDefaultValueSql("((0))");

                entity.Property(e => e.PaymentCode).HasColumnName("PAYMENTCODE");
                entity.Property(e => e.VendorFlightPassRequired).HasColumnName("FLIGHTPASSREQUIRED");
                entity.Property(e => e.ExternalCreditCardInfo).HasColumnName("EXTERNALCREDITCARDINFO");
                entity.Property(e => e.PickupOfficeWorkingHours).HasColumnName("PICKUPOFFICEWORKINGHOURS");
                entity.Property(e => e.ReturnOfficeWorkingHours).HasColumnName("RETURNOFFICEWORKINGHOURS");
                entity.Property(e => e.ApiDeliveryTypeId).HasColumnName("APIDELIVERYTYPEID");

            });

            modelBuilder.Entity<Scoreusagehistory>(entity =>
            {
                entity.ToTable("SCOREUSAGEHISTORY");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("ACTIVE")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Agencyid).HasColumnName("AGENCYID");

                entity.Property(e => e.Couponid).HasColumnName("COUPONID");

                entity.Property(e => e.Createdate)
                    .HasColumnName("CREATEDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Memberid).HasColumnName("MEMBERID");

                entity.Property(e => e.Moneyforreservationscore).HasColumnName("MONEYFORRESERVATIONSCORE");

                entity.Property(e => e.Reservationscoreformoney)
                    .HasColumnName("RESERVATIONSCOREFORMONEY")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Score).HasColumnName("SCORE");
            });

            modelBuilder.Entity<Smartvehiclesequence>(entity =>
            {
                entity.ToTable("SMARTVEHICLESEQUENCE");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasColumnName("ACTIVE");

                entity.Property(e => e.Createdate)
                    .HasColumnName("CREATEDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Enddate)
                    .HasColumnName("ENDDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Locationid).HasColumnName("LOCATIONID");

                entity.Property(e => e.Startdate)
                    .HasColumnName("STARTDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");

            });

            modelBuilder.Entity<SpecialProductPrice>(entity =>
            {
                entity.ToTable("SPECIALPRODUCTPRICE");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.AdditionalProductId).HasColumnName("ADDITIONALPRODUCTID");
                entity.Property(e => e.VendorId).HasColumnName("VENDORID");
                entity.Property(e => e.LocationId).HasColumnName("LOCATIONID");
                entity.Property(e => e.Price).HasColumnName("PRICE").HasColumnType("Number(18, 2)").HasDefaultValueSql("((0))");
                entity.Property(e => e.CurrencyId).HasColumnName("CURRENCYID");
            });

            modelBuilder.Entity<Staticlokasyon>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("STATICLOKASYON");

                entity.Property(e => e.Bolge)
                    .HasColumnName("BOLGE")
                    .HasMaxLength(250);

                entity.Property(e => e.Dilid).HasColumnName("DILID");

                entity.Property(e => e.Ekaraktif).HasColumnName("EKARAKTIF");

                entity.Property(e => e.Ekarid).HasColumnName("EKARID");

                entity.Property(e => e.Havalimani).HasColumnName("HAVALIMANI");

                entity.Property(e => e.Iata)
                    .HasColumnName("IATA")
                    .HasMaxLength(50);

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Mycaraktif).HasColumnName("MYCARAKTIF");

                entity.Property(e => e.Mycarid).HasColumnName("MYCARID");

                entity.Property(e => e.V3aktif).HasColumnName("V3AKTIF");

                entity.Property(e => e.V3id).HasColumnName("V3ID");
            });

            modelBuilder.Entity<Subvendor>(entity =>
            {
                entity.ToTable("SUBVENDOR");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("ACTIVE")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Apiprofitmarkup)
                    .HasColumnName("APIPROFITMARKUP")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Apiprofitmarkupadditionalproducts)
                    .HasColumnName("APIPROFITMARKUPADDITIONALPRODUCTS")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Apiprofitmarkuponewayfee)
                    .HasColumnName("APIPROFITMARKUPONEWAYFEE")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Currencyid).HasColumnName("CURRENCYID");

                entity.Property(e => e.Logo).HasColumnName("LOGO");

                entity.Property(e => e.Profitmarkup)
                    .HasColumnName("PROFITMARKUP")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Profitmarkupadditionalproducts)
                    .HasColumnName("PROFITMARKUPADDITIONALPRODUCTS")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Profitmarkuponewayfee)
                    .HasColumnName("PROFITMARKUPONEWAYFEE")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Subvendorid).HasColumnName("SUBVENDORID");

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");

                entity.Property(e => e.Vendorname)
                    .IsRequired()
                    .HasColumnName("VENDORNAME");

                entity.Property(e => e.Vendortype).HasColumnName("VENDORTYPE");
            });

            modelBuilder.Entity<Survey>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("SURVEY");

                entity.Property(e => e.Adminid).HasColumnName("ADMINID");

                entity.Property(e => e.Comment).HasColumnName("COMMENT");

                entity.Property(e => e.Email).HasColumnName("EMAIL");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Languageid).HasColumnName("LANGUAGEID");

                entity.Property(e => e.Reservationid).HasColumnName("RESERVATIONID");

                entity.Property(e => e.Senddate)
                    .HasColumnName("SENDDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Sendemail).HasColumnName("SENDEMAIL");

                entity.Property(e => e.Sendsms).HasColumnName("SENDSMS");

                entity.Property(e => e.Showonwebsite).HasColumnName("SHOWONWEBSITE");

                entity.Property(e => e.Surveyposttypeid).HasColumnName("SURVEYPOSTTYPEID");
            });

            modelBuilder.Entity<Surveyanswer>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("SURVEYANSWER");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Questionid).HasColumnName("QUESTIONID");

                entity.Property(e => e.Score).HasColumnName("SCORE");

                entity.Property(e => e.Surveyid).HasColumnName("SURVEYID");
            });

            modelBuilder.Entity<Surveyquestion>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.Languageid })
                    .HasName("PK__SURVEYQU__DE2261F371356842");

                entity.ToTable("SURVEYQUESTION");

                entity.Property(e => e.SurveyQuestionId).HasColumnName("SURVEYQUESTIONID");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Languageid).HasColumnName("LANGUAGEID");

                entity.Property(e => e.Question)
                    .IsRequired()
                    .HasColumnName("QUESTION");

                entity.Property(e => e.QuestionTitle)
                    .HasColumnName("QUESTIONTITLE");
            });

            modelBuilder.Entity<Surveystatus>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("SURVEYSTATUS");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Statusdate)
                    .HasColumnName("STATUSDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Surveyid).HasColumnName("SURVEYID");

                entity.Property(e => e.Surveystatustypeid).HasColumnName("SURVEYSTATUSTYPEID");
            });

            modelBuilder.Entity<Surveystatustype>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.Languageid })
                    .HasName("PK__SURVEYST__DE2261F3AFD33DD7");

                entity.ToTable("SURVEYSTATUSTYPE");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Languageid).HasColumnName("LANGUAGEID");

                entity.Property(e => e.Statusname)
                    .IsRequired()
                    .HasColumnName("STATUSNAME");
            });

            modelBuilder.Entity<Uye>(entity =>
            {
                entity.ToTable("UYE");

                entity.Property(e => e.Uyeid)
                    .HasColumnName("UYEID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Ad)
                    .HasColumnName("AD")
                    .HasMaxLength(250);

                entity.Property(e => e.Adres)
                    .HasColumnName("ADRES")
                    .HasMaxLength(500);

                entity.Property(e => e.Agencyid).HasColumnName("AGENCYID");

                entity.Property(e => e.Aktif).HasColumnName("AKTIF");

                entity.Property(e => e.Birthday).HasColumnName("BIRTHDAY");

                entity.Property(e => e.Commercialallowance).HasColumnName("COMMERCIALALLOWANCE");

                entity.Property(e => e.Defaultmember)
                    .HasColumnName("DEFAULTMEMBER")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Eposta)
                    .HasColumnName("EPOSTA")
                    .HasMaxLength(250);

                entity.Property(e => e.Ip)
                    .HasColumnName("IP")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Membertype)
                    .HasColumnName("MEMBERTYPE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Pwd)
                    .HasColumnName("PWD")
                    .HasMaxLength(250);

                entity.Property(e => e.Soyad)
                    .HasColumnName("SOYAD")
                    .HasMaxLength(250);

                entity.Property(e => e.Subagencyuser)
                    .HasColumnName("SUBAGENCYUSER")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Tcpasaport)
                    .HasColumnName("TCPASAPORT")
                    .HasMaxLength(100);

                entity.Property(e => e.Telefon)
                    .HasColumnName("TELEFON")
                    .HasMaxLength(250);

                entity.Property(e => e.Unvan)
                    .HasColumnName("UNVAN")
                    .HasMaxLength(250);

                entity.Property(e => e.Uyetip).HasColumnName("UYETIP");

                entity.Property(e => e.Uyetipmembertype)
                    .HasColumnName("UYETIPMEMBERTYPE")
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.Vergidaire)
                    .HasColumnName("VERGIDAIRE")
                    .HasMaxLength(100);

                entity.Property(e => e.Vergino)
                    .HasColumnName("VERGINO")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Vehiclebaggage>(entity =>
            {
                entity.HasKey(e => e.Baggageid);

                entity.ToTable("VEHICLEBAGGAGE");

                entity.Property(e => e.Baggageid).HasColumnName("BAGGAGEID");

                entity.Property(e => e.Active).HasColumnName("ACTIVE");

                entity.Property(e => e.Image).HasColumnName("IMAGE");

                entity.Property(e => e.Order).HasColumnName("_ORDER");
            });

            modelBuilder.Entity<Vehiclebaggagelang>(entity =>
            {
                entity.HasKey(e => new { e.Baggageid, e.Langid });

                entity.ToTable("VEHICLEBAGGAGELANG");

                entity.Property(e => e.Baggageid).HasColumnName("BAGGAGEID");

                entity.Property(e => e.Langid).HasColumnName("LANGID");

                entity.Property(e => e.Baggagename)
                    .IsRequired()
                    .HasColumnName("BAGGAGENAME");

                entity.HasOne(d => d.Baggage)
                    .WithMany(p => p.Vehiclebaggagelang)
                    .HasForeignKey(d => d.Baggageid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ARACBAGAJDIL_ARACBAGAJ");
            });

            modelBuilder.Entity<Vehiclebrand>(entity =>
            {
                entity.HasKey(e => e.Brandid);

                entity.ToTable("VEHICLEBRAND");

                entity.Property(e => e.Brandid).HasColumnName("BRANDID");

                entity.Property(e => e.Brandname)
                    .IsRequired()
                    .HasColumnName("BRANDNAME");

                entity.Property(e => e.Logo).HasColumnName("LOGO");
            });

            modelBuilder.Entity<Vehiclecategory>(entity =>
            {
                entity.HasKey(e => e.Categoryid);

                entity.ToTable("VEHICLECATEGORY");

                entity.Property(e => e.Categoryid).HasColumnName("CATEGORYID");

                entity.Property(e => e.Active).HasColumnName("ACTIVE");

                entity.Property(e => e.Order).HasColumnName("_ORDER");

                entity.Property(e => e.IconPath).HasColumnName("ICONPATH");
            });

            modelBuilder.Entity<Vehiclecategorylang>(entity =>
            {
                entity.HasKey(e => new { e.Langid, e.Categoryid });

                entity.ToTable("VEHICLECATEGORYLANG");

                entity.Property(e => e.Langid).HasColumnName("LANGID");

                entity.Property(e => e.Categoryid).HasColumnName("CATEGORYID");

                entity.Property(e => e.Categoryname)
                    .HasColumnName("CATEGORYNAME")
                    .HasMaxLength(50);

                entity.Property(e => e.CategoryImage).HasColumnName("CATEGORYIMAGE");
            });

            modelBuilder.Entity<Vehicleclass>(entity =>
            {
                entity.ToTable("VEHICLECLASS");

                entity.Property(e => e.Vehicleclassid).HasColumnName("VEHICLECLASSID");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("ACTIVE")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Aircondition)
                    .IsRequired()
                    .HasColumnName("AIRCONDITION")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Baggageid).HasColumnName("BAGGAGEID");

                entity.Property(e => e.Brandid).HasColumnName("BRANDID");

                entity.Property(e => e.Categoryid).HasColumnName("CATEGORYID");

                entity.Property(e => e.Depositamount)
                    .HasColumnName("DEPOSITAMOUNT")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Depositcurrencyid).HasColumnName("DEPOSITCURRENCYID");

                entity.Property(e => e.Depositdoublecreditcardrequiredactive)
                    .HasColumnName("DEPOSITDOUBLECREDITCARDREQUIREDACTIVE")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Fuelid).HasColumnName("FUELID");

                entity.Property(e => e.Imageurl).HasColumnName("IMAGEURL");

                entity.Property(e => e.Mindriverage).HasColumnName("MINDRIVERAGE");

                entity.Property(e => e.Mindrivinglicenseage).HasColumnName("MINDRIVINGLICENSEAGE");

                entity.Property(e => e.Modelid).HasColumnName("MODELID");

                entity.Property(e => e.Order).HasColumnName("_ORDER");

                entity.Property(e => e.Personid).HasColumnName("PERSONID");

                entity.Property(e => e.Recorddate)
                    .HasColumnName("RECORDDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Serialid).HasColumnName("SERIALID");

                entity.Property(e => e.Sippcode).HasColumnName("SIPPCODE");

                entity.Property(e => e.Transmissionid).HasColumnName("TRANSMISSIONID");

                entity.Property(e => e.Typeid).HasColumnName("TYPEID");

                entity.Property(e => e.Vehicleclasscode).HasColumnName("VEHICLECLASSCODE");

                entity.HasOne(v => v.VehicleBrand).WithMany(vb => vb.VehicleClasses).HasForeignKey(v => v.Brandid).OnDelete(DeleteBehavior.ClientSetNull);
                entity.HasOne(v => v.VehicleModel).WithMany(vm => vm.VehicleClasses).HasForeignKey(v => v.Modelid).OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Vehicleclasslang>(entity =>
            {
                entity.HasKey(e => new { e.Vehicleclassid, e.Langid });

                entity.ToTable("VEHICLECLASSLANG");

                entity.Property(e => e.Vehicleclassid).HasColumnName("VEHICLECLASSID");

                entity.Property(e => e.Langid).HasColumnName("LANGID");

                entity.Property(e => e.Description).HasColumnName("DESCRIPTION");

                entity.Property(e => e.Vehicleclassname)
                    .IsRequired()
                    .HasColumnName("VEHICLECLASSNAME");
            });

            modelBuilder.Entity<Vehicleclassvendor>(entity =>
            {
                entity.HasKey(e => new { e.Vehicleclassid, e.Vendorid });

                entity.ToTable("VEHICLECLASSVENDOR");

                entity.Property(e => e.Vehicleclassid).HasColumnName("VEHICLECLASSID");

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("ACTIVE")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Apivehicleclasscode)
                    .IsRequired()
                    .HasColumnName("APIVEHICLECLASSCODE");

                entity.Property(e => e.Apivehicleclassname)
                    .IsRequired()
                    .HasColumnName("APIVEHICLECLASSNAME");

                entity.Property(e => e.Dailykmlimit).HasColumnName("DAILYKMLIMIT");

                entity.Property(e => e.Deposit)
                    .HasColumnName("DEPOSIT")
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Maxkmlimit).HasColumnName("MAXKMLIMIT");

                entity.Property(e => e.Mindriverage).HasColumnName("MINDRIVERAGE");

                entity.Property(e => e.Mindrivinglicenseage).HasColumnName("MINDRIVINGLICENSEAGE");

                entity.Property(e => e.Recorddate)
                    .HasColumnName("RECORDDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Vehiclefuel>(entity =>
            {
                entity.HasKey(e => e.Fuelid);

                entity.ToTable("VEHICLEFUEL");

                entity.Property(e => e.Fuelid).HasColumnName("FUELID");

                entity.Property(e => e.Active).HasColumnName("ACTIVE");

                entity.Property(e => e.Order).HasColumnName("_ORDER");

                entity.Property(e => e.IconPath).HasColumnName("ICONPATH");
            });

            modelBuilder.Entity<Vehiclefuellang>(entity =>
            {
                entity.HasKey(e => new { e.Fuelid, e.Langid });

                entity.ToTable("VEHICLEFUELLANG");

                entity.Property(e => e.Fuelid).HasColumnName("FUELID");

                entity.Property(e => e.Langid).HasColumnName("LANGID");

                entity.Property(e => e.Fuelname)
                    .HasColumnName("FUELNAME")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<VehicleFilterMobile>(entity =>
            {
                entity.ToTable("VEHICLEFILTER");

                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.Key).HasColumnName("KEY");
                entity.Property(e => e.ParentKey).HasColumnName("PARENTKEY");
                entity.Property(e => e.Order).HasColumnName("ORDER");
                entity.Property(e => e.Header).HasColumnName("HEADER");
                entity.Property(e => e.Icon).HasColumnName("ICON");
                entity.Property(e => e.Value).HasColumnName("VALUE");
                entity.Property(e => e.Type).HasColumnName("TYPE");
                entity.Property(e => e.Name).HasColumnName("NAME");
                entity.Property(e => e.IsDefault).HasColumnName("ISDEFAULT");

            });

            modelBuilder.Entity<MobileAppVehicleBadge>(entity =>
            {
                entity.ToTable("MOBILEAPPVEHICLEBADGES");

                entity.HasKey(v => v.Id);
                entity.Property(v => v.Order).HasColumnName("ORDER");
                entity.Property(v => v.Text).HasColumnName("TEXT");
                entity.Property(v => v.TextColor).HasColumnName("TEXTCOLOR");
                entity.Property(v => v.BorderColor).HasColumnName("BORDERCOLOR");
                entity.Property(v => v.BackgroundColor).HasColumnName("BACKGROUNDCOLOR");
                entity.Property(v => v.ConditionId).HasColumnName("CONDITIONID");
                entity.Property(v => v.IconPath).HasColumnName("ICONATH");

            });

            modelBuilder.Entity<Vehiclemodel>(entity =>
            {
                entity.HasKey(e => e.Modelid);

                entity.ToTable("VEHICLEMODEL");

                entity.Property(e => e.Modelid).HasColumnName("MODELID");

                entity.Property(e => e.Brandid).HasColumnName("BRANDID");

                entity.Property(e => e.Modelname)
                    .IsRequired()
                    .HasColumnName("MODELNAME");
            });

            modelBuilder.Entity<Vehicleperson>(entity =>
            {
                entity.HasKey(e => e.Personid);

                entity.ToTable("VEHICLEPERSON");

                entity.Property(e => e.Personid).HasColumnName("PERSONID");

                entity.Property(e => e.Active).HasColumnName("ACTIVE");

                entity.Property(e => e.Order).HasColumnName("_ORDER");

                entity.Property(e => e.IconPath).HasColumnName("ICONPATH");
            });

            modelBuilder.Entity<Vehiclepersonlang>(entity =>
            {
                entity.HasKey(e => new { e.Personid, e.Langid });

                entity.ToTable("VEHICLEPERSONLANG");

                entity.Property(e => e.Personid).HasColumnName("PERSONID");

                entity.Property(e => e.Langid).HasColumnName("LANGID");

                entity.Property(e => e.Personname).HasColumnName("PERSONNAME");
            });

            modelBuilder.Entity<Vehicleresultstatistic>(entity =>
            {
                entity.ToTable("VEHICLERESULTSTATISTIC");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Agencyid).HasColumnName("AGENCYID");

                entity.Property(e => e.Apidailyprice)
                    .HasColumnName("APIDAILYPRICE")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Apivendorid).HasColumnName("APIVENDORID");

                entity.Property(e => e.Apivendorname).HasColumnName("APIVENDORNAME");

                entity.Property(e => e.Baggageid).HasColumnName("BAGGAGEID");

                entity.Property(e => e.Categoryid).HasColumnName("CATEGORYID");

                entity.Property(e => e.Currencyid).HasColumnName("CURRENCYID");

                entity.Property(e => e.Dailyprice)
                    .HasColumnName("DAILYPRICE")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Exchangerate)
                    .HasColumnName("EXCHANGERATE")
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Fuelid).HasColumnName("FUELID");

                entity.Property(e => e.Langid).HasColumnName("LANGID");

                entity.Property(e => e.Personid).HasColumnName("PERSONID");

                entity.Property(e => e.Pickupdate)
                    .HasColumnName("PICKUPDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Pickuplocationid).HasColumnName("PICKUPLOCATIONID");

                entity.Property(e => e.Recorddate)
                    .HasColumnName("RECORDDATE")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Rentalduration).HasColumnName("RENTALDURATION");

                entity.Property(e => e.Returndate)
                    .HasColumnName("RETURNDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Returnlocationid).HasColumnName("RETURNLOCATIONID");

                entity.Property(e => e.Transmissionid).HasColumnName("TRANSMISSIONID");

                entity.Property(e => e.Typeid).HasColumnName("TYPEID");

                entity.Property(e => e.Vehiclecode)
                    .IsRequired()
                    .HasColumnName("VEHICLECODE");

                entity.Property(e => e.Vehicleid).HasColumnName("VEHICLEID");

                entity.Property(e => e.Vehicleimageurl).HasColumnName("VEHICLEIMAGEURL");

                entity.Property(e => e.Vehiclename)
                    .IsRequired()
                    .HasColumnName("VEHICLENAME");

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");

                entity.Property(e => e.Vendorlogourl).HasColumnName("VENDORLOGOURL");

                entity.Property(e => e.Vendorname)
                    .IsRequired()
                    .HasColumnName("VENDORNAME");
            });

            modelBuilder.Entity<Vehicleseries>(entity =>
            {
                entity.HasKey(e => e.Serialid);

                entity.ToTable("VEHICLESERIES");

                entity.Property(e => e.Serialid).HasColumnName("SERIALID");

                entity.Property(e => e.Brandid).HasColumnName("BRANDID");

                entity.Property(e => e.Modelid).HasColumnName("MODELID");

                entity.Property(e => e.Serialname)
                    .IsRequired()
                    .HasColumnName("SERIALNAME");
            });

            modelBuilder.Entity<Vehicletransmission>(entity =>
            {
                entity.HasKey(e => e.Transmissionid);

                entity.ToTable("VEHICLETRANSMISSION");
                entity.Property(e => e.Transmissionid).HasColumnName("TRANSMISSIONID");
                entity.Property(e => e.Active).HasColumnName("ACTIVE");
                entity.Property(e => e.Order).HasColumnName("_ORDER");
                entity.Property(e => e.IconPath).HasColumnName("ICONPATH");
            });

            modelBuilder.Entity<Vehicletransmissionlang>(entity =>
            {
                entity.HasKey(e => new { e.Transmissionid, e.Langid });

                entity.ToTable("VEHICLETRANSMISSIONLANG");

                entity.Property(e => e.Transmissionid).HasColumnName("TRANSMISSIONID");
                entity.Property(e => e.Langid).HasColumnName("LANGID");
                entity.Property(e => e.Transmissionname)
                    .HasColumnName("TRANSMISSIONNAME")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Vehicletype>(entity =>
            {
                entity.HasKey(e => e.Typeid);

                entity.ToTable("VEHICLETYPE");

                entity.Property(e => e.Typeid).HasColumnName("TYPEID");
                entity.Property(e => e.Active).HasColumnName("ACTIVE");
                entity.Property(e => e.Order).HasColumnName("_ORDER");
                entity.Property(e => e.IconPath).HasColumnName("ICONPATH");
            });

            modelBuilder.Entity<Vehicletypelang>(entity =>
            {
                entity.HasKey(e => new { e.Typeid, e.Langid });

                entity.ToTable("VEHICLETYPELANG");

                entity.Property(e => e.Typeid).HasColumnName("TYPEID");
                entity.Property(e => e.Langid).HasColumnName("LANGID");
                entity.Property(e => e.Typename)
                    .HasColumnName("TYPENAME")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Vendor>(entity =>
            {
                entity.ToTable("VENDOR");

                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");
                entity.Property(e => e.Active).HasColumnName("ACTIVE").HasDefaultValueSql("((1))");
                entity.Property(e => e.Additionalproductworkingtype).HasColumnName("ADDITIONALPRODUCTWORKINGTYPE");
                entity.Property(e => e.Apibaseurl).HasColumnName("APIBASEURL");
                entity.Property(e => e.Apiclientid).HasColumnName("APICLIENTID");
                entity.Property(e => e.Apidescription).HasColumnName("APIDESCRIPTION");
                entity.Property(e => e.Apikey).HasColumnName("APIKEY").HasMaxLength(250);
                entity.Property(e => e.Apipassword).HasColumnName("APIPASSWORD").HasMaxLength(100);
                entity.Property(e => e.Apiphoneactive).HasColumnName("APIPHONEACTIVE").HasDefaultValueSql("((0))");
                entity.Property(e => e.Apitimeout).HasColumnName("APITIMEOUT");
                entity.Property(e => e.Availablecurrencies).HasColumnName("AVAILABLECURRENCIES");
                entity.Property(e => e.Companytitle).HasColumnName("COMPANYTITLE");
                entity.Property(e => e.Couponcodeactive).HasColumnName("COUPONCODEACTIVE").HasDefaultValueSql("((1))");
                entity.Property(e => e.Currencyid).HasColumnName("CURRENCYID");
                entity.Property(e => e.Depositcreditcardrequired).HasColumnName("DEPOSITCREDITCARDREQUIRED");
                entity.Property(e => e.Disabledeposit).HasColumnName("DISABLEDEPOSIT").HasDefaultValueSql("((0))");
                entity.Property(e => e.Documentrequiredshow).HasColumnName("DOCUMENTREQUIREDSHOW");
                entity.Property(e => e.Email).HasColumnName("EMAIL");
                entity.Property(e => e.Freecancellationhour).HasColumnName("FREECANCELLATIONHOUR").HasDefaultValueSql("((0))");
                entity.Property(e => e.Logo).HasColumnName("LOGO");
                entity.Property(e => e.Onewayfeeworkingtype).HasColumnName("ONEWAYFEEWORKINGTYPE");
                entity.Property(e => e.Personelnumberrequired).HasColumnName("PERSONELNUMBERREQUIRED").HasDefaultValueSql("((0))");
                entity.Property(e => e.Phone).HasColumnName("PHONE");
                entity.Property(e => e.Priceroundingtype).HasColumnName("PRICEROUNDINGTYPE");
                entity.Property(v => v.VendorOrder).HasColumnName("VENDORORDER").HasDefaultValue(0);
                entity.Property(e => e.Profitmarkup).HasColumnName("PROFITMARKUP").HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Profitmarkupadditionalproducts).HasColumnName("PROFITMARKUPADDITIONALPRODUCTS").HasColumnType("decimal(18, 2)").HasDefaultValueSql("((0))");
                entity.Property(e => e.Profitmarkupdailypriceactive).IsRequired().HasColumnName("PROFITMARKUPDAILYPRICEACTIVE").HasDefaultValueSql("((1))");
                entity.Property(e => e.Profitmarkupextraactive).IsRequired().HasColumnName("PROFITMARKUPEXTRAACTIVE").HasDefaultValueSql("((1))");
                entity.Property(e => e.Profitmarkuponewayfee).HasColumnName("PROFITMARKUPONEWAYFEE").HasColumnType("decimal(18, 2)").HasDefaultValueSql("((0))");
                entity.Property(e => e.Profitmarkuponewayfeeactive).IsRequired().HasColumnName("PROFITMARKUPONEWAYFEEACTIVE").HasDefaultValueSql("((1))");
                entity.Property(e => e.Rentalworkingtype).HasColumnName("RENTALWORKINGTYPE");
                entity.Property(e => e.Resagencynamesending).HasColumnName("RESAGENCYNAMESENDING");
                entity.Property(e => e.Reslistactive).HasColumnName("RESLISTACTIVE").HasDefaultValueSql("((1))");
                entity.Property(e => e.Secretkey).HasColumnName("SECRETKEY");
                entity.Property(e => e.Sellingbelowcostforcouponcode).HasColumnName("SELLINGBELOWCOSTFORCOUPONCODE").HasDefaultValueSql("((0))");
                entity.Property(e => e.Sendreservationmailtovendor).HasColumnName("SENDRESERVATIONMAILTOVENDOR").HasDefaultValueSql("((0))");
                entity.Property(e => e.Servicecharge).HasColumnName("SERVICECHARGE").HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Servicechargecurrencyid).HasColumnName("SERVICECHARGECURRENCYID");
                entity.Property(e => e.Showcustomernotearea).HasColumnName("SHOWCUSTOMERNOTEAREA").HasDefaultValueSql("((0))");
                entity.Property(e => e.Showflightnumberarea).HasColumnName("SHOWFLIGHTNUMBERAREA").HasDefaultValueSql("((0))");
                entity.Property(e => e.Usebrokerconfigurations).HasColumnName("USEBROKERCONFIGURATIONS").HasDefaultValueSql("((0))");
                entity.Property(e => e.Useonlydefaultcurrency).HasColumnName("USEONLYDEFAULTCURRENCY").HasDefaultValueSql("((0))");
                entity.Property(e => e.Vehiclemappingactive).HasColumnName("VEHICLEMAPPINGACTIVE").HasDefaultValueSql("((0))");
                entity.Property(e => e.Extramappingactive).HasColumnName("EXTRAMAPPINGACTIVE").HasDefaultValueSql("((0))");
                entity.Property(e => e.Vendorname).HasColumnName("VENDORNAME").HasMaxLength(150);
                entity.Property(e => e.Vendortype).HasColumnName("VENDORTYPE");
                entity.Property(e => e.CreditType)
                    .HasColumnName("CREDITTYPE")
                    .HasColumnType("smallint");
                entity.Property(e => e.BankName).HasColumnName("BANKNAME");
                entity.Property(e => e.BankBranchCode).HasColumnName("BANKBRANCHCODE");
                entity.Property(e => e.IBAN).HasColumnName("IBAN");
                entity.Property(e => e.AccountNumber).HasColumnName("ACCOUNTNUMBER");
                entity.Property(e => e.Country).HasColumnName("COUNTRY");
                entity.Property(e => e.City).HasColumnName("CITY");
                entity.Property(e => e.District).HasColumnName("DISTRICT");
                entity.Property(e => e.Address).HasColumnName("ADDRESS");
                entity.Property(e => e.InvoiceOwner).HasColumnName("INVOICEOWNER");
                entity.Property(e => e.PersonalNumber).HasColumnName("PERSONALNUMBER");
                entity.Property(e => e.TaxNumber).HasColumnName("TAXNUMBER");
                entity.Property(e => e.TaxOffice).HasColumnName("TAXOFFICE");
                entity.Property(e => e.Dbs).HasColumnName("DBS");
                entity.Property(e => e.VendorComissionInvoice).HasColumnName("VENDORCOMMISSIONINVOICE");
                entity.Property(e => e.IsPopular).HasColumnName("ISPOPULAR");
                entity.Property(e => e.VendorOrder).HasColumnName("VENDORORDER");
                entity.Property(e => e.FoundationYear).HasColumnName("FOUNDATIONYEAR");
                entity.Property(e => e.ShowSubVendorLogo).HasColumnName("SHOWSUBVENDORLOGO");
                entity.Property(e => e.AppearingProfitMarkup).HasColumnName("APPEARINGPROFITMARKUP").HasColumnType("decimal(18,2)"); ;
                entity.Property(e => e.FindeksRequired).HasColumnName("FINDEKSREQUIRED");
                entity.Property(e => e.BirthdayRequired).HasColumnName("BIRTHDAYREQUIRED");
                entity.Property(e => e.SendEmailToBranch).HasColumnName("SENDEMAILTOBRANCH");
                entity.Property(e => e.UseLocalDeposit).HasColumnName("USELOCALDEPOSIT");
                entity.Property(e => e.ToleranceTime).HasColumnName("TOLERANCETIME");
                entity.Property(e => e.FlightNumberRequired).HasColumnName("FLIGHTNUMBERREQUIRED");
                entity.Property(e => e.EarliestResTime).HasColumnName("EARLIESTRESTIME");
                entity.Property(e => e.CountryId).HasColumnName("COUNTRYID");
                entity.Property(e => e.SendAvailabilityRequest).HasColumnName("SENDAVAILABILITYREQUEST");
                entity.Property(e => e.ExtraDescriptionFromVendor).HasColumnName("EXTRADESCRIPTIONFROMVENDOR");
                entity.Property(e => e.SendDefaultMailAddress).HasColumnName("SENDDEFAULTMAILADDRESS");
                entity.Property(e => e.DeliveryTypeFromVendor).HasColumnName("DELIVERYTYPEFROMVENDOR");
                //entity.Property(e => e.HideLocationAddressOnPayment).HasColumnName("HIDELOCATIONADDRESSONPAYMENT");

            });

            modelBuilder.Entity<Vendorcontactinformation>(entity =>
            {
                entity.ToTable("VENDORCONTACTINFORMATION");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Address).HasColumnName("ADDRESS");
                entity.Property(e => e.Email).HasColumnName("EMAIL");
                entity.Property(e => e.Locationid).HasColumnName("LOCATIONID");
                entity.Property(e => e.Phonenumber).HasColumnName("PHONENUMBER");
                entity.Property(e => e.Vendorid).HasColumnName("VENDORID");
                entity.Property(e => e.Latitude).HasColumnName("LATITUDE");
                entity.Property(e => e.Longitude).HasColumnName("LONGITUDE");
                entity.Property(e => e.GoogleLink).HasColumnName("GoogleLink");
                entity.Property(e => e.ShortAddress).HasColumnName("SHORTADDRESS");
            });

            modelBuilder.Entity<Vendortype>(entity =>
            {
                entity.ToTable("VENDORTYPE");

                entity.Property(e => e.Vendortypeid)
                    .HasColumnName("VENDORTYPEID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Iscancelable).HasColumnName("ISCANCELABLE");

                entity.Property(e => e.Vendortypename)
                    .HasColumnName("VENDORTYPENAME")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<VendorVendor>(entity =>
            {
                entity.ToTable("VENDORVENDOR");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.VendorId).HasColumnName("VENDORID");
                entity.Property(e => e.VendorName).HasColumnName("VENDORNAME");
                entity.Property(e => e.Active).HasColumnName("ACTIVE").HasDefaultValueSql("((1))");
                entity.Property(e => e.MatchedVendorId).HasColumnName("MATCHEDVENDORID").HasDefaultValueSql("((0))");
                entity.Property(e => e.MatchedVendorName).HasColumnName("MATCHEDVENDORNAME").HasDefaultValueSql("(('Yok'))");
                entity.Property(e => e.UnlimitedKM).HasColumnName("UNLIMITEDKM").HasDefaultValueSql("((0))");
                entity.Property(e => e.FlightCardMandatory).HasColumnName("FLIGHTCARDMANDATORY").HasDefaultValueSql("((0))");
                entity.Property(e => e.PassportNumberRequired).HasColumnName("PASSPORTNUMBERREQUIRED");

                entity.Property(e => e.CreditType).HasColumnName("CREDITTYPE");
            });

            modelBuilder.Entity<Yonlendirme>(entity =>
            {
                entity.ToTable("YONLENDIRME");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Hedef)
                    .HasColumnName("HEDEF")
                    .HasMaxLength(500);

                entity.Property(e => e.Kaynak)
                    .HasColumnName("KAYNAK")
                    .HasMaxLength(500);
            });

            modelBuilder.Entity<VendorLocationFee>(entity =>
            {
                entity.ToTable("VENDORLOCATIONFEE");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");

            });

            modelBuilder.Entity<VendorLocationCondition>(entity =>
            {
                entity.ToTable("VENDORLOCATIONCONDITIONS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.VendorId).HasColumnName("VENDORID");
                entity.Property(e => e.LocationId).HasColumnName("LOCATIONID");
                entity.Property(e => e.Conditions).HasColumnName("CONDITIONS");
                entity.Property(e => e.LanguageId).HasColumnName("LANGUAGEID");

            });

            modelBuilder.Entity<VendorLocationDeliveryType>(entity =>
            {
                entity.ToTable("VENDORLOCATIONDELIVERYTYPES");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.VendorId).HasColumnName("VendorId");
                entity.Property(e => e.DeliveryTypeId).HasColumnName("DeliveryTypeId");
                entity.Property(e => e.LocationId).HasColumnName("LocationId");
            });


            modelBuilder.Entity<EmailLog>(entity =>
            {
                entity.ToTable("EMAILLOGS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.LogDate).HasColumnName("LogDate").HasColumnType("datetime").HasDefaultValueSql("(getdate())");
                entity.Property(e => e.IsSuccessful).HasColumnName("IsSuccessful");
                entity.Property(e => e.MailType).HasColumnName("MailType");
                entity.Property(e => e.SenderMail).HasColumnName("SenderMail");
                entity.Property(e => e.ReceiverMail).HasColumnName("ReceiverMail");
                entity.Property(e => e.MailBody).HasColumnName("MailBody");
                entity.Property(e => e.Message).HasColumnName("Message");
            });

            modelBuilder.Entity<CountryTaxRate>(entity =>
            {
                entity.ToTable("COUNTRYTAXRATE");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CountryId).HasColumnName("COUNTRYID");
                entity.Property(e => e.TaxRate).HasColumnName("TAXRATE");
            });

            modelBuilder.Entity<BultenLog>(entity =>
            {
                entity.ToTable("BULTENLOG");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Date).HasColumnName("DATE");
                entity.Property(e => e.Email).HasColumnName("EMAIL");
                entity.Property(e => e.Description).HasColumnName("DESCRIPTION");
                entity.Property(e => e.AgencyId).HasColumnName("AGENCYID");
                entity.Property(e => e.ContactPermission).HasColumnName("CONTACTPERMISSION");
                entity.Property(e => e.IP).HasColumnName("IP");
            });
            modelBuilder.Entity<SpecialRequest>(entity =>
            {
                entity.ToTable("SPECIALREQUEST");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).HasColumnName("ID");
                entity.Property(x => x.Active).HasColumnName("ACTIVE");
                entity.Property(x => x.Priority).HasColumnName("PRIORITY").IsRequired();
                entity.Property(x => x.SpecialRequestTariffId).HasColumnName("SPECIALREQUESTTARIFFID").IsRequired();
                entity.Property(x => x.AdditionalProductId).HasColumnName("ADDITIONALPRODUCTID").IsRequired();
                entity.Property(x => x.LastUpdateByUserId).HasColumnName("LASTUPDATEBYUSERID");
                entity.Property(x => x.Name).HasColumnName("NAME").IsRequired();
                entity.Property(x => x.ReservationStartDate).HasColumnName("RESERVATIONSTARTDATE");
                entity.Property(x => x.ReservationEndDate).HasColumnName("RESERVATIONENDDATE");
                entity.Property(x => x.PickupStartDate).HasColumnName("PICKUPSTARTDATE");
                entity.Property(x => x.PickupEndDate).HasColumnName("PICKUPENDDATE");
                entity.Property(x => x.EditDate).HasColumnName("EDITDATE");
                entity.Property(x => x.CreateDate).HasColumnName("CREATEDATE");
                entity.Property(x => x.CurrencyId).HasColumnName("CURRENCYID");
                entity.Property(x => x.MaximumAmount).HasColumnName("MAXIMUMAMOUNT").HasColumnType("decimal(10,2)");
                entity.Property(x => x.MaximumDay).HasColumnName("MAXIMUMDAY");
                entity.Property(x => x.MinimumAmount).HasColumnName("MINIMUMAMOUNT").HasColumnType("decimal(10,2)");
                entity.Property(x => x.MinimumDay).HasColumnName("MINIMUMDAY");

                entity.HasMany(x => x.Agencies)
                    .WithOne(x => x.SpecialRequest)
                    .HasForeignKey(x => x.SpecialRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(x => x.Locations)
                    .WithOne(x => x.SpecialRequest)
                    .HasForeignKey(x => x.SpecialRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(x => x.Vendors)
                    .WithOne(x => x.SpecialRequest)
                    .HasForeignKey(x => x.SpecialRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.SpecialRequestTariff)
                    .WithMany(x => x.SpecialRequests)
                    .HasForeignKey(x => x.SpecialRequestTariffId);

                entity.HasMany(x => x.Categories)
                .WithOne(x => x.SpecialRequest).HasForeignKey(x => x.SpecialRequestId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SpecialRequestAgency>(entity =>
            {
                entity.ToTable("SPECIALREQUESTAGENCY");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id).HasColumnName("ID");
                entity.Property(x => x.SpecialRequestId).HasColumnName("SPECIALREQUESTID").IsRequired();
                entity.Property(x => x.AgencyId).HasColumnName("AGENCYID").IsRequired();
            });

            modelBuilder.Entity<SpecialRequestLocation>(entity =>
            {
                entity.ToTable("SPECIALREQUESTLOCATION");

                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).HasColumnName("ID");
                entity.Property(x => x.SpecialRequestId).HasColumnName("SPECIALREQUESTID").IsRequired();
                entity.Property(x => x.LocationId).HasColumnName("LOCATIONID").IsRequired();
            });

            modelBuilder.Entity<SpecialRequestVendor>(entity =>
            {
                entity.ToTable("SPECIALREQUESTVENDOR");

                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).HasColumnName("ID");
                entity.Property(x => x.SpecialRequestId).HasColumnName("SPECIALREQUESTID").IsRequired();
                entity.Property(x => x.VendorId).HasColumnName("VENDORID").IsRequired();
            });

            modelBuilder.Entity<SpecialRequestCategory>(entity =>
            {
                entity.ToTable("SPECIALREQUESTCATEGORY");

                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).HasColumnName("ID");
                entity.Property(x => x.SpecialRequestId).HasColumnName("SPECIALREQUESTID").IsRequired();
                entity.Property(x => x.CategoryId).HasColumnName("CATEGORYID").IsRequired();
            });

            modelBuilder.Entity<SpecialRequestTariff>(entity =>
            {
                entity.ToTable("SPECIALREQUESTTARIFF");

                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).HasColumnName("ID");
                entity.Property(x => x.CurrencyId).HasColumnName("CURRENCYID").IsRequired();
                entity.Property(x => x.RentalTypeId).HasColumnName("RENTALTYPEID").IsRequired();
                entity.Property(x => x.Active).HasColumnName("ACTIVE").IsRequired();
                entity.Property(x => x.Code).HasColumnName("CODE");
                entity.Property(x => x.Description).HasColumnName("DESCRIPTION");
                entity.Property(x => x.Amount).HasColumnName("AMOUNT").HasColumnType("decimal(10,2)");
                entity.Property(x => x.MaxAmount).HasColumnName("MAXAMOUNT").HasColumnType("decimal(10,2)");
                entity.Property(x => x.LastUpdateByUser).HasColumnName("LASTUPDATEBYUSER").IsRequired();
                entity.Property(x => x.EditDate).HasColumnName("EDITDATE");

            });


            modelBuilder.Entity<BultenLog>(entity =>
            {
                entity.ToTable("BULTENLOG");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Date).HasColumnName("DATE");
                entity.Property(e => e.Email).HasColumnName("EMAIL");
                entity.Property(e => e.Description).HasColumnName("DESCRIPTION");
                entity.Property(e => e.AgencyId).HasColumnName("AGENCYID");
                entity.Property(e => e.ContactPermission).HasColumnName("CONTACTPERMISSION");
                entity.Property(e => e.IP).HasColumnName("IP");
            });
            #endregion

            #region Maps
            modelBuilder.ApplyConfiguration(new BankMap());
            modelBuilder.ApplyConfiguration(new BankBinCodeMap());
            modelBuilder.ApplyConfiguration(new BlockedMemberMap());
            modelBuilder.ApplyConfiguration(new BlockedDataMap());
            modelBuilder.ApplyConfiguration(new ExternalCommentMap());
            modelBuilder.ApplyConfiguration(new FindeksLogMap());
            modelBuilder.ApplyConfiguration(new LocationVendorInstallmentMap());
            modelBuilder.ApplyConfiguration(new MobileSortingParameterMap());
            modelBuilder.ApplyConfiguration(new MobileVehicleBadgeMap());
            modelBuilder.ApplyConfiguration(new MobileVehicleDetailMap());
            modelBuilder.ApplyConfiguration(new MobileVehicleFeatureMap());
            modelBuilder.ApplyConfiguration(new MobileVehicleListFastFilterMap());
            modelBuilder.ApplyConfiguration(new MobileVehicleListFilterMap());
            modelBuilder.ApplyConfiguration(new MobileVehiclePromotionMap());
            modelBuilder.ApplyConfiguration(new MobileVehicleSortingOptionMap());
            modelBuilder.ApplyConfiguration(new MobileAppSettingsMap());
            modelBuilder.ApplyConfiguration(new MobileFindeksUrlMap());
            modelBuilder.ApplyConfiguration(new InstallmentMap());
            modelBuilder.ApplyConfiguration(new PaymentSettingMap());
            modelBuilder.ApplyConfiguration(new Payment3dSecureMap());
            modelBuilder.ApplyConfiguration(new PaymentResultMap());
            modelBuilder.ApplyConfiguration(new ReservationDetailMap());
            modelBuilder.ApplyConfiguration(new ReservationDriverInfoMap());
            modelBuilder.ApplyConfiguration(new ReservationPaymentDetailMap());
            modelBuilder.ApplyConfiguration(new ReservationInvoiceAddressMap());
            modelBuilder.ApplyConfiguration(new ReservationSelectedExtraMap());
            modelBuilder.ApplyConfiguration(new ReservationVehicleInfoMap());
            modelBuilder.ApplyConfiguration(new VendorOfficeMap());
            #endregion

            #region Stored Procedure
            modelBuilder.Entity<VendorScoreDto>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.CurrentScore).HasColumnType("decimal(2,1)");
                entity.Property(e => e.Score).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Domain.Models.PaymentDto>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.Amount).HasColumnType("decimal(18,4)");
                entity.Property(e => e.RentAmount).HasColumnType("decimal(18,4)");
                entity.Property(e => e.RentAmountWithoutTax).HasColumnType("decimal(18,4)");
                entity.Property(e => e.DailyRentPrice).HasColumnType("decimal(18,4)");
                entity.Property(e => e.TaxRate).HasColumnType("decimal(4,4)");
                entity.Property(e => e.DailyRentPriceWithoutTax).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CancellationRefundAmount).HasColumnType("decimal(18,4)");
                entity.Property(e => e.ExtraAmount).HasColumnType("decimal(18,4)");
                entity.Property(e => e.ExtraAmountWithoutTax).HasColumnType("decimal(18,4)");
                entity.Property(e => e.DropAmount).HasColumnType("decimal(18,4)");
                entity.Property(e => e.DropAmountWithoutTax).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CouponDiscount).HasColumnType("decimal(18,4)");
                entity.Property(e => e.ExchangeRate).HasColumnType("decimal(10,2)");
                entity.Property(e => e.TLAmount).HasColumnType("decimal(18,4)");
                entity.Property(e => e.ProfitMarkupRental).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ProfitMatkupAdditionalProducts).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ProfitMatkupOneWay).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ServiceCharge).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<DeliveryTypeLanguage>(entity =>
            {
                entity.ToTable("DELIVERYTYPELANGUAGES", "dbo");
                entity.HasKey(dl => dl.Id);
                entity.HasIndex(dl => new { dl.DeliveryTypeId, dl.LanguageId });
                entity.Property(dl => dl.Id).HasColumnName("Id").IsRequired().UseIdentityColumn();
                entity.Property(dl => dl.DeliveryTypeId).HasColumnName("DeliveryTypeId").IsRequired();
                entity.Property(dl => dl.LanguageId).HasColumnName("LanguageId").IsRequired();
                entity.Property(dl => dl.Name).HasColumnName("Name").IsRequired();
            });

            modelBuilder.Entity<DataLayer>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<CouponDetailDto>().HasNoKey().ToView("CouponResultDto");
            modelBuilder.Entity<PopupContentDto>().HasNoKey().ToView("PopupContentDto");
            modelBuilder.Entity<BrokerLocationVehicleDetailDto>().HasNoKey().ToView("LocationVehicleDetailDto");
            modelBuilder.Entity<AgencyVendorDto>().HasNoKey().ToView("AgencyVendorDto");

            modelBuilder.Entity<ReservationReportModel>().HasNoKey().ToView("ReservationReportModel");
            modelBuilder.Entity<LogoReservation>().HasNoKey().ToView("LogoReservation");
            modelBuilder.Entity<CommentDto>().HasNoKey();
            modelBuilder.Entity<SearchLocationDto>().HasNoKey().ToView("SearchLocationDto");

            modelBuilder.Entity<ReservationFindeksDetail>(entity =>
            {
                entity.ToTable("RESERVATIONFINDEKSDETAIL", "dbo");

                entity.HasKey(r => r.Id);

                entity.Property(r => r.Id).HasColumnName("Id").IsRequired().UseIdentityColumn();
                entity.Property(r => r.IdentityNumber).HasColumnName("IdentityNumber").IsRequired();
                entity.Property(r => r.ReservationToken).HasColumnName("ReservationToken").IsRequired();
                entity.Property(r => r.VendorId).HasColumnName("VendorId").IsRequired();
                entity.Property(r => r.ReportDate).HasColumnName("ReportDate").IsRequired();
                entity.Property(r => r.BirthDate).HasColumnName("BirthDate").IsRequired();
                entity.Property(r => r.DriverLicenseDate).HasColumnName("DriverLicenseDate").IsRequired();
                entity.Property(r => r.RequestId).HasColumnName("RequestId");
                entity.Property(r => r.StepName).HasColumnName("StepName").IsRequired();
                entity.Property(r => r.IsSuccess).HasColumnName("IsSuccess").IsRequired();
                entity.Property(r => r.Message).HasColumnName("Message").IsRequired();
                entity.Property(r => r.PhoneNo).HasColumnName("PhoneNo");
                entity.Property(r => r.PhoneId).HasColumnName("PhoneId");
                entity.Property(r => r.isRequiredYoungDriverPacked).HasColumnName("isRequiredYoungDriverPacked").HasDefaultValue(false);
                entity.Property(r => r.isSuitableForCustomer).HasColumnName("isSuitableForCustomer").HasDefaultValue(false);
            });
            #endregion

            OnModelCreatingPartial(modelBuilder);

        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
