namespace KolayCAR.Broker.API.Models
{
    public partial class Additionalproduct
    {
        public int Productid { get; set; }
        public int Langid { get; set; }
        public int Producttype { get; set; }
        public string Productname { get; set; }
        public string Productdescription { get; set; }
        public int? Rentaltype { get; set; }
        public bool? Quantityincreasable { get; set; }
        public bool Active { get; set; }
        public string Iconpath { get; set; }
        public decimal? Defaultprice { get; set; }
        public string Productcode { get; set; }
        public int? Showdaycountstart { get; set; }
        public int? Showdaycountend { get; set; }
        public bool? Showinvehiclelist { get; set; }
        public int? Sequence { get; set; }
        public int? CurrencyId { get; set; }
        public bool? VendorExtraExists { get; set; }
        public bool ShowOnlyFullCreditVehicles { get; set; }
    }
}
