using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Central2
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this Central2ResponseBase.Group vehicle) =>
             vehicle != null ? new Vehicle
             {
                 VendorName = "Central",
                 VehicleId = Convert.ToInt32(vehicle.Group_ID),
                 VehicleCode = vehicle.Group_ID,
                 VehicleName = $"{vehicle.Group_Str}",
                 FuelType = GetCentralFuelType(vehicle.Fuel),
                 FuelTypeName = vehicle.Fuel,
                 TransmissionType = GetCentralTransmissionType(vehicle.Transmission),
                 TransmissionTypeName = vehicle.Transmission,
                 SippCode = vehicle.SIPP,
                 DepositPrice = vehicle.Provision.ToFloatNullSafe(),
                 VendorMinimumDriverAge = vehicle.Driver_Age.ToIntNullSafe(),
                 VendorMinimumDrivingLicenseAge = vehicle.Driving_License_Age.ToIntNullSafe(),
                 CurrencyCode = vehicle.Currency,
                 BaggageQuantityType = GetCentralBaggageTyppe(vehicle),
                 BaggageQuantityName = (vehicle.Big_Bags.ToIntNullSafe() + vehicle.Small_Bags.ToIntNullSafe()).ToString(),
                 PassangerQuantityType = (PassangerQuantityTypes)vehicle.Big_Bags.ToIntNullSafe() + vehicle.Small_Bags.ToIntNullSafe(),
                 VehicleImages = new List<VehicleImage>
                 {
                     new VehicleImage
                     {
                         Url = vehicle.Image1_Path
                     }
                 }
             }
             : null;

        public static List<Vehicle> Map(this List<Central2ResponseBase.Group> vehicles)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map());

            return _vehicles;
        }
        public static List<Vehicle> Map(this List<Central2ResponseBase.Car> apiVehicles, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();
            foreach (var vehicle in apiVehicles)
            {
                _vehicles.Add(vehicle.Map(additionalInformation, vendor));
            }
            return _vehicles;
        }
        private static Vehicle Map(this Central2ResponseBase.Car apiVehicle, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            return new Vehicle
            {
                VehicleId = apiVehicle.Group_ID.ToIntNullSafe(),
                VehicleCode = apiVehicle.Group_ID,
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = $"{apiVehicle.Brand} {apiVehicle.Type}",
                VendorLogo = additionalInformation.Vendor.Logo,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = apiVehicle.Days.ToIntNullSafe(),
                DailyPrice = apiVehicle.Daily_Rental.ToFloatNullSafe(),
                OneWayFee = apiVehicle.Drop.ToFloatNullSafe(),
                TotalPrice = apiVehicle.Total_Rental.ToFloatNullSafe(),
                IsAvailable = true,
                VehicleImages = new List<VehicleImage>
                {
                    new VehicleImage
                    {
                        Url = apiVehicle.Image_Path
                    }
                },
                VehicleType = VehicleTypes.None,
                TransmissionType = apiVehicle.Transmission == "Otomatik" ? TransmissionTypes.Automatic : apiVehicle.Transmission == "Manuel" ? TransmissionTypes.Manuel : TransmissionTypes.None,
                VehicleCategoryType = VehicleCategoryTypes.None,
                PassangerQuantityType = (PassangerQuantityTypes)apiVehicle.Chairs.ToIntNullSafe(),
                FuelType = apiVehicle.Fuel == "Benzin" ? FuelTypes.Gasoline : apiVehicle.Fuel == "Dizel" ? FuelTypes.Diesel : FuelTypes.None,
                BaggageQuantityType = (BaggageQuantityTypes)(apiVehicle.Big_Bags.ToIntNullSafe() + apiVehicle.Small_Bags.ToIntNullSafe()),
                IsThereAirCondition = true,
                VendorMinimumDriverAge = apiVehicle.Driver_Age.ToIntNullSafe(),
                VendorMinimumDrivingLicenseAge = apiVehicle.Driving_License_Age.ToIntNullSafe(),
                DailyPricePayNow = apiVehicle.Daily_Rental.ToFloatNullSafe(),
                TotalPricePayNow = apiVehicle.Total_Rental.ToFloatNullSafe(),
                DepositPrice = apiVehicle.Provision.ToFloatNullSafe(),
                TotalKMLimit = apiVehicle.Km_Limit.ToIntNullSafe(),
                FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                SippCode = apiVehicle.SIPP,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice,
                Extras = new List<Extra>
                {
                    new Extra
                    {
                        ExtraId = 1,
                        ExtraCode = "Cancel",
                        ExtraName = "İptal Güvence Paketi",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = apiVehicle.Cancel.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 2,
                        ExtraCode = "Additional_Driver",
                        ExtraName = "Ek Sürücü",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = apiVehicle.Additional_Driver.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 3,
                        ExtraCode = "Mini_Damage_Insurance",
                        ExtraName = "Mini Hasar Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = apiVehicle.Mini_Damage_Insurance.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 4,
                        ExtraCode = "Super_Mini_Damage_Insurance",
                        ExtraName = "Süper Mini Hasar Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = apiVehicle.Super_Mini_Damage_Insurance.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 5,
                        ExtraCode = "Max_Assurance",
                        ExtraName = "Maksimum Güvence Paketi",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = apiVehicle.Max_Assurance.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 6,
                        ExtraCode = "LCF",
                        ExtraName = "Lastik/Cam Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = apiVehicle.LCF.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 7,
                        ExtraCode = "Young_Drive",
                        ExtraName = "Genç Sürücü Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = apiVehicle.Young_Drive.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 8,
                        ExtraCode = "IMM",
                        ExtraName = "İhtiyari Mali Mesuliyet Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = apiVehicle.IMM.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 9,
                        ExtraCode = "PAI",
                        ExtraName = "Ferdi Kaza",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = apiVehicle.PAI.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 10,
                        ExtraCode = "Baby_Seat",
                        ExtraName = "Çocuk Koltuğu",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = apiVehicle.Baby_Seat.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 11,
                        ExtraCode = "Navigation",
                        ExtraName = "Navigasyon",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = apiVehicle.Navigation.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 12,
                        ExtraCode = "CDW",
                        ExtraName = "CDW",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = apiVehicle.CDW.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 12,
                        ExtraCode = "XKP",
                        ExtraName = "Maksimum Koruma Paketi",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = apiVehicle.XKP.ToFloatNullSafe()
                    },

                }
            };
        }

        public static BaggageQuantityTypes GetCentralBaggageTyppe(Central2ResponseBase.Group vehicle)
        {
            var count = vehicle.Big_Bags.ToIntNullSafe() + vehicle.Small_Bags.ToIntNullSafe();
            switch (count)
            {
                case 0: return BaggageQuantityTypes.None;
                case 1: return BaggageQuantityTypes.One;
                case 2: return BaggageQuantityTypes.Two;
                case 3: return BaggageQuantityTypes.Three;
                case 4: return BaggageQuantityTypes.Four;
                case 5: return BaggageQuantityTypes.Five;
                case 6: return BaggageQuantityTypes.Six;
                case 7: return BaggageQuantityTypes.Seven;
                case 8: return BaggageQuantityTypes.Eight;
                default: return BaggageQuantityTypes.Eight;
            }
        }
        public static FuelTypes GetCentralFuelType(string fuelTypeName)
        {
            switch (fuelTypeName)
            {
                default:
                case "Diesel":
                case "Dizel":
                    return FuelTypes.Diesel;
                case "Petrol":
                case "Benzin":
                case "Kurşunsuz":
                    return FuelTypes.Gasoline;
            }
        }

        public static TransmissionTypes GetCentralTransmissionType(string transmissionTypeName)
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

        public static PassangerQuantityTypes GetAkkorPassangerQuantityType(int passangerQuantityName)
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

        public static BaggageQuantityTypes GetAkkorBaggageQuantityType(int baggageQuantityName)
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
