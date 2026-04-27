# DOM Manipulation & Events

## Learning Objectives
- Select elements from the DOM using multiple strategies
- Create, modify, and remove DOM nodes
- Traverse the DOM tree
- Attach event listeners and control event propagation

## Why This Matters
React abstracts direct DOM manipulation -- you never call `appendChild` in React. But knowing what React is doing underneath makes you a better debugger. You will also use vanilla DOM APIs in Jest tests and browser DevTools. Event bubbling in particular is critical because React's synthetic event system is built on top of it.

> **Note:** This is a review reference for Week 3.

---

## The DOM as a Tree

```
document
└── html
    ├── head
    └── body
        ├── header
        │   └── nav
        │       └── a (href="/")
        └── main
            └── ul#product-list
                ├── li.product-card  (firstChild)
                ├── li.product-card
                └── li.product-card  (lastChild)
```

Every node is an object with properties and methods. `document` is the root entry point.

---

## Selecting Elements

```javascript
// By ID (returns one element or null)
const app = document.getElementById("app");

// CSS selector -- first match (or null)
const nav = document.querySelector("nav");
const btn = document.querySelector(".btn-primary");
const input = document.querySelector('input[type="email"]');

// CSS selector -- all matches (static NodeList)
const cards = document.querySelectorAll(".product-card");
cards.forEach(card => console.log(card.textContent));

// Older methods (still encountered in legacy code)
const items = document.getElementsByClassName("product-card"); // live HTMLCollection
const inputs = document.getElementsByTagName("input");
```

| Method | Returns | Live? | Notes |
|--------|---------|-------|-------|
| `getElementById` | Element or null | N/A | Fastest for ID lookups |
| `querySelector` | Element or null | No | Any CSS selector |
| `querySelectorAll` | NodeList | No (static) | Any CSS selector |
| `getElementsByClassName` | HTMLCollection | Yes | Updates when DOM changes |
| `getElementsByTagName` | HTMLCollection | Yes | |

---

## DOM Manipulation

### Reading and Writing Content

```javascript
const el = document.querySelector("h1");

el.textContent;          // get plain text (safe, no HTML parsing)
el.textContent = "New";  // set plain text

el.innerHTML;            // get HTML markup
el.innerHTML = "<b>New</b>"; // set HTML -- careful: XSS risk with user input
```

### Creating and Adding Elements

```javascript
// Create
const li = document.createElement("li");
li.textContent = "New Product";
li.classList.add("product-card");
li.setAttribute("data-id", "42");

// Add to DOM
const list = document.querySelector("#product-list");
list.appendChild(li);             // add as last child
list.prepend(li);                 // add as first child
list.insertBefore(li, list.firstChild); // insert before a reference node

// Modern (IE not supported, but fine for modern browsers)
list.append(li, anotherLi);      // append multiple nodes/strings
el.insertAdjacentHTML("beforeend", "<li>Fast insert</li>"); // no re-parsing
```

### Removing Elements

```javascript
li.remove();                    // remove from DOM
list.removeChild(li);           // older API -- remove specific child
```

### Attributes and Classes

```javascript
el.getAttribute("href");
el.setAttribute("href", "/new");
el.removeAttribute("disabled");
el.hasAttribute("required");    // boolean

el.classList.add("active");
el.classList.remove("active");
el.classList.toggle("active");  // add if absent, remove if present
el.classList.contains("active"); // boolean

// Inline styles (prefer CSS classes instead)
el.style.color = "red";
el.style.display = "none";
```

---

## Traversing the DOM

```javascript
const item = document.querySelector(".product-card");

// Parents
item.parentNode;           // nearest parent node (could be text node)
item.parentElement;        // nearest parent element

// Children
item.children;             // HTMLCollection of element children (no text nodes)
item.firstElementChild;
item.lastElementChild;
item.childNodes;           // includes text nodes, comments

// Siblings
item.nextElementSibling;
item.previousElementSibling;

// Closest ancestor matching a selector (useful in event delegation)
item.closest("ul");        // walks up the tree until it finds a match
```

---

## Events and Listeners

```javascript
const button = document.querySelector("#save-btn");

// Add listener
function handleClick(event) {
  console.log("clicked", event.target);
}
button.addEventListener("click", handleClick);

// Remove listener (must pass the same function reference)
button.removeEventListener("click", handleClick);

// Arrow function as listener -- cannot be removed (no reference saved)
button.addEventListener("click", (e) => console.log(e)); // OK if removal not needed
```

### Common Event Types

| Category | Events |
|----------|--------|
| Mouse | `click`, `dblclick`, `mousedown`, `mouseup`, `mousemove`, `mouseover`, `mouseout`, `contextmenu` |
| Keyboard | `keydown`, `keyup`, `keypress` (deprecated) |
| Form | `submit`, `change`, `input`, `focus`, `blur`, `reset` |
| Window | `load`, `DOMContentLoaded`, `resize`, `scroll`, `beforeunload` |
| Drag | `dragstart`, `dragover`, `drop` |

### The Event Object

```javascript
button.addEventListener("click", (event) => {
  event.target;          // element that triggered the event
  event.currentTarget;   // element the listener is attached to
  event.type;            // "click"
  event.key;             // for keyboard events: "Enter", "Escape", etc.
  event.clientX;         // mouse position
  event.preventDefault();  // stop default browser behavior
  event.stopPropagation(); // stop the event from bubbling up
});
```

---

## Event Bubbling and Capturing

When an event fires on an element, it travels in three phases:

```
1. CAPTURE phase  -- from document down to the target
2. TARGET phase   -- the element that was interacted with
3. BUBBLE phase   -- from the target back up to document
```

By default, `addEventListener` registers in the **bubble phase**.

```html
<ul id="list">
  <li id="item">Click me</li>
</ul>
```

```javascript
document.querySelector("#item").addEventListener("click", () => console.log("item"));
document.querySelector("#list").addEventListener("click", () => console.log("list"));
document.addEventListener("click", () => console.log("document"));

// Clicking #item logs: "item" → "list" → "document"  (bubble order)
```

### stopPropagation vs preventDefault

```javascript
// stopPropagation -- stop the event from traveling further (up OR down)
item.addEventListener("click", (e) => {
  e.stopPropagation(); // "list" and "document" listeners will NOT fire
});

// preventDefault -- stop the browser's default action, but bubbling continues
form.addEventListener("submit", (e) => {
  e.preventDefault(); // no page reload; event still bubbles
});
```

### Event Delegation

Attach ONE listener to a parent to handle events on many children:

```javascript
// Instead of adding a listener to each <li>
document.querySelector("#list").addEventListener("click", (event) => {
  const card = event.target.closest(".product-card");
  if (!card) return;
  console.log("Card clicked:", card.dataset.id);
});
```

This is efficient for dynamic lists and is the pattern React uses internally.

---

## Summary
- `querySelector` / `querySelectorAll` are the modern, flexible selection tools.
- Always use `textContent` (not `innerHTML`) when inserting user-provided text to avoid XSS.
- Events bubble from target up to `document` by default. `stopPropagation` breaks the chain; `preventDefault` blocks browser defaults.
- Event delegation: one parent listener handles all child events via `event.target`.

## Additional Resources
- [MDN: Introduction to the DOM](https://developer.mozilla.org/en-US/docs/Web/API/Document_Object_Model/Introduction)
- [MDN: EventTarget.addEventListener](https://developer.mozilla.org/en-US/docs/Web/API/EventTarget/addEventListener)
- [MDN: Event bubbling and capture](https://developer.mozilla.org/en-US/docs/Learn/JavaScript/Building_blocks/Events#event_bubbling_and_capture)
