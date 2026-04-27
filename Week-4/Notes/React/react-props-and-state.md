# React Props & State

## Learning Objectives
- Pass data to components using props and type them with TypeScript interfaces
- Explain the difference between props and state
- Add local state to a component using `useState`
- Handle user events in React components
- Pass data and callbacks between parent and child components

## Why This Matters
Props and state are the two data mechanisms in React. Every non-trivial component uses both. Getting these right -- and knowing which one to reach for -- is the foundation of everything you build in React. On Wednesday you will extend this with `useEffect`, `useReducer`, and the Context API, all of which build on these concepts.

---

## Props

**Props** (short for *properties*) are the inputs to a component. They flow **down** from parent to child -- a parent passes data to a child via props, and the child receives them as a read-only argument.

Think of props as the function parameters of a component.

```tsx
// Define the shape of the props with a TypeScript interface
interface ProductCardProps {
  name: string;
  price: number;
  inStock: boolean;
  category?: string;   // optional prop
}

// Receive props as a single object parameter
function ProductCard({ name, price, inStock, category }: ProductCardProps) {
  return (
    <div className="card">
      <h2>{name}</h2>
      <p>${price.toFixed(2)}</p>
      {category && <span>{category}</span>}
      <span>{inStock ? "In Stock" : "Sold Out"}</span>
    </div>
  );
}

// Parent passes props as JSX attributes
function App() {
  return (
    <ProductCard
      name="Laptop"
      price={999.99}
      inStock={true}
      category="Electronics"
    />
  );
}
```

### Props Are Read-Only

A component must never modify its own props. This is a hard rule in React.

```tsx
function BadComponent({ count }: { count: number }) {
  count++;  // WRONG -- do not mutate props
  return <p>{count}</p>;
}
```

If a component needs to transform a prop value for display, compute a new variable:

```tsx
function PriceDisplay({ price, currency }: { price: number; currency: string }) {
  const formatted = `${currency}${price.toFixed(2)}`; // derived value -- fine
  return <p>{formatted}</p>;
}
```

### The `children` Prop

Components can receive JSX content between their tags via the special `children` prop:

```tsx
interface CardProps {
  title: string;
  children: React.ReactNode;  // anything renderable: JSX, string, array, null
}

function Card({ title, children }: CardProps) {
  return (
    <div className="card">
      <h2>{title}</h2>
      <div className="card-body">{children}</div>
    </div>
  );
}

// Usage
<Card title="My Products">
  <ProductList products={products} />
  <p>Showing {products.length} items</p>
</Card>
```

### Default Prop Values

Use JavaScript default parameter syntax:

```tsx
interface ButtonProps {
  label: string;
  variant?: "primary" | "secondary" | "danger";
  disabled?: boolean;
}

function Button({ label, variant = "primary", disabled = false }: ButtonProps) {
  return (
    <button className={`btn btn--${variant}`} disabled={disabled}>
      {label}
    </button>
  );
}
```

---

## State

**State** is local, mutable data managed *inside* a component. When state changes, React re-renders the component and updates the DOM to reflect the new values.

State is to a component what instance variables are to a C# class -- private data that the component owns and can change.

### `useState`

`useState` is the hook that adds state to a function component. We will cover hooks in depth on Wednesday -- today, focus on the pattern.

```tsx
import { useState } from "react";

function Counter() {
  // useState returns [currentValue, setterFunction]
  const [count, setCount] = useState(0); // 0 is the initial value

  return (
    <div>
      <p>Count: {count}</p>
      <button onClick={() => setCount(count + 1)}>Increment</button>
      <button onClick={() => setCount(0)}>Reset</button>
    </div>
  );
}
```

`useState<number>(0)` TypeScript infers the type from the initial value. Explicit annotation:

```tsx
const [count, setCount] = useState<number>(0);
const [name, setName] = useState<string>("");
const [product, setProduct] = useState<Product | null>(null);
```

### Props vs State

| | Props | State |
|--|-------|-------|
| Owned by | Parent (passed in) | The component itself |
| Mutable? | No -- read-only | Yes -- via the setter |
| Causes re-render? | When parent re-renders | When setter is called |
| Purpose | Receive data from outside | Track data that changes inside |

A quick decision guide:
- Does this data come from outside the component? → **Props**
- Does this data change due to user interaction or internal logic? → **State**
- Is this a value computed from props or state? → **Derived value** (just a variable, no state needed)

---

## Event Handling

React wraps native DOM events in **SyntheticEvent** -- a cross-browser wrapper with the same API as native events.

```tsx
// Inline arrow function (simple cases)
<button onClick={() => console.log("clicked")}>Click</button>

// Named handler (preferred for complex logic)
function SearchBar() {
  const [query, setQuery] = useState("");

  function handleChange(event: React.ChangeEvent<HTMLInputElement>) {
    setQuery(event.target.value);
  }

  function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault(); // prevent page reload
    console.log("Search for:", query);
  }

  return (
    <form onSubmit={handleSubmit}>
      <input
        type="text"
        value={query}
        onChange={handleChange}
        placeholder="Search products..."
      />
      <button type="submit">Search</button>
    </form>
  );
}
```

### Common Event Types

| Handler prop | TypeScript type | Notes |
|-------------|-----------------|-------|
| `onClick` | `React.MouseEvent<HTMLButtonElement>` | Clicks on any element |
| `onChange` | `React.ChangeEvent<HTMLInputElement>` | Input value changes |
| `onSubmit` | `React.FormEvent<HTMLFormElement>` | Form submission |
| `onKeyDown` | `React.KeyboardEvent<HTMLInputElement>` | Key presses |
| `onFocus` / `onBlur` | `React.FocusEvent<HTMLInputElement>` | Focus changes |

For quick inline handlers you can omit the explicit type; TypeScript infers it:

```tsx
<input onChange={(e) => setName(e.target.value)} />
//                  ^ e is inferred as React.ChangeEvent<HTMLInputElement>
```

---

## Nested Components and Parent-Child Communication

### Data Down: Parent Passes Props to Child

```tsx
interface Product {
  id: number;
  name: string;
  price: number;
}

function ProductList() {
  const products: Product[] = [
    { id: 1, name: "Laptop", price: 999 },
    { id: 2, name: "Monitor", price: 349 },
  ];

  return (
    <ul>
      {products.map(p => (
        <ProductItem key={p.id} product={p} />
      ))}
    </ul>
  );
}

function ProductItem({ product }: { product: Product }) {
  return (
    <li>{product.name} -- ${product.price}</li>
  );
}
```

### Events Up: Child Notifies Parent via Callback Props

When a child needs to tell a parent something happened, the parent passes a **callback function** as a prop. The child calls it.

```tsx
interface ProductItemProps {
  product: Product;
  onSelect: (product: Product) => void; // callback prop
}

function ProductItem({ product, onSelect }: ProductItemProps) {
  return (
    <li>
      {product.name}
      <button onClick={() => onSelect(product)}>View Details</button>
    </li>
  );
}

function ProductList() {
  const [selected, setSelected] = useState<Product | null>(null);

  const products: Product[] = [
    { id: 1, name: "Laptop", price: 999 },
    { id: 2, name: "Monitor", price: 349 },
  ];

  return (
    <div>
      {selected && (
        <div className="detail-panel">
          <h2>{selected.name}</h2>
          <p>${selected.price}</p>
          <button onClick={() => setSelected(null)}>Close</button>
        </div>
      )}
      <ul>
        {products.map(p => (
          <ProductItem
            key={p.id}
            product={p}
            onSelect={setSelected}      // pass the state setter as the callback
          />
        ))}
      </ul>
    </div>
  );
}
```

This pattern -- **data flows down via props, events flow up via callbacks** -- is the core of React's one-way data flow. You will formalize this on Wednesday when you learn about lifting state.

---

## Lifting State

When two sibling components need to share the same data, the state lives in their **closest common ancestor**, and both components receive it as props.

```tsx
// Both SearchBar and ProductList need the search query
function App() {
  const [query, setQuery] = useState(""); // "lifted" to the parent

  const allProducts = [...];
  const filtered = allProducts.filter(p =>
    p.name.toLowerCase().includes(query.toLowerCase())
  );

  return (
    <div>
      <SearchBar query={query} onQueryChange={setQuery} />
      <ProductList products={filtered} />
    </div>
  );
}

function SearchBar({
  query,
  onQueryChange,
}: {
  query: string;
  onQueryChange: (q: string) => void;
}) {
  return (
    <input
      value={query}
      onChange={(e) => onQueryChange(e.target.value)}
      placeholder="Filter products..."
    />
  );
}

function ProductList({ products }: { products: Product[] }) {
  return (
    <ul>
      {products.map(p => <li key={p.id}>{p.name}</li>)}
    </ul>
  );
}
```

`App` owns the truth. `SearchBar` reads and updates it. `ProductList` reads the filtered result. Neither child holds state the other needs.

We will cover state management patterns in more depth on Wednesday, including the Context API for distributing state without passing props through many layers.

---

## Summary
- Props flow down from parent to child and are read-only inside the receiving component.
- State is local mutable data; call the setter to update it and trigger a re-render.
- `useState(initialValue)` returns `[currentValue, setter]`. TypeScript infers the type.
- Event handlers are camelCase props (e.g., `onClick`, `onChange`). Call `event.preventDefault()` to block browser defaults.
- Data flows down via props; events flow up via callback props. This is one-way data flow.
- Lift state to the closest common ancestor when siblings need to share it.

## Additional Resources
- [React: Passing Props to a Component](https://react.dev/learn/passing-props-to-a-component)
- [React: State: A Component's Memory](https://react.dev/learn/state-a-components-memory)
- [React: Sharing State Between Components](https://react.dev/learn/sharing-state-between-components)
