# Subqueries and Advanced Queries

## Learning Objectives

- Understand what subqueries are and when to use them
- Differentiate between scalar, row, and table subqueries
- Apply correlated subqueries for row-by-row comparisons
- Decide when to use subqueries vs. joins

## Why This Matters

Subqueries allow you to nest queries within queries, enabling complex logic that would be difficult or impossible with simple statements. They are essential for analytical queries, data comparisons, and solving problems that require multiple levels of filtering.

## The Concept

### What is a Subquery?

A subquery is a query nested inside another query. The inner query executes first, and its result is used by the outer query.

```sql
-- Find employees who earn above average
SELECT name, salary
FROM employees
WHERE salary > (SELECT AVG(salary) FROM employees);
```

### Types of Subqueries

**By Location:**
- In WHERE clause
- In FROM clause (derived tables)
- In SELECT clause (scalar subqueries)

**By Return Type:**
- Scalar: Returns single value
- Row: Returns single row
- Table: Returns multiple rows and columns

### Scalar Subqueries

Return exactly one value.

```sql
-- Compare to a single value
SELECT name, salary
FROM employees
WHERE salary > (SELECT AVG(salary) FROM employees);

-- Use in SELECT
SELECT 
    name,
    salary,
    (SELECT AVG(salary) FROM employees) AS company_avg,
    salary - (SELECT AVG(salary) FROM employees) AS diff_from_avg
FROM employees;
```

### Row Subqueries

Return a single row with multiple columns.

```sql
-- Match multiple columns
SELECT * FROM products
WHERE (category_id, brand_id) = (
    SELECT category_id, brand_id 
    FROM products 
    WHERE id = 100
);
```

### Table Subqueries

Return multiple rows (used with IN, ANY, ALL, EXISTS).

**IN Operator:**
```sql
-- Find customers who have orders
SELECT * FROM customers
WHERE id IN (SELECT DISTINCT customer_id FROM orders);

-- Find products never ordered
SELECT * FROM products
WHERE id NOT IN (SELECT DISTINCT product_id FROM order_items);
```

**ANY / SOME:**
```sql
-- Salary greater than any manager's salary
SELECT * FROM employees
WHERE salary > ANY (SELECT salary FROM employees WHERE job_title = 'Manager');
```

**ALL:**
```sql
-- Salary greater than all managers' salaries
SELECT * FROM employees
WHERE salary > ALL (SELECT salary FROM employees WHERE job_title = 'Manager');
```

**EXISTS:**
```sql
-- Customers who have at least one order
SELECT * FROM customers c
WHERE EXISTS (
    SELECT 1 FROM orders o WHERE o.customer_id = c.id
);

-- Customers with no orders
SELECT * FROM customers c
WHERE NOT EXISTS (
    SELECT 1 FROM orders o WHERE o.customer_id = c.id
);
```

### Correlated Subqueries

A correlated subquery references the outer query and executes once per outer row.

```sql
-- Employees earning above their department average
SELECT e.name, e.salary, e.department_id
FROM employees e
WHERE e.salary > (
    SELECT AVG(e2.salary)
    FROM employees e2
    WHERE e2.department_id = e.department_id  -- References outer query
);
```

**How it works:**
1. For each row in the outer query
2. Execute the inner query using values from the outer row
3. Compare and include/exclude based on result

**Another Example:**
```sql
-- Most recent order for each customer
SELECT * FROM orders o1
WHERE order_date = (
    SELECT MAX(o2.order_date)
    FROM orders o2
    WHERE o2.customer_id = o1.customer_id
);
```

### Subqueries in FROM (Derived Tables)

Use a subquery as a table in the FROM clause.

```sql
-- Calculate statistics from grouped data
SELECT 
    dept_stats.department,
    dept_stats.emp_count,
    dept_stats.avg_salary
FROM (
    SELECT 
        department,
        COUNT(*) AS emp_count,
        AVG(salary) AS avg_salary
    FROM employees
    GROUP BY department
) AS dept_stats
WHERE dept_stats.emp_count > 5;
```

### Subqueries vs. Joins

Many subqueries can be rewritten as joins:

**Subquery Version:**
```sql
SELECT name FROM customers
WHERE id IN (SELECT customer_id FROM orders WHERE total > 1000);
```

**Join Version:**
```sql
SELECT DISTINCT c.name
FROM customers c
JOIN orders o ON c.id = o.customer_id
WHERE o.total > 1000;
```

**When to use each:**

| Use Subquery | Use Join |
|--------------|----------|
| Need to check existence | Need data from both tables |
| Aggregate comparison | Multiple related records |
| Clearer logic | Better performance (often) |
| NOT IN scenarios | Complex relationships |

### Practical Examples

**Find Second Highest Value:**
```sql
SELECT MAX(salary)
FROM employees
WHERE salary < (SELECT MAX(salary) FROM employees);
```

**Employees with No Subordinates:**
```sql
SELECT * FROM employees e
WHERE NOT EXISTS (
    SELECT 1 FROM employees 
    WHERE manager_id = e.id
);
```

**Products Above Category Average:**
```sql
SELECT p.name, p.price, p.category_id
FROM products p
WHERE p.price > (
    SELECT AVG(p2.price)
    FROM products p2
    WHERE p2.category_id = p.category_id
);
```

**Top N Per Category:**
```sql
-- Top 3 products by price per category
SELECT * FROM products p1
WHERE (
    SELECT COUNT(*)
    FROM products p2
    WHERE p2.category_id = p1.category_id
    AND p2.price > p1.price
) < 3
ORDER BY category_id, price DESC;
```

### Common Table Expressions (CTE) Preview

MySQL 8.0+ supports CTEs for cleaner subqueries:

```sql
WITH high_earners AS (
    SELECT * FROM employees WHERE salary > 100000
)
SELECT department, COUNT(*) 
FROM high_earners
GROUP BY department;
```

CTEs make complex subqueries more readable.

## Summary

- Subqueries are queries nested within other queries
- **Scalar subqueries** return single values for comparisons
- **Table subqueries** return multiple rows (use with IN, EXISTS, ANY, ALL)
- **Correlated subqueries** reference the outer query for row-by-row logic
- **Derived tables** use subqueries in the FROM clause
- Consider joins as an alternative for better performance
- EXISTS is often more efficient than IN for large datasets

## Additional Resources

- [MySQL Subquery Reference](https://dev.mysql.com/doc/refman/8.0/en/subqueries.html)
- [Correlated Subqueries Explained](https://www.mysqltutorial.org/mysql-correlated-subquery/)
- [Subquery vs JOIN Performance](https://use-the-index-luke.com/sql/join/nested-loops-join)
