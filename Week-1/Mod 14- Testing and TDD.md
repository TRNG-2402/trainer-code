# Module 14: Testing & TDD

---

## 14.1 Kinds of Testing

| Level | Scope | Speed | Example |
|-------|-------|-------|---------|
| **Unit** | Single method/class | Very fast | `Calculator.Add(2,3)` returns 5 |
| **Integration** | Multiple components | Medium | API endpoint hits database correctly |
| **End-to-End** | Full application flow | Slow | User logs in and places an order |

**The testing pyramid:** Many unit tests (fast, cheap) → fewer integration tests → fewest E2E tests.

This module focuses on **unit testing**.

---

## 14.2 Arrange, Act, Assert (AAA)

Every unit test follows three steps:

```csharp
// Arrange — set up objects and data
var calc = new Calculator();

// Act — call the method being tested
int result = calc.Add(2, 3);

// Assert — verify the result
Assert.Equal(5, result);
```

---

## 14.3 xUnit Setup

**xUnit** is the most popular .NET testing framework.

### Create a Test Project

```bash
dotnet new xunit -n MyApp.Tests
dotnet sln add MyApp.Tests/MyApp.Tests.csproj
dotnet add MyApp.Tests reference MyApp.Core/MyApp.Core.csproj
```

### Writing Tests

```csharp
using Xunit;
using MyApp.Core;

public class CalculatorTests
{
    [Fact]
    public void Add_TwoPositiveNumbers_ReturnsSum()
    {
        // Arrange
        var calc = new Calculator();

        // Act
        int result = calc.Add(2, 3);

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public void Divide_ByZero_ThrowsException()
    {
        var calc = new Calculator();
        Assert.Throws<DivideByZeroException>(() => calc.Divide(10, 0));
    }
}
```

### Running Tests

```bash
dotnet test
```

In VS Code, C# Dev Kit provides a **Test Explorer** panel to run and debug tests visually.

### Common Assertions

```csharp
Assert.Equal(expected, actual);          // Values match
Assert.NotEqual(unexpected, actual);     // Values differ
Assert.True(condition);                  // Condition is true
Assert.False(condition);                 // Condition is false
Assert.Null(obj);                        // Object is null
Assert.NotNull(obj);                     // Object is not null
Assert.Contains("sub", fullString);      // String contains substring
Assert.Empty(collection);               // Collection is empty
Assert.Throws<T>(() => call());          // Method throws exception T
```

### Test Naming Convention

`MethodName_Scenario_ExpectedBehavior`

```
Add_TwoPositiveNumbers_ReturnsSum
Withdraw_ExceedsBalance_ThrowsException
IsAdult_AgeBelow18_ReturnsFalse
```

---

## 14.4 Theory Tests with InlineData

`[Fact]` tests one case. `[Theory]` with `[InlineData]` tests **multiple cases** with the same logic:

```csharp
[Theory]
[InlineData(2, 3, 5)]
[InlineData(-1, 1, 0)]
[InlineData(0, 0, 0)]
[InlineData(100, -50, 50)]
public void Add_VariousInputs_ReturnsExpectedSum(int a, int b, int expected)
{
    var calc = new Calculator();
    Assert.Equal(expected, calc.Add(a, b));
}

[Theory]
[InlineData(1, false)]
[InlineData(2, true)]
[InlineData(17, true)]
[InlineData(18, false)]
public void IsPrime_VariousInputs_ReturnsExpected(int number, bool expected)
{
    Assert.Equal(expected, MathHelper.IsPrime(number));
}
```

Each `[InlineData]` generates a **separate test case** — if one fails, you see exactly which input caused it.

---

## 14.5 Test-Driven Development (TDD)

TDD follows a strict cycle:

### 🔴 Red — Write a Failing Test

```csharp
[Fact]
public void IsValidEmail_ValidEmail_ReturnsTrue()
{
    Assert.True(StringValidator.IsValidEmail("alice@example.com"));
}

[Fact]
public void IsValidEmail_MissingAt_ReturnsFalse()
{
    Assert.False(StringValidator.IsValidEmail("aliceexample.com"));
}
```

Won't compile — `StringValidator` doesn't exist yet. That's expected.

### 🟢 Green — Write Minimum Code to Pass

```csharp
public static class StringValidator
{
    public static bool IsValidEmail(string email)
    {
        return email.Contains("@") && email.Contains(".");
    }
}
```

Tests pass. Not perfect, but satisfies the current tests.

### 🔵 Refactor — Improve, Add More Tests

```csharp
[Theory]
[InlineData("alice@example.com", true)]
[InlineData("user@domain.co.uk", true)]
[InlineData("noatsign.com", false)]
[InlineData("@nodomain", false)]
[InlineData("", false)]
public void IsValidEmail_VariousInputs(string email, bool expected)
{
    Assert.Equal(expected, StringValidator.IsValidEmail(email));
}
```

New tests fail → improve implementation → all green → repeat.

### TDD Benefits

- Forces you to think about **behavior before implementation**
- Creates a comprehensive test suite **automatically**
- Catches regressions **immediately**
- Results in smaller, focused methods

---

## Quick Reference

| Attribute | Purpose |
|-----------|---------|
| `[Fact]` | Test with no parameters (single case) |
| `[Theory]` | Parameterized test |
| `[InlineData(...)]` | Provides data to a Theory test |

| Command | Action |
|---------|--------|
| `dotnet new xunit -n Tests` | Create test project |
| `dotnet test` | Run all tests |
| `dotnet test --filter "ClassName"` | Run specific tests |

---

## Key Takeaways

- Structure every test as **Arrange → Act → Assert**
- Use `[Fact]` for single-case tests, `[Theory]` + `[InlineData]` for parameterized tests
- Name tests: `Method_Scenario_Expected`
- **TDD cycle: Red → Green → Refactor**
- Write the test first, then the minimum code to pass, then improve
- Run tests with `dotnet test` or VS Code's Test Explorer