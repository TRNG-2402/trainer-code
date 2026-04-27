# HTTP Review

## Learning Objectives
- Recall the purpose of each HTTP method
- Read and interpret common HTTP status codes
- Connect HTTP mechanics to the front-end code you will write this week

## Why This Matters
This week you will use the Fetch API and Axios to communicate with your ASP.NET Core API. Every request you fire uses one of these methods; every response carries one of these status codes. Recognizing them at a glance speeds up debugging.

> **Note:** HTTP was covered in depth in Week 1 (Friday). This is a quick reference only.

---

## What Is HTTP?

HTTP (HyperText Transfer Protocol) is the request-response protocol that browsers and servers use to communicate. Every time you call `fetch()` or `axios.get()`, you send an HTTP request and receive an HTTP response.

```
Client (browser / React app)
  --[HTTP Request]-->  Server (ASP.NET Core API)
  <--[HTTP Response]--
```

---

## HTTP Methods

| Method | Meaning | Safe? | Idempotent? | Typical use |
|--------|---------|-------|-------------|-------------|
| `GET` | Retrieve a resource | Yes | Yes | Fetch a list or single item |
| `POST` | Create a new resource | No | No | Submit a form, create an entity |
| `PUT` | Replace a resource entirely | No | Yes | Full update of an entity |
| `PATCH` | Partially update a resource | No | No | Update specific fields |
| `DELETE` | Remove a resource | No | Yes | Delete an entity |
| `OPTIONS` | Ask what methods a server allows | Yes | Yes | Sent automatically in CORS preflight |
| `HEAD` | Like GET but response body is empty | Yes | Yes | Check if a resource exists |

**Safe** = does not change server state.
**Idempotent** = calling it multiple times produces the same result as calling it once.

---

## HTTP Status Codes

### 1xx -- Informational
Rare in practice. The request was received and processing continues.

### 2xx -- Success

| Code | Name | Meaning |
|------|------|---------|
| `200` | OK | Request succeeded; response body contains the result |
| `201` | Created | Resource was created; `Location` header points to the new resource |
| `204` | No Content | Success, but no body to return (common for DELETE and PUT) |

### 3xx -- Redirection

| Code | Name | Meaning |
|------|------|---------|
| `301` | Moved Permanently | Resource has a new permanent URL |
| `302` | Found | Temporary redirect |
| `304` | Not Modified | Cached version is still valid (no body sent) |

### 4xx -- Client Errors

| Code | Name | Meaning |
|------|------|---------|
| `400` | Bad Request | Malformed syntax or invalid input (e.g., validation failure) |
| `401` | Unauthorized | Not authenticated; must log in first |
| `403` | Forbidden | Authenticated but not authorized for this action |
| `404` | Not Found | Resource does not exist |
| `405` | Method Not Allowed | HTTP method not supported for this endpoint |
| `409` | Conflict | State conflict (e.g., duplicate email on registration) |
| `422` | Unprocessable Entity | Semantically invalid (e.g., business rule violation) |
| `429` | Too Many Requests | Rate limit exceeded |

### 5xx -- Server Errors

| Code | Name | Meaning |
|------|------|---------|
| `500` | Internal Server Error | Unhandled exception on the server |
| `502` | Bad Gateway | Upstream server returned an invalid response |
| `503` | Service Unavailable | Server is down or overloaded |

---

## Quick Mapping: REST + HTTP

```
GET    /products         --> 200 OK + list
GET    /products/42      --> 200 OK + single item  (404 if not found)
POST   /products         --> 201 Created + new item
PUT    /products/42      --> 200 OK or 204 No Content
DELETE /products/42      --> 204 No Content         (404 if not found)
POST   /auth/login       --> 200 OK + token         (401 if bad credentials)
```

---

## Summary
- HTTP is request-response: client sends method + URL + optional body; server replies with status code + optional body.
- `GET` / `DELETE` carry no request body. `POST` / `PUT` / `PATCH` usually do.
- 2xx = success, 4xx = your mistake, 5xx = server mistake.
- This week you will see these codes in browser DevTools Network tab constantly.

## Additional Resources
- [MDN: HTTP request methods](https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods)
- [MDN: HTTP response status codes](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status)
- [HTTP Status Dogs](https://httpstatusdogs.com) (lighthearted quick-reference)
