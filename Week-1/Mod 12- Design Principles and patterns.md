# Module 12: Design Principles & Patterns

---

## 12.1 SOLID Principles

### S — Single Responsibility Principle (SRP)

A class should have **one reason to change** — it should do one thing well.

```csharp
// BAD — one class does everything
public class Employee
{
    public string Name { get; set; }
    public void SaveToDatabase() { }
    public void GenerateReport() { }
}

// GOOD — separate responsibilities
public class Employee { public string Name { get; set; } }
public class EmployeeRepository { public void Save(Employee e) { } }
public class ReportGenerator { public string Generate(Employee e) { return ""; } }
```

### O — Open/Closed Principle (OCP)

Open for **extension**, closed for **modification**. Add new behavior without changing existing code.

```csharp
// GOOD — new shapes don't require modifying existing code
public abstract class Shape { public abstract double Area(); }
public class Circle : Shape { public override double Area() => Math.PI * R * R; }
public class Triangle : Shape { public override double Area() => 0.5 * B * H; }
// Adding Rectangle = new class, no existing code touched
```

### L — Liskov Substitution Principle (LSP)

Derived classes must be **substitutable** for their base without breaking behavior. If code works with `Animal`, it should work with any `Animal` subclass.

### I — Interface Segregation Principle (ISP)

Don't force classes to implement interfaces they don't use. Prefer small, focused interfaces.

```csharp
// BAD
public interface IWorker { void Work(); void Eat(); void Sleep(); }
// A Robot can't Eat or Sleep!

// GOOD
public interface IWorkable { void Work(); }
public interface IFeedable { void Eat(); }

public class Robot : IWorkable { public void Work() { } }
public class Human : IWorkable, IFeedable { /* both */ }
```

### D — Dependency Inversion Principle (DIP)

Depend on **abstractions** (interfaces), not concrete implementations.

```csharp
// BAD — tightly coupled to SqlDatabase
public class OrderService { private SqlDatabase _db = new SqlDatabase(); }

// GOOD — depends on an interface
public class OrderService
{
    private readonly IOrderRepository _repo;
    public OrderService(IOrderRepository repo) { _repo = repo; }
}
```

---

## 12.2 Dependency Injection (DI)

Provide dependencies **from outside** rather than creating them internally.

```csharp
// Interface
public interface IMessageSender
{
    void Send(string message);
}

// Implementations
public class EmailSender : IMessageSender
{
    public void Send(string msg) => Console.WriteLine($"[EMAIL] {msg}");
}

public class SmsSender : IMessageSender
{
    public void Send(string msg) => Console.WriteLine($"[SMS] {msg}");
}

// Service — doesn't know or care which sender it uses
public class NotificationService
{
    private readonly IMessageSender _sender;

    public NotificationService(IMessageSender sender)  // Injected
    {
        _sender = sender;
    }

    public void Notify(string msg) => _sender.Send(msg);
}

// Usage — swap implementations without changing the service
var emailNotifier = new NotificationService(new EmailSender());
emailNotifier.Notify("Welcome!");

var smsNotifier = new NotificationService(new SmsSender());
smsNotifier.Notify("Welcome!");
```

**Benefits:** Flexibility (swap implementations), testability (inject mocks), separation of concerns.

---

## 12.3 Separation of Concerns (SoC)

Divide your application so each component handles **one aspect**:

```
Models/          ← Data structures (what data looks like)
Services/        ← Business logic (rules and processing)
Repositories/    ← Data access (how data is stored/retrieved)
Program.cs       ← UI / entry point (user interaction)
```

Each layer can change independently — switching from file storage to a database only changes the repository.

---

## 12.4 Singleton Pattern

Ensures a class has **exactly one instance** globally:

```csharp
public class AppSettings
{
    private static readonly Lazy<AppSettings> _instance =
        new(() => new AppSettings());

    public static AppSettings Instance => _instance.Value;

    private AppSettings()   // Private — can't be created externally
    {
        AppName = "MyApp";
    }

    public string AppName { get; set; }
}

// Same instance everywhere
var settings = AppSettings.Instance;
Console.WriteLine(settings.AppName);
```

**Use for:** configuration, logging, connection pools.

**Caution:** Singletons are global state — overuse makes code harder to test and couples components.

---

## 12.5 Factory Method Pattern

Delegate object creation to a factory — callers depend on the interface, not concrete classes:

```csharp
public interface INotification { void Send(string msg); }

public class EmailNotification : INotification
{
    public void Send(string msg) => Console.WriteLine($"[EMAIL] {msg}");
}

public class SmsNotification : INotification
{
    public void Send(string msg) => Console.WriteLine($"[SMS] {msg}");
}

public static class NotificationFactory
{
    public static INotification Create(string type) => type.ToLower() switch
    {
        "email" => new EmailNotification(),
        "sms"   => new SmsNotification(),
        _       => throw new ArgumentException($"Unknown: {type}")
    };
}

// Usage
INotification n = NotificationFactory.Create("email");
n.Send("Hello!");
```

**Benefits:** Adding new types requires only a new class + one line in the factory. No existing code changes.

---

## Key Takeaways

- **SOLID** = five principles for maintainable, extensible code
- **SRP:** One class, one job
- **DIP + DI:** Depend on interfaces, inject implementations from outside
- **Separation of Concerns:** Models / Services / Repositories / UI
- **Singleton:** One instance globally — use sparingly
- **Factory:** Centralize object creation behind an interface