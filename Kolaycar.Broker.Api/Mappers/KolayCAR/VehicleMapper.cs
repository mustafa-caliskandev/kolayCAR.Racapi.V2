using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using KolayCARResponse = KolayCAR.Broker.Domain.Models.Response;

namespace KolayCAR.Broker.API.Mappers.KolayCAR
{
    public static class VehicleMapper
    {
        public static Vehicle Map(
            this VEHICLE vehicle,
            ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
            vehicle != null ? new Vehicle
            {
                VehicleId = vehicle.VEHICLEID,
                VehicleCode = vehicle.VEHICLEID.ToString() + "-" + (vehicle.VENDORID != 0 ? vehicle.VENDORID : additionalInformation.APIVendorId).ToString(),
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = vehicle.VEHICLENAME,
                VendorLogo = additionalInformation.Vendor.Logo,
                VehicleDescription = vehicle.VEHICLEDESCRIPTION,
                SippCode = vehicle.SIPPCODE,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = vehicle.RENTALDURATION,
                DailyPrice = vehicle.DAILYPRICE,

                OneWayFee = vehicle.ONEWAYFEE,
                ExtraPrice = vehicle.EXTRAPRICE,
                TotalPrice = vehicle.TOTALPRICE,
                IsAvailable = vehicle.ISAVAILABLE == 1,
                RentalConditions = null,
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
                VendorMinimumDrivingLicenseAge = vehicle.VENDORMINDRIVINGLICENSEAGE,
                DailyPricePayNow = vehicle.DAILYPRICEPAYNOW,
                TotalPricePayNow = vehicle.TOTALPRICEPAYNOW,
                DepositPrice = vehicle.DEPOSITPRICE,
                TotalKMLimit = !string.IsNullOrEmpty(vehicle.TOTALKMLIMIT) ? vehicle.TOTALKMLIMIT.ToIntNullSafe() : (int?)null,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
                //FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false
            }
            : null;

        public static Vehicle Map(
            this VEHICLE vehicle) =>
            vehicle != null ? new Vehicle
            {
                VehicleId = vehicle.VEHICLEID,
                VehicleCode = $"{vehicle.VEHICLEID}-{vehicle.VENDORID}",
                VendorId = vehicle.VENDORID,
                VendorName = vehicle.VENDORNAME,
                VehicleName = $"{vehicle.VEHICLENAME} ({vehicle.FUELTYPE}-{vehicle.TRANSMISSIONTYPE}-{vehicle.VEHICLETYPE})",
                VendorLogo = vehicle.VENDORLOGO,
                VehicleDescription = vehicle.VEHICLEDESCRIPTION,
                SippCode = vehicle.SIPPCODE,
                RentalDuration = vehicle.RENTALDURATION,
                DailyPrice = vehicle.DAILYPRICE,
                OneWayFee = vehicle.ONEWAYFEE,
                ExtraPrice = vehicle.EXTRAPRICE,
                TotalPrice = vehicle.TOTALPRICE,
                IsAvailable = vehicle.ISAVAILABLE == 1,
                RentalConditions = null,
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
                VendorMinimumDrivingLicenseAge = vehicle.VENDORMINDRIVINGLICENSEAGE,
                DailyPricePayNow = vehicle.DAILYPRICEPAYNOW,
                TotalPricePayNow = vehicle.TOTALPRICEPAYNOW,
                DepositPrice = vehicle.DEPOSITPRICE,
                TotalKMLimit = !string.IsNullOrEmpty(vehicle.TOTALKMLIMIT) ? vehicle.TOTALKMLIMIT.ToIntNullSafe() : (int?)null
            }
            : null;


        ///km limit == 0 ise sınırsız
        ///!= 0 ise tanımlanmıs
        ///== "" (gösterme)

        public static List<Vehicle> Map(
            this List<KolayCARResponse.VEHICLE> vehicles,
            ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(additionalInformation, vendor));

            return _vehicles;
        }

        public static List<Vehicle> Map(
            this List<KolayCARResponse.VEHICLE> vehicles)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map());

            return _vehicles;
        }

        public static List<RentalCondition> Map(this List<KolayCARResponse.RENTALCONDITIONS> rentalConditions)
        {
            var _rentalConditions = new List<RentalCondition>();

            if (rentalConditions != null && rentalConditions.Count != 0)
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

            if (vehicleImages != null && vehicleImages.Count != 0)
                foreach (var image in vehicleImages)
                    _vehicleImages.Add(new VehicleImage
                    {
                        Url = image.VEHICLEIMAGE
                    });

            return _vehicleImages;
        }
    }
}
