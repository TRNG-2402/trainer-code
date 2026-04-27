# JavaScript Functions & Objects

## Learning Objectives
- Write functions using declarations, expressions, and arrow syntax
- Use default parameters, template literals, and destructuring
- Apply array iteration methods (`map`, `filter`, `reduce`, and others)
- Use spread and rest operators in functions and data manipulation

## Why This Matters
React component props, state transformations, and event handlers are all functions. Array methods like `map` are used to render lists. Destructuring and spread are used constantly in React patterns -- you will see them on Tuesday.

> **Note:** This is a review reference for Week 3.

---

## Function Syntax Comparison

```javascript
// Declaration -- hoisted, has its own `this`
function add(a, b) {
  return a + b;
}

// Expression -- NOT hoisted, has its own `this`
const multiply = function(a, b) {
  return a * b;
};

// Arrow function -- NOT hoisted, inherits `this` from enclosing scope
const subtract = (a, b) => a - b;       // implicit return when single expression
const square = (n) => n * n;
const greet = () => "Hello";

// Arrow with body -- explicit return required
const formatName = (first, last) => {
  const full = `${first} ${last}`;
  return full.trim();
};
```

**When to use which:**
- Arrow functions for callbacks, array methods, React event handlers.
- Function declarations for top-level utility functions (predictable hoisting).
- Never use arrow functions as object methods where you need `this` to refer to the object.

---

## Default Parameters

```javascript
function greet(name = "World") {
  return `Hello, ${name}!`;
}

greet("Alice"); // "Hello, Alice!"
greet();        // "Hello, World!"

// Default can be an expression
function createUser(name, role = "viewer", createdAt = new Date()) {
  return { name, role, createdAt };
}
```

---

## Template Literals

```javascript
const name = "Alice";
const score = 95;

// Multi-line strings
const message = `
  Hello, ${name}!
  Your score is ${score}.
  Grade: ${score >= 90 ? "A" : "B"}
`;

// Tagged templates (advanced -- for awareness)
const safeHtml = String.raw`<div>${name}</div>`; // raw string, no escape processing
```

---

## Arrays and Iteration Methods

```javascript
const products = [
  { id: 1, name: "Laptop",  price: 999,  inStock: true  },
  { id: 2, name: "Monitor", price: 349,  inStock: false },
  { id: 3, name: "Keyboard",price: 79,   inStock: true  },
];

// map -- transform each element, returns new array (same length)
const names = products.map(p => p.name);
// ["Laptop", "Monitor", "Keyboard"]

// filter -- keep elements matching predicate, returns new array (same or shorter)
const available = products.filter(p => p.inStock);
// [{ id: 1, ...}, { id: 3, ...}]

// reduce -- accumulate to a single value
const total = products.reduce((sum, p) => sum + p.price, 0);
// 1427

// find -- first element matching predicate (or undefined)
const laptop = products.find(p => p.id === 1);

// findIndex -- index of first match (or -1)
const idx = products.findIndex(p => p.name === "Monitor");

// some -- true if ANY element matches
const hasStock = products.some(p => p.inStock); // true

// every -- true if ALL elements match
const allAvailable = products.every(p => p.inStock); // false

// forEach -- side effects only, returns undefined
products.forEach(p => console.log(p.name));

// includes -- check primitive value membership
[1, 2, 3].includes(2); // true

// flat / flatMap
[[1, 2], [3, 4]].flat();              // [1, 2, 3, 4]
products.flatMap(p => [p.id, p.name]); // [1, "Laptop", 2, "Monitor", ...]

// sort (mutates -- clone first!)
const sorted = [...products].sort((a, b) => a.price - b.price);
```

---

## Object Literals

```javascript
const name = "Alice";
const role = "admin";

// Shorthand property (key === variable name)
const user = { name, role };
// Same as: { name: name, role: role }

// Computed keys
const field = "email";
const contact = { [field]: "alice@example.com" };
// { email: "alice@example.com" }

// Methods
const calculator = {
  value: 0,
  add(n) { this.value += n; return this; },
  result() { return this.value; },
};
calculator.add(5).add(3).result(); // 8

// Spread to merge / clone
const defaults = { theme: "light", lang: "en" };
const userPrefs = { lang: "es" };
const merged = { ...defaults, ...userPrefs };
// { theme: "light", lang: "es" }  -- userPrefs wins on conflict
```

---

## Destructuring

```javascript
// Object destructuring
const { name: userName, role = "viewer" } = user;
//      ^ rename         ^ default value

// Nested
const { address: { city } } = { address: { city: "Austin" } };

// Array destructuring
const [first, second, ...rest] = [1, 2, 3, 4, 5];
// first = 1, second = 2, rest = [3, 4, 5]

// Skip elements
const [, , third] = [1, 2, 3];
// third = 3

// In function parameters
function displayProduct({ name, price, inStock = true }) {
  return `${name}: $${price} (${inStock ? "in stock" : "sold out"})`;
}
displayProduct(products[0]);
```

---

## Spread and Rest Operators

Both use `...` -- context determines meaning.

```javascript
// SPREAD: expand an iterable into individual elements

// Arrays
const a = [1, 2, 3];
const b = [4, 5, 6];
const combined = [...a, ...b];        // [1, 2, 3, 4, 5, 6]
const copy = [...a];                  // shallow clone

// Objects
const updated = { ...product, price: 199 }; // clone + override

// Function call
Math.max(...a);                        // Math.max(1, 2, 3) = 3

// REST: collect remaining arguments into an array

// In function parameters
function sum(...numbers) {
  return numbers.reduce((acc, n) => acc + n, 0);
}
sum(1, 2, 3, 4); // 10

// In destructuring
const [head, ...tail] = [1, 2, 3, 4];
// head = 1, tail = [2, 3, 4]
```

**React pattern:** Spread is used constantly to create new state objects:
```javascript
// Correct: new object reference triggers re-render
setUser({ ...user, name: "Bob" });

// Wrong: mutating the existing object -- React will not re-render
user.name = "Bob";
setUser(user);
```

---

## Summary
- Arrow functions inherit `this`; regular functions define their own. Use arrows for callbacks.
- `map`, `filter`, `reduce` are the workhorses of React list rendering and state transformation.
- Spread creates shallow copies; use it to update objects and arrays immutably.
- Rest collects arguments; use it for variadic functions.

## Additional Resources
- [MDN: Arrow functions](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Functions/Arrow_functions)
- [MDN: Array methods](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array)
- [MDN: Destructuring assignment](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Destructuring_assignment)
