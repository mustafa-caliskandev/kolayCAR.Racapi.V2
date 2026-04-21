using System;

namespace KolayCAR.Broker.Domain.Models.StoredPorcedureModels
{
    public class ReservationReportModel
    {
        public string VendorName { get; set; }
        public string ReservationNumber { get; set; }
        public string ApiReservationNumber { get; set; }
        public DateTime ReservationDate { get; set; }
        public string CustomerName { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public string PickupLocation { get; set; }
        public string ReturnLocation { get; set; }
        public string VendorInvoiceNumber { get; set; }
        public string AgencyInvoiceNumber { get; set; }
        public string ReservationStatus { get; set; }
        public decimal ReservationPrice { get; set; }
        public decimal VendorProgressPayment { get; set; }
        public decimal BrokerProgressPayment { get; set; }
    }
}
