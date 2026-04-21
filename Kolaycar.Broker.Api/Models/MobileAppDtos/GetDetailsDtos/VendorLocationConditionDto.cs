using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.GetDetailsDtos
{
    public class VendorLocationConditionDto
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public int LocationId { get; set; }
        public string Conditions { get; set; }
        public List<string> Notes { get; set; }
    }
}
