# Service-Oriented Architecture

## Learning Objectives

- Define Service-Oriented Architecture (SOA) and articulate its core design philosophy.
- Contrast an SOA approach with a traditional monolithic architecture.
- Explain how REST is one expression of SOA principles in HTTP-based systems.
- Identify the trade-offs that motivate an organization to adopt SOA.

---

## Why This Matters

This week's Epic is **"From Zero to Data Layer."** You have spent Wednesday and Thursday building the persistence layer of a .NET application — EF Core, entities, migrations, CRUD. Before building the web-facing surface of that application (ASP.NET Core controllers, which arrive in Week 2), you need a shared vocabulary for *how services talk to each other* and *why they are structured that way*.

SOA is that vocabulary. Nearly every enterprise .NET system you encounter at Infosys will be decomposed into services — whether that is a classic WCF-era SOA stack or a modern REST/HTTP microservice landscape. Understanding SOA gives you the conceptual map before you start filling it in.

---

## The Concept

### What Is a Service?

A **service** is a discrete, self-contained unit of functionality that:

- Exposes a well-defined **interface** (the contract).
- Hides its internal implementation behind that contract.
- Can be consumed by any caller that speaks the agreed protocol, regardless of the caller's technology stack.

Think of a service as a vending machine. You interact with the buttons and the slot (the interface). You do not need to know how the internal dispensing mechanism works.

### Service-Oriented Architecture Defined

**SOA** is an architectural style where an application's functionality is composed of loosely-coupled, interoperable services that communicate over a network. The key properties of a well-designed SOA system are:

| Property | Meaning |
|---|---|
| **Loose coupling** | Services do not need knowledge of each other's internals. A change to Service A's implementation should not break Service B. |
| **Service contract** | Every service publishes a formal description of what it accepts and what it returns. |
| **Abstraction** | Implementation details are hidden from consumers. |
| **Reusability** | The same service can be consumed by multiple different clients or other services. |
| **Composability** | Complex business processes are built by orchestrating simpler services together. |
| **Discoverability** | Services can be located and understood without manual coordination (often via a registry or documentation). |

### SOA vs. Monolith

A **monolithic** application packages all functionality into a single deployable unit. The UI, business logic, and data access all run in the same process.

```
Monolith
┌─────────────────────────────────┐
│  UI Layer                       │
│  Business Logic Layer           │
│  Data Access Layer  (EF Core)   │
│  Database                       │
└─────────────────────────────────┘
        Single deployable unit
```

A **service-oriented** system splits that functionality into independently deployable services.

```
SOA System
┌──────────────┐   HTTP/Message   ┌──────────────┐
│  Order Svc   │ ──────────────── │  Inventory   │
│  (ASP.NET)   │                  │  Service     │
└──────────────┘                  └──────────────┘
       │                                 │
  SQL Database                    NoSQL Store
```

The trade-offs are real:

| Concern | Monolith | SOA / Microservices |
|---|---|---|
| **Deployment complexity** | Low — one artifact | High — many artifacts |
| **Development simplicity** | High early on | Requires service discipline |
| **Independent scaling** | Limited | Each service scales separately |
| **Team autonomy** | Low | Teams own individual services |
| **Failure blast radius** | One bug can take down the whole app | Failures are (ideally) isolated |
| **Debugging** | Simple stack trace | Distributed tracing required |

Neither is universally superior. Many successful systems start as monoliths and extract services as specific areas require independent scaling or team ownership.

### Where REST Fits

SOA is an *architectural philosophy*, not a protocol. Early SOA implementations (2000s) often used **SOAP** (Simple Object Access Protocol) over HTTP — XML-heavy, formally contract-driven via WSDL documents. SOAP remains prevalent in enterprise banking and insurance systems.

**REST** is a lighter-weight, resource-centric style that also runs over HTTP but does not require the formal SOAP envelope. REST has become the dominant style for public-facing and internal HTTP APIs in modern systems.

You will cover REST constraints in depth in the next reading (`intro-to-rest.md`). At this stage, recognize that REST is one answer to the question: *"How should services in an SOA system communicate over HTTP?"*

---

## Code Example

SOA patterns do not require complex code to illustrate — the concept is architectural. However, the following demonstrates the *contract-first* mindset in .NET using an interface:

```csharp
// The contract — any service implementation must honor this shape.
public interface IOrderService
{
    Task<Order> GetOrderAsync(int orderId);
    Task<Order> PlaceOrderAsync(CreateOrderRequest request);
    Task CancelOrderAsync(int orderId);
}

// The concrete implementation is hidden behind the interface.
// Callers depend on IOrderService, not on OrderService directly.
public class OrderService : IOrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Order> GetOrderAsync(int orderId)
    {
        return await _db.Orders.FindAsync(orderId)
               ?? throw new NotFoundException($"Order {orderId} not found.");
    }

    // ... other implementations
}
```

This is the same principle at the code level that SOA applies at the network level: callers depend on a contract, not an implementation.

---

## Summary

- SOA is an architectural style that decomposes application functionality into discrete, loosely-coupled services that communicate over a network.
- Each service exposes a contract (interface) and hides its implementation.
- SOA trades deployment complexity for independent scalability, team autonomy, and fault isolation.
- Monoliths are still a valid starting point; SOA is typically adopted when specific scaling or team-ownership pressures arise.
- REST is one of several protocols used to implement inter-service communication in an SOA system. The next readings cover REST and HTTP in detail.

---

## Additional Resources

- [Microsoft — Service-Oriented Architecture](https://learn.microsoft.com/en-us/azure/architecture/guide/architecture-styles/soa)
- [Martin Fowler — Microservices (foundational essay)](https://martinfowler.com/articles/microservices.html)
- [OASIS SOA Reference Model](https://www.oasis-open.org/committees/tc_home.php?wg_abbrev=soa-rm)
