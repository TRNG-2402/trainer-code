# Async JavaScript

## Learning Objectives
- Parse and stringify JSON data
- Create and chain Promises
- Fetch data from an API using the Fetch API
- Write asynchronous code using `async`/`await` with error handling

## Why This Matters
Every React app talks to a backend API. On Tuesday you will call `fetch()` inside a React component, and on Wednesday you will wrap it in a `useEffect`. This document is your reference for the underlying async machinery those abstractions depend on.

> **Note:** This is a review reference for Week 3.

---

## JSON

JSON (JavaScript Object Notation) is the standard data format for API communication. It looks like a JS object literal but with stricter rules.

```json
{
  "id": 1,
  "name": "Laptop",
  "price": 999.99,
  "inStock": true,
  "tags": ["electronics", "computers"],
  "specs": {
    "ram": "16GB",
    "storage": "512GB"
  }
}
```

**JSON rules (vs JS object literals):**
- Keys must be double-quoted strings.
- Values can be: string, number, boolean, null, array, object.
- No functions, no `undefined`, no comments, no trailing commas.

```javascript
// JS object → JSON string
const product = { id: 1, name: "Laptop", price: 999.99 };
const json = JSON.stringify(product);
// '{"id":1,"name":"Laptop","price":999.99}'

// Pretty print
JSON.stringify(product, null, 2);

// JSON string → JS object
const parsed = JSON.parse('{"id":1,"name":"Laptop"}');
console.log(parsed.name); // "Laptop"

// Common pitfall: JSON.parse throws on invalid JSON
try {
  JSON.parse("not json");
} catch (e) {
  console.error("Invalid JSON:", e.message);
}
```

---

## Promises

A `Promise` represents a value that will be available in the future (or an error if something went wrong).

```
Pending → Fulfilled (resolved with a value)
        → Rejected  (rejected with a reason/error)
```

### Creating a Promise

```javascript
const delay = (ms) =>
  new Promise((resolve, reject) => {
    if (ms < 0) reject(new Error("Negative delay"));
    setTimeout(() => resolve(`Done after ${ms}ms`), ms);
  });
```

### Consuming with `.then` / `.catch` / `.finally`

```javascript
delay(1000)
  .then(message => {
    console.log(message); // "Done after 1000ms"
    return message.toUpperCase(); // value for next .then
  })
  .then(upper => console.log(upper)) // "DONE AFTER 1000MS"
  .catch(error => console.error(error.message)) // handles any rejection above
  .finally(() => console.log("Always runs"));
```

### Composing Promises

```javascript
// Run in parallel, wait for all to resolve
const [users, products] = await Promise.all([
  fetch("/api/users").then(r => r.json()),
  fetch("/api/products").then(r => r.json()),
]);

// First to settle (resolve or reject) wins
const result = await Promise.race([fastRequest, slowRequest]);

// Like Promise.all but never rejects -- each result has { status, value/reason }
const results = await Promise.allSettled([p1, p2, p3]);
```

---

## Fetch API

`fetch` is the browser-native way to make HTTP requests. It returns a Promise.

### GET Request

```javascript
fetch("https://jsonplaceholder.typicode.com/products/1")
  .then(response => {
    if (!response.ok) {
      throw new Error(`HTTP error: ${response.status}`);
    }
    return response.json(); // another Promise
  })
  .then(data => console.log(data))
  .catch(error => console.error("Request failed:", error));
```

**Important:** `fetch` only rejects on network failure. A `404` or `500` response is still a resolved Promise. Always check `response.ok`.

### POST Request

```javascript
fetch("https://jsonplaceholder.typicode.com/posts", {
  method: "POST",
  headers: {
    "Content-Type": "application/json",
    "Authorization": `Bearer ${token}`,
  },
  body: JSON.stringify({ title: "New Post", body: "Content", userId: 1 }),
})
  .then(res => res.json())
  .then(created => console.log(created));
```

---

## `async` / `await`

`async`/`await` is syntax sugar over Promises -- the result is still a Promise under the hood. It makes async code read sequentially.

```javascript
async function getProduct(id) {
  const response = await fetch(`/api/products/${id}`);
  if (!response.ok) throw new Error(`Not found: ${response.status}`);
  return response.json();
}
```

### Error Handling

```javascript
async function loadDashboard() {
  try {
    const [user, products] = await Promise.all([
      fetchUser(),
      fetchProducts(),
    ]);
    renderDashboard(user, products);
  } catch (error) {
    showError(error.message);
  } finally {
    hideLoadingSpinner();
  }
}
```

### Async in Loops

```javascript
// Sequential -- each waits for the previous
for (const id of productIds) {
  const product = await fetchProduct(id); // one at a time
  console.log(product);
}

// Parallel -- all fire at once, wait for all to finish
const products = await Promise.all(
  productIds.map(id => fetchProduct(id))
);
```

### Common Mistakes

```javascript
// WRONG: forEach does not wait for async callbacks
productIds.forEach(async (id) => {
  await fetchProduct(id); // these run concurrently and forEach returns void
});

// CORRECT: use for...of or Promise.all
for (const id of productIds) {
  await fetchProduct(id);
}
```

---

## Putting It Together: Full Fetch Pattern

This is the pattern you will use in `useEffect` on Wednesday:

```javascript
async function fetchProducts(signal) {
  const response = await fetch("/api/products", { signal }); // signal for AbortController
  if (!response.ok) throw new Error(`Server error: ${response.status}`);
  return response.json();
}

// Usage
const controller = new AbortController();

fetchProducts(controller.signal)
  .then(data => setProducts(data))
  .catch(err => {
    if (err.name === "AbortError") return; // ignore cancellation
    setError(err.message);
  });

// Cancel the request (e.g., component unmounts)
controller.abort();
```

---

## Summary
- JSON: use `JSON.parse` / `JSON.stringify`. Always wrap `parse` in try/catch.
- Promises: three states (pending, fulfilled, rejected). Chain with `.then` / `.catch` / `.finally`.
- `fetch` always resolves unless the network fails -- check `response.ok` for HTTP errors.
- `async`/`await` makes Promises sequential-looking; wrap in `try/catch` for error handling.

## Additional Resources
- [MDN: Using Promises](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Using_promises)
- [MDN: Fetch API](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API/Using_Fetch)
- [MDN: async function](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/async_function)
