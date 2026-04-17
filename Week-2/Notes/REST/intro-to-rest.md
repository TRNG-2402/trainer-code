# Introduction to REST

## Learning Objectives

- State the six architectural constraints that define REST.
- Explain what "stateless" means in the context of an HTTP API.
- Describe the Richardson Maturity Model and identify where a given API falls on it.
- Distinguish REST (a style) from HTTP (a protocol) and from SOAP (a competing style).

---

## Why This Matters

ASP.NET Core is the framework you will build controllers in during Week 2. Those controllers will expose **REST APIs**. Before writing a single line of controller code, you need to understand the rules REST places on those APIs — because violating them produces systems that are difficult to consume, cache, and evolve.

REST is also the lingua franca of modern software integration. When you read a job description, an architecture diagram, or a sprint ticket that says "expose a REST endpoint," there is an assumed shared understanding behind that phrase. This reading establishes that shared understanding precisely.

---

## The Concept

### REST Is an Architectural Style, Not a Protocol

**REST** stands for **Representational State Transfer**. It was defined by Roy Fielding in his 2000 doctoral dissertation. REST is an *architectural style* — a set of constraints that, when applied to a system, produce well-defined desirable properties.

REST does not prescribe a wire format. In practice, REST APIs almost always run over HTTP and use JSON as the body format, but neither is technically mandatory under the pure definition.

### The Six HTTP Constraints

Fielding defined six constraints. A system that satisfies all six is described as **RESTful**.

#### 1. Client-Server

The UI (client) and the data storage (server) are separated. The client does not need to know how the server stores data. The server does not need to know how the client renders data.

**Benefit:** Both sides can evolve independently.

#### 2. Stateless

Every request from client to server must contain **all information necessary to understand the request**. The server holds no session state between requests.

```
Request A:  GET /orders/42   (must include auth token — server does not remember you)
Request B:  DELETE /orders/42 (must include auth token — server still does not remember you)
```

**Benefit:** Any server in a load-balanced pool can handle any request. Horizontal scaling is straightforward.

**Common confusion:** The API is stateless; the *application* can still have state (stored in a database). The server just does not store *request session state* in memory between calls.

#### 3. Cacheable

Responses must label themselves as cacheable or non-cacheable. If a response is cacheable, the client (or an intermediate proxy) may reuse it to avoid redundant requests.

HTTP already provides mechanisms for this: `Cache-Control`, `ETag`, `Last-Modified` headers. RESTful APIs must use them correctly.

**Benefit:** Reduced server load and improved client performance.

#### 4. Uniform Interface

This is the central constraint of REST, and it has four sub-constraints:

| Sub-constraint | Meaning |
|---|---|
| **Resource identification in requests** | Resources are identified by URIs (`/orders/42`). The URI identifies the resource, not the action. |
| **Resource manipulation through representations** | Clients manipulate resources through representations (e.g., a JSON body). The representation is distinct from the resource itself. |
| **Self-descriptive messages** | Each message includes enough information for the recipient to process it (Content-Type, method, etc.). |
| **HATEOAS** | Hypermedia As The Engine Of Application State — responses include links to related actions. Rarely fully implemented in practice. |

**Benefit:** Simplifies the overall system architecture. A client that understands HTTP and JSON can consume any REST API.

#### 5. Layered System

The client cannot tell whether it is connected directly to the server or to an intermediary (load balancer, CDN, API gateway, cache proxy). Each layer only knows about the layer immediately adjacent to it.

**Benefit:** Enables infrastructure scaling and security measures that are transparent to clients.

#### 6. Code on Demand (Optional)

Servers may optionally extend client functionality by delivering executable code (e.g., JavaScript returned in a response). This is the only optional constraint and is rarely relevant in pure API contexts.

---

### The Richardson Maturity Model

The **Richardson Maturity Model (RMM)**, described by Martin Fowler, provides a practical scale for measuring how RESTful an HTTP API is. It is not an official part of Fielding's definition but is widely used in industry discussions.

```
Level 0 — The Swamp of POX
Level 1 — Resources
Level 2 — HTTP Verbs
Level 3 — Hypermedia Controls (HATEOAS)
```

#### Level 0 — The Swamp of Plain Old XML

All requests go to a single endpoint. The action is described in the request body (or query parameter), not in the HTTP method or URI.

```
POST /orderService
Body: { "action": "getOrder", "id": 42 }

POST /orderService
Body: { "action": "deleteOrder", "id": 42 }
```

This is how most SOAP-era services behave. HTTP is used purely as a transport.

#### Level 1 — Resources

The API introduces distinct URIs per resource but still uses a single HTTP method (typically POST or GET) for all operations.

```
POST /orders/42          (to read order 42)
POST /orders/42/cancel   (to cancel — action baked into URI)
```

Better than Level 0, but the HTTP method carries no semantic meaning yet.

#### Level 2 — HTTP Verbs

The API uses HTTP methods as intended: `GET` to read, `POST` to create, `PUT`/`PATCH` to update, `DELETE` to remove. Status codes are used correctly.

```
GET    /orders/42       → 200 OK
POST   /orders          → 201 Created
DELETE /orders/42       → 204 No Content
```

This is the level that most production REST APIs operate at, and it is the target you will implement in Week 2 with ASP.NET Core.

#### Level 3 — Hypermedia Controls (HATEOAS)

Responses include links to related actions, allowing clients to discover the API dynamically without out-of-band documentation.

```json
{
  "id": 42,
  "status": "pending",
  "_links": {
    "self":   { "href": "/orders/42", "method": "GET"    },
    "cancel": { "href": "/orders/42", "method": "DELETE" },
    "pay":    { "href": "/orders/42/payment", "method": "POST" }
  }
}
```

Full Level 3 compliance is relatively rare due to client-side complexity. Some APIs (notably GitHub's API) implement partial HATEOAS.

---

## Summary

- REST is a set of six architectural constraints: client-server, stateless, cacheable, uniform interface, layered system, and (optionally) code on demand.
- Statelessness means every request is self-contained — the server stores no session data between requests.
- The uniform interface constraint — especially resource-based URIs and semantic HTTP verbs — is what distinguishes REST from ad-hoc HTTP usage.
- The Richardson Maturity Model provides a practical 0–3 scale for evaluating REST compliance. Level 2 (HTTP Verbs) is the practical industry standard.
- REST is a style; HTTP is the protocol it runs on. The next readings cover HTTP mechanics in detail.

---

## Additional Resources

- [Roy Fielding's Dissertation — Chapter 5 (REST)](https://www.ics.uci.edu/~fielding/pubs/dissertation/rest_arch_style.htm)
- [Martin Fowler — Richardson Maturity Model](https://martinfowler.com/articles/richardsonMaturityModel.html)
- [MDN — HTTP Overview](https://developer.mozilla.org/en-US/docs/Web/HTTP/Overview)
