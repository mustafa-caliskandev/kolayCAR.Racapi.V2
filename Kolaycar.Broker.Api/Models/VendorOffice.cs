using System;

namespace KolayCAR.Broker.API.Models
{
    public class VendorOffice
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public int VendorId { get; set; }

        public DateTime? OpeningTime { get; set; }
        public DateTime? ClosingTime { get; set; }
        public bool? FlightCardRequired { get; set; }

        /// <summary>
        /// Rezervasyonların web sitesi üzerinden iptal edilebilir olup olmamasını belirler
        /// Eğer true ise site üzerinde iptal seçenekleri çıkmakta, false ise iptal seçenekleri çıkmayıp çağrı merkezlerine yönlendirecek
        /// </summary>
        public bool? ReservationsCancellable { get; set; }
    }
}
