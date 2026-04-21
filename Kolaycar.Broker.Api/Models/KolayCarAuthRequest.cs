using System;

namespace KolayCAR.Broker.API.Models;

public class KolayCarAuthRequest
{
    public string ApiKey { get; set; }
    public string Password { get; set; }
    public Payload Payload { get; set; }
    public string source { get; set; }

}
public class Payload
{
    public DateTime IssuedAt { get; set; }
    public string SharedSecret { get; set; }
}
