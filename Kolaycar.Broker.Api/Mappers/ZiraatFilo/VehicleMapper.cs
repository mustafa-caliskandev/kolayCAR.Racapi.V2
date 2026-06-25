using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.ZiraatFilo
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this Domain.Models.Response.ZiraatFiloResposeBase.Group vehicle, string vendorName) =>
          vehicle != null ? new Vehicle
          {
              VendorName = vendorName,
              VehicleId = 0,
              VehicleCode = vehicle.Group_ID,
              VehicleName = $"{vehicle.Group_Name} - {vehicle.Group_Str}",
              FuelType = GetFuelType(vehicle.Fuel),
              FuelTypeName = vehicle.Fuel,
              TransmissionType = GetTransmissionType(vehicle.Transmission),
              TransmissionTypeName = vehicle.Transmission,
              SippCode = vehicle.SIPP,
              DepositPrice = vehicle.Provision.ToFloatNullSafe(),
              VendorMinimumDriverAge = vehicle.Driver_Age.ToIntNullSafe(),
              VendorMinimumDrivingLicenseAge = vehicle.Driving_License_Age.ToIntNullSafe()
          }
          : null;
        public static List<Vehicle> Map(this List<Domain.Models.Response.ZiraatFiloResposeBase.Group> vehicles, string vendorName)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(vendorName));

            return _vehicles;
        }
        public static FuelTypes GetFuelType(string fuelTypeName)
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
        public static TransmissionTypes GetTransmissionType(string transmissionTypeName)
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
        public static Vehicle Map(this ZiraatFiloResposeBase.Car vehicle,
           ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
           vehicle != null ? new Vehicle
           {
               VehicleId = vehicle.Group_ID.ToIntNullSafe(),
               VehicleCode = vehicle.Group_ID,
               VendorId = additionalInformation.Vendor.VendorId,
               VendorName = additionalInformation.Vendor.VendorName,
               VendorPhone = additionalInformation.Vendor.VendorPhone,
               VendorEmail = additionalInformation.Vendor.VendorEmail,
               VehicleName = vehicle.Car_Name,
               VendorLogo = additionalInformation.Vendor.Logo,
               SippCode = vehicle.SIPP, //vehicle.SIPP,
               PickupLocationId = additionalInformation.PickupLocationId,
               PickupLocationName = additionalInformation.PickupLocationName,
               ReturnLocationId = additionalInformation.ReturnLocationId,
               ReturnLocationName = additionalInformation.ReturnLocationName,
               PickupDateTime = additionalInformation.PickupDateTime,
               ReturnDateTime = additionalInformation.ReturnDateTime,
               RentalDuration = vehicle.Days.ToIntNullSafe(),
               DailyPrice = vehicle.Daily_Rental.ToFloatNullSafe(),
               OneWayFee = vehicle.Office_Price.ToFloatNullSafe(),
               TotalPrice = vehicle.Total_Rental.ToFloatNullSafe(),
               TotalKMLimit = vehicle.Km_Limit.ToIntNullSafe(),
               IsAvailable = vehicle.Total_Rental.ToFloatNullSafe() > 0,
               VehicleType = VehicleTypes.None,
               TransmissionType = GetTurevracTransmissionType(vehicle.Transmission),
               TransmissionTypeName = string.Empty,
               VehicleCategoryType = VehicleCategoryTypes.None,
               PassangerQuantityType = GetTurevracPassangerQuantityType(vehicle.Chairs.ToIntNullSafe()),
               PassangerQuantityName = vehicle.Chairs,
               FuelType = GetTurevracFuelType(vehicle.Fuel),
               FuelTypeName = vehicle.Fuel,
               IsThereAirCondition = true,
               VendorMinimumDriverAge = vehicle.Driver_Age.ToIntNullSafe(),
               VendorMinimumDrivingLicenseAge = vehicle.Driving_License_Age.ToIntNullSafe(),
               DailyPricePayNow = vehicle.Daily_Rental.ToFloatNullSafe(),
               TotalPricePayNow = vehicle.Total_Rental.ToFloatNullSafe(),
               DepositPrice = vehicle.Provision.ToFloatNullSafe(),
               FullCredit = vendor.CreditType == CreditType.FullCredit,
               RentalWorkingTypes = vendor.RentalWorkingType,
               ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice,
               Extras = new List<Extra>
               {
                    new Extra
                    {
                        ExtraId = 1,
                        ExtraCode = "XKP",
                        ExtraName = "Maksimum Koruma Paketi",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.XKP_Daily.ToFloatNullSafe(),
                        ExtraDescription =  "Maksimum Hasar Sigortası, Lastik-Cam Sigortası, Ferdi Kaza Sigortası, İhtiyari Mali Mesuliyet Sigortası ve İptal Güvence Paketini içeren koruma paketidir."
                    },
                    new Extra
                    {
                        ExtraId = 2,
                        ExtraCode = "Cancel",
                        ExtraName = "İptal Güvence",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Cancel_Daily.ToFloatNullSafe(),
                        ExtraDescription = "Kira başlangıç saatinin 1 saat öncesine kadar rezervasyonunuzu koşulsuz ve kesintisiz iptal edebilirsiniz."
                    },
                    new Extra
                    {
                        ExtraId = 3,
                        ExtraCode = "Diger",
                        ExtraName = "Diğer",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Diger_Daily.ToFloatNullSafe(),
                        ExtraDescription = "Ek Hizmet Diğer"
                    },
                    new Extra
                    {
                        ExtraId = 4,
                        ExtraCode = "Child_Seat",
                        ExtraName = "Çocuk Koltuğu",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Child_Seat_Daily.ToFloatNullSafe(),
                        ExtraDescription = "Seçilen ek ürün için ofis müsaitliğine göre ofiste ödeme alınacaktır."
                    },
                    new Extra
                    {
                        ExtraId = 5,
                        ExtraCode = "SKP",
                        ExtraName = "Süper Koruma Paketi",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.SKP_Daily.ToFloatNullSafe(),
                        ExtraDescription = "Süper Mini Hasar Sigortası, Lastik-Cam Sigortası ve İptal Güvence Paketini içeren koruma paketidir."
                    },
                    new Extra
                    {
                        ExtraId = 6,
                        ExtraCode = "IMM",
                        ExtraName = "İhtiyari Mali Mesuliyet Paketi",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.IMM_Daily.ToFloatNullSafe(),
                        ExtraDescription = "Zorunlu mali mesuliyet sigortasına ilave olarak, üçüncü şahıslara karşı maksimum 1.000.000 TL tutarında güvence sağlar."
                    },
                    new Extra
                    {
                        ExtraId = 7,
                        ExtraCode = "Young_Drive",
                        ExtraName = "Genç Sürücü Paketi",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Young_Drive_Daily.ToFloatNullSafe(),
                        ExtraDescription = "Talep edilen aracın yaş sınırına takılması durumunda, Mini Hasar Sigortası ücrete dahildir."
                    },
                    new Extra
                    {
                        ExtraId = 8,
                        ExtraCode = "PAI",
                        ExtraName = "Ferdi Kaza Güvence Paketi",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.PAI_Daily.ToFloatNullSafe(),
                        ExtraDescription = "Araç içerisinde bulunan kişilerin kaza nedeniyle ölümü veya sürekli sakatlık durumunda 10.000 TL’ye kadar tazmin hakkı tanıyan güvencedir. Ferdi Kaza Güvencesi kapsamında herhangi bir hastane veya sağlık gideri yer almamaktadır."
                    },
                    new Extra
                    {
                        ExtraId = 9,
                        ExtraCode = "Max_Assurance",
                        ExtraName = "Maksimum Hasar Güvence Paketi",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Max_Assurance_Daily.ToFloatNullSafe(),
                        ExtraDescription = "Araçta oluşabilecek kaporta hasarları 5000 TL’ye kadar rapor bulunmaksızın beyan ile karşılama hakkı sunar."
                    },
                    new Extra
                    {
                        ExtraId = 10,
                        ExtraCode = "Additional_Driver",
                        ExtraName = "Ek Sürücü",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Additional_Driver_Daily.ToFloatNullSafe(),
                        ExtraDescription = "Aracın, kiralayan şahıs dışındaki kişi ve / veya kişilerce kullanılabilmesini sağlamaktadır."
                    },
                    new Extra
                    {
                        ExtraId = 11,
                        ExtraCode = "Winter_Tire",
                        ExtraName = "Kış Lastiği",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Winter_Tire_Daily.ToFloatNullSafe(),
                        ExtraDescription = "Seçilen ek ürün için müsaitliğe göre ofiste ekleme yapılacak ve ödeme alınacaktır."
                    },
                    new Extra
                    {
                        ExtraId = 12,
                        ExtraCode = "MKP",
                        ExtraName = "Mini Koruma Paketi",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.MKP_Daily.ToFloatNullSafe(),
                        ExtraDescription = "Araçta oluşabilecek park hasarları olarak adlandırdığımız sürtme çizik göçük ezik vb. hasarların yazılı beyan ile teminat kapsamına alınmasını maddi hasarları 4.500 TL’ye kadar rapor bulunmaksızın beyan ile karşılama hakkı"
                    },
                    new Extra
                    {
                        ExtraId = 13,
                        ExtraCode = "Super_Mini_Damage_Insurance",
                        ExtraName = "Süper Mini Hasar Güvence Paketi",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Super_Mini_Damage_Insurance_Daily.ToFloatNullSafe(),
                        ExtraDescription = "Araçta oluşabilecek kaporta hasarlarını 3500 TL’ye kadar rapor bulunmaksızın beyan ile karşılama hakkı sunar."
                    },
                    new Extra
                    {
                        ExtraId = 14,
                        ExtraCode = "Mini_Damage_Insurance",
                        ExtraName = "Mini Hasar Güvence Paketi",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.Mini_Damage_Insurance_Daily.ToFloatNullSafe(),
                        ExtraDescription = "Araçta oluşabilecek kaporta hasarlarını 2500 TL’ye kadar rapor bulunmaksızın beyan ile karşılama hakkı sunar."
                    },
                    new Extra
                    {
                        ExtraId = 15,
                        ExtraCode = "LCF",
                        ExtraName = "Lastik/Cam Güvence Paketi",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                        Price = vehicle.LCF_Daily.ToFloatNullSafe(),
                        ExtraDescription = "Araçtaki standart kasko sigortasının kapsamadığı lastik ve cama verilen hasar maliyeti sorumluluğu, lastik ve cam sigortası ile 1 adet için güvence altına alınmaktadır."
                    }
               }
           }
           : null;

        public static List<Vehicle> Map(this List<ZiraatFiloResposeBase.Car> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                    _vehicles.Add(vehicle.Map(additionalInformation, vendor));

            return _vehicles;
        }

        public static FuelTypes GetTurevracFuelType(string fuelTypeName)
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

        public static TransmissionTypes GetTurevracTransmissionType(string transmissionTypeName)
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

        public static PassangerQuantityTypes GetTurevracPassangerQuantityType(int passangerQuantityName)
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

        public static BaggageQuantityTypes GetTurevracBaggageQuantityType(int baggageQuantityName)
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
