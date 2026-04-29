# React Forms

## Learning Objectives

- Build a controlled component where input value is bound to React state.
- Build an uncontrolled component using `useRef` and `defaultValue`.
- Decide when controlled vs uncontrolled is the right choice.
- Handle form submission correctly, including `preventDefault`.
- Manage state for a multi-field form.
- Apply manual client-side validation and recognize when a library like React Hook Form is appropriate.

## Why This Matters

Forms are how users give your app data — login, signup, search, checkout, the "Add Product" form your trainee project will need. In Week 3 you saw `useState` and `onChange` on a single input. Real forms have many fields, validation rules, error messages, and submission logic, and the way you structure that state is what separates a maintainable form from a tangle of `useState` calls. This week's epic — **From Component to Pipeline** — assumes the React app you ship is the kind users will actually fill out, and getting form mechanics right is non-negotiable.

## The Concept

### Controlled components

A **controlled component** has its input value driven by React state. Every keystroke fires `onChange`, you update state, and React re-renders with the new value as the input's `value` prop. State is the single source of truth.

```tsx
import { useState } from "react";

export function NameField() {
  const [name, setName] = useState("");

  return (
    <input
      type="text"
      value={name}
      onChange={(e) => setName(e.target.value)}
    />
  );
}
```

Why bother? Because state lives in React, you can:

- Read the current value at any time without touching the DOM.
- Validate on every keystroke and show an error immediately.
- Disable the submit button until the value is valid.
- Reset the field by calling `setName("")`.
- Format the value as the user types (uppercasing, masking a phone number, trimming whitespace).

Controlled is the default in modern React. If you are not sure, use controlled.

### Uncontrolled components

An **uncontrolled component** lets the DOM hold the value, the way a plain HTML form does. You read it only when you need it, usually at submit time, using a ref. Use `defaultValue` (not `value`) to set an initial value.

```tsx
import { useRef, FormEvent } from "react";

export function NameForm() {
  const nameRef = useRef<HTMLInputElement>(null);

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    console.log("submitted name:", nameRef.current?.value);
  };

  return (
    <form onSubmit={handleSubmit}>
      <input type="text" defaultValue="" ref={nameRef} />
      <button type="submit">Submit</button>
    </form>
  );
}
```

No re-render per keystroke, less code, but you also lose live validation and live formatting.

### When to choose which

- **Controlled** — you need live validation, conditional UI based on the value, formatting as the user types, or you want to drive the input from elsewhere in your state. This is most forms.
- **Uncontrolled** — a simple form where you only care at submit time, or a file input (`<input type="file">`), which is always uncontrolled because file values cannot be set programmatically.

Mixing them on a single input is a bug: do not pass both `value` and `defaultValue` to the same field.

### Submission handling

Always attach `onSubmit` to the `<form>` element, not `onClick` to the button. The form's submit event also fires when the user presses Enter inside an input, and that path is missed if you wire only the button. Call `e.preventDefault()` first thing in the handler — without it, the browser does a full-page reload.

```tsx
const handleSubmit = (e: FormEvent) => {
  e.preventDefault();
  // your submit logic here
};
```

### Multi-field form state

For a form with several fields, you have two reasonable options.

**Option A — one `useState` per field.** Fine for two or three fields. Gets noisy fast.

```tsx
const [name, setName] = useState("");
const [email, setEmail] = useState("");
const [password, setPassword] = useState("");
```

**Option B — one state object.** Scales better. Use a single `onChange` handler that keys off the input's `name` attribute.

```tsx
import { useState, ChangeEvent, FormEvent } from "react";

interface SignupForm {
  name: string;
  email: string;
  password: string;
}

export function Signup() {
  const [form, setForm] = useState<SignupForm>({ name: "", email: "", password: "" });

  const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    console.log(form);
  };

  return (
    <form onSubmit={handleSubmit}>
      <input name="name" value={form.name} onChange={handleChange} placeholder="Name" />
      <input name="email" value={form.email} onChange={handleChange} placeholder="Email" />
      <input name="password" type="password" value={form.password} onChange={handleChange} placeholder="Password" />
      <button type="submit">Sign up</button>
    </form>
  );
}
```

The functional updater `setForm((prev) => ({ ...prev, [name]: value }))` matters: if two changes fire close together, you do not want to lose one because you read stale state.

### Validation: manual vs library

**Manual validation** is fine for small forms. Track an `errors` object alongside `form`, run a validate function on submit (and optionally on blur or on change), block submission if errors exist.

```tsx
const validate = (f: SignupForm) => {
  const errors: Partial<Record<keyof SignupForm, string>> = {};
  if (!f.name.trim()) errors.name = "Name is required";
  if (!/^\S+@\S+\.\S+$/.test(f.email)) errors.email = "Email is invalid";
  if (f.password.length < 8) errors.password = "Password must be at least 8 characters";
  return errors;
};
```

**Library-based** validation is worth reaching for once your form has more than a handful of fields, conditional fields, async checks (e.g., "is this username taken?"), or shared validation rules across forms. **React Hook Form** is the most common choice — it uses uncontrolled inputs under the hood for performance and pairs with schema validators like `zod` or `yup` for the rules themselves. You will see it in real codebases. Reach for it when manual validation starts to feel like duplication; for now, write the validation by hand so you understand what the library is automating.

## Code Example

A controlled multi-field form with manual validation:

```tsx
import { useState, ChangeEvent, FormEvent } from "react";

interface LoginForm { email: string; password: string; }

export function Login() {
  const [form, setForm] = useState<LoginForm>({ email: "", password: "" });
  const [errors, setErrors] = useState<Partial<Record<keyof LoginForm, string>>>({});

  const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    const next: typeof errors = {};
    if (!/^\S+@\S+\.\S+$/.test(form.email)) next.email = "Email is invalid";
    if (form.password.length < 8) next.password = "Min 8 characters";
    setErrors(next);
    if (Object.keys(next).length === 0) {
      console.log("submitting", form);
    }
  };

  return (
    <form onSubmit={handleSubmit} noValidate>
      <input name="email" value={form.email} onChange={handleChange} placeholder="Email" />
      {errors.email && <span role="alert">{errors.email}</span>}

      <input name="password" type="password" value={form.password} onChange={handleChange} placeholder="Password" />
      {errors.password && <span role="alert">{errors.password}</span>}

      <button type="submit">Log in</button>
    </form>
  );
}
```

## Summary

- Controlled inputs bind `value` to state via `onChange`. State is the single source of truth.
- Uncontrolled inputs read from a ref at submit time and use `defaultValue` for initial value.
- Default to controlled. Use uncontrolled for file inputs or trivial submit-only forms.
- Wire `onSubmit` on the `<form>`, not `onClick` on the button. Always call `preventDefault`.
- For multi-field forms, prefer a single state object plus a name-keyed `onChange`.
- Validate manually until the form is complex enough to justify React Hook Form + a schema library.

## Additional Resources

- React docs: forms and controlled inputs — <https://react.dev/reference/react-dom/components/input>
- MDN: form submission and `preventDefault` — <https://developer.mozilla.org/en-US/docs/Web/API/Event/preventDefault>
- React Hook Form (when you outgrow manual validation) — <https://react-hook-form.com/>
