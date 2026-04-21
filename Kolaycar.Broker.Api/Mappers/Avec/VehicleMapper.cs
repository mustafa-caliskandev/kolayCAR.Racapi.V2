using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Avec
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this Car vehicle,
            ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
            vehicle != null ? new Vehicle
            {
                VehicleId = vehicle.Car_ID,
                VehicleCode = vehicle.Cars_General_ID.ToString(),
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = $"{vehicle.Brand} {vehicle.Type}",
                VendorLogo = additionalInformation.Vendor.Logo,
                SippCode = vehicle.Group_Name,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = vehicle.Days,
                DailyPrice = vehicle.Daily_Rental,
                OneWayFee = vehicle.Drop,
                TotalPrice = vehicle.Total_Rental,
                IsAvailable = vehicle.Daily_Rental > 0,
                VehicleImages = new List<VehicleImage>
                {
                    new VehicleImage
                    {
                        Url = vehicle.Image_Path != "https://avecccarrentals.com/arabalar/" ? vehicle.Image_Path : string.Empty
                    }
                },
                VehicleType = VehicleTypes.None,
                TransmissionType = GetAvecTransmissionType(vehicle.Transmission),
                TransmissionTypeName = vehicle.Transmission,
                VehicleCategoryType = VehicleCategoryTypes.None,
                PassangerQuantityType = GetAvecPassangerQuantityType(vehicle.Chairs),
                PassangerQuantityName = vehicle.Chairs.ToString(),
                FuelType = GetAvecFuelType(vehicle.Fuel),
                FuelTypeName = vehicle.Fuel,
                BaggageQuantityType = GetAvecBaggageQuantityType(vehicle.Big_Bags + vehicle.Small_Bags),
                BaggageQuantityName = (vehicle.Big_Bags + vehicle.Small_Bags).ToString(),
                IsThereAirCondition = true,
                VendorMinimumDriverAge = vehicle.Driving_License_Age,
                VendorMinimumDrivingLicenseAge = vehicle.Driver_Age,
                DailyPricePayNow = vehicle.Daily_Rental,
                TotalPricePayNow = vehicle.Total_Rental,
                DepositPrice = null,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice,
                Extras = new List<Extra>
                {
                    new Extra
                    {
                        ExtraId = 1,
                        ExtraCode = "Baby_Seat",
                        ExtraName = "Bebek koltuğu",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Baby_Seat
                    },
                    new Extra
                    {
                        ExtraId = 2,
                        ExtraCode = "Navigation",
                        ExtraName = "Navigasyon",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Navigation
                    },
                    new Extra
                    {
                        ExtraId = 3,
                        ExtraCode = "Private_Driver",
                        ExtraName = "Özel şoför",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Private_Driver
                    },
                    new Extra
                    {
                        ExtraId = 4,
                        ExtraCode = "Additional_Driver",
                        ExtraName = "Ek sürücü paketi",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Additional_Driver
                    },
                    new Extra
                    {
                        ExtraId = 5,
                        ExtraCode = "Child_Seat",
                        ExtraName = "Çocuk koltuğu",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Child_Seat
                    },
                    new Extra
                    {
                        ExtraId = 6,
                        ExtraCode = "Additional_KM",
                        ExtraName = "Ek km paketi",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Additional_KM
                    },
                    new Extra
                    {
                        ExtraId = 7,
                        ExtraCode = "Wifi",
                        ExtraName = "Wifi",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Wifi
                    },
                    new Extra
                    {
                        ExtraId = 8,
                        ExtraCode = "Young_Drive",
                        ExtraName = "Genç sürücü paketi",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Young_Drive
                    },
                    new Extra
                    {
                        ExtraId = 9,
                        ExtraCode = "SCDW",
                        ExtraName = "Süper kasko paketi",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.SCDW
                    },
                    new Extra
                    {
                        ExtraId = 10,
                        ExtraCode = "CDW",
                        ExtraName = "Hasar Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.CDW
                    },
                    new Extra
                    {
                        ExtraId = 11,
                        ExtraCode = "PAI",
                        ExtraName = "Ferdi Kaza Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.PAI
                    }
                }
            }
            : null;

        public static List<Vehicle> Map(this List<Car> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(additionalInformation: additionalInformation, vendor));

            return _vehicles;
        }

        public static Vehicle Map(this Car vehicle) =>
            vehicle != null ? new Vehicle
            {
                VendorName = "Avec",
                VehicleCode = vehicle.ID,
                VehicleName = $"{vehicle.Brand} {vehicle.Type} ({vehicle.Fuel}-{vehicle.Transmission}-{vehicle.SIPP})",
                SippCode = vehicle.SIPP,
                FuelTypeName = vehicle.Fuel,
                TransmissionTypeName = vehicle.Transmission
            }
            : null;

        public static List<Vehicle> Map(this List<Car> vehicles)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map());

            return _vehicles;
        }

        public static FuelTypes GetAvecFuelType(string fuelTypeName)
        {
            switch (fuelTypeName)
            {
                default:
                case "Dizel":
                    return FuelTypes.Diesel;
                case "Benzin":
                    return FuelTypes.Gasoline;
            }
        }

        public static TransmissionTypes GetAvecTransmissionType(string transmissionTypeName)
        {
            switch (transmissionTypeName)
            {
                default:
                case "Otomatik":
                    return TransmissionTypes.Automatic;
                case "Düz":
                    return TransmissionTypes.Manuel;
            }
        }

        public static PassangerQuantityTypes GetAvecPassangerQuantityType(string passangerQuantityName)
        {
            return passangerQuantityName switch
            {
                "2" => PassangerQuantityTypes.TwoPerson,
                "3" => PassangerQuantityTypes.ThreePerson,
                "4" => PassangerQuantityTypes.FourPerson,
                "5" => PassangerQuantityTypes.FivePerson,
                "6" => PassangerQuantityTypes.SixPerson,
                "7" => PassangerQuantityTypes.SevenPerson,
                "8" => PassangerQuantityTypes.EightPerson,
                "8+1" => PassangerQuantityTypes.NinePerson,
                "9" => PassangerQuantityTypes.NinePerson,
                "10" => PassangerQuantityTypes.TenPerson,
                "11" => PassangerQuantityTypes.ElevenPerson,
                "12" => PassangerQuantityTypes.TwelvePerson,
                "13" => PassangerQuantityTypes.ThirteenPerson,
                "14" => PassangerQuantityTypes.FourteenPerson,
                "15" => PassangerQuantityTypes.FifteenPerson,
                "16" => PassangerQuantityTypes.SixteenPerson,
                "17" => PassangerQuantityTypes.SeventeenPerson,
                "18" => PassangerQuantityTypes.EighteenPerson,
                "19" => PassangerQuantityTypes.NineteenPerson,
                "20" => PassangerQuantityTypes.TwentyPerson,
                _ => PassangerQuantityTypes.OnePerson,
            };
        }

        public static BaggageQuantityTypes GetAvecBaggageQuantityType(int baggageQuantityName)
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
