# SQL Joins

## Learning Objectives

- Understand why joins are necessary in relational databases
- Master different join types: INNER, LEFT, RIGHT, FULL, CROSS
- Apply self joins for hierarchical data
- Write efficient join queries

## Why This Matters

Normalized databases store data across multiple tables to reduce redundancy. Joins bring this data back together for meaningful queries. Without joins, you could only query one table at a time. Mastering joins is essential for extracting insights from relational databases.

## The Concept

### Why Joins?

Consider this normalized design:

```
CUSTOMERS                     ORDERS
+----+---------+             +----+--------+----------+
| id | name    |             | id | cust_id| total    |
+----+---------+             +----+--------+----------+
| 1  | Alice   |             | 1  | 1      | 150.00   |
| 2  | Bob     |             | 2  | 1      | 200.00   |
| 3  | Carol   |             | 3  | 2      | 75.00    |
+----+---------+             +----+--------+----------+
```

To see "customer name with their orders," you need to JOIN these tables.

### Sample Data for Examples

```sql
-- Customers table
CREATE TABLE customers (
    id INT PRIMARY KEY,
    name VARCHAR(100)
);

INSERT INTO customers VALUES (1, 'Alice'), (2, 'Bob'), (3, 'Carol');

-- Orders table
CREATE TABLE orders (
    id INT PRIMARY KEY,
    customer_id INT,
    total DECIMAL(10,2)
);

INSERT INTO orders VALUES (1, 1, 150.00), (2, 1, 200.00), (3, 2, 75.00);
```

### INNER JOIN

Returns only rows that have matching values in both tables.

```sql
SELECT customers.name, orders.total
FROM customers
INNER JOIN orders ON customers.id = orders.customer_id;

-- Result:
-- +---------+--------+
-- | name    | total  |
-- +---------+--------+
-- | Alice   | 150.00 |
-- | Alice   | 200.00 |
-- | Bob     | 75.00  |
-- +---------+--------+
-- Carol has no orders, so she doesn't appear
```

**Visual Representation:**
```
    CUSTOMERS          ORDERS
   +---------+       +---------+
   |         |       |         |
   |    +----+-------+----+    |
   |    |  INNER JOIN    |    |
   |    +----------------+    |
   |         |       |         |
   +---------+       +---------+
```

### LEFT JOIN (LEFT OUTER JOIN)

Returns all rows from the left table and matching rows from the right table. NULLs for non-matches.

```sql
SELECT customers.name, orders.total
FROM customers
LEFT JOIN orders ON customers.id = orders.customer_id;

-- Result:
-- +---------+--------+
-- | name    | total  |
-- +---------+--------+
-- | Alice   | 150.00 |
-- | Alice   | 200.00 |
-- | Bob     | 75.00  |
-- | Carol   | NULL   |  <- Carol included with NULL order
-- +---------+--------+
```

**Use Cases:**
- Find all customers, including those without orders
- Find items with no related records

```sql
-- Find customers with no orders
SELECT customers.name
FROM customers
LEFT JOIN orders ON customers.id = orders.customer_id
WHERE orders.id IS NULL;
```

### RIGHT JOIN (RIGHT OUTER JOIN)

Returns all rows from the right table and matching rows from the left table.

```sql
SELECT customers.name, orders.total
FROM customers
RIGHT JOIN orders ON customers.id = orders.customer_id;
```

In practice, RIGHT JOIN is rarely used. You can always rewrite it as a LEFT JOIN by swapping the table order.

### FULL OUTER JOIN

Returns all rows from both tables, with NULLs where there is no match.

**Note:** MySQL does not directly support FULL OUTER JOIN. Use UNION:

```sql
-- Simulating FULL OUTER JOIN in MySQL
SELECT customers.name, orders.total
FROM customers
LEFT JOIN orders ON customers.id = orders.customer_id
UNION
SELECT customers.name, orders.total
FROM customers
RIGHT JOIN orders ON customers.id = orders.customer_id;
```

### CROSS JOIN

Returns the Cartesian product (every combination of rows).

```sql
SELECT customers.name, orders.total
FROM customers
CROSS JOIN orders;

-- Result: 9 rows (3 customers x 3 orders = all combinations)
-- +---------+--------+
-- | name    | total  |
-- +---------+--------+
-- | Alice   | 150.00 |
-- | Alice   | 200.00 |
-- | Alice   | 75.00  |
-- | Bob     | 150.00 |
-- | Bob     | 200.00 |
-- | Bob     | 75.00  |
-- | Carol   | 150.00 |
-- | Carol   | 200.00 |
-- | Carol   | 75.00  |
-- +---------+--------+
```

**Use Cases:**
- Generate all possible combinations
- Create test data
- Calendar/time slot generation

### Self Join

A table joined to itself, useful for hierarchical data.

```sql
CREATE TABLE employees (
    id INT PRIMARY KEY,
    name VARCHAR(100),
    manager_id INT
);

INSERT INTO employees VALUES 
(1, 'Alice', NULL),      -- CEO, no manager
(2, 'Bob', 1),           -- Reports to Alice
(3, 'Carol', 1),         -- Reports to Alice
(4, 'David', 2);         -- Reports to Bob

-- Find employees with their managers
SELECT 
    e.name AS employee,
    m.name AS manager
FROM employees e
LEFT JOIN employees m ON e.manager_id = m.id;

-- Result:
-- +----------+---------+
-- | employee | manager |
-- +----------+---------+
-- | Alice    | NULL    |
-- | Bob      | Alice   |
-- | Carol    | Alice   |
-- | David    | Bob     |
-- +----------+---------+
```

### Multiple Joins

Join more than two tables:

```sql
SELECT 
    c.name AS customer,
    o.id AS order_id,
    p.name AS product,
    oi.quantity
FROM customers c
INNER JOIN orders o ON c.id = o.customer_id
INNER JOIN order_items oi ON o.id = oi.order_id
INNER JOIN products p ON oi.product_id = p.id;
```

### Join with Conditions

Add WHERE clauses to filter joined results:

```sql
SELECT customers.name, orders.total
FROM customers
INNER JOIN orders ON customers.id = orders.customer_id
WHERE orders.total > 100
ORDER BY orders.total DESC;
```

### Join Syntax Variations

**Explicit JOIN (Recommended):**
```sql
SELECT c.name, o.total
FROM customers c
INNER JOIN orders o ON c.id = o.customer_id;
```

**Implicit JOIN (Old Style):**
```sql
SELECT c.name, o.total
FROM customers c, orders o
WHERE c.id = o.customer_id;
```

Use explicit JOIN syntax for clarity and to avoid accidental Cartesian products.

### Table Aliases

Use short aliases for cleaner queries:

```sql
SELECT c.name, o.total, oi.quantity
FROM customers c
JOIN orders o ON c.id = o.customer_id
JOIN order_items oi ON o.id = oi.order_id;
```

### Join Summary

| Join Type | Returns |
|-----------|---------|
| INNER JOIN | Only matching rows from both tables |
| LEFT JOIN | All rows from left, matching from right (NULL if no match) |
| RIGHT JOIN | All rows from right, matching from left (NULL if no match) |
| FULL OUTER | All rows from both (simulated with UNION in MySQL) |
| CROSS JOIN | Cartesian product (all combinations) |
| SELF JOIN | Table joined to itself |

## Summary

- Joins combine data from multiple tables
- INNER JOIN returns only matching rows
- LEFT JOIN includes all left table rows with NULLs for non-matches
- RIGHT JOIN includes all right table rows (rarely used)
- CROSS JOIN produces all possible combinations
- Self joins connect a table to itself for hierarchical data
- Use explicit JOIN syntax and table aliases for clarity

## Additional Resources

- [MySQL JOIN Reference](https://dev.mysql.com/doc/refman/8.0/en/join.html)
- [Visual Explanation of SQL Joins](https://blog.codinghorror.com/a-visual-explanation-of-sql-joins/)
- [SQL Joins Tutorial](https://www.w3schools.com/sql/sql_join.asp)
