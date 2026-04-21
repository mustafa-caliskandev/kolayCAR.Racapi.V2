using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using System.Reflection;
using static KolayCAR.Broker.Domain.Models.Response.AvisResponseBase;

namespace KolayCAR.Broker.API.Mappers.Avis
{
    public static class VehicleMapper
    {
        public static List<Domain.Models.Vehicle> Map(this List<AvisResponseBase.Vehicle> vehicles, Vendor vendor)
        {
            var _vehicles = new List<Domain.Models.Vehicle>();
            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(vendor));

            return _vehicles;
        }

        public static Domain.Models.Vehicle Map(this AvisResponseBase.Vehicle vehicle, Vendor vendor) =>
           vehicle != null ? new Domain.Models.Vehicle
           {
               VendorName = vendor.VendorName,
               VehicleCode = vehicle.GroupName,
               VehicleName = $"{vehicle.VehicleName} ({vehicle.TransmissionName} - {vehicle.FuelTypeName}) - (Eş değer araç - {vehicle.EquivalentVehicle})",
               VendorMinimumDriverAge = vehicle.MinAge.ToIntNullSafe(),
               VendorMinimumDrivingLicenseAge = vehicle.MinDriverLicense.ToIntNullSafe(),
               FuelTypeName = vehicle.TransmissionName,
               TransmissionTypeName = vehicle.TransmissionName,
               DepositPrice = vehicle.Deposit.ToFloatNullSafe(),
               VehicleClassNo = vehicle.ClassNo,
               VehicleGroupName = vehicle.GroupName
           }
           : null;

        public static List<Domain.Models.Vehicle> Map(this List<AvailableVehicle> vehicles, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation)
        {
            var _vehicles = new List<Domain.Models.Vehicle>();
            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(vendor, additionalInformation));

            return _vehicles;
        }
        public static Domain.Models.Vehicle Map(this AvailableVehicle vehicle, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation) =>
         vehicle != null ? new Domain.Models.Vehicle
         {
             VehicleId = vehicle.category.vehicle_class_name.ToIntNullSafe(),
             VehicleName = vehicle.category.make,
             VehicleBrandName = vehicle.category.model,
             VehicleCode = vehicle.category.vehicle_class_code.ToStringNullSafe(),
             TransmissionType = GetAvisTransmissionType(vehicle.category.vehicle_transmission),
             TransmissionTypeName = vehicle.category.vehicle_transmission,
             PassangerQuantityType = GetAvisPassangerQuantityType(vehicle.capacity.seats),
             PassangerQuantityName = vehicle.capacity.seats,
             VehicleDescription = vehicle.category.name + vehicle.category.vehicle_transmission,
             VendorId = additionalInformation.Vendor.VendorId,
             VendorName = additionalInformation.Vendor.VendorName,
             VendorPhone = additionalInformation.Vendor.VendorPhone,
             VendorEmail = additionalInformation.Vendor.VendorEmail,
             VendorLogo = additionalInformation.Vendor.Logo,
             PickupLocationId = additionalInformation.PickupLocationId,
             PickupLocationName = additionalInformation.PickupLocationName,
             ReturnLocationId = additionalInformation.ReturnLocationId,
             ReturnLocationName = additionalInformation.ReturnLocationName,
             PickupDateTime = additionalInformation.PickupDateTime,
             ReturnDateTime = additionalInformation.ReturnDateTime,
             RentalDuration = vehicle.rate_totals.rate.rentdaycount,
             DailyPrice = vendor.VendorName == "Budget" ? (float)(vehicle.rate_totals.pay_now.original_vehicle_total / vehicle.rate_totals.rate.rentdaycount) : (float)(vehicle.rate_totals.pay_now.vehicle_total / vehicle.rate_totals.rate.rentdaycount),
             OneWayFee = (float)vehicle.rate_totals.pay_now.one_way_fee,
             TotalPrice = vendor.VendorName == "Budget" ? (float)vehicle.rate_totals.pay_now.original_vehicle_total : vehicle.rate_totals.pay_now.reservation_total.ToFloatNullSafe(),
             IsAvailable = true,
             IsThereAirCondition = true,
             DailyPricePayNow = (float)(vehicle.rate_totals.pay_now.vehicle_total / vehicle.rate_totals.rate.rentdaycount),
             TotalPricePayNow = (float)vehicle.rate_totals.pay_now.vehicle_total,
             DailyKMLimit = vehicle.category.kmLimit.ToIntNullSafe() * vehicle.rate_totals.rate.rentdaycount,
             TotalKMLimit = vehicle.category.kmLimit.ToIntNullSafe(),
             Extras = GetExtras(vehicle.rate_totals.extras),
             FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
             RentalWorkingTypes = vendor.RentalWorkingType,
             ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
         }
         : null;

        private static List<Extra> GetExtras(Extras extras)
        {
            List<Extra> extraList = new List<Extra>();

            foreach (PropertyInfo propertyInfo in extras.GetType().GetProperties())
            {
                var price = propertyInfo.GetValue(extras, null);

                if (double.TryParse(price?.ToString() ?? "", out var doublePrice))
                {
                    if (doublePrice > 0)
                        extraList.Add(new Extra { ExtraCode = propertyInfo.Name, Price = price.ToFloatNullSafe(), ExtraRentalType = ExtraRentalTypes.PerRental, ExtraId = 0 });
                }

            }

            //if (extras.additinoal_driver != 0)
            //{
            //    extraList.Add(new Extra { ExtraCode = "additinoal_driver", Price = extras.additinoal_driver.ToFloatNullSafe(), ExtraId = 1 });
            //}
            //if (extras.navigation != 0)
            //{
            //    extraList.Add(new Extra { ExtraCode = "navigation", Price = extras.navigation.ToFloatNullSafe(), ExtraId = 2 });
            //}
            //if (extras.baby_seat != 0)
            //{
            //    extraList.Add(new Extra { ExtraCode = "baby_seat", Price = extras.baby_seat.ToFloatNullSafe(), ExtraId = 3 });
            //}
            //if (extras.young_driver != 0)
            //{
            //    extraList.Add(new Extra { ExtraCode = "young_driver", Price = extras.young_driver.ToFloatNullSafe(), ExtraId = 6 });
            //}
            //if (extras.Bicycle != 0)
            //{
            //    extraList.Add(new Extra { ExtraCode = "Bicycle", Price = extras.Bicycle.ToFloatNullSafe(), ExtraId = 20});
            //}
            //if (extras.DubleConfortSet != 0)
            //{
            //    extraList.Add(new Extra { ExtraCode = "DubleConfortSet", Price = extras.DubleConfortSet.ToFloatNullSafe(), ExtraId = 21 });
            //}
            //if (extras.BasicConfortSet != 0)
            //{
            //    extraList.Add(new Extra { ExtraCode = "BasicConfortSet", Price = extras.BasicConfortSet.ToFloatNullSafe(), ExtraId = 22 });
            //}
            //if (extras.CampTableAndChair != 0)
            //{
            //    extraList.Add(new Extra { ExtraCode = "CampTableAndChair", Price = extras.CampTableAndChair.ToFloatNullSafe(), ExtraId = 23 });
            //}
            //if (extras.TravelRoutePlan != 0)
            //{
            //    extraList.Add(new Extra { ExtraCode = "TravelRoutePlan", Price = extras.TravelRoutePlan.ToFloatNullSafe(), ExtraId = 24 });
            //}
            //if (extras.SafetyBox != 0)
            //{
            //    extraList.Add(new Extra { ExtraCode = "SafetyBox", Price = extras.SafetyBox.ToFloatNullSafe(), ExtraId = 25 });
            //};
            // string[] arr = ["additinoal_driver", "navigation", "baby_seat", "young_driver", "Bicycle", "DubleConfortSet", "BasicConfortSet", "CampTableAndChair", "TravelRoutePlan", "SafetyBox"];

            return extraList;
        }

        private static PassangerQuantityTypes GetAvisPassangerQuantityType(string passenger_capacity)
        {
            return passenger_capacity switch
            {
                "1" => PassangerQuantityTypes.OnePerson,
                "2" => PassangerQuantityTypes.TwoPerson,
                "3" => PassangerQuantityTypes.ThreePerson,
                "4" => PassangerQuantityTypes.FourPerson,
                "5" => PassangerQuantityTypes.FivePerson,
                "6" => PassangerQuantityTypes.SixPerson,
                "7" => PassangerQuantityTypes.SevenPerson,
                "8" => PassangerQuantityTypes.EightPerson,
                "9" => PassangerQuantityTypes.NinePerson,
                "10" => PassangerQuantityTypes.TenPerson,
                _ => PassangerQuantityTypes.None
            };
        }

        private static TransmissionTypes GetAvisTransmissionType(string gear_type)
        {
            return gear_type switch
            {
                "Manual" => TransmissionTypes.Manuel,
                "Manuel" => TransmissionTypes.Manuel,
                "Automatic" => TransmissionTypes.Automatic,
                _ => TransmissionTypes.None
            };
        }
    }
}
