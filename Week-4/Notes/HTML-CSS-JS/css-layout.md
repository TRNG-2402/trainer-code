# CSS Layout: Responsive Design, Flexbox & Grid

## Learning Objectives
- Write media queries for responsive layouts
- Build one-dimensional layouts with Flexbox
- Build two-dimensional layouts with Grid
- Choose between Flexbox and Grid for a given layout problem

## Why This Matters
Your React components this week will need to look good at multiple screen sizes. Flexbox and Grid are the tools -- no third-party layout library needed. CSS Modules (Week 4) use the same properties you learn here.

> **Note:** This is a review reference for Week 3.

---

## Responsive Web Design

**Core idea:** one codebase, any screen size. Three ingredients:

```html
<!-- 1. Viewport meta tag (required for mobile scaling) -->
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
```

```css
/* 2. Fluid widths -- use % or max-width instead of fixed px */
.container {
  width: 100%;
  max-width: 1200px;
  margin: 0 auto;
}

/* 3. Media queries -- apply styles at breakpoints */
/* Mobile-first: base styles target small screens, queries add styles as screen grows */

/* Tablet and up */
@media (min-width: 768px) {
  .card-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

/* Desktop and up */
@media (min-width: 1024px) {
  .card-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}
```

Common breakpoints (approximate, adjust to your design):

| Name | Min-width |
|------|----------|
| Mobile (base) | 0px |
| Tablet | 768px |
| Desktop | 1024px |
| Wide | 1280px |

---

## Flexbox

Flexbox is for **one-dimensional** layout -- a row or a column.

```css
.container {
  display: flex;
}
```

### Container Properties

```css
.container {
  display: flex;

  /* Direction */
  flex-direction: row;         /* row | row-reverse | column | column-reverse */

  /* Wrapping */
  flex-wrap: nowrap;           /* nowrap | wrap | wrap-reverse */

  /* Main axis alignment (horizontal when row) */
  justify-content: flex-start; /* flex-start | flex-end | center | space-between | space-around | space-evenly */

  /* Cross axis alignment (vertical when row) */
  align-items: stretch;        /* stretch | flex-start | flex-end | center | baseline */

  /* Multiple lines cross axis */
  align-content: normal;       /* same values as justify-content */

  /* Shorthand: direction + wrap */
  flex-flow: row wrap;

  /* Gap between items */
  gap: 16px;
  gap: 16px 8px;               /* row-gap column-gap */
}
```

### Item Properties

```css
.item {
  /* Grow to fill available space (proportion) */
  flex-grow: 0;    /* default: don't grow */
  flex-grow: 1;    /* grow equally with siblings */

  /* Shrink when space is tight (proportion) */
  flex-shrink: 1;  /* default: shrink equally */

  /* Initial size before grow/shrink */
  flex-basis: auto; /* or a length: 200px, 30% */

  /* Shorthand: grow shrink basis */
  flex: 1;           /* flex: 1 1 0% */
  flex: 0 0 200px;   /* fixed width, no grow/shrink */

  /* Override container align-items for this item only */
  align-self: center;

  /* Reorder visually (does not change DOM order) */
  order: 0;
}
```

### Common Flex Patterns

```css
/* Center something both axes */
.centered {
  display: flex;
  justify-content: center;
  align-items: center;
}

/* Nav bar: logo left, links right */
.navbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

/* Equal-width columns */
.columns > * {
  flex: 1;
}
```

---

## Grid

Grid is for **two-dimensional** layout -- rows and columns simultaneously.

```css
.container {
  display: grid;
}
```

### Container Properties

```css
.container {
  display: grid;

  /* Define columns */
  grid-template-columns: 200px 1fr 1fr;    /* fixed + flexible */
  grid-template-columns: repeat(3, 1fr);   /* 3 equal columns */
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr)); /* responsive */

  /* Define rows */
  grid-template-rows: auto 1fr auto;       /* header + content + footer */

  /* Gap */
  gap: 24px;
  column-gap: 16px;
  row-gap: 24px;

  /* Named areas */
  grid-template-areas:
    "header header"
    "sidebar main"
    "footer footer";
}
```

### Item Properties

```css
.item {
  /* Span specific columns */
  grid-column: 1 / 3;       /* from line 1 to line 3 (spans 2 columns) */
  grid-column: 1 / -1;      /* span all columns */
  grid-column: span 2;      /* span 2 columns from current position */

  /* Span specific rows */
  grid-row: 2 / 4;

  /* Place in a named area */
  grid-area: header;
  grid-area: sidebar;
  grid-area: main;
  grid-area: footer;
}
```

### Named Areas Example

```css
.layout {
  display: grid;
  grid-template-columns: 240px 1fr;
  grid-template-rows: 60px 1fr 40px;
  grid-template-areas:
    "header  header"
    "sidebar main"
    "footer  footer";
  min-height: 100vh;
}

.header  { grid-area: header; }
.sidebar { grid-area: sidebar; }
.main    { grid-area: main; }
.footer  { grid-area: footer; }
```

---

## Flexbox vs Grid: When to Use Each

| Situation | Use |
|-----------|-----|
| Laying out items in one direction (nav, button group, card row) | Flexbox |
| Full-page layout with header/sidebar/main/footer | Grid |
| Items with unknown count that should wrap | Flexbox (`flex-wrap`) |
| Complex two-dimensional alignment | Grid |
| Centering a single item | Either (Flexbox is simpler) |
| Items need to align across rows AND columns | Grid |

They also compose: a Grid area can contain a Flex container, and vice versa.

---

## Summary
- Add the viewport meta tag or mobile layouts will not work.
- Media queries (mobile-first) adapt layouts at breakpoints using `min-width`.
- Flexbox = one axis at a time. Container sets direction and alignment; items set grow/shrink.
- Grid = two axes. Define columns and rows on the container; items span named lines or areas.

## Additional Resources
- [CSS Tricks: A Complete Guide to Flexbox](https://css-tricks.com/snippets/css/a-guide-to-flexbox/)
- [CSS Tricks: A Complete Guide to Grid](https://css-tricks.com/snippets/css/complete-guide-grid/)
- [MDN: Responsive design](https://developer.mozilla.org/en-US/docs/Learn/CSS/CSS_layout/Responsive_Design)
