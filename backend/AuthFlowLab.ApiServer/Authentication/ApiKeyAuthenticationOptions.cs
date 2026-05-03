using Microsoft.AspNetCore.Authentication;

namespace AuthFlowLab.ApiServer.Authentication;

public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public List<ApiKeyCredential> Keys { get; init; } = [];
}

public sealed class ApiKeyCredential
{
    public string Name { get; init; } = string.Empty;

    public string Value { get; init; } = string.Empty;
}
