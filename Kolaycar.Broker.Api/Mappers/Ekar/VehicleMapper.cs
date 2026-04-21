using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Ekar
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this EkarResponseBase.Musaitlik vehicle,
            ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
            vehicle != null ? new Vehicle
            {
                VehicleId = vehicle.Kayit_No.ToIntNullSafe(),
                VehicleCode = vehicle.Kayit_No.ToString(),
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = vehicle.Grup_Adi,
                VendorLogo = additionalInformation.Vendor.Logo,
                SippCode = vehicle.SIPP,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = vehicle.Kira_Gun.ToIntNullSafe(),
                DailyPrice = vehicle.G_Fiyat.ToFloatNullSafe(),
                OneWayFee = vehicle.Drop_Mesafe.ToFloatNullSafe(),
                TotalPrice = vehicle.T_Fiyat.ToFloatNullSafe(),
                IsAvailable = vehicle.G_Fiyat.ToFloatNullSafe() > 0,
                VehicleImages = new List<VehicleImage>
                {
                    new VehicleImage
                    {
                        Url = vehicle.Arac_Resmi
                    }
                },
                VehicleType = VehicleTypes.None,
                TransmissionType = TransmissionTypes.None,
                VehicleCategoryType = VehicleCategoryTypes.None,
                PassangerQuantityType = PassangerQuantityTypes.None,
                FuelType = FuelTypes.None,
                BaggageQuantityType = BaggageQuantityTypes.None,
                IsThereAirCondition = true,
                VendorMinimumDriverAge = vehicle.Surucu_Yas.ToIntNullSafe(),
                VendorMinimumDrivingLicenseAge = vehicle.Ehliyet_Yil.ToIntNullSafe(),
                DailyPricePayNow = vehicle.G_Fiyat.ToFloatNullSafe(),
                TotalPricePayNow = vehicle.T_Fiyat.ToFloatNullSafe(),
                DepositPrice = vehicle.Provizyon_Ucreti.ToFloatNullSafe(),
                TotalKMLimit = !string.IsNullOrEmpty(vehicle.KM_Siniri) ? vehicle.KM_Siniri.ToIntNullSafe() : (int?)null,
                FullCredit = vendor.CreditType == CreditType.FullCredit ? true : false,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice,
                Extras = new List<Extra>
                {
                    new Extra
                    {
                        ExtraId = 1,
                        ExtraCode = "CDW_Hesap_Fiyat",
                        ExtraName = "Mini Hasar Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.CDW_Hesap_Fiyat
                    },
                    new Extra
                    {
                        ExtraId = 2,
                        ExtraCode = "SCDW_Hesap_Fiyat",
                        ExtraName = "Full Kasko (SCDW)",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.SCDW_Hesap_Fiyat
                    },
                    new Extra
                    {
                        ExtraId = 3,
                        ExtraCode = "LCF_Hesap_Fiyat",
                        ExtraName = "Lastik-Cam-Far Sigortası (LCF)",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.LCF_Hesap_Fiyat
                    },
                    new Extra
                    {
                        ExtraId = 4,
                        ExtraCode = "PAI_Hesap_Fiyat",
                        ExtraName = "Ferdi Kaza Sigortası (PAI)",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.PAI_Hesap_Fiyat
                    },
                    new Extra
                    {
                        ExtraId = 5,
                        ExtraCode = "Navigasyon_Hesap_Fiyat",
                        ExtraName = "Navigasyon (GPS)",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Navigasyon_Hesap_Fiyat
                    },
                    new Extra
                    {
                        ExtraId = 6,
                        ExtraCode = "Ek_Surucu_Hesap_Fiyat",
                        ExtraName = "Ek Şoför",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Ek_Surucu_Hesap_Fiyat
                    },
                    new Extra
                    {
                        ExtraId = 7,
                        ExtraCode = "Bebek_Koltuk_Hesap_Fiyat",
                        ExtraName = "Bebek Koltuğu (0 – 2 yaş)",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Bebek_Koltuk_Hesap_Fiyat
                    },
                }
            }
            : null;

        public static List<Vehicle> Map(this List<EkarResponseBase.Musaitlik> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(additionalInformation: additionalInformation, vendor));

            return _vehicles;
        }

        public static Vehicle Map(this EkarResponseBase.Arac vehicle, bool getLongVehicleName = false) =>
            vehicle != null ? new Vehicle
            {
                VendorName = "Ekar",
                VehicleCode = vehicle.Grup_Kodu,
                VehicleName = !getLongVehicleName ? $"{vehicle.Marka} {vehicle.Tipi} ({vehicle.Yakit_Turu}-{vehicle.Vites}-{vehicle.SIPP})" : $"{vehicle.Marka} {vehicle.Tipi}",
                SippCode = vehicle.SIPP,
                FuelType = GetFuelType(vehicle.Yakit_Turu),
                FuelTypeName = vehicle.Yakit_Turu,
                TransmissionType = GetTransmissionType(vehicle.Vites),
                TransmissionTypeName = vehicle.Vites
            }
            : null;

        public static List<Vehicle> Map(this List<EkarResponseBase.Arac> vehicles, bool getLongVehicleName = false)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(getLongVehicleName: getLongVehicleName));

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
                case "Kurşunsuz":
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
    }
}
