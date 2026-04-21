using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.TurmobilResponseBase;

namespace KolayCAR.Broker.API.Mappers.Turmobil
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this vehicle vehicle, Vendor vendor) =>
             vehicle != null ? new Vehicle
             {
                 VendorName = vendor.VendorName,
                 VehicleId = Convert.ToInt32(vehicle.vehicleTypeId),
                 VehicleCode = vehicle.vehicleTypeId.ToStringNullSafe(),
                 VehicleName = $"{vehicle.vehicleTypeName}",
                 FuelType = GetTurmobilFuelType(vehicle.fuelType),
                 FuelTypeName = vehicle.fuelType,
                 TransmissionType = GetTurmobilTransmissionType(vehicle.gear),
                 TransmissionTypeName = vehicle.gear,
                 //SippCode = vehicle.SIPP,
                 DepositPrice = vehicle.provisionAmount.ToFloatNullSafe(),
                 VendorMinimumDriverAge = vehicle.ageLimit.ToIntNullSafe(),
                 VendorMinimumDrivingLicenseAge = vehicle.licenseAgeLimit.ToIntNullSafe()
             }
             : null;

        public static Vehicle Map(this locationVehicles vehicle, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            if (vehicle != null)
            {
                if (vehicle.hireDay == 0)
                {
                    vehicle.hireDay = 1;
                }
                return new Vehicle
                {
                    IsAvailable = true, // TODO : TEST ICIN ACILDI
                    VendorName = vendor.VendorName,
                    VehicleId = Convert.ToInt32(vehicle.vehicleTypeId),
                    VehicleCode = vehicle.vehicleTypeId.ToStringNullSafe(),
                    VendorId = additionalInformation.Vendor.VendorId,
                    VehicleName = $"{vehicle.vehicleTypeName}",
                    FuelType = GetTurmobilFuelType(vehicle.fuelType),
                    FuelTypeName = vehicle.fuelType,
                    TransmissionType = GetTurmobilTransmissionType(vehicle.gear),
                    TransmissionTypeName = vehicle.gear,
                    DepositPrice = vehicle.provisionAmount.ToFloatNullSafe(),
                    VendorMinimumDriverAge = vehicle.ageLimit.ToIntNullSafe(),
                    VendorMinimumDrivingLicenseAge = vehicle.licenseAgeLimit.ToIntNullSafe(),
                    //DailyPrice = vehicle.dailyAmountLater,
                    //DailyPrice = vehicle.dailyAmount.ToFloatNullSafe(),
                    DailyPrice = (vehicle.totalAmount / vehicle.hireDay).ToFloatNullSafe(),
                    DailyPricePayNow = (vehicle.totalAmount / vehicle.hireDay).ToFloatNullSafe(),
                    //TotalPrice = vehicle.totalAmountLater,
                    TotalPrice = vehicle.totalAmount.ToFloatNullSafe(),
                    TotalPricePayNow = vehicle.totalAmount.ToFloatNullSafe(),
                    RentalDuration = vehicle.hireDay,
                    DailyKMLimit = vehicle.dailyDistanceLimit,
                    TotalKMLimit = vehicle.totalDistanceLimit,
                    VendorLogo = additionalInformation.Vendor.Logo,
                    //VehicleDescription = "",
                    SippCode = string.Empty,
                    OneWayFee = vehicle.dropAmount.ToFloatNullSafe(),
                    //ExtraPrice = 0,
                    //RentalConditions = null,
                    VehicleImages = new List<VehicleImage>
                  {
                        new VehicleImage
                        {
                            Url = "/design/img/no-image.png"
                        }
                  },
                    VehicleType = VehicleTypes.None,
                    //VehicleTypeName = "",
                    VehicleCategoryType = VehicleCategoryTypes.None,
                    VehicleCategoryTypeName = vehicle.groupType,
                    PassangerQuantityType = PassangerQuantityTypes.None,
                    //PassangerQuantityName = "",
                    BaggageQuantityType = BaggageQuantityTypes.None,
                    //BaggageQuantityName = "",
                    IsThereAirCondition = true,
                    PickupLocationId = additionalInformation.PickupLocationId,
                    PickupLocationName = additionalInformation.PickupLocationName,
                    ReturnLocationId = additionalInformation.ReturnLocationId,
                    ReturnLocationName = additionalInformation.ReturnLocationName,
                    PickupDateTime = additionalInformation.PickupDateTime,
                    ReturnDateTime = additionalInformation.ReturnDateTime,
                    FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                    RentalWorkingTypes = vendor.RentalWorkingType,
                    ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
                };
            }
            else
                return null;
        }


        public static List<Vehicle> Map(this List<vehicle> vehicles, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(vendor));

            return _vehicles;
        }


        public static List<Vehicle> Map(this List<locationVehicles> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(additionalInformation, vendor));

            return _vehicles;
        }

        public static FuelTypes GetTurmobilFuelType(string fuelTypeName)
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

        public static TransmissionTypes GetTurmobilTransmissionType(string transmissionTypeName)
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

        public static PassangerQuantityTypes GetTurmobilPassangerQuantityType(int passangerQuantityName)
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

        public static BaggageQuantityTypes GetTurmobilBaggageQuantityType(int baggageQuantityName)
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
