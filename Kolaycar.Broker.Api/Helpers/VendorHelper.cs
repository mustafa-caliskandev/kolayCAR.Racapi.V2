using KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Helpers
{
    public class VendorHelper
    {
        public static Vendor SetVendorProfitMarkup(Vendor vendor, SubVendor subVendor)
        {
            if (vendor != null && subVendor != null)
            {
                vendor.ProfitMarkupDailyPrice = subVendor.ProfitMarkup ?? vendor.ProfitMarkupDailyPrice;
                vendor.ProfitMarkupAdditionalProducts = subVendor.ProfitMarkupAdditionalProducts ?? vendor.ProfitMarkupAdditionalProducts;
                vendor.ProfitMarkupOneWayFee = subVendor.ProfitMarkupOneWayFee ?? vendor.ProfitMarkupOneWayFee;
            }

            return vendor;
        }
    }
}
