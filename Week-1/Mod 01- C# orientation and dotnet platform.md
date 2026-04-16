# Module 1: C# Orientation & The .NET Platform

---

## 1.1 What Is a Full Stack Developer?

A full stack developer works across all layers of a software application:

- **Frontend (Client-Side):** The user interface — HTML, CSS, JavaScript, and frameworks like React or Angular.
- **Backend (Server-Side):** The logic and data processing that runs on a server — this is where **C# and .NET** come in.
- **Database / Data Layer:** Persistent storage for application data — SQL Server, PostgreSQL, and tools like Entity Framework.

In this course you'll learn to build all three layers, with C# and .NET powering the backend.

---

## 1.2 What Is .NET?

**.NET** is a free, open-source developer platform created by Microsoft. It is **not a language** — it's an ecosystem that supports multiple languages (C#, F#, VB.NET).

**Key facts:**

- **Cross-platform** — runs on Windows, macOS, and Linux.
- **Versatile** — build web apps, APIs, desktop apps, mobile apps, games, and more.
- **Modern** — .NET 5+ unified the older .NET Framework (Windows-only) and .NET Core (cross-platform) into a single platform.

**Brief History:**

| Year | Milestone |
|------|-----------|
| 2002 | .NET Framework 1.0 (Windows only) |
| 2016 | .NET Core 1.0 (cross-platform) |
| 2020 | .NET 5 — unified platform |
| 2024 | .NET 9 — latest release |

**The ecosystem includes:**

- **CLR (Common Language Runtime)** — executes your compiled code
- **BCL (Base Class Library)** — pre-built classes for I/O, collections, networking, etc.
- **ASP.NET Core** — web applications and APIs
- **Entity Framework Core** — database access (ORM)
- **NuGet** — package manager (like npm for JavaScript)

---

## 1.3 What Is C#?

**C#** (pronounced "C-sharp") is a modern, object-oriented, type-safe programming language. It is the primary language for .NET development.

**Key characteristics:**

- **Statically typed** — variable types are checked at compile time
- **Object-oriented** — classes, objects, inheritance, polymorphism
- **Managed** — memory handled by the garbage collector
- **Familiar syntax** — similar to Java, JavaScript, and C++

**Hello World in C#:**

```csharp
Console.WriteLine("Hello, World!");
```

> In modern C# (.NET 6+), this single line is a complete, valid program using **top-level statements**.

---

## 1.4 .NET SDK

The **.NET SDK** is everything you need to build and run .NET apps. It includes the runtime, compiler, CLI tools, and project templates.

**Essential commands:**

```bash
dotnet --version          # Check SDK version
dotnet --list-sdks        # List installed SDKs
dotnet new console -n MyApp   # Create a console app
cd MyApp
dotnet run                # Build and run
dotnet build              # Build only
```

**Generated project structure:**

```
MyApp/
├── MyApp.csproj     ← Project configuration (XML)
├── Program.cs       ← Your code (entry point)
└── obj/             ← Build artifacts
```

**Program.cs:**

```csharp
Console.WriteLine("Hello, World!");
```

**MyApp.csproj:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
</Project>
```

---

## 1.5 Managed Execution Process

C# uses a **two-step compilation** model:

**Step 1 — Build Time:** The Roslyn compiler converts C# source code into **Intermediate Language (IL)** — a platform-independent bytecode stored in `.dll` or `.exe` files.

**Step 2 — Run Time:** The **CLR (Common Language Runtime)** takes over:

- The **JIT compiler** converts IL to native machine code for your specific CPU/OS
- The **Garbage Collector** manages memory automatically
- The **Type System** enforces type safety

```
C# (.cs)  →  [Roslyn]  →  IL (.dll)  →  [JIT]  →  Native Code  →  Execution
```

**Key terms:**

| Term | Meaning |
|------|---------|
| CLR | Virtual machine that runs .NET code |
| IL | Platform-independent bytecode |
| JIT | Converts IL to native code at runtime |
| GC | Automatically frees unused memory |

---

## 1.6 .NET Ecosystem Overview

| Application Type | Technology |
|-----------------|-----------|
| Web Apps & APIs | ASP.NET Core |
| Web UI in C# | Blazor |
| Desktop | WPF, WinForms, .NET MAUI |
| Mobile | .NET MAUI |
| Games | Unity |
| Machine Learning | ML.NET |
| IoT | .NET IoT Libraries |

---

## 1.7 .NET Implementations

| Implementation | Status | Platform |
|---------------|--------|----------|
| .NET Framework | Legacy (maintenance only) | Windows only |
| .NET Core | Evolved into modern .NET | Cross-platform |
| .NET 5/6/7/8/9 | **Current** | Cross-platform |
| Mono | Merged into ecosystem | Xamarin, Unity |

**LTS (Long-Term Support)** releases like .NET 8 are supported for 3 years. **STS** releases like .NET 9 are supported for 18 months.

---

## 1.8 Common Language Infrastructure (CLI)

The **CLI** (ECMA-335 standard) is what makes .NET's multi-language support possible:

- **Common Type System (CTS)** — shared types across all .NET languages (`int` in C# = `Integer` in VB.NET = `System.Int32`)
- **Common Language Specification (CLS)** — interoperability rules so languages can share libraries
- **Intermediate Language (IL)** — common bytecode all .NET languages compile to

Because all .NET languages compile to IL, a C# project can reference an F# library seamlessly.

---

## 1.9 Automatic Memory Management

The **Garbage Collector (GC)** automatically frees memory from objects you no longer use.

**How it works:**

1. Objects are created on the heap
2. The GC periodically checks which objects are still reachable (referenced)
3. Unreachable objects are collected and their memory is freed

**Generational collection:**

| Generation | Description | Collection Frequency |
|-----------|-------------|---------------------|
| Gen 0 | Newly created objects | Frequent |
| Gen 1 | Survived one collection | Less frequent |
| Gen 2 | Long-lived objects | Rare |

Most objects are short-lived, so checking Gen 0 frequently is efficient.

> For resources outside .NET's control (file handles, database connections), you'll use `IDisposable` and `using` statements — covered in Module 11.

---

## Key Takeaways

- **.NET** is the platform; **C#** is the language
- The **.NET SDK** provides everything to build and run apps via the `dotnet` CLI
- C# compiles to **IL**, which the **JIT compiler** converts to native code at runtime
- The **Garbage Collector** manages memory automatically
- Modern .NET (5+) is cross-platform and unified
