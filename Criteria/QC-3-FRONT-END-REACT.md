# QC 3 - Front End Technologies - Criteria

## React

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must Know | Describe and implement functional components in React. | `function Greeting({ name }) { return <h1>Hello {name}</h1>; }` — a JS function returning JSX, the modern default over class components. |
| Must Know | Explain the difference between Single Page Applications and Multi Page Applications. | SPAs load one HTML shell and update the view client-side via JS (faster nav, richer state). MPAs request a new HTML document from the server per route (simpler, better default SEO). |
| Must Know | Utilize and explain common React hooks: useState, useEffect, and useContext. | `useState` holds local state; `useEffect` runs side-effects after render; `useContext` reads a value from a `Context.Provider` higher in the tree. |
| Must Know | Pass props to components and manage local component state. | `<UserCard user={u} />` passes data down (read-only); the child uses `useState` for state it owns internally. |
| Must Know | Create and run a React application using Vite CLI. | `npm create vite@latest my-app -- --template react-ts`<br>`cd my-app && npm install && npm run dev` |
| Must Know | Explain the lifecycle of a React component. | Mount (initial render + effects) → Update (re-render on state/prop change) → Unmount (cleanup). Hooks model lifecycle via `useEffect` dependencies and cleanup functions. |
| Must Know | Describe how the React Virtual DOM works and how it improves performance. | React renders to an in-memory tree (vDOM), diffs it against the previous tree, and applies only the minimal set of real DOM mutations — avoiding expensive full-DOM reflows. |
| Must Know | Make HTTP requests using Axios or Fetch and handle the response. | `const { data } = await axios.get('/api/products');`<br>`setProducts(data);` Wrap in `try/catch` and trigger from a `useEffect` or event handler. |
| Must Know | Write and explain JSX syntax and how it integrates with JavaScript. | `const el = <h1 className="title">{user.name}</h1>;` JSX is syntactic sugar over `React.createElement` calls — Vite/Babel transpile it to JS. |
| Must Know | Compare and contrast function components and class components. | Function components use hooks for state/effects and are concise. Class components use `this.state`, `this.setState`, and lifecycle methods (`componentDidMount`, etc.) — legacy style. |
| Must Know | Use useReducer for complex state management scenarios. | `const [state, dispatch] = useReducer(reducer, initialState);` — preferred over `useState` when next state depends on prior state across multiple actions (e.g., cart, form wizard). |
| Must Know | Explain and apply the principles of state immutability in React. | Never mutate state directly; always produce a new object/array. `setItems([...items, newItem])` not `items.push(newItem)`. Reference equality drives re-render decisions. |
| Must Know | Handle user input through form elements and manage form state. | `<input value={name} onChange={e => setName(e.target.value)} />` — controlled input, state lives in React. |
| Must Know | Implement component communication through props and callbacks (Parent to Child & vice versa). | Parent → Child via props (`<Child value={x} />`). Child → Parent via callback prop (`<Child onSubmit={handleSubmit} />`). |
| Must Know | Build and use nested component structures to model UI architecture. | `<App><Header/><Main><ProductList><ProductCard/></ProductList></Main></App>` — composition over inheritance, each component owns one concern. |
| Must Know | Use React Router to implement navigation in a single-page application. | `<BrowserRouter><Routes><Route path="/products" element={<Products/>}/></Routes></BrowserRouter>` — client-side routing without full page reloads. |
| Must Know | Apply styling to components using inline styles, CSS modules, or external stylesheets. | `<div style={{color:'red'}}>` (inline), `import s from './X.module.css'; <div className={s.title}>` (modules), or `import './app.css'` (global). |
| Must Know | Use Lists and Keys correctly to render dynamic components efficiently. | `{items.map(i => <Item key={i.id} data={i} />)}` — stable, unique keys (DB IDs) let React reconcile lists without re-mounting unchanged rows. |
| Should Know | Use Context.Provider tags to wrap components and distribute application state. | `<AuthContext.Provider value={{user, login}}>{children}</AuthContext.Provider>` — descendants read with `useContext(AuthContext)`. |
| Should Know | Route users between Components through the use of BrowserRouter. | Wrap the app in `<BrowserRouter>` once at the root; use `<Link to="/about">` and `useNavigate()` to move between routed components. |
| Should Know | Leverage route guards to change the routing behavior based on the given state. | `function ProtectedRoute({ children }) { const { user } = useAuth(); return user ? children : <Navigate to="/login" />; }` |
| Should Know | Use a Reducer to manage a set of complex known states. | `function cartReducer(state, action) { switch(action.type) { case 'ADD': return {...state, items:[...state.items, action.item]}; } }` |
| Should Know | Conditionally render a component based on user interaction and/or state. | `{isLoggedIn ? <Dashboard/> : <Login/>}` or `{error && <ErrorBanner msg={error}/>}` — short-circuit / ternary in JSX. |
| Should Know | Describe benefits of TypeScript in React development. | Compile-time type-checking on props, state, and API responses; better IDE autocomplete; safer refactors; explicit DTO contracts shared with the backend. |
| Should Know | Leverage NPM libraries in a React Project Evaluation to add functionality. | `npm install axios react-router-dom` — pull packages from the npm registry, recorded in `package.json`/`package-lock.json`. |
| Should Know | Lift state up to a parent component to share data between child components. | When two siblings need the same data, move the `useState` to their nearest common parent and pass value + setter down as props. |
| Should Know | Describe how one-way data flow works in React. | Data flows parent → child via props. Children notify parents through callbacks but cannot directly mutate parent state — keeps state changes predictable and traceable. |
| Should Know | Build a reusable component using TSX with type-checked props. | `type Props = { label: string; onClick: () => void };`<br>`export function Button({ label, onClick }: Props) { return <button onClick={onClick}>{label}</button>; }` |
| Nice to have | Use createContext and Context.Provider to manage global state. | `const ThemeContext = createContext<Theme>('light');` then wrap with `<ThemeContext.Provider value={theme}>` and consume via `useContext(ThemeContext)`. |
| Nice to have | Use refs to store information without triggering a re-render. | `const inputRef = useRef<HTMLInputElement>(null);` `inputRef.current?.focus();` — mutable holder for DOM nodes or values that shouldn't cause re-renders. |
| Nice to have | Use Jest and a React testing library to test components. | `render(<Button label="Save" onClick={fn}/>); fireEvent.click(screen.getByText('Save')); expect(fn).toHaveBeenCalled();` |
| Nice to have | Leverage advanced routing techniques to create parent-child routing, or through passing variables into routes. | Nested `<Route>` with `<Outlet/>` in the parent; URL params like `/products/:id` read via `useParams()`. |
| Nice to have | Explain and implement higher-order and container components for reusable logic. | HOC: `const withAuth = Cmp => props => <Cmp {...props} user={useAuth()}/>`. Container: smart component that fetches data and renders a presentational child. |
| Nice to have | Compare and implement controlled vs uncontrolled components in form handling. | Controlled: `<input value={x} onChange={...}/>` (React owns state). Uncontrolled: `<input ref={ref}/>` (DOM owns state, read via ref on submit). |

## HTML + CSS

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must Know | Understand what HTML is | HyperText Markup Language — declarative tag-based language describing the structure and semantics of a web page. Browsers parse it into the DOM. |
| Must Know | Describe the structure of an HTML Document and what is included in the different sections | `<!DOCTYPE html><html><head>...meta, title, links...</head><body>...visible content...</body></html>`. `head` holds metadata; `body` holds rendered content. |
| Must Know | List HTML Common Tags and why they are different from divs | Semantic tags (`<header>`, `<nav>`, `<main>`, `<article>`, `<section>`, `<footer>`) convey meaning to browsers, screen readers, and SEO crawlers. `<div>` is a generic, meaningless container. |
| Must Know | Describe how/where you link an external CSS sheet into an HTML Document | `<link rel="stylesheet" href="styles.css">` placed inside `<head>` so styles load before body content renders. |
| Must Know | Describe how/where you link an external JS sheet into an HTML Document | `<script src="app.js" defer></script>` typically at the end of `<body>` or with `defer`/`async` to avoid blocking parsing. |
| Must Know | Understand the structure of a CSS style rule | `selector { property: value; property: value; }` — e.g., `.btn { color: white; background: blue; }`. |
| Must Know | Understand and explain the CSS box model | Every element is a box: `content` → `padding` → `border` → `margin`. `box-sizing: border-box` makes width/height include padding+border. |
| Must Know | Describe the different ways to add styling to an HTML Document | Inline (`style` attribute), internal (`<style>` block in `<head>`), external (`<link>` to a `.css` file). |
| Must Know | Understand the correct syntax for styling different elements such as by tag, class, id, etc | Tag: `p { }`. Class: `.warning { }`. ID: `#header { }`. Attribute: `[type="text"] { }`. |
| Must Know | Know CSS priority in regards to inline, internal and external | Specificity + source order: inline > id > class > tag. Among equally specific rules, the later one wins. `!important` overrides everything (use sparingly). |
| Should Know | Construct an HTML form | `<form action="/submit" method="post"><label>Name:<input name="name"/></label><button type="submit">Send</button></form>` |
| Should Know | Add comments to an HTML Document | `<!-- this is an HTML comment -->` — ignored by browsers, useful for notes/disabling markup. |
| Should Know | Describe the syntax of different HTML elements/tags including the table tag | `<table><thead><tr><th>Header</th></tr></thead><tbody><tr><td>Cell</td></tr></tbody></table>` |
| Should Know | Take in user input using a variety of input tags (text, checkbox, etc) | `<input type="text">`, `<input type="checkbox">`, `<input type="radio">`, `<input type="email">`, `<select>`, `<textarea>`. |
| Nice to have | Describe the benefits of combinators and how to use them | Descendant `A B`, child `A > B`, adjacent sibling `A + B`, general sibling `A ~ B`. Used to target elements based on relational position in the DOM. |
| Nice to have | Understand how to make responsive webpages using CSS | Media queries: `@media (max-width: 768px) { ... }`. Flexbox / Grid for fluid layouts. Relative units (`rem`, `%`, `vw`). |
| Nice to have | Understand how to use BootStrap | Pre-built CSS framework — `<div class="container"><div class="row"><div class="col-md-6">...</div></div></div>`. Add via CDN or `npm install bootstrap`. |

## Javascript

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must Know | Describe what the DOM is | Document Object Model — the in-memory tree representation of an HTML document that JS can read and modify to dynamically change the page. |
| Must Know | Query the DOM for elements | `document.getElementById('id')`, `document.querySelector('.class')`, `document.querySelectorAll('div.item')`. |
| Must Know | Describe what event listeners are | Functions registered to respond to events (click, input, submit). `el.addEventListener('click', e => {...})`. |
| Must Know | Insert new elements into the DOM | `const el = document.createElement('li'); el.textContent = 'Item'; parent.appendChild(el);` |
| Must Know | Describe what Ajax is | Asynchronous JavaScript and XML — pattern of using JS to send/receive data from the server without a full page reload. Modernly uses Fetch/JSON, not XML. |
| Must Know | Explain what a JavaScript Promise is and when it is used to handle asynchronous operations. | An object representing the eventual result of an async operation (pending → fulfilled / rejected). Used for I/O like network calls, file reads, timers. |
| Must Know | Describe what type of object the Fetch API returns | `fetch()` returns a `Promise<Response>`. The `Response` exposes `.json()`, `.text()`, `.status`, `.ok`, etc. |
| Must Know | Explain what is JSON | JavaScript Object Notation — lightweight text format for structured data: `{"name":"Alice","age":30}`. Standard payload for REST APIs. |
| Must Know | Handle a failed request when using the Fetch API | `if (!res.ok) throw new Error(res.statusText);` — Fetch only rejects on network failure, not HTTP error status codes; check `res.ok` manually. |
| Must Know | Describe the different promise methods | `.then()` chains success handlers, `.catch()` handles rejections, `.finally()` runs regardless. Static: `Promise.all`, `Promise.race`, `Promise.allSettled`. |
| Must Know | I can explain the difference between synchronous and asynchronous programming | Synchronous: each statement blocks until done. Asynchronous: long-running ops return immediately and notify later via callback/Promise — event loop handles continuation. |
| Should Know | List the steps to sending an HTTP request using the Fetch API | 1) Call `fetch(url, options)`. 2) Await the `Response`. 3) Check `res.ok`. 4) Parse with `res.json()`. 5) Use the data. 6) Catch errors. |
| Should Know | List the steps to sending an Ajax request (using the XHR object) | 1) `const xhr = new XMLHttpRequest()`. 2) `xhr.open('GET', url)`. 3) Set `xhr.onload`/`onerror`. 4) `xhr.send()`. |
| Should Know | Describe the difference between Fetch and XHR | Fetch is Promise-based, modern, cleaner API. XHR is callback/event-based, older, more verbose, but supports upload progress events natively. |
| Should Know | Describe and explain the event loop | Single-threaded JS runtime model: call stack runs sync code; the loop pulls callbacks from the task/microtask queues onto the stack when it's empty. |
| Should Know | Describe what async/await is and how they compare to using .then() | Syntactic sugar over Promises. `const data = await fetch(url).then(r => r.json())` reads top-to-bottom like sync code, easier error handling via `try/catch`. |
| Should Know | Explain what is JSON.stringify() and JSON.parse() | `JSON.stringify(obj)` serializes a JS value to a JSON string. `JSON.parse(str)` deserializes a JSON string back into a JS value. |
| Nice to have | Describe what bubbling and capturing are and their difference | Capturing: event travels root → target. Bubbling: event travels target → root. Default listener phase is bubbling; pass `{capture: true}` to listen on the way down. |
| Nice to have | Describe some methods on the event object and what they do | `e.preventDefault()` blocks default browser action (form submit, link nav). `e.stopPropagation()` halts further bubbling/capturing. `e.target` is the element that triggered the event. |
| Nice to have | Explain how to chain multiple asynchronous operations using Promises or async/await | `.then(a).then(b).then(c)` chains values through. With async/await: `const a = await f(); const b = await g(a); const c = await h(b);` |
| Nice to have | Implement error handling using try-catch blocks with async/await | `try { const data = await fetch(url).then(r => r.json()); } catch (err) { console.error(err); }` |

## Typescript

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Should Know | Compare/contrast TypeScript to JavaScript | TS is a typed superset of JS — adds static types, interfaces, generics, enums. Compiles down to plain JS; types vanish at runtime. |
| Should Know | Describe and implement basic types in TypeScript | `let n: number = 5; let s: string = 'hi'; let ok: boolean = true; let arr: number[] = [1,2,3];` |
| Should Know | Implement user defined types in TypeScript. | `interface User { id: number; name: string; }` or `type User = { id: number; name: string };`. |
| Should Know | Describe and implement casting in TypeScript. | `const el = document.getElementById('x') as HTMLInputElement;` or `<HTMLInputElement>document.getElementById('x')`. |
| Should Know | Describe and demonstrate process to transpile and run TypeScript | `tsc app.ts` compiles to `app.js`; or `ts-node app.ts` runs directly. In Vite/React projects, the build tool handles TS automatically. |
| Must Know | Implement TypeScript outside of Angular/React environments using plain .ts files. | `npm i -D typescript && npx tsc --init` creates `tsconfig.json`. Author `.ts` files, run `tsc` to emit `.js` output. |
| Must Know | Describe the purpose of the "strict" flag in the tsconfig.js file. | `"strict": true` enables all strict type-checking options (`noImplicitAny`, `strictNullChecks`, etc.) — catches more bugs at compile time. |
| Must Know | Describe and implement union types. | `let id: string \| number;` — variable accepts either type. Narrow with `typeof id === 'string'` before string-only operations. |
| Must Know | Describe and implement type guards. | `function isUser(x: any): x is User { return x && typeof x.id === 'number'; }` — narrows a value's type within a conditional block. |
| Must Know | Describe and implement type aliasing. | `type ID = string \| number;` `type Callback = (err: Error \| null, data?: string) => void;` — give a complex type a friendly name. |
| Must Know | Describe and implement Assertion Functions. | `function assert(cond: any, msg?: string): asserts cond { if (!cond) throw new Error(msg); }` — narrows the type for the rest of the scope when it returns. |
| Nice to have | Configure the typescript compiler using options in the tsconfig.json based on Project Evaluation needs. | Adjust `target`, `module`, `outDir`, `strict`, `jsx`, `paths`, etc., to match runtime and tooling requirements. |
| Nice to have | Describe and leverage generic types | `function first<T>(arr: T[]): T { return arr[0]; }` — type parameter `T` is inferred at call site, preserving type safety across reusable code. |
