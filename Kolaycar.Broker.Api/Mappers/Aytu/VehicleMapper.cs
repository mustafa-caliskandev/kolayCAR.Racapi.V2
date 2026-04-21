using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Response.Aytu;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Aytu
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this AytuCar vehicle) =>
             vehicle != null ? new Vehicle
             {
                 VendorName = "Aytu",
                 VehicleId = Convert.ToInt32(vehicle._id),
                 VehicleCode = vehicle.Group_ID,
                 VehicleName = $"{vehicle.Group_Name} {vehicle.Brand} {vehicle.Type}",
                 FuelType = GetAytuFuelType(vehicle.Fuel),
                 FuelTypeName = vehicle.Fuel,
                 TransmissionType = GetAytuTransmissionType(vehicle.Transmission),
                 TransmissionTypeName = vehicle.Transmission,
                 SippCode = vehicle.SIPP,
                 DepositPrice = vehicle.Provision.ToFloatNullSafe(),
                 VendorMinimumDriverAge = vehicle.Driver_Age.ToIntNullSafe(),
                 VendorMinimumDrivingLicenseAge = vehicle.Driving_License_Age.ToIntNullSafe()
             }
             : null;

        public static Vehicle Map(this AytuRez vehicle,
            ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
            vehicle != null ? new Vehicle
            {
                VehicleId = vehicle.Group_ID.ToIntNullSafe(),
                VehicleCode = vehicle.Group_ID,
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = vehicle.Car_Name,
                VendorLogo = additionalInformation.Vendor.Logo,
                SippCode = vehicle.SIPP, //vehicle.SIPP,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = vehicle.Days.ToIntNullSafe(),
                DailyPrice = vehicle.Daily_Rental.ToFloatNullSafe(),
                OneWayFee = vehicle.Drop.ToFloatNullSafe(),
                TotalPrice = vehicle.Total_Rental.ToFloatNullSafe(),
                TotalKMLimit = vehicle.Km_Limit.ToIntNullSafe(),
                IsAvailable = vehicle.Total_Rental.ToFloatNullSafe() > 0,
                VehicleImages = new List<VehicleImage>
                {
                    new VehicleImage
                    {
                        Url = vehicle.Image_Path
                    }
                },
                VehicleType = VehicleTypes.None,
                TransmissionType = GetAytuTransmissionType(vehicle.Transmission),
                TransmissionTypeName = string.Empty,
                VehicleCategoryType = VehicleCategoryTypes.None,
                PassangerQuantityType = GetAytuPassangerQuantityType(vehicle.Chairs.ToIntNullSafe()),
                PassangerQuantityName = vehicle.Chairs,
                FuelType = GetAytuFuelType(vehicle.Fuel),
                FuelTypeName = string.Empty,
                BaggageQuantityType = GetAytuBaggageQuantityType(vehicle.Big_Bags.ToIntNullSafe() + vehicle.Small_Bags.ToIntNullSafe()),
                BaggageQuantityName = (vehicle.Big_Bags.ToIntNullSafe() + vehicle.Small_Bags.ToIntNullSafe()).ToString(),
                IsThereAirCondition = true,
                VendorMinimumDriverAge = vehicle.Driver_Age.ToIntNullSafe(),
                VendorMinimumDrivingLicenseAge = vehicle.Driving_License_Age.ToIntNullSafe(),
                DailyPricePayNow = vehicle.Daily_Rental.ToFloatNullSafe(),
                TotalPricePayNow = vehicle.Total_Rental.ToFloatNullSafe(),
                DepositPrice = vehicle.Provision.ToFloatNullSafe(),
                FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice,
                Extras = new List<Extra>
                {
                    new Extra
                    {
                        ExtraId = 1,
                        ExtraCode = "CDW",
                        ExtraName = "Hasar Sorumluluk Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.CDW.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 2,
                        ExtraCode = "SCDW",
                        ExtraName = "Süper Hasar Sorumluluk Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.SCDW.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 3,
                        ExtraCode = "LCF",
                        ExtraName = "Lastik, Cam, Far Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.LCF.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 4,
                        ExtraCode = "PAI",
                        ExtraName = "Ferdi Kaza Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.PAI.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 5,
                        ExtraCode = "Baby_Seat",
                        ExtraName = "Bebek Koltuğu",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Baby_Seat.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 6,
                        ExtraCode = "Navigation",
                        ExtraName = "Navigasyon",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Navigation.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 7,
                        ExtraCode = "Additional_Driver",
                        ExtraName = "Ek Sürücü",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Additional_Driver.ToFloatNullSafe()
                    },
                }
            }
            : null;

        public static List<Vehicle> Map(this List<AytuCar> vehicles)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map());

            return _vehicles;
        }

        public static List<Vehicle> Map(this List<AytuRez> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(additionalInformation: additionalInformation, vendor));

            return _vehicles;
        }

        public static FuelTypes GetAytuFuelType(string fuelTypeName)
        {
            switch (fuelTypeName)
            {
                default:
                case "Diesel":
                case "Dizel":
                    return FuelTypes.Diesel;
                case "Petrol":
                case "Kurşunsuz":
                    return FuelTypes.Gasoline;
            }
        }

        public static TransmissionTypes GetAytuTransmissionType(string transmissionTypeName)
        {
            switch (transmissionTypeName)
            {
                default:
                case "Automatic":
                case "Otomatik":
                    return TransmissionTypes.Automatic;
                case "Manuel":
                    return TransmissionTypes.Manuel;
            }
        }

        public static PassangerQuantityTypes GetAytuPassangerQuantityType(int passangerQuantityName)
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

        public static BaggageQuantityTypes GetAytuBaggageQuantityType(int baggageQuantityName)
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
