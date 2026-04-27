# Testing with Jest

## Learning Objectives
- Write test files using `describe`, `it`, and `expect`
- Apply the Arrange-Act-Assert pattern
- Mock dependencies with `jest.fn()` and `jest.spyOn()`
- Read a code coverage report

## Why This Matters
Your ASP.NET Core code had unit tests. This week your front-end JS/TS code gets them too. Jest is the standard test runner in the React ecosystem -- the same tool is used for utility functions, React component tests (with React Testing Library), and mocking API calls in tests.

---

## Setup

Jest is installed as a dev dependency:

```bash
npm install --save-dev jest
# For TypeScript projects:
npm install --save-dev jest ts-jest @types/jest
```

`package.json` test script:

```json
{
  "scripts": {
    "test": "jest",
    "test:watch": "jest --watch",
    "test:coverage": "jest --coverage"
  }
}
```

Minimal `jest.config.js` for TypeScript:

```javascript
module.exports = {
  preset: "ts-jest",
  testEnvironment: "node",
};
```

---

## Test File Structure

Jest automatically finds files matching `*.test.js`, `*.spec.js`, `*.test.ts`, or files in a `__tests__` folder.

```javascript
// src/utils/math.test.js

describe("add()", () => {
  it("returns the sum of two positive numbers", () => {
    // Arrange
    const a = 3;
    const b = 4;

    // Act
    const result = add(a, b);

    // Assert
    expect(result).toBe(7);
  });

  it("handles negative numbers", () => {
    expect(add(-1, -2)).toBe(-3);
  });
});
```

| Function | Purpose |
|----------|---------|
| `describe(name, fn)` | Groups related tests (can be nested) |
| `it(name, fn)` | Defines a single test case (alias: `test()`) |
| `beforeEach(fn)` | Runs before each test in the block |
| `afterEach(fn)` | Runs after each test in the block |
| `beforeAll(fn)` | Runs once before all tests in the block |
| `afterAll(fn)` | Runs once after all tests in the block |

---

## The AAA Pattern

Every test should follow three distinct phases:

```
Arrange  -- set up the data and dependencies the test needs
Act      -- call the function or trigger the behavior under test
Assert   -- verify the output matches expectations
```

Separating these phases makes tests easy to read and diagnose when they fail.

```javascript
it("filters products by availability", () => {
  // Arrange
  const products = [
    { id: 1, name: "A", inStock: true },
    { id: 2, name: "B", inStock: false },
    { id: 3, name: "C", inStock: true },
  ];

  // Act
  const result = filterAvailable(products);

  // Assert
  expect(result).toHaveLength(2);
  expect(result[0].name).toBe("A");
});
```

---

## `expect` Matchers

```javascript
// Equality
expect(2 + 2).toBe(4);            // strict equality (===)
expect({ a: 1 }).toEqual({ a: 1 }); // deep equality (use for objects/arrays)

// Truthiness
expect(true).toBeTruthy();
expect(0).toBeFalsy();
expect(null).toBeNull();
expect(undefined).toBeUndefined();
expect("text").toBeDefined();

// Numbers
expect(5).toBeGreaterThan(3);
expect(5).toBeGreaterThanOrEqual(5);
expect(3).toBeLessThan(5);
expect(0.1 + 0.2).toBeCloseTo(0.3); // floating-point safe

// Strings
expect("Hello World").toContain("World");
expect("hello").toMatch(/^hel/);

// Arrays
expect([1, 2, 3]).toContain(2);
expect([1, 2, 3]).toHaveLength(3);
expect([1, 2, 3]).toEqual(expect.arrayContaining([1, 3])); // subset

// Objects
expect({ a: 1, b: 2 }).toMatchObject({ a: 1 }); // subset match
expect({ a: 1 }).toHaveProperty("a", 1);

// Errors
expect(() => divide(1, 0)).toThrow();
expect(() => divide(1, 0)).toThrow("Division by zero");
expect(() => divide(1, 0)).toThrow(Error);

// Negation
expect(null).not.toBeDefined();
expect("hello").not.toContain("xyz");
```

---

## Mock Functions

Mocks replace real dependencies (API calls, timers, file system) with controlled fakes.

### `jest.fn()`

```javascript
const mockCallback = jest.fn();
[1, 2, 3].forEach(mockCallback);

expect(mockCallback).toHaveBeenCalledTimes(3);
expect(mockCallback).toHaveBeenCalledWith(1, 0, [1, 2, 3]); // (value, index, array)
expect(mockCallback).toHaveBeenLastCalledWith(3, 2, [1, 2, 3]);

// Control the return value
const mockAdd = jest.fn().mockReturnValue(42);
mockAdd(1, 2); // returns 42

// Return different values on sequential calls
const mockFetch = jest.fn()
  .mockReturnValueOnce({ status: 200 })
  .mockReturnValueOnce({ status: 404 });

// Return a resolved Promise
const mockApi = jest.fn().mockResolvedValue({ id: 1, name: "Laptop" });
await mockApi(); // { id: 1, name: "Laptop" }

// Return a rejected Promise
const mockFail = jest.fn().mockRejectedValue(new Error("Network error"));
```

### `jest.spyOn()`

Spy on an existing method without replacing it:

```javascript
const spy = jest.spyOn(console, "error").mockImplementation(() => {});

runCodeThatCallsConsoleError();

expect(spy).toHaveBeenCalledWith("Expected error message");
spy.mockRestore(); // put the original back
```

### Mocking Modules

```javascript
// In the test file, mock an entire module
jest.mock("./api/productService", () => ({
  getProducts: jest.fn().mockResolvedValue([{ id: 1, name: "Laptop" }]),
}));

import { getProducts } from "./api/productService";

it("fetches and displays products", async () => {
  const products = await getProducts();
  expect(products).toHaveLength(1);
});
```

---

## Code Coverage

```bash
npm test -- --coverage
```

Jest outputs a coverage summary table:

```
--------------------|---------|----------|---------|---------|
File                | % Stmts | % Branch | % Funcs | % Lines |
--------------------|---------|----------|---------|---------|
utils/math.js       |   91.67 |    83.33 |   100   |   91.67 |
--------------------|---------|----------|---------|---------|
```

| Column | Measures |
|--------|---------|
| **Stmts** | Percentage of statements executed by tests |
| **Branch** | Percentage of `if`/`switch` branches taken |
| **Funcs** | Percentage of functions called |
| **Lines** | Percentage of source lines executed |

A detailed HTML report is generated in `coverage/lcov-report/index.html`.

Set coverage thresholds in `jest.config.js` to fail the build if coverage drops:

```javascript
module.exports = {
  coverageThreshold: {
    global: {
      branches: 80,
      functions: 80,
      lines: 80,
      statements: 80,
    },
  },
};
```

---

## Summary
- Test files follow the naming convention `*.test.js` or `*.test.ts`.
- Use `describe` to group and `it`/`test` for individual cases.
- AAA: Arrange setup, Act on the code under test, Assert on the result.
- `jest.fn()` creates controllable fakes. `jest.spyOn()` wraps an existing method.
- `--coverage` shows which code paths your tests exercise.

## Additional Resources
- [Jest: Getting Started](https://jestjs.io/docs/getting-started)
- [Jest: Expect API reference](https://jestjs.io/docs/expect)
- [Jest: Mock Functions](https://jestjs.io/docs/mock-functions)
