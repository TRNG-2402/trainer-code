# React Styling

## Learning Objectives

- Apply plain CSS and CSS Modules to scope styles to a component.
- Use inline styles in JSX correctly, including the property naming rules and their limitations.
- Describe the styled-components / CSS-in-JS approach at a conceptual level and weigh its trade-offs.
- Use the `classnames` library to apply classes conditionally.
- Distinguish global styles from component-scoped styles and decide which to use.

## Why This Matters

In Week 3 you learned plain CSS and how to attach a stylesheet to a page. That is enough for a static site, but a React app is built from dozens or hundreds of small components, and naming collisions become a real problem the moment two engineers both write a `.card` class. This week's epic is **From Component to Pipeline** — turning loose components into a production app you can ship through CI/CD. Production apps need a styling strategy that is predictable across teams, survives refactoring, and does not silently break when two unrelated files use the same class name. The patterns below are the four approaches you will see in real React codebases.

## The Concept

### 1. Plain CSS (global)

The simplest approach is what you already know: write a `.css` file and import it from a component or from `main.tsx`. The styles apply to the entire document.

```tsx
// App.tsx
import "./App.css";

export function App() {
  return <button className="primary-btn">Save</button>;
}
```

```css
/* App.css */
.primary-btn { background: #2563eb; color: white; }
```

This works, but every class name lives in one global namespace. If another file also defines `.primary-btn`, whichever loads last wins. That is the core problem the next approaches solve.

### 2. CSS Modules (scoped)

CSS Modules give you scoped class names by convention. Name your file `Something.module.css` and import it as a JavaScript object. The build tool rewrites class names to be unique per file.

```tsx
// Button.tsx
import styles from "./Button.module.css";

export function Button() {
  return <button className={styles.primary}>Save</button>;
}
```

```css
/* Button.module.css */
.primary { background: #2563eb; color: white; }
```

At build time, `.primary` becomes something like `Button_primary__a3f9k`. No collisions are possible across files. This is the default styling approach in many React projects because the cost is low and the safety is high.

### 3. Inline styles

JSX accepts a `style` prop, but it takes an **object**, not a CSS string. Property names use camelCase, not kebab-case. Values that need units must include the unit as a string (or use a number for `px` shortcuts).

```tsx
const danger = { backgroundColor: "crimson", color: "white", padding: "8px 12px" };

export function DeleteButton() {
  return <button style={danger}>Delete</button>;
}
```

Limitations: no pseudo-classes (`:hover`, `:focus`), no media queries, no keyframes. Inline styles are best for small, dynamic, per-instance overrides — for example, setting a width from a prop. They are not a substitute for a stylesheet.

### 4. styled-components and CSS-in-JS (conceptual)

Libraries like `styled-components` and `Emotion` let you write CSS inside your JavaScript. A styled component is a normal React component that already has its CSS attached.

```tsx
import styled from "styled-components";

const PrimaryButton = styled.button`
  background: #2563eb;
  color: white;
  &:hover { background: #1d4ed8; }
`;

export function Save() {
  return <PrimaryButton>Save</PrimaryButton>;
}
```

**Trade-offs.** CSS-in-JS keeps styles co-located with the component and lets you use props and theme values directly in the styles. The cost is a runtime library, a small performance overhead (styles are injected as the component renders), and another set of debugging tools to learn. Many teams now prefer CSS Modules or utility-first frameworks for that reason. You should know the pattern exists and be able to read it.

### 5. The `classnames` library

When a class depends on state — active, disabled, error — string concatenation gets ugly fast. The `classnames` package takes any mix of strings, objects, and falsy values and returns a clean class string.

```tsx
import cx from "classnames";
import styles from "./Tab.module.css";

export function Tab({ label, isActive, isDisabled }: TabProps) {
  const className = cx(styles.tab, {
    [styles.active]: isActive,
    [styles.disabled]: isDisabled,
  });
  return <button className={className}>{label}</button>;
}
```

Without `classnames` you would write:

```tsx
const className = `${styles.tab}${isActive ? " " + styles.active : ""}${isDisabled ? " " + styles.disabled : ""}`;
```

The library version reads top-to-bottom and never produces stray spaces or `undefined` in the output.

### Global vs component-scoped — when to use each

- **Global styles:** CSS resets, font imports, root variables (`:root { --color-brand: ... }`), body/html rules. One global stylesheet, imported once at the app entry point.
- **Component-scoped styles:** every visual rule that belongs to a specific component. Use CSS Modules or styled-components.

The rule of thumb is: if the rule applies to the whole app, it is global. If only one component needs it, scope it. Avoid global selectors like `button { ... }` once your app is more than a handful of components — you will not be able to override them safely later.

## Code Example

A small component using a CSS Module plus `classnames`:

```tsx
// AlertBanner.tsx
import cx from "classnames";
import styles from "./AlertBanner.module.css";

type Severity = "info" | "warning" | "error";

interface AlertBannerProps {
  severity: Severity;
  message: string;
  dismissed?: boolean;
}

export function AlertBanner({ severity, message, dismissed }: AlertBannerProps) {
  const className = cx(styles.banner, styles[severity], {
    [styles.hidden]: dismissed,
  });
  return <div className={className}>{message}</div>;
}
```

```css
/* AlertBanner.module.css */
.banner { padding: 12px 16px; border-radius: 6px; font-weight: 500; }
.info    { background: #dbeafe; color: #1e3a8a; }
.warning { background: #fef3c7; color: #78350f; }
.error   { background: #fee2e2; color: #7f1d1d; }
.hidden  { display: none; }
```

The class name `styles.banner` is unique to this file, the severity-specific class is picked dynamically, and `dismissed` toggles visibility — all without polluting the global namespace.

## Summary

- Plain CSS is the simplest path but every class is global; collisions are likely as the app grows.
- CSS Modules give you scoped class names automatically through the `.module.css` filename convention.
- Inline styles are objects with camelCase keys; useful for one-off dynamic values, not a full styling solution.
- styled-components and other CSS-in-JS libraries co-locate CSS with the component at the cost of a runtime library.
- `classnames` cleans up conditional class logic.
- Keep resets and tokens global; keep component visuals scoped.

## Additional Resources

- React docs on styling: <https://react.dev/learn/passing-props-to-a-component#styling>
- CSS Modules specification and usage: <https://github.com/css-modules/css-modules>
- `classnames` library README: <https://github.com/JedWatson/classnames>
