# Module 7: OOP — Classes, Objects & Encapsulation

---

## 7.1 Four Principles of OOP

| Principle | Meaning |
|-----------|---------|
| **Encapsulation** | Bundle data + methods; restrict access to internals |
| **Abstraction** | Expose only what's needed; hide complexity |
| **Inheritance** | A class acquires members from another (Module 8) |
| **Polymorphism** | Different types respond to the same interface differently (Module 8) |

---

## 7.2 Classes and Objects

A **class** is a blueprint. An **object** is an instance of a class.

```csharp
public class Person
{
    private string _name;
    private int _age;

    public Person(string name, int age)
    {
        _name = name;
        _age = age;
    }

    public string Introduce() => $"Hi, I'm {_name}, age {_age}.";
}

// Creating an object
Person alice = new Person("Alice", 30);
Console.WriteLine(alice.Introduce());
```

---

## 7.3 Fields

**Fields** are variables declared in a class that hold the object's state.

```csharp
public class Car
{
    private string _make;          // Convention: _camelCase for private fields
    private double _mileage = 0;   // Can have defaults
}
```

Fields are typically **private** — use properties for controlled access.

---

## 7.4 Instance Methods

Methods that operate on an object's data:

```csharp
public class Car
{
    private double _mileage = 0;

    public void Drive(double miles) => _mileage += miles;
    public string GetStatus() => $"Mileage: {_mileage}";
}

var car = new Car();
car.Drive(150);
Console.WriteLine(car.GetStatus());  // "Mileage: 150"
```

---

## 7.5 Access Modifiers: public and private

- **`public`** — accessible from anywhere
- **`private`** — accessible only within the same class (default for members)

```csharp
public class BankAccount
{
    private decimal _balance;      // Only this class can touch it

    public void Deposit(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Must be positive");
        _balance += amount;
    }

    public decimal GetBalance() => _balance;
}

var account = new BankAccount();
account.Deposit(100);
// account._balance = 999;  // Compiler error — private!
```

---

## 7.6 Properties

Properties provide controlled access to data with a clean syntax:

```csharp
public class Person
{
    // Auto-implemented
    public string Name { get; set; }

    // Read-only
    public int Age { get; }

    // With validation
    private string _email;
    public string Email
    {
        get => _email;
        set
        {
            if (!value.Contains("@")) throw new ArgumentException("Invalid email");
            _email = value;
        }
    }

    // Computed (read-only, no backing field)
    public bool IsAdult => Age >= 18;

    public Person(string name, int age) { Name = name; Age = age; }
}
```

You can restrict accessors: `public string Name { get; private set; }`

---

## 7.7 Object Initializers

Set properties at creation time without a matching constructor:

```csharp
public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
}

var product = new Product
{
    Name = "Laptop",
    Price = 999.99m,
    Category = "Electronics"
};
```

---

## 7.8 Constructors

A **constructor** runs when an object is created:

```csharp
public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }

    // Default constructor
    public Product() { Name = "Unknown"; Price = 0; }

    // Parameterized
    public Product(string name, decimal price) { Name = name; Price = price; }

    // Constructor chaining
    public Product(string name) : this(name, 0) { }
}
```

> If you define any constructor, C# no longer provides a default parameterless one.

---

## 7.9 Encapsulation

Hide internals, expose a controlled interface. Prevents invalid state:

```csharp
public class Temperature
{
    private double _celsius;

    public double Celsius
    {
        get => _celsius;
        set
        {
            if (value < -273.15) throw new ArgumentException("Below absolute zero!");
            _celsius = value;
        }
    }

    public double Fahrenheit => (_celsius * 9 / 5) + 32;
}
```

---

## 7.10 Namespaces and using Directives

**Namespaces** group related classes:

```csharp
// File-scoped namespace (C# 10+)
namespace MyApp.Models;

public class Customer
{
    public string Name { get; set; }
}
```

**Import with `using`:**

```csharp
using MyApp.Models;
```

**Convention:** Namespace matches folder structure.

---

## 7.11 Class Organization

- **One class per file**, file name = class name
- Organize by folder: `Models/`, `Services/`, `Repositories/`
- Within a class: fields → constructors → properties → public methods → private methods

---

## 7.12 Abstract Classes

An **abstract class** can't be instantiated directly. It provides shared behavior and forces derived classes to implement specific methods.

```csharp
public abstract class Shape
{
    public string Color { get; set; }

    public abstract double Area();        // Must be overridden
    public abstract double Perimeter();   // Must be overridden

    public void Describe()                // Shared — inherited as-is
    {
        Console.WriteLine($"{Color} shape, area: {Area():F2}");
    }
}

public class Circle : Shape
{
    public double Radius { get; set; }
    public override double Area() => Math.PI * Radius * Radius;
    public override double Perimeter() => 2 * Math.PI * Radius;
}

// Shape s = new Shape();  // Error — can't instantiate abstract
```

---

## 7.13 Interfaces

An **interface** defines a contract — methods/properties a class must implement.

```csharp
public interface IMovable
{
    void Move(double x, double y);
    double Speed { get; }
}

public interface IDrawable
{
    void Draw();
}

// A class can implement MULTIPLE interfaces
public class Player : IMovable, IDrawable
{
    public double Speed => 5.0;
    public void Move(double x, double y) { /* ... */ }
    public void Draw() { /* ... */ }
}
```

**Interface vs. Abstract Class:**

| | Interface | Abstract Class |
|-|-----------|---------------|
| Multiple | Can implement many | Can inherit one |
| Fields | No | Yes |
| Constructors | No | Yes |
| Use when | "Can do" (capability) | "Is a" (base type) |

**Naming:** Always prefix with `I` — `IRepository`, `ILogger`.

---

## 7.14 Internal Access

**`internal`** — accessible within the same project, not from other projects.

**Full access modifier summary:**

| Modifier | Same Class | Same Project | Derived Class | Everywhere |
|----------|-----------|-------------|---------------|------------|
| `private` | ✓ | | | |
| `internal` | ✓ | ✓ | | |
| `protected` | ✓ | | ✓ | |
| `protected internal` | ✓ | ✓ | ✓ | |
| `public` | ✓ | ✓ | ✓ | ✓ |

---

## Key Takeaways

- A **class** is a blueprint; an **object** is an instance
- Use **private fields** + **public properties** for encapsulation
- **Constructors** initialize objects; chain them with `: this(...)`
- **Abstract classes** provide shared behavior + force overrides
- **Interfaces** define contracts; a class can implement multiple
- Organize: one class per file, namespaces match folders