using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.CredyCarResponseBase;

namespace KolayCAR.Broker.API.Mappers.CredyCar
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this CarsAvailability vehicle,
          ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
          vehicle != null ? new Vehicle
          {
              VehicleId = vehicle.Cars_General_ID,
              VehicleCode = vehicle.Cars_General_ID.ToStringNullSafe(),
              VendorId = additionalInformation.Vendor.VendorId,
              VendorName = additionalInformation.Vendor.VendorName,
              VendorPhone = additionalInformation.Vendor.VendorPhone,
              VendorEmail = additionalInformation.Vendor.VendorEmail,
              VehicleName = $"{vehicle.Brand} {vehicle.Type}",
              VendorLogo = additionalInformation.Vendor.Logo,
              SippCode = "", //vehicle.SIPP,
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
              TransmissionType = GetCredyCarTransmissionType(vehicle.Transmission),
              TransmissionTypeName = vehicle.Transmission,
              VehicleCategoryType = VehicleCategoryTypes.None,
              PassangerQuantityType = GetCredyCarPassangerQuantityType(vehicle.Chairs.ToStringNullSafe()),
              PassangerQuantityName = vehicle.Chairs.ToStringNullSafe(),
              FuelType = GetCredyCarFuelType(vehicle.Fuel),
              FuelTypeName = vehicle.Fuel,
              BaggageQuantityType = GetCredyCarBaggageQuantityType(vehicle.Big_Bags.ToIntNullSafe() + vehicle.Small_Bags.ToIntNullSafe()),
              BaggageQuantityName = (vehicle.Big_Bags.ToIntNullSafe() + vehicle.Small_Bags.ToIntNullSafe()).ToStringNullSafe(),
              IsThereAirCondition = true,
              VendorMinimumDriverAge = vehicle.Driver_Age,
              VendorMinimumDrivingLicenseAge = vehicle.Driving_License_Age,
              DailyPricePayNow = vehicle.Daily_Rental,
              TotalPricePayNow = vehicle.Total_Rental,
              DepositPrice = null,
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
                        ExtraCode = "SCDW",
                        ExtraName = "Süper kasko paketi",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.SCDW
                    },
                    new Extra
                    {
                        ExtraId = 7,
                        ExtraCode = "CDW",
                        ExtraName = "Hasar Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.CDW
                    },
                    new Extra
                    {
                        ExtraId = 8,
                        ExtraCode = "PAI",
                        ExtraName = "Ferdi Kaza Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.PAI
                    },
                    new Extra
                    {
                        ExtraId = 9,
                        ExtraCode = "TGI",
                        ExtraName = "Lastik-Cam-Far Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.TGI
                    },
                    new Extra
                    {
                        ExtraId = 10,
                        ExtraCode = "KM_150",
                        ExtraName = "İlave 150km",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.KM_150
                    },
                    new Extra
                    {
                        ExtraId = 11,
                        ExtraCode = "KM_400",
                        ExtraName = "İlave 400km",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.KM_400
                    }
              }
          }
          : null;

        public static List<Vehicle> Map(this List<CarsAvailability> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(additionalInformation: additionalInformation, vendor));

            return _vehicles;
        }
        public static Vehicle Map(this CarsList vehicle) =>
           vehicle != null ? new Vehicle
           {
               VendorName = "CredyCar",
               VehicleCode = vehicle.ID.ToStringNullSafe(),
               VehicleName = $"{vehicle.Brand} {vehicle.Type} ({vehicle.Fuel}-{vehicle.Transmission})",
               SippCode = "",
               FuelTypeName = vehicle.Fuel,
               TransmissionTypeName = vehicle.Transmission,
               DepositPrice = vehicle.Provizyon.ToFloatNullAvailable(),
               VendorMinimumDriverAge = vehicle.Driver_Age.ToIntNullSafe(),
               VendorMinimumDrivingLicenseAge = vehicle.Driver_Lisance_Age.ToIntNullSafe()
           }
           : null;

        public static List<Vehicle> Map(this List<CarsList> vehicles)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map());

            return _vehicles;
        }

        public static FuelTypes GetCredyCarFuelType(string fuelTypeName)
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

        public static TransmissionTypes GetCredyCarTransmissionType(string transmissionTypeName)
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

        public static PassangerQuantityTypes GetCredyCarPassangerQuantityType(string passangerQuantityName)
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

        public static BaggageQuantityTypes GetCredyCarBaggageQuantityType(int baggageQuantityName)
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
