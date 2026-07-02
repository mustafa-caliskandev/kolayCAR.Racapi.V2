using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;
using static KolayCAR.Broker.Domain.Models.Response.Yolcu360v2.Yolcu360v2VehicleResponseBase;

namespace KolayCAR.Broker.API.Mappers.Yolcu360v2
{
    public static class VehicleMapper
    {
        public static List<Vehicle> Map(this List<Yolcu360v2Vehicle> vehicleList, ResponseReservationStepsAdditionalInformation additionalInformation, Domain.Models.Vendor vendor, CurrencyTypes baseVendorCurrencyType, List<ExchangeRates> exchangeRates, IReadOnlyDictionary<string, VendorVendor> vendorVendorByName)
        {
            var _vehicles = new List<Vehicle>();
            int i = 0;
            foreach (var vehicle in vehicleList)
            {
                i++;
                _vehicles.Add(vehicle.Map(additionalInformation, vendor, baseVendorCurrencyType, exchangeRates, vendorVendorByName, i));
            }

            return _vehicles;
        }
        public static Vehicle Map(this Yolcu360v2Vehicle vehicle, ResponseReservationStepsAdditionalInformation additionalInformation, Domain.Models.Vendor vendor, CurrencyTypes baseVendorCurrencyType, List<ExchangeRates> exchangeRates, IReadOnlyDictionary<string, VendorVendor> vendorVendorByName, int index)
        {
            if (vehicle.rentalDurationInDays == 0)
                return null;

            var agency = additionalInformation.Agency;

            var pickupDay = ((int)additionalInformation.PickupDateTime.DayOfWeek + 6) % 7 + 1;
            var pickupTime = vehicle.appointment.checkInOffice.openingHours.FirstOrDefault(e => e.dayOfWeek == pickupDay);

            var returnDay = ((int)additionalInformation.ReturnDateTime.DayOfWeek + 6) % 7 + 1;
            var returnTime = vehicle.appointment.checkOutOffice.openingHours.FirstOrDefault(e => e.dayOfWeek == returnDay);

            string vendorName = vendor.ShowSubVendorLogo == true ? vehicle.vendor.name : vendor.VendorName;
            string vendorLogo = vendor.ShowSubVendorLogo == true ? vehicle.vendor.logo.url : vendor.Logo;

            var deposit = vehicle.rules.FirstOrDefault(r => r.name == "deposit")?.deposit;
            float? depositAmount = deposit == null ? null :
                (deposit.currency.ToEnum<CurrencyTypes>() == baseVendorCurrencyType
                    ? deposit.amount / 100f
                    : CalculationHelper.CurrencyExchange(exchangeRates, vendor, deposit.amount / 100f, deposit.currency.ToEnum<CurrencyTypes>(), baseVendorCurrencyType));

            var rangeLimit = vehicle.rules.FirstOrDefault(r => r.name == "rangeLimit")?.rangeLimit?.amount;
            var dailyKmLimit = (rangeLimit / vehicle.rentalDurationInDays).ToIntNullSafe();

            var supplierCreditType = ResolveSupplierCreditType(vehicle, vendorVendorByName);
            var effectiveCreditType = ResolveEffectiveCreditType(agency?.CreditType ?? CreditType.Non, supplierCreditType);
            var isFullCredit = effectiveCreditType == CreditType.FullCredit;

            var deliveryType = GetYolcu360DeliveryType(vehicle.appointment.checkInOffice.deliveryType.id);
            var fromOffice = (deliveryType == Domain.Models.DeliveryType.FromOffice || deliveryType == Domain.Models.DeliveryType.InTerminalOffice);

            return vehicle != null ? new Vehicle
            {
                VehicleId = index,
                BaseVendorId = vendor.VendorId,
                VehicleCode = vehicle.code,
                VendorId = vehicle.vendor.id * 100,
                VendorType = VendorTypes.Yolcu360v2,
                VehicleName = $"{vehicle.brand.name} {vehicle.model.name}",
                FuelType = GetYolcu360FuelType(vehicle.fuel.id),
                FuelTypeName = vehicle.fuel.name,
                TransmissionType = GetYolcu360TransmissionType(vehicle.transmission.id),
                TransmissionTypeName = vehicle.transmission.name,
                DepositPrice = (effectiveCreditType == CreditType.LimitedCredit || effectiveCreditType == CreditType.FullCredit) ? 0 : depositAmount,
                VendorMinimumDriverAge = vehicle.rules.FirstOrDefault(e => e.name == "minDriverAge")?.minDriverAge ?? null,
                VendorMinimumDrivingLicenseAge = vehicle.rules.FirstOrDefault(e => e.name == "minLicenseYear")?.minLicenseYear ?? null,
                DailyPrice = (vehicle.pricing.paymentTotal.amount / 100f) / vehicle.rentalDurationInDays,
                DailyPricePayNow = (vehicle.pricing.paymentTotal.amount / 100f) / vehicle.rentalDurationInDays,
                TotalPrice = vehicle.pricing.total.amount / 100f,
                TotalPricePayNow = vehicle.pricing.total.amount / 100f,
                RentalDuration = vehicle.rentalDurationInDays,
                DailyKMLimit = (effectiveCreditType == CreditType.LimitedCredit || effectiveCreditType == CreditType.FullCredit) ? 0 : dailyKmLimit,
                TotalKMLimit = (effectiveCreditType == CreditType.LimitedCredit || effectiveCreditType == CreditType.FullCredit) ? 0 : rangeLimit,
                VendorLogo = (bool)vendor.ShowSubVendorLogo ? vehicle.vendor.logo.url : vendor.Logo,
                VendorName = (bool)vendor.ShowSubVendorLogo ? vehicle.vendor.name : vendor.VendorName,
                SippCode = vehicle.sippCode,
                OneWayFee = (vehicle.pricing.prices.FirstOrDefault(e => e.type == "oneWayFee")?.amount?.amount / 100f).ToFloatNullSafe() + (vehicle.pricing.prices.FirstOrDefault(e => e.type == "deliveryFee")?.amount?.amount / 100f).ToFloatNullSafe(),
                VehicleImages = new List<VehicleImage>
                {
                    new VehicleImage
                    {
                        //Url = vehicle.images.FirstOrDefault(e=> e.size == "desktop").url
                        Url = vehicle.imageURL ?? ""
                    }
                },
                VehicleType = VehicleTypes.None,
                VehicleCategoryType = GetYolcu360VehicleCategoryType(vehicle.@class.id),
                VehicleCategoryTypeName = vehicle.@class.name,
                PassangerQuantityType = GetPassangerQuantityTypes(vehicle.seatCount),
                PassangerQuantityName = vehicle.seatCount + " Personen",
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                CreditType = effectiveCreditType,
                FullCredit = isFullCredit,
                DeliveryType = deliveryType,
                IsOffice = fromOffice,
                DepositCreditCardRequired = !(effectiveCreditType == CreditType.LimitedCredit || effectiveCreditType == CreditType.FullCredit),
                IsAvailable = true,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice,
                ApiDeliveryTypeId = vehicle.appointment.checkInOffice.deliveryType.id,
                PickUpOfficeHours = new OfficeHours
                {
                    OpeningTime = pickupTime?.open,
                    ClosingTime = pickupTime?.close
                },
                ReturnOfficeHours = new OfficeHours
                {
                    OpeningTime = returnTime?.open,
                    ClosingTime = returnTime?.close
                },
                PickupLocationAddress = vehicle.appointment.checkInOffice.address.street,
                ReturnLocationAddress = vehicle.appointment.checkOutOffice.address.street,
            } : null;
        }

        private static CreditType ResolveSupplierCreditType(Yolcu360v2Vehicle vehicle, IReadOnlyDictionary<string, VendorVendor> vendorVendorByName)
        {
            var apiVendorName = vehicle?.vendor?.name?.Trim();
            if (string.IsNullOrWhiteSpace(apiVendorName) || vendorVendorByName?.TryGetValue(apiVendorName, out var vendorVendor) != true)
                return CreditType.Non;

            return vendorVendor.CreditType switch
            {
                CreditType.FullCredit when vehicle.applicableForFullCredit.ToBoolNullSafe() => CreditType.FullCredit,
                CreditType.LimitedCredit when vehicle.applicableForLimitCredit.ToBoolNullSafe() => CreditType.LimitedCredit,
                _ => CreditType.Non
            };
        }

        private static CreditType ResolveEffectiveCreditType(CreditType agencyCreditType, CreditType supplierCreditType)
        {
            if (supplierCreditType == CreditType.Non)
                return CreditType.Non;

            if (agencyCreditType == supplierCreditType)
                return supplierCreditType;

            return agencyCreditType == CreditType.FullCredit && supplierCreditType == CreditType.LimitedCredit
                ? CreditType.LimitedCredit
                : CreditType.Non;
        }

        private static Domain.Models.DeliveryType GetYolcu360DeliveryType(int deliveryTypeId)
            => deliveryTypeId switch
            {
                1 => Domain.Models.DeliveryType.DeliveredToAddress,
                2 => Domain.Models.DeliveryType.NonTerminalValet,
                3 => Domain.Models.DeliveryType.FromOffice,
                4 => Domain.Models.DeliveryType.NonTerminalMeetAndGreet,
                5 => Domain.Models.DeliveryType.InTerminalOffice,
                6 => Domain.Models.DeliveryType.NonTerminalValet,
                _ => Domain.Models.DeliveryType.NonTerminalValet
            };

        private static FuelTypes GetYolcu360FuelType(int fuelId)
            => fuelId switch
            {
                1 => FuelTypes.Gasoline,
                2 => FuelTypes.Diesel,
                5 or 13 => FuelTypes.GasolineAndLPG,
                7 => FuelTypes.Hybrid,
                8 => FuelTypes.GasolineAndDiesel,
                9 => FuelTypes.Undefined,
                10 => FuelTypes.MultiFuel,
                11 => FuelTypes.Electric,
                14 or 17 => FuelTypes.HybritDiesel,
                15 => FuelTypes.HybritGasoline,
                16 => FuelTypes.Hydrogen,
                _ => FuelTypes.None
            };

        private static TransmissionTypes GetYolcu360TransmissionType(int transmissionId)
            => transmissionId switch
            {
                1 => TransmissionTypes.Manuel,
                2 => TransmissionTypes.Automatic,
                _ => TransmissionTypes.None
            };

        private static PassangerQuantityTypes GetPassangerQuantityTypes(int seatCount)
            => seatCount switch
            {
                1 => PassangerQuantityTypes.OnePerson,
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
                _ => PassangerQuantityTypes.None
            };

        private static VehicleCategoryTypes GetYolcu360VehicleCategoryType(int vehicleCategoryTypeId)
            => vehicleCategoryTypeId switch
            {
                1 => VehicleCategoryTypes.Economic,
                2 => VehicleCategoryTypes.Intermediate,
                3 => VehicleCategoryTypes.Standard,
                4 => VehicleCategoryTypes.Prestige,
                5 => VehicleCategoryTypes.Premium,
                6 or 7 => VehicleCategoryTypes.Compact,
                8 or 12 => VehicleCategoryTypes.Luxury,
                9 => VehicleCategoryTypes.Minivan,
                10 => VehicleCategoryTypes.SUV,
                11 => VehicleCategoryTypes.FullSize,
                13 => VehicleCategoryTypes.CompactElite,
                _ => VehicleCategoryTypes.None
            };
    }
}
