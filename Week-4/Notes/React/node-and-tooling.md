# Node.js, npm & Front-End Tooling

## Learning Objectives
- Explain what Node.js is and why front-end development depends on it
- Read and write a `package.json` file
- Use core npm commands with confidence
- Describe what a Single Page Application is and how it differs from a traditional web app
- Explain what Webpack does and why bundling exists

## Why This Matters
You will not write a single React file without Node.js running in the background. Every `npm install`, every `npm run dev`, every test run goes through Node. Understanding the tooling layer makes you a faster debugger -- when a dependency breaks or a build fails, you know where to look.

---

## What Is Node.js?

Node.js is a **JavaScript runtime** built on Chrome's V8 engine. It lets you run JavaScript outside the browser -- on your file system, on servers, and in build tools.

```
Browser (Chrome, Firefox)        Node.js
--------------------------       -------------------------------
Runs JS in the browser sandbox   Runs JS anywhere (CLI, server)
Has: window, document, fetch     Has: fs, path, process, http
Cannot access: file system       Cannot access: DOM, window
```

**Why front-end developers need it:**
- npm (Node Package Manager) is bundled with Node -- it is how you install React, TypeScript, Jest, and everything else.
- Build tools like Vite and Webpack run in Node to transform, bundle, and optimize your code before it reaches the browser.
- The TypeScript compiler (`tsc`) runs in Node.

### Installing Node

Download from [nodejs.org](https://nodejs.org). The LTS (Long-Term Support) version is recommended. Verify:

```bash
node --version    # v20.x.x (LTS as of 2026)
npm --version     # 10.x.x
```

---

## `package.json`

Every Node project has a `package.json` at its root. It is the manifest of your project: what it is, what it depends on, and how to run it.

```json
{
  "name": "product-catalog",
  "version": "1.0.0",
  "description": "React product catalog demo app",
  "private": true,
  "scripts": {
    "dev": "vite",
    "build": "tsc && vite build",
    "preview": "vite preview",
    "test": "jest",
    "test:coverage": "jest --coverage",
    "lint": "eslint src --ext .ts,.tsx"
  },
  "dependencies": {
    "react": "^18.3.0",
    "react-dom": "^18.3.0"
  },
  "devDependencies": {
    "@types/react": "^18.3.0",
    "@types/react-dom": "^18.3.0",
    "typescript": "^5.4.0",
    "vite": "^5.2.0",
    "@vitejs/plugin-react": "^4.2.0",
    "jest": "^29.0.0",
    "ts-jest": "^29.0.0"
  }
}
```

### Key Fields

| Field | Purpose |
|-------|---------|
| `name` | Package identifier (must be lowercase, no spaces) |
| `version` | SemVer: `MAJOR.MINOR.PATCH` |
| `private: true` | Prevents accidental `npm publish` |
| `scripts` | Runnable commands via `npm run <name>` |
| `dependencies` | Packages needed at runtime |
| `devDependencies` | Packages needed only during development/build |

### `dependencies` vs `devDependencies`

```bash
# Saved to dependencies (shipped to production)
npm install react react-dom

# Saved to devDependencies (build/test tools only)
npm install --save-dev typescript jest eslint
```

---

## npm Commands

```bash
# Initialize a new package.json (interactively)
npm init

# Initialize with defaults (skip prompts)
npm init -y

# Install all dependencies listed in package.json
npm install
npm ci                    # cleaner, faster, used in CI -- requires package-lock.json

# Install a specific package
npm install axios                     # → dependencies
npm install --save-dev jest           # → devDependencies
npm install --save-exact react@18.3.0 # pin exact version

# Uninstall
npm uninstall axios

# Run a script from package.json
npm run dev
npm run build
npm test                  # shortcut for npm run test

# List installed packages
npm list
npm list --depth=0        # top-level only

# Audit for security vulnerabilities
npm audit
npm audit fix             # auto-fix where possible

# Update packages
npm update
npm outdated              # show which packages have newer versions
```

### `package-lock.json`

Generated automatically by npm. Records the **exact** version of every dependency (including transitive ones). Always commit this file -- it ensures everyone on the team and CI builds use identical dependency versions.

---

## Semantic Versioning (SemVer)

Package versions follow the pattern `MAJOR.MINOR.PATCH`:

| Part | Increments when |
|------|----------------|
| `MAJOR` | Breaking changes |
| `MINOR` | New backwards-compatible features |
| `PATCH` | Bug fixes |

Version range prefixes in `package.json`:

```
"react": "^18.3.0"   // ^ = compatible: allows 18.x.x but not 19.x.x
"lodash": "~4.17.21" // ~ = patch: allows 4.17.x but not 4.18.x
"axios": "1.6.8"     // exact version -- no automatic updates
```

---

## Single Page Applications (SPAs)

A traditional **Multi-Page Application (MPA)** loads a new HTML page from the server on every navigation.

A **Single Page Application (SPA)** loads one HTML file once. After that, all navigation is handled by JavaScript -- the URL changes, the UI updates, but no full page reload occurs.

```
MPA (Traditional)
  Click "Products" → browser requests /products.html → server sends full page → browser reloads

SPA (React)
  Click "Products" → React Router updates URL → React renders <ProductList /> → no reload
```

### Trade-offs

| | MPA | SPA |
|--|-----|-----|
| Initial load | Fast first page | Larger JS bundle download |
| Subsequent navigation | Full reload | Instant (no network round-trip) |
| SEO | Easy (server sends full HTML) | Requires extra work (SSR/SSG) |
| Back/forward browser buttons | Works natively | Needs React Router |
| Complexity | Lower for simple sites | Higher, but better UX for apps |

React builds SPAs by default. Server-Side Rendering (SSR) frameworks like Next.js address the SEO trade-off -- you will cover those in a future track.

---

## Webpack and Bundling

Modern front-end code is split across hundreds of files: your components, npm packages, CSS, images. Browsers cannot load hundreds of separate files efficiently. A **bundler** solves this.

### What Webpack Does

```
Source Files (your code + node_modules)
  src/App.tsx
  src/components/ProductCard.tsx
  node_modules/react/...
  node_modules/axios/...
  src/styles/main.css

          ↓ Webpack processes ↓

Output (optimized for the browser)
  dist/bundle.js      (all JS merged, minified, tree-shaken)
  dist/bundle.css     (all styles merged)
  dist/index.html     (references the bundle)
```

### Webpack Concepts

| Concept | What it is |
|---------|-----------|
| **Entry** | The starting file Webpack begins from (`src/main.tsx`) |
| **Module graph** | Webpack follows all `import` statements to find every dependency |
| **Loader** | Transforms a non-JS file type so Webpack can process it (e.g., `ts-loader` for TypeScript, `css-loader` for CSS) |
| **Plugin** | Post-processes the output (e.g., `HtmlWebpackPlugin` injects the bundle into `index.html`) |
| **Output** | The final bundled file(s) written to `dist/` |
| **Dev server** | In-memory bundle with hot module replacement (HMR) -- changes appear in the browser without a full reload |
| **Tree shaking** | Dead code elimination -- unused exports from packages are stripped from the bundle |

### Vite vs Webpack

This course uses **Vite** instead of raw Webpack. Vite is a modern build tool that uses native ES modules in development (no bundling at all during dev -- extremely fast) and Rollup (similar to Webpack) for production builds.

```bash
# Create a Vite + React + TypeScript project
npm create vite@latest my-app -- --template react-ts
cd my-app
npm install
npm run dev   # starts dev server at http://localhost:5173
```

You get:
```
my-app/
  public/          -- static assets (favicon, etc.)
  src/
    main.tsx       -- entry point: renders <App /> into #root
    App.tsx        -- root component
    App.css
    vite-env.d.ts  -- Vite type declarations
  index.html       -- shell HTML (Vite injects scripts here)
  vite.config.ts
  tsconfig.json
  package.json
```

Vite and Webpack are interchangeable concepts at this level -- both bundle your code. Vite is faster for development.

---

## Summary
- Node.js runs JavaScript outside the browser and is required for all modern front-end tooling.
- `package.json` defines your project, its dependencies, and runnable scripts.
- `npm install` installs packages; `package-lock.json` pins exact versions -- always commit it.
- SPAs load once and handle navigation in JavaScript -- better UX for apps, requires more tooling.
- Webpack and Vite bundle hundreds of source files into optimized assets the browser can load efficiently.

## Additional Resources
- [Node.js: Official documentation](https://nodejs.org/en/docs)
- [npm: Documentation](https://docs.npmjs.com)
- [Vite: Getting Started](https://vitejs.dev/guide/)
