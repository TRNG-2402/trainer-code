# Module 6: Strings & Arrays

---

## 6.1 Strings

Strings in C# are **immutable** — any "modification" creates a new string.

### Creating Strings

```csharp
string greeting = "Hello, World!";
string empty = string.Empty;            // Preferred over ""
string message = $"Name: {name}, Age: {age}";  // Interpolation
```

### Common Operations

```csharp
string text = "  Hello, World!  ";

text.Length                      // 17
text.Trim()                     // "Hello, World!"
text.ToUpper()                  // "  HELLO, WORLD!  "
text.ToLower()                  // "  hello, world!  "
text.Contains("World")          // true
text.StartsWith("  Hello")      // true
text.IndexOf("World")           // 9
text.Trim().Substring(0, 5)     // "Hello"
text.Replace("World", "C#")     // "  Hello, C#!  "
```

### Split and Join

```csharp
string csv = "apple,banana,cherry";
string[] fruits = csv.Split(',');
// fruits[0]="apple", fruits[1]="banana", fruits[2]="cherry"

string joined = string.Join(" | ", fruits);
// "apple | banana | cherry"
```

### Special String Syntax

```csharp
// Verbatim — ignores escape characters (great for paths)
string path = @"C:\Users\Alice\file.txt";

// Raw string literal (C# 11+)
string json = """
    {
        "name": "Alice",
        "age": 30
    }
    """;
```

### Escape Characters

| Sequence | Meaning |
|----------|---------|
| `\n` | Newline |
| `\t` | Tab |
| `\\` | Backslash |
| `\"` | Double quote |

---

## 6.2 Arrays

An **array** is a fixed-size, ordered collection of elements of the same type.

### Declaration

```csharp
int[] numbers = { 1, 2, 3, 4, 5 };
int[] scores = new int[5];             // Size 5, default values (0)
string[] names = new string[] { "Alice", "Bob" };
```

### Accessing Elements (zero-indexed)

```csharp
Console.WriteLine(numbers[0]);     // 1 (first)
Console.WriteLine(numbers[4]);     // 5 (last)
Console.WriteLine(numbers.Length); // 5

numbers[2] = 99;                  // Update element
```

### Iterating

```csharp
// for — when you need the index
for (int i = 0; i < numbers.Length; i++)
    Console.WriteLine($"Index {i}: {numbers[i]}");

// foreach — when you just need values
foreach (int num in numbers)
    Console.WriteLine(num);
```

### Multi-Dimensional Arrays

```csharp
int[,] matrix = {
    { 1, 2, 3 },
    { 4, 5, 6 }
};
Console.WriteLine(matrix[1, 0]);  // 4 (row 1, col 0)
```

### Useful Array Methods

```csharp
Array.Sort(numbers);              // Sort in place
Array.Reverse(numbers);           // Reverse in place
int index = Array.IndexOf(numbers, 3);  // Find element
Array.Clear(numbers, 0, numbers.Length); // Reset to defaults
```

> **Key limitation:** Arrays have a **fixed size** — you can't add or remove elements. For dynamic collections, use `List<T>` (Module 9).

---

## Key Takeaways

- Strings are **immutable** — operations return new strings
- Use **string interpolation** (`$"Hello, {name}"`) for readable formatting
- `Split()` and `Join()` convert between strings and arrays
- Arrays are **fixed-size** and **zero-indexed**
- Use `for` when you need the index, `foreach` when you just need values