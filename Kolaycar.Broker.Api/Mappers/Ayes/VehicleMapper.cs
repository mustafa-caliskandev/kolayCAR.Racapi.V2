using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Ayes
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this AyesVehicle vehicle,
            AyesReservationInfo ayesReservationInfo,
            ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
            vehicle != null ? new Vehicle
            {
                VehicleId = vehicle.arac_id.ToIntNullSafe(),
                VehicleCode = vehicle.filo_id,
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = vehicle.baslik,
                VendorLogo = additionalInformation.Vendor.Logo,
                SippCode = string.Empty,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = ayesReservationInfo.toplam_gun,
                DailyPrice = vehicle.gunluk_ucret.ToFloatNullSafe(),
                OneWayFee = vehicle.drop_ucreti.ToFloatNullSafe(),
                TotalPrice = vehicle.toplam_ucret.ToFloatNullSafe(),
                IsAvailable = vehicle.gunluk_ucret.ToFloatNullSafe() > 0,
                VehicleImages = new List<VehicleImage>
                {
                    new VehicleImage
                    {
                        Url = vehicle.gorsel
                    }
                },
                VehicleType = VehicleTypes.None,
                TransmissionType = GetTransmissionType(vehicle.vites),
                TransmissionTypeName = vehicle.vites,
                VehicleCategoryType = vehicle.grup != null ? (VehicleCategoryTypes)vehicle.grup.id : VehicleCategoryTypes.None,
                VehicleCategoryTypeName = vehicle.grup != null ? vehicle.grup.isim : string.Empty,
                PassangerQuantityType = GetPassangerQuantityType(vehicle.kisi_sayisi.ToIntNullSafe()),
                PassangerQuantityName = vehicle.kisi_sayisi,
                FuelType = GetFuelType(vehicle.yakit),
                FuelTypeName = vehicle.yakit,
                BaggageQuantityType = GetBaggageQuantityType(vehicle.canta.ToIntNullSafe()),
                BaggageQuantityName = vehicle.canta,
                IsThereAirCondition = vehicle.klima.ToStringNullSafe().ToLower() == "var",
                VendorMinimumDriverAge = vehicle.surucu_yasi.ToIntNullSafe(),
                VendorMinimumDrivingLicenseAge = vehicle.ehliyet_yasi.ToIntNullSafe(),
                DailyPricePayNow = vehicle.gunluk_ucret.ToFloatNullSafe(),
                TotalPricePayNow = vehicle.toplam_ucret.ToFloatNullSafe(),
                DepositPrice = vehicle.provizyon,
                Extras = null,
                DailyKMLimit = vehicle.km_limit.ToIntNullSafe() / ((additionalInformation.ReturnDateTime - additionalInformation.PickupDateTime).TotalDays.ToIntNullSafe() == 0 ? 1 : (additionalInformation.ReturnDateTime - additionalInformation.PickupDateTime).TotalDays.ToIntNullSafe()),
                TotalKMLimit = vehicle.km_limit.ToIntNullSafe(),
                FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
            }
            : null;

        public static List<Vehicle> Map(this List<AyesVehicle> vehicles,
            AyesReservationInfo ayesReservationInfo,
            ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(ayesReservationInfo: ayesReservationInfo, additionalInformation: additionalInformation, vendor));

            return _vehicles;
        }

        public static Vehicle Map(this VehicleList vehicle) =>
       vehicle != null ? new Vehicle
       {
           VendorName = "Ayes",
           VehicleCode = vehicle.id,
           VehicleName = $"{vehicle.baslik} ({vehicle.yakit}-{vehicle.vites})",
           VendorMinimumDriverAge = vehicle.surucu_yasi.ToIntNullSafe(),
           VendorMinimumDrivingLicenseAge = vehicle.ehliyet_yasi.ToIntNullSafe(),
           SippCode = vehicle.sipp_kodu,
           FuelTypeName = vehicle.yakit,
           TransmissionTypeName = vehicle.vites,
           DepositPrice = vehicle.provizyon
       }
       : null;

        public static List<Vehicle> Map(this List<VehicleList> vehicles)
        {
            var _vehicles = new List<Vehicle>();
            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map());

            return _vehicles;
        }
        private static FuelTypes GetFuelType(string fuelTypeName)
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

        private static TransmissionTypes GetTransmissionType(string transmissionTypeName)
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

        private static BaggageQuantityTypes GetBaggageQuantityType(int baggageQuantityName)
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
    }
}
