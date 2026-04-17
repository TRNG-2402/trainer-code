# HTTP Request and Response Structure

## Learning Objectives

- Identify every component of an HTTP request: method, path, HTTP version, headers, and body.
- Identify every component of an HTTP response: status line, headers, and body.
- State the purpose of the most common HTTP headers: `Content-Type`, `Authorization`, `Accept`, and `Cache-Control`.
- Construct a valid raw HTTP request from memory for a given API scenario.

---

## Why This Matters

When you call an ASP.NET Core action method in Week 2, the framework parses an incoming HTTP request and hands you its pieces: the route, query parameters, the deserialized body, and the headers. When you return from that action, the framework builds an HTTP response from your return value. Understanding the raw structure of those messages means you can troubleshoot serialization issues, diagnose header-related auth failures, and reason about what the framework is doing on your behalf rather than treating it as a magic box.

---

## The Concept

### HTTP Request Structure

Every HTTP request has three sections: the **request line**, the **headers**, and the optional **body**.

```
POST /products HTTP/1.1
Host: api.example.com
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Accept: application/json
Content-Length: 58

{
  "name": "Widget Pro",
  "price": 29.99,
  "categoryId": 3
}
```

#### The Request Line

```
POST /products HTTP/1.1
│    │          │
│    │          └── HTTP version
│    └────────────── Request URI (path)
└─────────────────── HTTP method
```

The request line is always the first line. It specifies what action (`POST`) is being requested on what resource (`/products`) using what version of the protocol (`HTTP/1.1`).

Query parameters appear in the URI:

```
GET /products?categoryId=3&sort=price HTTP/1.1
```

#### Request Headers

Headers are key-value pairs, one per line, separated by a colon and a space. They carry metadata about the request.

```
Host: api.example.com
Content-Type: application/json
Authorization: Bearer <token>
Accept: application/json
Content-Length: 58
```

A blank line (`\r\n\r\n`) terminates the headers section. Everything after that blank line is the body.

#### Request Body

The body is optional. `GET`, `HEAD`, `DELETE`, and `OPTIONS` requests typically have no body.

`POST`, `PUT`, and `PATCH` requests carry the resource representation in the body. The `Content-Type` header tells the server how to parse it.

---

### HTTP Response Structure

Every HTTP response has three sections: the **status line**, the **headers**, and the optional **body**.

```
HTTP/1.1 201 Created
Content-Type: application/json
Location: /products/43
Date: Fri, 17 Apr 2026 13:00:00 GMT
Content-Length: 62

{
  "id": 43,
  "name": "Widget Pro",
  "price": 29.99,
  "categoryId": 3
}
```

#### The Status Line

```
HTTP/1.1 201 Created
│        │   │
│        │   └── Human-readable reason phrase
│        └────── Status code
└─────────────── HTTP version
```

The status code drives all client-side logic. The reason phrase (`Created`, `Not Found`, etc.) is informational and ignored by most clients in practice.

#### Response Headers

Response headers carry metadata about the response and instructions to the client.

```
Content-Type: application/json
Location: /products/43
Date: Fri, 17 Apr 2026 13:00:00 GMT
Content-Length: 62
Cache-Control: no-store
```

#### Response Body

The body contains the resource representation. It may be absent (`204 No Content`) or present (`200 OK` with a JSON payload). The `Content-Type` header tells the client how to parse it.

---

### Common Headers in Detail

#### Content-Type

Describes the media type (MIME type) of the body. Present on both requests (telling the server how to parse the body) and responses (telling the client how to parse the body).

| Value | Meaning |
|---|---|
| `application/json` | JSON body. The standard for REST APIs. |
| `application/xml` | XML body. |
| `multipart/form-data` | File upload. |
| `application/x-www-form-urlencoded` | HTML form submission. |
| `text/plain` | Plain text. |

In ASP.NET Core, returning `Ok(myObject)` serializes `myObject` to JSON and sets `Content-Type: application/json` automatically.

#### Authorization

Carries credentials for authenticating the request. The most common scheme in REST APIs is `Bearer`:

```
Authorization: Bearer <JWT-or-opaque-token>
```

The server reads this header, validates the token, and either processes the request or returns `401 Unauthorized`.

Other schemes you may encounter:

| Scheme | Use |
|---|---|
| `Basic` | Base64-encoded `username:password`. Only safe over HTTPS. |
| `Bearer` | OAuth 2.0 / JWT token. The REST API standard. |
| `ApiKey` | Some APIs use a custom `X-API-Key` header instead of `Authorization`. |

#### Accept

Tells the server what media types the client can process. The server uses this to perform **content negotiation**.

```
Accept: application/json
Accept: application/json, application/xml;q=0.9
```

The `q=0.9` is a quality factor (0 to 1) indicating preference. `q=1.0` (implicit default) is most preferred.

ASP.NET Core's content negotiation respects the `Accept` header and can return JSON or XML based on client preference, if configured.

#### Cache-Control

Directs caching behavior for both requests and responses.

| Directive | Meaning |
|---|---|
| `no-store` | Do not cache this response at all. |
| `no-cache` | Cache the response but always revalidate with the server before using it. |
| `max-age=3600` | The response is fresh for 3600 seconds (1 hour). |
| `private` | Only the end-client may cache this (not intermediate proxies). |
| `public` | Any intermediary may cache this. |

Correct `Cache-Control` usage on your API responses enables CDNs and client caches to reduce load on your server automatically.

---

### Raw Request/Response Examples

#### GET — Retrieve a single product

**Request:**

```http
GET /products/42 HTTP/1.1
Host: api.example.com
Authorization: Bearer eyJhbGciOiJIUz...
Accept: application/json
```

**Response:**

```http
HTTP/1.1 200 OK
Content-Type: application/json
Cache-Control: private, max-age=60
Content-Length: 78

{
  "id": 42,
  "name": "Widget Standard",
  "price": 19.99,
  "categoryId": 2
}
```

#### POST — Create a new product

**Request:**

```http
POST /products HTTP/1.1
Host: api.example.com
Authorization: Bearer eyJhbGciOiJIUz...
Content-Type: application/json
Accept: application/json
Content-Length: 58

{
  "name": "Widget Pro",
  "price": 29.99,
  "categoryId": 3
}
```

**Response:**

```http
HTTP/1.1 201 Created
Content-Type: application/json
Location: /products/43
Content-Length: 62

{
  "id": 43,
  "name": "Widget Pro",
  "price": 29.99,
  "categoryId": 3
}
```

#### DELETE — Remove a product

**Request:**

```http
DELETE /products/42 HTTP/1.1
Host: api.example.com
Authorization: Bearer eyJhbGciOiJIUz...
```

**Response:**

```http
HTTP/1.1 204 No Content
```

---

## Summary

- An HTTP request consists of a **request line** (method, path, HTTP version), **headers**, and an optional **body**.
- An HTTP response consists of a **status line** (version, status code, reason phrase), **headers**, and an optional **body**.
- `Content-Type` describes how to parse the body (request and response).
- `Authorization` carries the credential token. `Bearer` is the REST API standard.
- `Accept` lets the client express body format preferences. ASP.NET Core uses this for content negotiation.
- `Cache-Control` governs response caching by clients and intermediaries.

---

## Additional Resources

- [MDN — HTTP headers](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers)
- [MDN — HTTP messages](https://developer.mozilla.org/en-US/docs/Web/HTTP/Messages)
- [RFC 9110 — HTTP Semantics (Fields)](https://www.rfc-editor.org/rfc/rfc9110#name-fields)
