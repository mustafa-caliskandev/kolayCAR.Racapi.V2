using KolayCAR.Broker.API.Models.Dtos;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models.PaymentDto
{
    public class GetSummaryResponseDto
    {
        public List<BrokerApiExtraDto> Extras { get; set; }
        public BrokerApiVehicleDto Vehicle { get; set; }
        public BrokerApiCouponUsageResultTypes CouponUsageResultType { get; set; }
    }
    public enum BrokerApiCouponUsageResultTypes
    {
        None, // Başarılı
        GeneralError, // Kuponun DB tarafında hatası var
        GreaterThanPaymentAmount, // Kupon miktarı ödeme tutarından büyük
        GreaterThanTotalAmount // Kupon miktarı total tutarından büyük
    }
}
