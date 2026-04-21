using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Rigorent
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this RigorentVehicle vehicle,
            ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
            vehicle != null ? new Vehicle
            {
                VehicleId = vehicle.Arac_Kayit_No.ToIntNullSafe(),
                VehicleCode = vehicle.Arac_Kayit_No,
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = $"{vehicle.Arac_Marka} {vehicle.Arac_Tipi}",
                VendorLogo = additionalInformation.Vendor.Logo,
                SippCode = vehicle.SIPP,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = vehicle.Kira_Gun.ToIntNullSafe(),
                DailyPrice = vehicle.Gunluk_Kira.ToFloatNullSafe(),
                OneWayFee = vehicle.Drop_Bedeli.ToFloatNullSafe(),
                TotalPrice = vehicle.Genel_Toplam.ToFloatNullSafe(),
                IsAvailable = vehicle.Gunluk_Kira.ToFloatNullSafe() > 0,
                VehicleImages = new List<VehicleImage>
                {
                    new VehicleImage
                    {
                        Url = !string.IsNullOrEmpty(vehicle.Arac_Resim) ?
                        $"https://mycarturassist.com/arabalar/{vehicle.Arac_Resim}" : string.Empty
                    }
                },
                VehicleType = VehicleTypes.None,
                TransmissionType = GetTransmissionType(vehicle.Arac_Vites),
                TransmissionTypeName = vehicle.Arac_Vites,
                VehicleCategoryType = VehicleCategoryTypes.None,
                PassangerQuantityType = GetPassangerQuantityType(vehicle.Arac_Koltuk.ToIntNullSafe()),
                PassangerQuantityName = vehicle.Arac_Koltuk.ToString(),
                FuelType = GetFuelType(vehicle.Arac_Yakit),
                FuelTypeName = vehicle.Arac_Yakit,
                BaggageQuantityType = GetBaggageQuantityType(vehicle.Arac_K_Bagaj.ToIntNullSafe() + vehicle.Arac_B_Bagaj.ToIntNullSafe()),
                BaggageQuantityName = (vehicle.Arac_K_Bagaj.ToIntNullSafe() + vehicle.Arac_B_Bagaj.ToIntNullSafe()).ToString(),
                IsThereAirCondition = true,
                VendorMinimumDriverAge = vehicle.Yas.ToIntNullSafe(),
                VendorMinimumDrivingLicenseAge = vehicle.Ehliyet.ToIntNullSafe(),
                DailyPricePayNow = vehicle.Gunluk_Kira.ToFloatNullSafe(),
                TotalPricePayNow = vehicle.Genel_Toplam.ToFloatNullSafe(),
                DepositPrice = 0,
                FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice,
                Extras = new List<Extra>
                {
                    new Extra
                    {
                        ExtraId = 1,
                        ExtraCode = "B_Koltuk",
                        ExtraName = "Bebek koltuğu",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Bebek_Koltugu_Bedeli.ToFloatNullSafe()
                    },
                    new Extra
                    {
                        ExtraId = 2,
                        ExtraCode = "Navigasyon",
                        ExtraName = "Navigasyon",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Navigasyon_Bedeli.ToFloatNullSafe()
                    },

                    //Aşağıdaki ek ürünler Rigorentte api üzerinden servis edilmiyor
                    //new Extra
                    //{
                    //    ExtraId = 3,
                    //    ExtraCode = "Ozel_Sofor_Bedeli",
                    //    ExtraName = "Şoför",
                    //    ExtraRentalType = ExtraRentalTypes.PerRental,
                    //    ExtraQuantityIncreasable = false,
                    //    Price = vehicle.Ozel_Sofor_Bedeli.ToFloatNullSafe()
                    //},
                    //new Extra
                    //{
                    //    ExtraId = 4,
                    //    ExtraCode = "LCF_Bedeli",
                    //    ExtraName = "Lastik-Cam-Far Sigortası",
                    //    ExtraRentalType = ExtraRentalTypes.PerRental,
                    //    ExtraQuantityIncreasable = false,
                    //    Price = vehicle.LCF_Bedeli.ToFloatNullSafe()
                    //},
                    //new Extra
                    //{
                    //    ExtraId = 5,
                    //    ExtraCode = "SCDW_Bedeli",
                    //    ExtraName = "Süper Güvence",
                    //    ExtraRentalType = ExtraRentalTypes.PerRental,
                    //    ExtraQuantityIncreasable = false,
                    //    Price = vehicle.SCDW_Bedeli.ToFloatNullSafe()
                    //},
                    //new Extra
                    //{
                    //    ExtraId = 6,
                    //    ExtraCode = "Muaifiyetsiz_Bedeli",
                    //    ExtraName = "Muafiyatsiz Sigorta",
                    //    ExtraRentalType = ExtraRentalTypes.PerRental,
                    //    ExtraQuantityIncreasable = false,
                    //    Price = vehicle.Muaifiyetsiz_Bedeli.ToFloatNullSafe()
                    //},
                    //new Extra
                    //{
                    //    ExtraId = 7,
                    //    ExtraCode = "Km_250_Bedeli",
                    //    ExtraName = "Ekstra 250km",
                    //    ExtraRentalType = ExtraRentalTypes.PerRental,
                    //    ExtraQuantityIncreasable = false,
                    //    Price = vehicle.Km_250_Bedeli.ToFloatNullSafe()
                    //},
                    //new Extra
                    //{
                    //    ExtraId = 8,
                    //    ExtraCode = "Km_500_Bedeli",
                    //    ExtraName = "Ekstra 500km",
                    //    ExtraRentalType = ExtraRentalTypes.PerRental,
                    //    ExtraQuantityIncreasable = false,
                    //    Price = vehicle.Km_500_Bedeli.ToFloatNullSafe()
                    //},
                    //new Extra
                    //{
                    //    ExtraId = 9,
                    //    ExtraCode = "Km_1000_Bedeli",
                    //    ExtraName = "Ekstra 1000km",
                    //    ExtraRentalType = ExtraRentalTypes.PerRental,
                    //    ExtraQuantityIncreasable = false,
                    //    Price = vehicle.Km_1000_Bedeli.ToFloatNullSafe()
                    //},
                }
            }
            : null;

        public static List<Vehicle> Map(this List<RigorentVehicle> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(additionalInformation: additionalInformation, vendor));

            return _vehicles;
        }

        public static Vehicle Map(this RigorentVehicleListItem vehicle) =>
            vehicle != null ? new Vehicle
            {
                VendorName = "Rigorent",
                VehicleCode = vehicle.Kayit_No,
                VehicleName = $"{vehicle.Marka} {vehicle.Tipi} ({vehicle.Yakit_Turu}-{vehicle.Vites}-{vehicle.SIPP})",
                SippCode = vehicle.SIPP,
                FuelTypeName = vehicle.Yakit_Turu,
                TransmissionTypeName = vehicle.Vites
            }
            : null;

        public static List<Vehicle> Map(this List<RigorentVehicleListItem> vehicles)
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
                case "Düz":
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
