# Module 2: Development Environment

---

## 2.1 Visual Studio Code Setup

VS Code is our primary IDE. Install these extensions from the Extensions panel (`Ctrl+Shift+X`):

**Required:**

- **C# Dev Kit** (Microsoft) — IntelliSense, debugging, Solution Explorer
- **C#** (Microsoft) — core language support (auto-installed with Dev Kit)

**Recommended:**

- **GitLens** — enhanced Git integration
- **Error Lens** — inline error/warning display

### Key Shortcuts

| Action | Shortcut |
|--------|----------|
| Open terminal | `` Ctrl+` `` |
| Command palette | `Ctrl+Shift+P` |
| Go to file | `Ctrl+P` |
| Go to definition | `F12` |
| Peek definition | `Alt+F12` |
| Rename symbol | `F2` |
| Format document | `Shift+Alt+F` |
| Toggle line comment | `Ctrl+/` |
| Toggle block comment | `Shift+Alt+A` |

---

## 2.2 The Debugger

The debugger lets you pause execution, inspect variables, and step through code line by line.

### Setup

1. Open your project in VS Code
2. Press `Ctrl+Shift+P` → "Debug: Generate C# Assets" (first time only)
3. This creates `.vscode/launch.json`

### Breakpoints

Click in the gutter (left of line numbers) to set a **breakpoint** (red dot). Execution pauses when it hits that line.

**Conditional breakpoints:** Right-click a breakpoint → "Edit Breakpoint" → enter a condition (e.g., `i == 3`).

### Controls

| Action | Shortcut |
|--------|----------|
| Start debugging | `F5` |
| Stop debugging | `Shift+F5` |
| Step Over | `F10` |
| Step Into | `F11` |
| Step Out | `Shift+F11` |
| Continue | `F5` |

### Panels

- **Variables** — current values of local and global variables
- **Watch** — custom expressions you want to monitor
- **Call Stack** — chain of method calls leading to current point
- **Debug Console** — execute expressions while paused

---

## 2.3 Solutions

A **solution** (`.sln`) groups related projects together.

```bash
dotnet new sln -n MyApplication
dotnet new console -n MyApp.Console
dotnet new classlib -n MyApp.Core
dotnet sln add MyApp.Console/MyApp.Console.csproj
dotnet sln add MyApp.Core/MyApp.Core.csproj
dotnet sln list
```

```
MyApplication/
├── MyApplication.sln
├── MyApp.Console/
│   ├── MyApp.Console.csproj
│   └── Program.cs
└── MyApp.Core/
    ├── MyApp.Core.csproj
    └── Class1.cs
```

---

## 2.4 Projects

**Console App** — an executable with an entry point:

```bash
dotnet new console -n MyConsoleApp
```

**Class Library** — reusable code packaged as a `.dll` (no entry point):

```bash
dotnet new classlib -n MyLibrary
```

Use Console Apps for things that run. Use Class Libraries for reusable shared code.

---

## 2.5 MSBuild and The Project File

The `.csproj` file is an XML file that controls your project's build configuration:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

| Property | Purpose |
|----------|---------|
| `OutputType` | `Exe` (console app) or `Library` (class lib) |
| `TargetFramework` | .NET version (`net8.0`, `net9.0`) |
| `ImplicitUsings` | Auto-imports common namespaces |
| `Nullable` | Enables nullable reference type warnings |

---

## 2.6 Project References

To use code from one project in another:

```bash
dotnet add MyApp.Console/MyApp.Console.csproj reference MyApp.Core/MyApp.Core.csproj
```

This adds to the `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\MyApp.Core\MyApp.Core.csproj" />
</ItemGroup>
```

```csharp
// Now you can use classes from MyApp.Core
using MyApp.Core;

var calc = new Calculator();
Console.WriteLine(calc.Add(5, 3));  // 8
```

---

## 2.7 NuGet and Package References

**NuGet** is .NET's package manager. Browse packages at https://www.nuget.org.

```bash
dotnet add package Newtonsoft.Json       # Install
dotnet list package                       # List installed
dotnet remove package Newtonsoft.Json     # Remove
dotnet restore                            # Restore after cloning
```

Using a package:

```csharp
using Newtonsoft.Json;

var person = new { Name = "Alice", Age = 30 };
string json = JsonConvert.SerializeObject(person);
```

---

## 2.8 Building and Publishing Apps

```bash
dotnet build                   # Compile (output: bin/Debug/)
dotnet run                     # Build + execute
dotnet build -c Release        # Optimized build

# Publish
dotnet publish -c Release                              # Framework-dependent
dotnet publish -c Release --self-contained -r win-x64  # Self-contained
```

**Runtime identifiers:** `win-x64`, `linux-x64`, `osx-x64`, `osx-arm64`

---

## 2.9 Visual Studio vs. VS Code

| Feature | VS Code | Visual Studio |
|---------|---------|---------------|
| Price | Free | Free (Community) / Paid |
| Weight | ~200MB | ~8–20GB |
| Platform | Windows, Mac, Linux | Windows, Mac |
| C# Support | Via extensions | Built-in |
| GUI Designers | No | Yes |
| Best For | Lightweight, cross-platform | Large enterprise apps |

We use VS Code for this course.

---

## Key Takeaways

- VS Code + **C# Dev Kit** is our primary development setup
- The **debugger** (F5, F10, breakpoints) is essential — use it instead of `Console.WriteLine` debugging
- **Solutions** group projects; **Project References** connect them
- **NuGet** provides third-party packages
- `dotnet build`, `dotnet run`, `dotnet publish` are your core CLI commands
