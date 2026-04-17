# ASP.NET Identity

## Learning Objectives

- Describe what ASP.NET Identity is and the membership concerns it addresses.
- Identify the core types: `IdentityUser`, `IdentityRole`, `UserManager<T>`, `SignInManager<T>`, and `RoleManager<T>`.
- Understand the registration and login flow at the API level.
- Explain how ASP.NET Identity handles password hashing internally.
- Compare ASP.NET Identity with the manual JWT approach built earlier in the week.
- Recognize when to scaffold Identity into an existing project.

---

## Why This Matters

On Wednesday you built a login endpoint that issued JWT tokens manually: you created a user list, compared plaintext passwords, and signed tokens by hand. That approach demonstrated how authentication works mechanically. In production, however, you would never manage passwords, account lockouts, email confirmation, two-factor authentication, and role assignments yourself. ASP.NET Identity is the library Microsoft ships for exactly these concerns. It is opinionated, battle-tested, and deeply integrated with the ASP.NET Core DI and authentication systems.

Understanding ASP.NET Identity means you can onboard a new project's authentication stack quickly, extend it when requirements go beyond the defaults, and make an informed decision about when a simpler custom JWT approach is more appropriate.

---

## The Concept

### What ASP.NET Identity Is

ASP.NET Identity is a membership system that adds user management, password hashing, role management, claims management, external login providers, and two-factor authentication to an ASP.NET Core application. It is distributed as the `Microsoft.AspNetCore.Identity.EntityFrameworkCore` NuGet package and integrates with EF Core for persistence.

At its core, Identity provides:

- A `DbContext` extension that adds Identity-specific tables (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, etc.) to your database schema.
- High-level managers (`UserManager<T>`, `SignInManager<T>`, `RoleManager<T>`) that abstract CRUD operations, password hashing, lockouts, and token generation.
- Integration hooks into the ASP.NET Core authentication middleware so that a signed-in Identity user automatically populates `HttpContext.User`.

### Core Types

**`IdentityUser`**

The base user entity. It maps to the `AspNetUsers` table and exposes properties such as `Id`, `UserName`, `Email`, `EmailConfirmed`, `PasswordHash`, `PhoneNumber`, `TwoFactorEnabled`, `LockoutEnd`, and `AccessFailedCount`.

You typically create an application user class that extends `IdentityUser` to add domain-specific fields:

```csharp
public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

**`IdentityRole`**

The role entity. Maps to the `AspNetRoles` table. You can extend it just as you extend `IdentityUser`.

**`UserManager<TUser>`**

The primary service for user operations. Injected via DI. Key methods:

| Method | Description |
|---|---|
| `CreateAsync(user, password)` | Creates a user with a hashed password. |
| `FindByEmailAsync(email)` | Looks up a user by email. |
| `FindByIdAsync(id)` | Looks up a user by primary key. |
| `CheckPasswordAsync(user, password)` | Validates a password against the stored hash. |
| `AddToRoleAsync(user, roleName)` | Assigns a role to a user. |
| `GetClaimsAsync(user)` | Retrieves the user's custom claims. |
| `GenerateEmailConfirmationTokenAsync(user)` | Generates an email confirmation token. |
| `DeleteAsync(user)` | Deletes a user. |

**`SignInManager<TUser>`**

Handles the sign-in concern: validating credentials, managing the authentication cookie (for browser-based flows), and two-factor sign-in. For Web APIs issuing JWTs, `SignInManager` is less central -- you will typically use `UserManager.CheckPasswordAsync` and then issue a token yourself.

**`RoleManager<TRole>`**

Manages role creation, deletion, and lookup (`CreateAsync`, `FindByNameAsync`, `RoleExistsAsync`).

### Registration and Login Flow for a Web API

ASP.NET Identity does not ship HTTP endpoints; it is a service layer. You wire up your own endpoints (controllers or Minimal APIs) and call Identity's managers.

**Registration:**

```csharp
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterDto dto)
{
    var user = new AppUser
    {
        UserName = dto.Email,
        Email = dto.Email,
        FullName = dto.FullName
    };

    // CreateAsync hashes the password using PBKDF2 and saves the user.
    var result = await _userManager.CreateAsync(user, dto.Password);

    if (!result.Succeeded)
    {
        // result.Errors contains validation failures: weak password, duplicate email, etc.
        return BadRequest(result.Errors);
    }

    return Ok(new { message = "Registration successful." });
}
```

**Login with JWT issuance:**

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginDto dto)
{
    var user = await _userManager.FindByEmailAsync(dto.Email);

    if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        return Unauthorized(new { message = "Invalid credentials." });

    // Retrieve roles and claims from Identity.
    var roles = await _userManager.GetRolesAsync(user);
    var claims = await _userManager.GetClaimsAsync(user);

    // Build JWT claims list (same technique as Wednesday's manual approach).
    var tokenClaims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id),
        new(ClaimTypes.Email, user.Email!)
    };

    tokenClaims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
    tokenClaims.AddRange(claims);

    // Sign and return the token (same helper as Wednesday).
    var token = _jwtService.GenerateToken(tokenClaims);
    return Ok(new { token });
}
```

The key difference from Wednesday's manual approach: `UserManager` retrieves the user and verifies the hashed password. You no longer maintain a plaintext password list or write hashing code.

### Password Hashing

ASP.NET Identity hashes passwords using PBKDF2 with HMAC-SHA256, 128-bit salt, and 10,000+ iterations by default (the exact count increases with each framework version to stay ahead of hardware improvements). You never store or see the plaintext password. The hash is a self-contained string that encodes the algorithm version, iteration count, salt, and derived key.

You do not interact with the hashing mechanism directly. `CreateAsync(user, password)` hashes and stores; `CheckPasswordAsync(user, candidatePassword)` hashes the candidate and compares. If the framework upgrades its hashing algorithm, Identity automatically re-hashes on the next successful login.

### Setting Up Identity in a Project

**1. Install the package:**

```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.*" />
```

**2. Extend `IdentityDbContext` in your `DbContext`:**

```csharp
// AppDbContext.cs
public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    // Other domain DbSets...
}
```

**3. Register Identity services in `Program.cs`:**

```csharp
builder.Services
    .AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
```

**4. Create and apply the migration:**

```shell
dotnet ef migrations add AddIdentity
dotnet ef database update
```

The migration adds the `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, `AspNetUserTokens`, and `AspNetUserLogins` tables.

### Identity vs. Manual JWT: When to Use Each

| Factor | Manual JWT | ASP.NET Identity |
|---|---|---|
| Password management | You implement | Handled by `UserManager` |
| Hashing | You implement | Built-in PBKDF2 |
| Account lockout | You implement | Built-in |
| Email confirmation | You implement | Built-in token generation |
| External providers (Google, GitHub) | Non-trivial | Built-in via `AddGoogle()` etc. |
| Two-factor auth | Non-trivial | Built-in |
| Complexity | Lower for simple cases | Higher setup, lower ongoing cost |
| When to use | Internal services, machine-to-machine, minimal user store | User-facing applications with standard account management |

For a simple microservice with a known set of system accounts, a manual JWT approach with a configuration-backed user store may be perfectly justified. For a user-facing application where end users self-register, reset passwords, and expect standard account security behaviors, ASP.NET Identity is the correct choice.

### Scaffolding Identity into an Existing Project

The `dotnet aspnet-codegenerator` tool can scaffold Identity UI pages and the `DbContext` into an existing project. This is most useful for Razor Pages / MVC projects. For Web API projects, you typically add Identity manually as shown above rather than using the scaffolder, because the scaffolder generates Razor UI that a pure API project does not need.

```shell
# MVC/Razor Pages only -- for reference.
dotnet aspnet-codegenerator identity --dbContext AppDbContext --useDefaultUI
```

For the API projects you have been building this week, the manual setup described above is both simpler and more appropriate.

---

## Summary

- ASP.NET Identity is a complete membership system: user storage, password hashing, role management, claims, lockout, and external login support.
- `IdentityUser` and `IdentityRole` are the base entity types; extend them to add domain fields.
- `UserManager<T>` is the central service: create users, check passwords, manage roles and claims.
- For Web APIs, integrate Identity into your own login/registration endpoints rather than using Identity's built-in UI scaffolding.
- Passwords are hashed with PBKDF2; you never store or compare plaintext.
- Choose Identity over a manual JWT approach when user self-service, account security policies, or external login providers are requirements.

---

## Additional Resources

- [Introduction to Identity on ASP.NET Core - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Use Identity to secure a Web API backend - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization)
- [UserManager<TUser> Class - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1)
