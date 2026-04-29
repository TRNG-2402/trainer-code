# React Advanced Component Patterns

## Learning Objectives

- Explain what a Higher-Order Component (HOC) is and recognize the `withSomething(Base)` pattern.
- Identify HOC use cases (logging, auth guards, feature flags) and the limitations that pushed the community toward hooks.
- Apply the Specialization pattern to build a specific component on top of a general one.
- Separate concerns using the Container vs Presentational pattern.
- Build a typed component in TSX with a `Props` interface, typed `children`, and decide between `React.FC` and an explicit return type.

## Why This Matters

In Week 3 you built React components using props, state, and hooks. Those primitives carry you a long way, but as an app grows you start to notice the same logic repeated: every page checks "is the user logged in?", every list does its own loading and error rendering, every form has its own way of showing field errors. The patterns in this lesson exist to take that repetition out — to write the rule once and apply it to many components. This week's epic is **From Component to Pipeline**: a codebase that ships through CI/CD has to be readable by people who did not write it, and consistent component patterns are how a team keeps that readability over time.

## The Concept

### Higher-Order Components (HOCs)

A **Higher-Order Component** is a function that takes a component and returns a new component with extra behavior. Think of it as a wrapper that decorates the component you give it.

```tsx
// Pattern: const Enhanced = withSomething(Base);
```

A small example — a HOC that logs every render of whatever it wraps:

```tsx
import { ComponentType } from "react";

function withLogging<P extends object>(Wrapped: ComponentType<P>): ComponentType<P> {
  return function Logged(props: P) {
    console.log(`Rendering ${Wrapped.displayName ?? Wrapped.name}`);
    return <Wrapped {...props} />;
  };
}

const LoggedButton = withLogging(Button);
```

**Common HOC use cases.**

- **Logging / analytics** — wrap any component to record renders or mounts.
- **Auth guards** — `withAuth(Page)` redirects to login if there is no token, otherwise renders the page. You will see this pattern in the demo.
- **Feature flags** — `withFeatureFlag("newCheckout")(Checkout)` renders the new component when the flag is on, the old one when it is off.
- **Injecting data or services** — `withTheme(Component)` adds a `theme` prop pulled from context.

**Why hooks reduced HOC usage.** HOCs have real downsides. They produce a "wrapper hell" of nested components in the React DevTools tree. They make typing in TypeScript harder because you have to compose generics. They obscure where a prop comes from — a component receives `currentUser` and you cannot tell which of three HOCs added it. Hooks (`useAuth()`, `useFeatureFlag()`, `useTheme()`) solve the same problems with a clearer call site: the data is requested explicitly, in the body of the component that uses it, and TypeScript follows along naturally.

You should still know HOCs. Plenty of existing code uses them, several routing and data libraries still expose them, and the mental model — "wrap a component to extend it" — is useful even when you implement the same thing as a hook today.

### Specialization

**Specialization** is a small pattern: build a general component, then build a specific one on top of it by fixing some of the props.

```tsx
interface ButtonProps {
  label: string;
  variant: "primary" | "secondary" | "danger";
  onClick: () => void;
}

function Button({ label, variant, onClick }: ButtonProps) {
  return <button className={`btn btn-${variant}`} onClick={onClick}>{label}</button>;
}

// Specialized
function DangerButton({ label, onClick }: Omit<ButtonProps, "variant">) {
  return <Button label={label} variant="danger" onClick={onClick} />;
}
```

The benefit is that the specific component now has a smaller, clearer API. Callers do not need to remember the magic value `"danger"` — they just import `DangerButton`. When the design system changes the danger color, you update one place.

Specialization is what you reach for instead of an `if`-tree inside a single component. If you find yourself adding "and if `kind === 'submit'` also do X", consider splitting it into a specialized component instead.

### Container vs Presentational

The **Container vs Presentational** pattern (also called **smart vs dumb components**) splits one job into two:

- The **container** owns data: it fetches, manages state, knows the API. It does not render much directly.
- The **presentational** component owns appearance: given props, render markup. It does not fetch, does not navigate, does not call APIs.

```tsx
// Presentational — pure, easy to test, easy to reuse
interface ProductListViewProps {
  products: Product[];
  isLoading: boolean;
  error?: string;
}

function ProductListView({ products, isLoading, error }: ProductListViewProps) {
  if (isLoading) return <Spinner />;
  if (error) return <ErrorBanner message={error} />;
  return <ul>{products.map((p) => <li key={p.id}>{p.name}</li>)}</ul>;
}

// Container — owns the data lifecycle
function ProductListContainer() {
  const { products, isLoading, error } = useProducts();
  return <ProductListView products={products} isLoading={isLoading} error={error} />;
}
```

The presentational component does not care whether the data came from a server, from a cache, or from a test fixture. That makes it trivially reusable and trivially testable. The container is the only piece that needs to change when you swap data sources.

In modern React the container is often "a hook plus a small wrapper", because hooks like `useProducts` already encapsulate the fetching. The pattern is alive — the implementation just got lighter.

### Building a component with TSX

A typed component starts with a `Props` interface. Define the shape, then accept it in the function signature.

```tsx
interface CardProps {
  title: string;
  subtitle?: string;          // optional
  onSelect: (id: string) => void;
  id: string;
}

function Card({ title, subtitle, onSelect, id }: CardProps) {
  return (
    <div onClick={() => onSelect(id)}>
      <h3>{title}</h3>
      {subtitle && <p>{subtitle}</p>}
    </div>
  );
}
```

**Typing `children`.** When a component accepts JSX inside it, type `children` as `React.ReactNode`.

```tsx
import { ReactNode } from "react";

interface PanelProps {
  title: string;
  children: ReactNode;
}

function Panel({ title, children }: PanelProps) {
  return (
    <section>
      <h2>{title}</h2>
      {children}
    </section>
  );
}

// usage
<Panel title="Settings">
  <UserPreferences />
  <NotificationToggles />
</Panel>
```

`ReactNode` covers strings, numbers, JSX, arrays of those, `null`, and `undefined` — basically anything you can put between tags.

**`React.FC` vs explicit return type.** You will see two styles in the wild:

```tsx
// Style 1 — React.FC
const Card: React.FC<CardProps> = ({ title }) => <div>{title}</div>;

// Style 2 — explicit function, inferred return type
function Card({ title }: CardProps) {
  return <div>{title}</div>;
}
```

The community has largely moved away from `React.FC`. Reasons:

- It used to imply a `children` prop you might not want; older versions of React types added `children` automatically.
- `defaultProps` typing did not work well with `React.FC` for generic components.
- The plain function form is simpler, infers the return type correctly, and supports generics naturally.

Either compiles. Pick one style per project and stick with it. **Default to the plain function form** unless you are matching an existing codebase.

## Code Example

An auth-guard HOC, plus the modern hook equivalent so you can compare:

```tsx
// HOC version
import { ComponentType } from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "./AuthContext";

function withAuthGuard<P extends object>(Wrapped: ComponentType<P>): ComponentType<P> {
  return function Guarded(props: P) {
    const { token } = useAuth();
    if (!token) return <Navigate to="/login" replace />;
    return <Wrapped {...props} />;
  };
}

const ProtectedDashboard = withAuthGuard(Dashboard);
```

```tsx
// Hook + small wrapper component — modern equivalent
function RequireAuth({ children }: { children: ReactNode }) {
  const { token } = useAuth();
  if (!token) return <Navigate to="/login" replace />;
  return <>{children}</>;
}

// usage
<RequireAuth>
  <Dashboard />
</RequireAuth>
```

Same outcome. The hook+wrapper version is what most new code uses.

## Summary

- **HOCs** wrap a component to add behavior. Useful for cross-cutting concerns; less common in new code because hooks express the same ideas more cleanly.
- **Specialization** builds a specific component on top of a general one by fixing some props. Smaller API, fewer magic values at the call site.
- **Container vs Presentational** separates "knows the data" from "knows the markup". Today the container is often just a hook plus a wrapper.
- TSX components start from a `Props` interface. Type `children` as `ReactNode`.
- Prefer plain function components with explicit `Props` over `React.FC`.

## Additional Resources

- React docs on passing data with props (TypeScript-friendly examples) — <https://react.dev/learn/passing-props-to-a-component>
- React TypeScript Cheatsheet — <https://react-typescript-cheatsheet.netlify.app/>
- "From HOCs to Hooks" overview from the React team — <https://legacy.reactjs.org/docs/higher-order-components.html>
