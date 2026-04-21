using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Hara
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this HaraResponseBase.Cars vehicle,
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
                SippCode = vehicle.SIPP,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = vehicle.Days,
                DailyPrice = vehicle.Daily_Rental.ToFloatNullSafe(),
                OneWayFee = vehicle.Drop.ToFloatNullSafe(),
                TotalPrice = vehicle.Total_Rental.ToFloatNullSafe(),
                IsAvailable = vehicle.Daily_Rental.ToFloatNullSafe() > 0,
                VehicleImages = new List<VehicleImage>
                {
                    new VehicleImage
                    {
                        Url = vehicle.Image_Path.ToStringNullSafe().StartsWith("http://") ? vehicle.Image_Path.Replace("http://", "https://") : vehicle.Image_Path
                    }
                },
                VehicleType = VehicleTypes.None,
                TransmissionType = GetTransmissionType(vehicle.Transmission),
                TransmissionTypeName = vehicle.Transmission,
                VehicleCategoryType = VehicleCategoryTypes.None,
                PassangerQuantityType = GetPassangerQuantityType(vehicle.Chairs),
                PassangerQuantityName = vehicle.Chairs.ToString(),
                FuelType = GetFuelType(vehicle.Fuel),
                FuelTypeName = vehicle.Fuel,
                BaggageQuantityType = GetBaggageQuantityType(vehicle.Big_Bags + vehicle.Small_Bags),
                BaggageQuantityName = (vehicle.Big_Bags + vehicle.Small_Bags).ToString(),
                IsThereAirCondition = true,
                VendorMinimumDriverAge = vehicle.Driver_Age,
                VendorMinimumDrivingLicenseAge = vehicle.Driving_License_Age,
                DailyPricePayNow = vehicle.Daily_Rental.ToFloatNullSafe(),
                TotalPricePayNow = vehicle.Total_Rental.ToFloatNullSafe(),
                DepositPrice = vehicle.Provizyon.ToFloatNullAvailable(),
                TotalKMLimit = null /*!string.IsNullOrEmpty(vehicle.Km_Limit) ? vehicle.Km_Limit.ToIntNullSafe() : (int?)null*/,
                FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
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
                        Price = vehicle.Baby_Seat.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 2,
                        ExtraCode = "Navigation",
                        ExtraName = "Navigasyon",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Navigation.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 3,
                        ExtraCode = "Private_Driver",
                        ExtraName = "Özel şoför",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Private_Driver.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 4,
                        ExtraCode = "Additional_Driver",
                        ExtraName = "Ek sürücü paketi",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Additional_Driver.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 5,
                        ExtraCode = "Child_Seat",
                        ExtraName = "Çocuk koltuğu",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Child_Seat.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 6,
                        ExtraCode = "SCDW",
                        ExtraName = "Süper kasko paketi",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.SCDW.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 7,
                        ExtraCode = "CDW",
                        ExtraName = "Hasar Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.CDW.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 8,
                        ExtraCode = "TGI",
                        ExtraName = "Lastik Cam Far Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.TGI.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 9,
                        ExtraCode = "PAI",
                        ExtraName = "Ferdi Kaza Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.PAI.ToFloatNullSafe()
                    }
                }
            }
            : null;

        public static List<Vehicle> Map(this List<HaraResponseBase.Cars> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(additionalInformation: additionalInformation, vendor));

            return _vehicles;
        }

        public static Vehicle Map(this HaraResponseBase.Cars vehicle) =>
            vehicle != null ? new Vehicle
            {
                VendorName = "Hara",
                VehicleCode = vehicle.ID.ToString(),
                VehicleName = $"{vehicle.Brand} {vehicle.Type} ({vehicle.Fuel}-{vehicle.Transmission}-{vehicle.SIPP})",
                SippCode = vehicle.SIPP,
                FuelTypeName = vehicle.Fuel,
                TransmissionTypeName = vehicle.Transmission,
                VendorMinimumDriverAge = vehicle.Driver_Age,
                VendorMinimumDrivingLicenseAge = vehicle.Driving_License_Age
            }
            : null;

        public static List<Vehicle> Map(this List<HaraResponseBase.Cars> vehicles)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map());

            return _vehicles;
        }

        public static FuelTypes GetFuelType(string fuelTypeName)
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

        public static TransmissionTypes GetTransmissionType(string transmissionTypeName)
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

        public static PassangerQuantityTypes GetPassangerQuantityType(string passangerQuantityName)
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

        public static BaggageQuantityTypes GetBaggageQuantityType(int? baggageQuantityName)
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
