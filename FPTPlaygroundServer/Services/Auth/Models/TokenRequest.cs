using FPTPlaygroundServer.Data.Entities;
using Newtonsoft.Json;

namespace FPTPlaygroundServer.Services.Auth.Models;

public class TokenRequest
{
    [JsonProperty(nameof(UserId))]
    public Guid? UserId { get; set; }
    [JsonProperty(nameof(Email))]
    public string Email { get; set; } = default!;
    [JsonProperty(nameof(Role))]
    public Role Role { get; set; }
}
