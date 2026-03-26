# AspNetCoreAuthKit

[![NuGet](https://img.shields.io/nuget/v/AspNetCoreAuthKit.svg)](https://www.nuget.org/packages/AspNetCoreAuthKit)
[![Publish to NuGet](https://github.com/simoneM93/AspNetCoreAuthKit/actions/workflows/publish.yml/badge.svg)](https://github.com/simoneM93/AspNetCoreAuthKit/actions/workflows/publish.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/simoneM93/AspNetCoreAuthKit/blob/main/LICENSE)
[![GitHub Sponsors](https://img.shields.io/badge/Sponsor-%E2%9D%A4-ea4aaa?logo=github-sponsors)](https://github.com/sponsors/simoneM93)
[![Changelog](https://img.shields.io/badge/Changelog-view-blue)](https://github.com/simoneM93/AspNetCoreAuthKit/blob/main/CHANGELOG.md)

A zero-friction authentication & authorization toolkit for ASP.NET Core with JWT Bearer, API Key, Refresh Tokens, and dynamic claim policies — all wired up with a single fluent call.

> **Why AspNetCoreAuthKit?**
> Every ASP.NET Core project ends up writing the same boilerplate:
> JWT configuration, API Key middleware, refresh token logic, claim-based policies.
> AspNetCoreAuthKit wraps all of this into a clean, testable, opt-in abstraction.

---

## ✨ Features

- 🔐 **JWT Bearer** — generate, validate and refresh tokens with one method call
- 🔑 **API Key** — header or query string, static list or custom async validator
- ♻️ **Refresh Tokens** — token rotation, revoke single device or all devices
- 🛡️ **Dynamic claim policies** — `[RequireClaim("role", "admin")]` without manual registration
- 📋 **Common policies** — `AdminOnly`, `AuthenticatedUser`, `ApiKeyOrJwt` out of the box
- 🔀 **Multi-scheme** — JWT and API Key in parallel on the same app
- 🧩 **Extensible** — plug in your own `IRefreshTokenStore` (Redis, EF Core, etc.)
- 🧪 **DI-ready** — every module is opt-in, nothing is registered unless you ask

---

## 📋 Requirements

| Requirement | Minimum version |
|---|---|
| .NET | 8.0+ |
| ASP.NET Core | 8.0+ |

---

## 🚀 Installation

```bash
dotnet add package AspNetCoreAuthKit
```

---

## 🎯 Quick Start

### 1. Register services

```csharp
// Program.cs
builder.Services.AddAuthKit(opt => opt
    .UseJwt(jwt =>
    {
        jwt.SecretKey         = builder.Configuration["Auth:SecretKey"]!;
        jwt.Issuer            = "myapp";
        jwt.Audience          = "myapp-users";
        jwt.AccessTokenExpiry = TimeSpan.FromMinutes(15);
    })
    .UseApiKey(key =>
    {
        key.HeaderName = "X-Api-Key";
        key.ValidKeys  = builder.Configuration
            .GetSection("Auth:ApiKeys").Get<string[]>()!;
    })
    .UseRefreshTokens()
    .UseAuthorization()
);

app.UseAuthentication();
app.UseAuthorization();
```

### 2. Generate a token at login

```csharp
app.MapPost("/auth/login", async (LoginRequest req,
    IJwtTokenService tokenService,
    IRefreshTokenStore store) =>
{
    // validate credentials...

    var result = tokenService.GenerateToken(new TokenRequest
    {
        Subject = user.Id,
        Claims  = [new Claim("role", user.Role)]
    });

    await store.SaveAsync(result.RefreshToken, new RefreshTokenEntry
    {
        Subject   = user.Id,
        ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
    });

    return Results.Ok(result);
    // { accessToken, refreshToken, expiresAt }
});
```

### 3. Refresh a token


```csharp
app.MapPost("/auth/refresh", async (RefreshRequest req,
    IRefreshTokenService refreshService) =>
{
    try
    {
        var result = await refreshService.RefreshAsync(
            req.AccessToken,
            req.RefreshToken);
        return Results.Ok(result);
    }
    catch (RefreshTokenException)
    {
        return Results.Unauthorized();
    }
});
```

### 4. Protect edpoints with policies

```csharp
// Built-in policies
app.MapDelete("/users/{id}", ...).RequireAuthorization(CommonPolicies.AdminOnly);
app.MapGet("/dashboard", ...).RequireAuthorization(CommonPolicies.ApiKeyOrJwt);

// Declarative claim attribute
[RequireClaim("role", "admin")]
public IActionResult DeleteUser(int id) => Ok();

// Multiple accepted values (OR)
[RequireClaim("role", "admin", "moderator")]
public IActionResult BanUser(int id) => Ok();
```

---

## 📝 Refresh Tokens

AspNetCoreAuthKit implements token rotation by default: every refresh invalidates the old token and issues a new pair.

| Operation | Method |
|---|---|
| Refresh access token | ``` refreshService.RefreshAsync(accessToken, refreshToken) ``` |
| Logout (single device) | ``` refreshService.RevokeAsync(refreshToken) ``` |
| Logout (all devices) | ``` refreshService.RevokeAllAsync(subject) ``` |

### Custom store (e.g. Redis or EF Core):

```csharp 
public class RedisRefreshTokenStore : IRefreshTokenStore { ... }

.UseRefreshTokens(rt =>
{
    rt.CustomStore = typeof(RedisRefreshTokenStore);
})
```

---

## 📚 API Reference

``` IJwtTokenService ``` methods

| Method | Description |
|---|---|
| ``` GenerateToken(TokenRequest) ``` | Generates a new access + refresh token pair |
| ``` ValidateToken(string) ``` | Validates a token, returns ``` ClaimsPrincipal? ``` |
| ``` ValidateTokenIgnoreExpiry(string) ``` | Validates ignoring lifetime (used during refresh) |
| ``` GenerateRefreshToken() ``` | Generates a cryptographically secure random token |

``` TokenRequest ``` properties

| Property | Type | Description |
|---|---|---|
| ``` Subject ``` | ``` string ``` | User ID or unique identifier (required) |
| ``` Claims ``` | ``` IEnumerable<Claim> ``` | Additional claims to embed in the token 
| ``` ExpiresIn ``` | ``` TimeSpan? ``` | Per-token expiry override |

``` TokenRequest ``` constats

| Constant | Behaviour |
|---|---|
| ``` CommonPolicies.AuthenticatedUser ``` | Any authenticated user |
| ``` CommonPolicies.AdminOnly ``` | Requires admin role claim |
| ``` CommonPolicies.ApiKeyOrJwt ``` | Accepts either scheme |

---

## ⚙️ Configuration Reference

``` JwtAuthOptions ```

| Option | Type | Default | Description |
|---|---|---|---|
| ``` SecretKey ``` | ``` string ``` | (required) | HMAC signing key |
| ``` Issuer ``` | ``` string ``` | ``` "" ``` | Token issuer |
| ``` Audience ``` | ``` string ``` | ``` "" ``` | Token audience |
| ``` AccessTokenExpiry ``` | ``` TimeSpan ``` | ``` 1 hour ``` | Access token lifetime |
| ``` RefreshTokenExpiry ``` | ``` TimeSpan ``` | ``` 7 days ``` | Refresh token lifetime |
| ``` Algorithm ``` | ``` string ``` | ``` HmacSha256 ``` | Signing algorithm |
| ``` ClockSkew ``` | ``` TimeSpan ``` | ``` 30 seconds ``` | Clock skew tolerance |

``` ApiKeyOptions ```

| Option | Type | Default | Description |
|---|---|---|---|
| ``` HeaderName ``` | ``` string ``` | ``` X-Api-Key ``` | Header to read the key from |
| ``` QueryParamName ``` | ``` string? ``` | ``` null ``` | Query param name (disabled by default) |
| ``` ValidKeys ``` | ``` string[] ``` | ``` [] ``` | Static list of valid keys |
| ``` ValidateKeyAsync ``` | ``` Func<string, Task<bool>>? ``` | ``` null ``` | Custom async validator (overrides ``` ValidKeys ```) |

``` AuthKitAuthorizationOptions ```
| Option | Type | Default | Description |
|---|---|---|---|
| ``` RoleClaimType ``` | ``` string ``` | ``` "role" ``` | Claim type used for roles |
| ``` AdminRoleValue ``` | ``` string ``` | ``` "admin" ``` | Value that identifies an admin |

---

## 🧪 Testing

``` IJwtTokenService ``` and ``` IRefreshTokenStore ``` are plain interfaces — mock them directly in unit tests:

```csharp 
var tokenServiceMock = new Mock<IJwtTokenService>();

tokenServiceMock
    .Setup(s => s.GenerateToken(It.IsAny<TokenRequest>()))
    .Returns(new TokenResult
    {
        AccessToken  = "mocked.jwt.token",
        RefreshToken = "mocked-refresh-token",
        ExpiresAt    = DateTimeOffset.UtcNow.AddMinutes(15)
    });
```

---

## ❤️ Support

If you find AspNetCoreAuthKit useful, consider sponsoring its development.

[![Sponsor simoneM93](https://img.shields.io/badge/Sponsor-%E2%9D%A4-ea4aaa?logo=github-sponsors&style=for-the-badge)](https://github.com/sponsors/simoneM93)

---

## 📄 License

MIT — see [LICENSE](https://github.com/simoneM93/AspNetCoreAuthKit/blob/main/LICENSE) for details.