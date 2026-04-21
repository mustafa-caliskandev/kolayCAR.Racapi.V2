using System;

namespace KolayCAR.Broker.API.Models;

public class KolayCarAuthResponse
{
    public string accessToken { get; set; }
    public string tokenType { get; set; }
    public int expiresIn { get; set; }
    public string scope { get; set; }
    public string tokenId { get; set; }
    public DateTime issuedAt { get; set; }
}
