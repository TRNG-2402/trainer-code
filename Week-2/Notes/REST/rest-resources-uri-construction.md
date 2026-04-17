# REST Resources and URI Construction

## Learning Objectives

- Apply the "resources as nouns" rule to design clean URI surfaces.
- Explain the plural naming convention and when nested resources are appropriate.
- Distinguish between path parameters and query parameters and select the correct one for a given use case.
- Articulate at least three API versioning strategies and describe the trade-offs of each.
- Identify URI anti-patterns and correct them.

---

## Why This Matters

URI design is the most visible decision you make as an API author. Once a URI is published and consumed by clients, changing it is a breaking change. Every public REST API you have ever integrated with — GitHub, Stripe, Twilio — has a URI surface that was designed deliberately. The conventions are not arbitrary; they encode information that makes the API predictable.

When you build ASP.NET Core controllers in Week 2, you will define routes using attributes like `[Route("api/products")]` and `[HttpGet("{id}")]`. The patterns in this reading are the principles you will be encoding in those attributes.

---

## The Concept

### Resources as Nouns

The foundational rule of REST URI design is that URIs identify **resources** (things), not **actions** (verbs). The HTTP method already carries the action.

| Wrong (verb in URI) | Correct (noun-based) |
|---|---|
| `POST /createProduct` | `POST /products` |
| `GET /getProduct/42` | `GET /products/42` |
| `DELETE /deleteProduct/42` | `DELETE /products/42` |
| `POST /products/42/doUpdate` | `PUT /products/42` |

The pattern is: `{method} /{resource}/{identifier}`.

### Plural Naming Convention

Resource names in URIs should be **plural nouns**. This treats the URI as a collection, which is consistent whether you are addressing the collection itself or an item within it.

```
/products            → the collection of all products
/products/42         → one specific product in that collection
/orders              → the collection of all orders
/orders/7            → one specific order
```

Using plural consistently means a client can intuit the URI for any resource without out-of-band documentation. The API surface becomes predictable.

### Hierarchical Nesting (Sub-Resources)

When a resource logically exists within another resource, nesting expresses that relationship:

```
/orders/7/items          → the line items belonging to order 7
/orders/7/items/3        → line item 3 of order 7
/users/15/addresses      → the addresses belonging to user 15
```

**Rules for nesting:**

1. Keep nesting shallow. Two levels is the practical maximum for most APIs. Three or more levels become unwieldy for clients.
2. Only nest when the child resource does not have a meaningful independent identity. If a resource makes sense on its own (e.g., `Product` can exist independently of a category), a top-level URI is preferable.

```
/categories/3/products    -- acceptable if products are always viewed in category context
/products?categoryId=3    -- preferred if products are a first-class resource
```

### Path Parameters vs. Query Parameters

Both path parameters and query parameters pass data to the server, but they serve different purposes.

**Path parameters** identify a specific resource or sub-resource. They are a required part of the URI structure.

```
/products/{id}
/orders/{orderId}/items/{itemId}
```

**Query parameters** filter, sort, paginate, or project the resource collection. They are optional modifiers.

```
/products?categoryId=3&sort=price&order=asc&page=2&pageSize=25
/orders?status=pending&createdAfter=2026-01-01
```

The rule of thumb:

| Use case | Mechanism |
|---|---|
| Identifying a specific resource | Path parameter |
| Filtering a collection | Query parameter |
| Sorting a collection | Query parameter |
| Paginating a collection | Query parameter |
| Selecting which fields to return (sparse fieldsets) | Query parameter |

Mixing these up produces confusing URIs:

```
// Wrong — filtering in the path
GET /products/category/3/sort/price

// Correct
GET /products?categoryId=3&sort=price
```

### Versioning Strategies

REST APIs change over time. Versioning strategies allow you to evolve the API without breaking existing clients.

#### Option 1 — URI Path Versioning

The version is embedded in the URI path.

```
GET /v1/products/42
GET /v2/products/42
```

**Pros:** Simple to understand, easy to test in a browser or Postman, easy to route in ASP.NET Core.

**Cons:** Technically violates the REST principle that a URI should identify a single resource unambiguously (the same resource now appears at two URIs). The URI is no longer purely about the resource.

This is the most common approach in industry despite the theoretical objection.

#### Option 2 — Query Parameter Versioning

```
GET /products/42?api-version=2.0
```

**Pros:** The base URI is stable. Clients that do not specify a version receive the default (usually the latest stable).

**Cons:** Easy to forget to include. Pollutes query parameter space. Harder to route at the proxy level.

#### Option 3 — Header Versioning (Content Negotiation)

```
GET /products/42
Accept: application/vnd.example.v2+json
```

**Pros:** The URI is clean. Follows HTTP content negotiation semantics precisely.

**Cons:** Not visible in a browser address bar or basic Postman test. Harder to document and explain to consumers.

**Industry default:** URI path versioning (`/v1/`, `/v2/`) is most widely used because it is most visible and easiest to route.

---

### Avoiding Verbs in URIs

Verbs in URIs are the most common anti-pattern in early API designs. They are almost always avoidable by choosing the correct HTTP method.

| Verb-URI (wrong) | REST URI (correct) | Method |
|---|---|---|
| `/products/search` | `/products?name=widget` | `GET` |
| `/orders/42/cancel` | `/orders/42` | `PATCH` with `{ "status": "cancelled" }` |
| `/users/15/activate` | `/users/15` | `PATCH` with `{ "active": true }` |
| `/reports/generate` | `/reports` | `POST` |

The exception is **controller-style actions** that represent a process rather than a resource mutation. Some teams use a `/actions` sub-resource or a `/:action` suffix for these:

```
POST /payments/42/refunds      -- preferred
POST /payments/42/actions/refund  -- acceptable, documents the intent
```

---

### URI Design Checklist

Before finalizing a URI surface, verify each endpoint against these rules:

- [ ] The URI identifies a resource (noun), not an action (verb).
- [ ] Resource names are plural.
- [ ] Path parameters identify specific resources; query parameters filter or sort.
- [ ] Nesting depth is at most two levels, and nesting is used only where the parent-child relationship is inherent.
- [ ] A versioning strategy is chosen and applied consistently.
- [ ] No HTTP method names appear in the URI (`getProduct`, `deleteOrder`).

---

### Full Example — Blog API URI Surface

Consider a Blog domain with Users, Posts, and Comments.

```
Users
GET     /v1/users                    -- list all users
POST    /v1/users                    -- create a user
GET     /v1/users/{userId}           -- get a specific user
PUT     /v1/users/{userId}           -- replace a user
PATCH   /v1/users/{userId}           -- partial update a user
DELETE  /v1/users/{userId}           -- delete a user

Posts
GET     /v1/posts                    -- list all posts (filter by ?authorId=)
POST    /v1/posts                    -- create a post
GET     /v1/posts/{postId}           -- get a specific post
PUT     /v1/posts/{postId}           -- replace a post
PATCH   /v1/posts/{postId}           -- partial update (e.g., publish)
DELETE  /v1/posts/{postId}           -- delete a post

Comments (nested — a comment belongs to a post)
GET     /v1/posts/{postId}/comments         -- list comments on a post
POST    /v1/posts/{postId}/comments         -- add a comment to a post
GET     /v1/posts/{postId}/comments/{id}    -- get a specific comment
PATCH   /v1/posts/{postId}/comments/{id}    -- edit a comment
DELETE  /v1/posts/{postId}/comments/{id}    -- delete a comment
```

---

## Summary

- URIs identify resources (nouns), not actions. HTTP methods carry the action.
- Resource names are plural. The collection and item URIs form a predictable pair: `/products` and `/products/{id}`.
- Path parameters identify specific resources. Query parameters filter, sort, and paginate collections.
- Keep nesting shallow (two levels maximum). Nest only when the parent-child relationship is inherent to the resource's identity.
- URI path versioning (`/v1/`) is the most common versioning strategy. Choose one strategy and apply it consistently.
- Avoid verbs in URIs. If an action does not map cleanly to a CRUD operation, model it as a status change via `PATCH` or as a new sub-resource.

---

## Additional Resources

- [Microsoft — RESTful API Design Guidelines](https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design)
- [REST API Tutorial — Resource Naming](https://restfulapi.net/resource-naming/)
- [Zalando RESTful API Guidelines](https://opensource.zalando.com/restful-api-guidelines/)
