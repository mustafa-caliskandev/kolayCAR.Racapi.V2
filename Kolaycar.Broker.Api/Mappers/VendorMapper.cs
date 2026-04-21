using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers
{
    public static class VendorMapper
    {
        public static CommonModels.Vendor Map(this Vendor vendor, bool encrypt = true, CommonModels.Agency agency = null, Agencyvendorprofitmarkup agencyVendorProfitMarkup = null)
        {
            Serilog.Log.Debug("{@Vendor}", vendor);
            float ProfitMarkupDailyPrice = vendor.Profitmarkup.ToFloatNullSafe();
            float ProfitMarkupAdditionalProducts = vendor.Profitmarkupadditionalproducts.ToFloatNullSafe();
            float ProfitMarkupOneWayFee = vendor.Profitmarkuponewayfee.ToFloatNullSafe();

            if (agency != null && agencyVendorProfitMarkup != null)
            {
                ProfitMarkupDailyPrice = agencyVendorProfitMarkup.Profitmarkup != null ? agencyVendorProfitMarkup.Profitmarkup.ToFloatNullSafe() : vendor.Profitmarkup.ToFloatNullSafe();
                ProfitMarkupAdditionalProducts = agencyVendorProfitMarkup.Profitmarkupadditionalproducts != null ? agencyVendorProfitMarkup.Profitmarkupadditionalproducts.ToFloatNullSafe() : vendor.Profitmarkupadditionalproducts.ToFloatNullSafe();
                ProfitMarkupOneWayFee = agencyVendorProfitMarkup.Profitmarkuponewayfee != null ? agencyVendorProfitMarkup.Profitmarkuponewayfee.ToFloatNullSafe() : vendor.Profitmarkuponewayfee.ToFloatNullSafe();
            }

            return vendor != null ? new CommonModels.Vendor
            {
                VendorId = vendor.Vendorid,
                VendorName = vendor.Vendorname,
                VendorPhone = vendor.Phone,
                VendorEmail = vendor.Email,
                VendorType = (CommonModels.VendorTypes)vendor.Vendortype,
                ApiKey = encrypt ? EncryptionHelper.Encrypt(vendor.Apikey) : vendor.Apikey,
                ApiPassword = encrypt ? EncryptionHelper.Encrypt(vendor.Apipassword) : vendor.Apipassword,
                Active = vendor.Active ?? false,
                ReservationListActive = vendor.Reslistactive ?? false,
                ProfitMarkupDailyPrice = ProfitMarkupDailyPrice,
                ProfitMarkupAdditionalProducts = ProfitMarkupAdditionalProducts,
                ProfitMarkupOneWayFee = ProfitMarkupOneWayFee,
                PriceRoundingType = (CommonModels.PriceRoundingTypes)vendor.Priceroundingtype,
                CurrencyType = (CommonModels.CurrencyTypes)vendor.Currencyid - 1,
                AvailableCurrencies = !string.IsNullOrEmpty(vendor.Availablecurrencies) ? vendor.Availablecurrencies.Split(',').ToList().Select(x => (CommonModels.CurrencyTypes)(x.ToIntNullSafe() - 1)).ToList() : null,
                APIBaseUrl = vendor.Apibaseurl,
                ServiceCharge = vendor.Servicecharge.ToFloatNullSafe(),
                ServiceChargeCurrencyType = (CommonModels.CurrencyTypes)vendor.Servicechargecurrencyid - 1,
                ProfitMarkupDailyPriceActive = ProfitMarkupDailyPrice != 0,
                ProfitMarkupAdditionalProductsActive = ProfitMarkupAdditionalProducts != 0,
                ProfitMarkupOneWayFeeActive = ProfitMarkupOneWayFee != 0,
                Logo = vendor.Logo,
                DepositCreditCardRequired = vendor.Depositcreditcardrequired ?? false,
                VehicleMappingActive = vendor.Vehiclemappingactive ?? false,
                ApiClientId = vendor.Apiclientid,
                SecretKey = vendor.Secretkey,
                APIPhoneActive = vendor.Apiphoneactive ?? false,
                APITimeout = vendor.Apitimeout ?? 0,
                CompanyTitle = vendor.Companytitle,
                DisableDeposit = vendor.Disabledeposit ?? false,
                PersonelNumberRequired = vendor.Personelnumberrequired ?? false,
                SendReservationMailToVendor = vendor.Sendreservationmailtovendor ?? false,
                UseOnlyDefaultCurrency = vendor.Useonlydefaultcurrency ?? false,
                ResAgencyNameSending = vendor.Resagencynamesending,
                SellingBelowCostForCouponCode = vendor.Sellingbelowcostforcouponcode ?? false,
                UseBrokerConfigurations = vendor.Usebrokerconfigurations ?? false,
                RentalWorkingType = (CommonModels.VendorWorkingTypes)vendor.Rentalworkingtype,
                AdditionalProductWorkingType = (CommonModels.VendorWorkingTypes)vendor.Additionalproductworkingtype,
                OneWayFeeWorkingType = (CommonModels.VendorWorkingTypes)vendor.Onewayfeeworkingtype,
                CouponCodeActive = vendor.Couponcodeactive ?? true,
                FreeCancellationHour = vendor.Freecancellationhour ?? 0,
                ShowCustomerNoteArea = vendor.Showcustomernotearea ?? false,
                ShowFlightNumberArea = vendor.Showflightnumberarea ?? false,
                CreditType = (CommonModels.CreditType)vendor.CreditType,
                ShowSubVendorLogo = vendor.ShowSubVendorLogo ?? false,
                AppearingProfitMarkup = vendor.AppearingProfitMarkup ?? 0,
                FindeksRequired = vendor.FindeksRequired ?? false,
                BirthdayRequired = vendor.BirthdayRequired ?? false,
                SendEmailToBranch = vendor.SendEmailToBranch ?? false,
                UseLocalDeposit = vendor.UseLocalDeposit ?? false,
                ToleranceTime = vendor.ToleranceTime ?? 0,
                VendorOrder = vendor.VendorOrder,
                FlightNumberRequired = vendor.FlightNumberRequired ?? false,
                EarliestResTime = vendor.EarliestResTime,
                SendAvailabilityRequest = vendor.SendAvailabilityRequest ?? false,
                ExtraDescriptionFromVendor = vendor.ExtraDescriptionFromVendor ?? false,
                SendDefaultMailAddress = vendor.SendDefaultMailAddress ?? false,
                DeliveryTypeFromVendor = vendor.DeliveryTypeFromVendor ?? false,
                HideLocationAddressOnPayment = vendor.HideLocationAddressOnPayment
            }
            : null;
        }

        public static List<CommonModels.Vendor> Map(this List<Vendor> vendors, CommonModels.Agency agency = null, List<Agencyvendorprofitmarkup> agencyVendorProfitMarkupList = null)
        {
            var _vendors = new List<CommonModels.Vendor>();

            if (vendors != null && vendors.Count != 0)
                foreach (var vendor in vendors)
                {
                    try
                    {
                        _vendors.Add(vendor.Map(agency: agency, agencyVendorProfitMarkup: agencyVendorProfitMarkupList.Where(x => x.Vendorid == vendor.Vendorid).FirstOrDefault()));
                    }
                    catch (System.Exception ex)
                    {
                        Serilog.Log
                            .ForContext("Agency", agency)
                            .ForContext("AgencyVendorMarkupList", agencyVendorProfitMarkupList)
                            .ForContext("Vendor", vendor.Vendorname)
                            .Error("{@VendorMapperError}", ex.Message);
                    }
                }

            return _vendors;
        }

        public static List<CommonModels.VendorVendor> Map(this List<VendorVendor> vendorVendors)
        {
            var list = new List<CommonModels.VendorVendor>();

            list = vendorVendors.Select(x => new CommonModels.VendorVendor
            {
                Id = x.Id,
                VendorName = x.VendorName,
                Active = x.Active,
                VendorId = x.VendorId,
                HgsPackage = x.HgsPackage,
                MatchedVendorId = x.MatchedVendorId,
                MatchedVendorName = x.MatchedVendorName,
                FlightCardMandatory = x.FlightCardMandatory
            }).ToList();

            return list;

            //if(vendorVendors != null && vendorVendors.Count > 0)
            //{
            //    foreach(var item in vendorVendors)
            //    {
            //        list.Add(item.Map());
            //    }
            //}

            //return list;
        }

        public static IEnumerable<API.Models.VendorVendor> Map(this IEnumerable<Domain.Models.VendorVendor> vendorVendors)
        {
            var list = new List<API.Models.VendorVendor>();

            list = vendorVendors.Select(x => new API.Models.VendorVendor
            {
                Id = x.Id,
                VendorName = x.VendorName,
                Active = x.Active,
                VendorId = x.VendorId,
                HgsPackage = x.HgsPackage,
                MatchedVendorId = x.MatchedVendorId,
                MatchedVendorName = x.MatchedVendorName,
                FlightCardMandatory = x.FlightCardMandatory
            }).ToList();

            return list;
        }
        public static CommonModels.VendorVendor Map(this VendorVendor vendor)
        {
            return vendor != null ? new CommonModels.VendorVendor
            {
                Active = vendor.Active,
                VendorId = vendor.VendorId,
                VendorName = vendor.VendorName,
                Id = vendor.Id
            } : null;
        }
    }
}
