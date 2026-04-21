using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Turevrac2
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this Turevrac2ResponseBase.AvailableVehicle vehicle, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
            vehicle != null
               ? new Vehicle
               {
                   VehicleId = vehicle.group_id.ToIntNullSafe(),
                   VehicleCode = vehicle.group_id,
                   VendorId = additionalInformation.Vendor.VendorId,
                   VendorName = additionalInformation.Vendor.VendorName,
                   VendorPhone = additionalInformation.Vendor.VendorPhone,
                   VendorEmail = additionalInformation.Vendor.VendorEmail,
                   VehicleName = vehicle.car_name,
                   VendorLogo = additionalInformation.Vendor.Logo,
                   SippCode = vehicle.sipp,
                   PickupLocationId = additionalInformation.PickupLocationId,
                   PickupLocationName = additionalInformation.PickupLocationName,
                   ReturnLocationId = additionalInformation.ReturnLocationId,
                   ReturnLocationName = additionalInformation.ReturnLocationName,
                   PickupDateTime = additionalInformation.PickupDateTime,
                   ReturnDateTime = additionalInformation.ReturnDateTime,
                   RentalDuration = vehicle.days.ToIntNullSafe(),
                   DailyPrice = vehicle.daily_rental.ToFloatNullSafe(),
                   OneWayFee = vehicle.drop.ToFloatNullSafe(),
                   TotalPrice = vehicle.total_rental.ToFloatNullSafe(),
                   TotalKMLimit = vehicle.km_limit.ToIntNullSafe(),
                   IsAvailable = vehicle.total_rental.ToFloatNullSafe() > 0,
                   VehicleImages = new List<VehicleImage>
                    {
                        new VehicleImage
                        {
                            Url = vehicle.image_path
                        }
                    },
                   VehicleType = VehicleTypes.None,
                   TransmissionType = GetTurevracTransmissionType(vehicle.transmission),
                   TransmissionTypeName = string.Empty,
                   VehicleCategoryType = VehicleCategoryTypes.None,
                   PassangerQuantityType = GetTurevrac2PassangerQuantityType(vehicle.chairs.ToIntNullSafe()),
                   PassangerQuantityName = vehicle.chairs,
                   FuelType = GetTurevrac2FuelType(vehicle.fuel),
                   FuelTypeName = vehicle.fuel,
                   BaggageQuantityType = GetTurevrac2BaggageQuantityType(vehicle.big_bags.ToIntNullSafe() + vehicle.small_bags.ToIntNullSafe()),
                   BaggageQuantityName = (vehicle.big_bags.ToIntNullSafe() + vehicle.small_bags.ToIntNullSafe()).ToString(),
                   IsThereAirCondition = true,
                   VendorMinimumDriverAge = vehicle.driver_age.ToIntNullSafe(),
                   VendorMinimumDrivingLicenseAge = vehicle.driving_license_age.ToIntNullSafe(),
                   DailyPricePayNow = vehicle.daily_rental.ToFloatNullSafe(),
                   TotalPricePayNow = vehicle.total_rental.ToFloatNullSafe(),
                   DepositPrice = vehicle.provision.ToFloatNullSafe(),
                   FullCredit = vendor.CreditType == CreditType.FullCredit,
                   Extras = GetExtraList(vehicle.Services),
                   RentalWorkingTypes = vendor.RentalWorkingType,
                   ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
               }
            : null;

        private static List<Extra> GetExtraList(List<Turevrac2ResponseBase.Service> services)
        {
            var extraList = new List<Extra>();
            int id = 1;
            if (services != null)
            {
                if (services.Count > 0)
                {
                    foreach (var item in services)
                    {
                        extraList.Add(
                            new Extra
                            {
                                ExtraId = id,
                                ExtraCode = item.service_name,
                                ExtraName = item.service_title,
                                ExtraRentalType = ExtraRentalTypes.PerRental,
                                ExtraQuantityIncreasable = false,
                                Price = item.service_total_price.ToFloatNullSafe()
                            }
                            );
                        id++;
                    }
                }
            }
            return extraList;
        }

        public static List<Vehicle> Map(this List<Turevrac2ResponseBase.AvailableVehicle> availableVehicles, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var vehicles = new List<Vehicle>();
            if (availableVehicles != null)
                foreach (var vehicle in availableVehicles)
                    vehicles.Add(vehicle.Map(additionalInformation, vendor));
            return vehicles;
        }

        public static Vehicle Map(this Turevrac2ResponseBase.Vehicle vehicle, Vendor vendor) =>
            vehicle != null
                ? new Vehicle
                {
                    VendorName = vendor.VendorName,
                    VehicleId = vehicle.group_id.ToIntNullSafe(),
                    VehicleCode = vehicle.group_id,
                    VehicleName = $"{vehicle.group_name} - {vehicle.group_str}",
                    FuelType = GetTurevrac2FuelType(vehicle.fuel),
                    FuelTypeName = vehicle.fuel,
                    TransmissionType = GetTurevracTransmissionType(vehicle.transmission),
                    TransmissionTypeName = vehicle.transmission,
                    SippCode = vehicle.sipp,
                    DepositPrice = vehicle.provision.ToFloatNullSafe(),
                    VendorMinimumDriverAge = vehicle.driver_age.ToIntNullSafe(),
                    VendorMinimumDrivingLicenseAge = vehicle.driving_license_age.ToIntNullSafe()
                }
                : null;
        public static List<Vehicle> Map(this List<Turevrac2ResponseBase.Vehicle> list, Vendor vendor)
        {
            var vehicleList = new List<Vehicle>();
            if (list != null)
                foreach (var vehicle in list)
                {
                    vehicleList.Add(vehicle.Map(vendor));
                }
            return vehicleList;
        }
        private static TransmissionTypes GetTurevracTransmissionType(string transmission)
        {
            switch (transmission)
            {
                default:
                case "Automatic":
                case "Otomatik":
                    return TransmissionTypes.Automatic;
                case "Manuel":
                    return TransmissionTypes.Manuel;
            }
        }

        private static FuelTypes GetTurevrac2FuelType(string fuel)
        {
            {
                switch (fuel)
                {
                    default:
                    case "Diesel":
                    case "Dizel":
                        return FuelTypes.Diesel;
                    case "Petrol":
                    case "Benzin":
                    case "Kurşunsuz":
                        return FuelTypes.Gasoline;
                    case "Hybrid":
                    case "Elektrik":
                        return FuelTypes.HybritGasoline;
                }
            }
        }
        public static PassangerQuantityTypes GetTurevrac2PassangerQuantityType(int passangerQuantityName)
        {
            return passangerQuantityName switch
            {
                2 => PassangerQuantityTypes.TwoPerson,
                3 => PassangerQuantityTypes.ThreePerson,
                4 => PassangerQuantityTypes.FourPerson,
                5 => PassangerQuantityTypes.FivePerson,
                6 => PassangerQuantityTypes.SixPerson,
                7 => PassangerQuantityTypes.SevenPerson,
                8 => PassangerQuantityTypes.EightPerson,
                9 => PassangerQuantityTypes.NinePerson,
                10 => PassangerQuantityTypes.TenPerson,
                11 => PassangerQuantityTypes.ElevenPerson,
                12 => PassangerQuantityTypes.TwelvePerson,
                13 => PassangerQuantityTypes.ThirteenPerson,
                14 => PassangerQuantityTypes.FourteenPerson,
                15 => PassangerQuantityTypes.FifteenPerson,
                16 => PassangerQuantityTypes.SixteenPerson,
                17 => PassangerQuantityTypes.SeventeenPerson,
                18 => PassangerQuantityTypes.EighteenPerson,
                19 => PassangerQuantityTypes.NineteenPerson,
                20 => PassangerQuantityTypes.TwentyPerson,
                _ => PassangerQuantityTypes.OnePerson,
            };
        }
        public static BaggageQuantityTypes GetTurevrac2BaggageQuantityType(int baggageQuantityName)
        {
            return baggageQuantityName switch
            {
                2 => BaggageQuantityTypes.Two,
                3 => BaggageQuantityTypes.Three,
                4 => BaggageQuantityTypes.Four,
                5 => BaggageQuantityTypes.Five,
                6 => BaggageQuantityTypes.Six,
                7 => BaggageQuantityTypes.Seven,
                8 => BaggageQuantityTypes.Eight,
                _ => BaggageQuantityTypes.One,
            };
        }
    }
}
