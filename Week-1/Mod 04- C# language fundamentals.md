# Module 4: C# Language Fundamentals

---

## 4.1 C# Basics

**Top-level statements** (modern C#) — the simplest way to write a program:

```csharp
Console.WriteLine("Hello, World!");
```

**Traditional structure** (explicit Main method):

```csharp
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
    }
}
```

**Rules:** Statements end with `;`. Code blocks use `{ }`. C# is **case-sensitive**.

---

## 4.2 Common Built-in Data Types

| Type | Description | Example | Size |
|------|-------------|---------|------|
| `int` | Whole numbers | `42` | 4 bytes |
| `long` | Large whole numbers | `9999999999L` | 8 bytes |
| `double` | Decimals (default) | `3.14` | 8 bytes |
| `float` | Decimals (less precise) | `3.14f` | 4 bytes |
| `decimal` | Financial/precise | `19.99m` | 16 bytes |
| `bool` | True / false | `true` | 1 byte |
| `char` | Single character | `'A'` | 2 bytes |
| `string` | Text | `"Hello"` | varies |

```csharp
// Explicit typing
int age = 30;
double price = 9.99;
string name = "Alice";
bool isActive = true;

// Type inference with var
var count = 10;        // int
var message = "Hi";    // string
```

**Rule of thumb:** `int` for whole numbers, `double` for general decimals, `decimal` for money.

---

## 4.3 More Data Types

### Extended Numeric Types

| Type | Range | Use Case |
|------|-------|----------|
| `byte` | 0–255 | Small positive numbers |
| `short` | ±32,767 | Rarely used |
| `int` | ±2.1 billion | Default for whole numbers |
| `long` | ±9.2 quintillion | Large IDs |
| `decimal` | 28–29 digits | Financial calculations |

### DateTime and TimeSpan

```csharp
DateTime now = DateTime.Now;
DateTime birthday = new DateTime(1995, 6, 15);

// Formatting
Console.WriteLine(now.ToString("yyyy-MM-dd"));    // 2026-04-03
Console.WriteLine(now.ToString("dddd, MMMM dd")); // Thursday, April 03

// Arithmetic
DateTime future = now.AddDays(30);
TimeSpan duration = future - now;
Console.WriteLine(duration.Days);  // 30

// .NET 6+ types
DateOnly today = DateOnly.FromDateTime(DateTime.Now);
TimeOnly time = TimeOnly.FromDateTime(DateTime.Now);
```

---

## 4.4 Basic Operators

### Arithmetic

```csharp
int a = 10, b = 3;
a + b    // 13
a - b    // 7
a * b    // 30
a / b    // 3  ← integer division truncates!
a % b    // 1  ← remainder
```

> For decimal results: `10.0 / 3` or `(double)10 / 3`

### Assignment

`+=`, `-=`, `*=`, `/=`, `%=`, `++`, `--`

### Comparison (return `bool`)

`==`, `!=`, `>`, `<`, `>=`, `<=`

---

## 4.5 Input & Output

### Output

```csharp
Console.WriteLine("With newline");
Console.Write("Without newline: ");

// String interpolation
string name = "Alice";
Console.WriteLine($"Hello, {name}!");

// Number formatting
double price = 19.999;
Console.WriteLine($"{price:F2}");   // 20.00
Console.WriteLine($"{price:C}");    // $20.00
```

### Input

```csharp
Console.Write("Name: ");
string name = Console.ReadLine();    // Always returns string

// Parsing numbers
int age = int.Parse(Console.ReadLine());          // Throws on invalid input

// Safe parsing (preferred)
if (int.TryParse(Console.ReadLine(), out int num))
    Console.WriteLine($"Got: {num}");
else
    Console.WriteLine("Invalid number.");
```

> Always prefer `TryParse` over `Parse` — it won't crash on bad input.

---

## 4.6 Comments

```csharp
// Single-line comment

/*
   Multi-line
   comment
*/

/// <summary>
/// XML doc comment — appears in IntelliSense
/// </summary>
```

**Tip:** Comment *why*, not *what*. The code should be readable enough to show "what."

**VS Code:** `Ctrl+/` for line comments, `Shift+Alt+A` for block comments.

---

## 4.7 Expressions, Statements, and Variables

**Expression** — evaluates to a value: `5 + 3`, `x > 10`, `"Hello " + name`

**Statement** — performs an action, ends with `;`: `int x = 5;`, `Console.WriteLine("Hi");`

**Variable rules:** Must start with letter or `_`. Can contain letters, digits, `_`. Case-sensitive. Can't be a C# keyword.

```csharp
int myAge = 25;
string _firstName = "Alice";
const double Pi = 3.14159;   // Constant — cannot be reassigned
```

---

## 4.8 Basic Type Conversions

### Implicit (safe, automatic)

```csharp
int i = 100;
long l = i;       // int → long (automatic)
double d = i;     // int → double (automatic)
```

### Explicit (casting — risk of data loss)

```csharp
double d = 9.78;
int i = (int)d;   // 9 — truncated, NOT rounded
```

### Parsing

```csharp
int n = int.Parse("42");                       // Throws if invalid
bool ok = int.TryParse("42", out int result);  // Returns false if invalid
int x = Convert.ToInt32("123");                // Convert class
string s = 42.ToString();                      // Any type → string
```

---

## 4.9 Selection and Iteration

### if / else if / else

```csharp
if (score >= 90)
    Console.WriteLine("A");
else if (score >= 80)
    Console.WriteLine("B");
else
    Console.WriteLine("F");
```

### switch

```csharp
switch (day)
{
    case 1: Console.WriteLine("Monday"); break;
    case 6:
    case 7: Console.WriteLine("Weekend!"); break;
    default: Console.WriteLine("Invalid"); break;
}
```

### for

```csharp
for (int i = 0; i < 5; i++)
    Console.WriteLine(i);
```

### while

```csharp
int count = 0;
while (count < 5)
{
    Console.WriteLine(count);
    count++;
}
```

### do...while (runs at least once)

```csharp
string input;
do
{
    Console.Write("Enter 'quit': ");
    input = Console.ReadLine();
} while (input != "quit");
```

### foreach

```csharp
string[] fruits = { "Apple", "Banana", "Cherry" };
foreach (string fruit in fruits)
    Console.WriteLine(fruit);
```

---

## 4.10 Break and Continue

```csharp
// break — exit loop immediately
for (int i = 0; i < 10; i++)
{
    if (i == 5) break;
    Console.WriteLine(i);     // 0, 1, 2, 3, 4
}

// continue — skip to next iteration
for (int i = 0; i < 10; i++)
{
    if (i % 2 == 0) continue; // Skip evens
    Console.WriteLine(i);     // 1, 3, 5, 7, 9
}
```

---

## 4.11 Conditional Operators

```csharp
// && (AND) — both must be true
if (age >= 18 && hasLicense)
    Console.WriteLine("Can drive.");

// || (OR) — at least one true
if (day == "Sat" || day == "Sun")
    Console.WriteLine("Weekend!");

// ! (NOT) — inverts
if (!isLoggedIn)
    Console.WriteLine("Please log in.");

// Ternary — compact if/else returning a value
string status = age >= 18 ? "Adult" : "Minor";
```

**Short-circuit evaluation:** `&&` and `||` skip the second operand if the first determines the result.

```csharp
// Safe — if name is null, .Length is never evaluated
if (name != null && name.Length > 0)
    Console.WriteLine(name);
```

---

## 4.12 The Main Method

**Traditional (explicit):**

```csharp
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello!");
        if (args.Length > 0)
            Console.WriteLine($"Arg: {args[0]}");
    }
}
```

**Top-level statements (modern):** The compiler generates `Main` for you. The `args` variable is available automatically.

```csharp
Console.WriteLine("Hello!");
if (args.Length > 0)
    Console.WriteLine($"Arg: {args[0]}");
```

```bash
dotnet run -- hello world    # args[0]="hello", args[1]="world"
```

---

## 4.13 Value Types, Reference Types, Stack & Heap

### Value Types (stored on stack)

Assignment **copies** the value. Types: `int`, `double`, `bool`, `char`, `decimal`, `struct`, `enum`.

```csharp
int a = 10;
int b = a;    // b is a COPY
b = 20;
Console.WriteLine(a);  // 10 — unchanged
```

### Reference Types (stored on heap)

Assignment copies the **reference**, not the object. Types: `string`, `object`, arrays, classes, `List<T>`.

```csharp
int[] arr1 = { 1, 2, 3 };
int[] arr2 = arr1;        // Points to SAME array
arr2[0] = 99;
Console.WriteLine(arr1[0]);  // 99 — both see the change
```

### Memory Layout

```
STACK                           HEAP
┌─────────────────┐
│ a = 10          │  (value — stored directly)
│ b = 20          │  (independent copy)
│ arr1 = ref ─────────────→ [1, 2, 3]
│ arr2 = ref ─────────────→ (same object!)
└─────────────────┘
```

> **Strings** are reference types but behave like value types because they're **immutable** — any change creates a new string.

---

## Key Takeaways

- C# is **statically typed** — every variable has a type at compile time
- Use `TryParse` instead of `Parse` for safe input handling
- **Control flow:** if/else, switch, for, while, do...while, foreach
- `break` exits a loop; `continue` skips to the next iteration
- **Value types** (stack) are copied on assignment; **reference types** (heap) share the object
- Use `var` when the type is obvious; use explicit types for clarity
