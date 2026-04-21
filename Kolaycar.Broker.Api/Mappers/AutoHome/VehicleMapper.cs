using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Mappers.AutoHome
{
    public static class VehicleMapper
    {

        public static Vehicle Map(this AutoHomeResponseBase.Vehicle vehicle,
            ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
            =>
             vehicle != null ? new Vehicle
             {
                 VehicleId = vehicle.SubGroupId,
                 VehicleCode = vehicle.SubGroupShortName,
                 VendorId = additionalInformation.Vendor.VendorId,
                 VendorName = additionalInformation.Vendor.VendorName,
                 VendorPhone = additionalInformation.Vendor.VendorPhone,
                 VendorEmail = additionalInformation.Vendor.VendorEmail,
                 VehicleName = vehicle.SubGroupShortName,
                 VendorLogo = additionalInformation.Vendor.Logo,
                 SippCode = vehicle.SubGroupShortName,
                 PickupLocationId = additionalInformation.PickupLocationId,
                 PickupLocationName = additionalInformation.PickupLocationName,
                 ReturnLocationId = additionalInformation.ReturnLocationId,
                 ReturnLocationName = additionalInformation.ReturnLocationName,
                 PickupDateTime = additionalInformation.PickupDateTime,
                 ReturnDateTime = additionalInformation.ReturnDateTime,
                 RentalDuration = vehicle.Days.ToIntNullSafe(),
                 DailyPrice = vehicle.DiscountedDailyPrice.ToFloatNullSafe(),
                 OneWayFee = vehicle.DropPrice.ToFloatNullSafe(),
                 TotalPrice = vehicle.GrandTotal.ToFloatNullSafe(),
                 IsAvailable = true,
                 VehicleType = VehicleTypes.None,
                 TransmissionType = TransmissionTypes.None,
                 VehicleCategoryType = VehicleCategoryTypes.None,
                 PassangerQuantityType = PassangerQuantityTypes.None,
                 FuelType = FuelTypes.Diesel,
                 BaggageQuantityType = BaggageQuantityTypes.None,
                 IsThereAirCondition = true,
                 VendorMinimumDriverAge = vehicle.DriverMinAge,
                 VendorMinimumDrivingLicenseAge = vehicle.DriverMinLicenceYear,
                 DepositPrice = vehicle.ProvisionAmountTRY.ToFloatNullSafe(),
                 TotalKMLimit = vehicle.TotalKmLimit.ToIntNullSafe(),
                 DailyPricePayNow = vehicle.DiscountedDailyPrice.ToFloatNullSafe(),
                 TotalPricePayNow = vehicle.GrandTotal.ToFloatNullSafe(),
                 DailyKMLimit = vehicle.DailyKmLimit.ToIntNullSafe(),
                 FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                 // Extras = CircularResponseBase.packages.Map()
                 Extras = GetExtras(vehicle.Extras.Where(e => e.WebOnlineSelling == "1").ToList()),
                 RentalWorkingTypes = vendor.RentalWorkingType,
                 ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
             } : null;

        private static List<Extra> GetExtras(List<AutoHomeResponseBase.Extra> extras)
        {
            var extraList = new List<Extra>();
            if (extras != null)
            {
                foreach (var extra in extras)
                {
                    extraList.Add(new Extra
                    {
                        ExtraCode = extra.ProductCode,
                        ExtraName = extra.ProductName,
                        Price = extra.Amount.ToFloatNullSafe(),
                        ExtraDescription = extra.Description,
                        ExtraRentalType = ExtraRentalTypes.Daily
                    });
                }
            }
            return extraList;
        }

        public static Vehicle Map(this AutoHomeResponseBase.Vehicle vehicle, Vendor vendor)
        {
            if (vehicle != null)
            {
                return new Vehicle
                {
                    VendorName = vendor.VendorName,
                    VehicleId = vehicle.SubGroupId,
                    VehicleCode = vehicle.SubGroupShortName,
                    VehicleName = $"{vehicle.SubGroupName}",
                    //VehicleName = $"{vehicle.ModelName} ({vehicle.FuelType}-{vehicle.Transmission})",
                    //SippCode = vehicle.SubGroupShortName.ToStringNullSafe(),
                    FuelTypeName = vehicle.FuelType,
                    TransmissionTypeName = vehicle.Transmission,
                    VendorMinimumDriverAge = vehicle.YoungDriverMinAge.ToIntNullSafe(),
                    //VendorMinimumDrivingLicenseAge = vehicle.dri.ToIntNullSafe(),
                    DepositPrice = vehicle.ProvisionAmountTRY.ToFloatNullSafe(),
                    IsAvailable = true,
                };
            }
            else
            {
                return null;
            }
        }


        public static List<Vehicle> Map(this List<AutoHomeResponseBase.Vehicle> data, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();
            foreach (var item in data)
            {
                _vehicles.Add(item.Map(vendor));

            }
            return _vehicles;
        }

        public static List<Vehicle> Map(this List<AutoHomeResponseBase.Vehicle> data, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();
            foreach (var item in data)
            {
                _vehicles.Add(item.Map(additionalInformation, vendor));
            }
            return _vehicles;
        }
    }



    //public static List<Vehicle> Map(this List<CircularResponseBase.AvaibilityVehicleResponse> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, CircularResponseBase CircularResponseBase)
    //{
    //    var _vehicles = new List<Vehicle>();

    //    if (vehicles != null && vehicles.Count != 0)
    //        foreach (var vehicle in vehicles)
    //            _vehicles.Add(vehicle.Map(CircularResponseBase: CircularResponseBase, additionalInformation: additionalInformation));

    //    return _vehicles;
    //}



    //    public static List<Vehicle> Map(this AutoHomeResponseBase.Location data, ResponseReservationStepsAdditionalInformation additionalInformation, string baseUrl = "")
    //    {
    //    var _vehicles = new List<Vehicle>();
    //    foreach (var item in data.groups)
    //    {
    //        _vehicles.Add(item.Map(additionalInformation, baseUrl: baseUrl, dropPrice: data.reservation.dropprice.ToFloatNullSafe()));
    //    }
    //    return _vehicles;
    //    }

    //    public static List<Vehicle> Map(this List<AutoHomeResponseBase.Vehicle> data)
    //    {
    //        var _vehicles = new List<Vehicle>();
    //        foreach (var item in data)
    //        {
    //            _vehicles.Add(item.Map());

    //        }
    //        return _vehicles;
    //    }
}

