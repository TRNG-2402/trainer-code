# Module 13: Advanced C# Features

---

## 13.1 Structs

A **struct** is a **value type** (stored on the stack). Use for small, lightweight data.

```csharp
public struct Point
{
    public double X { get; }
    public double Y { get; }

    public Point(double x, double y) { X = x; Y = y; }

    public double DistanceTo(Point other)
    {
        double dx = X - other.X, dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}

var p1 = new Point(0, 0);
var p2 = new Point(3, 4);
Console.WriteLine(p1.DistanceTo(p2));  // 5
```

**Struct vs. Class:** Structs are value types (copied on assignment). Classes are reference types (reference copied). Use structs for small, frequently created data (coordinates, colors).

---

## 13.2 Enums

Named constants grouped into a type:

```csharp
public enum OrderStatus
{
    Pending,      // 0
    Processing,   // 1
    Shipped,      // 2
    Delivered,    // 3
    Cancelled     // 4
}

OrderStatus status = OrderStatus.Shipped;

if (status == OrderStatus.Shipped)
    Console.WriteLine("On its way!");

// Conversions
string text = status.ToString();                    // "Shipped"
OrderStatus parsed = Enum.Parse<OrderStatus>("Delivered");
int value = (int)status;                            // 2
OrderStatus fromInt = (OrderStatus)3;               // Delivered
```

### Custom Values

```csharp
public enum HttpCode { Ok = 200, NotFound = 404, ServerError = 500 }
```

### Flags Enum (bitwise combinations)

```csharp
[Flags]
public enum Permissions
{
    None = 0, Read = 1, Write = 2, Execute = 4,
    All = Read | Write | Execute
}

Permissions p = Permissions.Read | Permissions.Write;
bool canWrite = p.HasFlag(Permissions.Write);  // true
```

---

## 13.3 Generics

Write type-safe code that works with **any type**:

### Generic Method

```csharp
static T Max<T>(T a, T b) where T : IComparable<T>
{
    return a.CompareTo(b) > 0 ? a : b;
}

Max(10, 20);           // 20
Max("apple", "zoo");   // "zoo"
```

### Generic Class

```csharp
public class Repository<T>
{
    private readonly List<T> _items = new();
    public void Add(T item) => _items.Add(item);
    public IEnumerable<T> GetAll() => _items;
}

var repo = new Repository<string>();
repo.Add("Hello");
```

### Constraints

```csharp
where T : class            // Reference type only
where T : struct           // Value type only
where T : new()            // Must have parameterless constructor
where T : IComparable<T>   // Must implement interface
where T : BaseClass        // Must inherit from class
```

---

## 13.4 Lambda Expressions

Short, inline anonymous functions:

```csharp
// Expression lambda
Func<int, int> square = x => x * x;
Console.WriteLine(square(5));  // 25

// Statement lambda (with body)
Func<int, int, int> add = (a, b) =>
{
    return a + b;
};

// Action (no return value)
Action<string> greet = name => Console.WriteLine($"Hello, {name}!");
```

### With Collections

```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5, 6 };

var evens = numbers.Where(n => n % 2 == 0).ToList();      // { 2, 4, 6 }
var doubled = numbers.Select(n => n * 2).ToList();          // { 2, 4, 6, 8, 10, 12 }
int sum = numbers.Aggregate(0, (acc, n) => acc + n);        // 21
```

---

## 13.5 LINQ

**Language-Integrated Query** — a consistent way to query any data source.

```csharp
var people = new List<Person>
{
    new() { Name = "Alice", Age = 30 },
    new() { Name = "Bob", Age = 25 },
    new() { Name = "Charlie", Age = 35 },
    new() { Name = "Diana", Age = 28 }
};
```

### Filtering and Transforming

```csharp
var adults = people.Where(p => p.Age >= 30);
var names = people.Select(p => p.Name);
var sorted = people.OrderBy(p => p.Age);
var sortedDesc = people.OrderByDescending(p => p.Age);
```

### Aggregation

```csharp
int totalAge = people.Sum(p => p.Age);
double avgAge = people.Average(p => p.Age);
int maxAge = people.Max(p => p.Age);
int count = people.Count(p => p.Age > 25);
```

### Element Access

```csharp
var first = people.First(p => p.Age > 25);
var firstOrNull = people.FirstOrDefault(p => p.Age > 100);  // null
```

### Existence Checks

```csharp
bool anyOver30 = people.Any(p => p.Age > 30);    // true
bool allOver20 = people.All(p => p.Age > 20);    // true
```

### Chaining

```csharp
var result = people
    .Where(p => p.Age >= 25)
    .OrderBy(p => p.Name)
    .Select(p => $"{p.Name} ({p.Age})")
    .ToList();
```

### Query Syntax (alternative, SQL-like)

```csharp
var result = from p in people
             where p.Age >= 30
             orderby p.Name
             select p.Name;
```

---

## 13.6 Pattern Matching

### Type Pattern

```csharp
object obj = "Hello";
if (obj is string s)
    Console.WriteLine(s.ToUpper());
```

### Switch Expression

```csharp
string Classify(int n) => n switch
{
    < 0            => "Negative",
    0              => "Zero",
    > 0 and <= 10  => "Small positive",
    _              => "Large positive"
};
```

### Property Pattern

```csharp
string Describe(Person p) => p switch
{
    { Age: < 18 }                    => "Minor",
    { Age: >= 18, Name: "Alice" }    => "Alice the adult",
    { Age: >= 65 }                   => "Senior",
    _                                => "Adult"
};
```

---

## 13.7 Nullable Reference Types

With `<Nullable>enable</Nullable>` (default in modern projects), reference types are non-nullable by default:

```csharp
string name = "Alice";     // Cannot be null
string? nickname = null;    // Explicitly nullable

// Compiler warns if you access nullable without checking
Console.WriteLine(nickname.Length);  // Warning!

// Safe access
if (nickname != null)
    Console.WriteLine(nickname.Length);
```

---

## 13.8 Null-Safe Operators

### Null-Conditional (`?.`)

Short-circuits to `null` if the left side is null:

```csharp
string? name = null;
int? length = name?.Length;              // null (no crash)
string? city = person?.Address?.City;    // null if any part is null
```

### Null-Coalescing (`??`)

Provides a default when null:

```csharp
string result = input ?? "default";
string city = person?.Address?.City ?? "Unknown";
```

### Null-Coalescing Assignment (`??=`)

```csharp
List<string>? items = null;
items ??= new List<string>();   // Assigns only if null
```

---

## 13.9 Advanced Type Conversions

### `is` and `as`

```csharp
object obj = "Hello";

// 'is' with pattern (preferred)
if (obj is string s)
    Console.WriteLine(s.Length);

// 'as' — returns null if wrong type
string? str = obj as string;
```

### User-Defined Conversions

```csharp
public struct Celsius
{
    public double Value { get; }
    public Celsius(double v) { Value = v; }

    public static implicit operator double(Celsius c) => c.Value;
    public static explicit operator Celsius(double d) => new Celsius(d);
}

Celsius temp = new(100);
double d = temp;              // Implicit
Celsius c = (Celsius)37.5;   // Explicit (requires cast)
```

---

## 13.10 Regular Expressions

```csharp
using System.Text.RegularExpressions;

string input = "Contact support@example.com or sales@example.com";

// Check match
bool has = Regex.IsMatch(input, @"[\w.]+@[\w.]+\.\w+");  // true

// Find first
Match m = Regex.Match(input, @"[\w.]+@[\w.]+\.\w+");
Console.WriteLine(m.Value);  // "support@example.com"

// Find all
foreach (Match match in Regex.Matches(input, @"[\w.]+@[\w.]+\.\w+"))
    Console.WriteLine(match.Value);

// Replace
string clean = Regex.Replace(input, @"[\w.]+@[\w.]+\.\w+", "[REDACTED]");

// Validate phone
bool valid = Regex.IsMatch("555-123-4567", @"^\d{3}-\d{3}-\d{4}$");
```

### Common Patterns

| Pattern | Meaning |
|---------|---------|
| `\d` | Digit |
| `\w` | Word character (letter, digit, `_`) |
| `\s` | Whitespace |
| `.` | Any character |
| `+` | One or more |
| `*` | Zero or more |
| `?` | Zero or one |
| `^...$` | Start/end of string |
| `[a-z]` | Character range |
| `(group)` | Capture group |

---

## Key Takeaways

- **Structs** are value types — use for small, lightweight data
- **Enums** give named constants with type safety
- **Generics** (`<T>`) enable type-safe, reusable code
- **Lambdas** (`x => x * 2`) are concise inline functions
- **LINQ** provides powerful collection querying (Where, Select, OrderBy, Sum, etc.)
- **Pattern matching** makes type checks and conditional logic concise
- **`?.`** and **`??`** protect against null safely
- **Regex** handles string pattern matching and validation