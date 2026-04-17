# CORS

## Learning Objectives

- Explain the browser's same-origin policy and why it exists.
- Define Cross-Origin Resource Sharing (CORS) and describe how preflight requests work.
- Configure CORS in ASP.NET Core using named policies registered in `Program.cs`.
- Apply CORS at the global, controller, and action level using middleware and attributes.
- Identify common CORS configuration mistakes, particularly ordering errors with authentication middleware.

---

## Why This Matters

The Product Catalog API you have been building is consumed by a front-end application running on a different origin -- perhaps a React app at `http://localhost:3000`. Without CORS configuration, every cross-origin fetch from that front end will be blocked by the browser before the request even reaches your controller. The user sees nothing; no useful error is surfaced to the developer without inspecting the browser console.

Understanding CORS is not optional for any API that serves a browser-based client. It is a security boundary enforced by browsers, not by your server, and it must be deliberately configured. Misconfiguring it -- either too restrictively or too permissively -- causes either broken user experiences or security vulnerabilities.

---

## The Concept

### The Same-Origin Policy

A browser enforces the same-origin policy: JavaScript running on page A can only make XMLHttpRequest or `fetch` calls to the same origin as page A. An origin is the combination of scheme (protocol), host, and port.

| Origin A | Origin B | Same Origin? |
|---|---|---|
| `http://localhost:3000` | `http://localhost:3000` | Yes |
| `http://localhost:3000` | `http://localhost:5000` | No (different port) |
| `https://myapp.com` | `http://myapp.com` | No (different scheme) |
| `https://api.myapp.com` | `https://myapp.com` | No (different subdomain) |

The same-origin policy prevents malicious scripts on one page from reading data from a different site while sharing the user's credentials (cookies, session tokens). It is a core browser security mechanism, not a server policy.

### What CORS Is

CORS is a protocol that allows a server to declare which origins are permitted to make cross-origin requests to it. The server communicates this via HTTP response headers, and the browser enforces the declaration.

CORS does not protect the server -- the server receives the request regardless. CORS protects the browser: it decides, based on the server's response headers, whether to expose the response body to the requesting JavaScript. This distinction matters: CORS is not a firewall; it is a browser-side access control mechanism.

### Preflight Requests

For HTTP methods other than GET and HEAD (and POST with certain content types), the browser sends a preliminary `OPTIONS` request before the actual request. This is the preflight. Its purpose is to ask the server: "Are you willing to accept a `PUT` request from `http://localhost:3000` with a `Content-Type: application/json` header?"

A preflight request looks like this:

```
OPTIONS /api/products/1 HTTP/1.1
Origin: http://localhost:3000
Access-Control-Request-Method: PUT
Access-Control-Request-Headers: Content-Type, Authorization
```

The server must respond with the appropriate `Access-Control-Allow-*` headers:

```
HTTP/1.1 204 No Content
Access-Control-Allow-Origin: http://localhost:3000
Access-Control-Allow-Methods: GET, POST, PUT, DELETE
Access-Control-Allow-Headers: Content-Type, Authorization
Access-Control-Max-Age: 86400
```

`Access-Control-Max-Age` tells the browser how long (in seconds) it can cache the preflight result, avoiding a preflight on every subsequent request.

If the preflight response is missing or incorrect, the browser blocks the actual request and reports a CORS error. The actual request never executes from the browser's perspective, though the server may log an `OPTIONS` call.

### Configuring CORS in ASP.NET Core

ASP.NET Core handles CORS through the `AddCors` service and `UseCors` middleware. Responses to preflight requests and the injection of `Access-Control-Allow-Origin` headers are managed automatically when a matching policy is found.

**Step 1 -- Define a named policy in `Program.cs`:**

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontEnd", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });

    options.AddPolicy("AllowAll", policy =>
    {
        // Permissive policy for development only.
        // Do not use in production.
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

**Step 2 -- Enable the middleware in the request pipeline:**

```csharp
// Order matters. UseCors must come before UseAuthentication, UseAuthorization, and MapControllers.
app.UseRouting();

app.UseCors("AllowFrontEnd"); // Must be here, between UseRouting and UseAuthorization.

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
```

> **Ordering is critical.** If `UseCors` is placed after `UseAuthentication`, the CORS headers will not be added to 401 responses. The browser sees a 401 without CORS headers and reports a network error rather than an authentication error -- a confusing failure mode.

### Applying Policies at Different Levels

**Global (all endpoints use the same policy):**

```csharp
app.MapControllers().RequireCors("AllowFrontEnd");
// Or: app.UseCors("AllowFrontEnd"); applies to all middleware after it.
```

**Controller level:**

```csharp
[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowFrontEnd")]
public class ProductsController : ControllerBase
{
    // All actions in this controller use AllowFrontEnd policy.
}
```

**Action level:**

```csharp
[HttpGet]
[EnableCors("AllowAll")] // Override for a single action.
public IActionResult GetPublicData() { ... }
```

**Disabling CORS for a specific action within a globally-enabled controller:**

```csharp
[HttpDelete("{id}")]
[DisableCors]
public IActionResult Delete(int id) { ... }
```

### `AllowCredentials` and the Origin Restriction

When a cross-origin request must include credentials (cookies, `Authorization` headers with `credentials: 'include'` in the `fetch` call), the response must include `Access-Control-Allow-Credentials: true`. ASP.NET Core sets this when you call `.AllowCredentials()` on the policy.

However, CORS does not permit `AllowAnyOrigin()` combined with `AllowCredentials()`. This combination is a security violation and ASP.NET Core will throw an `InvalidOperationException` at startup. You must enumerate the allowed origins explicitly when credentials are involved:

```csharp
options.AddPolicy("AllowFrontEndWithCredentials", policy =>
{
    policy.WithOrigins("http://localhost:3000", "https://myapp.com")
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials();
});
```

### Common Pitfalls

| Pitfall | Cause | Fix |
|---|---|---|
| Browser CORS error on 401 responses | `UseCors` placed after `UseAuthentication` | Move `UseCors` before `UseAuthentication` |
| `AllowAnyOrigin` + `AllowCredentials` throws | Framework rejects the insecure combination | Use `WithOrigins(...)` when credentials are required |
| Preflight returns 404 | `OPTIONS` requests not reaching the CORS middleware | Ensure `UseCors` is before `MapControllers` |
| Policy not applied | Policy name typo, or `UseCors` called without a policy name | Verify string matches exactly |
| Works in dev, fails in production | Dev uses `AllowAll`, production uses a specific policy without the correct origin | Confirm production origin is included in the policy |

---

## Summary

- The same-origin policy is a browser security rule that blocks cross-origin JavaScript requests by default.
- CORS is the protocol servers use to declare which origins may access their resources; it is enforced by browsers, not by servers.
- For non-simple requests (PUT, DELETE, JSON POST), browsers send a preflight `OPTIONS` request before the actual request.
- In ASP.NET Core, CORS is configured with `AddCors` + named policies in `Program.cs` and activated with `UseCors` in the pipeline.
- `UseCors` must be placed after `UseRouting` and before `UseAuthentication` to ensure CORS headers appear on all responses, including 401s.
- Combining `AllowAnyOrigin` with `AllowCredentials` is invalid; use explicit origins when credentials are required.

---

## Additional Resources

- [Enable Cross-Origin Requests (CORS) in ASP.NET Core - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/security/cors)
- [MDN Web Docs: Cross-Origin Resource Sharing (CORS)](https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS)
- [MDN Web Docs: Preflight request](https://developer.mozilla.org/en-US/docs/Glossary/Preflight_request)
