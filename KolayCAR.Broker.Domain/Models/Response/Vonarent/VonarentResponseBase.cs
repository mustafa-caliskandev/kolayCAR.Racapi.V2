using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response.Vonarent;

public class VonarentResponseBase
{
    public string item { get; set; }
    public int status { get; set; }
    public string message { get; set; }
    public Dictionary<string, List<string>> errors { get; set; }
}
