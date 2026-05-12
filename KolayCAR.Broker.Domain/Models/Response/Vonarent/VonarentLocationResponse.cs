using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response.Vonarent;

public class VonarentLocationResponse : VonarentResponseBase
{
    public List<VonarentLocationItem> items { get; set; }
}

public class VonarentLocationItem
{
    public string id { get; set; }
    public string name { get; set; }
}
