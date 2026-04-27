# React Overview

## Learning Objectives
- Explain what React is and where it fits in the front-end ecosystem
- Distinguish a library from a framework and explain the trade-off
- Compare React, React Native, and Angular at a high level
- Explain what JSX is and how it gets transformed to JavaScript
- Set up a React + TypeScript project with Vite

## Why This Matters
You are spending the rest of this week and the beginning of Week 4 in React. Understanding *what* React is (and what it is not) shapes how you reason about the code you write. A common mistake is treating React like a full framework and looking for things that are not there -- routing, HTTP, form management are all opt-in libraries, not built-ins.

---

## What Is React?

React is a **JavaScript library for building user interfaces**. It was created by Facebook (Meta) in 2013 and is open source.

Two words matter in that definition:

**Library:** React does one thing -- rendering UI as a tree of components. It does not include a router, an HTTP client, a state manager, or a form library. You compose those from separate packages.

**User interfaces:** React's job is to take your data and turn it into DOM nodes (or native views in React Native). It manages *when* and *what* to update.

### The Core Problem React Solves

Before React (and similar libraries), keeping the UI in sync with data was manual:

```javascript
// Before React: manual DOM sync is fragile and verbose
let count = 0;
const btn = document.querySelector("#count-btn");
const display = document.querySelector("#count-display");

btn.addEventListener("click", () => {
  count++;
  display.textContent = count; // developer must remember to update every place
});
```

React inverts this model. You describe *what* the UI should look like for a given state, and React figures out *how* to update the DOM efficiently:

```jsx
// React: declare the UI as a function of state
function Counter() {
  const [count, setCount] = React.useState(0);
  return (
    <div>
      <p>{count}</p>
      <button onClick={() => setCount(count + 1)}>Increment</button>
    </div>
  );
}
```

When `count` changes, React re-runs the function and updates only the DOM nodes that changed.

---

## Library vs Framework

| | Library | Framework |
|--|---------|-----------|
| **Who calls who** | You call the library's functions | The framework calls your code (inversion of control) |
| **Scope** | Solves one specific problem | Provides the full application structure |
| **Flexibility** | High -- compose what you need | Lower -- you work within the framework's conventions |
| **Decision burden** | Higher -- you choose every tool | Lower -- framework makes the choices |
| **Examples** | React, Axios, Lodash | Angular, Next.js, ASP.NET Core |

React is a library. You decide the router (React Router), the HTTP layer (Axios or Fetch), the state manager (Context, Redux, Zustand), and the form library (React Hook Form, Formik).

This is a feature, not a bug -- it means React can be integrated into existing projects incrementally, and teams can pick best-of-breed tools for each concern.

---

## React vs React Native vs Angular

### React Native

React Native applies the React component model to **native mobile apps** (iOS and Android). Instead of rendering HTML elements, it renders native UI controls.

| | React (web) | React Native (mobile) |
|--|------------|----------------------|
| Target | Browser DOM | iOS / Android native |
| Elements | `<div>`, `<p>`, `<button>` | `<View>`, `<Text>`, `<TouchableOpacity>` |
| Styling | CSS | StyleSheet API (CSS-like but not identical) |
| Deployment | Browser | App Store / Google Play |
| Skills transfer | Yes -- component model and hooks are identical | Yes -- same patterns, different primitives |

### Angular

Angular is a **full framework** from Google, written in TypeScript.

| | React | Angular |
|--|-------|---------|
| Type | Library | Full framework |
| Language | JS / TypeScript | TypeScript (required) |
| Rendering | Virtual DOM | Change detection (zones) |
| Templating | JSX (JS in HTML) | Templates (HTML + directives) |
| State | External (Context, Redux) | Services + RxJS |
| Router | React Router (separate) | Angular Router (built-in) |
| Forms | React Hook Form (separate) | Reactive Forms (built-in) |
| Learning curve | Moderate | Higher initially (more opinionated) |
| Market share | Very high | High (enterprise) |

Neither is objectively better. React's flexibility makes it dominant for web products; Angular's opinionated structure is common in large enterprise teams that value convention over configuration.

---

## JSX

JSX (JavaScript XML) is a syntax extension that lets you write HTML-like markup directly inside JavaScript. It is React's template language.

```jsx
// JSX (what you write)
const element = <h1 className="title">Hello, {name}!</h1>;

// What the TypeScript/Babel compiler transforms it into
const element = React.createElement("h1", { className: "title" }, `Hello, ${name}!`);
```

You write JSX; the build tool (Vite) compiles it to `React.createElement` calls. The browser never sees JSX.

### JSX Rules

```jsx
// 1. One root element per return (or use a Fragment)
return (
  <div>
    <h1>Title</h1>
    <p>Body</p>
  </div>
);

// Fragment -- no extra DOM node
return (
  <>
    <h1>Title</h1>
    <p>Body</p>
  </>
);

// 2. All tags must be closed (self-close if no children)
<input type="text" />   // correct
<br />                  // correct
// <input type="text">  // error -- not closed

// 3. class → className, for → htmlFor (class and for are JS reserved words)
<div className="container">
<label htmlFor="email">Email</label>

// 4. JavaScript expressions go in curly braces {}
const product = { name: "Laptop", price: 999 };
<p>{product.name}: ${product.price.toFixed(2)}</p>

// 5. Inline styles use an object with camelCase properties
<div style={{ backgroundColor: "blue", fontSize: "16px" }}>

// 6. Boolean attributes
<input disabled />          // same as disabled={true}
<input disabled={false} />  // attribute is omitted

// 7. Comments in JSX
{/* This is a JSX comment */}
```

### Expressions, Not Statements

JSX accepts *expressions* inside `{}` -- things that evaluate to a value. Statements (`if`, `for`) are not expressions and cannot go directly in JSX. Use ternary or logical operators instead:

```jsx
// Conditional rendering
<p>{isLoggedIn ? "Welcome back!" : "Please log in."}</p>

// Render only when condition is true
{isAdmin && <AdminPanel />}

// Derived value
<p>{products.length} products found</p>
```

---

## React in the Modern Front-End Stack

```
index.html          -- one HTML file, has <div id="root"></div>
    ↓
main.tsx            -- entry point: ReactDOM.createRoot("#root").render(<App />)
    ↓
App.tsx             -- root component, sets up Router, Providers
    ↓
pages/              -- top-level route components
components/         -- reusable UI components
hooks/              -- custom hooks
services/           -- API call functions (Axios/Fetch wrappers)
types/              -- TypeScript interfaces and types
```

React handles only the `components/` layer. Everything else is either your code or a separate library.

---

## Setting Up a React + TypeScript Project

```bash
npm create vite@latest product-catalog -- --template react-ts
cd product-catalog
npm install
npm run dev
```

Vite opens a dev server at `http://localhost:5173`. Edits to any `.tsx` file hot-reload instantly.

### Project Structure

```
product-catalog/
  src/
    App.tsx          -- replace with your root component
    App.css          -- delete or repurpose
    main.tsx         -- do not touch (entry point)
    vite-env.d.ts    -- Vite type declarations
  index.html
  vite.config.ts
  tsconfig.json
  package.json
```

### `main.tsx` (entry point)

```tsx
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "./App";
import "./index.css";

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <App />
  </StrictMode>
);
```

`StrictMode` is a development-only wrapper that highlights potential problems (double-invokes render functions to catch side effects). It has no effect in production builds.

---

## Summary
- React is a UI library -- it renders components to the DOM and nothing more.
- A library lets you choose your own tools; a framework provides them. React is a library.
- React Native uses the same component model to target iOS and Android instead of the browser.
- Angular is a full framework -- more opinionated, more built-in, common in enterprise.
- JSX is HTML-like syntax inside JavaScript. It compiles to `React.createElement` calls. The browser never sees it.
- Use `className` not `class`, close all tags, put expressions (not statements) in `{}`.

## Additional Resources
- [React: Quick Start](https://react.dev/learn)
- [React: Writing Markup with JSX](https://react.dev/learn/writing-markup-with-jsx)
- [Vite: Getting Started](https://vitejs.dev/guide/)
