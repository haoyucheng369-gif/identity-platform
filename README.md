# Identity Platform

Identity Platform is a full-stack identity and access system built with ASP.NET Core and React. It implements a custom OAuth2/OIDC authorization server, protected APIs, browser login, service-to-service access, Microsoft Entra ID federation, and a BFF token-handling model.

The project focuses on enterprise identity boundaries: token issuance, JWT validation, scopes and roles, federated sign-in, browser session protection, API access control, and token handling outside the browser.

## Core Capabilities

- Custom OAuth2/OIDC authorization server with authorization code + PKCE, client credentials, discovery, UserInfo, and JWKS.
- API server accepting both first-party JWTs and direct Microsoft Entra ID access tokens.
- BFF backend that stores access tokens server-side and gives the browser only an HttpOnly session cookie.
- React SPA with local login, federated SSO, direct Entra login, token inspection, and claims inspection.
- Authorization policies for scopes, roles, service tokens, and API keys.
- Backend tests, Docker support, and HTTP request collections.

## Authentication Flows

The platform supports five authentication modes. Each mode has a different token path.

| Mode | Flow | Purpose | Token used by API |
| --- | --- | --- | --- |
| 1 | Local login | First-party browser sign-in | Authorization Server JWT |
| 2 | Federated SSO | Entra proves user identity, local system maps permissions | Authorization Server JWT |
| 3 | Direct Entra login | SPA uses Entra token directly | Entra access token |
| 4 | BFF login | Browser keeps tokens out of JavaScript | BFF-held access token |
| 5 | Client credentials | Service-to-service access | Authorization Server service token |

### Mode 1: Local Login

```mermaid
flowchart LR
    Browser[React SPA / Browser]
    Auth[Authorization Server]
    API[Protected API]

    Browser -->|1. authorize + login with PKCE| Auth
    Auth -->|2. authorization code| Browser
    Browser -->|3. code + code_verifier| Auth
    Auth -->|4. first-party JWT| Browser
    Browser -->|5. Bearer first-party JWT| API
```

### Mode 2: Federated SSO

```mermaid
flowchart LR
    Browser[React SPA / Browser]
    Auth[Authorization Server]
    Entra[Microsoft Entra ID]
    API[Protected API]

    Browser -->|1. authorize + login with PKCE| Auth
    Auth -->|2. external OIDC challenge| Entra
    Entra -->|3. external identity| Auth
    Auth -->|4. authorization code| Browser
    Browser -->|5. code + code_verifier| Auth
    Auth -->|6. mapped first-party JWT| Browser
    Browser -->|7. Bearer first-party JWT| API
```

### Mode 3: Direct Entra Login

```mermaid
flowchart LR
    Browser[React SPA / Browser]
    Entra[Microsoft Entra ID]
    API[Protected API]

    Browser -->|1. MSAL login| Entra
    Entra -->|2. Entra access token| Browser
    Browser -->|3. Bearer Entra access token| API
```

### Mode 4: BFF Login

```mermaid
flowchart LR
    Browser[React SPA / Browser]
    BFF[BFF Backend]
    Auth[Authorization Server]
    API[Protected API]

    Browser -->|1. start login| BFF
    BFF -->|2. redirect with BFF-generated code_challenge| Browser
    Browser -->|3. follow redirect and sign in| Auth
    Auth -->|4. redirect to BFF callback with code| Browser
    Browser -->|5. callback with code and state| BFF
    BFF -->|6. exchange code with client_id + client_secret + BFF code_verifier| Auth
    Auth -->|7. access token to BFF| BFF
    BFF -->|8. HttpOnly session cookie| Browser
    Browser -->|9. cookie API request| BFF
    BFF -->|10. Bearer access token| API
```

### Mode 5: Client Credentials

```mermaid
flowchart LR
    Service[Worker Service]
    Auth[Authorization Server]
    API[Protected API]

    Service -->|1. client_id + client_secret| Auth
    Auth -->|2. service JWT| Service
    Service -->|3. Bearer service JWT| API
```

## Detailed Flow Sequences

These sequence diagrams show the request order behind each mode.

### Mode 1: Local Login Sequence

```mermaid
sequenceDiagram
    participant Browser as React SPA / Browser
    participant Auth as Authorization Server
    participant API as Protected API

    Browser->>Auth: /connect/authorize with code_challenge
    Auth-->>Browser: Login page if no Auth cookie
    Browser->>Auth: Submit username/password
    Auth-->>Browser: Auth cookie + redirect with authorization code
    Browser->>Auth: /connect/token with code_verifier
    Auth-->>Browser: access_token and id_token
    Browser->>API: Bearer access_token
    API-->>Browser: Protected content
```

### Mode 2: Federated SSO Sequence

```mermaid
sequenceDiagram
    participant Browser as React SPA / Browser
    participant Auth as Authorization Server
    participant Entra as Microsoft Entra ID
    participant API as Protected API

    Browser->>Auth: /connect/authorize with code_challenge
    Auth-->>Browser: Login page with Microsoft option
    Browser->>Auth: Start external login
    Auth->>Entra: OIDC challenge
    Entra-->>Auth: External identity
    Auth-->>Browser: Auth cookie + redirect with authorization code
    Browser->>Auth: /connect/token with code_verifier
    Auth-->>Browser: first-party access_token and id_token
    Browser->>API: Bearer first-party access_token
    API-->>Browser: Protected content
```

### Mode 3: Direct Entra Login Sequence

```mermaid
sequenceDiagram
    participant Browser as React SPA / Browser
    participant Entra as Microsoft Entra ID
    participant API as Protected API

    Browser->>Entra: MSAL login redirect
    Entra-->>Browser: Entra access token
    Browser->>API: Bearer Entra access token
    API-->>Browser: Protected content
```

### Mode 4: BFF Login Sequence

```mermaid
sequenceDiagram
    participant Browser as React SPA / Browser
    participant BFF as BFF Backend
    participant Auth as Authorization Server
    participant API as Protected API

    Browser->>BFF: /bff/login
    BFF-->>Browser: Redirect to /connect/authorize with BFF code_challenge
    Browser->>Auth: Follow redirect and sign in
    Auth-->>Browser: Redirect to BFF callback with authorization code
    Browser->>BFF: /bff/callback with code and state
    BFF->>Auth: /connect/token with client_id, client_secret, and BFF code_verifier
    Auth-->>BFF: access_token
    BFF-->>Browser: HttpOnly session cookie
    Browser->>BFF: API request with session cookie
    BFF->>API: Bearer access_token
    API-->>BFF: Protected content
    BFF-->>Browser: Response
```

### Mode 5: Client Credentials Sequence

```mermaid
sequenceDiagram
    participant Service as Worker Service
    participant Auth as Authorization Server
    participant API as Protected API

    Service->>Auth: /connect/token with client_id and client_secret
    Auth-->>Service: service access_token
    Service->>API: Bearer service access_token
    API-->>Service: Service-only content
```

## Authorization Model

| Access type | Example |
| --- | --- |
| User scopes | `content.read`, `content.write` |
| Roles | `Admin` |
| Service tokens | `token_type=service` |
| Entra scopes | `access_as_user`, optional `write_as_user` |
| API key | `X-Api-Key` for internal tool access |

## Run Locally

Start the full stack:

```powershell
docker compose up --build
```

Run services directly:

```powershell
dotnet run --project backend\EnterpriseIdentityPlatform.AuthServer\EnterpriseIdentityPlatform.AuthServer.csproj --urls http://localhost:5001
dotnet run --project backend\EnterpriseIdentityPlatform.ApiServer\EnterpriseIdentityPlatform.ApiServer.csproj --urls http://localhost:5002
dotnet run --project backend\EnterpriseIdentityPlatform.Bff\EnterpriseIdentityPlatform.Bff.csproj --urls http://localhost:5003
```

Run the web client:

```powershell
cd frontend\EnterpriseIdentityPlatform.Web
npm install
npm run dev
```

Open `http://localhost:5173`.

## Local Credentials

| Type | Identifier | Secret | Access |
| --- | --- | --- | --- |
| User | `user` | `user123` | `content.read` |
| User | `admin` | `admin123` | `content.read content.write`, `Admin` role |
| Client | `worker-service` | `worker-secret` | `content.read content.write` |
| SPA | `demo-spa` | none | `openid profile content.read content.write` |
| BFF | `demo-bff` | `bff-secret` | `openid profile content.read content.write` |
| API key | `internal-tool` | `dev-api-key-123` | `X-Api-Key` |

## Project Structure

```text
backend/
  EnterpriseIdentityPlatform.AuthServer       OAuth2/OIDC server, login, token issuance, JWKS
  EnterpriseIdentityPlatform.ApiServer        protected API and authorization policies
  EnterpriseIdentityPlatform.Bff              server-side token session and CSRF-protected proxy
  *.Tests                                     endpoint and authentication tests
frontend/
  EnterpriseIdentityPlatform.Web              React client for login modes and API inspection
```

## Verify

```powershell
dotnet test backend\EnterpriseIdentityPlatform.sln
cd frontend\EnterpriseIdentityPlatform.Web
npm install
npm run build
```

HTTP examples are available in:

- `backend/EnterpriseIdentityPlatform.http`
- `backend/EnterpriseIdentityPlatform.AuthServer/EnterpriseIdentityPlatform.AuthServer.http`
- `backend/EnterpriseIdentityPlatform.ApiServer/EnterpriseIdentityPlatform.ApiServer.http`
