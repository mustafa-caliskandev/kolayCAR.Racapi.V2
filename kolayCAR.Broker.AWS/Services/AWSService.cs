using kolayCAR.Broker.AWS.Extensions;
using kolayCAR.Broker.AWS.Helpers;
using kolayCAR.Broker.AWS.Models.ApiModels;
using kolayCAR.Broker.AWS.Models.AwsModels.Kinesis;
using KolayCAR.Broker.Domain.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kolayCAR.Broker.AWS.Services
{
    public interface IAWSService
    {
        public Task<bool> PushListingData(List<Vehicle> vehicleList, VehicleFilterDto vehicleFilterDto, string deviceType);
        public Task<bool> PushCheckoutData(Vehicle vehicle, Vendor vendor, UserDetailModel userDetail, CouponModel couponDetail, string sessionId, string paymentCode, string postType, string deviceType);
        public Task<bool> PushCancelData(KinesisCancelModel kinesisCancelModel);
        public Task<bool> PushErrorData(Vehicle vehicle, Vendor vendor, UserDetailModel userDetail, CouponModel couponDetail, ErrorDto errorDto, string sessionId, string paymentCode);
    }

    public class AWSService : IAWSService
    {
        private readonly IConfiguration _configuration;

        private readonly AwsKinesisHelper _awsKinesisHelper;

        public AWSService(
            IConfiguration configuration,
            AwsKinesisHelper awsKinesisHelper)
        {
            _configuration = configuration;

            _awsKinesisHelper = awsKinesisHelper;
        }

        #region List data
        /// <summary>
        /// Araç listeleme sayfasındaki verileri gönderir
        /// </summary>
        /// <param name="searchDto">Arama listesi</param>
        /// <param name="vehicleList">Araç listesi</param>
        /// <param name="vehicleListFilterDto">Filtre listesi</param>
        /// <param name="deviceType">İstek yapan cihazın tipi</param>
        /// <returns></returns>
        public async Task<bool> PushListingData(
            List<Vehicle> vehicleList,
            VehicleFilterDto vehicleFilterDto,
            string deviceType)
        {
            if (!_configuration.GetSectionValueBool("AWSKinesis", "Active"))
            {
                return true;
            }

            string
                streamName = _configuration.GetSectionValueString("AWSKinesis", "ListingStreamName"),
                streamArn = _configuration.GetSectionValueString("AWSKinesis", "ListingArnUrl");
            var listingData = _awsKinesisHelper.CreateListingModel(vehicleList, vehicleFilterDto, deviceType);
            return await _awsKinesisHelper.PutRecordAsync(streamArn, streamName, listingData);
        }
        #endregion

        #region Checkout
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="vendor"></param>
        /// <param name="sessionId"></param>
        /// <param name="paymentCode"></param>
        /// <param name="postType"></param>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public async Task<bool> PushCheckoutData(
            Vehicle vehicle,
            Vendor vendor,
            UserDetailModel userDetail,
            CouponModel couponDetail,
            string sessionId,
            string paymentCode,
            string postType,
            string deviceType)
        {
            if (!_configuration.GetSectionValueBool("AWSKinesis", "Active"))
            {
                return true;
            }

            var kinesisCheckoutModel = new KinesisCheckoutModel();
            if (vehicle != null)
            {
                var extraVehicleDtoRentalPrice = (decimal)vehicle.DailyPrice * vehicle.RentalDuration;
                var obAllowance = _awsKinesisHelper.CalculateBrokerAllowance(extraVehicleDtoRentalPrice,
                    (int)vendor.RentalWorkingType, (decimal)vendor.ProfitMarkupDailyPrice);

                kinesisCheckoutModel.SessionCode = sessionId;
                kinesisCheckoutModel.Status = postType switch
                {
                    "ReservateNow" => KinesisStatus.CheckoutOpen,
                    "Get3d" => KinesisStatus.PaymentClicked,
                    "PaymentSuccess" => KinesisStatus.PaymentSuccess
                };
                kinesisCheckoutModel.ReservationToken = vehicle.ReservationToken;
                kinesisCheckoutModel.PaymentCode = paymentCode;

                kinesisCheckoutModel.PickupDate = vehicle.PickupDateTime;
                kinesisCheckoutModel.ReturnDate = vehicle.ReturnDateTime;
                kinesisCheckoutModel.PickupLocationId = vehicle.PickupLocationId;
                kinesisCheckoutModel.PickupLocationName = vehicle.PickupLocationName;
                kinesisCheckoutModel.ReturnLocationId = vehicle.ReturnLocationId;
                kinesisCheckoutModel.ReturnLocationName = vehicle.ReturnLocationName;
                kinesisCheckoutModel.RentalDuration = vehicle.RentalDuration;

                kinesisCheckoutModel.VendorName = vehicle.VendorName;

                kinesisCheckoutModel.VehicleBrandName = vehicle.VehicleBrandName;
                kinesisCheckoutModel.VehicleModelName = vehicle.VehicleModelName;
                kinesisCheckoutModel.VehicleCategoryTypeName = vehicle.VehicleCategoryTypeName;
                kinesisCheckoutModel.FuelTypeName = vehicle.FuelTypeName;
                kinesisCheckoutModel.TransmissionTypeName = vehicle.TransmissionTypeName;
                kinesisCheckoutModel.PassangerQuantityTypeName = vehicle.PassangerQuantityName;
                kinesisCheckoutModel.BaggageQuantityTypeName = vehicle.BaggageQuantityName;
                kinesisCheckoutModel.VehicleTypeName = vehicle.VehicleTypeName;
                kinesisCheckoutModel.DailyPrice = vehicle.DailyPrice;
                kinesisCheckoutModel.TotalPrice = (float)Math.Round((vehicle.DailyPrice * vehicle.RentalDuration), 2);
                kinesisCheckoutModel.DepositPrice = vehicle.DepositPrice;
                kinesisCheckoutModel.TotalKMLimit = vehicle.TotalKMLimit;
                kinesisCheckoutModel.OneWayFee = vehicle.OneWayFee;
                kinesisCheckoutModel.VendorMinimumDriverAge = vehicle.VendorMinimumDriverAge;
                kinesisCheckoutModel.VendorMinimumDrivingLicenseAge = vehicle.VendorMinimumDrivingLicenseAge;

                if (userDetail != null)
                {
                    kinesisCheckoutModel.CustomerEmail = userDetail.CustomerEmail;
                    kinesisCheckoutModel.CustomerPhone = userDetail.CustomerPhone;
                    kinesisCheckoutModel.PaymentMethod = userDetail.PaymentMethod;
                    kinesisCheckoutModel.PaymentCard = userDetail.PaymentCard;
                    kinesisCheckoutModel.PaymentType = userDetail.PaymentType;
                    kinesisCheckoutModel.InstallmentCount = userDetail.InstallmentCount;
                    kinesisCheckoutModel.LateCharge = userDetail.LateCharge;
                    kinesisCheckoutModel.ContactPermission = userDetail.ContactPermission;
                }

                if (couponDetail is { } && couponDetail.CouponCode != string.Empty)
                {
                    kinesisCheckoutModel.CouponCode = couponDetail.CouponCode;
                    kinesisCheckoutModel.CouponName = couponDetail.CouponName;
                    kinesisCheckoutModel.CouponAmount = $"{couponDetail.CouponAmount}";
                    kinesisCheckoutModel.CouponPaymentType = couponDetail.CouponPaymentType;
                }

                kinesisCheckoutModel.VendorCommission = extraVehicleDtoRentalPrice - obAllowance;
                kinesisCheckoutModel.ObCommission = obAllowance;
                kinesisCheckoutModel.DeviceType = deviceType;
            }

            string
                streamName = _configuration.GetSectionValueString("AWSKinesis", "CheckoutStreamName"),
                streamArn = _configuration.GetSectionValueString("AWSKinesis", "CheckoutArnUrl");
            var checkoutModel = _awsKinesisHelper.CreateCheckoutModel(kinesisCheckoutModel);
            return await _awsKinesisHelper.PutRecordAsync(streamArn, streamName, checkoutModel);
        }
        #endregion

        #region Cancel
        /// <summary>
        /// 
        /// </summary>
        /// <param name="kinesisCancelModel"></param>
        /// <returns></returns>
        public async Task<bool> PushCancelData(KinesisCancelModel kinesisCancelModel)
        {
            if (!_configuration.GetSectionValueBool("AWSKinesis", "Active"))
            {
                return true;
            }

            string
                streamName = _configuration.GetSectionValueString("AWSKinesis", "CancelStreamName"),
                streamArn = _configuration.GetSectionValueString("AWSKinesis", "CancelArnUrl");
            var cancelModel = _awsKinesisHelper.CreateCancelModel(kinesisCancelModel);
            return await _awsKinesisHelper.PutRecordAsync(streamArn, streamName, cancelModel);
        }
        #endregion

        #region Error
        /// <summary>
        /// 
        /// </summary>
        /// <param name="kinesisErrorModel"></param>
        /// <returns></returns>
        public async Task<bool> PushErrorData(Vehicle vehicle, Vendor vendor, UserDetailModel userDetail, CouponModel couponDetail, ErrorDto errorDto, string sessionId, string paymentCode)
        {
            if (!_configuration.GetSectionValueBool("AWSKinesis", "Active"))
            {
                return true;
            }

            var extraVehicleDtoRentalPrice = (decimal)vehicle.DailyPrice * vehicle.RentalDuration;
            var obAllowance = _awsKinesisHelper.CalculateBrokerAllowance(extraVehicleDtoRentalPrice,
                (int)vendor.RentalWorkingType, (decimal)vendor.ProfitMarkupDailyPrice);

            var kinesisErrorModel = new KinesisErrorModel
            {
                ReservationToken = vehicle.ReservationToken,
                PaymentCode = paymentCode,

                PickupDate = vehicle.PickupDateTime,
                ReturnDate = vehicle.ReturnDateTime,
                PickupLocationId = vehicle.PickupLocationId,
                PickupLocationName = vehicle.PickupLocationName,
                ReturnLocationId = vehicle.ReturnLocationId,
                ReturnLocationName = vehicle.ReturnLocationName,
                RentalDuration = vehicle.RentalDuration,

                VendorName = vendor.VendorName,

                VehicleBrandName = vehicle.VehicleBrandName,
                VehicleModelName = vehicle.VehicleModelName,
                VehicleCategoryTypeName = vehicle.VehicleCategoryTypeName,
                FuelTypeName = vehicle.FuelTypeName,
                TransmissionTypeName = vehicle.TransmissionTypeName,
                PassangerQuantityTypeName = vehicle.PassangerQuantityName,
                BaggageQuantityTypeName = vehicle.BaggageQuantityName,
                VehicleTypeName = vehicle.VehicleTypeName,
                DailyPrice = vehicle.DailyPrice,
                TotalPrice = (float)Math.Round((vehicle.DailyPrice * vehicle.RentalDuration), 2),
                DepositPrice = vehicle.DepositPrice,
                TotalKMLimit = vehicle.TotalKMLimit,
                OneWayFee = vehicle.OneWayFee,
                VendorMinimumDriverAge = vehicle.VendorMinimumDriverAge,
                VendorMinimumDrivingLicenseAge = vehicle.VendorMinimumDrivingLicenseAge,

                AllExtras = vehicle.Extras.ToJson(),
                SelectedExtras = vehicle.Extras.ToJson(),

                CustomerEmail = userDetail.CustomerEmail,
                CustomerPhone = userDetail.CustomerPhone,
                ContactPermission = userDetail.ContactPermission,

                VendorCommission = extraVehicleDtoRentalPrice - obAllowance,
                ObCommission = obAllowance,

                CouponCode = string.IsNullOrEmpty(couponDetail.CouponCode) ? null : couponDetail.CouponCode,
                CouponName = null,
                CouponAmount = $"{couponDetail.CouponAmount}",
                CouponPaymentType = "ObiletNonRefundable",

                PaymentMethod = "Masterpass Ödeme Sistemi",
                PaymentCard = string.IsNullOrWhiteSpace(userDetail.PaymentCard) ? "" : $"{userDetail.PaymentCard[..6]}*****{userDetail.PaymentCard[^4..]}",
                PaymentType = "cash",
                InstallmentCount = userDetail.InstallmentCount,
                LateCharge = userDetail.LateCharge,

                Message = errorDto.Message,
                SystemMessage = errorDto.SystemMessage,
                ErrorStage = errorDto.ErrorStage.ToInt(),
                ErrorType = errorDto.ErrorType
            };

            string
                streamName = _configuration.GetSectionValueString("AWSKinesis", "ErrorStreamName"),
                streamArn = _configuration.GetSectionValueString("AWSKinesis", "ErrorArnUrl");
            var errorModel = _awsKinesisHelper.CreateErrorModel(kinesisErrorModel);
            return await _awsKinesisHelper.PutRecordAsync(streamArn, streamName, errorModel);
        }
        #endregion
    }
}
