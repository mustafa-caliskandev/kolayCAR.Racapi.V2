using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.OtoturResponseBase;

namespace KolayCAR.Broker.API.Mappers.Ototur
{
    public static class VehicleMapper
    {

        public static Vehicle Map(this OtoturAvailableVehicleResponse vehicle,
          ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
          vehicle != null ? new Vehicle
          {
              VehicleId = Int32.Parse(vehicle.aracid),
              VehicleCode = vehicle.aracid.ToStringNullSafe(),
              VendorId = additionalInformation.Vendor.VendorId,
              VendorName = additionalInformation.Vendor.VendorName,
              VendorPhone = additionalInformation.Vendor.VendorPhone,
              VendorEmail = additionalInformation.Vendor.VendorEmail,
              VehicleName = vehicle.vehicleGroupName,
              VendorLogo = additionalInformation.Vendor.Logo,
              SippCode = vehicle.sipp,
              PickupLocationId = additionalInformation.PickupLocationId,
              PickupLocationName = additionalInformation.PickupLocationName,
              ReturnLocationId = additionalInformation.ReturnLocationId,
              ReturnLocationName = additionalInformation.ReturnLocationName,
              PickupDateTime = additionalInformation.PickupDateTime,
              ReturnDateTime = additionalInformation.ReturnDateTime,
              RentalDuration = Int32.Parse(vehicle.rentalday),
              DailyPrice = vehicle.dailyRentalPrice.Replace(",", "").ToFloatNullSafe(),
              OneWayFee = vehicle.dropPrice.Replace(",", "").ToFloatNullSafe(),
              TotalPrice = vehicle.totalRentalPrice.Replace(",", "").ToFloatNullSafe(),
              IsAvailable = true,
              VehicleType = VehicleTypes.None,
              TransmissionType = TransmissionTypes.Manuel,
              TransmissionTypeName = string.Empty,
              VehicleCategoryType = VehicleCategoryTypes.None,
              PassangerQuantityType = PassangerQuantityTypes.FivePerson,
              PassangerQuantityName = vehicle.passenger,
              FuelType = FuelTypes.Gasoline,
              FuelTypeName = "Gasoline",
              BaggageQuantityType = BaggageQuantityTypes.Five,
              BaggageQuantityName = "Five",
              IsThereAirCondition = true,
              VendorMinimumDriverAge = Int32.Parse(vehicle.minDriverAge),
              VendorMinimumDrivingLicenseAge = Int32.Parse(vehicle.minLicenseYear),
              DailyPricePayNow = vehicle.dailyRentalPrice.Replace(",", "").ToFloatNullSafe(),
              TotalPricePayNow = vehicle.totalRentalPrice.Replace(",", "").ToFloatNullSafe(),
              DepositPrice = vehicle.blockedAmount.Replace(",", "").ToFloatNullSafe(),
              DailyKMLimit = Int32.Parse(vehicle.kilometerlimit),
              TotalKMLimit = (Int32.Parse(vehicle.kilometerlimit) * Int32.Parse(vehicle.rentalday)),
              Extras = GetExtras(vehicle),
              FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
              RentalWorkingTypes = vendor.RentalWorkingType,
              ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
          }

          : null;
        public static List<Vehicle> Map(this List<OtoturAvailableVehicleResponse> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(additionalInformation, vendor));

            return _vehicles;
        }
        public static Vehicle Map(this OtoturVehicleResponse vehicle) =>
               vehicle != null ? new Vehicle
               {
                   VendorName = "Ototur",
                   VehicleCode = vehicle.aracid,
                   VehicleName = $"{vehicle.marka} {vehicle.model} ({vehicle.fuel}-{vehicle.transmission})",
                   SippCode = vehicle.sipp,
                   FuelTypeName = vehicle.fuel,
                   TransmissionTypeName = vehicle.transmission,
                   DailyKMLimit = vehicle.kilometerlimit.ToIntNullSafe(),
                   VendorMinimumDriverAge = vehicle.minDriverAge.ToIntNullSafe(),
                   VendorMinimumDrivingLicenseAge = vehicle.minLicenseYear.ToIntNullSafe()
               }
               : null;
        public static List<Vehicle> Map(this List<OtoturVehicleResponse> vehicles)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map());

            return _vehicles;
        }

        public static List<Domain.Models.Extra> GetExtras(OtoturAvailableVehicleResponse vehicle)
        {
            List<Domain.Models.Extra> vehicleExtras = new List<Domain.Models.Extra>();
            foreach (var extra in vehicle.extras)
            {
                vehicleExtras.Add(new Domain.Models.Extra
                {
                    ExtraId = Int32.Parse(extra.id),
                    ExtraCode = extra.id.ToStringNullSafe(),
                    ExtraName = extra.name,
                    ExtraRentalType = extra.sellType == "PER_DAY" ? ExtraRentalTypes.Daily : extra.sellType == "PER_RENTAL" ? ExtraRentalTypes.PerRental : ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false,
                    Price = extra.totalAmount.Replace(",", "").ToFloatNullSafe(),
                });
            }
            return vehicleExtras;
        }
    }
}
