using Newtonsoft.Json;

namespace FPTPlaygroundServer.Services.Auth.Models;

public class TokenRequest
{
    [JsonProperty(nameof(UserId))]
    public Guid? UserId { get; set; }
    [JsonProperty(nameof(Email))]
    public string Email { get; set; } = default!;
}
