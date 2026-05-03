namespace AuthFlowLab.AuthServer.Options;

public sealed class AuthOptions
{
    // access_token lifetime in minutes. JwtService returns this as expires_in seconds.
    public int AccessTokenMinutes { get; init; } = 30;

    // Lab user store. Real systems normally use a database or ASP.NET Core Identity.
    public List<AuthUser> Users { get; init; } = [];

    // OAuth2 client registry. Each client declares grant types, scopes, and redirect URIs.
    public List<AuthClient> Clients { get; init; } = [];
}

public sealed class AuthUser
{
    public string Username { get; init; } = "";

    public string Password { get; init; } = "";

    public string Role { get; init; } = "";

    public List<string> Scopes { get; init; } = [];
}

public sealed class AuthClient
{
    public string ClientId { get; init; } = "";

    // Lab secret. Real systems should hash it or store it in a secret manager.
    public string ClientSecret { get; init; } = "";

    // Controls which OAuth2 grant types this client may use.
    public List<string> AllowedGrantTypes { get; init; } = [];

    // Controls the maximum API scopes this client may request.
    public List<string> Scopes { get; init; } = [];

    // authorization_code flow must validate redirect_uri against pre-registered values.
    public List<string> RedirectUris { get; init; } = [];
}
