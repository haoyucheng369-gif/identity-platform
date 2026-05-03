using System.Text.Json.Serialization;

namespace AuthFlowLab.AuthServer.Models;

// OAuth2/OIDC token endpoint response. id_token is present only for OpenID Connect requests.
public sealed record TokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("scope")] string Scope,
    [property: JsonPropertyName("id_token")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? IdToken = null);
