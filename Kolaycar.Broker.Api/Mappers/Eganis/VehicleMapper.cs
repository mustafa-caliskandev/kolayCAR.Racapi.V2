using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;


namespace KolayCAR.Broker.API.Mappers.Eganis
{
    public static class VehicleMapper
    {

        public static List<CommonModels.Vehicle> Map(this List<EganisResponseBase.VehicleResponse.VehicleRes> vehicles, CommonModels.Vendor vendor)
        {

            var _vehicles = new List<CommonModels.Vehicle>();

            if (vehicles?.Count > 0)
            {
                foreach (var vehicle in vehicles)
                {
                    _vehicles.Add(vehicle.Map(vendor));
                }
            }
            return _vehicles;
        }

        public static CommonModels.Vehicle Map(this EganisResponseBase.VehicleResponse.VehicleRes vehicle, CommonModels.Vendor vendor) =>
            vehicle != null ? new CommonModels.Vehicle
            {
                VendorName = vendor.VendorName,
                VehicleId = vehicle.vehGroupId,
                VehicleCode = vehicle.vehGroupId.ToString(),
                SippCode = vehicle.sippCode,
                VehicleName = vehicle.groupName,
                VehicleImages = new List<VehicleImage>
                {
                    new VehicleImage
                    {
                        Url = vehicle.imagePath
                    },
                },
                FuelType = GetEganisFuelType(vehicle.fuelType),
                TransmissionType = GetEganisTransmissionType(vehicle.transmissionType),
                FuelTypeName = vehicle.fuelType,
                TransmissionTypeName = vehicle.transmissionType,
                VendorMinimumDriverAge = vehicle.driverAge,
                VendorMinimumDrivingLicenseAge = vehicle.drivingLicenseAge,
                PassangerQuantityType = GetEganisPassangerQuantityType(vehicle.seatCount),
                PassangerQuantityName = vehicle.seatCount.ToString(),
                BaggageQuantityType = GetEganisBaggageQuantityType(vehicle.smallLuggageCapacity + vehicle.largeLuggageCapacity),
                BaggageQuantityName = $"large Luggage Capacity = {vehicle.largeLuggageCapacity} | small Luggage Capacity = {vehicle.smallLuggageCapacity}",

            } : null;



        public static List<CommonModels.Vehicle> Map(this List<EganisResponseBase.VehicleResponse.VehicleRes> vehicles, CommonModels.Vendor vendor,
            ResponseReservationStepsAdditionalInformation additionalInformation)
        {

            var _vehicles = new List<CommonModels.Vehicle>();

            if (vehicles?.Count > 0)
            {
                foreach (var vehicle in vehicles)
                {
                    _vehicles.Add(vehicle.Map(vendor, additionalInformation));
                }
            }
            return _vehicles;
        }


        public static CommonModels.Vehicle Map(this EganisResponseBase.VehicleResponse.VehicleRes vehicle, CommonModels.Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation) =>
            vehicle != null ? new CommonModels.Vehicle
            {
                VendorName = vendor.VendorName,
                VehicleId = vehicle.vehGroupId,
                VehicleCode = vehicle.vehGroupId.ToString(),
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VendorLogo = additionalInformation.Vendor.Logo,
                SippCode = vehicle.sippCode,
                VehicleName = vehicle.vehicleName,
                PickupLocationId = additionalInformation.PickupLocationId,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = vehicle.totalDays,

                VehicleImages = new List<VehicleImage>
                {
                            new VehicleImage
                            {
                                Url = vehicle.imagePath
                            },
                },
                FuelType = GetEganisFuelType(vehicle.fuelType),
                TransmissionType = GetEganisTransmissionType(vehicle.transmissionType),
                FuelTypeName = vehicle.fuelType,
                TransmissionTypeName = vehicle.transmissionType,
                VendorMinimumDriverAge = vehicle.driverAge,
                VendorMinimumDrivingLicenseAge = vehicle.drivingLicenseAge,
                PassangerQuantityType = GetEganisPassangerQuantityType(vehicle.seatCount),
                PassangerQuantityName = vehicle.seatCount.ToString(),
                BaggageQuantityType = GetEganisBaggageQuantityType(vehicle.smallLuggageCapacity + vehicle.largeLuggageCapacity),
                BaggageQuantityName = $"large Luggage Capacity = {vehicle.largeLuggageCapacity} | small Luggage Capacity = {vehicle.smallLuggageCapacity}",
                OneWayFee = vehicle.dropFee,
                TotalKMLimit = vehicle.kmLimit,
                DailyKMLimit = (vehicle.kmLimit / vehicle.totalDays).ToIntNullSafe(),
                DailyPrice = vehicle.dailyRentalFee,
                TotalPrice = vehicle.rentalFee,
                ApiDailyPrice = vehicle.dailyRentalFee,
                IsAvailable = true,
                Extras = vehicle.extras.Map(vendor),
                DepositPrice = vehicle.provosionFee,
                
                
            } : null;



        private static FuelTypes GetEganisFuelType(string fuelTypeName)
        {
            switch (fuelTypeName)
            {
                default:
                case "Diesel":
                case "Dizel":
                    return FuelTypes.Diesel;
                case "Petrol":
                case "Kurşunsuz":
                case "Benzin":
                    return FuelTypes.Gasoline;
            }
        }

        private static TransmissionTypes GetEganisTransmissionType(string transmissionTypeName)
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

        private static PassangerQuantityTypes GetEganisPassangerQuantityType(int passangerQuantityName)
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

        private static BaggageQuantityTypes GetEganisBaggageQuantityType(int baggageQuantityName)
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
