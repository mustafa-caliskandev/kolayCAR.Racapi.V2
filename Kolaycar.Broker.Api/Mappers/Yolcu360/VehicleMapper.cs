using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Yolcu360
{
    public static class VehicleMapper
    {
        public static List<VendorVendor> Map(this List<Yolcu360CarListingAgencyResponseBase.Vendor> vendors, int vendorId)
        {
            var vendorVendors = new List<VendorVendor>();

            foreach (var vendor in vendors)
            {
                vendorVendors.Add(vendor.Map(vendorId));
            }

            return vendorVendors;
        }
        public static VendorVendor Map(this Yolcu360CarListingAgencyResponseBase.Vendor vendor, int vendorId)
        {
            return vendor != null ? new VendorVendor
            {
                VendorId = vendorId,
                VendorName = vendor.value,
                Active = false
            } : null;
        }
        public static List<Vehicle> Map(this List<Yolcu360CarListingAgencyResponseBase.Data> getVehicleResponse, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor,Agency agency)
        {
            var _vehicles = new List<Vehicle>();

            foreach (var vehicle in getVehicleResponse)
            {
                //int index = getVehicleResponse.IndexOf(vehicle);
                _vehicles.Add(vehicle.Map(additionalInformation, vendor, agency,index: getVehicleResponse.IndexOf(vehicle)));
            }

            return _vehicles;
        }
        public static Vehicle Map(this Yolcu360CarListingAgencyResponseBase.Data vehicle, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, Agency agency, int index = 0)
        {
            var totalPrice = vehicle.pricing.agencyPrice / 100;
            var dailyPrice = totalPrice / (double)vehicle.rentalPeriod.count;
            var oneWayPrice = vehicle.pricing.oneWayPrice / 100;
            var deliveryFee = vehicle.pricing.deliveryFee / 100;
            if (deliveryFee != 0 && deliveryFee != null)
            {
                oneWayPrice = (double)(oneWayPrice + deliveryFee);
            }
            string vendorName = vendor.ShowSubVendorLogo == true ? vehicle.vendor.name : vendor.VendorName;
            string vendorLogo = vendor.ShowSubVendorLogo == true ? vehicle.vendor.logoUrl : vendor.Logo;
            // int vendorid2 = additionalInformation.Vendor.VendorId
            return vehicle != null ? new Vehicle
            {
                IsAvailable = true,
                //VendorName = vehicle.vendor.name,
                VendorName = vendorName,
                VehicleId = index,
                BaseVendorId = vendor.VendorId,
                VehicleCode = vehicle.listingId,
                //VendorId = additionalInformation.Vendor.VendorId,
                VendorId = vehicle.vendor.id * 100,
                VehicleName = vehicle.car.brand.name + " " + vehicle.car.name,
                FuelType = GetYolcu360FuelType(vehicle.car.fuel),
                //FuelTypeName = vehicle.car.fuel,
                FuelTypeName = "Benzin",
                TransmissionType = GetYolcu360TransmissionType(vehicle.car.transmission),
                //TransmissionTypeName = vehicle.car.transmission,
                TransmissionTypeName = "Manuel",
                DepositPrice = (vendor.CreditType == CreditType.FullCredit && agency.CreditType == CreditType.FullCredit && vehicle.vendor.supportsFullCredit.ToBoolNullSafe()) ? 0 : vehicle.pricing.provision.ToFloatNullSafe() / 100,
                VendorMinimumDriverAge = vehicle.rules.driverAge,
                VendorMinimumDrivingLicenseAge = vehicle.rules.licenseYears,
                DailyPrice = dailyPrice.ToFloatNullSafe(),
                DailyPricePayNow = dailyPrice.ToFloatNullSafe(),
                TotalPrice = totalPrice.ToFloatNullSafe(),
                TotalPricePayNow = totalPrice.ToFloatNullSafe(),
                RentalDuration = vehicle.rentalPeriod.count,
                DailyKMLimit = vehicle.rules.totalRangeLimit / vehicle.rentalPeriod.count,
                TotalKMLimit = vehicle.rules.totalRangeLimit,
                VendorLogo = vendorLogo,
                SippCode = vehicle.car.sippCode,
                OneWayFee = oneWayPrice.ToFloatNullSafe(),
                VehicleImages = new List<VehicleImage>
                {
                    //new VehicleImage
                    //{
                    //    Url = vehicle.car.image.small
                    //},
                    new VehicleImage
                    {
                        Url = vehicle.car.image.medium
                    }
                    //new VehicleImage
                    //{
                    //    Url = vehicle.car.image.large
                    //},
                },
                VehicleType = VehicleTypes.None,
                VehicleCategoryType = GetYolcu360VehicleCategoryType(vehicle.car.@class),
                //VehicleCategoryTypeName = vehicle.car.@class,
                VehicleCategoryTypeName = "Ekonomik",
                PassangerQuantityType = GetPassangerQuantityTypes(vehicle.car.seats.ToStringNullSafe()),
                PassangerQuantityName = vehicle.car.seats.ToString() + " Personen",
                BaggageQuantityType = GetYolcu360BaggageQuantityType(vehicle.car.bigBagCount, vehicle.car.smallBagCount),
                BaggageQuantityName = (vehicle.car.bigBagCount + vehicle.car.smallBagCount).ToString() + " Bagaj",
                IsThereAirCondition = true,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                CreditType = vendor.CreditType == CreditType.FullCredit
                    && agency.CreditType == CreditType.FullCredit
                    && vehicle.vendor.supportsFullCredit.ToBoolNullSafe()
                        ? CreditType.FullCredit
                        : CreditType.Non,
                FullCredit = vendor.CreditType == CreditType.FullCredit
                    && agency.CreditType == CreditType.FullCredit
                    && vehicle.vendor.supportsFullCredit.ToBoolNullSafe(),
                VendorType = VendorTypes.Yolcu360,
                DeliveryType = GetYolcu360DeliveryType(vehicle.office.deliveryType),
                IsOffice = GetYolcu360DeliveryType(vehicle.office.deliveryType) == DeliveryType.FromOffice,
                DepositCreditCardRequired = !vehicle.vendor.supportsFullCredit.ToBoolNullSafe(),
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice
            } : null;
        }
        private static DeliveryType GetYolcu360DeliveryType(string type)
        {
            switch (type)
            {
                case "nonTerminalMeetAndGreet": return DeliveryType.MeetAndGreet;
                case "meetAndGreet": return DeliveryType.MeetAndGreet;
                case "inTerminalOffice": return DeliveryType.FromOffice;
                case "deliveredToAddress": return DeliveryType.MeetAndGreet;
                case "fromOffice": return DeliveryType.FromOffice;
                case "nonTerminalValet": return DeliveryType.MeetAndGreet;
                default: return DeliveryType.None;
            }
        }
        private static FuelTypes GetYolcu360FuelType(string fuelType)
        {
            switch (fuelType)
            {
                case "gas": return FuelTypes.Gasoline;
                case "diesel": return FuelTypes.Diesel;
                case "lpg": return FuelTypes.GasolineAndLPG;
                case "diesel-gas-mixed": return FuelTypes.GasolineAndDiesel;
                case "hybrid": return FuelTypes.HybritGasoline;
                default: return FuelTypes.None;
            }
        }
        private static TransmissionTypes GetYolcu360TransmissionType(string transmissionType)
        {
            switch (transmissionType)
            {
                case "manual": return TransmissionTypes.Manuel;
                case "automatic": return TransmissionTypes.Automatic;
                default: return TransmissionTypes.None;
            }
        }
        private static PassangerQuantityTypes GetPassangerQuantityTypes(string seat)
        {
            switch (seat)
            {
                case "1": return PassangerQuantityTypes.OnePerson;
                case "2": return PassangerQuantityTypes.TwoPerson;
                case "3": return PassangerQuantityTypes.ThreePerson;
                case "4": return PassangerQuantityTypes.FourPerson;
                case "5": return PassangerQuantityTypes.FivePerson;
                case "6": return PassangerQuantityTypes.SixPerson;
                case "7": return PassangerQuantityTypes.SevenPerson;
                case "8": return PassangerQuantityTypes.EightPerson;
                case "9": return PassangerQuantityTypes.NinePerson;
                case "10": return PassangerQuantityTypes.TenPerson;
                case "11": return PassangerQuantityTypes.ElevenPerson;
                case "12": return PassangerQuantityTypes.TwelvePerson;
                case "13": return PassangerQuantityTypes.ThirteenPerson;
                case "14": return PassangerQuantityTypes.FourteenPerson;
                case "15": return PassangerQuantityTypes.FifteenPerson;
                case "16": return PassangerQuantityTypes.SixteenPerson;
                case "17": return PassangerQuantityTypes.SeventeenPerson;
                case "18": return PassangerQuantityTypes.EighteenPerson;
                case "19": return PassangerQuantityTypes.NineteenPerson;
                case "20": return PassangerQuantityTypes.TwentyPerson;
                default: return PassangerQuantityTypes.FifteenPerson;
            }
        }
        private static VehicleCategoryTypes GetYolcu360VehicleCategoryType(string vehicleCategoryType)
        {
            switch (vehicleCategoryType)
            {
                case "economic": return VehicleCategoryTypes.Economic;
                case "intermediate": return VehicleCategoryTypes.Intermediate;
                case "suv": return VehicleCategoryTypes.SUV;
                case "comfort": return VehicleCategoryTypes.FullSize;
                case "premium": return VehicleCategoryTypes.Premium;
                case "luxury": return VehicleCategoryTypes.Luxury;
                case "compact": return VehicleCategoryTypes.Compact;
                case "compactElite": return VehicleCategoryTypes.CompactElite;
                case "standard": return VehicleCategoryTypes.Standard;
                case "van": return VehicleCategoryTypes.Minivan;
                case "mediumSuv": return VehicleCategoryTypes.SUV4x2;
                case "prestige": return VehicleCategoryTypes.Prestige;
                default: return VehicleCategoryTypes.None;
            }
        }
        private static BaggageQuantityTypes GetYolcu360BaggageQuantityType(int bigBagCount, int smallBagCount)
        {
            var total = bigBagCount + smallBagCount;
            switch (total)
            {
                case 1: return BaggageQuantityTypes.One;
                case 2: return BaggageQuantityTypes.Two;
                case 3: return BaggageQuantityTypes.Three;
                case 4: return BaggageQuantityTypes.Four;
                case 5: return BaggageQuantityTypes.Five;
                case 6: return BaggageQuantityTypes.Six;
                case 7: return BaggageQuantityTypes.Seven;
                case 8: return BaggageQuantityTypes.Eight;
                default: return BaggageQuantityTypes.None;
            }
        }
    }
}
