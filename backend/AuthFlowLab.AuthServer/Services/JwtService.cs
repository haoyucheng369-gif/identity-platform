using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthFlowLab.AuthServer.Models;
using AuthFlowLab.AuthServer.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthFlowLab.AuthServer.Services;

public class JwtService
{
    private readonly IConfiguration _configuration;
    private readonly AuthOptions _authOptions;
    private readonly RsaKeyService _rsaKeyService;

    public JwtService(
        IConfiguration configuration,
        IOptions<AuthOptions> authOptions,
        RsaKeyService rsaKeyService)
    {
        _configuration = configuration;
        _authOptions = authOptions.Value;
        _rsaKeyService = rsaKeyService;
    }

    public TokenResponse GenerateUserToken(
        string username,
        string role,
        IEnumerable<string> scopes,
        string? clientId = null,
        string? nonce = null)
    {
        var scopeList = scopes.ToList();
        var scope = string.Join(' ', scopeList);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role),
            new("scope", scope),
            new("token_type", "user")
        };

        var accessToken = GenerateJwt(claims, GetApiAudience());
        var idToken = scopeList.Contains("openid", StringComparer.Ordinal)
            ? GenerateIdToken(username, role, clientId, nonce)
            : null;

        return CreateTokenResponse(accessToken, scope, idToken);
    }

    public TokenResponse GenerateServiceToken(string clientId, IEnumerable<string> scopes)
    {
        var scope = string.Join(' ', scopes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, clientId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64),
            new("client_id", clientId),
            new("scope", scope),
            new("token_type", "service")
        };

        return CreateTokenResponse(GenerateJwt(claims, GetApiAudience()), scope);
    }

    private string GenerateIdToken(string username, string role, string? clientId, string? nonce)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new InvalidOperationException("OIDC id_token requires a client id audience.");
        }

        /*
         * id_token is for the client application, not for the API.
         * Its aud is the OAuth/OIDC client_id, while access_token aud is the API audience.
         */
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Name, username),
            new(ClaimTypes.Role, role)
        };

        if (!string.IsNullOrWhiteSpace(nonce))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, nonce));
        }

        return GenerateJwt(claims, clientId);
    }

    private string GenerateJwt(List<Claim> claims, string audience)
    {
        var token = new JwtSecurityToken(
            issuer: GetIssuer(),
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_authOptions.AccessTokenMinutes),
            signingCredentials: _rsaKeyService.CreateSigningCredentials());

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private TokenResponse CreateTokenResponse(string accessToken, string scope, string? idToken = null)
    {
        return new TokenResponse(
            accessToken,
            "Bearer",
            _authOptions.AccessTokenMinutes * 60,
            scope,
            idToken);
    }

    private string GetIssuer()
    {
        return _configuration["Jwt:Issuer"] ?? "http://127.0.0.1:5001";
    }

    private string GetApiAudience()
    {
        return _configuration["Jwt:Audience"] ?? "api-server";
    }
}
