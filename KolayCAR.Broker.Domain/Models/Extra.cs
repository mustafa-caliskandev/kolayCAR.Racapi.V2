namespace KolayCAR.Broker.Domain.Models
{
    public class Extra : RestrictedExtra
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public int? ShowDayCountStart { get; set; }
        public int? ShowDayCountEnd { get; set; }
        public int Sequence { get; set; }
        public bool? IsRequired { get; set; } = false;
        public float DefaultPrice { get; set; }
        public float ApiPrice { get; set; }

        public string? ApiExtraCode { get; set; }
        public float AgencyAmount { get; set; }
        public bool VendorExtraExists { get; set; }
        public string Label { get; set; }
        public int Piece { get; set; }
        public CurrencyTypes? CurrencyType { get; set; }
        public string DamageInsuranceCategory { get; set; }
        public bool ShowInVehicleList { get; set; }
        public float UnitPrice { get; set; } //For Obilet Mobile
    }

    public class RestrictedExtra
    {
        public int ExtraId { get; set; }
        public string ExtraCode { get; set; }
        public string ExtraName { get; set; }
        public string ExtraDescription { get; set; }
        public ExtraRentalTypes? ExtraRentalType { get; set; }
        public AdditionalProductTypes? ExtraType { get; set; }
        public bool ExtraQuantityIncreasable { get; set; }
        public float Price { get; set; }
        public string Icon { get; set; }
        public string CurrencyCode { get; set; }

        public string Code { get; set; }

    }

    public class ExtraListItem
    {
        public int ExtraId { get; set; }
        public string ExtraCode { get; set; }
        public string ExtraName { get; set; }
        public string ExtraDescription { get; set; }
        public AdditionalProductTypes ExtraType { get; set; }
        public string Icon { get; set; }
    }

    public class ReservationExtra : RestrictedReservationExtra
    {
    }

    public class RestrictedReservationExtra : Extra
    {
        public int Piece { get; set; }

    }
    public class CyrptExtra
    {
        public int I { get; set; }
        public string C { get; set; }
        public ExtraRentalTypes R { get; set; }
        public AdditionalProductTypes T { get; set; }
        public float P { get; set; }
        public float A { get; set; }
        public string AC { get; set; }
        public string N { get; set; }
        public string CD { get; set; }
        public bool V { get; set; }
    }
    public enum ExtraRentalTypes
    {
        PerRental,
        Daily
    }
}
