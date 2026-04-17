# HTTP Status Codes

## Learning Objectives

- Describe the five status code families and the general meaning of each.
- Select the appropriate status code for a given API scenario.
- Distinguish between client errors (4xx) and server errors (5xx) and their diagnostic implications.
- Identify the key codes required for a well-designed CRUD API.

---

## Why This Matters

Status codes are the **response vocabulary** of HTTP. They tell the client exactly what happened, without requiring the client to parse the body. Correctly choosing status codes is one of the highest-leverage improvements you can make to an API's usability:

- A client library can automatically retry on `503` but not on `400`.
- A client can cache a `200` response but not a `201`.
- A developer integrating your API can distinguish a "your request was wrong" problem (`4xx`) from a "our server is broken" problem (`5xx`) at a glance.

When you implement ASP.NET Core action methods in Week 2, you will return specific `IActionResult` types like `Ok()`, `Created()`, `BadRequest()`, and `NotFound()`. Each of those maps directly to a status code covered in this reading.

---

## The Concept

### The Five Families

HTTP status codes are three-digit integers grouped by their first digit.

| Family | Range | Meaning |
|---|---|---|
| **1xx — Informational** | 100–199 | The request was received. Continue processing. |
| **2xx — Success** | 200–299 | The request was received, understood, and accepted. |
| **3xx — Redirection** | 300–399 | Further action is needed to complete the request. |
| **4xx — Client Error** | 400–499 | The request contained an error attributable to the client. |
| **5xx — Server Error** | 500–599 | The server failed to fulfill a valid request. |

---

### 2xx — Success Codes

#### 200 OK

The most common success response. The request succeeded and the response body contains the result.

Use for: `GET` and `PUT`/`PATCH` responses that return the updated resource.

#### 201 Created

The request succeeded and a new resource was created as a result.

When to use: Always on a successful `POST` that creates a resource. Include a `Location` header pointing to the new resource.

```
HTTP/1.1 201 Created
Location: /products/43
Content-Type: application/json

{ "id": 43, "name": "Widget Pro", "price": 29.99 }
```

#### 204 No Content

The request succeeded but there is no body to return.

When to use: Successful `DELETE` operations, and `PUT`/`PATCH` operations where you choose not to return the updated resource.

---

### 3xx — Redirection Codes

#### 301 Moved Permanently

The resource has been permanently moved to a new URI (provided in the `Location` header). Clients and search engines should update their references.

When to use: API versioning migrations (`/v1/products` redirecting to `/v2/products`), or when endpoints are renamed.

#### 304 Not Modified

Used in conjunction with conditional requests (`If-None-Match`, `If-Modified-Since`). The client already has a valid cached copy — do not re-send the body.

Benefit: Reduces bandwidth. The client sends its cached `ETag` with the request; if the server's current `ETag` matches, it returns `304` with no body.

---

### 4xx — Client Error Codes

4xx codes mean the **client made a mistake**. The server received the request but cannot or will not process it due to something the client did.

#### 400 Bad Request

The request body or parameters are malformed and cannot be understood by the server.

```
POST /products
{ "name": "", price: "not-a-number" }
→ 400 Bad Request
Body: { "error": "Price must be a valid decimal number." }
```

Use for: Syntactically invalid JSON, missing required fields, type mismatches.

#### 401 Unauthorized

The request requires authentication, and the client did not provide valid credentials (or provided none at all).

```
GET /products
→ 401 Unauthorized
WWW-Authenticate: Bearer realm="api.example.com"
```

Note the confusing naming: `401 Unauthorized` means "not authenticated." The code for authenticated-but-not-permitted is `403`.

#### 403 Forbidden

The client is authenticated (the server knows who you are), but you do not have permission to access the requested resource.

```
DELETE /products/42
Authorization: Bearer <valid-token-for-readonly-user>
→ 403 Forbidden
```

Use for: Role-based access control failures. Do not use `404` to hide the existence of a resource from a forbidden user — that is a separate decision (security through obscurity) and should be explicit.

#### 404 Not Found

The resource does not exist at the specified URI.

```
GET /products/9999
→ 404 Not Found
```

Also used intentionally to indicate that an endpoint does not exist at all.

#### 409 Conflict

The request cannot be completed because it conflicts with the current state of the resource.

```
POST /users
{ "email": "alice@example.com" }
// alice@example.com already exists
→ 409 Conflict
Body: { "error": "A user with this email address already exists." }
```

Use for: Unique constraint violations, optimistic concurrency conflicts, state machine violations (e.g., trying to ship an already-shipped order).

#### 422 Unprocessable Entity

The request body is syntactically valid (parseable JSON) but semantically invalid (the data violates business rules).

```
POST /orders
{ "quantity": -5, "productId": 42 }
→ 422 Unprocessable Entity
Body: { "error": "Quantity must be greater than zero." }
```

The distinction between `400` and `422` is subtle and evolving. A common convention:
- `400` = the server cannot parse the request at all.
- `422` = the server parsed the request but the contents fail validation.

ASP.NET Core's built-in model validation returns `400` by default; some teams configure it to return `422`. Be consistent within a single API.

---

### 5xx — Server Error Codes

5xx codes mean **the server failed**. The client's request may have been valid, but the server encountered an error processing it.

#### 500 Internal Server Error

An unexpected condition occurred on the server. This is the generic catch-all for unhandled exceptions.

```
GET /products/42
→ 500 Internal Server Error
// NullReferenceException in ProductService
```

Production APIs should never return raw exception details in the body. Return a generic error message; log the full stack trace internally.

#### 503 Service Unavailable

The server is temporarily unable to handle the request. Common causes: the server is starting up, overloaded, or a critical dependency (the database) is unavailable.

Clients and load balancers treat `503` as retryable. The response should include a `Retry-After` header indicating when the client can try again.

```
HTTP/1.1 503 Service Unavailable
Retry-After: 30
```

---

### Quick Reference — CRUD API Status Codes

| Operation | Success Code | Common Error Codes |
|---|---|---|
| `POST` — Create | `201 Created` | `400` (validation), `409` (duplicate), `422` (business rule) |
| `GET` — Read | `200 OK` | `404` (not found), `401`/`403` (access) |
| `PUT`/`PATCH` — Update | `200 OK` or `204 No Content` | `400`, `404`, `409`, `422` |
| `DELETE` — Delete | `204 No Content` | `404` (not found), `403` (forbidden) |

---

## Summary

- The five status code families are 1xx (informational), 2xx (success), 3xx (redirection), 4xx (client error), and 5xx (server error).
- Key 2xx codes: `200 OK`, `201 Created`, `204 No Content`.
- Key 3xx codes: `301 Moved Permanently`, `304 Not Modified`.
- Key 4xx codes: `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `409 Conflict`, `422 Unprocessable Entity`.
- Key 5xx codes: `500 Internal Server Error`, `503 Service Unavailable`.
- `401` means unauthenticated; `403` means authenticated but unauthorized.
- `400` and `422` address different levels of invalid input — invalid format vs. invalid business content.

---

## Additional Resources

- [MDN — HTTP response status codes](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status)
- [RFC 9110 — HTTP Semantics (Status Codes)](https://www.rfc-editor.org/rfc/rfc9110#name-status-codes)
- [httpstatuses.com — Quick reference](https://httpstatuses.com/)
