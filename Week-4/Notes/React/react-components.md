# React Components

## Learning Objectives
- Write a function component that returns JSX
- Explain why function components are preferred over class components
- Describe how the virtual DOM enables efficient updates
- Render a list with `.map()` and explain why the `key` prop is required
- Choose a correct `key` value for a given data set

## Why This Matters
Components are the atomic unit of React. Everything you build is a component tree. Understanding the virtual DOM tells you *why* React re-renders when it does -- which directly informs how to write efficient code in Week 4 when you optimize with `useMemo` and `useCallback`.

---

## What Is a Component?

A React component is a **function that accepts props (input) and returns JSX (output)**.

```tsx
function Greeting() {
  return <h1>Hello, World!</h1>;
}
```

That is the entire definition. No class, no lifecycle methods, no template file separate from logic.

Components compose into trees:

```tsx
function App() {
  return (
    <main>
      <Header />
      <ProductList />
      <Footer />
    </main>
  );
}
```

When React renders `<App />`, it calls `App()`, which returns JSX that references `Header`, `ProductList`, and `Footer`. React then calls each of those, building the full UI tree.

---

## Function Components

```tsx
// Minimal component
function ProductCard() {
  return (
    <div className="card">
      <h2>Laptop</h2>
      <p>$999.99</p>
    </div>
  );
}

// With TypeScript: explicit return type annotation is optional but good practice
function ProductCard(): JSX.Element {
  return (
    <div className="card">
      <h2>Laptop</h2>
    </div>
  );
}

// Arrow function style (common for small, one-off components)
const Badge = () => <span className="badge">New</span>;
```

### Naming Convention

Component names **must** start with a capital letter. This is how React distinguishes components from HTML elements:

```jsx
<div>      // lowercase → native HTML element
<ProductCard />  // uppercase → React component
```

---

## Function Components vs Class Components

Before React 16.8 (2019), stateful components had to be written as classes. Hooks replaced that need. You will still encounter class components in legacy code, so it is worth recognizing the pattern.

```tsx
// Class component (legacy -- still valid, but not used in new code)
import { Component } from "react";

class Counter extends Component<{}, { count: number }> {
  constructor(props: {}) {
    super(props);
    this.state = { count: 0 };
  }

  increment = () => this.setState({ count: this.state.count + 1 });

  render() {
    return (
      <button onClick={this.increment}>{this.state.count}</button>
    );
  }
}

// Function component equivalent (modern)
import { useState } from "react";

function Counter() {
  const [count, setCount] = useState(0);
  return (
    <button onClick={() => setCount(count + 1)}>{count}</button>
  );
}
```

| | Function Component | Class Component |
|--|-------------------|----------------|
| State | `useState` hook | `this.state` |
| Lifecycle | `useEffect` hook | `componentDidMount`, `componentDidUpdate`, etc. |
| `this` binding | Not needed | Required (common source of bugs) |
| Boilerplate | Minimal | Verbose |
| Testability | Easy -- pure function | Harder -- instance methods |
| Performance | Same | Same (React optimizes both) |

**Use function components for all new code.**

---

## The Virtual DOM

The virtual DOM is a lightweight JavaScript object representation of the actual DOM tree. React keeps a virtual DOM in memory and uses it to minimize real DOM updates, which are expensive.

### How It Works

1. **Initial render:** React calls your component functions, builds a virtual DOM tree, and commits it to the real DOM.
2. **State changes:** When state updates, React re-runs the affected component function and builds a **new** virtual DOM tree.
3. **Diffing (reconciliation):** React compares the new virtual DOM against the previous one, identifying the minimal set of changes.
4. **Commit:** React applies only those changes to the real DOM.

```
State changes
    ↓
React calls component function → new virtual DOM tree
    ↓
React diffs old virtual DOM vs new virtual DOM
    ↓
React updates only the changed real DOM nodes
```

### Why This Matters

Direct DOM manipulation is slow because the browser must recalculate layout, styles, and repaint. React batches and minimizes DOM writes. For most apps, this happens so fast it is invisible -- but it is also why React can efficiently handle lists of hundreds of items.

### Reconciliation Rules

React uses these heuristics during diffing:
- Elements of a **different type** (e.g., `<div>` becomes `<span>`) cause React to tear down the old tree and build a new one.
- Elements of the **same type** cause React to update only the changed attributes.
- Lists use the `key` prop (see below) to match old items to new items.

---

## Rendering Lists

To render a list of items, use JavaScript's `.map()` inside JSX:

```tsx
interface Product {
  id: number;
  name: string;
  price: number;
}

const products: Product[] = [
  { id: 1, name: "Laptop",   price: 999 },
  { id: 2, name: "Monitor",  price: 349 },
  { id: 3, name: "Keyboard", price: 79  },
];

function ProductList() {
  return (
    <ul>
      {products.map(product => (
        <li key={product.id}>
          {product.name} -- ${product.price}
        </li>
      ))}
    </ul>
  );
}
```

The `map` returns an array of JSX elements. React renders each element in order.

---

## The `key` Prop

The `key` prop is a **special React attribute** that helps React identify which items in a list have changed, been added, or been removed during reconciliation.

```tsx
// Without key: React warns and cannot efficiently diff the list
{products.map(p => <li>{p.name}</li>)}    // Warning: each child should have a key

// With key: React can track each item
{products.map(p => <li key={p.id}>{p.name}</li>)}
```

### Why Keys Matter

Consider a list of 3 items. You delete the first one. Without keys:
- React does not know which item was removed. It updates the text of items 1 and 2 to match items 2 and 3 respectively, then deletes item 3. Three DOM operations.

With keys:
- React knows item `key=1` is gone. One DOM removal. Zero updates needed.

For a form field inside a list item, this is even more important: without the right key, React might reuse a DOM node (with its focused state, typed text, etc.) for the wrong item.

### Rules for Keys

```tsx
// GOOD: stable, unique ID from data
{products.map(p => <ProductCard key={p.id} product={p} />)}

// GOOD: unique string
{tabs.map(tab => <Tab key={tab.slug} tab={tab} />)}

// BAD: array index (acceptable only for static, never-reordered lists)
{products.map((p, index) => <li key={index}>{p.name}</li>)}
// Problem: if you sort or insert items, indices change and React diffs incorrectly

// BAD: random key (generates a new key every render -- defeats the purpose)
{products.map(p => <li key={Math.random()}>{p.name}</li>)}
```

**Rule:** Keys must be stable (same value across re-renders), unique among siblings, and ideally come from your data (database IDs are perfect).

Keys only need to be unique **within the same list** -- the same key can be reused in a different list on the page.

---

## Composing Components

Components are composable -- build small ones and assemble them into larger ones:

```tsx
interface Product {
  id: number;
  name: string;
  price: number;
  inStock: boolean;
}

function StockBadge({ inStock }: { inStock: boolean }) {
  return (
    <span className={inStock ? "badge--green" : "badge--red"}>
      {inStock ? "In Stock" : "Sold Out"}
    </span>
  );
}

function ProductCard({ product }: { product: Product }) {
  return (
    <div className="card">
      <h2>{product.name}</h2>
      <p>${product.price.toFixed(2)}</p>
      <StockBadge inStock={product.inStock} />
    </div>
  );
}

function ProductList({ products }: { products: Product[] }) {
  return (
    <div className="product-grid">
      {products.map(product => (
        <ProductCard key={product.id} product={product} />
      ))}
    </div>
  );
}
```

Notice: each component has a single, well-defined responsibility. `StockBadge` only knows about `inStock`. `ProductCard` composes `StockBadge`. `ProductList` composes `ProductCard`s.

---

## Summary
- A React component is a function that takes props and returns JSX.
- Always use function components. Class components exist in legacy code only.
- The virtual DOM lets React minimize expensive real DOM updates through diffing (reconciliation).
- Use `.map()` to render lists. Every list item must have a `key` prop.
- Keys must be stable and unique among siblings. Use data IDs, never array indices on dynamic lists.

## Additional Resources
- [React: Your First Component](https://react.dev/learn/your-first-component)
- [React: Rendering Lists](https://react.dev/learn/rendering-lists)
- [React: Preserving and Resetting State](https://react.dev/learn/preserving-and-resetting-state)
