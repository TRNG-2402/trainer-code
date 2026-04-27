# JavaScript Fundamentals

## Learning Objectives
- Identify primitive and reference types and explain how they are stored
- Predict the result of implicit type coercion
- Declare variables with the correct scope keyword
- Write control flow and handle runtime errors

## Why This Matters
TypeScript (Tuesday) adds a type layer on top of these exact JS behaviors. React state management (Wednesday) depends on reference type identity. Understanding the foundation prevents the "why is my state not updating" problems that trip up everyone early on.

> **Note:** This is a review reference for Week 3.

---

## Data Types

### Primitive Types

Stored by value. Immutable. Compared by value.

| Type | Example | Notes |
|------|---------|-------|
| `string` | `"hello"`, `'hi'`, `` `hi` `` | Immutable character sequence |
| `number` | `42`, `3.14`, `NaN`, `Infinity` | One type for all numbers (64-bit float) |
| `boolean` | `true`, `false` | |
| `null` | `null` | Intentional absence of value |
| `undefined` | `undefined` | Variable declared but not assigned |
| `symbol` | `Symbol('id')` | Unique identifier, rarely used directly |
| `bigint` | `9007199254740993n` | Integers beyond `Number.MAX_SAFE_INTEGER` |

### Reference Types

Stored by reference (pointer to a heap location). Two variables can point to the same object.

```javascript
// Primitives: copied by value
let a = 5;
let b = a;
b = 10;
console.log(a); // 5 -- unaffected

// Reference types: copied by reference
const obj1 = { name: "Alice" };
const obj2 = obj1;
obj2.name = "Bob";
console.log(obj1.name); // "Bob" -- same object!
```

Reference types: `object`, `array`, `function`, `Map`, `Set`, `Date`.

---

## Type Coercion

JavaScript automatically converts types in some operations. This surprises most developers.

```javascript
// Implicit coercion
"5" + 3        // "53"   -- + with a string triggers string concatenation
"5" - 3        // 2      -- - triggers numeric conversion
"5" * "3"      // 15
true + 1       // 2      -- true coerces to 1
null + 1       // 1      -- null coerces to 0
undefined + 1  // NaN

// Loose equality (==) coerces before comparing
0 == false     // true
"" == false    // true
null == undefined // true

// Strict equality (===) does NOT coerce -- always use this
0 === false    // false
"5" === 5      // false
```

**Rule:** Always use `===` and `!==`. Avoid `==`.

---

## Variables: `let`, `const`, `var`

```javascript
// var -- function-scoped, hoisted (avoid in modern code)
var x = 1;

// let -- block-scoped, not hoisted to usable state
let count = 0;
count = 1; // OK

// const -- block-scoped, binding cannot be reassigned
const PI = 3.14159;
// PI = 3;  // TypeError

// const with objects: the binding is fixed, the contents are not
const user = { name: "Alice" };
user.name = "Bob"; // OK -- mutating the object, not reassigning the binding
// user = {};      // TypeError
```

| | `var` | `let` | `const` |
|--|-------|-------|---------|
| Scope | Function | Block | Block |
| Hoisted | Yes (as `undefined`) | Yes (TDZ) | Yes (TDZ) |
| Reassignable | Yes | Yes | No (binding) |
| Re-declarable | Yes | No | No |

---

## Strict Mode

```javascript
"use strict"; // at the top of a file or function

// Catches common mistakes:
x = 5;           // ReferenceError -- undeclared variable
delete Object.prototype; // TypeError -- can't delete non-configurable
function(a, a) {} // SyntaxError -- duplicate parameter name
```

ES modules and class bodies are strict by default. Use `"use strict"` in scripts.

---

## Naming Conventions

| Convention | Use for | Example |
|-----------|---------|---------|
| `camelCase` | Variables, functions | `productCount`, `getUserById` |
| `PascalCase` | Classes, React components, constructors | `ProductCard`, `UserService` |
| `SCREAMING_SNAKE_CASE` | Constants with semantic meaning | `MAX_RETRY_COUNT`, `BASE_URL` |
| `_leadingUnderscore` | Private by convention (rare in modern code) | `_cache` |

---

## Operators

```javascript
// Assignment
let x = 5;
x += 2;   // x = x + 2 = 7
x -= 1;   // 6
x *= 3;   // 18
x /= 2;   // 9
x **= 2;  // 81
x %= 5;   // 1

// Arithmetic
5 + 2     // 7
5 - 2     // 3
5 * 2     // 10
5 / 2     // 2.5
5 % 2     // 1  (remainder)
2 ** 3    // 8  (exponentiation)

// Comparison (always use ===)
5 === 5   // true
5 !== 4   // true
5 > 4     // true
5 >= 5    // true

// Logical
true && false  // false
true || false  // true
!true          // false

// Short-circuit evaluation
null ?? "default"    // "default"  (nullish coalescing -- null or undefined)
user?.name           // undefined if user is null/undefined (optional chaining)
```

---

## Control Flow

```javascript
// if / else if / else
if (score >= 90) {
  grade = "A";
} else if (score >= 80) {
  grade = "B";
} else {
  grade = "F";
}

// Ternary
const label = isAdmin ? "Admin" : "User";

// switch
switch (day) {
  case "Monday":
    console.log("Start of week");
    break;
  case "Friday":
    console.log("End of week");
    break;
  default:
    console.log("Midweek");
}

// Loops
for (let i = 0; i < 5; i++) { }
while (condition) { }
do { } while (condition);

for (const item of array) { }       // iterates values (arrays, strings)
for (const key in object) { }       // iterates own enumerable keys (objects)
```

---

## Errors

```javascript
try {
  const data = JSON.parse(badInput); // throws SyntaxError
} catch (error) {
  console.error(error.name);    // "SyntaxError"
  console.error(error.message); // description
} finally {
  // always runs -- use for cleanup
}

// Throwing your own error
function divide(a, b) {
  if (b === 0) throw new Error("Division by zero");
  return a / b;
}
```

| Error Type | When it occurs |
|-----------|---------------|
| `SyntaxError` | Invalid JS syntax (often at parse time) |
| `ReferenceError` | Accessing an undeclared variable |
| `TypeError` | Wrong type for an operation (calling a non-function, null access) |
| `RangeError` | Value out of allowed range |

---

## Hoisting

```javascript
// var declarations are hoisted and initialized as undefined
console.log(x); // undefined (not ReferenceError)
var x = 5;

// Function declarations are fully hoisted
greet(); // works
function greet() { console.log("hi"); }

// let and const are hoisted but NOT initialized (Temporal Dead Zone)
console.log(y); // ReferenceError: Cannot access 'y' before initialization
let y = 10;

// Function expressions are NOT hoisted
sayHi(); // TypeError: sayHi is not a function
var sayHi = function() { console.log("hi"); };
```

---

## Summary
- Primitives are copied by value; reference types are copied by reference. This matters in React state.
- Always use `===`, not `==`. Implicit coercion causes hard-to-spot bugs.
- Prefer `const` by default, use `let` when you need to reassign, never use `var`.
- Wrap risky code in `try/catch`; use `finally` for cleanup.

## Additional Resources
- [MDN: JavaScript data types and data structures](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Data_structures)
- [MDN: Expressions and operators](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators)
- [MDN: Control flow and error handling](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Control_flow_and_error_handling)
