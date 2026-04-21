using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Wishcar
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this WishcarResponseBase.AvaibilityVehicleResponse vehicle,
        ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, int vehicleIndex = 0)
        {
            int rentalDuration = vehicle.GUN.ToIntNullSafe();
            float dailyPrice = vehicle.TUTAR.ToFloatNullSafe();
            float totalPrice = dailyPrice * rentalDuration;

            return vehicle != null ? new Vehicle
            {
                VehicleId = vehicleIndex,
                VehicleCode = vehicle.ARACNO.ToString(),
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = vehicle.ARACADI.ToString(),
                VendorLogo = additionalInformation.Vendor.Logo,
                SippCode = string.Empty,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = rentalDuration,
                DailyPrice = dailyPrice,
                OneWayFee = vehicle.DROPUCRET.ToFloatNullSafe(),
                //TotalPrice = vehicle.TUTAR.ToFloatNullSafe(),
                TotalPrice = totalPrice,
                IsAvailable = vehicle.TUTAR.ToFloatNullSafe() > 0,
                VehicleImages = new List<VehicleImage>
                {
                    new VehicleImage { Url = vehicle.RESIM }
                },
                VehicleType = GetVehicleType(vehicle.GOVDETIPI),
                VehicleTypeName = vehicle.GOVDETIPI,
                TransmissionType = GetTransmissionType(vehicle.VITES_TR),
                TransmissionTypeName = vehicle.VITES_TR,
                VehicleCategoryType = VehicleCategoryTypes.None,
                PassangerQuantityType = GetPassangerQuantityType(vehicle.KISI.ToIntNullSafe()),
                PassangerQuantityName = vehicle.KISI,
                FuelType = GetFuelType(vehicle.YAKIT_TR),
                FuelTypeName = vehicle.YAKIT_TR,
                BaggageQuantityType = GetBaggageQuantityType(vehicle.BAGAJ.ToIntNullSafe()),
                BaggageQuantityName = vehicle.BAGAJ,
                IsThereAirCondition = true,
                VendorMinimumDriverAge = vehicle.YASSINIRI.ToIntNullSafe(),
                VendorMinimumDrivingLicenseAge = vehicle.EHLIYET_YIL.ToIntNullSafe(),
                DailyPricePayNow = dailyPrice,
                TotalPricePayNow = totalPrice,
                FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                Extras = null,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
            }
            : null;
        }
        public static Vehicle Map(this WishcarResponseBase.AvaibilityVehicleResponse vehicle,
        ReservationToken reservationToken = null, int vehicleIndex = 0)
        {
            int rentalDuration = vehicle.GUN.ToIntNullSafe();
            float dailyPrice = vehicle.TUTAR.ToFloatNullSafe() / rentalDuration;

            return vehicle != null ? new Vehicle
            {
                VehicleId = vehicleIndex,
                VehicleCode = vehicle.ARACNO.ToString(),
                VehicleName = vehicle.ARACADI.ToString() + " " + vehicle.YAKIT_TR.ToString() + " - " + vehicle.VITES_TR.ToString(),
                VehicleImages = new List<VehicleImage>
                {
                    new VehicleImage { Url = vehicle.RESIM }
                },
                VehicleType = GetVehicleType(vehicle.GOVDETIPI),
                VehicleTypeName = vehicle.GOVDETIPI,
                TransmissionType = GetTransmissionType(vehicle.VITES_TR),
                TransmissionTypeName = vehicle.VITES_TR,
                VehicleCategoryType = VehicleCategoryTypes.None,
                PassangerQuantityType = GetPassangerQuantityType(vehicle.KISI.ToIntNullSafe()),
                PassangerQuantityName = vehicle.KISI,
                FuelType = GetFuelType(vehicle.YAKIT_TR),
                FuelTypeName = vehicle.YAKIT_TR,
                BaggageQuantityType = GetBaggageQuantityType(vehicle.BAGAJ.ToIntNullSafe()),
                BaggageQuantityName = vehicle.BAGAJ,
                IsThereAirCondition = true,
                VendorMinimumDriverAge = vehicle.YASSINIRI.ToIntNullSafe(),
                VendorMinimumDrivingLicenseAge = vehicle.EHLIYET_YIL.ToIntNullSafe()
            }
            : null;
        }

        public static List<Vehicle> Map(this List<WishcarResponseBase.AvaibilityVehicleResponse> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();
            int i = 0;
            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                {
                    _vehicles.Add(vehicle.Map(additionalInformation: additionalInformation, vendor, vehicleIndex: i));
                    i++;
                }


            return _vehicles;
        }
        public static List<Vehicle> Map(this List<WishcarResponseBase.AvaibilityVehicleResponse> vehicles)
        {
            var _vehicles = new List<Vehicle>();
            int i = 0;
            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                {
                    _vehicles.Add(vehicle.Map(vehicleIndex: i));
                    i++;
                }

            return _vehicles;
        }

        public static FuelTypes GetFuelType(string fuelTypeName)
        {
            switch (fuelTypeName)
            {
                default:
                case "Diesel":
                case "Dizel":
                    return FuelTypes.Diesel;
                case "Petrol":
                case "Benzin":
                    return FuelTypes.Gasoline;
            }
        }

        public static TransmissionTypes GetTransmissionType(string transmissionTypeName)
        {
            switch (transmissionTypeName)
            {
                default:
                case "Auto Transmission":
                    return TransmissionTypes.Automatic;
                case "Manuel Transmission":
                    return TransmissionTypes.Manuel;
            }
        }
        public static VehicleTypes GetVehicleType(string vehicleTypeName)
        {
            switch (vehicleTypeName)
            {
                default:
                case "Hatchback":
                    return VehicleTypes.FiveDoorHatchback;
                case "Sedan":
                    return VehicleTypes.Sedan;
                case "SUV":
                    return VehicleTypes.SUV;
                case "Minibüs":
                case "Kamyonet":
                    return VehicleTypes.Van;
            }
        }
        public static PassangerQuantityTypes GetPassangerQuantityType(int passangerQuantityName)
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

        public static BaggageQuantityTypes GetBaggageQuantityType(int baggageQuantityName)
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
