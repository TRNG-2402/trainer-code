# Aggregate Functions

## Learning Objectives

- Understand the purpose and use of aggregate functions
- Master COUNT, SUM, AVG, MIN, and MAX
- Apply GROUP BY to organize aggregated data
- Use HAVING to filter grouped results

## Why This Matters

Aggregate functions transform many rows into summary values. Instead of seeing every individual sale, you can see total sales, average order value, or maximum transaction. These functions are essential for reporting, analytics, and understanding data at scale.

## The Concept

### What are Aggregate Functions?

Aggregate functions perform calculations across multiple rows and return a single result:

| Function | Purpose |
|----------|---------|
| COUNT() | Count rows |
| SUM() | Total of values |
| AVG() | Average of values |
| MIN() | Smallest value |
| MAX() | Largest value |

### COUNT()

Counts the number of rows or non-NULL values.

```sql
-- Count all rows
SELECT COUNT(*) FROM orders;

-- Count non-NULL values in a column
SELECT COUNT(email) FROM customers;

-- Count distinct values
SELECT COUNT(DISTINCT country) FROM customers;
```

**COUNT(*) vs COUNT(column):**
```sql
-- Sample data with NULL values
| id | email           |
|----|-----------------|
| 1  | alice@mail.com  |
| 2  | NULL            |
| 3  | bob@mail.com    |

SELECT COUNT(*) FROM customers;      -- Returns 3
SELECT COUNT(email) FROM customers;  -- Returns 2 (skips NULL)
```

### SUM()

Calculates the total of numeric values.

```sql
-- Total of all order amounts
SELECT SUM(total) FROM orders;

-- Total sales for a specific customer
SELECT SUM(total) FROM orders WHERE customer_id = 1;
```

### AVG()

Calculates the average of numeric values.

```sql
-- Average order value
SELECT AVG(total) FROM orders;

-- Average salary by department
SELECT AVG(salary) FROM employees WHERE department = 'Engineering';
```

Note: AVG() ignores NULL values.

### MIN() and MAX()

Find the smallest and largest values.

```sql
-- Cheapest and most expensive products
SELECT MIN(price), MAX(price) FROM products;

-- Earliest and latest order dates
SELECT MIN(order_date), MAX(order_date) FROM orders;

-- Also works with strings (alphabetical order)
SELECT MIN(name), MAX(name) FROM customers;
```

### GROUP BY

GROUP BY organizes rows into groups for aggregation.

**Basic Grouping:**
```sql
-- Total orders per customer
SELECT customer_id, COUNT(*) AS order_count
FROM orders
GROUP BY customer_id;

-- Result:
-- +-------------+-------------+
-- | customer_id | order_count |
-- +-------------+-------------+
-- | 1           | 5           |
-- | 2           | 3           |
-- | 3           | 7           |
-- +-------------+-------------+
```

**Multiple Aggregates:**
```sql
SELECT 
    customer_id,
    COUNT(*) AS order_count,
    SUM(total) AS total_spent,
    AVG(total) AS avg_order,
    MIN(order_date) AS first_order,
    MAX(order_date) AS last_order
FROM orders
GROUP BY customer_id;
```

**Group by Multiple Columns:**
```sql
-- Totals by year and month
SELECT 
    YEAR(order_date) AS year,
    MONTH(order_date) AS month,
    SUM(total) AS monthly_total
FROM orders
GROUP BY YEAR(order_date), MONTH(order_date)
ORDER BY year, month;
```

### HAVING Clause

HAVING filters grouped results (WHERE filters before grouping).

```sql
-- Customers with more than 5 orders
SELECT customer_id, COUNT(*) AS order_count
FROM orders
GROUP BY customer_id
HAVING COUNT(*) > 5;

-- Products with average rating below 3
SELECT product_id, AVG(rating) AS avg_rating
FROM reviews
GROUP BY product_id
HAVING AVG(rating) < 3;
```

**WHERE vs HAVING:**

| Clause | Filters | Applied |
|--------|---------|---------|
| WHERE | Individual rows | Before grouping |
| HAVING | Groups | After grouping |

```sql
-- WHERE filters rows, then HAVING filters groups
SELECT department, AVG(salary) AS avg_salary
FROM employees
WHERE hire_date > '2020-01-01'   -- Filter: only recent hires
GROUP BY department
HAVING AVG(salary) > 50000;       -- Filter: only high-paying departments
```

### Complete Example

```sql
-- Sales report by category
SELECT 
    c.name AS category,
    COUNT(DISTINCT o.id) AS total_orders,
    COUNT(oi.id) AS items_sold,
    SUM(oi.quantity * oi.unit_price) AS total_revenue,
    AVG(oi.unit_price) AS avg_item_price,
    MIN(o.order_date) AS first_sale,
    MAX(o.order_date) AS last_sale
FROM categories c
JOIN products p ON c.id = p.category_id
JOIN order_items oi ON p.id = oi.product_id
JOIN orders o ON oi.order_id = o.id
WHERE o.status = 'completed'
GROUP BY c.id, c.name
HAVING SUM(oi.quantity * oi.unit_price) > 1000
ORDER BY total_revenue DESC;
```

### Common Patterns

**Top N per Group:**
```sql
-- Top 3 products by sales
SELECT product_id, SUM(quantity) AS units_sold
FROM order_items
GROUP BY product_id
ORDER BY units_sold DESC
LIMIT 3;
```

**Percentage of Total:**
```sql
SELECT 
    department,
    SUM(salary) AS dept_total,
    SUM(salary) / (SELECT SUM(salary) FROM employees) * 100 AS percent_of_total
FROM employees
GROUP BY department;
```

**Running Totals (Preview):**
We will cover window functions for running totals in advanced topics.

### GROUP BY Rules

1. Every non-aggregated column in SELECT must be in GROUP BY
2. You can GROUP BY columns not in SELECT
3. Use column aliases in ORDER BY, not in HAVING

```sql
-- Valid
SELECT department, COUNT(*) 
FROM employees 
GROUP BY department;

-- Invalid (name not in GROUP BY)
SELECT department, name, COUNT(*)
FROM employees
GROUP BY department;
```

## Summary

- **COUNT()** counts rows or non-NULL values
- **SUM()** totals numeric values
- **AVG()** calculates averages (ignores NULL)
- **MIN()/MAX()** find smallest/largest values
- **GROUP BY** organizes rows into groups for aggregation
- **HAVING** filters groups (vs WHERE which filters rows)
- All non-aggregated SELECT columns must appear in GROUP BY

## Additional Resources

- [MySQL Aggregate Functions](https://dev.mysql.com/doc/refman/8.0/en/aggregate-functions.html)
- [SQL GROUP BY Tutorial](https://www.w3schools.com/sql/sql_groupby.asp)
- [Aggregate Functions Guide](https://www.mysqltutorial.org/mysql-aggregate-functions/)
