using KolayCAR.Broker.Domain.Models;
using System;
using System.Collections.Generic;
using KolayCARResponse = KolayCAR.Broker.Domain.Models.Response;

namespace KolayCAR.Broker.Domain.Mappers.KolayCAR
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this KolayCARResponse.VEHICLE vehicle) =>
            vehicle != null ? new Vehicle
            {
                VehicleId = vehicle.VEHICLEID,
                VendorId = vehicle.VENDORID,
                VendorName = vehicle.VENDORNAME,
                VendorVehicleName = vehicle.VENDORVEHICLENAME,
                VendorLogo = vehicle.VENDORLOGO,
                VehicleName = vehicle.VEHICLENAME,
                VehicleDescription = vehicle.VEHICLEDESCRIPTION,
                SippCode = vehicle.SIPPCODE,
                PickupLocationId = vehicle.PICKUPLOCATIONID,
                PickupLocationName = vehicle.PICKUPLOCATIONNAME,
                ReturnLocationId = vehicle.RETURNLOCATIONID,
                ReturnLocationName = vehicle.RETURNLOCATIONNAME,
                VendorPickupLocationId = vehicle.VENDORPICKUPLOCATIONID,
                VendorPickupLocationName = vehicle.VENDORPICKUPLOCATIONNAME,
                VendorReturnLocationId = vehicle.VENDORPICKUPLOCATIONID,
                VendorReturnLocationName = vehicle.VENDORPICKUPLOCATIONNAME,
                PickupDateTime = vehicle.PICKUPDATETIME,
                ReturnDateTime = vehicle.RETURNDATETIME,
                MinimumRentalDurationDays = vehicle.MINIMUMRENTALDURATIONDAYS,
                RentalDuration = vehicle.RENTALDURATION,
                DailyPrice = (float)Math.Round(vehicle.DAILYPRICE, 2),
                OneWayFee = (float)Math.Round(vehicle.ONEWAYFEE, 2),
                ExtraPrice = (float)Math.Round(vehicle.EXTRAPRICE, 2),
                TotalPrice = (float)Math.Round(vehicle.TOTALPRICE, 2),
                IsAvailable = vehicle.ISAVAILABLE == 1,
                RentalConditions = vehicle.RENTALCONDITIONS.Map(),
                VehicleImages = vehicle.VEHICLEIMAGES.Map(),
                VehicleType = (VehicleTypes)(Convert.ToInt32(vehicle.VEHICLETYPEID) - 1),
                VehicleTypeName = vehicle.VEHICLETYPE,
                TransmissionType = (TransmissionTypes)(Convert.ToInt32(vehicle.TRANSMISSIONTYPEID - 1)),
                TransmissionTypeName = vehicle.TRANSMISSIONTYPE,
                VehicleCategoryType = (VehicleCategoryTypes)(Convert.ToInt32(vehicle.VEHICLECATEGORYID - 1)),
                VehicleCategoryTypeName = vehicle.VEHICLECATEGORY,
                PassangerQuantityType = (PassangerQuantityTypes)(Convert.ToInt32(vehicle.PASSENGERQUANTITYID - 1)),
                PassangerQuantityName = vehicle.PASSENGERQUANTITY,
                FuelType = (FuelTypes)(Convert.ToInt32(vehicle.FUELTYPEID - 1)),
                FuelTypeName = vehicle.FUELTYPE,
                BaggageQuantityType = (BaggageQuantityTypes)(Convert.ToInt32(vehicle.BAGGAGEQUANTITYID - 1)),
                BaggageQuantityName = vehicle.BAGGAGEQUANTITY,
                IsThereAirCondition = vehicle.ISAIRCONDITION,
                VendorMinimumDriverAge = vehicle.VENDORMINDRIVERAGE,
                VendorMinimumDrivingLicanseAge = vehicle.VENDORMINDRIVINGLICENSEAGE,
                DeliveryPaymentActive = vehicle.DELIVERYPAYMENTACTIVE,
                CreditCardPaymentActive = vehicle.CREDITCARDPAYMENTTYPE != 0,
                CreditCardPaymentType = (CreditCardPaymentTypes)vehicle.ADVANCEPAYMENTTYPE,
                DailyPricePayNow = (float)Math.Round(vehicle.DAILYPRICEPAYNOW, 2),
                TotalPricePayNow = (float)Math.Round(vehicle.TOTALPRICEPAYNOW, 2),
                DiscountType = vehicle.DISCOUNTTYPE != null ? (DiscountTypes)Convert.ToInt32(vehicle.DISCOUNTTYPE) : DiscountTypes.None,
                DiscountPercent = vehicle.DISCOUNTPERCENT,
                DiscountAmount = (float)Math.Round(vehicle.DISCOUNTPRICE, 2),
                DiscountTotalPrice = (float)Math.Round(vehicle.DISCOUNTTOTALPRICE, 2),
                DiscountTotalPricePayNow = (float)Math.Round(vehicle.DISCOUNTTOTALPRICEPAYNOW, 2),
                DiscountPercentPayNow = vehicle.DISCOUNTPERCENTEPAYNOW,
                DepositPrice = (float)Math.Round(vehicle.DEPOSITPRICE, 2)
            }
            : null;

        public static List<Vehicle> Map(this List<KolayCARResponse.VEHICLE> vehicles)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map());

            return _vehicles;
        }

        public static List<RentalCondition> Map(this List<KolayCARResponse.RENTALCONDITIONS> rentalConditions)
        {
            var _rentalConditions = new List<RentalCondition>();

            if (rentalConditions.Count != 0)
                foreach (var condition in rentalConditions)
                    _rentalConditions.Add(new RentalCondition
                    {
                        ConditionName = condition.RENTALCONDITIONNAME
                    });

            return _rentalConditions;
        }

        public static List<VehicleImage> Map(this List<KolayCARResponse.VEHICLEIMAGES> vehicleImages)
        {
            var _vehicleImages = new List<VehicleImage>();

            if (vehicleImages.Count != 0)
                foreach (var image in vehicleImages)
                    _vehicleImages.Add(new VehicleImage
                    {
                        Url = image.VEHICLEIMAGE
                    });

            return _vehicleImages;
        }
    }
}
