using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System;
using System.Collections.Generic;


namespace KolayCAR.Broker.API.Mappers.Enuygun
{
    public static class VehicleMapper
    {

        public static List<Vehicle> Map(this List<EnuygunResponse.Search.Reservation> apiVehicles, ResponseReservationStepsAdditionalInformation additionalInformation, CurrencyTypes currency, Vendor vendor, string requestID, List<ExchangeRates> exchangeRates)
        {

            var _vehicles = new List<Vehicle>();

            foreach (var item in apiVehicles)
            {
                _vehicles.Add(item.Map(additionalInformation, currency, vendor, requestID, exchangeRates));
            }

            return _vehicles;
        }



        public static Vehicle Map(this EnuygunResponse.Search.Reservation apiVehicle, ResponseReservationStepsAdditionalInformation additionalInformation, CurrencyTypes currency, Vendor vendor, string requestID, List<ExchangeRates> exchangeRates)
        {
            //var TotalPrice = apiVehicle.breakdowns[0].type == "reservation" ? apiVehicle.breakdowns[0].chargePrice.ToFloatNullSafe() : 0;

            //var TotalPrice = apiVehicle.breakdowns[0].type == "reservation" ? apiVehicle.breakdowns[0].raw.totalPrice : 0;
            var apiCurrency = (CurrencyTypes)Enum.Parse(typeof(CurrencyTypes), apiVehicle.provisionPrice.currency);
            var TotalPrice = apiVehicle.breakdowns[0].type == "reservation" ? apiVehicle.breakdowns[0].totalPrice.ToFloatNullSafe() : 0;
            var depositPrice = CalculationHelper.CurrencyExchange(exchangeRates, vendor, apiVehicle.provisionPrice.price.ToLongNullSafe(), apiCurrency, currency);
            float oneWayPrice = 0;
            bool IsPayOnDeliveryOnewayFee = false;


            //if (apiVehicle.breakdowns[1].type == "drop")
            //{
            //    if (apiVehicle.breakdowns[1].chargePrice.ToFloatNullSafe() > 0 && apiVehicle.breakdowns[1].officePrice.ToFloatNullSafe() == 0)
            //    {
            //        oneWayPrice = apiVehicle.breakdowns[1].chargePrice.ToFloatNullSafe();
            //        IsPayOnDeliveryOnewayFee = false;
            //    }
            //    else if (apiVehicle.breakdowns[1].chargePrice.ToFloatNullSafe() == 0 && apiVehicle.breakdowns[1].officePrice.ToFloatNullSafe() > 0)
            //    {
            //        oneWayPrice = apiVehicle.breakdowns[1].officePrice.ToFloatNullSafe();
            //        IsPayOnDeliveryOnewayFee = true;
            //    }
            //    else
            //    {
            //        oneWayPrice = 0;
            //        IsPayOnDeliveryOnewayFee = false;
            //    }
            //}

            if (apiVehicle.breakdowns[1].type == "drop")
            {
                oneWayPrice = apiVehicle.breakdowns[1].raw.totalPrice.ToFloatNullSafe();

            }

            string vendorName = vendor.ShowSubVendorLogo == true ? apiVehicle.company.name : vendor.VendorName;
            string vendorLogo = vendor.ShowSubVendorLogo == true ? apiVehicle.company.logoUri : vendor.Logo;



            var vehicle = new Vehicle
            {
                VehicleId = apiVehicle.vehicle.matchCode.ToIntNullSafe(), //Bu kısım sorulacak.
                VehicleCode = apiVehicle.vehicle.matchCode,
                VendorId = additionalInformation.Vendor.VendorId,
                VendorName = vendorName,
                ApiVendorName = apiVehicle.company.name,
                //SpecialVendorName = apiVehicle.company.name,
                //SpecialVendorLogo = apiVehicle.company.logoUri,  
                //VendorLogo = additionalInformation.Vendor.Logo,
                VendorLogo = vendorLogo,
                VendorEmail = additionalInformation.Vendor.VendorEmail,
                VendorPhone = additionalInformation.Vendor.VendorPhone,
                VehicleName = apiVehicle.vehicle.brand + " " + apiVehicle.vehicle.name,
                PickupLocationId = additionalInformation.PickupLocationId,
                PickupLocationName = additionalInformation.PickupLocationName,
                ReturnLocationId = additionalInformation.ReturnLocationId,
                ReturnLocationName = additionalInformation.ReturnLocationName,
                PickupDateTime = additionalInformation.PickupDateTime,
                ReturnDateTime = additionalInformation.ReturnDateTime,
                RentalDuration = apiVehicle.days,
                //DailyPrice = apiVehicle.price.dailyPrice.ToFloatNullSafe(),
                //OneWayFee = apiVehicle.breakdowns[1].type == "drop" ? 
                //    apiVehicle.breakdowns[1].chargePrice.ToFloatNullSafe() > 0 && apiVehicle.breakdowns[1].totalPrice.ToFloatNullSafe() == 0 ?
                //     apiVehicle.breakdowns[1].chargePrice.ToFloatNullSafe() : 
                //     apiVehicle.breakdowns[1].chargePrice.ToFloatNullSafe() == 0 && apiVehicle.breakdowns[1].totalPrice.ToFloatNullSafe() > 0 ?
                //     apiVehicle.breakdowns[1].officePrice.ToFloatNullSafe() : 0
                //     : 0,

                //TotalPrice = apiVehicle.price.totalPrice.ToFloatNullSafe(),
                OneWayFee = oneWayPrice,
                IsOneWayFeePOA = IsPayOnDeliveryOnewayFee,
                TotalPrice = TotalPrice,
                DailyPrice = TotalPrice / apiVehicle.days,
                ApiDailyPrice = TotalPrice / apiVehicle.days,
                IsAvailable = true,
                VehicleImages = new List<VehicleImage>
                {
                    new VehicleImage
                    {
                        Url =apiVehicle.vehicle.imageUrl,
                    }
                },
                VehicleCategoryType = GetEnUygunVehicleCategoryType(apiVehicle.vehicle.@class),
                PassangerQuantityType = (PassangerQuantityTypes)apiVehicle.vehicle.chair,
                PassangerQuantityName = apiVehicle.vehicle.chair.ToStringNullSafe() + " Kişi",
                FuelType = GetEnUygunFuelType(apiVehicle.vehicle.fuel),
                TransmissionType = GetEnUygunTransmissionType(apiVehicle.vehicle.transmission),
                DeliveryType = GetEnUygunDeliveryType(apiVehicle.deliveryType),
                BaggageQuantityType = BaggageQuantityTypes.None,
                IsThereAirCondition = true,
                VendorMinimumDriverAge = apiVehicle.driverAge,
                VendorMinimumDrivingLicenseAge = apiVehicle.licenceYear,
                TotalPricePayNow = TotalPrice,
                //DepositPrice = apiVehicle.provisionPrice.price.ToFloatNullSafe(),
                DepositPrice = depositPrice,
                TotalKMLimit = apiVehicle.limitedKm,
                DailyKMLimit = apiVehicle.dailyLimitedKm,
                FullCredit = (vendor.CreditType == CreditType.FullCredit) ? true : false,
                IsOffice = GetEnUygunDeliveryType(apiVehicle.deliveryType) == DeliveryType.FromOffice,
                VehicleType = VehicleTypes.None,
                VehicleBrandName = apiVehicle.vehicle.brand,
                VehicleModelName = apiVehicle.vehicle.name,
                VendorType = VendorTypes.EnUygun,
                ShortAddress = "Request Id = " + requestID + " Reservation ID = " + apiVehicle.referenceId,
                //IsPayOnDelivery = apiVehicle.breakdowns[0].officePrice.ToFloatNullSafe() > 0  ? true : false,
                RentalWorkingTypes = vendor.RentalWorkingType,
                ProfitMarkupDailyPrice = vendor.ProfitMarkupDailyPrice,

            };

            return vehicle;
        }


        private static VehicleCategoryTypes GetEnUygunVehicleCategoryType(string vehicleCategoryType)
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

        private static FuelTypes GetEnUygunFuelType(string fuelType)
        {
            switch (fuelType)
            {
                case "gas": return FuelTypes.Gasoline;
                case "diesel": return FuelTypes.Diesel;
                case "lpg": return FuelTypes.GasolineAndLPG;
                case "diesel-gas-mixed": return FuelTypes.HybritDiesel;
                case "hybrid": return FuelTypes.HybritGasoline;
                case "gas-diesel": return FuelTypes.GasolineAndDiesel;
                case "electric": return FuelTypes.Electric;
                default: return FuelTypes.None;
            }
        }

        private static TransmissionTypes GetEnUygunTransmissionType(string transmissionType)
        {
            switch (transmissionType)
            {
                case "manual": return TransmissionTypes.Manuel;
                case "automatic": return TransmissionTypes.Automatic;
                default: return TransmissionTypes.None;
            }
        }

        private static DeliveryType GetEnUygunDeliveryType(string type)
        {
            switch (type)
            {
                case "officeDelivery": return DeliveryType.FromOffice;
                case "airportMeetAndGreet": return DeliveryType.MeetAndGreet;
                default: return DeliveryType.None;
            }
        }


    }

}
