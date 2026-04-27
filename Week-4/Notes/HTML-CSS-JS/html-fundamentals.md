# HTML Fundamentals

## Learning Objectives
- Identify the structure of a valid HTML document
- Use tags, elements, and attributes correctly
- Distinguish inline from block elements
- Apply semantic HTML tags to improve readability and accessibility

## Why This Matters
React components ultimately render HTML to the browser DOM. Understanding the underlying document model keeps you grounded in what is actually happening when a component renders -- a React `<div>` is not magic; it becomes a real DOM node.

> **Note:** This is a review reference for Week 3. Full depth is available in your Week 1 resources.

---

## Document Structure

Every valid HTML document follows this skeleton:

```html
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Page Title</title>
    <link rel="stylesheet" href="styles.css" />
  </head>
  <body>
    <!-- visible content here -->
    <script src="main.js" defer></script>
  </body>
</html>
```

| Part | Purpose |
|------|---------|
| `<!DOCTYPE html>` | Tells the browser to use the HTML5 parser |
| `<head>` | Metadata, title, CSS links -- not rendered visibly |
| `<body>` | Everything the user sees |
| `defer` on `<script>` | Loads JS after the DOM is parsed |

---

## The DOM

The browser parses HTML into a **Document Object Model (DOM)** -- a tree of nodes that JavaScript (and React) read and modify at runtime.

```
document
└── html
    ├── head
    │   └── title ("My App")
    └── body
        ├── header
        ├── main
        │   └── section
        │       └── article
        └── footer
```

---

## Tags, Elements, and Attributes

These three terms are distinct:

| Concept | Example | Notes |
|---------|---------|-------|
| **Tag** | `<p>` / `</p>` | The syntax marker |
| **Element** | `<p>Hello</p>` | Opening tag + content + closing tag |
| **Self-closing element** | `<img />` | No content, no closing tag needed |
| **Attribute** | `<a href="/home">` | Key-value pair on the opening tag |
| **Global attributes** | `id`, `class`, `style`, `data-*`, `hidden`, `tabindex` | Valid on any element |

```html
<!-- Attribute examples -->
<img src="photo.jpg" alt="A product photo" width="300" />
<input type="email" name="email" placeholder="you@example.com" required />
<div id="app" class="container" data-theme="dark"></div>
```

---

## Inline vs Block Elements

| Type | Behavior | Common Examples |
|------|----------|----------------|
| **Block** | Takes full available width; starts on a new line | `<div>`, `<p>`, `<h1>`–`<h6>`, `<ul>`, `<ol>`, `<li>`, `<section>`, `<article>` |
| **Inline** | Flows within text; only as wide as its content | `<span>`, `<a>`, `<strong>`, `<em>`, `<img>`, `<button>`, `<label>` |

---

## Common Tags Quick Reference

```html
<!-- Headings (block) -->
<h1> <h2> <h3> <h4> <h5> <h6>

<!-- Text content (block) -->
<p>  <blockquote>  <pre>  <hr>  <br>

<!-- Inline text -->
<span>  <strong>  <em>  <code>  <a href="">

<!-- Lists -->
<ul>   <!-- unordered -->
  <li>
<ol>   <!-- ordered -->
  <li>
<dl>   <!-- description list -->
  <dt>  <dd>

<!-- Media -->
<img src="" alt="" />
<video src="" controls></video>
<audio src="" controls></audio>

<!-- Containers -->
<div>  <span>

<!-- Tables -->
<table>
  <thead><tr><th></th></tr></thead>
  <tbody><tr><td></td></tr></tbody>
```

---

## Semantic HTML

Semantic elements communicate the *meaning* of content to browsers and screen readers. Prefer them over generic `<div>` and `<span>`.

```html
<header>   <!-- site or section header (logo, nav) -->
<nav>      <!-- primary navigation links -->
<main>     <!-- primary page content; only one per page -->
<section>  <!-- thematic grouping of related content -->
<article>  <!-- self-contained content: blog post, card, comment -->
<aside>    <!-- related but secondary content: sidebar, callout -->
<footer>   <!-- site or section footer -->
<figure>   <!-- self-contained illustration, diagram, or code block -->
<figcaption> <!-- caption for a <figure> -->
<time datetime="2026-04-24">April 24, 2026</time>
```

---

## Summary

- HTML defines structure and meaning, not visual style.
- Elements = tags + content + closing tag. Attributes add metadata.
- Block elements stack; inline elements flow with text.
- Semantic tags improve accessibility, SEO, and code readability.
- The DOM is the runtime tree the browser and JavaScript both operate on.

## Additional Resources
- [MDN: HTML elements reference](https://developer.mozilla.org/en-US/docs/Web/HTML/Element)
- [MDN: Block-level elements](https://developer.mozilla.org/en-US/docs/Web/HTML/Block-level_elements)
- [web.dev: Learn HTML -- Document structure](https://web.dev/learn/html/document-structure/)
