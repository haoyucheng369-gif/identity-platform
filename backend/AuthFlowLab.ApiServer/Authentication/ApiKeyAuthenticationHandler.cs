using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace AuthFlowLab.ApiServer.Authentication;

public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // API keys are validated locally by ApiServer. They do not call AuthServer or create JWTs.
        if (!Request.Headers.TryGetValue(ApiKeyAuthenticationDefaults.HeaderName, out var headerValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var apiKey = headerValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("The API key is empty."));
        }

        var credential = Options.Keys.FirstOrDefault(key => key.Value == apiKey);
        if (credential is null)
        {
            return Task.FromResult(AuthenticateResult.Fail("The API key is invalid."));
        }

        // Once authenticated, expose claims so authorization policies can make finer decisions.
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, credential.Name),
            new Claim(ClaimTypes.Name, credential.Name),
            new Claim("token_type", "api_key"),
            new Claim("api_key_name", credential.Name)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
