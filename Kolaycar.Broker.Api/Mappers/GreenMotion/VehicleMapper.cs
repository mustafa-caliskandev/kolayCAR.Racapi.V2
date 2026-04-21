using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KolayCAR.Broker.API.Mappers.GreenMotion
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this GreenMotionVehicle vehicle,
            ResponseReservationStepsAdditionalInformation additionalInformation,
            int rentalDuration,
            Vendor vendor,
            bool fullCredit,
            float onewayFee = 0) =>
            vehicle != null ? new Vehicle
            {
                VehicleId = vehicle._id.ToIntNullSafe(),
                VehicleCode = vehicle.groupName.ToStringNullSafe(),
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = additionalInformation.Vendor.VendorName,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VehicleName = vehicle._name,
                VendorLogo = additionalInformation.Vendor.Logo,
                SippCode = vehicle.acriss,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = rentalDuration,
                DailyPrice = additionalInformation.Vendor.CreditType == CreditType.FullCredit ? vehicle.fullcredit.__text.ToFloatNullSafe() / rentalDuration : vehicle.total.__text.ToFloatNullSafe() / rentalDuration,
                //OneWayFee = onewayFee == 0 ? 0 : onewayFee,
                TotalPrice = additionalInformation.Vendor.CreditType == CreditType.FullCredit ? vehicle.fullcredit.__text.ToFloatNullSafe() : vehicle.total.__text.ToFloatNullSafe(),
                IsAvailable = additionalInformation.Vendor.CreditType == CreditType.FullCredit ? vehicle.fullcredit.__text.ToFloatNullSafe() > 0 : vehicle.total.__text.ToFloatNullSafe() > 0,
                VehicleImages = new List<VehicleImage>
                {
                    new VehicleImage
                    {
                        Url = HttpUtility.UrlDecode(vehicle._image)
                    }
                },
                VehicleType = VehicleTypes.None,
                TransmissionType = GetTransmissionType(vehicle.transmission),
                TransmissionTypeName = vehicle.transmission,
                VehicleCategoryType = VehicleCategoryTypes.None,
                PassangerQuantityType = GetPassangerQuantityType(vehicle.adults.ToIntNullSafe() + vehicle.children.ToIntNullSafe()),
                PassangerQuantityName = (vehicle.adults.ToIntNullSafe() + vehicle.children.ToIntNullSafe()).ToString(),
                FuelType = GetFuelType(vehicle.fuel),
                FuelTypeName = vehicle.fuel,
                BaggageQuantityType = GetBaggageQuantityType(vehicle.luggageSmall.ToIntNullSafe() + vehicle.luggageMed.ToIntNullSafe() + vehicle.luggageLarge.ToIntNullSafe()),
                BaggageQuantityName = (vehicle.luggageSmall.ToIntNullSafe() + vehicle.luggageMed.ToIntNullSafe() + vehicle.luggageLarge.ToIntNullSafe()).ToString(),
                IsThereAirCondition = vehicle.airConditioning.ToBoolNullSafe(),
                VendorMinimumDriverAge = null,
                VendorMinimumDrivingLicenseAge = null,
                DailyPricePayNow = additionalInformation.Vendor.CreditType == CreditType.FullCredit ? vehicle.fullcredit.__text.ToFloatNullSafe() / rentalDuration : vehicle.total.__text.ToFloatNullSafe() / rentalDuration,
                TotalPricePayNow = additionalInformation.Vendor.CreditType == CreditType.FullCredit ? vehicle.fullcredit.__text.ToFloatNullSafe() : vehicle.total.__text.ToFloatNullSafe(),
                DepositPrice = vehicle.deposit.ToFloatNullAvailable(),
                Extras = new List<Extra>(),
                FullCredit = (vendor.CreditType == CreditType.FullCredit && fullCredit) ? true : false,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
                //TotalKmLimit = vehicle.mileage.ToIntNullSafe()
            }
            : null;

        public static List<Vehicle> Map(this GreenMotionResponse getVehiclesResponse,
            ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, float onewayFee = 0) =>
            getVehiclesResponse.vehicles.vehicle.Map(
                additionalInformation,
                getVehiclesResponse,
                vendor,
                getVehiclesResponse.full_credit.ToBoolNullSafe(),
                onewayFee: onewayFee);

        public static List<Vehicle> Map(this List<GreenMotionVehicle> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation,
            GreenMotionResponse getVehiclesResponse, Vendor vendor, bool fullCredit = false,
            ReservationToken reservationToken = null, float onewayFee = 0)
        {
            var _vehicles = new List<Vehicle>();

            if (vehicles != null && vehicles.Count != 0)
                foreach (var vehicle in vehicles)
                {
                    var vehicleItem = vehicle.Map(
                        additionalInformation: additionalInformation,
                        rentalDuration: getVehiclesResponse.days.ToIntNullSafe(),
                        vendor,
                        fullCredit,
                        onewayFee: onewayFee);

                    vehicleItem.Extras = vehicle.insurance_options.Map();

                    var additionalExtras = getVehiclesResponse.optionalextras.Map().Where(x => x.ExtraType == AdditionalProductTypes.Extra).ToList();

                    vehicleItem.Extras = vehicleItem.Extras.Union(additionalExtras).Distinct(new CompareExtras()).ToList();

                    _vehicles.Add(vehicleItem);
                }

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
                    return FuelTypes.Gasoline;
            }
        }

        public static TransmissionTypes GetTransmissionType(string transmissionTypeName)
        {
            switch (transmissionTypeName)
            {
                default:
                case "Auto Unspecified Drive":
                    return TransmissionTypes.Automatic;
                case "Manual Unspecified Drive":
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

        class CompareExtras : IEqualityComparer<Extra>
        {
            public bool Equals(Extra x, Extra y)
            {
                return x.ExtraCode == y.ExtraCode;
            }
            public int GetHashCode(Extra codeh)
            {
                return codeh.ExtraCode.GetHashCode();
            }
        }
    }
}
