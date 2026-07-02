using kolayCAR.Broker.AWS.Helpers;
using kolayCAR.Broker.AWS.Logger.Telegram;
using kolayCAR.Broker.AWS.Services;
using KolayCAR.Broker.API.CompiledModels;
using KolayCAR.Broker.API.Creators;
using KolayCAR.Broker.API.Extensions;
using KolayCAR.Broker.API.Factories.Abstract;
using KolayCAR.Broker.API.Factories.Concrete;
using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Helpers.Swagger;
using KolayCAR.Broker.API.Helpers.Telegram;
using KolayCAR.Broker.API.Middleware;
using KolayCAR.Broker.API.Middleware.Extensions;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Repositories.Concrete;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.MSSqlServer;
using StackExchange.Redis;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.SustainedLowLatency;
Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.ViewSettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.AWSSettings.json", optional: true, reloadOnChange: true);

var cultureInfo = new CultureInfo("tr-TR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var appSettingsSection = builder.Configuration.GetSection("AppSettings");
var appSettings = appSettingsSection.Get<AppSettings>();
var dbPassword = appSettingsSection.GetValue<string>("DbPassword");

ValidateAppSettings(appSettingsSection, appSettings, dbPassword);

CacheSettings.Initialize(
    useCache: appSettings.UseCache,
    brokerName: appSettings.BrokerName
);

var connectionString = new DbConnectionHelper(builder.Configuration).ConnectionString;

builder.Services.AddCors();
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
var postmanEndpointFilter = PostmanEndpointFilter.Load("docRacapi.txt");
builder.Services.AddSwaggerGen(options =>
{
    options.DocInclusionPredicate((documentName, apiDescription) =>
        !string.IsNullOrWhiteSpace(apiDescription.HttpMethod) &&
        postmanEndpointFilter.ShouldInclude(apiDescription));
    options.ResolveConflictingActions(apiDescriptions => System.Linq.Enumerable.First(apiDescriptions));
    options.OperationFilter<PostmanOperationFilter>();
});

builder.Services.AddDbContextPool<BrokerContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(1000, TimeSpan.FromSeconds(5), null);
    });
    options.UseLazyLoadingProxies(false);
    options.UseModel(BrokerContextModel.Instance);
}, poolSize: 128);

builder.Services.AddDbContext<LoggingDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(1000, TimeSpan.FromSeconds(1), null);
    });
    options.UseLazyLoadingProxies(false);
});

builder.Services.Configure<AppSettings>(appSettingsSection);

// AddAutoMapper - using type of Program since Startup is gone
builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);



// ... inside builder ...
builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
    options.JsonSerializerOptions.AllowTrailingCommas = true;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IDBHelper>(new ADODBHelper(connectionString));

var key = Encoding.ASCII.GetBytes(appSettings.Secret);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddScoped<ILocationProviderFactory, LocationProviderFactory>();
builder.Services.AddScoped<IVehicleProviderFactory, VehicleProviderFactory>();
builder.Services.AddScoped<IExtraProviderFactory, ExtraProviderFactory>();
builder.Services.AddScoped<IReservationProviderFactory, ReservationProviderFactory>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IExtraService, ExtraService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<ISummaryService, SummaryService>();
builder.Services.AddScoped<IAgencyService, AgencyService>();
builder.Services.AddScoped<ISettingService, SettingService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();
builder.Services.AddScoped<IReservationStepsService, ReservationStepsService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<ILogoService, LogoService>();
builder.Services.AddScoped<ILogoServiceV2, LogoServiceV2>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IMobileService, MobileService>();
builder.Services.AddScoped<IFindeksService, FindeksService>();
builder.Services.AddScoped<IAgencyVendorService, AgencyVendorService>();
builder.Services.AddScoped<ITelegramBot>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    return IsTelegramEnabled(configuration)
        ? new TelegramBot(configuration, serviceProvider.GetRequiredService<IHttpContextAccessor>())
        : new DisabledTelegramBot();
});
builder.Services.AddScoped<ILabelService, LabelService>();
builder.Services.AddScoped<ISurveyService, SurveyService>();
builder.Services.AddScoped<IBlockedMemberService, BlockedMemberService>();
builder.Services.AddScoped<IPaymentSettingService, PaymentSettingService>();
builder.Services.AddScoped<IReservationTokenService, ReservationTokenService>();
builder.Services.AddScoped<IReservationDetailService, ReservationDetailService>();
builder.Services.AddScoped<IParameterService, ParameterService>();
builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IReservationPaymentDetailService, ReservationPaymentDetailService>();
builder.Services.AddScoped<IPaymentResultService, PaymentResultService>();
builder.Services.AddScoped<IBankBinCodeService, BankBinCodeService>();
builder.Services.AddScoped<ILocationVendorInstallmentService, LocationVendorInstallmentService>();
builder.Services.AddScoped<IBankService, BankService>();
builder.Services.AddScoped<IInstallmentService, InstallmentService>();
builder.Services.AddScoped<IVendorOfficeService, VendorOfficeService>();
builder.Services.AddScoped<IVendorLocationConditionsService, VendorLocationConditionsService>();
builder.Services.AddScoped<IMemoryCacheService, MemoryCacheService>();
builder.Services.AddScoped<IAWSService, AWSService>();
builder.Services.AddScoped<IProfitMarkupService, ProfitMarkupService>();
builder.Services.AddScoped<ILocationVendorClosedDateService, LocationVendorClosedDateService>();
builder.Services.AddScoped<IAgencyLocationService, AgencyLocationService>();
builder.Services.AddScoped<ILocationVendorService, LocationVendorService>();
builder.Services.AddScoped<ISubVendorService, SubVendorService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IRentalConditionService, RentalConditionService>();
builder.Services.AddScoped<IResTokenService, ResTokenService>();
builder.Services.AddScoped<IVendorVendorService, VendorVendorService>();
builder.Services.AddScoped<IBultenService, BultenService>();
builder.Services.AddScoped<IBultenLogService, BultenLogService>();
builder.Services.AddScoped<NetResysService, NetResysService>();
builder.Services.AddScoped<ISpecialRequestService, SpecialRequestService>();
builder.Services.AddScoped<ISpecialRequestAgencyService, SpecialRequestAgencyService>();
builder.Services.AddScoped<ISpecialRequestVendorService, SpecialRequestVendorService>();
builder.Services.AddScoped<ISpecialRequestLocationService, SpecialRequestLocationService>();
builder.Services.AddScoped<ISpecialRequestTariffService, SpecialRequestTariffService>();
builder.Services.AddScoped<ICurrentAccountService, CurrentAccountService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IVendorContactInformationService, VendorContactInformationService>();
builder.Services.AddScoped<IAdditionalProductService, AdditionalProductService>();

builder.Services.AddScoped<IAgencyLocationRepository, AgencyLocationRepository>();
builder.Services.AddScoped<IAgencyRepository, AgencyRepository>();
builder.Services.AddScoped<IAgencyVendorProfitMarkupRepository, AgencyVendorProfitMarkupRepository>();
builder.Services.AddScoped<IVendorContactInformationRepository, VendorContactInformationRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
builder.Services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<ILocationVendorClosedDateRepository, LocationVendorClosedDateRepository>();
builder.Services.AddScoped<ILocationVendorRepository, LocationVendorRepository>();
builder.Services.AddScoped<IResTokenRepository, ResTokenRepository>();
builder.Services.AddScoped<ISubVendorRepository, SubVendorRepository>();
builder.Services.AddScoped<IVendorRepository, VendorRepository>();
builder.Services.AddScoped<IVendorVendorRepository, VendorVendorRepository>();
builder.Services.AddScoped<IRentalConditionRepository, RentalConditionRepository>();
builder.Services.AddScoped<IBultenRepository, BultenRepository>();
builder.Services.AddScoped<IBultenLogRepository, BultenLogRepository>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<IVendorOfficeRepository, VendorOfficeRepository>();
builder.Services.AddScoped<ISpecialRequestRepository, SpecialRequestRepository>();
builder.Services.AddScoped<ISpecialRequestAgencyRepository, SpecialRequestAgencyRepository>();
builder.Services.AddScoped<ISpecialRequestVendorRepository, SpecialRequestVendorRepository>();
builder.Services.AddScoped<ISpecialRequestLocationRepository, SpecialRequestLocationRepository>();
builder.Services.AddScoped<ISpecialRequestTariffRepository, SpecialRequestTariffRepository>();
builder.Services.AddScoped<ICurrentAccountRepository, CurrentAccountRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IVendorLocationDeliveryTypeService, VendorLocationDeliveryTypeService>();
builder.Services.AddScoped<IVendorLocationDeliveryTypeRepository, VendorLocationDeliveryTypeRepository>();
builder.Services.AddScoped<IAdditionalProductRepository, AdditionalProductRepository>();

builder.Services.AddScoped<Creator>();
builder.Services.AddScoped<HttpContextHelper>();
builder.Services.AddScoped<AwsKinesisHelper>();
builder.Services.AddScoped<IAWSTelegramBot>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    return IsTelegramEnabled(configuration)
        ? new AWSTelegramBot(configuration, serviceProvider.GetRequiredService<IHttpContextAccessor>())
        : new DisabledAWSTelegramBot();
});
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromMinutes(15);
});

var cacheProvider = builder.Configuration["Cache:Provider"];

if (cacheProvider == "Garnet")
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration["Redis:ConnectionString"];
        options.InstanceName = builder.Configuration["Redis:InstanceName"];
    });

    builder.Services.AddSingleton<IConnectionMultiplexer>(
        ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"])
    );

    builder.Services.AddScoped<ICacheService, GarnetCacheService>();
}
else if (cacheProvider == "MultiInstance")
{
    builder.Services.AddScoped<ICacheService, MultiInstanceCacheService>();
}
else
{
    builder.Services.AddSingleton<ICacheService, CacheService>();
}

var logTable = !builder.Environment.IsDevelopment() ? appSettings.LogTable : "APILOG_DEV";
var defaultLogLevel = GetLogEventLevel(builder.Configuration["Serilog:MinimumLevel:Default"], LogEventLevel.Warning);
var microsoftLogLevel = GetLogEventLevel(builder.Configuration["Serilog:MinimumLevel:Override:Microsoft"], LogEventLevel.Warning);
var systemLogLevel = GetLogEventLevel(builder.Configuration["Serilog:MinimumLevel:Override:System"], LogEventLevel.Warning);

var columnOptions = new ColumnOptions
{
    AdditionalColumns = new Collection<SqlColumn>
    {
        new SqlColumn { ColumnName = "ActionName", DataType = SqlDbType.NVarChar },
        new SqlColumn { ColumnName = "RequestPath", DataType = SqlDbType.NVarChar },
        new SqlColumn { ColumnName = "RequestId", DataType = SqlDbType.NVarChar },
        new SqlColumn { ColumnName = "Environment", DataType = SqlDbType.NVarChar },
        new SqlColumn { ColumnName = "ClientIP", DataType = SqlDbType.NVarChar },
    }
};

columnOptions.Store.Remove(StandardColumn.Properties);
columnOptions.Store.Add(StandardColumn.LogEvent);

Serilog.Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(defaultLogLevel)
    .MinimumLevel.Override("Microsoft", microsoftLogLevel)
    .MinimumLevel.Override("System", systemLogLevel)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithExceptionDetails()
    .Enrich.WithProperty("BuildDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm"))
    .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console()
    .WriteTo.Async(a => a.MSSqlServer(
        connectionString: connectionString,
        sinkOptions: new MSSqlServerSinkOptions
        {
            TableName = logTable,
            AutoCreateSqlTable = true,
            BatchPostingLimit = 50,
            BatchPeriod = TimeSpan.FromSeconds(5)
        },
        columnOptions: columnOptions
    ))
    .CreateLogger();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddSerilog();
});


var app = builder.Build();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseSwagger();
app.UseSwaggerUI();
app.MapScalarApiReference("/scalar", options =>
{
    options
        .WithTitle("KolayCAR Broker API Docs")
        .WithTheme(ScalarTheme.DeepSpace)
        .WithLayout(ScalarLayout.Modern)
        .WithDarkMode(true)
        .WithTestRequestButton(true)
        .WithDownloadButton(true)
        .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json")
        .AddDocument("v1", "KolayCAR Broker API");
});
app.UseReDoc(options =>
{
    options.RoutePrefix = "redoc-raw";
    options.DocumentTitle = "KolayCAR Broker API Docs";
    options.SpecUrl("/swagger/v1/swagger.json");
});

app.Use(async (context, next) =>
{
    var requestPath = context.Request.Path.Value ?? string.Empty;
    var isDocsRoot = string.Equals(requestPath, "/docs", StringComparison.OrdinalIgnoreCase);
    var isPortalRedocPath = requestPath.StartsWith("/redoc", StringComparison.OrdinalIgnoreCase) &&
        !requestPath.StartsWith("/redoc-raw", StringComparison.OrdinalIgnoreCase);

    if (isDocsRoot || isPortalRedocPath)
    {
        context.Response.Redirect("/docs/");
        return;
    }

    if (string.Equals(requestPath, "/docs/", StringComparison.OrdinalIgnoreCase))
    {
        context.Request.Path = "/docs/index.html";
    }

    await next();
});

app.UseStaticFiles();

app.UseMiddleware<LogEnricherMiddleware>();

app.ExceptionMiddleware();
app.RequestHandleMiddleware();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();
app.UseVisitorSessionGenerateMiddleware();

app.MapControllers();

app.MapControllerRoute(
    name: "Default",
    pattern: "{controller=Home}/{action=Index}");

app.Run();

static void ValidateAppSettings(IConfigurationSection appSettingsSection, AppSettings appSettings, string dbPassword)
{
    if (!appSettingsSection.Exists() ||
        appSettings == null ||
        string.IsNullOrWhiteSpace(appSettings.Secret) ||
        string.IsNullOrWhiteSpace(appSettings.ConnectionString) ||
        string.IsNullOrWhiteSpace(appSettings.BrokerName) ||
        string.IsNullOrWhiteSpace(dbPassword))
    {
        throw new InvalidOperationException(
            "Required configuration is missing. Ensure appsettings.json is included in the publish output and AppSettings/DbPassword are populated.");
    }
}

static LogEventLevel GetLogEventLevel(string configuredValue, LogEventLevel fallbackLevel)
{
    return Enum.TryParse(configuredValue, ignoreCase: true, out LogEventLevel parsedLevel)
        ? parsedLevel
        : fallbackLevel;
}

static bool IsTelegramEnabled(IConfiguration configuration)
{
    var telegramBotId = configuration.GetSectionValueString("Telegram", "BotId").Decrypt();
    return !string.IsNullOrWhiteSpace(telegramBotId);
}



