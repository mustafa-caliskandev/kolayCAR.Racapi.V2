using HtmlAgilityPack;
using KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.Infrastructure.Helpers
{
    public class EmailHelper
    {
        public static string PrepareReservationEmailHtml(string html, ReservationStatusTypes reservationStatusTypes)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var removeClassName = reservationStatusTypes != ReservationStatusTypes.Cancelled ? "{show_cancel_reservation}" : "{show_new_reservation}";
            var removeElements = doc.DocumentNode
                .SelectNodes("//table[@class='" + removeClassName + "']");

            if (removeElements != null)
            {
                foreach (var element in removeElements)
                {
                    element.Remove();
                }
            }

            return doc.DocumentNode.OuterHtml;
        }
    }
}
