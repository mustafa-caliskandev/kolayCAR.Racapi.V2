using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Coupon
    {
        public int Id { get; set; }
        public int? AdminId { get; set; }
        public int? AgencyId { get; set; }
        public int? MemberId { get; set; }
        public bool? Active { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? MultipleUsage { get; set; }
        public int? UsageCount { get; set; }

        public int CreationType { get; set; }
        public DateTime CreationDate { get; set; }
        public int DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public int? CurrencyId { get; set; }
        public DateTime? StartDate { get; set; } // Kiralama aralığı başlangıcı
        public DateTime? EndDate { get; set; } // Kiralama aralığı bitişi
        public DateTime? CouponStartDate { get; set; } // Kupon geçerlilik aralığı başlangıcı
        public DateTime? CouponEndDate { get; set; } // Kupon geçerlilik aralığı bitişi
        public int? MinimumDay { get; set; }
        public int? MaximumDay { get; set; }
        public decimal? MinimumAmount { get; set; }
        public decimal? MaximumAmount { get; set; }
        public int? PickupLocation { get; set; }
        public int? ReturnLocation { get; set; }
        public int? VendorId { get; set; }
        public int? MaxUsageCount { get; set; }
        public long? ConnectedReservationId { get; set; } // Foreign Key olarak bağlı değil!
        public bool? ShowPrice { get; set; }
        public string CustomerMailAddress { get; set; }
        public decimal? VendorDiscountValue { get; set; }
        public bool? AnyoneCanUse { get; set; }
        public bool? UseOncePerEMail { get; set; } // TODO : Bu alan gereksiz oldu kaldırılacak
        public bool? LimitedUsage { get; set; } // Kısıtlı kullanım mı?
        public string LimitedUsageField { get; set; } // Kısıtlı kullanım hangi alana bağlı
        public int? LimitedUsageCount { get; set; } // Maksimum kullanım sayısı
        public bool? ShowInVehicleList { get; set; } // Araç listesinde araçların yanında gösterilsin mi?
    }
}
