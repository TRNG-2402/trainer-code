# React HTTP with Axios

## Learning Objectives

- Place HTTP calls correctly inside a React component (in `useEffect`, not in the render body).
- Use Axios for `GET`, `POST`, `PUT`, and `DELETE` requests and read the response structure.
- Configure an Axios instance with `baseURL`, default headers, and interceptors.
- Compare Axios and Fetch on the dimensions that matter: JSON parsing, error handling, interceptors, and request cancellation.
- Implement the loading / error / data state pattern when a component talks to an external API.

## Why This Matters

Almost every React app you ship will need to fetch data from somewhere — your own ASP.NET Core API from Week 2, a third-party service, or a SaaS endpoint. In Week 1 you learned the HTTP basics (verbs, status codes, JSON). What is new here is *where* and *how* that call lives inside a React component without breaking the rendering model. This week's epic — **From Component to Pipeline** — assumes the React app is talking to a real backend, and the patterns below are how production React apps stay reliable when the network is slow, the server is down, or the user navigates away mid-request.

## The Concept

### Where to put HTTP calls

A React component's render function runs every time state or props change — sometimes many times per interaction. **Never call HTTP inside the render body**:

```tsx
// WRONG — this fires on every render and causes an infinite loop
function ProductList() {
  const data = axios.get("/api/products");   // do not do this
  return <ul>{/* ... */}</ul>;
}
```

The right place is `useEffect`, which runs *after* render and only when its dependencies change.

```tsx
import { useEffect, useState } from "react";
import axios from "axios";

function ProductList() {
  const [products, setProducts] = useState<Product[]>([]);

  useEffect(() => {
    axios.get<Product[]>("/api/products").then((res) => setProducts(res.data));
  }, []);   // empty deps = run once after mount

  return <ul>{products.map((p) => <li key={p.id}>{p.name}</li>)}</ul>;
}
```

The empty dependency array means "run after the first render only". If you depended on a value (say a product ID from the URL), you would put it in the array so the effect re-runs when it changes.

### Axios basics

Install: `npm install axios`. Then import where you need it.

```tsx
import axios from "axios";

// GET
const res = await axios.get<Product[]>("/api/products");
console.log(res.data);     // the parsed JSON body
console.log(res.status);   // 200
console.log(res.headers);  // response headers

// POST — second arg is the body, axios serializes to JSON automatically
await axios.post("/api/products", { name: "Widget", price: 9.99 });

// PUT
await axios.put(`/api/products/${id}`, { name: "Updated", price: 12.50 });

// DELETE
await axios.delete(`/api/products/${id}`);
```

The response object always has the same shape: `{ data, status, statusText, headers, config }`. The body lives on `data`.

### Axios instance: baseURL, default headers, interceptors

A common pattern is to create one Axios *instance* with shared configuration and use it everywhere instead of the bare `axios` object.

```tsx
// api.ts
import axios from "axios";

export const api = axios.create({
  baseURL: "https://api.example.com",
  headers: { "Content-Type": "application/json" },
  timeout: 10000,
});
```

Now every call goes through `api`:

```tsx
import { api } from "./api";

await api.get("/products");           // -> https://api.example.com/products
await api.post("/cart", { sku });
```

**Interceptors** let you modify every request or every response in one place. The most common use is attaching an auth token from context or storage.

```tsx
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (res) => res,
  (error) => {
    if (error.response?.status === 401) {
      // token expired — kick to login
      window.location.href = "/login";
    }
    return Promise.reject(error);
  }
);
```

You write that once. Every call through the instance is now authenticated and handles 401s consistently.

### Axios vs Fetch

`fetch` is built into the browser. Axios is a third-party library. For most apps, the differences below tip the choice toward Axios — but you should know both.

| Concern              | Fetch                                                | Axios                                       |
| -------------------- | ---------------------------------------------------- | ------------------------------------------- |
| JSON parsing         | Manual: `await res.json()`                           | Automatic: `res.data` is already parsed     |
| Error on 4xx/5xx     | Resolves anyway — you must check `res.ok` yourself   | Rejects automatically on non-2xx status     |
| Request body         | Manual: `JSON.stringify(body)` and set headers       | Automatic for plain objects                 |
| Interceptors         | None — you write a wrapper                           | Built-in, request and response              |
| Request cancellation | `AbortController`                                    | `AbortController` (also legacy CancelToken) |
| Bundle size          | 0 (built-in)                                         | ~13 KB minified+gzipped                     |
| Default timeout      | None                                                 | Configurable per instance                   |

The same `GET` in each:

```tsx
// Fetch
const res = await fetch("/api/products");
if (!res.ok) throw new Error(`HTTP ${res.status}`);
const products = await res.json();
```

```tsx
// Axios
const { data: products } = await axios.get<Product[]>("/api/products");
```

If your app makes many calls, the boilerplate adds up — Axios pays for itself. If you make one call, Fetch is fine.

### The loading / error / data state pattern

A component fetching data has three observable states: **loading**, **error**, and **data**. Track all three; render each explicitly.

```tsx
import { useEffect, useState } from "react";
import { api } from "./api";

interface Product { id: number; name: string; price: number; }

export function ProductList() {
  const [products, setProducts] = useState<Product[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    setIsLoading(true);
    setError(null);

    api.get<Product[]>("/products")
      .then((res) => { if (!cancelled) setProducts(res.data); })
      .catch((err) => { if (!cancelled) setError(err.message); })
      .finally(() => { if (!cancelled) setIsLoading(false); });

    return () => { cancelled = true; };   // cleanup if component unmounts mid-request
  }, []);

  if (isLoading) return <p>Loading…</p>;
  if (error) return <p role="alert">Failed to load products: {error}</p>;
  return <ul>{products.map((p) => <li key={p.id}>{p.name} — ${p.price}</li>)}</ul>;
}
```

The `cancelled` flag matters because the user might navigate away while the request is still in flight. Without it, when the response finally arrives you would call `setProducts` on an unmounted component.

This three-state pattern is so common that you usually extract it into a custom hook (`useProducts`) so each component does not repeat it. The instructor demo will show that.

## Code Example

A small custom hook that wraps the loading / error / data pattern:

```tsx
// useApi.ts
import { useEffect, useState } from "react";
import { api } from "./api";

export function useApi<T>(path: string) {
  const [data, setData] = useState<T | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    setIsLoading(true);
    setError(null);

    api.get<T>(path)
      .then((res) => { if (!cancelled) setData(res.data); })
      .catch((err) => { if (!cancelled) setError(err.message); })
      .finally(() => { if (!cancelled) setIsLoading(false); });

    return () => { cancelled = true; };
  }, [path]);

  return { data, isLoading, error };
}
```

```tsx
// ProductList.tsx — the component is now tiny
import { useApi } from "./useApi";

interface Product { id: number; name: string; price: number; }

export function ProductList() {
  const { data, isLoading, error } = useApi<Product[]>("/products");

  if (isLoading) return <p>Loading…</p>;
  if (error) return <p role="alert">Error: {error}</p>;
  return <ul>{data?.map((p) => <li key={p.id}>{p.name}</li>)}</ul>;
}
```

That extraction is exactly the Container vs Presentational idea applied to data fetching — the hook is the container; the component is presentational.

## Summary

- Put HTTP calls in `useEffect`, never in the render body.
- Axios returns a response object with the parsed JSON on `res.data`. POST/PUT serialize the body for you.
- Create one Axios instance per backend with `baseURL`, default headers, and interceptors. Use it everywhere.
- Axios beats Fetch on parsing, error semantics, interceptors, and timeouts; Fetch wins on zero bundle cost.
- Track loading, error, and data explicitly. Cancel pending updates if the component unmounts mid-request.
- Extract the three-state pattern into a custom hook so each component does not repeat it.

## Additional Resources

- Axios docs — <https://axios-http.com/docs/intro>
- React docs: synchronizing with effects — <https://react.dev/learn/synchronizing-with-effects>
- MDN: Fetch API — <https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API>
