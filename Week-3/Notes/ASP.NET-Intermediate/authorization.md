# Authorization

## Learning Objectives

- Articulate the role of authorization in the security pipeline and how it differs from authentication.
- Apply role-based authorization using `[Authorize(Roles = "...")]`.
- Define and enforce policy-based authorization using `AddPolicy`, `RequireClaim`, and `RequireRole`.
- Implement a custom `IAuthorizationHandler` for requirements that exceed built-in checks.
- Understand the concept of resource-based authorization and when it applies.

---

## Why This Matters

Authentication, covered in `authentication.md`, answers "Who are you?" Authorization answers the follow-up question: "What are you allowed to do?" In any real system, not all authenticated users have the same permissions. A customer should be able to view their own orders; an administrator should be able to delete any record. Without authorization, a valid JWT token grants unrestricted access to every protected endpoint -- which is not access control, it is just a login gate.

ASP.NET Core ships with a flexible authorization framework that scales from simple role checks to arbitrarily complex, context-aware business rules, all expressed in consistent, composable patterns.

---

## Where Authorization Sits

Authorization middleware is registered after authentication in the pipeline. This is mandatory -- the framework must know who the user is before it can decide what they can do.

```csharp
app.UseAuthentication();   // Must come first
app.UseAuthorization();    // Must come second
```

When an authenticated user is denied access, the framework returns **403 Forbidden**. When an unauthenticated caller hits a protected endpoint, the authentication handler returns **401 Unauthorized** -- the authorization layer is never reached.

---

## Role-Based Authorization

Role-based authorization is the simplest mechanism. It checks whether one of the authenticated user's role claims matches a specified role name.

### Asserting roles in the JWT

When you issue a token, include a `ClaimTypes.Role` claim (or the JWT-standard `role` short name):

```csharp
var claims = new[]
{
    new Claim(ClaimTypes.Name,           "alice"),
    new Claim(ClaimTypes.Role,           "Admin"),
    new Claim(ClaimTypes.Role,           "Inventory")   // A user can have multiple roles
};
```

### Applying the role constraint

```csharp
[ApiController]
[Route("products")]
[Authorize]                                    // All actions require authentication
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() { /* ... */ }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]               // Only Admins can delete
    public IActionResult Delete(int id) { /* ... */ }

    [HttpPost]
    [Authorize(Roles = "Admin,Inventory")]     // Either role is accepted
    public IActionResult Create([FromBody] CreateProductDto dto) { /* ... */ }
}
```

`Roles` accepts a comma-separated list; any matching role grants access (logical OR). To require all roles simultaneously, stack multiple `[Authorize]` attributes -- each attribute is evaluated independently and all must pass.

---

## Policy-Based Authorization

Role-based authorization is convenient but limited: it can only check a claim value against a hard-coded string. Policy-based authorization is the recommended approach because:

- Policies decouple authorization logic from the attribute declaration.
- Policies can combine multiple requirements (role, claim, custom logic) in a single named unit.
- Policies make it easier to centralize and test access control rules.

### Defining policies in `Program.cs`

```csharp
builder.Services.AddAuthorization(options =>
{
    // Simple role requirement expressed as a policy
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // Require a specific claim value
    options.AddPolicy("PremiumUser", policy =>
        policy.RequireClaim("subscription", "premium"));

    // Combine requirements: authenticated + specific role + specific claim
    options.AddPolicy("SeniorAdmin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin");
        policy.RequireClaim("experience_level", "senior");
    });
});
```

All requirements within a single policy use logical AND -- the user must satisfy every requirement listed.

### Applying a policy

```csharp
[HttpDelete("{id:int}")]
[Authorize(Policy = "AdminOnly")]
public IActionResult Delete(int id) { /* ... */ }
```

### Common built-in requirement methods

| Method | What it checks |
|---|---|
| `RequireAuthenticatedUser()` | User is authenticated (implicit when using `[Authorize]`) |
| `RequireRole("Admin")` | User has the `Admin` role claim |
| `RequireClaim("sub")` | User has a claim of that type (any value) |
| `RequireClaim("tier", "gold")` | User has a claim with that exact value |
| `RequireUserName("system")` | User's name claim equals the specified value |

---

## Custom Authorization Handlers

When built-in requirement methods are insufficient -- for example, "allow the request only if the user's department claim matches the resource's assigned department" -- you implement a custom handler.

### Step 1: Define a requirement

An `IAuthorizationRequirement` is a marker class that carries any data the handler needs.

```csharp
public class MinimumExperienceRequirement : IAuthorizationRequirement
{
    public int MinimumYears { get; }
    public MinimumExperienceRequirement(int years) => MinimumYears = years;
}
```

### Step 2: Implement the handler

```csharp
public class MinimumExperienceHandler
    : AuthorizationHandler<MinimumExperienceRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MinimumExperienceRequirement requirement)
    {
        var yearsValue = context.User.FindFirstValue("years_experience");

        if (int.TryParse(yearsValue, out int years) && years >= requirement.MinimumYears)
            context.Succeed(requirement);
        // Do not call Fail() -- other handlers may still satisfy the requirement
        
        return Task.CompletedTask;
    }
}
```

Key points:
- Call `context.Succeed(requirement)` to signal the requirement is satisfied.
- Call `context.Fail()` to hard-fail regardless of other handlers.
- Returning without calling either means this handler neither succeeds nor fails -- other handlers get a chance.

### Step 3: Register the policy and handler

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SeniorEngineer", policy =>
        policy.Requirements.Add(new MinimumExperienceRequirement(5)));
});

// Register the handler in DI
builder.Services.AddSingleton<IAuthorizationHandler, MinimumExperienceHandler>();
```

### Step 4: Apply the policy

```csharp
[HttpPut("{id:int}")]
[Authorize(Policy = "SeniorEngineer")]
public IActionResult Update(int id, [FromBody] UpdateProductDto dto) { /* ... */ }
```

---

## Resource-Based Authorization (Conceptual)

Standard attribute-based authorization evaluates the policy before the action method executes -- at that point, the resource (e.g., the specific database row) has not been loaded. Resource-based authorization defers the check until inside the action, once the resource is retrieved.

```csharp
[HttpPut("{id:int}")]
[Authorize]
public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
{
    var product = await _productService.GetByIdAsync(id);
    if (product is null) return NotFound();

    // Check authorization against the specific resource
    var authResult = await _authorizationService
        .AuthorizeAsync(User, product, "CanEditProduct");

    if (!authResult.Succeeded) return Forbid();

    // Proceed with the update
    await _productService.UpdateAsync(id, dto);
    return NoContent();
}
```

Resource-based authorization requires injecting `IAuthorizationService` and writing a handler that receives the resource as its second generic type parameter: `AuthorizationHandler<TRequirement, TResource>`. This pattern is covered in greater depth in advanced ASP.NET Core material.

---

## Combining Roles and Policies

Role-based and policy-based approaches are not mutually exclusive. You can reference roles within a policy, stack multiple `[Authorize]` attributes, or mix `[Authorize(Roles = "...")]` with `[Authorize(Policy = "...")]` on the same endpoint. Each authorization attribute is evaluated independently and all must succeed.

```csharp
// Both the role check AND the policy must pass
[Authorize(Roles = "Admin")]
[Authorize(Policy = "SeniorEngineer")]
[HttpDelete("{id:int}")]
public IActionResult Delete(int id) { /* ... */ }
```

---

## Summary

- Authorization determines what an authenticated user **can do** -- it runs after authentication in the middleware pipeline.
- **Role-based authorization** is the quickest approach for coarse-grained access control.
- **Policy-based authorization** is the recommended approach: policies are named, composable, and testable units of access control logic.
- Policies can combine built-in methods (`RequireRole`, `RequireClaim`) with custom `IAuthorizationHandler` implementations for complex business rules.
- **Resource-based authorization** defers the access check until the resource is loaded, enabling fine-grained per-record decisions.
- A failed authentication challenge returns **401**; a failed authorization check returns **403**.

---

## Additional Resources

- [Authorization in ASP.NET Core (Microsoft Docs)](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction)
- [Policy-based authorization (Microsoft Docs)](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies)
- [Custom authorization handlers (Microsoft Docs)](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased)
