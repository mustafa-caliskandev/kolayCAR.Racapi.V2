using System;

namespace KolayCAR.Broker.API.Models.Dtos
{
    public class CouponProcedureParametersDto
    {
        public string CouponCode { get; set; }
        public string CustomerMailAddress { get; set; }
        public int LanguageId { get; set; }
        public int CurrencyId { get; set; }
        public int VendorId { get; set; }
        public string TotalPrice { get; set; }

        private string pickupDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
        public string PickupDate
        {
            get
            {
                return pickupDate;
            }
            set
            {
                var dateValue = DateTime.TryParse(value, out var date);
                pickupDate = dateValue ? date.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd");
            }
        }
        private string returnDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
        public string ReturnDate
        {
            get
            {
                return returnDate;
            }
            set
            {
                var dateValue = DateTime.TryParse(value, out var date);
                returnDate = dateValue ? date.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd");
            }
        }
        public int PickupLocationId { get; set; } = 0;
        public int ReturnLocationId { get; set; } = 0;
        public int RentalDuration { get; set; } = 0;
        public int AgencyId { get; set; } = 0;
        public int MemberId { get; set; } = 0;
        public int PaymentType { get; set; } = 0;

        public string MobileType { get; set; } = "";
    }
}
