using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Requests.Vonarent;

public class VonarentSelectExtrasRequest
{
    public List<VonarentSelectedExtraItem> Extras { get; set; } = new();
}

public class VonarentSelectedExtraItem
{
    public string Id { get; set; }
    public int Quantity { get; set; }
}
