# React Context API

## Learning Objectives
- Explain the prop drilling problem and when it warrants a solution
- Create a context with `createContext()`
- Provide a context value to a component subtree with `Context.Provider`
- Consume context in any descendant with `useContext`
- Recognize when Context is and is not the right tool

## Why This Matters
By the end of this week you will have built a React app where state needs to flow through three or more component layers. Prop drilling that data becomes brittle and verbose. Context is React's built-in answer. In Week 4 you will use the same pattern to distribute auth state across the entire app, and again for the cart in your exercise.

---

## The Prop Drilling Problem

Prop drilling happens when an intermediate component must pass props it does not use just to get data to a deeply nested descendant.

```tsx
// theme needs to reach ThemeToggle, but it must pass through NavBar and Header
function App() {
  const [theme, setTheme] = useState<"light" | "dark">("light");
  return <NavBar theme={theme} onThemeChange={setTheme} />;
}

function NavBar({ theme, onThemeChange }: NavBarProps) {
  // NavBar does not use theme or onThemeChange -- just passes them down
  return <Header theme={theme} onThemeChange={onThemeChange} />;
}

function Header({ theme, onThemeChange }: HeaderProps) {
  // Header also does not use them -- passes them further down
  return <ThemeToggle theme={theme} onThemeChange={onThemeChange} />;
}

function ThemeToggle({ theme, onThemeChange }: ThemeToggleProps) {
  // ThemeToggle is the only one that actually uses these
  return (
    <button onClick={() => onThemeChange(theme === "light" ? "dark" : "light")}>
      {theme === "light" ? "Dark Mode" : "Light Mode"}
    </button>
  );
}
```

`NavBar` and `Header` are polluted with props they do not use. Adding a new prop means editing every layer even if only the bottom layer consumes it.

---

## The Context API

Context creates a "broadcast channel" -- a provider puts a value on the channel, and any descendant can tune in (subscribe) without the intermediate layers being involved.

Three steps:
1. **Create** the context with `createContext()`
2. **Provide** the value with `<Context.Provider value={...}>`
3. **Consume** the value with `useContext(Context)`

---

## Step 1: `createContext()`

```tsx
import { createContext } from "react";

// createContext(defaultValue) -- the default is only used when a component
// is rendered outside of any Provider for this context
const ThemeContext = createContext<"light" | "dark">("light");
```

The type argument tells TypeScript what the context value's type is. Put context definitions in their own files for reuse:

```
src/
  context/
    ThemeContext.tsx
    AuthContext.tsx
    CartContext.tsx
```

---

## Step 2: `Context.Provider`

Wrap the part of the tree that should have access to the value:

```tsx
function App() {
  const [theme, setTheme] = useState<"light" | "dark">("light");

  return (
    <ThemeContext.Provider value={theme}>
      <NavBar />                {/* no theme prop needed */}
    </ThemeContext.Provider>
  );
}
```

All descendants of `ThemeContext.Provider` -- at any depth -- can access `theme`. Components outside the Provider receive the default value passed to `createContext()`.

Providers can be nested. The nearest ancestor Provider wins:

```tsx
<ThemeContext.Provider value="light">
  <div>
    <ThemeContext.Provider value="dark">
      {/* components here see "dark" */}
    </ThemeContext.Provider>
    {/* components here see "light" */}
  </div>
</ThemeContext.Provider>
```

---

## Step 3: `useContext()`

```tsx
import { useContext } from "react";

function ThemeToggle() {
  const theme = useContext(ThemeContext); // reads from the nearest Provider above
  return <p>Current theme: {theme}</p>;
}
```

No props. No intermediate layers. `ThemeToggle` connects directly to the Provider.

---

## Distributing State and Setters

The most common pattern is providing both the state value and its setter so descendants can read AND update:

```tsx
import { createContext, useContext, useState } from "react";

// Define the context type
interface ThemeContextType {
  theme: "light" | "dark";
  toggleTheme: () => void;
}

// Create with a placeholder default (or use undefined and guard at runtime)
const ThemeContext = createContext<ThemeContextType>({
  theme: "light",
  toggleTheme: () => {},
});

// Provider component -- encapsulates the state
export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const [theme, setTheme] = useState<"light" | "dark">("light");

  function toggleTheme() {
    setTheme(prev => (prev === "light" ? "dark" : "light"));
  }

  return (
    <ThemeContext.Provider value={{ theme, toggleTheme }}>
      {children}
    </ThemeContext.Provider>
  );
}

// Custom hook -- encapsulates useContext + provides a nice API
export function useTheme() {
  return useContext(ThemeContext);
}
```

Usage:

```tsx
// main.tsx -- wrap the app
<ThemeProvider>
  <App />
</ThemeProvider>

// ThemeToggle.tsx -- any descendant, any depth
function ThemeToggle() {
  const { theme, toggleTheme } = useTheme();
  return (
    <button onClick={toggleTheme}>
      Switch to {theme === "light" ? "Dark" : "Light"} Mode
    </button>
  );
}

// NavBar.tsx -- no props needed; NavBar is not modified at all
function NavBar() {
  return (
    <nav>
      <Logo />
      <ThemeToggle />    {/* works without NavBar knowing about theme */}
    </nav>
  );
}
```

---

## Practical Example: Auth Context

```tsx
// context/AuthContext.tsx
interface User {
  id: number;
  name: string;
  role: "admin" | "viewer";
}

interface AuthContextType {
  user: User | null;
  login: (user: User) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType>({
  user: null,
  login: () => {},
  logout: () => {},
});

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);

  const login = (user: User) => setUser(user);
  const logout = () => setUser(null);

  return (
    <AuthContext.Provider value={{ user, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
```

```tsx
// Any component in the tree
function UserMenu() {
  const { user, logout } = useAuth();
  if (!user) return <a href="/login">Sign In</a>;
  return (
    <div>
      <span>Hello, {user.name}</span>
      <button onClick={logout}>Sign Out</button>
    </div>
  );
}

function AdminPanel() {
  const { user } = useAuth();
  if (user?.role !== "admin") return <p>Access denied.</p>;
  return <div>Admin content...</div>;
}
```

---

## When NOT to Use Context

Context is not a silver bullet. Misusing it causes performance problems and over-engineering.

### Performance: Context re-renders all consumers

Every component that calls `useContext(SomeContext)` **re-renders whenever the context value changes**. If your context value is an object that is recreated on every parent render, every consumer re-renders even if the relevant data did not change.

```tsx
// PROBLEM: new object on every App render → all consumers re-render
function App() {
  const [user, setUser] = useState(null);
  return (
    <AuthContext.Provider value={{ user, setUser }}> {/* new object each render */}
      <AppContent />
    </AuthContext.Provider>
  );
}

// FIX: memoize the value to prevent unnecessary re-renders
function App() {
  const [user, setUser] = useState(null);
  const authValue = useMemo(() => ({ user, setUser }), [user]);
  return (
    <AuthContext.Provider value={authValue}>
      <AppContent />
    </AuthContext.Provider>
  );
}
```

`useMemo` is covered in Week 4.

### When to Use vs When Not to

| Use Context | Do NOT use Context |
|-------------|-------------------|
| Auth state (current user) | High-frequency updates (every keystroke) |
| Theme / locale | Large amounts of state with many independent pieces |
| Feature flags | State that only 2-3 closely related components share (just lift it) |
| Shopping cart shared across many pages | State local to one component or a small subtree |

For high-frequency or complex state, Zustand or Redux are better choices.

### Avoid Nesting Too Many Providers

If your application root looks like:

```tsx
<AuthProvider>
  <ThemeProvider>
    <CartProvider>
      <NotificationProvider>
        <FeatureFlagProvider>
          <App />
        </FeatureFlagProvider>
      </NotificationProvider>
    </CartProvider>
  </ThemeProvider>
</AuthProvider>
```

...that is a signal the application has grown beyond what Context can cleanly manage. In Week 4 you will see how Zustand can collapse multiple stores into a simpler model.

---

## Summary
- Prop drilling: intermediate components pass props they do not use. Context eliminates this.
- Three steps: `createContext(default)` → `<Context.Provider value={...}>` → `useContext(Context)`.
- Wrap state + setter in a Provider component and export a custom hook (`useAuth`, `useTheme`) for a clean API.
- Context re-renders all consumers on every value change. Use `useMemo` to stabilize object values.
- Use Context for global, low-frequency state (auth, theme). Use Zustand or Redux for high-frequency or complex domain state.

## Additional Resources
- [React: Passing Data Deeply with Context](https://react.dev/learn/passing-data-deeply-with-context)
- [React: `useContext`](https://react.dev/reference/react/useContext)
- [React: Before You Use Context](https://react.dev/learn/passing-data-deeply-with-context#before-you-use-context)
