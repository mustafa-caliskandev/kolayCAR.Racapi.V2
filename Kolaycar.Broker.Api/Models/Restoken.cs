using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Restoken
    {
        public long Id { get; set; }
        public string SessionId { get; set; }
        public string Uniqueid { get; set; }
        public byte[] Token { get; set; }
        public DateTime? Recorddate { get; set; }
    }
}
