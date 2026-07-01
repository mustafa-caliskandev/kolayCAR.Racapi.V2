namespace KolayCAR.Broker.Domain.Models.Requests.Vonarent;

public class VonarentSetCustomerRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public VonarentMobile Mobile { get; set; }
    public string Email { get; set; }
}

public class VonarentMobile
{
    public string code { get; set; }
    public string number { get; set; }
}