# ASP.NET Core Overview

## Learning Objectives

- Describe what ASP.NET Core is and where it fits in the broader .NET ecosystem.
- Explain the cross-platform story and hosting model behind ASP.NET Core.
- Contrast ASP.NET Core with the older ASP.NET Framework.
- Trace a request through the ASP.NET Core pipeline at a conceptual level.

---

## Why This Matters

Last week's Epic was **"From Zero to Data Layer."** You built out EF Core entities, relationships, migrations, and CRUD operations — everything below the HTTP surface. This week's Epic is **"From Framework to Production-Ready API."** ASP.NET Core is the framework that sits on top of that data layer and exposes it to the world.

Before writing a single controller, you need a mental map of the host that runs your application, the server that handles TCP connections, and the pipeline that processes every incoming request. Engineers who skip this foundation frequently misdiagnose pipeline issues, misconfigure middleware, or wonder why something that "should work" does not show up.

---

## The Concept

### What ASP.NET Core Is

**ASP.NET Core** is Microsoft's open-source, cross-platform web framework for building web APIs, web applications, and real-time services on .NET. It is not a new version of the classic ASP.NET Framework — it is a ground-up redesign with different goals:

| Goal | What it means in practice |
|---|---|
| Cross-platform | Runs on Windows, Linux, and macOS via .NET (formerly .NET Core). |
| High performance | Kestrel, the built-in HTTP server, consistently ranks among the fastest web servers in TechEmpower benchmarks. |
| Modular | The framework is composed of NuGet packages. You include only what you need. |
| Cloud-native | Designed with containerization (Docker) and cloud deployment (Azure, AWS) as first-class concerns. |
| Open source | Developed publicly on GitHub at `dotnet/aspnetcore`. |

### ASP.NET Core vs. ASP.NET Framework

| Dimension | ASP.NET Framework (classic) | ASP.NET Core |
|---|---|---|
| Platform | Windows only (IIS) | Windows, Linux, macOS |
| Runtime | .NET Framework (4.x) | .NET 6, 7, 8+ (unified) |
| Deployment | IIS required | Self-hosted, IIS, Nginx, Docker |
| Performance | Good | Significantly higher |
| Modularity | Monolithic System.Web assembly | Composable NuGet packages |
| Web Forms | Yes | No — explicitly excluded |
| Active development | Maintenance mode | Active, all new features here |

In enterprise settings you will encounter both codebases. New projects should use ASP.NET Core. Legacy systems may still run on ASP.NET Framework, and the migration path is well-documented.

### The Hosting Model

When you run an ASP.NET Core application, three layers are always present:

```
Your Application Code
        |
   ASP.NET Core Pipeline  (middleware, routing, controllers)
        |
     Kestrel  (cross-platform HTTP server)
        |
  [Optional: Reverse Proxy — IIS, Nginx, Apache]
        |
     Network / Internet
```

**Kestrel** is the default, built-in HTTP server. It handles raw TCP, TLS termination, and HTTP/1.1 and HTTP/2 protocol parsing. In production, a reverse proxy like Nginx or IIS is placed in front of Kestrel to handle SSL offloading, load balancing, and static file serving.

**The Host** (`WebApplication` or `IHost`) is the object that owns the application's lifetime, configuration, dependency injection container, and the server. You create and configure the host in `Program.cs`.

### The Request Pipeline (Conceptual)

Every HTTP request travels through a sequence of **middleware** components. Each component can:

1. Do work before passing the request to the next component.
2. Pass the request down the chain (`await next(context)`).
3. Do work on the way back out (after the next component has responded).
4. Short-circuit — respond immediately without calling the next component.

```
Request
  |
  v
[Middleware: ExceptionHandler]
  |
  v
[Middleware: HTTPS Redirection]
  |
  v
[Middleware: Static Files]
  |
  v
[Middleware: Routing]
  |
  v
[Middleware: Authentication]
  |
  v
[Middleware: Authorization]
  |
  v
[Middleware: Controller Endpoint]
  |
  v
Response (travels back up the chain)
```

The order in which middleware is registered in `Program.cs` is the order in which it executes. This is one of the most common sources of bugs in ASP.NET Core applications — you will cover middleware in depth on Tuesday.

---

## Code Example

The following is a minimal `Program.cs` for a Web API project. Every line is deliberate.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register services into the DI container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

Notice the two-phase pattern:

- **`builder` phase** — configure services (dependency injection) before the app starts.
- **`app` phase** — configure the pipeline (middleware) and start listening.

This separation is intentional and enforced: you cannot add services after `builder.Build()` is called, and you should not add middleware before it.

---

## Summary

- ASP.NET Core is a cross-platform, high-performance, open-source web framework built on top of .NET.
- It replaces the classic ASP.NET Framework for all new development and runs on Windows, Linux, and macOS.
- Kestrel is the built-in HTTP server; a reverse proxy is added in front of it for production deployments.
- Every request passes through a configurable middleware pipeline, registered in `Program.cs` in a specific order.
- `Program.cs` follows a deliberate two-phase pattern: register services first, then configure the pipeline.

---

## Additional Resources

- [Microsoft Docs — ASP.NET Core Overview](https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core)
- [Microsoft Docs — ASP.NET Core Fundamentals](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/)
- [TechEmpower Framework Benchmarks](https://www.techempower.com/benchmarks/) — see where ASP.NET Core ranks relative to other frameworks.
