# HTTP Verbs

## Learning Objectives

- State the semantic meaning of the five primary HTTP methods: GET, POST, PUT, PATCH, and DELETE.
- Define idempotency and safety, and correctly classify each method by those properties.
- Map each HTTP method to the corresponding CRUD operation.
- Identify common misuse patterns and explain why they violate REST semantics.

---

## Why This Matters

HTTP methods are the **action vocabulary** of a REST API. Using them correctly is not pedantic formalism — it has real operational consequences. Clients, proxies, CDNs, and caches all make decisions based on the HTTP method. A `GET` request can be safely retried automatically by a browser; a `POST` cannot. A `PUT` can be cached by an intermediary; a `DELETE` triggers specific proxy behaviors.

When you implement ASP.NET Core controllers next week, every action method will be decorated with an HTTP method attribute (`[HttpGet]`, `[HttpPost]`, etc.). Understanding the semantics behind those attributes is what separates well-designed APIs from ones that cause client bugs in production.

---

## The Concept

### The Five Primary Methods

#### GET

`GET` retrieves a representation of a resource. It has no body.

```
GET /products/42 HTTP/1.1
Host: api.example.com
Authorization: Bearer <token>
```

`GET` is the foundation of the web. Every browser navigation, every hyperlink click, is a `GET`. An API `GET` request should never modify server state.

#### POST

`POST` submits data to create a new resource or trigger a process. The body contains the representation of the new resource.

```
POST /products HTTP/1.1
Host: api.example.com
Content-Type: application/json
Authorization: Bearer <token>

{
  "name": "Widget Pro",
  "price": 29.99,
  "categoryId": 3
}
```

On success, the server typically responds with `201 Created` and a `Location` header pointing to the newly created resource.

#### PUT

`PUT` replaces a resource entirely with the provided representation. The client sends the complete updated resource; the server replaces whatever was there.

```
PUT /products/42 HTTP/1.1
Host: api.example.com
Content-Type: application/json

{
  "id": 42,
  "name": "Widget Pro v2",
  "price": 34.99,
  "categoryId": 3
}
```

If the resource does not exist, `PUT` may create it (though this is API-specific). The key characteristic is that `PUT` is a **full replacement** — any fields omitted from the body are effectively deleted from the resource.

#### PATCH

`PATCH` applies a **partial update** to a resource. Only the fields included in the body are changed; all other fields retain their current values.

```
PATCH /products/42 HTTP/1.1
Host: api.example.com
Content-Type: application/json

{
  "price": 39.99
}
```

Use `PATCH` when updating one or two fields of a large resource. Sending the entire resource via `PUT` is wasteful and error-prone (a client might accidentally clear a field it did not intend to touch).

#### DELETE

`DELETE` removes the specified resource.

```
DELETE /products/42 HTTP/1.1
Host: api.example.com
Authorization: Bearer <token>
```

A successful `DELETE` responds with `204 No Content` (resource deleted, no body) or occasionally `200 OK` with a confirmation body.

---

### Idempotency and Safety

These two properties are fundamental to how HTTP methods are expected to behave. HTTP clients, libraries, and infrastructure rely on these properties to decide whether to retry, cache, or forward requests.

| Property | Definition |
|---|---|
| **Safe** | A method is safe if it does not modify server state. Safe methods are read-only. |
| **Idempotent** | A method is idempotent if making the same request multiple times produces the same server state as making it once. |

**Important:** Idempotency is about the **server state outcome**, not the response. A `DELETE` returns `204` on the first call and `404` on a repeat call — the responses differ, but the resulting server state (resource does not exist) is the same. Therefore `DELETE` is idempotent.

| Method | Safe | Idempotent | Notes |
|---|---|---|---|
| **GET** | Yes | Yes | Read-only, no state change. |
| **HEAD** | Yes | Yes | Same as GET but response has no body. Used to check resource existence. |
| **OPTIONS** | Yes | Yes | Returns the allowed methods for a URI. |
| **POST** | No | **No** | Two identical `POST /orders` requests create two orders. |
| **PUT** | No | Yes | Replacing a resource twice with the same data yields the same result. |
| **PATCH** | No | Not always | Depends on implementation. A `PATCH` that sets `{"price": 39.99}` is idempotent; one that says `{"discount": "+5%"}` is not. |
| **DELETE** | No | Yes | Deleting a non-existent resource may return 404, but the state is the same. |

### Mapping to CRUD

| CRUD Operation | HTTP Method | URI Pattern | Success Code |
|---|---|---|---|
| Create | `POST` | `/resources` | `201 Created` |
| Read (list) | `GET` | `/resources` | `200 OK` |
| Read (single) | `GET` | `/resources/{id}` | `200 OK` |
| Update (full) | `PUT` | `/resources/{id}` | `200 OK` or `204` |
| Update (partial) | `PATCH` | `/resources/{id}` | `200 OK` or `204` |
| Delete | `DELETE` | `/resources/{id}` | `204 No Content` |

---

### Common Misuse Patterns

#### Using GET to perform mutations

```
GET /products/42/delete    -- Wrong
DELETE /products/42        -- Correct
```

`GET` requests can be bookmarked, prefetched, and cached. A `GET` that deletes data will be triggered by crawlers, link previewers, and monitoring tools — with destructive results.

#### Using POST for all operations

Level 0 in the Richardson Maturity Model is the result of using `POST` for everything. It bypasses all the semantic value HTTP methods provide and makes the API impossible to cache or standardize.

#### Sending a partial body via PUT

```json
// PUT /products/42 with only { "price": 39.99 }
// This will null out name, categoryId, and all other fields
// in a naive PUT implementation. Use PATCH instead.
```

#### Using DELETE with a body

The HTTP specification does not prohibit a body on `DELETE`, but many proxies and frameworks strip it. Do not rely on a `DELETE` body for anything critical. If you need to pass data with a delete (e.g., specifying a reason), use a query parameter or a separate `PATCH` status before deletion.

---

## Summary

- The five primary HTTP methods are `GET`, `POST`, `PUT`, `PATCH`, and `DELETE`, each with distinct semantics.
- **Safe** methods do not modify state. **Idempotent** methods can be repeated without changing the outcome beyond the first call.
- `GET` and `HEAD` are both safe and idempotent. `POST` is neither. `PUT` and `DELETE` are idempotent but not safe.
- `PUT` performs a full resource replacement; `PATCH` performs a partial update.
- Misusing HTTP methods (GET for mutations, POST for everything) defeats caching, breaks client retries, and produces fragile APIs.

---

## Additional Resources

- [MDN — HTTP request methods](https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods)
- [RFC 9110 — HTTP Semantics (Methods)](https://www.rfc-editor.org/rfc/rfc9110#name-methods)
- [REST API Tutorial — HTTP Methods](https://restfulapi.net/http-methods/)
