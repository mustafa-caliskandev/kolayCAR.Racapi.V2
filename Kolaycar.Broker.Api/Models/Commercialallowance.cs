using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Commercialallowance
    {
        public int Id { get; set; }
        public long Record { get; set; }
        public int Recordtypeid { get; set; }
        public bool Active { get; set; }
        public DateTime Recorddate { get; set; }
    }
}
