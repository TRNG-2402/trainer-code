# React State Management

## Learning Objectives
- Explain why React requires immutable state updates
- Write correct immutable update patterns for objects, arrays, and nested structures
- Apply the lifting state pattern to share state between sibling components
- Describe React's one-way data flow and why two-way binding is avoided
- Recognize when a dedicated state management library would be appropriate

## Why This Matters
Mutating state directly is the single most common source of bugs for React beginners -- the component does not re-render, and the developer cannot understand why. Getting immutability right makes your components predictable. Understanding one-way data flow helps you architect component trees that are easy to debug and test.

---

## Immutability in React

React detects state changes by **reference equality**. When you call `setState`, React compares the new state reference to the old one. If the reference is the same, React assumes nothing changed and skips the re-render.

```tsx
// WRONG: same object reference -- React skips re-render
const [user, setUser] = useState({ name: "Alice", role: "admin" });

user.name = "Bob";    // mutating the existing object
setUser(user);        // same reference as before → React does NOT re-render
```

```tsx
// CORRECT: new object reference -- React sees the change
setUser({ ...user, name: "Bob" }); // spread creates a new object → re-renders
```

This is the same principle behind C#'s `record` types -- treat values as immutable and produce new copies on change.

---

## Immutable Update Patterns

### Updating a Top-Level Field

```tsx
// State: { name: string; price: number; inStock: boolean }
setProduct(prev => ({ ...prev, price: 599 }));
```

### Updating a Nested Field

```tsx
// State: { user: { name: string; address: { city: string } } }
setProfile(prev => ({
  ...prev,
  user: {
    ...prev.user,
    address: {
      ...prev.user.address,
      city: "Austin",
    },
  },
}));
```

If your state nesting goes more than two levels deep, consider flattening it or using `useReducer`.

### Adding to an Array

```tsx
setItems(prev => [...prev, newItem]);
```

### Removing from an Array

```tsx
setItems(prev => prev.filter(item => item.id !== targetId));
```

### Updating One Item in an Array

```tsx
setItems(prev =>
  prev.map(item =>
    item.id === targetId ? { ...item, quantity: item.quantity + 1 } : item
  )
);
```

### Reordering an Array

```tsx
// Move item at index `from` to index `to`
setItems(prev => {
  const next = [...prev];          // clone first
  const [removed] = next.splice(from, 1);
  next.splice(to, 0, removed);
  return next;
});
```

**Always clone before sorting or splicing:**

```tsx
// WRONG: Array.sort mutates in place
setItems(prev => prev.sort((a, b) => a.name.localeCompare(b.name)));

// CORRECT: clone, then sort
setItems(prev => [...prev].sort((a, b) => a.name.localeCompare(b.name)));
```

---

## Lifting State

When two or more sibling components need to read or write the same piece of data, that state must live in their **closest common ancestor**. The ancestor holds the state and passes it down as props.

### The Problem: Siblings Cannot Share State Directly

```tsx
// Siblings cannot talk to each other directly
function SearchBar() {
  const [query, setQuery] = useState(""); // SearchBar owns query
  // ...
}

function ProductList() {
  // Cannot read query from SearchBar -- different component instances
}
```

### The Solution: Lift State to the Parent

```tsx
function App() {
  const [query, setQuery] = useState(""); // lifted to common ancestor

  return (
    <>
      <SearchBar query={query} onQueryChange={setQuery} />
      <ProductList filterQuery={query} />
    </>
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
      placeholder="Search..."
    />
  );
}

function ProductList({ filterQuery }: { filterQuery: string }) {
  const allProducts: Product[] = useSomeDataSource();
  const filtered = allProducts.filter(p =>
    p.name.toLowerCase().includes(filterQuery.toLowerCase())
  );
  return <ul>{filtered.map(p => <li key={p.id}>{p.name}</li>)}</ul>;
}
```

### How Far to Lift?

Lift state to the **lowest ancestor** that needs it. Lifting higher than necessary causes unnecessary re-renders of components that do not care about that state.

---

## One-Way Data Flow

React enforces a **one-way data flow**: data flows **down** through props, events flow **up** through callbacks.

```
     App (owns state)
      ↓ props        ↑ callbacks
   SearchBar       ProductList
```

No component can reach up and modify a parent's state directly. This constraint makes the application predictable: you can always trace where state lives and which component can change it.

### Why Not Two-Way Binding?

Frameworks like Angular support two-way binding -- a template variable automatically reflects changes in both directions. This is convenient for simple forms but creates hidden dependencies:

```
Two-way binding: component A mutates data → component B changes → triggers A again → ...
```

With two-way binding, debugging state changes means tracing a web of bidirectional connections. In React:

```
State change → explicit setter call → re-render → clear data flow
```

The callback pattern feels more verbose, but every state change is explicit and traceable.

---

## Derived State

Do not put data into state if it can be computed from existing state or props. Derived state leads to synchronization bugs (the computed value gets out of sync with the source).

```tsx
// WRONG: totalPrice in state is redundant and can go out of sync
const [items, setItems] = useState<CartItem[]>([]);
const [totalPrice, setTotalPrice] = useState(0); // must be manually kept in sync

// CORRECT: compute on render -- always in sync, no extra setState
const [items, setItems] = useState<CartItem[]>([]);
const totalPrice = items.reduce((sum, i) => sum + i.product.price * i.quantity, 0);
```

If derived computation is expensive, use `useMemo` to cache it (covered in Week 4).

---

## Alternatives for State Management

As applications grow, passing props through many layers (prop drilling) becomes painful. The solutions, in order of complexity:

### 1. Context API (covered today)

React's built-in solution for distributing state across a component tree without prop drilling. Best for low-frequency updates: theme, current user, locale.

### 2. Zustand

Minimal third-party state library. A global store accessed with hooks -- no boilerplate, no Provider required.

```tsx
// Conceptual -- not hands-on this week
import { create } from "zustand";

const useCartStore = create((set) => ({
  items: [],
  addItem: (product) => set(state => ({ items: [...state.items, product] })),
}));

function ProductCard({ product }) {
  const addItem = useCartStore(state => state.addItem);
  return <button onClick={() => addItem(product)}>Add to Cart</button>;
}
```

### 3. Redux Toolkit

The enterprise-standard state library. Powerful but more setup. Best for large teams with complex shared state, where strict patterns matter for maintainability.

### 4. Jotai / Recoil

Atom-based state -- individual pieces of state (atoms) can be read/written anywhere. Good for fine-grained reactivity.

### When to Reach for a Library

| Scenario | Approach |
|---------|---------|
| Simple page with a few components | `useState` + lifting |
| Auth state, theme, current user shared across the app | Context API |
| Complex domain state (cart, order flow, multi-step forms) | `useReducer` + Context, or Zustand |
| Large team, complex app, need strict patterns | Redux Toolkit |
| Fine-grained reactivity with independent atoms | Jotai |

For this course, you will use `useState`, `useReducer`, and the Context API. That covers the vast majority of real-world React applications.

---

## Summary
- React detects state changes by reference. Never mutate state -- always return a new object or array.
- Spread (`{ ...prev, field: value }`) and array methods (`map`, `filter`, `[...prev]`) are your tools for immutable updates.
- Lift state to the closest common ancestor when siblings share it. Do not lift higher than needed.
- Data flows down via props. Events flow up via callbacks. This is intentional and makes bugs traceable.
- Derived values belong in variables during render, not in state.
- Context, Zustand, and Redux are solutions to prop drilling as applications grow.

## Additional Resources
- [React: Updating Objects in State](https://react.dev/learn/updating-objects-in-state)
- [React: Updating Arrays in State](https://react.dev/learn/updating-arrays-in-state)
- [React: Sharing State Between Components](https://react.dev/learn/sharing-state-between-components)
