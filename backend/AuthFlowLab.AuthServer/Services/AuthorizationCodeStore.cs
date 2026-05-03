using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace AuthFlowLab.AuthServer.Services;

public sealed class AuthorizationCodeStore
{
    private readonly ConcurrentDictionary<string, AuthorizationCodeRecord> _codes = new();

    public string Create(
        string clientId,
        string redirectUri,
        string username,
        string role,
        IReadOnlyCollection<string> scopes,
        string codeChallenge,
        string codeChallengeMethod,
        string? nonce)
    {
        /*
         * The authorization code is a short-lived one-time credential.
         *
         * It does not contain all data by itself. Instead, this store maps:
         *   code -> client_id, redirect_uri, user, scopes, code_challenge, nonce, expires_at
         *
         * Real systems usually store this in Redis or a database so multiple AuthServer
         * instances can consume the same authorization transaction.
         */
        var code = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
        var record = new AuthorizationCodeRecord(
            code,
            clientId,
            redirectUri,
            username,
            role,
            scopes.ToArray(),
            codeChallenge,
            codeChallengeMethod,
            nonce,
            DateTimeOffset.UtcNow.AddMinutes(5));

        _codes[code] = record;
        return code;
    }

    public bool TryConsume(string code, out AuthorizationCodeRecord? record)
    {
        /*
         * Consume means "read and delete".
         * This enforces the OAuth2 rule that an authorization code can be exchanged only once.
         */
        if (!_codes.TryRemove(code, out record))
        {
            return false;
        }

        if (record.ExpiresAtUtc < DateTimeOffset.UtcNow)
        {
            record = null;
            return false;
        }

        return true;
    }
}

public sealed record AuthorizationCodeRecord(
    string Code,
    string ClientId,
    string RedirectUri,
    string Username,
    string Role,
    IReadOnlyCollection<string> Scopes,
    string CodeChallenge,
    string CodeChallengeMethod,
    string? Nonce,
    DateTimeOffset ExpiresAtUtc);
