using System;

namespace KolayCAR.Broker.API.Models
{
    public class BlockedMember
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public DateTime? BlockedStartDate { get; set; }
        public DateTime? BlockedEndDate { get; set; }
    }
}
