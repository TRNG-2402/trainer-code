# JavaScript OOP & Classes

## Learning Objectives
- Explain the four OOP pillars as they apply in JavaScript
- Predict what `this` refers to in different calling contexts
- Write ES6 classes with constructors, methods, and inheritance

## Why This Matters
React class components (less common today but still present in legacy code) and many JS libraries use class syntax. More importantly, the `this` keyword behavior in regular functions vs arrow functions is a frequent source of bugs in event handlers -- a topic you will encounter in both vanilla JS and React.

> **Note:** This is a review reference for Week 3.

---

## OOP Pillars in JavaScript

| Pillar | Meaning | JS mechanism |
|--------|---------|-------------|
| **Encapsulation** | Bundle data and behavior; hide implementation details | Classes + private fields (`#field`) |
| **Inheritance** | Child class reuses and extends parent behavior | `extends` / `super` |
| **Polymorphism** | Same interface, different implementations | Method overriding in subclasses |
| **Abstraction** | Expose only what callers need | Methods as the public API; private fields as internals |

---

## The `this` Keyword

`this` refers to the object that is the *execution context* of the current function. The rules:

### Rule 1 -- Default Binding
In non-strict mode, `this` in a free function call is `globalThis` (the `window` in browsers). In strict mode it is `undefined`.

```javascript
"use strict";
function show() {
  console.log(this); // undefined
}
show();
```

### Rule 2 -- Implicit Binding
When a function is called as a method of an object, `this` is that object.

```javascript
const cart = {
  total: 0,
  add(price) {
    this.total += price; // this = cart
  },
};
cart.add(99);
console.log(cart.total); // 99
```

### Rule 3 -- Explicit Binding
`call`, `apply`, and `bind` let you set `this` explicitly.

```javascript
function greet(greeting) {
  return `${greeting}, ${this.name}`;
}
const user = { name: "Alice" };

greet.call(user, "Hello");         // "Hello, Alice"
greet.apply(user, ["Hi"]);         // "Hi, Alice"
const boundGreet = greet.bind(user);
boundGreet("Hey");                 // "Hey, Alice"
```

### Rule 4 -- `new` Binding
When a function is called with `new`, `this` is the newly created object.

```javascript
function Product(name, price) {
  this.name = name;
  this.price = price;
}
const p = new Product("Laptop", 999);
console.log(p.name); // "Laptop"
```

### Arrow Functions -- No `this`

Arrow functions do NOT have their own `this`. They capture `this` from the enclosing lexical scope at the time they are defined.

```javascript
class Timer {
  constructor() {
    this.seconds = 0;
  }

  start() {
    // Arrow: this is the Timer instance
    setInterval(() => {
      this.seconds++;
    }, 1000);

    // Regular function: this would be undefined (strict) or globalThis
    // setInterval(function() { this.seconds++; }, 1000); // WRONG
  }
}
```

---

## ES6 Classes

Classes are syntactic sugar over JavaScript's prototype-based inheritance.

```javascript
class Product {
  // Private field (only accessible inside the class)
  #id;

  constructor(id, name, price) {
    this.#id = id;
    this.name = name;
    this.price = price;
  }

  // Instance method
  getLabel() {
    return `${this.name} -- $${this.price.toFixed(2)}`;
  }

  // Getter
  get id() {
    return this.#id;
  }

  // Static method (called on the class, not an instance)
  static fromJSON(json) {
    const { id, name, price } = JSON.parse(json);
    return new Product(id, name, price);
  }
}

const laptop = new Product(1, "Laptop", 999.99);
console.log(laptop.getLabel());    // "Laptop -- $999.99"
console.log(laptop.id);            // 1
// console.log(laptop.#id);        // SyntaxError: private field

const p2 = Product.fromJSON('{"id":2,"name":"Monitor","price":349}');
```

---

## Inheritance with `extends` and `super`

```javascript
class DigitalProduct extends Product {
  constructor(id, name, price, downloadUrl) {
    super(id, name, price); // must call super before using this
    this.downloadUrl = downloadUrl;
  }

  // Override parent method (polymorphism)
  getLabel() {
    return `${super.getLabel()} [Digital]`;
  }

  download() {
    console.log(`Downloading from ${this.downloadUrl}`);
  }
}

const ebook = new DigitalProduct(3, "JS Handbook", 29, "https://example.com/dl");
console.log(ebook.getLabel()); // "JS Handbook -- $29.00 [Digital]"
ebook.download();

// instanceof checks the prototype chain
console.log(ebook instanceof DigitalProduct); // true
console.log(ebook instanceof Product);        // true
```

---

## Prototypal Inheritance (Conceptual)

Behind the scenes, `class` syntax creates a constructor function and links objects via the prototype chain. When you access `ebook.getLabel()`, JavaScript looks up the chain: `ebook` object → `DigitalProduct.prototype` → `Product.prototype` → `Object.prototype` → `null`. ES6 classes make this readable without exposing the prototype machinery. You will not need to manipulate prototypes directly in this course.

---

## Summary
- OOP in JS: encapsulation via private fields, inheritance via `extends`, polymorphism via method overriding.
- `this` depends on HOW a function is called, not where it is defined (except arrow functions, which capture lexically).
- Arrow functions have no `this` -- use them for callbacks inside class methods.
- `super(args)` must be called in a subclass constructor before accessing `this`.

## Additional Resources
- [MDN: Classes](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Classes)
- [MDN: `this`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/this)
- [MDN: Inheritance and the prototype chain](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Inheritance_and_the_prototype_chain)
