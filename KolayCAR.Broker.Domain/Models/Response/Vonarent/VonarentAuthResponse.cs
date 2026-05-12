namespace KolayCAR.Broker.Domain.Models.Response.Vonarent;

public class VonarentAuthResponse : VonarentResponseBase
{
    public string token { get; set; }
    public string refreshToken { get; set; }
}
