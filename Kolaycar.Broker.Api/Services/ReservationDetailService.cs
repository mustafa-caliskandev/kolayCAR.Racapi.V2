using KolayCAR.Broker.API.Extensions;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Models.MobileAppDtos.ReservationDtos;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IReservationDetailService
    {
        Task<ReservationDetail> AddWithDetail(ReservateNowDtoMobile reservateNowDto);
        Task<ReservationDetail> GetByReservationTokenWithDetail(string reservationToken);
        Task<ReservationDetail> GetByReservationToken(string reservationToken);
    }
    public class ReservationDetailService : IReservationDetailService
    {
        private readonly BrokerContext _context;
        public ReservationDetailService(BrokerContext context)
        {
            _context = context;
        }

        public async Task<ReservationDetail> AddWithDetail(ReservateNowDtoMobile reservateNowDto)
        {
            try
            {
                #region Arama Loglanıyor
                var reservationDetail = new ReservationDetail()
                {
                    PaymentType = (int)reservateNowDto.PaymentType,
                    ReservationToken = reservateNowDto.ReservationToken,
                    CouponCode = reservateNowDto.CouponCode,
                    SelectedExtrasJson = reservateNowDto.SelectedExtrasJson,
                    ReservationNote = reservateNowDto.ReservationNote,
                    ConfirmConditions = reservateNowDto.ConfirmConditions,
                    InvoiceToDifferentAddress = reservateNowDto.InvoiceToDifferentAddress,
                    CouponDiscountAmount = reservateNowDto.CouponDiscountAmount,
                    ServiceCharge = reservateNowDto.ServiceCharge,
                    MemberId = reservateNowDto.MemberId,
                    CurrencyId = reservateNowDto.CurrencyId,
                    SkyScannerRedirectId = reservateNowDto.SkyScannerRedirectId,
                    PickupLocation = reservateNowDto.PickupLocation,
                    PickupDate = reservateNowDto.PickupDate,
                    ReturnLocation = reservateNowDto.ReturnLocation,
                    ReturnDate = reservateNowDto.ReturnDate,
                    VendorName = reservateNowDto.VendorName,
                    FullCredit = reservateNowDto.FullCredit,
                };

                await _context.ReservationDetails.AddAsync(reservationDetail);
                await _context.SaveChangesAsync();

                if (reservationDetail != null && reservationDetail.Id > 0)
                {
                    var driverInfo = new ReservationDriverInfo()
                    {
                        ReservationDetailId = reservationDetail.Id,
                        IdentityNumber = reservateNowDto.IdentityNumber,
                        Name = reservateNowDto.Name,
                        Surname = reservateNowDto.Surname,
                        Email = reservateNowDto.Email,
                        CountryPhoneCode = reservateNowDto.CountryPhoneCode ?? "",
                        PhoneNumber = reservateNowDto.PhoneNumber ?? "",
                        Gender = (reservateNowDto.Gender ?? ""),
                        FlightNumber = reservateNowDto.FlightNumber ?? "",
                        ContactPermission = reservateNowDto.ContactPermission,
                        IsNonTurkishCitizen = reservateNowDto.IsNonTurkishCitizen,
                        Birthday = reservateNowDto.BirthDay
                    };
                    await _context.ReservationDriverInfos.AddAsync(driverInfo);
                    await _context.SaveChangesAsync();

                    if (!string.IsNullOrEmpty(reservateNowDto.Address))
                    {
                        var invoiceAdress = new ReservationInvoiceAddress()
                        {
                            ReservationDetailId = reservationDetail.Id,
                            Address = reservateNowDto.Address ?? "",
                            Title = reservateNowDto.Title ?? "",
                            TaxOffice = reservateNowDto.TaxOffice ?? "",
                            TaxNumber = reservateNowDto.TaxNumber ?? "",
                            Country = reservateNowDto.Country ?? "",
                            City = reservateNowDto.City ?? "",
                            District = reservateNowDto.District ?? "",
                            ZipCode = reservateNowDto.ZipCode ?? ""
                        };
                        await _context.ReservationInvoiceAddresses.AddAsync(invoiceAdress);
                        await _context.SaveChangesAsync();
                    }

                    var reservationPaymentDetail = new ReservationPaymentDetail()
                    {
                        ReservationDetailId = reservationDetail.Id,
                        CreditCardNumber = reservateNowDto?.CreditCardNumber ?? "",
                        CreditCardOwnerName = reservateNowDto?.CreditCardOwnerName ?? "",
                        ExpireDate = reservateNowDto?.ExpireDate ?? "",
                        Cvc = reservateNowDto?.Cvc ?? "",
                        ExpireMonth = reservateNowDto?.ExpireMonth ?? -1,
                        ExpireYear = reservateNowDto?.ExpireYear ?? -1,
                        InstallmentCount = reservateNowDto?.InstallmentCount ?? 0,
                        InstallmentCommissionAmount = reservateNowDto?.InstallmentCommissionAmount ?? 0,
                        TotalPrice = (reservateNowDto?.TotalPrice ?? 0),
                        PaidAmount = (reservateNowDto?.PaidAmount ?? 0),
                        AdditionalProductPricePoa = reservateNowDto.AdditionalProductPricePoa.ToBool(),
                        OneWayFeePoa = reservateNowDto.OneWayFeePoa.ToBool(),
                        PaymentCode = reservateNowDto.PaymentCode ?? "",
                    };
                    await _context.ReservationPaymentDetails.AddAsync(reservationPaymentDetail);
                    await _context.SaveChangesAsync();

                    if (!string.IsNullOrEmpty(reservateNowDto.SelectedExtrasJson) && reservateNowDto.SelectedExtrasJson.ValidateJson())
                    {
                        IEnumerable<ReservationDetailSelectedExtra> selectedExtras = JsonConvert.DeserializeObject<IEnumerable<ReservationDetailSelectedExtra>>(reservateNowDto.SelectedExtrasJson);
                        IEnumerable<ReservationSelectedExtra> reservationSelectedExtras = CreateExtras(selectedExtras, reservationDetail.Id);

                        await _context.ReservationSelectedExtras.AddRangeAsync(reservationSelectedExtras);
                        await _context.SaveChangesAsync();
                    }

                    var reservationVehicleInfo = new ReservationVehicleInfo()
                    {
                        ReservationDetailId = reservationDetail.Id,

                        VehicleBrandName = reservateNowDto.VehicleBrandName ?? "",
                        VehicleModelName = reservateNowDto.VehicleModelName ?? "",
                        VehicleName = reservateNowDto.VehicleName ?? "",
                        FuelName = reservateNowDto.FuelName,
                        TransmissionName = reservateNowDto.TransmissionName,
                        PersonName = reservateNowDto.PersonName,
                        CategoryName = reservateNowDto.CategoryName ?? "",
                        TypeName = reservateNowDto.TypeName ?? "",

                        DeliveryType = reservateNowDto.DeliveryType,
                        DailyPrice = reservateNowDto.DailyPrice,
                        Deposit = reservateNowDto.Deposit,
                        KmLimit = reservateNowDto.KmLimit,
                        OneWayFee = reservateNowDto.OneWayFee,
                        RentalDuration = reservateNowDto.RentalDuration,

                        ExtraJson = reservateNowDto.ExtraJson,
                        ExtraNames = reservateNowDto.ExtraNames,
                        ExtraAmount = reservateNowDto.ExtraAmount,
                        SpecialDailyPrice = reservateNowDto.SpecialDailyPrice,
                        SpecialOneWayFee = reservateNowDto.SpecialOneWayFee,

                        MinimumAge = reservateNowDto.MinimumAge,
                        MinimumLicenseAge = reservateNowDto.MinimumLicenseAge,
                    };
                    await _context.ReservationVehicleInfos.AddAsync(reservationVehicleInfo);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("ReservationDetail cannot saved");
                }
                #endregion
            }
            catch (Exception ex)
            {
                return Activator.CreateInstance<ReservationDetail>();
            }
            return await GetByReservationTokenWithDetail(reservateNowDto.ReservationToken);
        }

        public async Task<ReservationDetail> GetByReservationTokenWithDetail(string reservationToken)
        {
            return await _context.ReservationDetails
                .Include(r => r.ReservationDriverInfos)
                .Include(r => r.ReservationInvoiceAddresses)
                .Include(r => r.ReservationPaymentDetails)
                .Include(r => r.ReservationSelectedExtras)
                .Include(r => r.ReservationVehicleInfos)
                .OrderByDescending(r => r.Id)
                .FirstOrDefaultAsync(r => r.ReservationToken == reservationToken);
        }

        public async Task<ReservationDetail> GetByReservationToken(string reservationToken)
        {
            return await _context.ReservationDetails
                .OrderByDescending(r => r.Id)
                .FirstOrDefaultAsync(r => r.ReservationToken == reservationToken);
        }

        #region Functions
        private IEnumerable<ReservationSelectedExtra> CreateExtras(IEnumerable<ReservationDetailSelectedExtra> selectedExtras, int reservationPaymentDetailId)
        {
            foreach (var extra in selectedExtras)
            {
                var reservationSelectedExtra = new ReservationSelectedExtra()
                {
                    ReservationDetailId = reservationPaymentDetailId,
                    ExtraRentalType = extra.ExtraRentalType,
                    RentalDuration = extra.RentalDuration,
                    Name = extra.Name,
                    Price = (decimal)extra.Price
                };
                yield return reservationSelectedExtra;
            }
        }
        #endregion
    }
}
