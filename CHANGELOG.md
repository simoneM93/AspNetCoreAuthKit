# AspNetCoreAuthKit

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

---

## [1.0.0] - YYYY-MM-DD

### Added
- Initial release
- `IJwtTokenService` interface with `GenerateToken`, `ValidateToken`, `ValidateTokenIgnoreExpiry`, `GenerateRefreshToken`
- `JwtTokenService` — implementation with cryptographically secure token generation via `RandomNumberGenerator`
- `JwtAuthOptions` — configurable `SecretKey`, `Issuer`, `Audience`, `AccessTokenExpiry`, `RefreshTokenExpiry`, `Algorithm`, `ClockSkew`
- `TokenRequest` record with `Subject`, `Claims`, and per-token `ExpiresIn` override
- `TokenResult` record with `AccessToken`, `RefreshToken`, `ExpiresAt`
- `AddSimpleJwtBearer(Action<JwtAuthOptions>)` extension method
- Disabled `InboundClaimTypeMap` to preserve original JWT claim names (`sub`, `role`, etc.) without WS-Federation mapping
- `ClockSkew` exposed as configurable option (default: 30 seconds)
- `ApiKeyAuthHandler` — native `IAuthenticationHandler` implementation for API Key scheme
- `ApiKeyOptions` — configurable `HeaderName`, `QueryParamName`, `ValidKeys`, `ValidateKeyAsync`
- Support for both header-based and query string-based API Key extraction
- Custom async validator delegate (`Func<string, Task<bool>>`) for database or external lookups
- `AddApiKeyAuthentication(Action<ApiKeyOptions>)` extension method
- `RequireClaimAttribute` — declarative claim-based authorization without manual policy registration
- `DynamicClaimPolicyProvider` — builds `[RequireClaim]` policies dynamically at runtime via `IAuthorizationPolicyProvider`
- `CommonPolicies` — built-in named policies: `AuthenticatedUser`, `AdminOnly`, `ApiKeyOrJwt`
- `AuthKitAuthorizationOptions` — configurable `RoleClaimType`, `AdminRoleValue`, `JwtScheme`, `ApiKeyScheme`
- Multi-scheme policy (`ApiKeyOrJwt`) auto-registered when both JWT and API Key modules are active
- `IRefreshTokenStore` — abstraction for refresh token persistence
- `InMemoryRefreshTokenStore` — thread-safe default implementation via `ConcurrentDictionary`
- `RefreshTokenEntry` record with `Subject`, `ExpiresAt`, `IsRevoked`, `CreatedAt`, `Metadata`
- `IRefreshTokenService` / `RefreshTokenService` — `RefreshAsync` with **token rotation**, `RevokeAsync`, `RevokeAllAsync`
- `RefreshTokenException` — typed exception for invalid, expired or revoked refresh tokens
- `AddRefreshTokens(Action<RefreshTokenStoreOptions>?)` extension method with opt-in custom store via `RefreshTokenStoreOptions.CustomStore`
- `AddAuthKit(Action<AuthKitOptions>)` — single fluent entry point that orchestrates all modules
- `AuthKitOptions` — master builder with `UseJwt`, `UseApiKey`, `UseAuthorization`, `UseRefreshTokens` (all opt-in)
- `AuthKitDefaults` — centralized constants for scheme names, claim types, header names and policy names
- Unit test suite (`AspNetCoreAuthKit.Tests`) — 26 tests covering JWT generation/validation, API Key integration via `TestServer`, refresh token store, refresh flow with token rotation, and dynamic claim policy provider
- MIT license

---

[Unreleased]: https://github.com/simoneM93/AspNetCoreCacheKit/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/simoneM93/AspNetCoreCacheKit/releases/tag/v1.0.0
