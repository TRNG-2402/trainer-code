# React Routing with React Router

## Learning Objectives

- Distinguish client-side routing from server-side routing and explain the SPA trade-offs.
- Configure React Router v6 with `BrowserRouter`, `Routes`, `Route`, `Link`, and `NavLink`.
- Build nested routes and use `Outlet` for shared layouts.
- Read URL parameters with `useParams` and navigate programmatically with `useNavigate`.
- Implement a protected-route pattern that redirects unauthenticated users.

## Why This Matters

A real app has more than one screen. Users expect URLs that they can bookmark, share, and reload. Browser back and forward should work. In Week 3 your React app rendered a single screen at a time controlled by component state — that is not enough for an app with `/products`, `/products/42`, `/cart`, and `/login`. **React Router** is the de-facto solution. This week's epic — **From Component to Pipeline** — assumes you are shipping a multi-page React app through CI/CD, and routing is what makes "multi-page" actually work in a Single-Page Application.

## The Concept

### Client-side vs server-side routing

In **server-side routing** (the traditional web), every URL change asks the server for a new HTML page. The server decides what to send. The page reloads. Simple, robust, but slow — every navigation costs a full request/response cycle and a fresh download of CSS, JS, and shared layout markup.

In **client-side routing**, the server hands the browser one HTML shell on first load. JavaScript then watches the URL bar and swaps in the right components when the URL changes — no full reload. This is what makes a **Single-Page Application (SPA)** feel fast.

**Trade-offs of SPA routing:**

- **Pros:** instant transitions, shared layout stays mounted, smoother UX, less network traffic per click.
- **Cons:** larger initial JavaScript bundle, the server must serve the same shell for every URL (or 404s on refresh of `/products/42`), and search-engine indexing needs extra care if SEO matters.

React Router is what wires the URL to your components without a page reload.

### Setup: BrowserRouter at the root

Wrap your app once at the entry point. Everything inside can use routing.

```tsx
// main.tsx
import { BrowserRouter } from "react-router-dom";
import { App } from "./App";

createRoot(document.getElementById("root")!).render(
  <BrowserRouter>
    <App />
  </BrowserRouter>
);
```

`BrowserRouter` uses the HTML5 History API (clean URLs like `/products/42`). There is also `HashRouter` (`/#/products/42`) for environments where you cannot configure the server to serve `index.html` for every path.

### Routes and Route

`Routes` is the matcher. Inside it, you list `Route` elements that pair a URL path with a component. Exactly one `Route` matches per URL.

```tsx
import { Routes, Route } from "react-router-dom";

export function App() {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="/products" element={<ProductList />} />
      <Route path="/products/:id" element={<ProductDetail />} />
      <Route path="*" element={<NotFound />} />
    </Routes>
  );
}
```

The `*` path is a catch-all for unknown URLs.

### Link and NavLink

Never use `<a href="/products">` for in-app navigation — it triggers a full page reload. Use `Link` instead. It updates the URL via the History API and lets React Router swap the component.

```tsx
import { Link } from "react-router-dom";

<Link to="/products">Browse products</Link>
```

`NavLink` is `Link` with active-state awareness — handy for nav menus, because it tells you whether its target matches the current URL.

```tsx
import { NavLink } from "react-router-dom";

<NavLink
  to="/products"
  className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}
>
  Products
</NavLink>
```

### Nested routes and Outlet

Nested routes let a parent route render shared chrome (header, sidebar, layout) and let children render the body. `Outlet` is the placeholder where the child renders.

```tsx
// AppLayout.tsx — the parent
import { Outlet, NavLink } from "react-router-dom";

export function AppLayout() {
  return (
    <div className="app">
      <nav>
        <NavLink to="/">Home</NavLink>
        <NavLink to="/products">Products</NavLink>
        <NavLink to="/cart">Cart</NavLink>
      </nav>
      <main>
        <Outlet />   {/* child route renders here */}
      </main>
    </div>
  );
}
```

```tsx
// route configuration with nesting
<Routes>
  <Route element={<AppLayout />}>
    <Route path="/" element={<Home />} />
    <Route path="/products" element={<ProductList />} />
    <Route path="/products/:id" element={<ProductDetail />} />
    <Route path="/cart" element={<Cart />} />
  </Route>
</Routes>
```

The `AppLayout` mounts once. As you navigate `/`, `/products`, `/cart`, only the `Outlet` content swaps. That is why SPAs feel instant.

### Route parameters and useParams

A path segment starting with `:` is a parameter. Read it with `useParams`.

```tsx
// Route definition
<Route path="/products/:id" element={<ProductDetail />} />
```

```tsx
// ProductDetail.tsx
import { useParams } from "react-router-dom";

export function ProductDetail() {
  const { id } = useParams<{ id: string }>();
  return <h1>Product #{id}</h1>;
}
```

Params are always strings. If you need a number, parse it (`Number(id)`).

### Programmatic navigation with useNavigate

Sometimes navigation happens from code, not from a click — after a successful login, after submitting a form, after a timeout. `useNavigate` gives you a function to do that.

```tsx
import { useNavigate } from "react-router-dom";

export function LoginForm() {
  const navigate = useNavigate();

  const handleSuccess = () => {
    navigate("/products");          // push a new entry on the history stack
    // navigate("/products", { replace: true });   // replace current entry
    // navigate(-1);                                 // go back one step
  };
  // ...
}
```

Use `replace: true` for redirects that should not show up in the back button (e.g., redirecting away from a login page after success).

### Protected routes

A protected route redirects users who do not meet some condition — usually "are you logged in?". The clean v6 pattern is a small wrapper component plus `Navigate`.

```tsx
import { Navigate } from "react-router-dom";
import { ReactNode } from "react";
import { useAuth } from "./AuthContext";

function RequireAuth({ children }: { children: ReactNode }) {
  const { token } = useAuth();
  if (!token) return <Navigate to="/login" replace />;
  return <>{children}</>;
}
```

```tsx
// Apply it where needed
<Route
  path="/cart"
  element={
    <RequireAuth>
      <Cart />
    </RequireAuth>
  }
/>
```

Using `replace` on the redirect prevents the protected URL from sitting in browser history — the back button will not bounce the user back to a redirect loop.

## Code Example

A small but realistic setup combining everything:

```tsx
// main.tsx
import { BrowserRouter } from "react-router-dom";
import { AuthProvider } from "./AuthContext";
import { App } from "./App";

createRoot(document.getElementById("root")!).render(
  <BrowserRouter>
    <AuthProvider>
      <App />
    </AuthProvider>
  </BrowserRouter>
);
```

```tsx
// App.tsx
import { Routes, Route, Outlet, NavLink, Navigate } from "react-router-dom";
import { ReactNode } from "react";
import { useAuth } from "./AuthContext";

function Layout() {
  return (
    <div>
      <nav>
        <NavLink to="/">Home</NavLink>
        <NavLink to="/products">Products</NavLink>
        <NavLink to="/cart">Cart</NavLink>
      </nav>
      <main><Outlet /></main>
    </div>
  );
}

function RequireAuth({ children }: { children: ReactNode }) {
  const { token } = useAuth();
  return token ? <>{children}</> : <Navigate to="/login" replace />;
}

export function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Home />} />
        <Route path="/products" element={<ProductList />} />
        <Route path="/products/:id" element={<ProductDetail />} />
        <Route path="/cart" element={<RequireAuth><Cart /></RequireAuth>} />
        <Route path="/login" element={<Login />} />
        <Route path="*" element={<NotFound />} />
      </Route>
    </Routes>
  );
}
```

## Summary

- Server-side routing reloads the page; client-side routing swaps components in place. SPAs use the second.
- `BrowserRouter` wraps your app once. `Routes` matches a URL to one `Route`.
- `Link` and `NavLink` replace `<a>` for in-app navigation. `NavLink` tracks active state.
- Nested routes share layout via `Outlet` — the parent stays mounted while the child swaps.
- `useParams` reads URL parameters; `useNavigate` navigates from code.
- A protected-route wrapper plus `<Navigate replace />` is the v6 way to gate a route on authentication.

## Additional Resources

- React Router v6 official docs — <https://reactrouter.com/en/main>
- MDN: History API (the browser feature React Router uses) — <https://developer.mozilla.org/en-US/docs/Web/API/History_API>
- Tutorial: nested routes and `Outlet` — <https://reactrouter.com/en/main/start/tutorial>
