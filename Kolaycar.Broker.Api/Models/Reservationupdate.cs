using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Reservationupdate
    {
        public int Id { get; set; }
        public long Reservationid { get; set; }
        public string Reservatinonumber { get; set; }
        public int Propertyid { get; set; }
        public string Oldvalue { get; set; }
        public string Newvalue { get; set; }
        public DateTime Updatedate { get; set; }
        public int Userid { get; set; }
        public string Usernamesurname { get; set; }
        public Guid Uniqueid { get; set; }
        public int? Additionalproductid { get; set; }
    }
}
