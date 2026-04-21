using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Responses.YesOto
{
    public class YesOtoAgeGroupData
    {
        public string id { get; set; }
        public int? minAge { get; set; }
        public int? maxAge { get; set; }
    }

    // This response is an array directly
    public class YesOtoAgeGroupListResponse : List<YesOtoAgeGroupData> 
    {
    }
}
