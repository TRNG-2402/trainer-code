# Caching

## Learning Objectives

- Explain why caching is a performance optimization strategy, not an optional concern, for production APIs.
- Implement in-memory caching with `IMemoryCache` in an ASP.NET Core service.
- Apply response caching using the `[ResponseCache]` attribute and `ResponseCachingMiddleware`.
- Understand the `IDistributedCache` abstraction and when to move beyond in-memory caching.
- Describe cache invalidation strategies and their trade-offs.

---

## Why This Matters

Every time a client calls `GET /api/products`, your application may execute a database query, serialize results, and return the payload. If the product catalog changes infrequently -- say, once every ten minutes -- repeating that query for every request wastes CPU, memory, and database connection time. At scale, this becomes a bottleneck.

Caching stores the result of an expensive operation and serves it to subsequent callers without repeating the operation. Done correctly, caching dramatically reduces response latency and database load, two metrics that appear in every production SLA. The production-ready API goal for this week demands that you understand when and how to cache.

---

## The Concept

### What Gets Cached and Why

Not all data is a good caching candidate. Evaluate data by two dimensions:

- **Read frequency:** Data that is read far more often than it is written benefits most from caching.
- **Staleness tolerance:** If clients can tolerate data that is a few seconds or minutes old, caching is appropriate. If data must be real-time (e.g., financial quotes, inventory counts), caching may be harmful.

Product lists, category trees, and configuration values are classic caching candidates. User-specific data and order details usually are not.

### In-Memory Caching with `IMemoryCache`

`IMemoryCache` stores cached entries in the process's own memory. It is the simplest caching option and requires no external infrastructure. It does not survive application restarts and does not share state across multiple server instances (pods/nodes in a scaled-out deployment).

**Registration:**

```csharp
// Program.cs
builder.Services.AddMemoryCache();
```

**Usage in a service:**

```csharp
public class ProductService : IProductService
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private const string ProductsCacheKey = "products_all";

    public ProductService(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        // Try to get the cached value first.
        if (_cache.TryGetValue(ProductsCacheKey, out List<Product>? cached))
        {
            return cached!;
        }

        // Cache miss -- query the database.
        var products = await _db.Products.AsNoTracking().ToListAsync();

        // Store in cache with a 60-second absolute expiration.
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
        };

        _cache.Set(ProductsCacheKey, products, options);
        return products;
    }
}
```

The pattern is always the same: check the cache first (`TryGetValue`), return early on a hit, perform the expensive operation on a miss, store the result, and return it.

### Cache Entry Options

`MemoryCacheEntryOptions` provides fine-grained control over cache lifetime:

| Option | Description |
|---|---|
| `AbsoluteExpiration` | Entry expires at a specific `DateTimeOffset`. |
| `AbsoluteExpirationRelativeToNow` | Entry expires after a `TimeSpan` from the time it was set. Use this most often. |
| `SlidingExpiration` | Entry expiration resets each time the entry is accessed. Useful for session-like data. |
| `Priority` | Hint to the eviction algorithm (`Low`, `Normal`, `High`, `NeverRemove`). |
| `Size` | Logical size for eviction purposes when `SizeLimit` is set on the cache. |

Combining `AbsoluteExpiration` with `SlidingExpiration` is valid: the entry expires when either limit is first reached.

### Cache Invalidation

Cache invalidation is one of the hardest problems in software engineering. There are three common strategies:

**1. Time-to-live (TTL) expiration.** The cache entry expires after a fixed time. Simple and predictable. Acceptable when brief staleness is tolerable.

**2. Explicit invalidation on write.** When data changes (POST, PUT, DELETE), remove the relevant cache entry immediately.

```csharp
public async Task UpdateAsync(Product product)
{
    _db.Products.Update(product);
    await _db.SaveChangesAsync();

    // Invalidate the cache so the next GET fetches fresh data.
    _cache.Remove(ProductsCacheKey);
}
```

**3. Cache-aside with versioning.** Store a version key alongside the data. On a write, increment the version. On a read, compare versions. This is more complex but handles distributed scenarios better.

For most APIs, combining a short TTL with explicit invalidation on writes is the right balance.

### Response Caching

Response caching instructs HTTP clients (browsers, proxies, CDNs) to cache entire HTTP responses. It operates at the HTTP layer, not the application layer. ASP.NET Core supports it via `ResponseCachingMiddleware` server-side and the `[ResponseCache]` attribute on controllers or actions.

**Registration:**

```csharp
// Program.cs
builder.Services.AddResponseCaching();

// ...

app.UseResponseCaching(); // Must be placed before routing and endpoints.
```

**Attribute usage:**

```csharp
[HttpGet]
[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "category" })]
public async Task<IActionResult> GetProducts([FromQuery] string? category)
{
    var products = await _productService.GetAllAsync();
    return Ok(products);
}
```

`Duration` sets the `max-age` value in the `Cache-Control` response header. `Location` controls whether caching is allowed by proxies (`Any`) or only the client (`Client`). `VaryByQueryKeys` causes separate cache entries per distinct query string value.

> **When to use which:** Use `IMemoryCache` for server-side caching of data that is expensive to compute (database queries, external API calls). Use `[ResponseCache]` to leverage HTTP caching infrastructure (browsers, CDNs) for public, cacheable responses. They can be used together.

### Distributed Caching with `IDistributedCache`

When an API runs on multiple server instances (load-balanced), `IMemoryCache` is inadequate: each instance has its own isolated cache. A cache write on instance A is invisible to instance B.

`IDistributedCache` is an ASP.NET Core abstraction over a shared, external cache store. Common providers:

| Provider | NuGet Package |
|---|---|
| Redis | `Microsoft.Extensions.Caching.StackExchangeRedis` |
| SQL Server | `Microsoft.Extensions.Caching.SqlServer` |
| NCache | Third-party |

**Conceptual registration (Redis):**

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
```

The `IDistributedCache` API works with `byte[]` or `string`, so you must serialize your objects manually (typically with `System.Text.Json`):

```csharp
public async Task<List<Product>> GetAllAsync()
{
    var key = "products_all";
    var cached = await _distributedCache.GetStringAsync(key);

    if (cached is not null)
    {
        return JsonSerializer.Deserialize<List<Product>>(cached)!;
    }

    var products = await _db.Products.AsNoTracking().ToListAsync();

    await _distributedCache.SetStringAsync(key,
        JsonSerializer.Serialize(products),
        new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
        });

    return products;
}
```

We will not implement a distributed cache in this week's exercises; the concept is introduced here as a production consideration you will encounter when APIs scale horizontally.

### ETags (Brief Introduction)

An ETag (Entity Tag) is an HTTP response header containing a hash of the response body. Clients can send this hash back in the `If-None-Match` request header. If the server's current hash matches, it returns `304 Not Modified` with no body, saving bandwidth even when cache entries have expired. ASP.NET Core does not generate ETags automatically for Web API responses; you would implement them manually or via a library. This is an advanced HTTP caching topic beyond this week's scope.

---

## Summary

- `IMemoryCache` caches data in the process's own memory -- fast and simple, but not shared across instances.
- Register with `AddMemoryCache()`, inject into services, and use `TryGetValue` + `Set` with `MemoryCacheEntryOptions`.
- Invalidate explicitly on writes to prevent stale data accumulating beyond the TTL.
- `[ResponseCache]` + `ResponseCachingMiddleware` operate at the HTTP layer, enabling client and proxy caching via `Cache-Control` headers.
- `IDistributedCache` (backed by Redis or SQL Server) is the correct solution for multi-instance deployments; it shares a single cache across all server nodes.
- Choose TTL and invalidation strategy based on how frequently data changes and how much staleness is acceptable.

---

## Additional Resources

- [Caching in ASP.NET Core - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/overview)
- [IMemoryCache - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory)
- [Distributed caching in ASP.NET Core - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed)
