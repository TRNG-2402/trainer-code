# Session Management

## Learning Objectives

- Explain the difference between stateless and stateful request handling.
- Configure ASP.NET Core session middleware with `AddSession` and `UseSession`.
- Store and retrieve values from `HttpContext.Session` using the typed extension methods.
- Describe the role of the session cookie and how ASP.NET Core uses it to correlate requests.
- Configure session options including timeout, cookie name, and `HttpOnly` behavior.
- Understand the conceptual difference between in-process session and distributed session.

---

## Why This Matters

HTTP is a stateless protocol by design: each request is independent and carries no inherent memory of previous interactions. For a pure REST API backed by JWT authentication (as covered in `authentication.md`), statelessness is a feature -- it enables horizontal scaling with no shared state between server instances.

However, some application requirements are genuinely stateful. Shopping carts, multi-step wizards, per-user preference caches, and rate-limit counters are all examples of server-side state that must persist across multiple requests from the same user without being embedded in a JWT or sent as a request body each time. Session management is the mechanism ASP.NET Core provides for that use case.

Understanding sessions also deepens your mental model of the HTTP layer: you will see exactly how a bare cookie is used to correlate requests, which reinforces the material in `cookies.md` that follows.

---

## Stateless vs. Stateful

| Property | Stateless (JWT/REST) | Stateful (Session) |
|---|---|---|
| Server memory | No per-user state on server | Server holds a session store entry per user |
| Horizontal scaling | Easy -- any instance handles any request | Requires shared session store (Redis, SQL) or sticky sessions |
| Token size | Grows with claims (sent on every request) | Tiny session ID cookie; data stays server-side |
| Security surface | Token holds data; theft = impersonation | Session ID is a pointer; server validates it |
| Typical use | APIs, microservices | Web apps, multi-step flows, per-user server state |

For an API-first application, stateless JWT is preferred. Sessions are the right tool when you need lightweight, server-managed per-user storage that does not belong in the JWT payload.

---

## How Session Works

1. On the first request, the server generates a unique **session ID** (a random, opaque string).
2. The session ID is sent to the browser as a **cookie** (typically named `.AspNetCore.Session` by default).
3. A corresponding entry is created in the **session store** (in-memory by default) keyed by that ID.
4. On subsequent requests, the browser sends the cookie automatically; the middleware looks up the session ID in the store and attaches the stored data to `HttpContext.Session`.
5. At the end of the response, any mutations to `HttpContext.Session` are persisted back to the store.

---

## Configuring Session Middleware

### 1. Register services

```csharp
builder.Services.AddDistributedMemoryCache();  // In-memory session store (development only)

builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromMinutes(30); // Session expires after 30 min of inactivity
    options.Cookie.Name        = ".MyApp.Session";
    options.Cookie.HttpOnly    = true;  // Inaccessible to JavaScript
    options.Cookie.IsEssential = true;  // Not subject to consent middleware blocking
});
```

`AddDistributedMemoryCache` registers an `IDistributedCache` implementation that stores data in the process's memory. This is suitable for development and single-instance deployments. For production multi-instance deployments, replace this with a distributed provider (discussed below).

### 2. Add middleware to the pipeline

```csharp
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();        // Must come after auth, before endpoint execution

app.MapControllers();
```

`UseSession` must appear after authentication so that session access inside controllers gets authenticated user context if needed. It must appear before endpoint execution so the session data is available inside action methods.

---

## Reading and Writing Session Data

`HttpContext.Session` exposes `ISession`, which stores data as byte arrays keyed by strings. The built-in extension methods for strings and integers make common cases ergonomic.

### Storing values

```csharp
// Storing a string
HttpContext.Session.SetString("username", "alice");

// Storing an integer
HttpContext.Session.SetInt32("visit_count", 1);

// Storing arbitrary objects -- serialize to JSON manually
var cart = new ShoppingCart { Items = new List<CartItem>() };
var json = System.Text.Json.JsonSerializer.Serialize(cart);
HttpContext.Session.SetString("cart", json);
```

### Retrieving values

```csharp
// Retrieve a string (returns null if not set)
string? username = HttpContext.Session.GetString("username");

// Retrieve an integer (returns null if not set)
int? count = HttpContext.Session.GetInt32("visit_count");

// Retrieve and deserialize an object
var cartJson = HttpContext.Session.GetString("cart");
ShoppingCart? cart = cartJson is not null
    ? System.Text.Json.JsonSerializer.Deserialize<ShoppingCart>(cartJson)
    : null;
```

### Removing and clearing

```csharp
HttpContext.Session.Remove("username");  // Remove a specific key
HttpContext.Session.Clear();             // Remove all session data
```

---

## Practical Example: Request Counter

The following controller stores a per-user request counter in session and returns the current count in a custom HTTP response header.

```csharp
[ApiController]
[Route("products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service) => _service = service;

    [HttpGet]
    public IActionResult GetAll()
    {
        // Read, increment, and write back the counter
        int count = HttpContext.Session.GetInt32("request_count") ?? 0;
        count++;
        HttpContext.Session.SetInt32("request_count", count);

        // Surface the counter in a response header
        Response.Headers["X-Request-Count"] = count.ToString();

        return Ok(_service.GetAll());
    }
}
```

---

## Session Options Reference

| Option | Type | Default | Purpose |
|---|---|---|---|
| `IdleTimeout` | `TimeSpan` | 20 minutes | Session expires if idle for this duration |
| `IOTimeout` | `TimeSpan` | 1 minute | Maximum time for async session load/save operations |
| `Cookie.Name` | `string` | `.AspNetCore.Session` | Name of the session cookie |
| `Cookie.HttpOnly` | `bool` | `true` | Prevents JavaScript access to the cookie |
| `Cookie.SecurePolicy` | `CookieSecurePolicy` | `None` | Set to `Always` in production to require HTTPS |
| `Cookie.SameSite` | `SameSiteMode` | `Lax` | Controls cross-site cookie behavior |
| `Cookie.IsEssential` | `bool` | `false` | If `true`, cookie is sent regardless of consent policy |

---

## In-Process vs. Distributed Session

### In-process session (development)

`AddDistributedMemoryCache` stores session data in the web server's process memory. This is simple and fast but has two critical limitations:

1. **Not scalable** -- if two server instances handle requests from the same user, the second instance has no access to session data stored on the first.
2. **Not persistent** -- all session data is lost on application restart.

### Distributed session (production)

For production multi-instance deployments, replace `AddDistributedMemoryCache` with a proper distributed cache provider. The interface is `IDistributedCache` and the session middleware uses it transparently -- no code changes in your controllers are required.

```csharp
// Redis (requires StackExchange.Redis NuGet package)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName  = "MyApp:";
});

// SQL Server (requires Microsoft.Extensions.Caching.SqlServer)
builder.Services.AddSqlServerCache(options =>
{
    options.ConnectionString = builder.Configuration
        .GetConnectionString("DefaultConnection");
    options.SchemaName = "dbo";
    options.TableName  = "SessionCache";
});
```

Distributed session stores are covered in more detail in the caching material on Thursday.

---

## Session vs. JWT for State

A common design question is when to put data in a JWT claim vs. in session storage. The rule of thumb:

- **JWT claims** -- data that is needed on most or all requests, changes infrequently, and is acceptable for the client to hold (user ID, role, permissions).
- **Session** -- data that is large, changes frequently, is sensitive enough not to be held client-side, or is temporary and scoped to a single user workflow (shopping cart, wizard state, rate limit counters).

Avoid storing large objects in JWTs. Tokens are sent on every request; a 10 KB token imposes real network overhead. Move that data server-side into session.

---

## Summary

- **Session middleware** provides a server-side, per-user key-value store correlated to the client by a session cookie.
- Register with `AddSession` + `AddDistributedMemoryCache`, and activate with `UseSession` after `UseAuthentication`.
- `HttpContext.Session` exposes `SetString`, `GetString`, `SetInt32`, `GetInt32`, and `Remove` / `Clear` convenience methods.
- The **session cookie** contains only the session ID -- the actual data lives in the session store, not in the browser.
- Set `Cookie.HttpOnly = true` and `Cookie.SecurePolicy = Always` in production.
- In-process memory cache is development-only; production deployments need a distributed store (Redis, SQL Server).
- Prefer JWT for authentication data; use session for transient, per-user server-side state.

---

## Additional Resources

- [Session and state management in ASP.NET Core (Microsoft Docs)](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state)
- [Distributed caching in ASP.NET Core (Microsoft Docs)](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed)
- [IDistributedCache interface reference (Microsoft Docs)](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.distributed.idistributedcache)
