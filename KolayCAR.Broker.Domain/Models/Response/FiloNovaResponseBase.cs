using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class FiloNovaResponseBase
    {
        public List<GroupCodeInformation> groupCodeInformation { get; set; }
        public List<Country> countries { get; set; }
        public List<City> cities { get; set; }
        public List<Branch> branchs { get; set; }
        public List<WorkingHour> workingHours { get; set; }
        public List<TaxOffice> taxOffices { get; set; }
        public List<District> districts { get; set; }
        public ResponseResult responseResult { get; set; }
        public List<AvailabilityData> availabilityData { get; set; }
        public List<AdditionalProduct> additionalProducts { get; set; }
        public string trackingNumber { get; set; }
        public double totalDuration { get; set; }
        public double exchangeRate { get; set; }
        public double oneWayFeeAmount { get; set; }
        public string reservationId { get; set; }
        public List<string> reservationItemList { get; set; }
        public string pnrNumber { get; set; }



        public class GroupCodeInformation
        {
            public string groupCodeName { get; set; }
            public string groupCodeId { get; set; }
            public string groupCodeDescription { get; set; }
            public string transmissionName { get; set; }
            public string fuelTypeName { get; set; }
            public int findeksPoint { get; set; }
            public bool isDoubleCard { get; set; }
            public int minimumAge { get; set; }
            public int minimumDriverLicense { get; set; }
            public int youngDriverAge { get; set; }
            public int youngDriverMinimumLicense { get; set; }
            public double depositAmount { get; set; }
            public int transmission { get; set; }
            public int fuelType { get; set; }
            public int segment { get; set; }
            public string segmentName { get; set; }
            public string showRoomBrandName { get; set; }
            public string showRoomModelName { get; set; }
            public string webImageURL { get; set; }
        }

        public class Country
        {
            public string countryName { get; set; }
            public string countryCode { get; set; }
            public string countryId { get; set; }
            public string countryDialCode { get; set; }
        }

        public class City
        {
            public string cityId { get; set; }
            public string cityName { get; set; }
            public string countryId { get; set; }
        }

        public class Branch
        {
            public string branchId { get; set; }
            public string branchName { get; set; }
            public string cityId { get; set; }
            public string cityName { get; set; }
            public string addressDetail { get; set; }
            public double latitude { get; set; }
            public double longitude { get; set; }
            public string telephone { get; set; }
            public string emailaddress { get; set; }
            public string seoKeyword { get; set; }
            public string seoTitle { get; set; }
            public string seoDescription { get; set; }
        }

        public class WorkingHour
        {
            public string workingHourId { get; set; }
            public string branchId { get; set; }
            public string branchName { get; set; }
            public int dayCode { get; set; }
            public int beginingTime { get; set; }
            public int endTime { get; set; }
        }

        public class TaxOffice
        {
            public string taxOfficeName { get; set; }
            public string taxOfficeId { get; set; }
            public string cityId { get; set; }
            public string cityName { get; set; }
        }

        public class District
        {
            public string districtId { get; set; }
            public string cityId { get; set; }
            public string districtName { get; set; }
        }

        public class CancelReservationResult
        {
            public ResponseResult responseResult { get; set; }
        }
        public class ResponseResult
        {
            public bool result { get; set; }
            public object exceptionDetail { get; set; }
        }

        public class AvailabilityData
        {
            public string groupCodeName { get; set; }
            public string groupCodeId { get; set; }
            public double ratio { get; set; }
            public double payAmount { get; set; }
            public double dailyAmount { get; set; }
            public int kmLimit { get; set; }
        }

        public class AdditionalProduct
        {
            public string productId { get; set; }
            public string productName { get; set; }
            public int productType { get; set; }
            public string productCode { get; set; }
            public int maxPieces { get; set; }
            public int webRank { get; set; }
            public string productDescription { get; set; }
            public string webIconURL { get; set; }
            public double actualAmount { get; set; }
            public object dailyAmount { get; set; }
            public int value { get; set; }
            public double tobePaidAmount { get; set; }
            public bool isMandatory { get; set; }
            public double monthlyPackagePrice { get; set; }
            public int priceCalculationType { get; set; }
        }

    }
}
