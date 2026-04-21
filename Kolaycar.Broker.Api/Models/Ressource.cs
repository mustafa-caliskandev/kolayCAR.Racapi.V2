using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Ressource
    {
        public int Id { get; set; }
        public string Sourcename { get; set; }
        public bool? IsActive { get; set; }
        public int? Orders { get; set; }
        public DateTime? AddDate { get; set; }
    }
}
