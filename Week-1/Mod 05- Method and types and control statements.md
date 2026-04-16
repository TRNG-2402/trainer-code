# Module 5: Methods, Types & Conventions

---

## 5.1 Methods

A **method** is a reusable block of code that performs a task.

```csharp
static int Add(int a, int b)
{
    return a + b;
}

int result = Add(5, 3);  // 8
```

**void methods** don't return a value:

```csharp
static void Greet(string name)
{
    Console.WriteLine($"Hello, {name}!");
}
```

### Local Functions (with top-level statements)

```csharp
// Call the function
string msg = BuildGreeting("Alice", 30);
Console.WriteLine(msg);

// Define it below
string BuildGreeting(string name, int age)
{
    return $"Hello, {name}! You are {age}.";
}
```

### Optional and Named Parameters

```csharp
static void Print(string message, int times = 1)
{
    for (int i = 0; i < times; i++)
        Console.WriteLine(message);
}

Print("Hello");              // 1 time (default)
Print("Hello", 3);           // 3 times
Print(times: 2, message: "Hi");  // Named — order doesn't matter
```

---

## 5.2 Method Overloading

Multiple methods with the **same name** but **different parameter lists**:

```csharp
static int Add(int a, int b) => a + b;
static double Add(double a, double b) => a + b;
static int Add(int a, int b, int c) => a + b + c;

Add(2, 3);       // calls int version → 5
Add(2.5, 3.5);   // calls double version → 6.0
Add(1, 2, 3);    // calls three-param version → 6
```

**Rule:** Overloads must differ in number or types of parameters. Return type alone is not enough.

---

## 5.3 Static Methods and Code Reuse

**Static members** belong to the class, not to instances:

```csharp
public class MathUtils
{
    public static int Square(int n) => n * n;
}

int result = MathUtils.Square(5);  // 25 — no instance needed
```

**Static classes** can't be instantiated:

```csharp
public static class StringHelper
{
    public static string Capitalize(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToUpper(input[0]) + input.Substring(1).ToLower();
    }
}
```

**Use static for:** utility/helper methods, factory methods, constants.

**Avoid when:** the method needs instance data, or you need testability (static methods are hard to mock).

---

## 5.4 Coding Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Local variables | camelCase | `int itemCount` |
| Parameters | camelCase | `string firstName` |
| Methods | PascalCase | `void CalculateTotal()` |
| Classes | PascalCase | `class OrderService` |
| Interfaces | `I` + PascalCase | `interface IRepository` |
| Constants | PascalCase | `const int MaxRetries` |
| Private fields | `_camelCase` | `private int _count` |
| Properties | PascalCase | `public string Name { get; set; }` |

**Guidelines:**

- Braces on their own line (Allman style)
- One class per file, file name = class name
- Use meaningful names (`customerAge` not `ca`)
- Avoid magic numbers — use named constants

```csharp
// BAD
if (attempts > 3) { ... }

// GOOD
const int MaxAttempts = 3;
if (attempts > MaxAttempts) { ... }
```

**VS Code:** `Shift+Alt+F` to auto-format.

---

## 5.5 Implicit Typing (var)

`var` lets the compiler infer the type:

```csharp
var count = 10;                     // int
var name = "Alice";                 // string
var prices = new List<double>();    // List<double>
```

`var` is **not dynamic** — the type is fixed at compile time.

**Use `var` when** the type is obvious. **Use explicit types when** clarity matters:

```csharp
var name = "Alice";                        // Clearly a string — var is fine
decimal total = CalculateTotal(order);     // Explicit makes the type clear
```

---

## Key Takeaways

- Methods are reusable blocks with parameters and return types
- **Overloading** = same name, different parameter lists
- **Static methods** belong to the class — call with `ClassName.Method()`
- Follow C# naming conventions consistently (PascalCase for methods/classes, camelCase for variables)
- `var` is type inference, not dynamic typing