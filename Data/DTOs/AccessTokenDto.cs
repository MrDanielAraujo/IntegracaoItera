using System.Text.Json.Serialization;

namespace IntegracaoItera.Data.DTOs;

public class AccessTokenDto
{
    /// <summary>
    /// Token de acesso JWT.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string Token { get; set; } = string.Empty;
}
