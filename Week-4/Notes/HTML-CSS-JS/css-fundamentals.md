# CSS Fundamentals

## Learning Objectives
- Apply the three methods of attaching CSS to an HTML document
- Use element, class, ID, and combinator selectors
- Explain the box model and how it affects layout
- Calculate CSS specificity to resolve rule conflicts
- Define and consume CSS custom properties (variables)

## Why This Matters
React does not impose a CSS strategy -- you will use plain CSS this week and CSS Modules in Week 4. Either way, you are writing CSS selectors and properties. Knowing specificity prevents hours of debugging "why isn't my style applying."

> **Note:** This is a review reference for Week 3.

---

## Three Ways to Add CSS

```html
<!-- 1. Inline (highest specificity, hard to maintain) -->
<p style="color: red; font-size: 16px;">Hello</p>

<!-- 2. Internal (style block in <head>) -->
<style>
  p { color: blue; }
</style>

<!-- 3. External (preferred -- separate file, cacheable) -->
<link rel="stylesheet" href="styles.css" />
```

| Type | Specificity | Reusability | Best for |
|------|-------------|-------------|---------|
| Inline | Highest | None | One-off overrides, JS-driven styles |
| Internal | Medium | Page-only | Prototypes, style-in-template demos |
| External | Normal | Entire site | Production use |

---

## The Box Model

Every element is a box with four layers:

```
+---------------------------------+
|           Margin                |  (outside, transparent)
|  +---------------------------+  |
|  |        Border             |  |
|  |  +---------------------+  |  |
|  |  |      Padding        |  |  |
|  |  |  +--------------+   |  |  |
|  |  |  |   Content    |   |  |  |
|  |  |  +--------------+   |  |  |
|  |  +---------------------+  |  |
|  +---------------------------+  |
+---------------------------------+
```

```css
.box {
  width: 200px;        /* content width */
  padding: 16px;       /* inside border */
  border: 2px solid #333;
  margin: 24px auto;   /* outside, auto centers block elements */

  /* box-sizing: border-box makes width include padding + border */
  box-sizing: border-box;
}
```

`box-sizing: border-box` is almost universally applied globally:
```css
*, *::before, *::after { box-sizing: border-box; }
```

---

## Selectors

### Basic Selectors

```css
p { }               /* element -- all <p> tags */
.card { }           /* class -- any element with class="card" */
#app { }            /* ID -- element with id="app" (one per page) */
* { }               /* universal -- every element */
```

### Combinator Selectors

```css
div p { }           /* descendant -- any <p> inside a <div> */
div > p { }         /* child -- direct child <p> of <div> */
h2 + p { }          /* adjacent sibling -- <p> immediately after <h2> */
h2 ~ p { }          /* general sibling -- all <p> after <h2> in same parent */
```

### Attribute Selectors

```css
input[type="email"] { }      /* exact attribute value */
a[href^="https"] { }         /* attribute starts with */
img[src$=".png"] { }         /* attribute ends with */
```

### Pseudo-Classes and Pseudo-Elements

```css
a:hover { }            /* mouse over */
input:focus { }        /* element has focus */
li:first-child { }     /* first child element */
li:last-child { }
li:nth-child(2n) { }   /* every even item */
p::first-line { }      /* pseudo-element: first line of text */
p::before { content: "* "; }  /* insert content before */
```

---

## Cascading and Specificity

When multiple rules target the same element, the browser uses specificity to decide which wins.

### Specificity Score (a, b, c)

| Selector type | Points |
|---------------|--------|
| Inline style | `1,0,0,0` -- always wins (except `!important`) |
| ID (`#id`) | `0,1,0,0` |
| Class (`.class`), attribute, pseudo-class | `0,0,1,0` |
| Element (`p`), pseudo-element (`::before`) | `0,0,0,1` |

```css
p                 { color: black; }   /* 0,0,0,1 */
.text             { color: blue; }    /* 0,0,1,0 -- wins over p */
#intro            { color: green; }   /* 0,1,0,0 -- wins over .text */
p.text#intro      { color: red; }     /* 0,1,1,1 -- most specific */
```

**Cascading tie-breaker:** When specificity is equal, the *last rule in source order* wins.

Avoid `!important` -- it breaks the cascade and makes debugging painful.

---

## Common CSS Properties Quick Reference

```css
/* Typography */
font-family: 'Segoe UI', sans-serif;
font-size: 1rem;          /* 1rem = root font size (usually 16px) */
font-weight: 700;         /* 400 = normal, 700 = bold */
line-height: 1.5;
color: #333;
text-align: center;
text-decoration: none;    /* remove underline from links */
letter-spacing: 0.05em;

/* Background */
background-color: #f5f5f5;
background-image: url('bg.png');

/* Borders and radius */
border: 1px solid #ccc;
border-radius: 8px;

/* Sizing */
width: 100%;
max-width: 1200px;
height: auto;

/* Display */
display: block | inline | inline-block | flex | grid | none;

/* Positioning */
position: static | relative | absolute | fixed | sticky;
top / right / bottom / left: 0;
z-index: 10;

/* Overflow */
overflow: hidden | scroll | auto;
```

---

## CSS Custom Properties (Variables)

```css
/* Define on :root (globally available) */
:root {
  --color-primary: #0057b7;
  --color-text: #1a1a1a;
  --spacing-md: 1rem;
  --border-radius: 6px;
}

/* Consume anywhere */
.button {
  background-color: var(--color-primary);
  padding: var(--spacing-md);
  border-radius: var(--border-radius);
}

/* Override in a local scope */
.card {
  --color-primary: #e63946;  /* only affects elements inside .card */
}
```

Variables cascade like any CSS property. They are reactive -- change the variable value and all consumers update.

---

## Summary
- External stylesheets are preferred. Inline styles are highest specificity.
- The box model has four layers: content, padding, border, margin. Use `border-box` globally.
- Specificity: inline > ID > class > element. Source order breaks ties.
- CSS variables (`--name`) keep values consistent and easy to update.

## Additional Resources
- [MDN: CSS specificity](https://developer.mozilla.org/en-US/docs/Web/CSS/Specificity)
- [MDN: CSS Box Model](https://developer.mozilla.org/en-US/docs/Learn/CSS/Building_blocks/The_box_model)
- [MDN: Using CSS custom properties](https://developer.mozilla.org/en-US/docs/Web/CSS/Using_CSS_custom_properties)
