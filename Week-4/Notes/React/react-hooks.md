# React Hooks

## Learning Objectives
- Explain what hooks are and list the two rules that govern their use
- Map React component lifecycle phases to their corresponding hooks
- Manage local state correctly with `useState`, including object and array state
- Run side effects and clean them up with `useEffect`
- Model complex state transitions with `useReducer`

## Why This Matters
Hooks are the mechanism that turned React function components from simple rendering functions into fully capable stateful, side-effect-aware components. Every non-trivial React component you write from this point forward will use at least `useState` and `useEffect`. Understanding when `useReducer` is appropriate saves you from state management complexity before you even need a dedicated library.

---

## What Are Hooks?

Hooks are functions that let you "hook into" React features from inside a function component. They were introduced in React 16.8 (2019) to replace class component lifecycle methods and the need to split logic across `componentDidMount`, `componentDidUpdate`, and `componentWillUnmount`.

All built-in hooks start with the prefix `use`. You can also write your own custom hooks (covered in Week 4).

### The Two Rules of Hooks

These are not suggestions -- React's behavior is undefined if you break them:

**Rule 1: Only call hooks at the top level.**

Never call hooks inside loops, conditions, or nested functions. React identifies hooks by their call order in each render. Conditional calls break that order.

```tsx
// WRONG
function ProductList() {
  if (someCondition) {
    const [count, setCount] = useState(0); // inside a condition
  }
}

// CORRECT
function ProductList() {
  const [count, setCount] = useState(0); // always at the top level
  if (someCondition) {
    // use count here
  }
}
```

**Rule 2: Only call hooks from React function components or custom hooks.**

Never call hooks from regular JavaScript functions, class methods, or outside of React.

---

## Component Lifecycle and Hooks

Class components had explicit lifecycle methods. Function components express the same phases through hooks:

| Lifecycle phase | What happens | Hook equivalent |
|----------------|-------------|----------------|
| Mount | Component appears in the DOM for the first time | `useEffect(() => { ... }, [])` |
| Update | State or props change, component re-renders | `useState` setter; `useEffect` with deps |
| Unmount | Component is removed from the DOM | Cleanup function in `useEffect` |

---

## `useState`

`useState` adds local state to a function component. Covered briefly on Tuesday -- today you go deeper.

### Basic Usage

```tsx
const [value, setValue] = useState(initialValue);
```

- `value` is the current state value (read-only in render).
- `setValue` is the setter -- calling it schedules a re-render.
- `initialValue` is only used on the first render.

### Updating Primitive State

```tsx
const [count, setCount] = useState(0);
const [name, setName] = useState("");
const [isOpen, setIsOpen] = useState(false);

// Direct value
setCount(5);
setIsOpen(true);

// Functional update -- use when new value depends on previous value
setCount(prev => prev + 1);  // safe in async callbacks and batched updates
```

### Updating Object State

**Never mutate state.** Create a new object:

```tsx
interface FormData {
  name: string;
  email: string;
  role: string;
}

const [form, setForm] = useState<FormData>({ name: "", email: "", role: "viewer" });

// Update one field -- spread the existing state, then override
function handleChange(field: keyof FormData, value: string) {
  setForm(prev => ({ ...prev, [field]: value }));
}

// WRONG: mutating state directly -- React will not see the change
form.name = "Alice";
setForm(form); // same object reference -- React skips re-render
```

### Updating Array State

```tsx
const [products, setProducts] = useState<Product[]>([]);

// Add
setProducts(prev => [...prev, newProduct]);

// Remove
setProducts(prev => prev.filter(p => p.id !== targetId));

// Update one item
setProducts(prev =>
  prev.map(p => p.id === targetId ? { ...p, price: newPrice } : p)
);

// WRONG: mutating the array
products.push(newProduct);  // same reference -- React misses the change
setProducts(products);
```

### Lazy Initialization

If initial state is expensive to compute, pass a function. React calls it only once:

```tsx
// Called every render (wasteful if expensive)
const [data, setData] = useState(expensiveComputation());

// Called only on the first render
const [data, setData] = useState(() => expensiveComputation());
```

---

## `useEffect`

`useEffect` runs code *after* React commits changes to the DOM. Use it for side effects: API calls, subscriptions, timers, DOM measurements.

### Signature

```tsx
useEffect(() => {
  // effect code -- runs after render

  return () => {
    // cleanup code -- runs before the next effect or on unmount
  };
}, [dependency1, dependency2]);
```

### The Dependency Array

The second argument controls *when* the effect re-runs:

```tsx
// No dependency array -- runs after EVERY render
useEffect(() => {
  console.log("rendered");
});

// Empty array -- runs only ONCE after initial mount
useEffect(() => {
  console.log("mounted");
}, []);

// With dependencies -- runs after mount AND whenever any dep changes
useEffect(() => {
  console.log("userId changed to", userId);
}, [userId]);
```

### Fetching Data

```tsx
function ProductDetail({ productId }: { productId: number }) {
  const [product, setProduct] = useState<Product | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false; // guard against setting state on an unmounted component

    setLoading(true);
    setError(null);

    fetch(`/api/products/${productId}`)
      .then(res => {
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        return res.json();
      })
      .then((data: Product) => {
        if (!cancelled) setProduct(data);
      })
      .catch(err => {
        if (!cancelled) setError(err.message);
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true; // cleanup: ignore response if component unmounted
    };
  }, [productId]); // re-fetch when productId changes

  if (loading) return <p>Loading...</p>;
  if (error) return <p>Error: {error}</p>;
  if (!product) return null;
  return <h1>{product.name}</h1>;
}
```

### Subscriptions and Cleanup

```tsx
useEffect(() => {
  const handler = (event: KeyboardEvent) => {
    if (event.key === "Escape") setModalOpen(false);
  };
  window.addEventListener("keydown", handler);

  // Cleanup: remove the listener when the component unmounts
  return () => window.removeEventListener("keydown", handler);
}, []);
```

### Common Mistakes

```tsx
// MISTAKE 1: missing dependency causes stale closure
const [count, setCount] = useState(0);
useEffect(() => {
  const id = setInterval(() => {
    console.log(count); // stale: always logs initial value
  }, 1000);
  return () => clearInterval(id);
}, []); // count is missing from deps

// FIX: add count to deps, or use functional update
useEffect(() => {
  const id = setInterval(() => {
    setCount(prev => prev + 1); // no closure over count needed
  }, 1000);
  return () => clearInterval(id);
}, []);

// MISTAKE 2: async function directly as the effect callback
useEffect(async () => {  // WRONG -- useEffect expects void or cleanup, not a Promise
  const data = await fetchProducts();
}, []);

// FIX: define async function inside and call it
useEffect(() => {
  async function load() {
    const data = await fetchProducts();
    setProducts(data);
  }
  load();
}, []);
```

---

## `useReducer`

`useReducer` is an alternative to `useState` for complex state that involves multiple sub-values or where the next state depends on the previous in non-trivial ways.

It follows the **reducer pattern** from functional programming (same as Redux at its core):
- **State**: the current data.
- **Action**: a description of what happened (`{ type: "ADD_ITEM", payload: product }`).
- **Reducer**: a pure function `(state, action) => newState`.
- **Dispatch**: a function you call to send an action to the reducer.

### When to Use `useReducer` vs `useState`

| Situation | Use |
|-----------|-----|
| One or two independent boolean/string/number values | `useState` |
| Multiple related values that change together | `useReducer` |
| Next state depends on previous in complex ways | `useReducer` |
| Multiple actions trigger state transitions | `useReducer` |

### Example: Shopping Cart

```tsx
interface CartItem {
  product: Product;
  quantity: number;
}

interface CartState {
  items: CartItem[];
}

type CartAction =
  | { type: "ADD_ITEM"; payload: Product }
  | { type: "REMOVE_ITEM"; payload: number }   // product id
  | { type: "CLEAR_CART" };

function cartReducer(state: CartState, action: CartAction): CartState {
  switch (action.type) {
    case "ADD_ITEM": {
      const existing = state.items.find(i => i.product.id === action.payload.id);
      if (existing) {
        return {
          items: state.items.map(i =>
            i.product.id === action.payload.id
              ? { ...i, quantity: i.quantity + 1 }
              : i
          ),
        };
      }
      return { items: [...state.items, { product: action.payload, quantity: 1 }] };
    }

    case "REMOVE_ITEM":
      return { items: state.items.filter(i => i.product.id !== action.payload) };

    case "CLEAR_CART":
      return { items: [] };

    default:
      return state;
  }
}

function Cart() {
  const [cart, dispatch] = useReducer(cartReducer, { items: [] });

  const totalPrice = cart.items.reduce(
    (sum, i) => sum + i.product.price * i.quantity,
    0
  );

  return (
    <div>
      <h2>Cart ({cart.items.length} items)</h2>
      <ul>
        {cart.items.map(item => (
          <li key={item.product.id}>
            {item.product.name} x{item.quantity}
            <button onClick={() => dispatch({ type: "REMOVE_ITEM", payload: item.product.id })}>
              Remove
            </button>
          </li>
        ))}
      </ul>
      <p>Total: ${totalPrice.toFixed(2)}</p>
      <button onClick={() => dispatch({ type: "CLEAR_CART" })}>Clear Cart</button>
    </div>
  );
}
```

The reducer is a **pure function** -- no side effects, no API calls, no mutation. Given the same state and action, it always returns the same new state. This makes it trivially testable:

```typescript
it("adds an item to an empty cart", () => {
  const initial: CartState = { items: [] };
  const action: CartAction = { type: "ADD_ITEM", payload: laptop };
  const result = cartReducer(initial, action);
  expect(result.items).toHaveLength(1);
  expect(result.items[0].quantity).toBe(1);
});
```

---

## Summary
- Hooks let function components manage state, run side effects, and tap into React's lifecycle.
- Two rules: call hooks at the top level only, and only from function components or custom hooks.
- `useState`: for local, independent values. Create new objects/arrays -- never mutate.
- `useEffect`: runs after render. The dependency array controls when. Always return cleanup.
- `useReducer`: for state with multiple related sub-values or complex transitions. The reducer must be pure.

## Additional Resources
- [React: `useState`](https://react.dev/reference/react/useState)
- [React: `useEffect`](https://react.dev/reference/react/useEffect)
- [React: `useReducer`](https://react.dev/reference/react/useReducer)
