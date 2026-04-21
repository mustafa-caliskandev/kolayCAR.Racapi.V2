using Amazon;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using Amazon.Runtime;
using kolayCAR.Broker.AWS.Extensions;
using kolayCAR.Broker.AWS.Logger.Telegram;
using kolayCAR.Broker.AWS.Models.ApiModels;
using kolayCAR.Broker.AWS.Models.AwsModels.Kinesis;
using KolayCAR.Broker.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace kolayCAR.Broker.AWS.Helpers
{
    public class AwsKinesisHelper
    {
        #region Constructor & Definations
        public static string UserSessionName { get; } = "user-code";

        public readonly IConfiguration _configuration;
        public readonly IHttpContextAccessor _httpContextAccessor;
        public readonly IAWSTelegramBot _awsTelegramBot;

        /// <summary>
        /// 
        /// </summary>
        public AwsKinesisHelper(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IAWSTelegramBot awsTelegramBot
            )
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _awsTelegramBot = awsTelegramBot;
        }
        #endregion

        #region Functions
        public async Task<bool> PutRecordAsync(
            string streamArn,
            string streamName,
            IKinesisModel jsonData)
        {
            var result = false;
            try
            {
                #region Test Kodu
                //var kinesisClient = new AmazonKinesisClient(credentials, RegionEndpoint.EUWest1);

                //var request = new PutRecordRequest
                //{
                //    StreamName = "RentaCarListing",
                //    Data = new MemoryStream(Encoding.UTF8.GetBytes(jsonData.ToJson())),
                //    PartitionKey = "url-response-times"
                //};

                //try
                //{
                //    var response = await kinesisClient.PutRecordAsync(request);
                //    Console.WriteLine($"Record sent. SequenceNumber: {response.SequenceNumber}");
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine($"Error: {ex.Message}");
                //} 
                #endregion

                string
                    accessKey = _configuration.GetSectionValueString("AWSKinesis", "AccessKey"),
                    secretKey = _configuration.GetSectionValueString("AWSKinesis", "SecretKey"),
                    regionName = _configuration.GetSectionValueString("AWSKinesis", "Region");
                var credentials = new BasicAWSCredentials(accessKey, secretKey);

                var jsonText = JsonConvert.SerializeObject(jsonData, settings: new JsonSerializerSettings
                {
                    FloatParseHandling = FloatParseHandling.Decimal,
                    Culture = CultureInfo.GetCultureInfo("en-EN"),
                    DateFormatString = "yyyy-MM-dd HH:mm:ss"
                });

                var dataAsBytes = Encoding.UTF8.GetBytes(jsonText);
                using var memoryStream = new MemoryStream(dataAsBytes);
                try
                {
                    var request = new PutRecordRequest
                    {
                        StreamName = streamName,
                        StreamARN = streamArn,
                        PartitionKey = "url-response-times",
                        Data = memoryStream,
                    };

                    var region = RegionEndpoint.GetBySystemName(regionName);
                    var kinesisClient = new AmazonKinesisClient(credentials, region);
                    var response = await kinesisClient.PutRecordAsync(request);
                    result = response.HttpStatusCode == HttpStatusCode.OK;
                    await ResultLogging(jsonData, response);
                }
                catch (Exception ex)
                {
                    await _awsTelegramBot.SendMessage($"AWS Put Records: Arn:{streamArn} \n {ex}", TelegramMessageType.Error);
                }
            }
            catch (Exception ex)
            {
                // TODO: LOGLAMA YAPILACAK!
                await _awsTelegramBot.SendMessage($"Amazon AWS Error! Arn:{streamArn} Error: {ex}", TelegramMessageType.Error);
            }
            return result;
        }

        public async Task ResultLogging(IKinesisModel jsonData, PutRecordResponse response)
        {
            var successStatus = response.HttpStatusCode == HttpStatusCode.OK ? "Başarılı" : "Hatalı";
            var postType = jsonData switch
            {
                ListingModel _ => "Listing",
                CheckoutModel _ => "Checkout",
                CancelModel _ => "Cancel",
                ErrorModel _ => "Error",
                _ => ""
            };

            if (jsonData is CheckoutModel)
            {
                var data = jsonData as CheckoutModel;
                postType = $"{postType} ({data.State})";
            }

            var message = $"(racapi) Type: {postType} Status: {successStatus}";
            await _awsTelegramBot.SendSpecialMessage(_configuration.GetSectionValueString("Telegram", "AWSBot").Decrypt(), message, jsonData.ToJson(), TelegramMessageType.Error);
        }

        public ListingModel CreateListingModel(
            List<Vehicle> vehicleList,
            VehicleFilterDto vehicleFilterDto,
            string deviceType)
        {
            var firstVehicleForInformation = vehicleList.FirstOrDefault();
            if (firstVehicleForInformation == null) return new ListingModel();

            var pickupDateTime = firstVehicleForInformation.PickupDateTime;
            var dropDateTime = firstVehicleForInformation.ReturnDateTime;

            var listModels = new ListingModel
            {
                User = string.Empty,
                Session = _httpContextAccessor?.HttpContext?.Session.GetString(UserSessionName) ?? "",
                Device = string.Empty,
                DeviceType = GetDeviceType(deviceType),
                Date = DateTime.Now,
                ApplicationEnv = GetEnvironment(),
                PickupDate = pickupDateTime,
                DropDate = dropDateTime,
                PickupPointId = firstVehicleForInformation.PickupLocationId,
                PickupPoint = firstVehicleForInformation.PickupLocationName,
                DropPointId = firstVehicleForInformation.ReturnLocationId,
                DropPoint = firstVehicleForInformation.ReturnLocationName,
                IsPickupAirport = false,
                IsDropAirport = false,
                IsPickupDropSame = firstVehicleForInformation.PickupLocationId == firstVehicleForInformation.ReturnLocationId,
                RentalPeriod = (dropDateTime - pickupDateTime).Days,
                IsFiltered = vehicleFilterDto != null,
                Filters = CreateFilterModel(vehicleFilterDto)
            };

            var listedVehicles = new List<CarModel>();
            foreach (var vehicle in vehicleList)
            {
                listedVehicles.Add(CreateCarModel(vehicle));
            }

            listModels.ListedCars = listedVehicles.ToArray();
            return listModels;
        }

        public string GetEnvironment()
        {
#if DEBUG
            return "Local";
#endif
            return _configuration.GetSectionValueBool("AWSKinesis", "IsProduction") ? "Production" : "Stage";
        }

        public CheckoutModel CreateCheckoutModel(KinesisCheckoutModel kinesisCheckoutModel)
        {
            var state = (int)kinesisCheckoutModel.Status;

            return new CheckoutModel
            {
                User = string.Empty, // Login yapısı olmadığı için boş bırakıldı
                Session = kinesisCheckoutModel.SessionCode,
                Device = string.Empty,
                DeviceType = GetDeviceType(kinesisCheckoutModel.DeviceType),
                Date = DateTime.Now,
                ApplicationEnv = GetEnvironment(),
                ReservationToken = kinesisCheckoutModel.ReservationToken,
                PickupDate = kinesisCheckoutModel.PickupDate,
                DropDate = kinesisCheckoutModel.ReturnDate,
                PickupPointId = kinesisCheckoutModel.PickupLocationId,
                PickupPoint = kinesisCheckoutModel.PickupLocationName,
                DropPointId = kinesisCheckoutModel.ReturnLocationId,
                DropPoint = kinesisCheckoutModel.ReturnLocationName,
                IsPickupAirport = kinesisCheckoutModel.IsPickupAirport,
                IsDropAirport = kinesisCheckoutModel.IsReturnAirport,
                IsPickupDropSame = kinesisCheckoutModel.PickupLocationId == kinesisCheckoutModel.ReturnLocationId,
                RentalPeriod = kinesisCheckoutModel.RentalDuration,
                Car = CreateCarModel(kinesisCheckoutModel),
                AdditionalProducts = ConvertExtrasToStringArray(kinesisCheckoutModel.AllExtras),
                State = state,
                OrderCode = kinesisCheckoutModel.PaymentCode,
                UserDetail = new UserDetailModel
                {
                    CustomerEmail = kinesisCheckoutModel.CustomerEmail,
                    CustomerPhone = kinesisCheckoutModel.CustomerPhone?.ReplaceIfExists(" ", "") ?? "",
                    PaymentMethod = kinesisCheckoutModel.PaymentMethod,
                    PaymentCard = kinesisCheckoutModel.PaymentCard,
                    PaymentType = kinesisCheckoutModel.PaymentType,
                    InstallmentCount = kinesisCheckoutModel.InstallmentCount,
                    LateCharge = kinesisCheckoutModel.LateCharge,
                    ContactPermission = kinesisCheckoutModel.ContactPermission
                },
                Coupons = new List<CouponModel>
                {
                    new CouponModel()
                    {
                        CouponCode = kinesisCheckoutModel.CouponCode,
                        CouponName = kinesisCheckoutModel.CouponName,
                        CouponAmount = kinesisCheckoutModel.CouponAmount.ToDecimal(),
                        CouponPaymentType = kinesisCheckoutModel.CouponPaymentType
                    }
                }.ToArray(),
                VendorCommission = kinesisCheckoutModel.VendorCommission,
                ObCommission = kinesisCheckoutModel.ObCommission,
            };
        }

        public CancelModel CreateCancelModel(KinesisCancelModel kinesisCancelModel)
        {
            var state = (int)kinesisCancelModel.Status;

            return new CancelModel()
            {
                User = string.Empty, // Login yapısı olmadığı için boş bırakıldı
                Session = _httpContextAccessor?.HttpContext?.Session.GetString(UserSessionName) ?? "",
                Device = string.Empty,
                DeviceType = GetDeviceType(kinesisCancelModel.DeviceType),
                Date = DateTime.Now,
                ApplicationEnv = GetEnvironment(),
                ReservationToken = kinesisCancelModel.ReservationToken,
                PickupDate = kinesisCancelModel.PickupDate,
                DropDate = kinesisCancelModel.ReturnDate,
                PickupPointId = kinesisCancelModel.PickupLocationId,
                PickupPoint = kinesisCancelModel.PickupLocationName,
                DropPointId = kinesisCancelModel.ReturnLocationId,
                DropPoint = kinesisCancelModel.ReturnLocationName,
                IsPickupAirport = kinesisCancelModel.IsPickupAirport,
                IsDropAirport = kinesisCancelModel.IsReturnAirport,
                IsPickupDropSame = kinesisCancelModel.PickupLocationId == kinesisCancelModel.ReturnLocationId,
                RentalPeriod = kinesisCancelModel.RentalDuration,
                Car = CreateCarModel(kinesisCancelModel),
                AdditionalProducts = ConvertExtrasToStringArray(kinesisCancelModel.AllExtras),
                State = state,
                OrderCode = kinesisCancelModel.PaymentCode,
                UserDetail = new UserDetailModel
                {
                    CustomerEmail = kinesisCancelModel.CustomerEmail,
                    CustomerPhone = kinesisCancelModel.CustomerPhone?.ReplaceIfExists(" ", "") ?? "",
                    PaymentMethod = kinesisCancelModel.PaymentMethod,
                    PaymentCard = kinesisCancelModel.PaymentCard,
                    PaymentType = kinesisCancelModel.PaymentType,
                    InstallmentCount = kinesisCancelModel.InstallmentCount,
                    LateCharge = kinesisCancelModel.LateCharge,
                    ContactPermission = kinesisCancelModel.ContactPermission
                },
                Coupons = new List<CouponModel>
                {
                    new CouponModel()
                    {
                        CouponCode = kinesisCancelModel.CouponCode,
                        CouponName = kinesisCancelModel.CouponName,
                        CouponAmount = kinesisCancelModel.CouponAmount.ToDecimal(),
                        CouponPaymentType = kinesisCancelModel.CouponPaymentType
                    }
                }.ToArray(),
                VendorCommission = kinesisCancelModel.VendorCommission,
                ObCommission = kinesisCancelModel.ObCommission,
                Channel = "Online",
                CancelReason = kinesisCancelModel.CancelReason
            };
        }

        public ErrorModel CreateErrorModel(KinesisErrorModel kinesisErrorModel)
        {
            var state = (int)kinesisErrorModel.Status;

            return new ErrorModel()
            {
                User = string.Empty, // Login yapısı olmadığı için boş bırakıldı
                Session = _httpContextAccessor?.HttpContext?.Session.GetString(UserSessionName) ?? "",
                Device = string.Empty,
                DeviceType = GetDeviceType(kinesisErrorModel.DeviceType),
                Date = DateTime.Now,
                ApplicationEnv = GetEnvironment(),
                ReservationToken = kinesisErrorModel.ReservationToken,
                PickupDate = kinesisErrorModel.PickupDate,
                DropDate = kinesisErrorModel.ReturnDate,
                PickupPointId = kinesisErrorModel.PickupLocationId,
                PickupPoint = kinesisErrorModel.PickupLocationName,
                DropPointId = kinesisErrorModel.ReturnLocationId,
                DropPoint = kinesisErrorModel.ReturnLocationName,
                IsPickupAirport = kinesisErrorModel.IsPickupAirport,
                IsDropAirport = kinesisErrorModel.IsReturnAirport,
                IsPickupDropSame = kinesisErrorModel.PickupLocationId == kinesisErrorModel.ReturnLocationId,
                RentalPeriod = kinesisErrorModel.RentalDuration,
                Car = CreateCarModel(kinesisErrorModel),
                AdditionalProducts = ConvertExtrasToStringArray(kinesisErrorModel.AllExtras),
                State = state,
                OrderCode = kinesisErrorModel.PaymentCode,
                UserDetail = new UserDetailModel
                {
                    CustomerEmail = kinesisErrorModel.CustomerEmail,
                    CustomerPhone = kinesisErrorModel.CustomerPhone?.ReplaceIfExists(" ", "") ?? "",
                    PaymentMethod = kinesisErrorModel.PaymentMethod,
                    PaymentCard = kinesisErrorModel.PaymentCard,
                    PaymentType = kinesisErrorModel.PaymentType,
                    InstallmentCount = kinesisErrorModel.InstallmentCount,
                    LateCharge = kinesisErrorModel.LateCharge,
                    ContactPermission = kinesisErrorModel.ContactPermission
                },
                Coupons = new List<CouponModel>
                {
                    new CouponModel()
                    {
                        CouponCode = kinesisErrorModel.CouponCode,
                        CouponName = kinesisErrorModel.CouponName,
                        CouponAmount = kinesisErrorModel.CouponAmount.ToDecimal(),
                        CouponPaymentType = kinesisErrorModel.CouponPaymentType
                    }
                }.ToArray(),
                VendorCommission = kinesisErrorModel.VendorCommission,
                ObCommission = kinesisErrorModel.ObCommission,
                Message = kinesisErrorModel.Message,
                SystemMessage = kinesisErrorModel.SystemMessage,
                ErrorStage = kinesisErrorModel.ErrorStage,
                ErrorType = kinesisErrorModel.ErrorType
            };
        }

        public FilterModel CreateFilterModel(VehicleFilterDto vehicleFilterDto)
        {
            // Sorting Types
            // 0 => Varsayılan sıralama
            // 1 => En düşük fiyat
            // 2 => En yüksek fiyat
            // 3 => Tedarikçi adı
            return vehicleFilterDto == null
                ? Activator.CreateInstance<FilterModel>()
                : new FilterModel
                {
                    SortType = vehicleFilterDto.SortType switch
                    {
                        0 => "Varsayılan sıralama",
                        1 => "En düşük fiyat",
                        2 => "En yüksek fiyat",
                        3 => "Tedarikçi adı",
                        _ => "Varsayılan sıralama"
                    },
                    Vendors = string.Join(',', vehicleFilterDto.VendorList),
                    VehicleCategories = string.Join(',', vehicleFilterDto.CategoryList),
                    VehicleModels = string.Join(',', vehicleFilterDto.ModelList),
                    Fuels = string.Join(',', vehicleFilterDto.FuelList),
                    Transmissions = string.Join(',', vehicleFilterDto.TransmissionList),
                    MinimumAges = string.Join(',', vehicleFilterDto.DriverAgeList),
                    MinimumDrivingLicenseYears = string.Join(',', vehicleFilterDto.LicenseYearList),
                    DeliveryTypes = string.Join(',', vehicleFilterDto.DeliveryTypeList),
                    MinPrice = $"{vehicleFilterDto.MinimumPrice}".ToInt(),
                    MaxPrice = $"{vehicleFilterDto.MaximumPrice}".ToInt(),
                    MinKm = $"{vehicleFilterDto.MinimumKm}".ToInt(),
                    MaxKm = $"{vehicleFilterDto.MaximumKm}".ToInt(),
                    MinimumDeposit = $"{vehicleFilterDto.MinimumDeposit}".ToInt(),
                    MaximumDeposit = $"{vehicleFilterDto.MaximumDeposit}".ToInt(),
                };
        }

        /// <summary>
        /// Browser/MobileBrowser/Android/iOS değerlerinden birisini dönmeli
        /// </summary>
        /// <returns></returns>
        public string GetDeviceType(string agencyType)
        {
            // TODO : Login bilgisine göre cihaz tanımını al
            var device = agencyType switch
            {
                "Web" => "Browser",
                "Android App" => "AndroidAppGSM",
                "iOS App" => "iOSApp",
                _ => "MobileBrowser"
            };
            return device;
        }

        public string[] ConvertExtrasToStringArray(string extrasJson)
        {
            var extras = Array.Empty<string>();

            try
            {
                if (!string.IsNullOrEmpty(extrasJson) && extrasJson.ValidateJson())
                {
                    var extraList = JsonConvert.DeserializeObject<List<AdditionalProductModel>>(extrasJson);
                    extras = extraList.Any()
                        ? extraList.Where(e => !string.IsNullOrEmpty(e.extraName)).OrderBy(e => e.extraName).Select(e => e.extraName).ToArray()
                        : Array.Empty<string>();
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return extras;
        }

        public int ConvertPassengerAndBaggageToInt(string name)
        {
            try
            {
                if (!string.IsNullOrEmpty(name))
                {
                    var splitName = name.Split(' ');
                    if (int.TryParse(splitName[0], out int pass))
                    {
                        return pass;
                    }
                }
            }
            catch (Exception)
            {
                // TODO : Loglama yap
            }
            return 0;
        }

        public CarModel CreateCarModel(Vehicle kinesisCheckoutModel)
        {
            return new CarModel
            {
                ReservationToken = kinesisCheckoutModel.ReservationToken,
                Brand = kinesisCheckoutModel.VehicleBrandName,
                Model = kinesisCheckoutModel.VehicleModelName,
                Supplier = kinesisCheckoutModel.VendorName,
                Group = kinesisCheckoutModel.VehicleCategoryTypeName,
                FuelType = kinesisCheckoutModel.FuelTypeName,
                TransmissionType = kinesisCheckoutModel.TransmissionTypeName,
                CancellationPolicy = "Free Cancel",
                VehicleType = kinesisCheckoutModel.VehicleTypeName,
                TotalKm = kinesisCheckoutModel.TotalKMLimit ?? 0,
                MinimumDriverAge = kinesisCheckoutModel.VendorMinimumDriverAge ?? 0,
                MinimumLicenseYear = kinesisCheckoutModel.VendorMinimumDrivingLicenseAge ?? 0,
                Deposit = kinesisCheckoutModel.DepositPrice ?? 0,
                DailyPrice = kinesisCheckoutModel.DailyPrice,
                TotalPrice = kinesisCheckoutModel.TotalPrice,
                DropPrice = kinesisCheckoutModel.OneWayFee,
                PeopleLuggage = new PeopleLuggage
                {
                    People = (int)kinesisCheckoutModel.PassangerQuantityType,
                    Luggage = (int)kinesisCheckoutModel.BaggageQuantityType
                }
            };
        }

        public CarModel CreateCarModel(KinesisCheckoutModel kinesisCheckoutModel)
        {
            return new CarModel
            {
                ReservationToken = kinesisCheckoutModel.ReservationToken,
                Brand = kinesisCheckoutModel.VehicleBrandName,
                Model = kinesisCheckoutModel.VehicleModelName,
                Supplier = kinesisCheckoutModel.VendorName,
                Group = kinesisCheckoutModel.VehicleCategoryTypeName,
                FuelType = kinesisCheckoutModel.FuelTypeName,
                TransmissionType = kinesisCheckoutModel.TransmissionTypeName,
                CancellationPolicy = "Free Cancel",
                VehicleType = kinesisCheckoutModel.VehicleTypeName,
                TotalKm = (int)(kinesisCheckoutModel.TotalKMLimit ?? 0),
                MinimumDriverAge = kinesisCheckoutModel.VendorMinimumDriverAge ?? 0,
                MinimumLicenseYear = kinesisCheckoutModel.VendorMinimumDrivingLicenseAge ?? 0,
                Deposit = kinesisCheckoutModel.DepositPrice ?? 0,
                DailyPrice = kinesisCheckoutModel.DailyPrice ?? 0,
                TotalPrice = kinesisCheckoutModel.TotalPrice ?? 0,
                DropPrice = kinesisCheckoutModel.OneWayFee ?? 0,
                PeopleLuggage = new PeopleLuggage
                {
                    People = ConvertPassengerAndBaggageToInt(kinesisCheckoutModel.PassangerQuantityTypeName),
                    Luggage = ConvertPassengerAndBaggageToInt(kinesisCheckoutModel.BaggageQuantityTypeName)
                }
            };
        }

        public CarModel CreateCarModel(KinesisCancelModel kinesisCheckoutModel)
        {
            return new CarModel
            {
                ReservationToken = kinesisCheckoutModel.ReservationToken,
                Brand = kinesisCheckoutModel.VehicleBrandName,
                Model = kinesisCheckoutModel.VehicleModelName,
                Supplier = kinesisCheckoutModel.VendorName,
                Group = kinesisCheckoutModel.VehicleCategoryTypeName,
                FuelType = kinesisCheckoutModel.FuelTypeName,
                TransmissionType = kinesisCheckoutModel.TransmissionTypeName,
                CancellationPolicy = "Free Cancel",
                VehicleType = kinesisCheckoutModel.VehicleTypeName,
                TotalKm = (int)(kinesisCheckoutModel.TotalKMLimit ?? 0),
                MinimumDriverAge = kinesisCheckoutModel.VendorMinimumDriverAge ?? 0,
                MinimumLicenseYear = kinesisCheckoutModel.VendorMinimumDrivingLicenseAge ?? 0,
                Deposit = kinesisCheckoutModel.DepositPrice ?? 0,
                DailyPrice = kinesisCheckoutModel.DailyPrice ?? 0,
                TotalPrice = kinesisCheckoutModel.TotalPrice ?? 0,
                DropPrice = kinesisCheckoutModel.OneWayFee ?? 0,
                PeopleLuggage = new PeopleLuggage
                {
                    People = ConvertPassengerAndBaggageToInt(kinesisCheckoutModel.PassangerQuantityTypeName),
                    Luggage = ConvertPassengerAndBaggageToInt(kinesisCheckoutModel.BaggageQuantityTypeName)
                }
            };
        }

        public CarModel CreateCarModel(KinesisErrorModel kinesisCheckoutModel)
        {
            return new CarModel
            {
                ReservationToken = kinesisCheckoutModel.ReservationToken,
                Brand = kinesisCheckoutModel.VehicleBrandName,
                Model = kinesisCheckoutModel.VehicleModelName,
                Supplier = kinesisCheckoutModel.VendorName,
                Group = kinesisCheckoutModel.VehicleCategoryTypeName,
                FuelType = kinesisCheckoutModel.FuelTypeName,
                TransmissionType = kinesisCheckoutModel.TransmissionTypeName,
                CancellationPolicy = "Free Cancel",
                VehicleType = kinesisCheckoutModel.VehicleTypeName,
                TotalKm = (int)(kinesisCheckoutModel.TotalKMLimit ?? 0),
                MinimumDriverAge = kinesisCheckoutModel.VendorMinimumDriverAge ?? 0,
                MinimumLicenseYear = kinesisCheckoutModel.VendorMinimumDrivingLicenseAge ?? 0,
                Deposit = kinesisCheckoutModel.DepositPrice ?? 0,
                DailyPrice = kinesisCheckoutModel.DailyPrice ?? 0,
                TotalPrice = kinesisCheckoutModel.TotalPrice ?? 0,
                DropPrice = kinesisCheckoutModel.OneWayFee ?? 0,
                PeopleLuggage = new PeopleLuggage
                {
                    People = ConvertPassengerAndBaggageToInt(kinesisCheckoutModel.PassangerQuantityTypeName),
                    Luggage = ConvertPassengerAndBaggageToInt(kinesisCheckoutModel.BaggageQuantityTypeName)
                }
            };
        }

        public decimal CalculateBrokerAllowance(
            decimal rentalAmount,
            int workingType,
            decimal profitMarkup)
        {
            return
                workingType == 0
                    ? Math.Round(rentalAmount * (profitMarkup / 100), 4)
                    : rentalAmount - Math.Round(((rentalAmount * 100) / (100 + profitMarkup)), 4);
        }
        #endregion
    }
}
