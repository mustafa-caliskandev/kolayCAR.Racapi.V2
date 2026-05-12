using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response.Vonarent;

public class VonarentExtraResponse : VonarentResponseBase
{
    public List<VonarentExtraItem> items { get; set; }
}

public class VonarentExtraItem
{
    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string extraCategoryId { get; set; }
    public string extraCategoryName { get; set; }
    public float price { get; set; }
    public string currency { get; set; }
    public int isMultipleSelectable { get; set; }
    public string priceCalculationType { get; set; }
}
