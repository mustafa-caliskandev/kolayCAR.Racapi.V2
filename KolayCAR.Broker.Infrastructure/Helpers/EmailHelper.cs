using HtmlAgilityPack;
using KolayCAR.Broker.Domain.Models;
using System.Linq;
using System;

namespace KolayCAR.Broker.Infrastructure.Helpers
{
    public class EmailHelper
    {
        public static string PrepareReservationEmailHtml(string html, ReservationStatusTypes reservationStatusTypes)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var removeMarkers = reservationStatusTypes != ReservationStatusTypes.Cancelled
                ? new[] { "{show_cancel_reservation}", "show_cancel_reservation" }
                : new[] { "{show_new_reservation}", "show_new_reservation" };

            RemoveElementsByAttributeMarkers(doc, removeMarkers);

            if (reservationStatusTypes != ReservationStatusTypes.Cancelled)
            {
                RemoveElementsContainingText(
                    doc,
                    "THIS RESERVATION HAS BEEN CANCELED",
                    "THIS RESERVATION HAS BEEN CANCELLED");
            }

            return doc.DocumentNode.OuterHtml;
        }

        private static void RemoveElementsByAttributeMarkers(HtmlDocument doc, string[] removeMarkers)
        {
            var removeElements = doc.DocumentNode
                .Descendants()
                .Where(node => node.Attributes.Any(attribute =>
                    removeMarkers.Any(marker =>
                        attribute.Value.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)))
                .ToList();

            foreach (var element in removeElements)
            {
                element.Remove();
            }
        }

        private static void RemoveElementsContainingText(HtmlDocument doc, params string[] textMarkers)
        {
            var removeElements = doc.DocumentNode
                .Descendants()
                .Where(node => node.NodeType == HtmlNodeType.Text &&
                    textMarkers.Any(marker =>
                        HtmlEntity.DeEntitize(node.InnerText).IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0))
                .Select(GetRemovableContainer)
                .Where(node => node != null)
                .Distinct()
                .ToList();

            foreach (var element in removeElements)
            {
                element.Remove();
            }
        }

        private static HtmlNode GetRemovableContainer(HtmlNode node)
        {
            var removableNodeNames = new[] { "tr", "div", "table", "p" };

            foreach (var ancestor in node.Ancestors())
            {
                if (ancestor.Name.Equals("body", StringComparison.OrdinalIgnoreCase) ||
                    ancestor.Name.Equals("html", StringComparison.OrdinalIgnoreCase))
                    break;

                if (removableNodeNames.Any(name => ancestor.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    return ancestor;
            }

            return node.ParentNode;
        }
    }
}
