using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Otocar
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this OtocarResponseBase.araclar vehicle,
          ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
          vehicle != null ? new Vehicle
          {
              VehicleId = vehicle.arac_Id,
              VehicleCode = vehicle.arac_Id.ToStringNullSafe(),
              VendorId = additionalInformation.Vendor.VendorId,
              VendorName = additionalInformation.Vendor.VendorName,
              VendorPhone = additionalInformation.Vendor.VendorPhone,
              VendorEmail = additionalInformation.Vendor.VendorEmail,
              VehicleName = $"{vehicle.brand} {vehicle.model}",
              VendorLogo = additionalInformation.Vendor.Logo,
              SippCode = vehicle.segment,
              PickupLocationId = additionalInformation.PickupLocationId,
              PickupLocationName = additionalInformation.PickupLocationName,
              ReturnLocationId = additionalInformation.ReturnLocationId,
              ReturnLocationName = additionalInformation.ReturnLocationName,
              PickupDateTime = additionalInformation.PickupDateTime,
              ReturnDateTime = additionalInformation.ReturnDateTime,
              RentalDuration = vehicle.gun != 0 ? vehicle.gun : (int)Math.Ceiling((additionalInformation.ReturnDateTime - additionalInformation.PickupDateTime).TotalDays),
              DailyPrice = vehicle.daily_price_turkish_lira,
              OneWayFee = vehicle.drop_ucret,
              TotalPrice = vehicle.total_price_turkish_lira,
              IsAvailable = vehicle.daily_price_turkish_lira > 0,
              VehicleImages = new List<VehicleImage>
              {
                    new VehicleImage
                    {
                        Url = vehicle.vehicle_picture != "https://www.otocarrental.com/arabalar/" ? vehicle.vehicle_picture : string.Empty
                    }
              },
              VehicleType = VehicleTypes.None,
              TransmissionTypeName = vehicle.gear,
              VehicleCategoryType = VehicleCategoryTypes.None,
              PassangerQuantityName = vehicle.yolcu.ToStringNullSafe(),
              FuelTypeName = vehicle.fuel,
              BaggageQuantityName = vehicle.bagaj.ToStringNullSafe(),
              IsThereAirCondition = true,
              VendorMinimumDriverAge = vehicle.yas,
              VendorMinimumDrivingLicenseAge = vehicle.ehliyet,
              DailyPricePayNow = vehicle.daily_price_turkish_lira,
              TotalPricePayNow = vehicle.total_price_turkish_lira,
              DepositPrice = vehicle.provizyon,
              DailyKMLimit = vehicle.gunluk_km.ToIntNullSafe(),
              TotalKMLimit = (vehicle.gunluk_km * (vehicle.gun != 0 ? vehicle.gun : additionalInformation.RentalDuration)).ToIntNullSafe(),
              FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
              RentalWorkingTypes = vendor.RentalWorkingType,
              ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice,
              Extras = new List<Extra>
              {

                    new Extra
                    {
                        ExtraId = 1,
                        ExtraCode = "bebek_koltuk",
                        ExtraName = "Bebek koltuğu",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.bebek_koltuk
                    },
                    new Extra
                    {
                        ExtraId = 2,
                        ExtraCode = "lcf_sigorta",
                        ExtraName = "Lcf Sigortası",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.lcf_sigorta
                    },
                    new Extra
                    {
                        ExtraId = 3,
                        ExtraCode = "ek_sofor",
                        ExtraName = "Ek şoför",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.ek_sofor
                    },
                    new Extra
                    {
                        ExtraId = 4,
                        ExtraCode = "navigasyon",
                        ExtraName = "Navigasyon Cihazı",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.navigasyon
                    },
                    new Extra
                    {
                        ExtraId = 5,
                        ExtraCode = "superkasko",
                        ExtraName = "Süper Kasko",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.superkasko
                    }
              }
          }
          : null;
        public static List<Vehicle> Map(this List<OtocarResponseBase.araclar> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(additionalInformation: additionalInformation, vendor));

            return _vehicles;
        }
        public static Vehicle Map(this OtocarResponseBase.Fleet vehicle) =>
           vehicle != null ? new Vehicle
           {
               VendorName = "Otocar",
               VehicleCode = vehicle.Id.ToStringNullSafe(),
               VehicleName = $"{vehicle.brand} {vehicle.model} ({vehicle.fuel}-{vehicle.gear})",
               SippCode = string.Empty,
               FuelTypeName = vehicle.fuel,
               TransmissionTypeName = vehicle.gear,
               DepositPrice = vehicle.provizyon,
               VendorMinimumDriverAge = vehicle.yas,
               VendorMinimumDrivingLicenseAge = vehicle.ehliyet
           }
           : null;
        public static List<Vehicle> Map(this List<OtocarResponseBase.Fleet> vehicles)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map());

            return _vehicles;
        }
    }
}
