# Module 9: Collections & Data Structures

---

## 9.1 List\<T\>

A **generic**, dynamically-sized, type-safe list — the go-to collection.

```csharp
List<string> names = new List<string>();

names.Add("Alice");
names.Add("Bob");
names.Add("Charlie");

Console.WriteLine(names[0]);      // "Alice"
Console.WriteLine(names.Count);   // 3

names.Insert(1, "Zara");          // Insert at index 1
names.Remove("Bob");              // Remove by value
names.RemoveAt(0);                // Remove by index

bool found = names.Contains("Alice");  // Check existence
names.Sort();                          // Sort in place

// Initialize inline
var numbers = new List<int> { 10, 20, 30, 40 };
```

**Generics (`<T>`):** The type parameter ensures type safety. `List<int>` only holds ints, `List<Person>` only holds Person objects. Errors are caught at compile time, not runtime.

---

## 9.2 foreach and IEnumerable\<T\>

`foreach` works on anything that implements `IEnumerable<T>` — arrays, lists, dictionaries, hash sets, and more.

```csharp
foreach (int num in numbers)
    Console.WriteLine(num);
```

**Use `IEnumerable<T>` as a parameter** to accept any collection:

```csharp
static void PrintAll(IEnumerable<string> items)
{
    foreach (var item in items)
        Console.WriteLine(item);
}

PrintAll(new List<string> { "a", "b" });  // Works
PrintAll(new string[] { "x", "y" });      // Also works
```

---

## 9.3 Dictionary\<TKey, TValue\>

Key-value pairs with **O(1) average lookup**:

```csharp
var capitals = new Dictionary<string, string>
{
    { "France", "Paris" },
    { "Japan", "Tokyo" },
    { "Brazil", "Brasília" }
};

// Access
Console.WriteLine(capitals["France"]);  // "Paris"

// Add / update
capitals["Germany"] = "Berlin";

// Safe access (avoid KeyNotFoundException)
if (capitals.TryGetValue("Italy", out string capital))
    Console.WriteLine(capital);
else
    Console.WriteLine("Not found");

// Check key exists
bool has = capitals.ContainsKey("Japan");  // true

// Remove
capitals.Remove("Brazil");

// Iterate
foreach (var (country, city) in capitals)
    Console.WriteLine($"{country}: {city}");
```

> **Keys must be unique.** Always use `TryGetValue` or `ContainsKey` to avoid exceptions.

---

## 9.4 HashSet\<T\>

Stores **unique elements** with **O(1) lookup**:

```csharp
var tags = new HashSet<string> { "csharp", "dotnet", "backend" };

tags.Add("csharp");           // Ignored — already exists
tags.Contains("dotnet");      // true (fast!)
tags.Remove("backend");
Console.WriteLine(tags.Count); // 2
```

### Set Operations

```csharp
var a = new HashSet<int> { 1, 2, 3, 4 };
var b = new HashSet<int> { 3, 4, 5, 6 };

a.UnionWith(b);       // { 1, 2, 3, 4, 5, 6 }
a.IntersectWith(b);   // { 3, 4 }
a.ExceptWith(b);      // { 1, 2 }
```

---

## 9.5 Stack, Queue, and LinkedList

### Stack\<T\> — Last In, First Out (LIFO)

```csharp
var stack = new Stack<string>();
stack.Push("First");
stack.Push("Second");
stack.Push("Third");

stack.Pop();   // "Third"
stack.Peek();  // "Second" (doesn't remove)
```

*Use cases: undo/redo, expression evaluation, backtracking.*

### Queue\<T\> — First In, First Out (FIFO)

```csharp
var queue = new Queue<string>();
queue.Enqueue("Customer 1");
queue.Enqueue("Customer 2");

queue.Dequeue();  // "Customer 1"
queue.Peek();     // "Customer 2"
```

*Use cases: task scheduling, print queues, BFS.*

### LinkedList\<T\> — Doubly-linked list

```csharp
var list = new LinkedList<string>();
list.AddLast("A");
list.AddLast("B");
list.AddFirst("Z");

var node = list.Find("A");
list.AddAfter(node, "A2");
// Z → A → A2 → B
```

*Use cases: frequent insertion/removal in the middle of a sequence.*

---

## 9.6 Big O Notation

Describes how an operation's time grows with input size:

| Notation | Name | Example |
|----------|------|---------|
| O(1) | Constant | Dictionary lookup, HashSet.Contains |
| O(log n) | Logarithmic | Binary search |
| O(n) | Linear | List.Contains, looping an array |
| O(n log n) | Linearithmic | Sorting (.Sort()) |
| O(n²) | Quadratic | Nested loops |

### Collection Performance

| Operation | List | Dictionary | HashSet | LinkedList |
|-----------|------|------------|---------|------------|
| Access by index | O(1) | — | — | O(n) |
| Search | O(n) | O(1) | O(1) | O(n) |
| Insert (end) | O(1)* | O(1) | O(1) | O(1) |
| Insert (middle) | O(n) | — | — | O(1)** |
| Delete | O(n) | O(1) | O(1) | O(1)** |

*Amortized. **With reference to node.

---

## Choosing the Right Collection

| Need | Use |
|------|-----|
| Ordered list with index access | `List<T>` |
| Key-value lookups | `Dictionary<TKey, TValue>` |
| Unique elements, fast membership check | `HashSet<T>` |
| LIFO (undo, backtracking) | `Stack<T>` |
| FIFO (queues, scheduling) | `Queue<T>` |
| Frequent mid-sequence insertions | `LinkedList<T>` |

---

## Key Takeaways

- `List<T>` is the default collection — dynamic size, type-safe
- `Dictionary<K,V>` for fast key-based lookups; keys must be unique
- `HashSet<T>` for unique elements and set operations
- `foreach` works on any `IEnumerable<T>`
- Choose collections based on the operations you need and their Big O complexity