# Authentication

## Learning Objectives

- Distinguish authentication from authorization and explain why the separation matters in API design.
- Describe the ASP.NET Core authentication model: schemes, handlers, and `HttpContext.User`.
- Explain claims-based identity and how claims represent user attributes.
- Walk through the JWT bearer authentication flow from token issuance to request validation.
- Configure `AddAuthentication` and `AddJwtBearer` in `Program.cs` and apply the `[Authorize]` attribute to controllers.

---

## Why This Matters

The Product Catalog API you built on Monday and extended on Tuesday is currently wide open -- any caller can read, create, update, or delete data without identifying themselves. In production, that is unacceptable for any non-public resource. Authentication is the mechanism that answers the question "Who is making this request?" before the application decides what that caller is allowed to do.

This week's epic is *From Framework to Production-Ready API*. A production-ready API must verify caller identity. Understanding ASP.NET Core's authentication model -- and specifically JSON Web Tokens (JWT) -- gives you the industry-standard approach used across virtually every modern web API and microservices system.

---

## Authentication vs. Authorization

These two terms are frequently conflated, but they describe distinct steps in the security pipeline.

| Term | Question answered | Example |
|---|---|---|
| **Authentication** | Who are you? | Verifying that the caller is user `alice@example.com` |
| **Authorization** | What are you allowed to do? | Checking that Alice has the `Admin` role before allowing a DELETE |

ASP.NET Core enforces this order in its middleware pipeline: authentication middleware runs before authorization middleware. A caller who fails authentication never reaches the authorization check. We will cover authorization in depth tomorrow.

---

## The ASP.NET Core Authentication Model

ASP.NET Core's authentication system is built around three concepts:

### Authentication Schemes

A scheme is a named configuration that tells the framework which authentication handler to invoke. A single application can register multiple schemes -- JWT bearer for its API endpoints and cookies for a web UI, for example. Each scheme has a string name used to reference it.

### Authentication Handlers

A handler is the code that reads the incoming request, validates the credential it finds, and populates `HttpContext.User` if validation succeeds. The `Microsoft.AspNetCore.Authentication.JwtBearer` package provides the `JwtBearerHandler`, which:

1. Reads the `Authorization: Bearer <token>` request header.
2. Validates the token's signature, issuer, audience, and expiry.
3. Extracts the token's claims and populates `HttpContext.User` as a `ClaimsPrincipal`.

### `HttpContext.User`

Every authenticated request carries a `ClaimsPrincipal` on `HttpContext.User`. This principal contains one or more `ClaimsIdentity` objects, each of which holds a collection of `Claim` instances.

```csharp
// Accessing the current user inside a controller action
string? username = HttpContext.User.FindFirstValue(ClaimTypes.Name);
bool isAdmin   = HttpContext.User.IsInRole("Admin");
```

If the request is unauthenticated, `HttpContext.User.Identity?.IsAuthenticated` returns `false`.

---

## Claims-Based Identity

A **claim** is a name-value pair that asserts something about the user. Claims are embedded inside the JWT by the issuing server and can represent any attribute.

Common standard claim types (defined in `System.Security.Claims.ClaimTypes`):

| Claim type constant | URI shorthand | Typical value |
|---|---|---|
| `ClaimTypes.Name` | `http://schemas.../name` | `"alice"` |
| `ClaimTypes.Email` | `http://schemas.../emailaddress` | `"alice@example.com"` |
| `ClaimTypes.Role` | `http://schemas.../role` | `"Admin"` |
| `ClaimTypes.NameIdentifier` | `http://schemas.../nameidentifier` | `"user-guid-123"` |

JWT tokens also use shorter standard claim names defined by RFC 7519 (`sub`, `name`, `email`, `role`). The `JwtBearerHandler` maps these to the longer `ClaimTypes` URIs by default, though this mapping can be disabled if you prefer to work with the short names directly.

---

## JWT Bearer Authentication Flow

JSON Web Tokens are the dominant credential format for stateless REST APIs. A JWT is a compact, URL-safe token consisting of three Base64URL-encoded sections separated by periods:

```
Header.Payload.Signature
```

- **Header** -- token type and signing algorithm (e.g., `HS256` or `RS256`).
- **Payload** -- the claims (user data and token metadata such as `exp` and `iss`).
- **Signature** -- an HMAC or RSA signature over the header and payload, proving the token has not been tampered with.

### Typical flow

```
Client                          API Server
  |                                 |
  |-- POST /auth/login ------------>|
  |    { username, password }       |
  |                                 |--> Validate credentials
  |                                 |--> Build claims
  |                                 |--> Sign JWT
  |<-- 200 OK { token: "eyJ..." } --|
  |                                 |
  |-- GET /products --------------->|
  |   Authorization: Bearer eyJ...  |
  |                                 |--> JwtBearerHandler validates token
  |                                 |--> Populates HttpContext.User
  |<-- 200 OK [ ... ] -------------|
```

The server **does not store any session state**. Every request is self-contained: the token carries the user's identity and claims. This is the property that makes JWT well-suited for distributed systems and microservices.

---

## Configuring JWT Bearer Authentication

### 1. Add the NuGet package

```xml
<!-- .csproj -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.*" />
```

### 2. Register the scheme in `Program.cs`

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Pull JWT settings from appsettings.json
var jwtKey    = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(options =>
{
    // Set the default scheme so [Authorize] uses JWT bearer by default
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidIssuer              = jwtIssuer,

        ValidateAudience         = true,
        ValidAudience            = jwtAudience,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(jwtKey)),

        ValidateLifetime         = true,       // Rejects expired tokens
        ClockSkew                = TimeSpan.Zero // No tolerance for clock drift
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Order matters: Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();
```

### 3. `appsettings.json` JWT section

```json
{
  "Jwt": {
    "Key": "replace-with-a-long-random-secret-key-in-production",
    "Issuer": "ProductCatalogApi",
    "Audience": "ProductCatalogApiClients"
  }
}
```

> **Note:** Store secrets in environment variables or user secrets in development. Never commit a real signing key to source control. We covered user secrets on Monday in `environment-and-configuration.md`.

---

## Issuing a JWT Token

The login endpoint builds a `JwtSecurityToken`, signs it, and serializes it to a string.

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config) => _config = config;

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Placeholder credential check -- replace with real user lookup tomorrow
        if (request.Username != "alice" || request.Password != "secret")
            return Unauthorized();

        var claims = new[]
        {
            new Claim(ClaimTypes.Name,           request.Username),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role,           "User")
        };

        var key   = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:   _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims:   claims,
            expires:  DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}

public record LoginRequest(string Username, string Password);
```

---

## Protecting Endpoints with `[Authorize]`

Once authentication is configured, applying `[Authorize]` to a controller or action requires a valid, unexpired token on every request to that endpoint.

```csharp
[ApiController]
[Route("products")]
[Authorize]                    // All actions require authentication
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() { /* ... */ }

    [HttpPost]
    public IActionResult Create([FromBody] CreateProductDto dto) { /* ... */ }

    [AllowAnonymous]           // Override [Authorize] for a specific action
    [HttpGet("public-count")]
    public IActionResult GetCount() { /* ... */ }
}
```

- **401 Unauthorized** is returned by the `JwtBearerHandler` challenge when no token is present or the token is invalid.
- **403 Forbidden** is returned by authorization middleware when the user is authenticated but lacks the required permission. We will explore this in depth in `authorization.md`.

---

## Token Validation Parameters in Detail

| Parameter | Purpose |
|---|---|
| `ValidateIssuer` | Ensures the `iss` claim matches your expected issuer. |
| `ValidateAudience` | Ensures the `aud` claim matches your expected audience. |
| `ValidateIssuerSigningKey` | Verifies the token signature with your secret key. |
| `ValidateLifetime` | Rejects tokens past their `exp` claim. |
| `ClockSkew` | Tolerance for clock differences between servers. Set to `TimeSpan.Zero` for strict validation. |

Disabling any of these in production is a security risk and should only be done for local debugging with full awareness of the implications.

---

## Summary

- **Authentication** identifies who is making the request; **authorization** decides what they can do.
- ASP.NET Core uses **schemes** and **handlers** to separate authentication mechanism from application logic.
- A **claim** is a name-value pair embedded in the token describing an attribute of the authenticated user.
- **JWT bearer tokens** are stateless, self-contained credentials validated entirely by the receiving server without database lookups.
- Configure JWT with `AddAuthentication().AddJwtBearer(...)` and always place `UseAuthentication()` before `UseAuthorization()` in the middleware pipeline.
- `[Authorize]` requires a valid token; `[AllowAnonymous]` exempts specific actions.
- Never store signing keys in source code. Use environment variables or the user secrets mechanism covered on Monday.

---

## Additional Resources

- [ASP.NET Core Authentication overview (Microsoft Docs)](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- [JWT bearer authentication in ASP.NET Core (Microsoft Docs)](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [JSON Web Tokens -- Introduction (jwt.io)](https://jwt.io/introduction)
