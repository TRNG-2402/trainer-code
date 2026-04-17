# Cookies

## Learning Objectives

- Explain what an HTTP cookie is and how the browser and server exchange it.
- Append cookies to responses and read cookies from requests in ASP.NET Core.
- Configure cookie options: `HttpOnly`, `Secure`, `SameSite`, `Expires`, `Path`, and `Domain`.
- Distinguish cookie-based authentication from token-based (JWT) authentication and describe when each is appropriate.
- Identify GDPR considerations that affect how cookies should be deployed.

---

## Why This Matters

You encountered session cookies in `session-management.md` -- the framework manages those for you. However, applications routinely need to write custom cookies: user preference persistence, consent records, lightweight feature flags, or analytics identifiers -- all of which live in cookies the developer creates and controls directly.

Understanding how cookies work at the protocol level, and how ASP.NET Core surfaces that protocol, lets you make deliberate decisions about security attributes rather than relying on framework defaults that may not match your security requirements.

---

## What Is an HTTP Cookie

An HTTP cookie is a small string of data that the server sends to the browser in a `Set-Cookie` response header. The browser stores the value and automatically includes it in every subsequent request to the same origin via the `Cookie` request header.

### Set-Cookie (server to browser)

```
Set-Cookie: user_preference=dark_mode; Path=/; HttpOnly; Secure; SameSite=Lax; Expires=Fri, 17 Apr 2027 00:00:00 GMT
```

### Cookie (browser to server)

```
Cookie: user_preference=dark_mode; .AspNetCore.Session=abc123xyz
```

Cookies are origin-scoped by default: they are only sent to the domain and path that set them. This scope is configurable with the `Domain` and `Path` attributes.

---

## Writing Cookies in ASP.NET Core

`Response.Cookies` is the API surface for appending cookies to the current response.

### Basic append

```csharp
[HttpPost("preferences")]
public IActionResult SetPreference([FromBody] PreferenceRequest request)
{
    Response.Cookies.Append("user_preference", request.Theme);
    return NoContent();
}
```

### Append with options

```csharp
[HttpPost("preferences")]
public IActionResult SetPreference([FromBody] PreferenceRequest request)
{
    var options = new CookieOptions
    {
        HttpOnly  = true,                                   // Not accessible via JavaScript
        Secure    = true,                                   // HTTPS only
        SameSite  = SameSiteMode.Lax,                      // Controls cross-site behavior
        Expires   = DateTimeOffset.UtcNow.AddDays(365),    // Persistent for 1 year
        Path      = "/",                                    // Sent on all paths
        Domain    = "api.example.com"                      // Scope to this domain only
    };

    Response.Cookies.Append("user_preference", request.Theme, options);
    return NoContent();
}
```

---

## Reading Cookies in ASP.NET Core

`Request.Cookies` provides a dictionary-like interface to all cookies the browser sent.

```csharp
[HttpGet("preferences")]
public IActionResult GetPreference()
{
    // Returns null if the cookie is not present
    string? theme = Request.Cookies["user_preference"];

    if (theme is null)
        return Ok(new { theme = "default" });

    return Ok(new { theme });
}
```

There is no `TryGetValue` overload, so pattern-match on null when the cookie may be absent:

```csharp
if (Request.Cookies.TryGetValue("user_preference", out string? theme))
{
    // cookie was present
}
```

---

## Deleting a Cookie

Cookies can only be deleted by instructing the browser to expire them immediately. You cannot directly remove a cookie from the browser; you overwrite it with an expiry in the past.

```csharp
[HttpDelete("preferences")]
public IActionResult ClearPreference()
{
    Response.Cookies.Delete("user_preference");
    // Equivalent to:
    // Response.Cookies.Append("user_preference", "", new CookieOptions
    // {
    //     Expires = DateTimeOffset.UtcNow.AddDays(-1)
    // });
    return NoContent();
}
```

---

## Cookie Options Reference

| Option | Type | Purpose |
|---|---|---|
| `HttpOnly` | `bool` | When `true`, the cookie is inaccessible to `document.cookie` in JavaScript. Mitigates XSS-based cookie theft. |
| `Secure` | `bool` | When `true`, the browser only sends the cookie over HTTPS. Always set to `true` in production. |
| `SameSite` | `SameSiteMode` | Controls whether the cookie is sent on cross-site requests. See below. |
| `Expires` | `DateTimeOffset?` | Absolute expiry. If not set, the cookie is a session cookie (deleted when the browser closes). |
| `MaxAge` | `TimeSpan?` | Relative expiry. Overrides `Expires` if both are set. |
| `Path` | `string` | URL path scope. Defaults to `/` (all paths). |
| `Domain` | `string?` | Domain scope. Defaults to the exact host. Set to `.example.com` to share across subdomains. |
| `IsEssential` | `bool` | If `true`, cookie is written even when consent has not been granted (see GDPR section). |

### `SameSite` modes

| Mode | Behavior |
|---|---|
| `Strict` | Cookie is never sent on cross-site requests (most secure). |
| `Lax` | Cookie is sent on top-level navigations (e.g., link clicks) but not on cross-site sub-requests (e.g., images, iframes). Browser default. |
| `None` | Cookie is sent on all requests, including cross-site. Requires `Secure = true`. |
| `Unspecified` | No `SameSite` attribute is written; browser falls back to its default (typically `Lax`). |

Use `SameSite = None` only when a cookie must be accessible in a cross-site context, such as a widget embedded in a third-party page.

---

## Session Cookie vs. Persistent Cookie

| Type | `Expires` / `MaxAge` set? | Behavior |
|---|---|---|
| Session cookie | No | Deleted when the browser session ends (tab/window closed or browser quit). |
| Persistent cookie | Yes | Survives browser restarts until the expiry date is reached. |

A "session cookie" in this context refers to browser session persistence -- not to ASP.NET Core's session middleware, which uses its own session cookie that is typically also a browser-session cookie.

---

## Cookie-Based Authentication vs. Token-Based Authentication

Both approaches can authenticate requests, but they have significantly different characteristics.

| Aspect | Cookie-based | Token-based (JWT Bearer) |
|---|---|---|
| Credential transport | Automatic (browser sends cookie on every request to the domain) | Manual (client adds `Authorization: Bearer ...` header) |
| Storage | Browser cookie store | Client-managed (localStorage, memory, secure storage) |
| Cross-origin support | Limited by SameSite/CORS; requires specific configuration | Natural -- the header is explicitly attached per request |
| CSRF exposure | Yes -- the browser sends cookies automatically; requires CSRF tokens | No -- the `Authorization` header is not sent automatically |
| Ideal client | Traditional server-rendered web applications | SPAs, mobile apps, microservice-to-microservice calls |

ASP.NET Core supports cookie authentication via `AddCookie()` in `AddAuthentication`. For the API context of this course, JWT bearer is the preferred choice. Cookie authentication is the appropriate choice for server-rendered MVC applications where the client is always a browser.

---

## GDPR Considerations

The EU General Data Protection Regulation (and similar laws in other jurisdictions) requires user consent before setting non-essential cookies. ASP.NET Core provides `ICookieConsentFeature` and the `CookiePolicy` middleware to manage consent.

### What counts as "essential"

- Authentication cookies (session, login state) -- **essential**.
- CSRF protection cookies -- **essential**.
- Analytics, advertising, personalization cookies -- **non-essential**.

### Registering the cookie policy

```csharp
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded    = context => true;    // Enable consent checking
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});

// In the pipeline
app.UseCookiePolicy();   // Must come before UseAuthentication
app.UseAuthentication();
```

### Marking a cookie as essential

```csharp
Response.Cookies.Append("user_preference", value, new CookieOptions
{
    IsEssential = true   // Written regardless of consent status
});
```

When `CheckConsentNeeded` returns `true` and the user has not consented, any cookie with `IsEssential = false` is silently suppressed by the `CookiePolicy` middleware. The application code sees no error; the cookie simply is not written.

Full GDPR compliance involves a consent UI, a consent record, and a mechanism to withdraw consent -- all of which are application-level concerns beyond the scope of this module.

---

## Summary

- An HTTP cookie is a browser-stored string sent to the server as a `Cookie` header on every matching request.
- Write cookies with `Response.Cookies.Append(name, value, options)` and read them with `Request.Cookies[name]`.
- Delete a cookie by calling `Response.Cookies.Delete(name)`, which sets an expired `Set-Cookie` header.
- Key security attributes: `HttpOnly` (no JavaScript access), `Secure` (HTTPS only), `SameSite` (cross-site send behavior). In production, all three should be set deliberately.
- `Expires` / `MaxAge` control cookie lifetime; omitting both creates a session cookie that expires when the browser closes.
- Cookie-based authentication is best for browser-based server-rendered apps; JWT bearer is best for APIs, SPAs, and mobile clients.
- GDPR requires consent before writing non-essential cookies; use `IsEssential = true` for authentication and security cookies.

---

## Additional Resources

- [Use cookies in ASP.NET Core (Microsoft Docs)](https://learn.microsoft.com/en-us/aspnet/core/security/cookies)
- [Cookie authentication in ASP.NET Core (Microsoft Docs)](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie)
- [MDN Web Docs -- HTTP Cookies](https://developer.mozilla.org/en-US/docs/Web/HTTP/Cookies)
