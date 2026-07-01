using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.Pandora2ResponseBase;

namespace KolayCAR.Broker.API.Mappers.Pandora2
{
    public static class VehicleMapper
    {
        public static Vehicle Map(this AvailableVehicle vehicle, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, bool extraPricePayToDelivery = false)
        {
            if (vehicle == null)
                return null;

            var rentalDuration = vehicle.DaysForPayment > 0 ? vehicle.DaysForPayment : additionalInformation.RentalDuration;
            var netTotalRent = Pandora2MapperHelper.ToMoney(vehicle.NetTotalRentAmount);
            var netTotalAmount = Pandora2MapperHelper.ToMoney(vehicle.NetTotalAmount);
            var dailyPrice = Pandora2MapperHelper.ToMoney(vehicle.DailyRentAmount);
            var feeAmount = netTotalAmount > netTotalRent ? netTotalAmount - netTotalRent : 0;

            //var mandatoryFees = vehicle.OtherFees.Where(e=> e.Mandatory)

            var vendorName = (bool)vendor.ShowSubVendorLogo ? vehicle.Supplier?.Name : vendor.VendorName;
            var vendorPhone = (bool)vendor.ShowSubVendorLogo ? vehicle.Supplier?.Phone : vendor.VendorPhone;
            var vendorLogo = (bool)vendor.ShowSubVendorLogo ? vehicle.Supplier?.Logo : vendor.Logo;

            return new Vehicle
            {
                VehicleId = Pandora2MapperHelper.ToStableId(vehicle.Id),
                VehicleCode = vehicle.Id,
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = vendorName,
                VendorPhone = vendorPhone,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VendorLogo = vendorLogo,
                VehicleName = vehicle.ModelName,
                SippCode = vehicle.SIPP,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                PickupLocationCode = additionalInformation.APIPickupLocationCode,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                ReturnLocationCode = additionalInformation.APIReturnLocationCode,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = rentalDuration,
                DailyPrice = dailyPrice,
                OneWayFee = feeAmount,
                TotalPrice = netTotalAmount,
                IsAvailable = netTotalAmount > 0,
                VehicleImages = !string.IsNullOrEmpty(vehicle.ModelImageURL)
                    ? new List<VehicleImage> { new VehicleImage { Url = vehicle.ModelImageURL } }
                    : new List<VehicleImage>(),
                VehicleType = VehicleTypes.None,
                TransmissionType = VehicleTransmissionType(vehicle.CarTransmissionType?.Name),
                TransmissionTypeName = vehicle.CarTransmissionType?.Name,
                VehicleCategoryType = VehicleCategoryTypes.None,
                VehicleCategoryTypeName = "",
                PassangerQuantityType = VehiclePassangerType(vehicle.PassengerCapacity),
                PassangerQuantityName = vehicle.PassengerCapacity,
                FuelType = VehicleFuelType(vehicle.FuelType?.Name),
                FuelTypeName = vehicle.FuelType?.Name,
                BaggageQuantityType = VehicleBaggageType((vehicle.SmallBagsCapacity ?? 0) + (vehicle.BigBagsCapacity ?? 0)),
                BaggageQuantityName = vehicle.SmallBagsCapacity.ToStringNullSafe(),
                IsThereAirCondition = vehicle.AirConditioning,
                VendorMinimumDriverAge = vehicle.MinDriverAge,
                VendorMinimumDrivingLicenseAge = vehicle.MinLicenseAge,
                DepositPrice = Pandora2MapperHelper.ToMoney(vehicle.Deposit),
                DailyPricePayNow = dailyPrice,
                TotalPricePayNow = netTotalAmount,
                Extras = vehicle.Extras.Map(extraPricePayToDelivery),
                FullCredit = vendor.CreditType == CreditType.FullCredit,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice,
                ApiVendorName = vehicle.Supplier?.Name,
                PickupLocationAddress = vehicle.Supplier?.Address,
                CurrencyCode = vehicle.Currency,
                Excess = (int)Pandora2MapperHelper.ToMoney(vehicle.CdwExcess)
            };
        }

        public static List<Vehicle> Map(this List<AvailableVehicle> vehicles, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, bool extraPricePayToDelivery = false)
        {
            var mappedVehicles = new List<Vehicle>();

            if (vehicles != null)
                foreach (var vehicle in vehicles)
                    mappedVehicles.Add(vehicle.Map(additionalInformation, vendor, extraPricePayToDelivery));

            return mappedVehicles;
        }

        private static TransmissionTypes VehicleTransmissionType(string name)
        {
            var normalized = name.ToStringNullSafe().ToLowerInvariant();
            return normalized.Contains("auto") ? TransmissionTypes.Automatic :
                normalized.Contains("manual") ? TransmissionTypes.Manuel :
                TransmissionTypes.None;
        }

        private static FuelTypes VehicleFuelType(string name)
        {
            var normalized = name.ToStringNullSafe().ToLowerInvariant();
            return normalized.Contains("diesel") ? FuelTypes.Diesel :
                normalized.Contains("gasoline") || normalized.Contains("petrol") ? FuelTypes.Gasoline :
                FuelTypes.None;
        }

        private static PassangerQuantityTypes VehiclePassangerType(string capacity)
        {
            if (string.IsNullOrWhiteSpace(capacity))
                return PassangerQuantityTypes.None;

            var totalCapacity = 0;
            foreach (var capacityPart in capacity.Split('+', System.StringSplitOptions.RemoveEmptyEntries))
            {
                if (!int.TryParse(capacityPart.Trim(), out var parsedCapacity))
                    return PassangerQuantityTypes.None;

                totalCapacity += parsedCapacity;
            }

            return totalCapacity switch
            {
                2 => PassangerQuantityTypes.TwoPerson,
                4 => PassangerQuantityTypes.FourPerson,
                5 => PassangerQuantityTypes.FivePerson,
                7 => PassangerQuantityTypes.SevenPerson,
                9 => PassangerQuantityTypes.NinePerson,
                _ => PassangerQuantityTypes.None
            };
        }

        private static BaggageQuantityTypes VehicleBaggageType(int capacity) =>
            capacity switch
            {
                1 => BaggageQuantityTypes.One,
                2 => BaggageQuantityTypes.Two,
                3 => BaggageQuantityTypes.Three,
                4 => BaggageQuantityTypes.Four,
                _ => BaggageQuantityTypes.None
            };
    }
}
