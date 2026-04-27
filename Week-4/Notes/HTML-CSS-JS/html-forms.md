# HTML Forms

## Learning Objectives
- Build a form with the correct structural attributes
- Choose the right input type for each field
- Apply HTML5 validation constraints
- Understand the difference between client-side and server-side validation

## Why This Matters
Forms are the primary way users send data to an application. On Tuesday you will build controlled form components in React -- those components still render HTML `<input>` elements, so knowing the native attributes prevents surprises.

> **Note:** This is a review reference for Week 3.

---

## The Form Element

```html
<form action="/submit" method="POST" enctype="application/x-www-form-urlencoded">
  <!-- inputs here -->
</form>
```

| Attribute | Values | Purpose |
|-----------|--------|---------|
| `action` | URL string | Where to send the form data |
| `method` | `GET`, `POST` | HTTP method used on submit |
| `enctype` | `application/x-www-form-urlencoded` (default), `multipart/form-data` (file uploads), `text/plain` | How the data is encoded |
| `novalidate` | boolean | Disables browser HTML5 validation |

---

## Input Types Quick Reference

```html
<input type="text"     name="username"  placeholder="Enter username" />
<input type="email"    name="email"     placeholder="you@example.com" />
<input type="password" name="password" />
<input type="number"   name="age"       min="18" max="120" />
<input type="date"     name="dob" />
<input type="checkbox" name="agree"     value="yes" />
<input type="radio"    name="plan"      value="monthly" />
<input type="file"     name="avatar"    accept="image/*" />
<input type="hidden"   name="csrf"      value="token123" />
<input type="range"    name="volume"    min="0" max="100" step="5" />
<input type="submit"   value="Save" />
<input type="reset"    value="Clear" />
```

| Type | Use case |
|------|---------|
| `text` | Free-form short text |
| `email` | Email addresses (auto-validates format) |
| `password` | Masked input |
| `number` | Numeric input with optional min/max/step |
| `checkbox` | Boolean or multi-select toggles |
| `radio` | Single choice from a named group |
| `file` | File picker (use `multipart/form-data` on form) |
| `hidden` | Data submitted with the form but not shown to user |
| `date` | Date picker |
| `range` | Slider |

---

## Select and Multi-Select

```html
<!-- Single select -->
<select name="country">
  <option value="">-- Choose a country --</option>
  <option value="us">United States</option>
  <option value="mx" selected>Mexico</option>
</select>

<!-- Multi-select -->
<select name="skills" multiple size="4">
  <option value="js">JavaScript</option>
  <option value="ts">TypeScript</option>
  <option value="react">React</option>
  <option value="dotnet">.NET</option>
</select>
```

For multi-select, use `name="skills[]"` (or handle array in server/JS) and hold `Ctrl`/`Cmd` to select multiple options.

---

## Other Form Elements

```html
<!-- Multi-line text -->
<textarea name="bio" rows="4" cols="50" placeholder="Tell us about yourself"></textarea>

<!-- Clickable button (does not submit by default) -->
<button type="button" onclick="doSomething()">Click Me</button>

<!-- Submit button -->
<button type="submit">Save</button>

<!-- Group related fields -->
<fieldset>
  <legend>Shipping Address</legend>
  <!-- inputs -->
</fieldset>

<!-- Accessible label (always link labels to inputs) -->
<label for="email">Email</label>
<input id="email" type="email" name="email" />
```

---

## HTML5 Validation Attributes

```html
<input
  type="email"
  name="email"
  required
  minlength="5"
  maxlength="100"
  pattern="[a-z0-9._%+\-]+@[a-z0-9.\-]+\.[a-z]{2,}$"
/>
```

| Attribute | Applies to | Effect |
|-----------|-----------|--------|
| `required` | All inputs | Field must have a value |
| `minlength` / `maxlength` | text, email, password, textarea | Character count range |
| `min` / `max` | number, date, range | Value range |
| `step` | number, range, date | Allowed increment |
| `pattern` | text, email, url, tel | Regex the value must match |
| `type="email"` | `<input>` | Built-in email format check |

The browser blocks form submission and shows native error messages when these fail.

---

## Submitting a Form

Default behavior on submit: browser serializes field values and sends an HTTP request to `action` using `method`. The page reloads.

To intercept in JavaScript:

```javascript
document.querySelector('form').addEventListener('submit', (event) => {
  event.preventDefault(); // stop the browser reload
  const data = new FormData(event.target);
  console.log(Object.fromEntries(data)); // { email: '...', ... }
});
```

In React you will call `event.preventDefault()` in the `onSubmit` handler the same way.

---

## Client-Side vs Server-Side Validation

| | Client-Side (HTML5 / JS) | Server-Side (.NET, etc.) |
|--|--------------------------|--------------------------|
| **When** | Before request is sent | After request arrives |
| **Purpose** | Fast UX feedback, reduce bad requests | Enforce security and business rules |
| **Can be bypassed?** | Yes -- never rely on it alone | No -- always the final authority |
| **Example** | `required`, regex pattern, JS check | ASP.NET Data Annotations, FluentValidation |

Always validate on the server. Client-side validation is a convenience, not a security control.

---

## Summary
- `<form>` attributes (`action`, `method`, `enctype`) control how data is sent.
- Choose `input type` carefully -- browsers use it for keyboard, validation, and UI.
- HTML5 validation attributes (`required`, `pattern`, `min`/`max`) provide fast UX feedback.
- Always validate on the server; client-side validation can be bypassed.

## Additional Resources
- [MDN: `<form>` element](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/form)
- [MDN: `<input>` types](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input)
- [MDN: Client-side form validation](https://developer.mozilla.org/en-US/docs/Learn/Forms/Form_validation)
