# Learner Walkthrough: JWT Authentication + Role-Based Authorization

> **Format:** Self-paced, follow-along. Every step shows you what you'll see in your editor, what to change, and a quick way to verify it worked before moving on. If a verify step fails, **stop and fix it** — every later step depends on the previous one.
>
> **Estimated time:** 45–60 minutes solo.
>
> **Target project:** `trainer-code/Week-3/WebStorefrontApp/WebStorefront.API/`
> (csproj name `TechSupportSystem.csproj`, namespace root `ProductCatalog.*`)
>
> **Prerequisite reading:** `trainer-code/Week-3/Notes/ASP.NET-Intermediate/authentication.md` and `authorization.md`. The notes explain *what* JWT bearer auth is and *why* claims/policies/schemes exist. This doc is the *how* — implementation only.

---

## What's already in your project

These files were disseminated last week. **Do not recreate them.** Open each and confirm it's there:

| File | State |
|---|---|
| `Models/User.cs` | Complete: `UserId`, `Username`, `PasswordHash`, `Role` |
| `Data/AppDbContext.cs` | `DbSet<User> Users` + fluent config (`HasKey` + unique `HasIndex` on `Username`) + `HasData` seed for `alice`/`admin` |
| `Migrations/20260420152126_InitialCreate.*` | Pre-User schema only — creates `Categories`, `Tags`, `Products`, `ProductTag`. **The `Users` table is NOT in this migration; you'll generate it in Step 0 below.** |
| `DTOs/LoginDTO.cs`, `DTOs/TokenResponseDTO.cs` | Complete |
| `Controllers/AuthController.cs` | Class declared with `[Route]`, `[ApiController]`, `[AllowAnonymous]`, ctor-injected `IAuthService`. **No action methods yet.** |
| `Services/IAuthService.cs` | Interface declared, **empty body** |
| `Services/AuthService.cs` | Class declared, ctor-injected `IUserRepo` + `IConfiguration`. **No methods.** |
| `Data/IUserRepo.cs` | Interface declared, **empty body** |
| `Data/UserRepo.cs` | Class declared, ctor-injected `AppDbContext`. **No methods.** |

> **Sanity check:** Open SQL Server Management Studio (or Azure Data Studio, Or your VSCode MSSQL extension, etc) and run `SELECT * FROM Users`.
>
> **Expected: the query fails with "Invalid object name 'Users'."** That is correct — the `User` entity is in `AppDbContext` but no migration has yet been generated for it. You'll fix that in Step 0.
>
> If `Categories`, `Products`, `Tags`, or `ProductTag` are also missing, run `dotnet ef database update` from `WebStorefront.API/` — those came with previous demos and should already be in your DB. You can find the migration files themselves inside the trainer-code demo files.

---

## Step 0 — Generate the Users migration

The `User` entity is wired into `AppDbContext` (`DbSet<User> Users`, fluent `HasKey` / unique `HasIndex`, `HasData` seed for alice/admin), but the disseminated `InitialCreate` migration was generated *before* the entity was added. The model snapshot still describes a four-table schema. You need to generate a follow-up migration that creates `Users` + applies the seed.

From `WebStorefront.API/`:

```bash
dotnet ef migrations add AddUserEntity
dotnet ef database update
```

> **What this does:** EF Core compares the current `AppDbContext` model against `AppDbContextModelSnapshot.cs`, sees the new `User` entity, and emits a migration that `CreateTable("Users", ...)` plus `InsertData(...)` for the alice/admin seed rows. `database update` applies it to the Azure SQL DB pointed at by `DevConnection`.

> **Verify:** re-run `SELECT * FROM Users` in SSMS / Azure Data Studio. You should now see two rows:
>
> | UserId | Username | PasswordHash | Role |
> |---|---|---|---|
> | 1 | alice | secret | User |
> | 2 | admin | admin | Admin |
>
> If the table still doesn't appear, check the output of `dotnet ef migrations add` for errors — most commonly a stale `obj/` cache (delete `bin/` and `obj/`, re-run) or an unbuildable project (fix the build first).

---

## What you're adding today

| # | Step | File touched |
|---|---|---|
| 0 | Generate `Users` migration + apply | `Migrations/` |
| 1 | Add `Microsoft.AspNetCore.Authentication.JwtBearer` package | `TechSupportSystem.csproj` |
| 2 | Add `Jwt` section to appsettings | `appsettings.Development.json` |
| 3 | Fill `IUserRepo` + `UserRepo` (`GetByUsernameAsync`) | `Data/IUserRepo.cs`, `Data/UserRepo.cs` |
| 4 | Fill `IAuthService` (`LoginAsync` signature) | `Services/IAuthService.cs` |
| 5 | Fill `AuthService` (`LoginAsync` + `BuildToken`) | `Services/AuthService.cs` |
| 6 | Add `Login` action | `Controllers/AuthController.cs` |
| 7 | Extend exception middleware — 401 arm | `Middleware/GlobalExceptionMiddleware.cs` |
| 8 | Register auth scheme + DI + Swagger Bearer | `Program.cs` |
| 9 | Insert `UseAuthentication` before `UseAuthorization` | `Program.cs` |
| 10 | Decorate `CategoryController` | `Controllers/CategoryController.cs` |
| 11 | Decorate `ProductController` | `Controllers/ProductController.cs` |
| 12 | End-to-end Swagger validation | Browser |

### The two questions

| | Question | Failure code |
|---|---|---|
| **Authentication** | Who are you? | **401 Unauthorized** |
| **Authorization** | What are you allowed to do? | **403 Forbidden** |

### The pipeline you're building

```
Request
  → [GlobalExceptionMiddleware]   ← already in (Demo 1)
  → HttpsRedirection
  → Swagger (dev only)
  → [ResponseCaching]              ← already in (Demo 2)
  → [UseAuthentication]            ← NEW today
  → [UseAuthorization]             ← already in but inert; today it actually works
  → MapControllers
```

Authentication and authorization sit **inside** the exception handler's wrapper, so a bad login still comes back as the structured JSON body the middleware emits.

---

## Step 1 — Add the JwtBearer NuGet package

Open `TechSupportSystem.csproj`. You'll see the existing item group:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.5" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.6">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.6" />
  <PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
</ItemGroup>
```

Add this single line inside the `<ItemGroup>`:

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.6" />
```

> **Note:** Version is pinned to `10.0.6` to match the EF Core packages. Don't use a wildcard — keep version drift out of your code when possible!

From a terminal in `WebStorefront.API/`:

```bash
dotnet restore
```

> **Why this package?** Microsoft ships the JWT bearer handler in-box. We do not reach for a third-party library for core auth. `System.IdentityModel.Tokens.Jwt` (used to *build* tokens in Step 5) comes along as a transitive dependency — you do not need to add it explicitly.

> **Verify:** `dotnet build`. The output ends with `Build succeeded` and shows zero errors. If you see a missing-package error, your `dotnet restore` did not pull `JwtBearer` — re-run it.

---

## Step 2 — Add the `Jwt` section to `appsettings.Development.json`

Open `appsettings.Development.json`. Current contents:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DevConnection": "Server=tcp:trng2402.database.windows.net,1433;..."
  }
}
```

Add a `Jwt` section as a sibling of `ConnectionStrings`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DevConnection": "Server=tcp:trng2402.database.windows.net,1433;..."
  },
  "Jwt": {
    "Key": "dev-only-key-min-32-bytes-change-me-in-prod",
    "Issuer": "WebStorefrontApi",
    "Audience": "WebStorefrontClients"
  }
}
```

> **Why these three values?**
> - **`Key`** — the symmetric secret used to sign every token. **Must be ≥ 32 bytes / 256 bits** for HS256. Anything shorter and the framework throws `IDX10653` at startup. The example above is 47 characters — safe.
> - **`Issuer`** — embedded in every token as the `iss` claim. The validator rejects tokens whose `iss` doesn't match.
> - **`Audience`** — embedded as `aud`. Same idea: rejects tokens minted for other audiences.
>
> **This is teaching-only config.** In production: env vars, Azure Key Vault, or `dotnet user-secrets`. Never commit a production signing key to source control.

> **Verify:** `dotnet run`. The app boots, Kestrel logs `Now listening on: https://localhost:...`, and Swagger UI loads at `/swagger`. Stop the app (`Ctrl+C`) before continuing.

---

## Step 3 — Fill in `IUserRepo` + `UserRepo`

### 3a. `Data/IUserRepo.cs` (current)

```csharp
using ProductCatalog.Models;

namespace ProductCatalog.Data;

// We follow the same pattern as IProductRepo / ICategoryRepo:
// the repo talks to EF Core, and nobody above the repo layer
// sees a DbContext. This keeps the service layer free of query code.
public interface IUserRepo
{

}
```

Add one method to the interface body:

```csharp
public interface IUserRepo
{
    Task<User?> GetByUsernameAsync(string username);
}
```

### 3b. `Data/UserRepo.cs` (current)

```csharp
public class UserRepo : IUserRepo
{
    private readonly AppDbContext _context;

    public UserRepo(AppDbContext context)
    {
        _context = context;
    }

}
```

Add the method body inside the class:

```csharp
public async Task<User?> GetByUsernameAsync(string username)
{
    // AsNoTracking: we're only reading, never updating from this query.
    // Skipping change-tracking is a tiny perf win and makes the intent clear.
    return await _context.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.Username == username);
}
```

> **Why `FirstOrDefaultAsync`?** We expect zero-or-one row; zero means "no such user." `SingleOrDefaultAsync` would also be correct (and would throw if the unique-index constraint was somehow violated) — either is fine.
>
> **Why no `throw` on miss?** The repo doesn't decide what "user not found" *means*. The service decides — and it'll throw `UnauthorizedAccessException` in Step 5. **Repos return data; services enforce rules.**

> **Verify:** `dotnet build`. Clean build, zero errors. If you see "the name `AsNoTracking` does not exist," check your `using Microsoft.EntityFrameworkCore;` is present at the top of `UserRepo.cs` — it's already there in the skeleton, but worth double-checking.

---

## Step 4 — Fill in `IAuthService`

`Services/IAuthService.cs` (current):

```csharp
using ProductCatalog.DTOs;

namespace ProductCatalog.Services;

public interface IAuthService
{

}
```

Add one method:

```csharp
public interface IAuthService
{
    Task<TokenResponseDTO> LoginAsync(LoginDTO loginDto);
}
```

> **Verify:** `dotnet build`. Clean build. (You'll get a build error in `AuthService.cs` if it doesn't yet implement the interface — that's expected, you'll fix it in the next step.)

---

## Step 5 — Fill in `AuthService` (`LoginAsync` + `BuildToken`)

This is the meat of the auth wiring. Open `Services/AuthService.cs`. Current state:

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ProductCatalog.Data;
using ProductCatalog.DTOs;
using ProductCatalog.Models;

namespace ProductCatalog.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepo _userRepo;
    private readonly IConfiguration _config;

    public AuthService(IUserRepo userRepo, IConfiguration config)
    {
        _userRepo = userRepo;
        _config = config;
    }

}
```

Add two methods inside the class — one public `LoginAsync` and one private `BuildToken`:

```csharp
public async Task<TokenResponseDTO> LoginAsync(LoginDTO loginDto)
{
    // Validate the DTO minimally - null/empty creds are a 401, not a 500.
    if (string.IsNullOrWhiteSpace(loginDto.Username) ||
        string.IsNullOrWhiteSpace(loginDto.Password))
    {
        throw new UnauthorizedAccessException("Username and password are required.");
    }

    User? user = await _userRepo.GetByUsernameAsync(loginDto.Username);

    // One branch, one message. We DO NOT tell the caller which of the two
    // conditions failed (user not found vs wrong password) - that leaks
    // whether an account exists and enables username enumeration.
    if (user is null || user.PasswordHash != loginDto.Password)
    {
        throw new UnauthorizedAccessException("Invalid username or password.");
    }

    return BuildToken(user);
}

private TokenResponseDTO BuildToken(User user)
{
    // Claims are the "things we know about you" that ride inside the JWT.
    // Anyone who can read the token can read these - DO NOT put secrets here.
    Claim[] claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Name,           user.Username),
        new Claim(ClaimTypes.Role,           user.Role)
    };

    string jwtKey = _config["Jwt:Key"]
        ?? throw new InvalidOperationException("Jwt:Key missing from config.");
    string jwtIssuer = _config["Jwt:Issuer"]!;
    string jwtAudience = _config["Jwt:Audience"]!;

    SymmetricSecurityKey key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwtKey));
    SigningCredentials creds = new SigningCredentials(
        key, SecurityAlgorithms.HmacSha256);

    DateTime expires = DateTime.UtcNow.AddHours(1);

    JwtSecurityToken token = new JwtSecurityToken(
        issuer:             jwtIssuer,
        audience:           jwtAudience,
        claims:             claims,
        expires:            expires,
        signingCredentials: creds
    );

    string serialized = new JwtSecurityTokenHandler().WriteToken(token);

    return new TokenResponseDTO
    {
        Token = serialized,
        ExpiresAt = expires
    };
}
```

### What each piece is doing

- **`UnauthorizedAccessException`** — thrown so the existing `GlobalExceptionMiddleware` can map it to a proper 401 JSON body (you add that arm in Step 7). Controllers and services stay free of HTTP-status code.
- **One failure message for two conditions** — username enumeration is a real attack. We refuse to confirm whether an account exists.
- **Claims are not secrets.** Base64URL is not encryption; anyone with the token can decode the payload at jwt.io. The signature prevents tampering, not snooping.
- **1-hour expiry** — industry default for an *access* token. Real systems issue a short-lived access token plus a long-lived refresh token; we don't model refresh here.
- **`?? throw` on `Jwt:Key`** — die loud at login if the key is missing. `appsettings` has it (Step 2), so this is a safety net.
- **`HmacSha256`** — symmetric signing. Same key validates and signs. Asymmetric (RS256) is also supported but adds a public/private key pair we don't need.

> **Verify:** `dotnet build`. Clean build. If you get "the name `JwtSecurityToken` does not exist," your `using System.IdentityModel.Tokens.Jwt;` is missing — re-check the using block at the top of the file. (It's already present in the skeleton.)

---

## Step 6 — Add the `Login` action to `AuthController`

Open `Controllers/AuthController.cs`. Current state:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.DTOs;
using ProductCatalog.Services;

namespace ProductCatalog.Controllers;

// [AllowAnonymous] is critical here. Once we add [Authorize] to the other
// controllers below, if we ever decided to apply a global authorization filter
// we'd lock ourselves out of the one endpoint needed to get a token.
// Explicit > implicit.
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

}
```

Add the `Login` action inside the class:

```csharp
// POST /api/Auth/login
// No try/catch - UnauthorizedAccessException bubbles to GlobalExceptionMiddleware
// and becomes a proper 401 with the structured JSON body.
[HttpPost("login")]
public async Task<ActionResult<TokenResponseDTO>> Login(LoginDTO loginDto)
{
    return await _authService.LoginAsync(loginDto);
}
```

> **Why no try/catch?** The architecture pays rent here. The service throws a typed exception; the middleware maps it. The controller stays as thin as `CategoryController.GetCategories` — service call in, `ActionResult` out.

> **Verify:** `dotnet build`. Clean build. (End-to-end testing comes after Step 9 once the auth handler is registered.)

---

## Step 7 — Extend `GlobalExceptionMiddleware` (the 401 arm)

Open `Middleware/GlobalExceptionMiddleware.cs`. Find the switch statement inside `HandleExceptionAsync` (around line 58):

```csharp
switch (ex)
{
    case KeyNotFoundException _:
        statusCode = 404;
        break;
    case ArgumentOutOfRangeException _:
        statusCode = 400;
        break;
    case ArgumentException _:
        statusCode = 400;
        break;
    case NullReferenceException _:
        statusCode = 404;
        break;
    default:
        statusCode = 500;
        break;
}
```

Insert one new case above `default`:

```csharp
case UnauthorizedAccessException _:
    statusCode = 401;
    break;
```

After the edit:

```csharp
switch (ex)
{
    case KeyNotFoundException _:
        statusCode = 404;
        break;
    case ArgumentOutOfRangeException _:
        statusCode = 400;
        break;
    case ArgumentException _:
        statusCode = 400;
        break;
    case NullReferenceException _:
        statusCode = 404;
        break;
    case UnauthorizedAccessException _:
        statusCode = 401;
        break;
    default:
        statusCode = 500;
        break;
}
```

> **The payoff:** every new failure mode the system ever has is one new `case` in this switch. The auth feature didn't force you to touch error-handling code anywhere else. That's the global exception handler doing its job.

> **Verify:** `dotnet build`. Clean.

---

## Step 8 — Wire up `Program.cs`

This step has four sub-edits in `Program.cs`. Take them one at a time.

### 8a. Add `using` statements at the very top

Current top of `Program.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Data;
using ProductCatalog.Middleware;
using ProductCatalog.Services;
```

Add four more:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
```

### 8b. Replace `AddSwaggerGen()` with the Bearer-aware version

Current line 14:

```csharp
builder.Services.AddSwaggerGen(); // Adding Swagger
```

Replace with:

```csharp
// Swagger, now with a Bearer security definition so the UI shows an
// "Authorize" padlock. Without this, you have to paste tokens via curl.
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Paste the JWT from /api/Auth/login. No 'Bearer ' prefix needed."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
```

### 8c. Register the JWT bearer authentication scheme

Insert this block **after** `AddMemoryCache()` (around line 21) and **before** `AddDbContext` (line 26):

```csharp
// Authentication: register the JWT bearer scheme and tell it how to validate tokens.
string jwtKey      = builder.Configuration["Jwt:Key"]!;
string jwtIssuer   = builder.Configuration["Jwt:Issuer"]!;
string jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(options =>
{
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

        ValidateLifetime         = true,         // enforce the exp claim
        ClockSkew                = TimeSpan.Zero // strict - no 5-min grace
    };
});

builder.Services.AddAuthorization();
```

### 8d. Register the new repo + service in DI

Find the existing Category and Product registrations (around lines 33–38):

```csharp
// Category stuff
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepo, CategoryRepo>();

// Product stuff
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepo, ProductRepo>();
```

Add two more lines below them:

```csharp
// Auth layer
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IAuthService, AuthService>();
```

### Why every validation parameter is on

| Parameter | Why on |
|---|---|
| `ValidateIssuer` | Reject tokens minted by another system that happens to use the same key. |
| `ValidateAudience` | Reject tokens issued for a different audience. |
| `ValidateIssuerSigningKey` | The whole point of the signature — verify it. |
| `ValidateLifetime` | Honor the `exp` claim. Without this, an expired token is still accepted. |
| `ClockSkew = TimeSpan.Zero` | Strict expiry. Default is 5 minutes — a token that expired 4 min ago is still valid by default. Real systems often leave the default to tolerate clock drift between API servers. We use zero so behavior matches your intuition during testing. |

`DefaultAuthenticateScheme` and `DefaultChallengeScheme` tell `[Authorize]` *which* scheme to run. Even a single-scheme app has to name its default.

> **Verify:** `dotnet build`. Clean. (Don't `dotnet run` yet — you still need Step 9 to put authentication into the request pipeline.)

---

## Step 9 — Insert `UseAuthentication()` into the pipeline

Scroll to the pipeline section near the bottom of `Program.cs` (around lines 66–72). Current state:

```csharp
app.UseResponseCaching();


app.UseAuthorization();

app.MapControllers();
```

Insert `app.UseAuthentication();` **immediately before** `app.UseAuthorization()`:

```csharp
app.UseResponseCaching();

app.UseAuthentication();   // NEW - must run BEFORE UseAuthorization
app.UseAuthorization();

app.MapControllers();
```

### The ordering rule

```
Request
  → [GlobalExceptionMiddleware]   ← outermost
  → HttpsRedirection
  → Swagger (dev)
  → [ResponseCaching]
  → [UseAuthentication]           ← populates HttpContext.User
  → [UseAuthorization]            ← reads HttpContext.User
  → MapControllers
```

> **Why authentication first?** Authorization runs *against* `HttpContext.User`. Authentication is what *populates* it. Swap the two lines and every request looks anonymous — every `[Authorize]` endpoint returns 401 even for valid tokens. The framework won't stop you from putting them in the wrong order; the behavior is just silently wrong.

> **Note on response caching:** the `ResponseCachingMiddleware` refuses to cache any response whose request bore an `Authorization` header. So once you attach a token, the cache misses by design. Not a bug.

> **Verify:** `dotnet run`. The app starts without throwing `IDX10653` (key length error from Step 2) or scheme registration errors. Open `https://localhost:<port>/swagger`. **You should now see a green "Authorize" padlock at the top right of the Swagger UI.** That padlock is proof that Step 8b worked. Stop the app before continuing.

---

## Step 10 — Decorate `CategoryController`

Open `Controllers/CategoryController.cs`. **Critical:** all the existing teaching comments — including the commented-out `// try` / `// catch` blocks and the `// COMING SOON!` note — must stay intact. You're only adding `[Authorize]` attributes.

Add one `using` at the top:

```csharp
using Microsoft.AspNetCore.Authorization;
```

Add `[Authorize]` to the class declaration:

```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize]   // NEW - every action requires a valid token by default
public class CategoryController : ControllerBase
```

Add `[Authorize(Roles = "Admin")]` to the `DeleteCategory` action:

```csharp
[HttpDelete("{categoryId}")]
[Authorize(Roles = "Admin")]   // NEW - tightens the class-level [Authorize]
public async Task<ActionResult> DeleteCategory(int categoryId)
{
    // Just to show off Global Exception Handling in ASP.NET - COMING SOON!
    // We won't even use a try-catch.
    // try{
        await _categoryService.DeleteCategoryAsync(categoryId);
    // }
    // catch (Exception e)
    // {
    //     return BadRequest(e.Message);
    // }

    return NoContent(); // Returns a 204 No Content - things went smooth, but no data return
}
```

> **Class-level vs action-level `[Authorize]`:** the class-level attribute is "default closed" — every action requires a token. Stacking `[Authorize(Roles = "Admin")]` on `DeleteCategory` is **additive**: both must pass — authenticated AND in the Admin role.

> **Verify:** `dotnet build`. Clean.

---

## Step 11 — Decorate `ProductController`

Same pattern. Add `using Microsoft.AspNetCore.Authorization;` at the top. Add class-level `[Authorize]`. Then *override* the class-level attribute on `GetProducts` so anonymous users can browse the catalog.

```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize]                    // NEW - default closed
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [AllowAnonymous]           // NEW - punch a hole for public browsing
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        // try
        // {
            return await _productService.GetAllProductsAsync();
        // }
        // catch (Exception e)
        // {
        //     return BadRequest(e.Message);
        // }
    }

    [HttpPatch]
    public async Task<ActionResult> AddTagToProduct(TagProductDTO updateInfo)
    {
        // try
        // {
            await _productService.AddTagToProductAsync(updateInfo);
        // } catch (Exception e)
        // {
        //     return BadRequest(e.Message);
        // }

        return NoContent();
    }
}
```

> **`[AllowAnonymous]` wins.** When both `[Authorize]` (class) and `[AllowAnonymous]` (action) are present, `[AllowAnonymous]` always takes precedence. There's no "stronger" `[Authorize]` that overrides it — that's by design.

> **Same comment rule as Step 10.** Don't strip the commented-out try/catch blocks.

> **Verify:** `dotnet build`. Clean.

---

## Step 12 — End-to-end Swagger validation

Run the app:

```bash
dotnet run
```

Open `https://localhost:<port>/swagger`.

Walk through these nine tests in order. Each test names the expected status code and (where relevant) the expected response. **If any test fails, stop and re-read the corresponding step before continuing.**

### Test 1 — Public endpoint still works (anonymous)

Request:

```http
GET /api/Product
```

Expected: **200 OK** with the seeded `Thinkpad Charger`. `[AllowAnonymous]` wins.

### Test 2 — Protected endpoint without a token

Request:

```http
GET /api/Category
```

Expected: **401 Unauthorized**. The response includes a `WWW-Authenticate: Bearer` header. The body is empty — the challenge is the message.

### Test 3 — Login as alice

Request:

```http
POST /api/Auth/login
Content-Type: application/json

{ "username": "alice", "password": "secret" }
```

Expected: **200 OK** with a body shaped like:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ...",
  "expiresAt": "2026-04-27T22:34:12Z"
}
```

**Copy the token value.**

### Test 4 — Authorize in Swagger

Click the green **Authorize** padlock at the top right. Paste the token (no `Bearer ` prefix — the security definition adds it). Click **Authorize**. The padlock turns closed.

> **Optional side quest:** paste the token at `jwt.io`. You'll see the decoded payload — `nameid`, `unique_name`, `role`, `iss`, `aud`, `exp`. None of it is encrypted. The signature is what prevents tampering, not secrecy.

### Test 5 — Protected GET, with a token

Request:

```http
GET /api/Category
```

Expected: **200 OK** with the three seeded categories.

### Test 6 — Role-protected endpoint as alice (role = User)

Request:

```http
DELETE /api/Category/1
```

Expected: **403 Forbidden**. Alice is authenticated, but her `Role` claim is `User`, not `Admin`.

> **Pause and contrast Test 2 vs Test 6.** 401 means *"I don't know who you are."* 403 means *"I know who you are, and you can't do this."* That one-character status-code difference carries the entire authentication-vs-authorization distinction.

### Test 7 — Re-login as admin, retry the delete

Click **Authorize → Logout** in Swagger. Re-login:

```http
POST /api/Auth/login
{ "username": "admin", "password": "admin" }
```

Paste the new token into Authorize.

```http
DELETE /api/Category/1
```

Expected: **204 No Content**. (Or, if you already deleted id 1 in a previous run, **404** with the structured JSON body from `GlobalExceptionMiddleware` — that's also a win, because it proves the middleware still formats non-auth errors correctly.)

### Test 8 — Bad credentials

Request:

```http
POST /api/Auth/login
{ "username": "alice", "password": "wrong" }
```

Expected:

```json
{
  "status": 401,
  "message": "Invalid username or password."
}
```

That JSON came out of `GlobalExceptionMiddleware`'s new arm (Step 7). Neither the controller nor the service formatted this response — the middleware did. That is the architecture paying rent.

### Test 9 — Stale token (optional, demonstrates expiry)

Either wait an hour, **or** quick-test it: open `appsettings.Development.json`, change one character in the `Jwt:Key` value, save, restart the app. Any token issued before the restart now has an invalid signature. Retry:

```http
GET /api/Category
```

Expected: **401** with `WWW-Authenticate: Bearer error="invalid_token"` in the response headers.

(Don't forget to revert the key change.)

---

## Step 13 — Common pitfalls (read if anything broke)

| If you see... | Check... |
|---|---|
| **Every endpoint returns 401, even with a valid token** | `app.UseAuthentication()` is *before* `app.UseAuthorization()` (Step 9). Easy to swap by accident. |
| **`IDX10653` at startup** | Your `Jwt:Key` in `appsettings.Development.json` is shorter than 32 bytes. The error message confusingly mentions "128 bits" — the practical floor is 32 bytes / 256 bits for HS256. |
| **Login itself returns 401** | `AuthController` is missing `[AllowAnonymous]`. (Already on the skeleton, but worth checking.) |
| **Swagger Authorize doesn't show a padlock** | Step 8b wasn't applied — the security definition isn't registered. |
| **Swagger sends the token but you still get 401** | You may have pasted `Bearer eyJ...` instead of just the raw `eyJ...`. The Swagger security definition adds the `Bearer ` prefix itself. (Curl users still write `-H "Authorization: Bearer eyJ..."`.) |
| **Tokens whose `exp` passed are still accepted** | `ClockSkew` is at the default 5-minute grace. We set it to `TimeSpan.Zero` in Step 8c — confirm. |
| **Login returns 401 even with the correct password** | Plaintext string compare is case- and whitespace-sensitive. `Alice` ≠ `alice`; `secret ` (trailing space) ≠ `secret`. |
| **Login returns 401 and you think creds are right** | Did the `Users` table actually get created? Run `SELECT * FROM Users`. If "Invalid object name 'Users'", you skipped **Step 0** — run `dotnet ef migrations add AddUserEntity` then `dotnet ef database update`. The 401 lies — it'll look like "wrong credentials" but it's really "no table." |

---

## Step 14 — What we did NOT cover (intentional)

For completeness, here's what's missing from this demo and where to learn it:

1. **Password hashing.** The single most important real-world next step. Replace plaintext compare with `PasswordHasher<User>` (from `Microsoft.AspNetCore.Identity`, doesn't require full Identity) or BCrypt.Net-Next. Non-negotiable for anything real.
2. **Refresh tokens.** Real systems issue a long-lived refresh token alongside the short-lived access token. The client trades the refresh for new access tokens without re-prompting for credentials.
3. **Policy-based authorization.** `[Authorize(Roles = "Admin")]` is the coarse tool. `[Authorize(Policy = "CanDeleteCategories")]` composes role + claim + custom handler into a named, testable unit. See `Notes/ASP.NET-Intermediate/authorization.md`.
4. **ASP.NET Core Identity.** `AddIdentity<User, Role>()` plus `IdentityDbContext` plus `UserManager`/`SignInManager` gives you registration, password reset, lockout, 2FA, external logins. Big surface area. See `Notes/ASP.NET-Advanced/aspnet-identity.md`.
5. **Resource-based authorization.** "Users can only edit orders they own" — can't be expressed as an attribute, needs `IAuthorizationService` inside the action. See `authorization.md`.
6. **CORS** — becomes relevant when the Week-4 React SPA calls this API cross-origin with a token. See `Notes/ASP.NET-Advanced/cors.md`.

---

## Self-check (answer without looking at the code)

1. *Why* must `app.UseAuthentication()` sit before `app.UseAuthorization()`, and why are both still *inside* `UseMiddleware<GlobalExceptionMiddleware>()` from the first demo?
2. *What* is the difference between the 401 in Test 2 and the 403 in Test 6? Which middleware/attribute returned each?
3. *Why* is storing `PasswordHash` as plaintext here a demo-only shortcut, and what's the smallest change that would fix it?

If you can answer all three, you understand the mental model. If not, re-run Tests 2, 3, and 6 back-to-back and let the status codes do the teaching.

---

## End-state checklist

If all of these are true, you've finished:

- [ ] `dotnet build` is clean.
- [ ] `dotnet run` boots without `IDX10653` or scheme errors.
- [ ] Swagger UI shows a green Authorize padlock.
- [ ] `GET /api/Product` returns 200 anonymously.
- [ ] `GET /api/Category` returns 401 anonymously.
- [ ] `POST /api/Auth/login` with `alice/secret` returns a token.
- [ ] After authorizing in Swagger, `GET /api/Category` returns 200.
- [ ] `DELETE /api/Category/1` as alice returns 403.
- [ ] After re-logging in as admin, `DELETE /api/Category/1` returns 204 (or 404 with the middleware's JSON body).
- [ ] `POST /api/Auth/login` with bad credentials returns the structured 401 JSON body.
