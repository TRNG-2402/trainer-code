# Filters

## Learning Objectives

- Identify the five ASP.NET Core filter types and describe the distinct phase each one targets.
- Explain the execution order of filters in the pipeline.
- Implement `IActionFilter` and `IAsyncActionFilter`.
- Register filters globally, at the controller level, and at the action level.
- Use `[ServiceFilter]` and `[TypeFilter]` to resolve DI-aware filters from the container.

---

## Why This Matters

Middleware handles cross-cutting concerns at the HTTP pipeline level -- before routing has matched a request to an endpoint. Filters handle cross-cutting concerns that are specific to MVC/controller execution: they run after routing has succeeded and before (or after) a specific action method executes.

If you need to log every incoming HTTP request regardless of whether a controller is involved, use middleware. If you need to validate a business rule before a specific controller action runs -- or audit the result it returns -- use a filter.

Filters let you enforce concerns such as audit logging, performance measurement, input sanitization, and consistent error formatting without embedding that logic directly inside action methods. The result is action methods that contain only business logic, which are easier to read, test, and maintain.

---

## The Concept

### The Five Filter Types

ASP.NET Core MVC defines five filter types, each targeting a specific phase of the MVC execution pipeline:

| Filter Type | Interface | Runs When |
|---|---|---|
| **Authorization** | `IAuthorizationFilter` | First -- before all other filters. Short-circuits if the user is not authorized. |
| **Resource** | `IResourceFilter` | After authorization, before model binding. Can short-circuit or cache results. |
| **Action** | `IActionFilter` | Before and after the action method executes. The most commonly used filter type. |
| **Exception** | `IExceptionFilter` | When an unhandled exception is thrown within the filter pipeline. |
| **Result** | `IResultFilter` | Before and after the action result (`IActionResult`) is executed (i.e., before and after the response is written). |

The execution order for a request that passes all filters is:

```
Authorization filter
  -> Resource filter (before)
    -> Model binding
      -> Action filter (before)
        -> Action method executes
      -> Action filter (after)
    -> Result filter (before)
      -> Action result executes (response written)
    -> Result filter (after)
  -> Resource filter (after)
```

Exception filters intercept at any point within `-> Action method executes` and `-> Action result executes`.

### Action Filters -- The Most Common Type

Action filters are the workhorse filter type. They execute code immediately before and after an action method.

#### Synchronous implementation

```csharp
public class LogActionFilter : IActionFilter
{
    private readonly ILogger<LogActionFilter> _logger;

    public LogActionFilter(ILogger<LogActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Runs before the action method
        _logger.LogInformation(
            "Executing {Controller}.{Action} with args: {Args}",
            context.RouteData.Values["controller"],
            context.RouteData.Values["action"],
            context.ActionArguments);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Runs after the action method
        _logger.LogInformation(
            "Executed {Controller}.{Action}",
            context.RouteData.Values["controller"],
            context.RouteData.Values["action"]);
    }
}
```

#### Asynchronous implementation

For async action filters, implement `IAsyncActionFilter` and its single method `OnActionExecutionAsync`. The call to `next()` is where the action method executes.

```csharp
public class TimingActionFilter : IAsyncActionFilter
{
    private readonly ILogger<TimingActionFilter> _logger;

    public TimingActionFilter(ILogger<TimingActionFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Everything before next() is "before"
        var executedContext = await next(); // Action method runs here
        // Everything after next() is "after"

        stopwatch.Stop();

        if (executedContext.Exception is not null)
        {
            _logger.LogError(executedContext.Exception, "Action threw an exception.");
        }
        else
        {
            _logger.LogInformation(
                "Action completed in {Elapsed} ms.", stopwatch.ElapsedMilliseconds);
        }
    }
}
```

### Exception Filters

Exception filters handle unhandled exceptions thrown by action methods or other filters. They provide a centralized place to translate exceptions into consistent API error responses.

```csharp
public class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception caught by filter.");

        context.Result = new ObjectResult(new
        {
            title = "An unexpected error occurred.",
            status = 500,
            detail = context.Exception.Message
        })
        {
            StatusCode = 500
        };

        // Mark the exception as handled so it is not re-thrown
        context.ExceptionHandled = true;
    }
}
```

Note: for global exception handling at the pipeline level (outside MVC), use the `UseExceptionHandler` middleware covered in `middleware.md`. Exception filters only catch exceptions within the MVC filter pipeline.

### Filter Registration

Filters can be registered at three scopes. When multiple filters of the same type apply to a request, the broader scope runs first.

#### Global registration (applies to every action)

```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<LogActionFilter>();
    // Or with a type filter for DI support:
    options.Filters.Add(typeof(TimingActionFilter));
});
```

#### Controller-level (applies to all actions on the controller)

```csharp
[ApiController]
[Route("api/[controller]")]
[ServiceFilter(typeof(LogActionFilter))]
public class ProductsController : ControllerBase
{
    // ...
}
```

#### Action-level (applies to a single action)

```csharp
[HttpDelete("{id:int}")]
[TypeFilter(typeof(ApiExceptionFilter))]
public IActionResult Delete(int id)
{
    // ...
}
```

### `[ServiceFilter]` vs. `[TypeFilter]`

Filters applied as attributes must have constructors compatible with attribute argument restrictions (no interfaces, no complex objects). When a filter requires services from the DI container, you cannot pass those dependencies as attribute constructor arguments. Two attributes solve this problem differently:

| Attribute | Behavior |
|---|---|
| `[ServiceFilter(typeof(T))]` | Resolves the filter instance from the DI container. The filter type **must be registered** in the container (`builder.Services.AddScoped<T>()`). |
| `[TypeFilter(typeof(T))]` | Creates the filter instance using the DI container to resolve its constructor parameters, but the filter type does **not** need to be registered separately. Supports passing additional constructor arguments via `Arguments`. |

```csharp
// ServiceFilter -- filter must be registered in DI
builder.Services.AddScoped<LogActionFilter>();

[ServiceFilter(typeof(LogActionFilter))]
public class ProductsController : ControllerBase { }

// TypeFilter -- no prior registration needed
[TypeFilter(typeof(ApiExceptionFilter))]
public IActionResult Delete(int id) { }
```

---

## Code Example

A controller with a globally registered timing filter and a controller-scoped log filter, demonstrating the full registration pattern:

```csharp
// Program.cs
builder.Services.AddScoped<LogActionFilter>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<TimingActionFilter>(); // Global -- runs on every action
});

// ProductsController.cs
[ApiController]
[Route("api/[controller]")]
[ServiceFilter(typeof(LogActionFilter))] // Controller-scoped
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ProductDto>> GetAll()
    {
        return Ok(_service.GetAll());
    }

    [HttpPost]
    [TypeFilter(typeof(ApiExceptionFilter))] // Action-scoped
    public ActionResult<ProductDto> Create([FromBody] CreateProductDto dto)
    {
        var result = _service.Create(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
```

For a `POST /api/products` request, the execution order is:

1. `TimingActionFilter.OnActionExecutionAsync` (before) -- global
2. `LogActionFilter.OnActionExecuting` -- controller-scoped
3. `ApiExceptionFilter.OnException` intercepts if `_service.Create` throws
4. `Create` action method executes
5. `LogActionFilter.OnActionExecuted` -- controller-scoped
6. `TimingActionFilter.OnActionExecutionAsync` (after) -- global

---

## Summary

- ASP.NET Core defines five filter types: Authorization, Resource, Action, Exception, and Result -- each targeting a distinct phase of MVC execution.
- Action filters are the most common type; implement `IActionFilter` (sync) or `IAsyncActionFilter` (async).
- In `IAsyncActionFilter`, the call to `next()` is the boundary between before and after logic.
- Register filters globally (via `AddControllers` options), at the controller class, or at the action method.
- Use `[ServiceFilter]` when the filter is registered in the DI container; use `[TypeFilter]` when you need the container to resolve dependencies without pre-registering the filter itself.

---

## Additional Resources

- [Microsoft Docs -- Filters in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters)
- [Microsoft Docs -- Filter scopes and order of execution](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters#filter-scopes-and-order-of-execution)
- [Microsoft Docs -- Dependency injection in filters](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters#dependency-injection)
