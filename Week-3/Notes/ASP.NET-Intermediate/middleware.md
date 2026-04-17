# Middleware

## Learning Objectives

- Define middleware and explain its role in the ASP.NET Core request pipeline.
- Use `Use`, `Map`, and `Run` to compose a pipeline.
- Explain why middleware ordering is significant.
- Identify key built-in middleware components and when to use them.
- Write and register a custom middleware class.
- Explain short-circuiting and when it is appropriate.

---

## Why This Matters

Every HTTP request that arrives at your ASP.NET Core application passes through a chain of components before it ever reaches a controller action. That chain is the middleware pipeline. Logging, authentication, CORS enforcement, response compression, and exception handling are all implemented as middleware -- not as framework magic buried inside the runtime, but as ordinary components you can inspect, reorder, and replace.

Understanding the pipeline is the key to answering the questions that come up in every non-trivial API project: "Why does my CORS policy not seem to apply?" or "Why is my exception handler not catching errors from my authentication layer?" Almost always, the answer is middleware ordering.

This week's Epic centers on taking a controller-based API from functional to production-ready. Cross-cutting concerns -- timing, logging, security, caching -- are all handled in the pipeline, making middleware the first topic on that journey.

---

## The Concept

### What Middleware Is

Middleware is a component that sits in the HTTP request/response pipeline. Each component:

1. Receives the `HttpContext` for the current request.
2. Optionally performs work before the next component runs (pre-processing).
3. Calls the next component in the pipeline (or short-circuits and responds directly).
4. Optionally performs work after the next component returns (post-processing).

This is the classic "chain of responsibility" pattern. The pipeline is built up in `Program.cs` during application startup; the order in which you add components is the order in which they execute on every request.

### `Use`, `Map`, and `Run`

ASP.NET Core provides three primary extension methods on `IApplicationBuilder` (accessible via `WebApplication`) for composing the pipeline:

#### `Use` -- General-purpose middleware

Calls the next delegate in the pipeline. This is the standard extension point.

```csharp
app.Use(async (context, next) =>
{
    // Pre-processing: runs before the rest of the pipeline
    Console.WriteLine($"Incoming: {context.Request.Method} {context.Request.Path}");

    await next(context);  // Hand off to the next component

    // Post-processing: runs after the rest of the pipeline has responded
    Console.WriteLine($"Outgoing: {context.Response.StatusCode}");
});
```

#### `Run` -- Terminal middleware

Terminates the pipeline. No `next` delegate is called. Use this for a component that always produces a response.

```csharp
app.Run(async context =>
{
    await context.Response.WriteAsync("Request handled by terminal middleware.");
});
```

Placing a `Run` delegate before other middleware in the pipeline will prevent those components from ever executing, which is a common source of confusion.

#### `Map` -- Branch the pipeline

Executes a separate branch of the pipeline when the request path starts with a given prefix. Useful for splitting handling of distinct sub-paths.

```csharp
app.Map("/health", healthApp =>
{
    healthApp.Run(async context =>
    {
        await context.Response.WriteAsync("Healthy");
    });
});
```

Requests to `/health` follow the branch; all other requests continue down the main pipeline.

### Ordering Matters

The pipeline executes in the order middleware is registered. This is not a detail -- it is the central rule of middleware composition.

A concrete example: if you call `UseAuthentication()` after `UseRouting()` but before `UseAuthorization()`, the framework knows who the user is before it checks whether they are allowed to access the matched endpoint. Reversing the order of `UseAuthentication` and `UseAuthorization` will not cause a compilation error -- it will cause silent authorization failures that are difficult to debug.

The recommended ordering for a typical Web API (as prescribed by the ASP.NET Core team) is:

```
ExceptionHandler / HSTS
Routing (UseRouting)
CORS (UseCors)
Authentication (UseAuthentication)
Authorization (UseAuthorization)
Custom middleware
Endpoint dispatch (UseEndpoints / MapControllers)
```

### Built-in Middleware

ASP.NET Core ships with a rich set of ready-to-use middleware components:

| Method | Purpose |
|---|---|
| `UseExceptionHandler` | Catches unhandled exceptions and produces an error response. |
| `UseHsts` | Adds the HTTP Strict Transport Security header in production. |
| `UseHttpsRedirection` | Redirects HTTP requests to HTTPS. |
| `UseStaticFiles` | Serves files from `wwwroot` without hitting the controller pipeline. |
| `UseRouting` | Matches incoming requests against defined endpoint routes. |
| `UseCors` | Applies CORS policies before the request reaches controllers. |
| `UseAuthentication` | Resolves the current user's identity from the request credentials. |
| `UseAuthorization` | Enforces access policies for the matched endpoint. |
| `UseResponseCaching` | Adds response caching behavior. |
| `UseResponseCompression` | Compresses response bodies (gzip/Brotli). |

### Writing Custom Middleware

Inline lambda middleware (using `app.Use`) is fine for quick experiments, but for reusable, testable middleware, the convention is a dedicated class.

A custom middleware class must:
1. Accept a `RequestDelegate next` in its constructor.
2. Expose a `InvokeAsync(HttpContext context)` method.

```csharp
public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestTimingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();
        Console.WriteLine(
            $"{context.Request.Method} {context.Request.Path} completed in {stopwatch.ElapsedMilliseconds} ms " +
            $"with status {context.Response.StatusCode}");
    }
}
```

Register it with the `UseMiddleware<T>` extension:

```csharp
app.UseMiddleware<RequestTimingMiddleware>();
```

You will build exactly this middleware in today's exercise.

### Short-Circuiting the Pipeline

A middleware component short-circuits the pipeline when it responds to the request directly -- without calling `next`. This is appropriate when:

- A request fails a precondition (e.g., a required API key is missing).
- A cached response is available and fresh.
- The path matches a health check endpoint that does not need the full controller pipeline.

```csharp
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("X-Api-Key"))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("API key required.");
        return; // Short-circuit: do NOT call next
    }

    await next(context);
});
```

Be deliberate about short-circuiting. A middleware that short-circuits too eagerly can prevent authentication, logging, or exception handling from running.

---

## Code Example

A complete `Program.cs` demonstrating a typical middleware pipeline for a Web API, with the custom `RequestTimingMiddleware` from above:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var app = builder.Build();

// 1. Exception handling -- must be first so it can catch errors from all downstream components
app.UseExceptionHandler("/error");

// 2. HTTPS redirection
app.UseHttpsRedirection();

// 3. Custom request timing -- before routing so it times the full dispatch
app.UseMiddleware<RequestTimingMiddleware>();

// 4. Routing -- matches the request to an endpoint
app.UseRouting();

// 5. Authentication -- establishes identity
app.UseAuthentication();

// 6. Authorization -- enforces access policies
app.UseAuthorization();

// 7. Dispatch to controllers
app.MapControllers();

app.Run();
```

If you swap steps 5 and 6, `[Authorize]` attributes will stop working as expected -- the authorization middleware will run before the framework knows who the user is.

---

## Summary

- Middleware components form a sequential pipeline through which every HTTP request and response flows.
- `Use` adds general-purpose middleware; `Run` adds terminal middleware; `Map` branches the pipeline by path.
- Ordering is deterministic and consequential -- always follow the recommended sequence.
- Write reusable middleware as a class with a `RequestDelegate` constructor parameter and an `InvokeAsync` method.
- Short-circuiting stops the pipeline at a given component and is appropriate for fast-fail scenarios, but must be placed deliberately to avoid bypassing required components.

---

## Additional Resources

- [Microsoft Docs -- ASP.NET Core Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
- [Microsoft Docs -- Write custom ASP.NET Core middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write)
- [Microsoft Docs -- ASP.NET Core middleware order](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-8.0#middleware-order)
