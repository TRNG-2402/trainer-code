# HTTP Request/Response Lifecycle

## Learning Objectives

- Trace the full journey of an HTTP request from browser or API client to server and back.
- Explain the roles of DNS, TCP, and TLS at each stage of the lifecycle.
- Describe what "server processing" involves in the context of an ASP.NET Core application.
- Identify where failures can occur in the lifecycle and what they signal diagnostically.

---

## Why This Matters

When a REST API call fails, the error could have occurred at any one of a dozen points: DNS resolution, TCP establishment, TLS handshake, routing, authentication, business logic, the database, or the response serialization. A developer who cannot mentally trace the lifecycle defaults to "the API is broken." A developer who understands the lifecycle can narrow down the failure immediately.

This reading also directly prepares you for Week 2. When you write an ASP.NET Core controller action, you are writing code that runs at the **"server processing"** stage described below. Understanding everything that happened before your code ran — and everything that happens after — is essential for reasoning about middleware, authentication, and error handling.

---

## The Concept

The lifecycle of a single HTTP/HTTPS request spans roughly seven stages.

```
[1] DNS Resolution
      |
[2] TCP Connection
      |
[3] TLS Handshake  (HTTPS only)
      |
[4] HTTP Request Transmission
      |
[5] Server Processing
      |
[6] HTTP Response Transmission
      |
[7] Client Processing
```

---

### Stage 1 — DNS Resolution

Before the client can connect to the server, it must resolve the **hostname** (e.g., `api.example.com`) to an **IP address** (e.g., `104.21.45.12`).

The client checks several sources in order:

1. **Local cache** — Has this hostname been recently resolved? If so, is the record still within its TTL (Time To Live)?
2. **OS hosts file** — (`C:\Windows\System32\drivers\etc\hosts` on Windows). Useful for local development overrides.
3. **Recursive DNS resolver** — Typically provided by your ISP or a public resolver (8.8.8.8, 1.1.1.1). This resolver traverses the DNS hierarchy (root → TLD → authoritative nameserver) to return the IP.

Once the IP is known, DNS resolution is complete for the duration of the TTL. Subsequent requests to the same host within that window skip this step.

**Diagnostic relevance:** `ERR_NAME_NOT_RESOLVED` in a browser, or "No such host" in .NET, indicates DNS failure — the hostname cannot be resolved. This is a network infrastructure issue, not an API issue.

---

### Stage 2 — TCP Connection

The client opens a TCP connection to the server's IP on port **80** (HTTP) or **443** (HTTPS) using the three-way handshake:

```
Client → Server:  SYN
Server → Client:  SYN-ACK
Client → Server:  ACK
                  ↑ Connection established
```

This round trip adds latency before a single byte of HTTP data is transmitted. HTTP/2 mitigates this by reusing a single TCP connection across multiple concurrent requests (multiplexing), as covered in `intro-to-http.md`.

**Diagnostic relevance:** A `Connection timed out` or `ECONNREFUSED` error means the TCP connection failed. Either the server is not listening on the target port, or a firewall is blocking the connection.

---

### Stage 3 — TLS Handshake (HTTPS Only)

After TCP is established, the client and server negotiate an encrypted channel using TLS:

1. **ClientHello** — Client advertises supported TLS versions and cipher suites.
2. **ServerHello** — Server selects the TLS version and cipher suite, presents its certificate.
3. **Certificate validation** — Client verifies the certificate against a trusted CA chain and checks the hostname matches.
4. **Key exchange** — Client and server derive a shared session key without transmitting it directly.
5. **Finished** — Both sides confirm the handshake. Subsequent data is encrypted with the session key.

Everything from this point forward — URI path, headers, body — is encrypted and cannot be read by intermediaries (ISPs, network proxies in a passive listen position).

**Diagnostic relevance:** `SSL_ERROR_RX_RECORD_TOO_LONG`, `CERTIFICATE_VERIFY_FAILED`, and similar errors indicate TLS failures. In development, using a self-signed certificate without trusting it will produce `CERTIFICATE_UNKNOWN` errors. Run `dotnet dev-certs https --trust` to resolve this in ASP.NET Core development.

---

### Stage 4 — HTTP Request Transmission

The client sends the HTTP request message over the established (and encrypted, for HTTPS) connection. The request is covered in detail in `http-request-response-structure.md`, but at this stage its contents transit the network as TCP segments broken into IP packets.

For small requests (a typical REST `GET`), the entire request fits in a single segment and the one-way transmission time is negligible over a fast network. Large file uploads — a multipart `POST` — require the server to reassemble many segments before the request is complete.

---

### Stage 5 — Server Processing

The server receives the raw bytes, and a chain of components processes the request before your business logic is invoked.

In an ASP.NET Core application, this chain is the **middleware pipeline**:

```
Request
  |
  ├── HTTPS Redirection Middleware
  ├── Static Files Middleware
  ├── Routing Middleware  ← matches URI to a controller action
  ├── Authentication Middleware  ← validates the bearer token / cookie
  ├── Authorization Middleware  ← checks permissions
  ├── [Your Controller Action]  ← business logic + EF Core database call
  |       |
  |       └── Database (SQL Server / SQLite via EF Core)
  |
Response <──────────────────────────────────────────
```

Your controller action is responsible for:

1. Interpreting the request (route params, query params, body deserialization).
2. Calling the appropriate service / repository (which may query EF Core).
3. Constructing and returning the appropriate response (status code + body).

The middleware layers handles authentication, authorization, and routing **before** your action runs.

**Diagnostic relevance:** A `500 Internal Server Error` occurred in Stage 5. An `ECONNRESET` mid-request usually means the server crashed or was restarted while processing. A `408 Request Timeout` means the server received the request but the client did not finish sending it within the timeout window.

---

### Stage 6 — HTTP Response Transmission

The server serializes the response (status line + headers + body) and transmits it back over the same TCP connection. The client receives and reassembles the TCP segments in order.

For large responses (e.g., a collection of thousands of records), ASP.NET Core can stream the response using chunked transfer encoding, allowing the client to begin processing before the full response is received.

---

### Stage 7 — Client Processing

The client (browser, Postman, HttpClient in another .NET service):

1. Reads the status code to determine outcome.
2. Reads response headers (`Content-Type`, `Cache-Control`, `Location`, etc.).
3. Parses and processes the body based on `Content-Type` (typically deserializing JSON).
4. Optionally caches the response per `Cache-Control` directives.

An API client written in .NET uses `System.Net.Http.HttpClient` for this stage:

```csharp
using var client = new HttpClient();
client.BaseAddress = new Uri("https://api.example.com");

HttpResponseMessage response = await client.GetAsync("/products/42");

response.EnsureSuccessStatusCode();    // throws on 4xx/5xx

string json = await response.Content.ReadAsStringAsync();
Product? product = JsonSerializer.Deserialize<Product>(json);
```

---

## Summary

- A single HTTPS request passes through seven stages: DNS resolution, TCP connection, TLS handshake, HTTP request transmission, server processing, HTTP response transmission, and client processing.
- DNS resolution maps a hostname to an IP address. TCP establishes a reliable, ordered connection. TLS adds encryption and authentication.
- Server processing in ASP.NET Core involves the middleware pipeline before and after your controller action.
- Understanding where in the lifecycle an error occurs is the foundation for efficient API debugging.

---

## Additional Resources

- [MDN — A typical HTTP session](https://developer.mozilla.org/en-US/docs/Web/HTTP/Session)
- [Cloudflare — How DNS works](https://www.cloudflare.com/learning/dns/what-is-dns/)
- [Microsoft — ASP.NET Core Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
