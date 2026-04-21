using System;

namespace KolayCAR.Broker.API.Models
{
    public class ReservationDriverInfo
    {
        public int Id { get; set; }
        public int ReservationDetailId { get; set; }

        public string IdentityNumber { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string CountryPhoneCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public string FlightNumber { get; set; }

        public bool? ContactPermission { get; set; }
        public bool? IsNonTurkishCitizen { get; set; }

        public DateTime? Birthday { get; set; }

        public virtual ReservationDetail ReservationDetail { get; set; }
    }
}
