namespace KolayCAR.Broker.Domain.Models.Response.Yolcu360v2;

public class Yolcu360v2AuthResponse
{
    public string accessToken { get; set; }
    public string accessTokenExpireAt { get; set; }
    public string refreshToken { get; set; }
    public string refreshTokenExpireAt { get; set; }
}
