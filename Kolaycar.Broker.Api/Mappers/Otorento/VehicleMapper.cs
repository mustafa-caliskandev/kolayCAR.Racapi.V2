using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web;

namespace KolayCAR.Broker.API.Mappers.Otorento
{
    public static class VehicleMapper
    {

        public static Vehicle Map(this OtorentoResponseBase.VehicleModel vehicle,
           ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor
        )
        {


            return vehicle != null ? new Vehicle
            {
                //VehicleId = int.Parse(VehicleID(vehicle.Id)),
                VehicleId = VehicleID(vehicle.Id),
                VehicleCode = vehicle.Id.ToString(),
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = vehicle.Brand + vehicle.Serial,
                VendorLogo = additionalInformation.Vendor.Logo,
                SippCode = vehicle.Sipp,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                VehicleImages = new List<VehicleImage>
                {
                    new VehicleImage
                    {
                        Url = HttpUtility.UrlDecode(vehicle.ImgB),
                    },

                },
                RentalDuration = vehicle.NumberOfDays.ToIntNullSafe(),
                //RentalDuration = GetRentalDuration(additionalInformation.PickupDateTime, additionalInformation.ReturnDateTime),
                //DailyPrice = vehicle.IntegralDailyPrice.ToFloatNullSafe(),
                DailyPrice = vehicle.Price.ToFloatNullSafe()/vehicle.NumberOfDays.ToIntNullSafe(),
                OneWayFee = vehicle.DropPrice.ToFloatNullSafe(),
                TotalPrice = vehicle.Price.ToFloatNullSafe(),
                IsAvailable = true,
                TransmissionType = GetTransmissionType(vehicle.Transmission),
                TransmissionTypeName = vehicle.Transmission,
                VehicleCategoryType = VehicleCategoryTypes.None,
                VehicleCategoryTypeName = vehicle.Class,
                PassangerQuantityType = GetPassangerQuantityType(vehicle.NumberOfSeats.ToIntNullSafe()),
                PassangerQuantityName = vehicle.NumberOfSeats.ToString() + " Kişi",
                FuelType = GetFuelType(vehicle.Fuel),
                FuelTypeName = vehicle.Fuel,
                BaggageQuantityType = GetBaggageQuantityType(vehicle.NumberOfDoors.ToIntNullSafe()),
                BaggageQuantityName = vehicle.NumberOfDoors.ToString() + " Bagaj",
                IsThereAirCondition = true,
                VendorMinimumDriverAge = vehicle.DriverAge.ToIntNullSafe(),
                VendorMinimumDrivingLicenseAge = vehicle.DriverLicenceYear.ToIntNullSafe(),
                DepositPrice = vehicle.Deposit.ToFloatNullSafe(),
                TotalKMLimit = TotalKM(vehicle.MileageLimit).ToIntNullSafe(),
                DailyPricePayNow = vehicle.Price.ToFloatNullSafe() / vehicle.NumberOfDays.ToIntNullSafe(),
                TotalPricePayNow = vehicle.Price.ToFloatNullSafe(),
                IsOffice = vehicle.isOffice.ToBoolNullSafe(),
                BaseVendorId = ProviderId(vehicle.Id),
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
                // Extras = CircularResponseBase.packages.Map()

                //Extras = new List<Extra> { }

            }
            : null;

        }



        public static Vehicle Map(this OtorentoResponseBase.VehicleModel vehicle) =>
        vehicle != null ? new Vehicle

        {
            VendorName = "Otorento",
            // VehicleCode = VehicleID(vehicle.Id),
            VehicleName = $"{vehicle.Brand} ({vehicle.Fuel}-{vehicle.Transmission})",
            SippCode = vehicle.Sipp,
            FuelTypeName = vehicle.Fuel,
            TransmissionTypeName = vehicle.Transmission,
            VendorMinimumDriverAge = vehicle.DriverAge.ToIntNullSafe(),
            VendorMinimumDrivingLicenseAge = vehicle.DriverLicenceYear.ToIntNullSafe(),
            DepositPrice = vehicle.Deposit.ToFloatNullSafe(),
            IsAvailable = true,
        }
        : null;

        public static List<Vehicle> Map(this List<OtorentoResponseBase.VehicleModel> data)
        {
            var _vehicles = new List<Vehicle>();
            foreach (var item in data)
            {
                _vehicles.Add(item.Map());

            }
            return _vehicles;
        }

        public static List<Vehicle> Map(this List<OtorentoResponseBase.VehicleModel> data, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();
            foreach (var item in data)
            {
                _vehicles.Add(item.Map(additionalInformation, vendor));
            }
            return _vehicles;
        }

        public static int VehicleID(string vehicle)
        {

            var otorentoVehicleIdModel = JsonConvert.DeserializeObject<OtorentoVehicleIdModel>(vehicle);
            return otorentoVehicleIdModel.Id;
        }

        public static int ProviderId(string vehicle)
        {

            var otorentoProviderIdModel = JsonConvert.DeserializeObject<OtorentoVehicleIdModel>(vehicle);
            return otorentoProviderIdModel.providerId;
        }



        public static string TotalKM(string km)
        {
            string[] totalkm = km.Split(' ');

            return totalkm[0];
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

                case "Benzin Elektrik":
                    return FuelTypes.HybritGasoline;

                case "Elektrikli":
                    return FuelTypes.Electric;

                case "Hybrid":
                    return FuelTypes.HybritDiesel;
            }
        }
        public static TransmissionTypes GetTransmissionType(string transmissionTypeName)
        {
            switch (transmissionTypeName)
            {
                default:
                case "Otomatik":
                    return TransmissionTypes.Automatic;
                case "Manuel":
                    return TransmissionTypes.Manuel;
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
        public static VehicleCategoryTypes GetVehicleCategoryTypes(string vehicleclass)
        {
            switch (vehicleclass)
            {
                default:
                case "Ekonomi":
                    return VehicleCategoryTypes.Economic;
                case "Orta Sınıf":
                    return VehicleCategoryTypes.Standard;
                case "Üst Sınıf":
                    return VehicleCategoryTypes.Luxury;
            }

        }
        public static int GetRentalDuration(DateTime pickupdatetime, DateTime returndatetime)
        {
            TimeSpan günfarki = returndatetime - pickupdatetime;
            int rentalduration = günfarki.Days;

            return rentalduration;
        }


    }
    public class OtorentoVehicleIdModel
    {
        public int Id { get; set; }
        public int providerId { get; set; }
        public string reservationToken { get; set; }
    }
}
