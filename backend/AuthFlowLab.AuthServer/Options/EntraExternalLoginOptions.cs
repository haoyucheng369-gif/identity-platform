namespace AuthFlowLab.AuthServer.Options;

// 这里集中管理 Auth Server 作为客户端去连接 Entra ID 时需要的外部登录配置。
public sealed class EntraExternalLoginOptions
{
    // Enabled=false 时不会显示 Microsoft 登录入口，也不会注册 Entra OIDC handler。
    public bool Enabled { get; init; }

    // Entra tenant 的 OIDC authority，例如 https://login.microsoftonline.com/{tenant-id}/v2.0。
    public string Authority { get; init; } = string.Empty;

    // Auth Server 在 Entra 中注册的 Web 应用 client id。
    public string ClientId { get; init; } = string.Empty;

    // Auth Server 后端安全保存的 client secret，不能放到前端。
    public string ClientSecret { get; init; } = string.Empty;

    // Entra 登录完成后回调 Auth Server 的地址，必须和 Azure Portal 中配置一致。
    public string CallbackPath { get; init; } = "/signin-entra";

    // 用哪个 Entra claim 去匹配本地用户，匹配成功后再使用本地 scopes 和 roles。
    public string UserNameClaim { get; init; } = "preferred_username";

    // 这里只请求登录认证需要的 OIDC scopes，不直接请求业务 API 权限。
    public List<string> Scopes { get; init; } = ["openid", "profile", "email"];
}
