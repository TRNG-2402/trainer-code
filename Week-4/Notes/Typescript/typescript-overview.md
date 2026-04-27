# TypeScript Overview

## Learning Objectives
- Explain what TypeScript is and why it exists
- Install and configure the TypeScript compiler
- Annotate variables, functions, and objects with TypeScript types
- Write interfaces and type aliases and know when to use each
- Read a generic function signature and write a simple generic

## Why This Matters
Every React file you write this week will be a `.tsx` file -- TypeScript + JSX. TypeScript catches the entire category of "I called a method on something that was undefined" before the code ever runs. On a team, it also serves as living documentation: the type signature of a function tells every other developer exactly what it expects and what it returns, without reading the body.

The TypeScript you learn today is also directly applicable to your ASP.NET Core work -- the concepts of interfaces, generics, and strict typing map directly to C#.

---

## JavaScript vs TypeScript

| | JavaScript | TypeScript |
|--|-----------|-----------|
| Typing | Dynamic (types resolved at runtime) | Static (types checked at compile time) |
| Errors caught | At runtime (in the browser or Node) | At compile time (in the editor) |
| File extension | `.js`, `.jsx` | `.ts`, `.tsx` |
| Runs in browser | Directly | No -- must compile to JS first |
| Tooling/IDE support | Good | Excellent (full autocomplete, refactoring) |
| Learning curve | Lower initially | Slightly higher, pays off quickly |

TypeScript is a **superset** of JavaScript. Every valid `.js` file is also valid TypeScript. You add types on top; the compiler strips them away and outputs plain JavaScript.

---

## Installation and Setup

### Install the Compiler

```bash
# Global (available anywhere)
npm install -g typescript

# Project-local (preferred -- keeps versions consistent per project)
npm install --save-dev typescript
```

### Compile a File

```bash
tsc hello.ts          # produces hello.js
tsc --watch hello.ts  # recompile on save
```

### Initialize a Project

```bash
tsc --init            # creates tsconfig.json
```

---

## `tsconfig.json`

This file tells the compiler how to behave for the whole project.

```json
{
  "compilerOptions": {
    "target": "ES2020",        // output JS version
    "module": "ESNext",        // module format (ES modules for Vite/browser)
    "lib": ["ES2020", "DOM"],  // type definitions to include
    "outDir": "./dist",        // where to put compiled JS
    "rootDir": "./src",        // where source TS lives
    "strict": true,            // enables ALL strict checks (recommended)
    "noUnusedLocals": true,    // error on unused variables
    "noUnusedParameters": true,
    "esModuleInterop": true,   // cleaner imports of CommonJS modules
    "skipLibCheck": true,      // skip type checking of .d.ts files in node_modules
    "jsx": "react-jsx"         // for .tsx files (Vite projects use this)
  },
  "include": ["src"],
  "exclude": ["node_modules", "dist"]
}
```

The most important setting is `"strict": true`. It enables a set of checks that catch the most common TypeScript mistakes. Always use it.

---

## Basic Types

TypeScript infers types when possible. You add explicit annotations when inference cannot help or when you want to document intent.

```typescript
// Type inference -- no annotation needed
let count = 0;        // TypeScript infers: number
let name = "Alice";   // infers: string
let active = true;    // infers: boolean

// Explicit annotation (: Type after the variable name)
let score: number = 100;
let username: string = "alice";
let isLoggedIn: boolean = false;
```

### Primitive Types

```typescript
let id: number = 1;
let title: string = "Product";
let available: boolean = true;
let nothing: null = null;
let notSet: undefined = undefined;
```

### `any` and `unknown`

```typescript
// any -- disables type checking for this variable (avoid)
let data: any = "hello";
data = 42;           // OK, no error
data.toUpperCase();  // OK at compile time -- but wrong at runtime if data is 42

// unknown -- like any, but forces you to check the type before using it
let input: unknown = fetchSomething();
if (typeof input === "string") {
  input.toUpperCase(); // OK -- TypeScript knows it's a string here
}
```

Prefer `unknown` over `any`. When you use `any`, you are opting out of TypeScript entirely for that variable.

### `void` and `never`

```typescript
// void -- function returns nothing
function logMessage(msg: string): void {
  console.log(msg);
}

// never -- function never returns (throws or loops forever)
function throwError(message: string): never {
  throw new Error(message);
}
```

### Arrays and Tuples

```typescript
// Arrays
const ids: number[] = [1, 2, 3];
const names: Array<string> = ["Alice", "Bob"]; // generic syntax, equivalent

// Tuple -- fixed-length array with specific types at each position
const point: [number, number] = [10, 20];
const entry: [string, number] = ["age", 25];
```

### Union Types

A value that can be one of several types:

```typescript
let id: number | string = 1;
id = "abc"; // also valid

function formatId(id: number | string): string {
  if (typeof id === "number") {
    return `#${id.toString().padStart(4, "0")}`;
  }
  return id;
}
```

### Type Assertions

When you know more about a type than TypeScript can infer:

```typescript
const input = document.querySelector("#username") as HTMLInputElement;
input.value; // TypeScript now knows this is an HTMLInputElement, not just Element
```

Use assertions sparingly. If you are asserting constantly, your types are probably too loose.

---

## Functions

```typescript
// Parameter and return type annotations
function add(a: number, b: number): number {
  return a + b;
}

// Arrow function
const multiply = (a: number, b: number): number => a * b;

// Optional parameters (must come after required)
function greet(name: string, greeting?: string): string {
  return `${greeting ?? "Hello"}, ${name}!`;
}

// Default parameters
function createSlug(title: string, separator: string = "-"): string {
  return title.toLowerCase().replace(/\s+/g, separator);
}

// Rest parameters
function sum(...numbers: number[]): number {
  return numbers.reduce((acc, n) => acc + n, 0);
}
```

---

## Interfaces

Interfaces define the shape of an object. They are the most common way to type objects and React props in this course.

```typescript
interface Product {
  id: number;
  name: string;
  price: number;
  inStock: boolean;
  description?: string;   // optional -- may be undefined
  readonly sku: string;   // cannot be modified after creation
}

// Use the interface as a type annotation
const laptop: Product = {
  id: 1,
  name: "Laptop",
  price: 999.99,
  inStock: true,
  sku: "LAP-001",
  // description is optional, so omitting it is fine
};

// laptop.sku = "NEW"; // Error: cannot assign to 'sku' because it is read-only
```

### Extending Interfaces

```typescript
interface Animal {
  name: string;
  sound(): string;
}

interface Dog extends Animal {
  breed: string;
}

const rex: Dog = {
  name: "Rex",
  breed: "Labrador",
  sound() { return "Woof"; },
};
```

### Function Interfaces

```typescript
interface Formatter {
  (value: number, currency: string): string;
}

const formatPrice: Formatter = (value, currency) =>
  `${currency}${value.toFixed(2)}`;
```

---

## Type Aliases

`type` creates a named alias for any type expression.

```typescript
type ProductId = number;
type Status = "active" | "inactive" | "pending"; // union literal type

let currentStatus: Status = "active";
// currentStatus = "deleted"; // Error: not assignable to type Status

type Coordinates = {
  x: number;
  y: number;
};

type StringOrNumber = string | number;
```

### Interfaces vs Type Aliases

| | `interface` | `type` |
|--|------------|--------|
| Object shapes | Yes (primary use) | Yes |
| Primitive aliases | No | Yes (`type Id = number`) |
| Union / intersection | No (extends only) | Yes (`A | B`, `A & B`) |
| Extending | `extends` keyword | `&` intersection |
| Declaration merging | Yes (two `interface Foo` merge) | No (error on duplicate) |
| Recursive types | Yes | Yes (with some limits) |

**Rule of thumb:**
- Use `interface` for object shapes (component props, API response models).
- Use `type` for unions, intersections, primitives, and utility aliases.

---

## Generics

Generics let you write reusable code that works with any type while preserving type safety.

```typescript
// Without generics: loses type info
function identity(arg: any): any {
  return arg;
}

// With generics: type is preserved
function identity<T>(arg: T): T {
  return arg;
}

const str = identity("hello");  // T inferred as string; result is string
const num = identity(42);       // T inferred as number; result is number
```

### Generic Functions

```typescript
function firstElement<T>(arr: T[]): T | undefined {
  return arr[0];
}

firstElement([1, 2, 3]);       // returns number | undefined
firstElement(["a", "b"]);      // returns string | undefined
firstElement([]);              // returns undefined
```

### Generic Interfaces

```typescript
interface ApiResponse<T> {
  data: T;
  status: number;
  message: string;
}

// Concrete uses
type ProductResponse = ApiResponse<Product>;
type ProductListResponse = ApiResponse<Product[]>;

const response: ProductListResponse = {
  data: [laptop],
  status: 200,
  message: "OK",
};
```

You will see this pattern constantly when typing API responses in your React components.

### Constraints

```typescript
// T must have at least a name property
function getLabel<T extends { name: string }>(item: T): string {
  return item.name;
}

getLabel({ name: "Laptop", price: 999 }); // OK
// getLabel({ price: 999 });              // Error: missing 'name'
```

---

## Practical TypeScript Patterns

### Object Destructuring with Types

```typescript
function displayProduct({ name, price, inStock }: Product): string {
  return `${name} -- $${price} (${inStock ? "available" : "sold out"})`;
}
```

### Typed Arrays from Map/Filter

```typescript
const products: Product[] = [];
const names: string[] = products.map(p => p.name);        // string[]
const available: Product[] = products.filter(p => p.inStock); // Product[]
```

### `as const` for Literal Arrays

```typescript
const ROLES = ["admin", "viewer", "editor"] as const;
type Role = typeof ROLES[number]; // "admin" | "viewer" | "editor"
```

---

## Summary
- TypeScript adds static types to JavaScript. Errors are caught at compile time, not runtime.
- Use `strict: true` in `tsconfig.json` -- always.
- Prefer `unknown` over `any`. `any` opts out of type checking.
- `interface` for object shapes; `type` for unions, primitives, and aliases.
- Generics make reusable code type-safe. `<T>` is a type placeholder filled in at call time.
- All TypeScript is erased at compile time -- the browser only ever runs JavaScript.

## Additional Resources
- [TypeScript: The TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/intro.html)
- [TypeScript: TypeScript in 5 minutes](https://www.typescriptlang.org/docs/handbook/typescript-in-5-minutes.html)
- [MDN: TypeScript](https://developer.mozilla.org/en-US/docs/Learn/Tools_and_testing/Client-side_JavaScript_frameworks/Introduction#typescript)
