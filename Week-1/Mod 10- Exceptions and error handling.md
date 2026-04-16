# Module 10: Exceptions & Error Handling

---

## 10.1 Exceptions

An **exception** is an object representing an error during execution. If not caught, the program crashes.

### Common Exceptions

| Exception | Cause |
|-----------|-------|
| `NullReferenceException` | Accessing a member on `null` |
| `IndexOutOfRangeException` | Array index out of bounds |
| `ArgumentException` | Invalid argument value |
| `FormatException` | `int.Parse("abc")` |
| `DivideByZeroException` | Integer division by zero |
| `FileNotFoundException` | File doesn't exist |
| `InvalidOperationException` | Operation invalid for current state |
| `KeyNotFoundException` | Dictionary key doesn't exist |

All exceptions inherit from `System.Exception`, which provides `Message`, `StackTrace`, and `InnerException`.

---

## 10.2 try, catch, and finally

### Basic try/catch

```csharp
try
{
    int number = int.Parse(Console.ReadLine());
    Console.WriteLine($"You entered: {number}");
}
catch (FormatException ex)
{
    Console.WriteLine($"Invalid input: {ex.Message}");
}
```

### Multiple catch Blocks

Catch **specific exceptions first**, then more general ones:

```csharp
try
{
    int[] nums = { 1, 2, 3 };
    int index = int.Parse(Console.ReadLine());
    Console.WriteLine(nums[index]);
}
catch (FormatException)
{
    Console.WriteLine("Not a valid number.");
}
catch (IndexOutOfRangeException)
{
    Console.WriteLine("Index out of bounds.");
}
catch (Exception ex)   // Catch-all — always last
{
    Console.WriteLine($"Unexpected: {ex.Message}");
}
```

### finally Block

Always executes, whether an exception occurred or not — use for cleanup:

```csharp
StreamReader reader = null;
try
{
    reader = new StreamReader("data.txt");
    Console.WriteLine(reader.ReadToEnd());
}
catch (FileNotFoundException)
{
    Console.WriteLine("File not found.");
}
finally
{
    reader?.Close();   // Cleanup runs no matter what
}
```

---

## 10.3 Throwing Exceptions

### Throwing in Your Code

```csharp
public void Withdraw(decimal amount)
{
    if (amount <= 0)
        throw new ArgumentException("Must be positive.", nameof(amount));
    if (amount > Balance)
        throw new InvalidOperationException("Insufficient funds.");
    Balance -= amount;
}
```

### Re-throwing

Use `throw;` (not `throw ex;`) to preserve the original stack trace:

```csharp
catch (Exception ex)
{
    Console.WriteLine($"Logged: {ex.Message}");
    throw;   // Preserves stack trace
}
```

### Custom Exceptions

```csharp
public class InsufficientFundsException : Exception
{
    public decimal Balance { get; }
    public decimal Amount { get; }

    public InsufficientFundsException(decimal balance, decimal amount)
        : base($"Insufficient funds. Balance: {balance:C}, Attempted: {amount:C}")
    {
        Balance = balance;
        Amount = amount;
    }
}

// Usage
throw new InsufficientFundsException(account.Balance, withdrawAmount);
```

---

## Best Practices

| Do | Don't |
|----|-------|
| Catch specific exception types | Catch generic `Exception` as a habit |
| Include meaningful messages | Use vague messages like "Error occurred" |
| Use `throw;` to re-throw | Use `throw ex;` (resets stack trace) |
| Use `nameof()` for argument names | Hardcode argument names as strings |
| Handle or re-throw — pick one | Silently swallow: `catch { }` |

```csharp
// BAD — hides bugs
try { DoSomething(); } catch { }

// GOOD — handle meaningfully or re-throw
try { DoSomething(); }
catch (SpecificException ex)
{
    LogError(ex);
    throw;
}
```

---

## Key Takeaways

- Exceptions represent runtime errors; uncaught exceptions crash the program
- Use `try/catch` with **specific types first**, general `Exception` last
- `finally` always runs — ideal for cleanup
- `throw` signals errors; `throw;` re-throws preserving the stack trace
- Never silently swallow exceptions