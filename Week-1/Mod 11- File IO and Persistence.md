# Module 11: File I/O, Serialization & Persistence

---

## 11.1 File I/O

### Simple File Operations

```csharp
// Write (creates or overwrites)
File.WriteAllText("output.txt", "Hello, File I/O!");

// Read entire file
string content = File.ReadAllText("output.txt");

// Write lines
File.WriteAllLines("lines.txt", new[] { "Line 1", "Line 2", "Line 3" });

// Read all lines
string[] lines = File.ReadAllLines("lines.txt");

// Append (doesn't overwrite)
File.AppendAllText("output.txt", "\nAppended line.");
```

### Path Utilities

```csharp
string path = Path.Combine("data", "subfolder", "file.txt");  // Safe path joining
bool exists = File.Exists("output.txt");
string dir = Path.GetDirectoryName(path);
string ext = Path.GetExtension(path);
string name = Path.GetFileName(path);
```

### Stream-Based I/O (for larger files)

```csharp
// Writing
using StreamWriter writer = new StreamWriter("log.txt", append: true);
writer.WriteLine($"[{DateTime.Now}] Event logged");

// Reading line by line
using StreamReader reader = new StreamReader("log.txt");
string line;
while ((line = reader.ReadLine()) != null)
    Console.WriteLine(line);
```

### Directory Operations

```csharp
Directory.CreateDirectory("data");
bool exists = Directory.Exists("data");
string[] files = Directory.GetFiles(".", "*.txt");
```

---

## 11.2 IDisposable and `using` Statements

Some resources (file handles, database connections, network sockets) exist outside .NET's managed heap. The garbage collector doesn't handle them — you must **explicitly release them**.

Classes with unmanaged resources implement `IDisposable` (a `Dispose()` method).

### The `using` Statement

Ensures `Dispose()` is called automatically — even if an exception occurs:

```csharp
// Block form
using (StreamReader reader = new StreamReader("data.txt"))
{
    string content = reader.ReadToEnd();
}  // Dispose() called here

// Declaration form (C# 8+) — disposes at end of scope
using StreamReader reader = new StreamReader("data.txt");
string content = reader.ReadToEnd();
// Dispose() called when method/block ends
```

> **Rule of thumb:** If a class implements `IDisposable`, always wrap it in `using`.

---

## 11.3 Serialization and Persistence

**Serialization** = object → storable format (JSON). **Deserialization** = stored format → object.

### JSON with System.Text.Json

```csharp
using System.Text.Json;

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

// Serialize
var person = new Person { Name = "Alice", Age = 30 };
var options = new JsonSerializerOptions { WriteIndented = true };
string json = JsonSerializer.Serialize(person, options);

// Deserialize
Person loaded = JsonSerializer.Deserialize<Person>(json);
```

### Persisting to Files

```csharp
// Save a list to file
List<Person> people = new() { new() { Name = "Alice", Age = 30 } };
string json = JsonSerializer.Serialize(people, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText("people.json", json);

// Load from file
string data = File.ReadAllText("people.json");
List<Person> result = JsonSerializer.Deserialize<List<Person>>(data);
```

### Reusable JsonFileStore Pattern

```csharp
public class JsonFileStore<T>
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _opts = new() { WriteIndented = true };

    public JsonFileStore(string filePath) { _filePath = filePath; }

    public List<T> LoadAll()
    {
        if (!File.Exists(_filePath)) return new List<T>();
        string json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<T>>(json, _opts) ?? new List<T>();
    }

    public void SaveAll(List<T> items)
    {
        File.WriteAllText(_filePath, JsonSerializer.Serialize(items, _opts));
    }
}

// Usage
var store = new JsonFileStore<Person>("people.json");
var people = store.LoadAll();
people.Add(new Person { Name = "Bob", Age = 25 });
store.SaveAll(people);
```

---

## 11.4 Logging

### Why Logging?

`Console.WriteLine` is fine for learning, but real apps need persistent, structured logging to diagnose issues without a debugger attached.

### Simple File Logger

```csharp
public static class SimpleLogger
{
    private static readonly string _logFile = "app.log";

    public static void Log(string level, string message)
    {
        string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
        File.AppendAllText(_logFile, entry + Environment.NewLine);
        Console.WriteLine(entry);
    }

    public static void Info(string msg) => Log("INFO", msg);
    public static void Warning(string msg) => Log("WARN", msg);
    public static void Error(string msg) => Log("ERROR", msg);
}

SimpleLogger.Info("Application started");
SimpleLogger.Error("Failed to connect");
```

### Log Levels

| Level | Purpose |
|-------|---------|
| Debug | Detailed diagnostics (dev only) |
| Info | General operational events |
| Warning | Unexpected but not critical |
| Error | Operation failure |
| Critical | Application-wide failure |

> In production, use frameworks like **Serilog**, **NLog**, or `Microsoft.Extensions.Logging`.

---

## Key Takeaways

- `File.ReadAllText` / `WriteAllText` for simple file operations
- Use `using` statements with **any** `IDisposable` type (streams, connections)
- `System.Text.Json` for JSON serialization/deserialization
- The `JsonFileStore<T>` pattern provides a reusable file-based data layer
- Log operations to files with severity levels for production diagnostics