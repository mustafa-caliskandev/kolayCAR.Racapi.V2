using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.KolayCARBroker
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this Vehicle vehicle,
             ResponseReservationStepsAdditionalInformation additionalInformation,
             ReservationToken reservationToken = null)
        {
            var _vehicle = vehicle;

            _vehicle.VehicleCode = vehicle.VehicleId == 0 ? vehicle.VehicleCode : vehicle.VehicleId.ToStringNullSafe();
            _vehicle.VehicleImages = vehicle.VehicleImages;
            _vehicle.VendorId = additionalInformation.Vendor.VendorId;
            _vehicle.BaseVendorId = vehicle.VendorId;
            _vehicle.VendorName = additionalInformation.Vendor.VendorName;
            _vehicle.VendorPhone = additionalInformation.Vendor.VendorPhone;
            _vehicle.VendorEmail = additionalInformation.Vendor.VendorEmail;
            _vehicle.VendorLogo = additionalInformation.Vendor.Logo;
            _vehicle.PickupLocationId = additionalInformation.PickupLocationId;
            _vehicle.PickupLocationName = additionalInformation.PickupLocationName;
            _vehicle.ReturnLocationId = additionalInformation.ReturnLocationId;
            _vehicle.ReturnLocationName = additionalInformation.ReturnLocationName;
            _vehicle.PickupDateTime = additionalInformation.PickupDateTime;
            _vehicle.ReturnDateTime = additionalInformation.ReturnDateTime;
            _vehicle.ReservationToken = EncryptionHelper.EncryptAES256(reservationToken.ToStringNullSafe());
            _vehicle.TotalKMLimit = _vehicle.TotalKMLimit != null ? _vehicle.TotalKMLimit.ToIntNullSafe() : (int?)null;
            _vehicle.ActivePaymentTypes = new List<PaymentTypes>();
            _vehicle.CreditType = vehicle.CreditType != CreditType.Non
                ? vehicle.CreditType
                : additionalInformation.Vendor.CreditType == CreditType.FullCredit && vehicle.FullCredit.ToBoolNullSafe()
                    ? CreditType.FullCredit
                    : CreditType.Non;
            _vehicle.FullCredit = _vehicle.CreditType == CreditType.FullCredit;
            _vehicle.ApiDailyPrice = vehicle.DailyPrice;
            _vehicle.RentalWorkingTypes = additionalInformation.Vendor.RentalWorkingType;
            _vehicle.ProfitMarkupDailyPrice = additionalInformation.Vendor.ProfitMarkupDailyPrice;
            return _vehicle;
        }

        public static List<Vehicle> Map(this List<Vehicle> vehicles,
            ResponseReservationStepsAdditionalInformation additionalInformation)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                {
                    if (vehicle != null)
                        _vehicles.Add(vehicle.Map(additionalInformation: additionalInformation));
                }

            return _vehicles;
        }

        public static Vehicle Map(this Vehicle vehicle) =>
        vehicle != null ? new Vehicle
        {
            VendorName = "KolayCARBroker",
            VehicleCode = vehicle.VehicleId.ToStringNullSafe(),
            VehicleName = $"{vehicle.VehicleName} {vehicle.VehicleTypeName} ({vehicle.FuelTypeName}-{vehicle.TransmissionTypeName})",
            SippCode = vehicle.SippCode,
            FuelTypeName = vehicle.FuelTypeName,
            TransmissionTypeName = vehicle.TransmissionTypeName
        }
        : null;

        public static List<Vehicle> Map(this List<Vehicle> vehicles)
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
