# Introduction to HTTP

## Learning Objectives

- Describe HTTP as an application-layer protocol and explain where it sits in the network stack.
- Trace the high-level request/response model of an HTTP exchange.
- Articulate the conceptual role of TCP as the transport for HTTP.
- Identify the primary differences between HTTP/1.1 and HTTP/2.

---

## Why This Matters

Every REST API you build with ASP.NET Core communicates over HTTP. HTTP is not a black box — it is a documented, well-understood protocol with specific rules about how requests and responses are formed, transmitted, and interpreted. Knowing those rules makes you a better API designer: you will understand *why* status codes, headers, and methods matter, not just *that* they exist.

You will also be on-call to diagnose networking and API issues in production. A developer who understands HTTP can read a raw network trace or a Postman request and know immediately what is wrong. A developer who does not is limited to guessing.

---

## The Concept

### The Network Layer Model

Networks are typically modeled in layers. Each layer provides services to the layer above it and depends on the layer below it. HTTP operates at the **Application Layer** — the topmost layer in the model where user-facing protocols live.

```
Application Layer  →  HTTP, HTTPS, FTP, SMTP, DNS
Transport Layer    →  TCP, UDP
Internet Layer     →  IP (IPv4, IPv6)
Link Layer         →  Ethernet, Wi-Fi (hardware)
```

When you make an HTTP request:

1. Your application calls the HTTP library.
2. The HTTP library formats the request according to the HTTP specification.
3. HTTP hands the formatted message to **TCP** at the transport layer.
4. TCP guarantees reliable, ordered delivery of that message across the network.
5. IP routes the TCP segments through the internet to the destination server.
6. The server's TCP layer reassembles the segments and hands the HTTP message up to the application.

You do not normally interact with TCP directly in .NET development. But knowing it exists explains *why* HTTP is reliable (in contrast to UDP, which offers no delivery guarantee and is used for video streaming, DNS, etc.).

### HTTP as a Request/Response Protocol

HTTP follows a strict **request/response** pattern:

1. The **client** initiates. It opens a connection to the server and sends an HTTP request.
2. The **server** responds. It processes the request and sends back exactly one HTTP response.
3. The exchange for that request is complete.

This maps directly to the **stateless** REST constraint you read about in `intro-to-rest.md`. The server responds to the current request and immediately forgets the exchange. The next request must be entirely self-contained.

```
Client                          Server
  │                               │
  │── GET /products/12 ──────────>│
  │                               │  (processes request)
  │<── 200 OK  { ... JSON ... } ──│
  │                               │
  │── DELETE /products/12 ───────>│
  │                               │  (processes request, no memory of GET)
  │<── 204 No Content ────────────│
```

### TCP Connection Basics

Before HTTP can exchange a request and response, a **TCP connection** must be established. TCP uses a **three-way handshake**:

```
Client → Server:  SYN    (I want to connect)
Server → Client:  SYN-ACK (I acknowledge, I also want to connect)
Client → Server:  ACK    (Acknowledged. Connection established.)
```

Only after this handshake can the HTTP request be sent. This is why there is measurable latency on the first request to a server — the handshake takes at least one round trip before any application data moves.

For HTTPS (HTTP over TLS), the TLS handshake adds additional round trips for certificate exchange and cipher negotiation. You will encounter headers like `Authorization` and `Content-Type` in tomorrow's readings — these travel inside the TLS tunnel, meaning they are encrypted in transit.

### HTTP/1.1

HTTP/1.1 (1997, still the baseline) introduced persistent connections (keep-alive) so that the TCP connection does not have to be torn down and re-established after every single request. However, HTTP/1.1 still processes requests **serially** on each connection — a client must wait for a response before sending the next request on the same connection.

The workaround was to open **multiple parallel TCP connections** to the same server (typically 6 per origin in browsers). This is effective but wasteful.

### HTTP/2

HTTP/2 (2015) keeps the same HTTP semantics — same methods, headers, status codes — but changes the transport layer:

| Feature | HTTP/1.1 | HTTP/2 |
|---|---|---|
| **Multiplexing** | No — one request at a time per connection | Yes — multiple requests share a single TCP connection concurrently |
| **Header compression** | No | Yes (HPACK algorithm) |
| **Server push** | No | Yes — server can proactively send resources the client will need |
| **Binary protocol** | No (text) | Yes — more efficient to parse |
| **Connection overhead** | High — multiple TCP connections needed | Low — single connection |

From an ASP.NET Core developer's perspective, HTTP/2 support is enabled by default on Kestrel (the built-in .NET web server) when the client supports it. You do not typically configure it manually. The important point is that **your API code is HTTP-version-agnostic** — the methods, status codes, and headers you write work the same under both versions.

### HTTPS and TLS

**HTTPS** is HTTP carried over a **TLS** (Transport Layer Security) encrypted connection. TLS is not part of HTTP itself — it is a separate protocol that wraps HTTP at the transport layer.

For your ASP.NET Core APIs in development, you will use `https://localhost` with a self-signed development certificate. In production, a certificate issued by a trusted Certificate Authority (CA) is used. ASP.NET Core handles the TLS handshake automatically via Kestrel or IIS.

The practical implications for API design are minimal — you write the same controller code regardless. The operational implication is that **all headers, body content, and URIs are encrypted end-to-end** in an HTTPS exchange.

---

## Summary

- HTTP is an **application-layer protocol** that sits on top of TCP, which provides reliable, ordered delivery.
- HTTP is a **request/response protocol**: one client request produces exactly one server response, after which the server retains no state.
- A TCP three-way handshake must complete before HTTP data can flow, contributing to initial connection latency.
- HTTP/1.1 is the text-based baseline that processes requests serially per connection.
- HTTP/2 introduces **binary multiplexing** — multiple concurrent requests over a single TCP connection — along with header compression and server push.
- HTTPS wraps HTTP in TLS encryption. Your API code is unaffected; TLS is handled at the infrastructure layer.

---

## Additional Resources

- [MDN — HTTP Overview](https://developer.mozilla.org/en-US/docs/Web/HTTP/Overview)
- [MDN — Evolution of HTTP](https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/Evolution_of_HTTP)
- [Cloudflare — What is HTTP/2?](https://www.cloudflare.com/learning/performance/http2-vs-http1.1/)
