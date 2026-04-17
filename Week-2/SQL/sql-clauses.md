# SQL Clauses

## Learning Objectives

- Master the WHERE clause for filtering rows
- Apply ORDER BY for sorting results
- Use GROUP BY for aggregation
- Filter groups with HAVING
- Limit results with LIMIT and DISTINCT

## Why This Matters

SQL clauses control what data you get and how it is presented. Mastering these clauses allows you to write precise queries that return exactly the data you need, sorted and filtered as required. These are the building blocks of every SELECT statement.

## The Concept

### The SELECT Statement Structure

```sql
SELECT columns
FROM tables
WHERE row_conditions
GROUP BY grouping_columns
HAVING group_conditions
ORDER BY sort_columns
LIMIT count;
```

Each clause has a specific purpose and must appear in this order.

### WHERE Clause

Filters rows based on conditions.

**Basic Comparisons:**
```sql
SELECT * FROM products WHERE price > 100;
SELECT * FROM products WHERE price = 99.99;
SELECT * FROM products WHERE name = 'Widget';
SELECT * FROM products WHERE price != 50;  -- or <>
```

**Logical Operators:**
```sql
-- AND: both conditions must be true
SELECT * FROM products 
WHERE price > 50 AND stock > 0;

-- OR: either condition can be true
SELECT * FROM products 
WHERE category = 'Electronics' OR category = 'Computers';

-- NOT: negates a condition
SELECT * FROM products WHERE NOT category = 'Obsolete';
```

**Range and List:**
```sql
-- BETWEEN (inclusive)
SELECT * FROM products WHERE price BETWEEN 50 AND 100;

-- IN (matches any value in list)
SELECT * FROM customers WHERE country IN ('USA', 'Canada', 'UK');

-- NOT IN
SELECT * FROM orders WHERE status NOT IN ('cancelled', 'refunded');
```

**Pattern Matching:**
```sql
-- LIKE with wildcards
SELECT * FROM products WHERE name LIKE 'Pro%';      -- Starts with 'Pro'
SELECT * FROM products WHERE name LIKE '%Widget';   -- Ends with 'Widget'
SELECT * FROM products WHERE name LIKE '%ultra%';   -- Contains 'ultra'
SELECT * FROM products WHERE sku LIKE 'A_B%';       -- _ matches single char
```

**NULL Handling:**
```sql
-- Check for NULL
SELECT * FROM customers WHERE phone IS NULL;
SELECT * FROM customers WHERE phone IS NOT NULL;

-- NULL in comparisons (always use IS NULL)
SELECT * FROM customers WHERE phone = NULL;  -- WRONG: always returns empty
```

### ORDER BY Clause

Sorts result rows.

```sql
-- Ascending (default)
SELECT * FROM products ORDER BY price;
SELECT * FROM products ORDER BY price ASC;

-- Descending
SELECT * FROM products ORDER BY price DESC;

-- Multiple columns
SELECT * FROM employees 
ORDER BY department ASC, salary DESC;

-- Order by column position
SELECT name, price FROM products ORDER BY 2 DESC;  -- Order by price

-- Order by expression
SELECT * FROM products ORDER BY price * quantity DESC;
```

**NULL Sorting:**
```sql
-- NULLs sort last in ASC, first in DESC (in MySQL)
SELECT * FROM customers ORDER BY phone ASC;  -- NULLs at end
```

### GROUP BY Clause

Groups rows for aggregation.

```sql
-- Basic grouping
SELECT department, COUNT(*) 
FROM employees 
GROUP BY department;

-- Multiple columns
SELECT department, job_title, AVG(salary)
FROM employees
GROUP BY department, job_title;

-- With expressions
SELECT YEAR(order_date), MONTH(order_date), SUM(total)
FROM orders
GROUP BY YEAR(order_date), MONTH(order_date);
```

**Rule:** Every non-aggregated column in SELECT must appear in GROUP BY.

### HAVING Clause

Filters groups (used with GROUP BY).

```sql
-- Filter groups by count
SELECT department, COUNT(*) AS emp_count
FROM employees
GROUP BY department
HAVING COUNT(*) > 5;

-- Filter by aggregate value
SELECT product_id, AVG(rating) AS avg_rating
FROM reviews
GROUP BY product_id
HAVING AVG(rating) >= 4.0;

-- Combine with WHERE
SELECT department, AVG(salary)
FROM employees
WHERE hire_date > '2020-01-01'  -- Filter rows first
GROUP BY department
HAVING AVG(salary) > 60000;     -- Filter groups after
```

### DISTINCT Keyword

Removes duplicate rows.

```sql
-- Unique values in one column
SELECT DISTINCT country FROM customers;

-- Unique combinations
SELECT DISTINCT country, city FROM customers;

-- With COUNT
SELECT COUNT(DISTINCT country) FROM customers;
```

### LIMIT Clause

Restricts the number of rows returned.

```sql
-- First 10 rows
SELECT * FROM products LIMIT 10;

-- For pagination: LIMIT offset, count
SELECT * FROM products LIMIT 10, 10;  -- Skip 10, return next 10
-- or
SELECT * FROM products LIMIT 10 OFFSET 10;

-- Top N pattern
SELECT * FROM products 
ORDER BY sales DESC 
LIMIT 5;  -- Top 5 best sellers
```

### Combining Clauses

**Complete Example:**
```sql
SELECT 
    c.name AS category,
    COUNT(*) AS product_count,
    AVG(p.price) AS avg_price,
    SUM(p.stock) AS total_stock
FROM products p
JOIN categories c ON p.category_id = c.id
WHERE p.is_active = TRUE                    -- Filter rows
GROUP BY c.id, c.name                        -- Group by category
HAVING COUNT(*) >= 5                         -- At least 5 products
ORDER BY avg_price DESC                      -- Sort by price
LIMIT 10;                                    -- Top 10 only
```

### Clause Execution Order

The logical order in which SQL processes clauses:

1. **FROM/JOIN** - Get data from tables
2. **WHERE** - Filter individual rows
3. **GROUP BY** - Form groups
4. **HAVING** - Filter groups
5. **SELECT** - Choose columns
6. **DISTINCT** - Remove duplicates
7. **ORDER BY** - Sort results
8. **LIMIT** - Restrict output

This is why you cannot use column aliases in WHERE but can in ORDER BY:

```sql
-- WRONG: alias not available in WHERE
SELECT price * quantity AS total 
FROM items 
WHERE total > 100;

-- CORRECT: use the expression or a subquery
SELECT price * quantity AS total 
FROM items 
WHERE price * quantity > 100;

-- CORRECT: alias works in ORDER BY
SELECT price * quantity AS total 
FROM items 
ORDER BY total DESC;
```

### Common Patterns

**Pagination:**
```sql
-- Page 1 (items 1-10)
SELECT * FROM products ORDER BY id LIMIT 10 OFFSET 0;
-- Page 2 (items 11-20)
SELECT * FROM products ORDER BY id LIMIT 10 OFFSET 10;
-- Page N
SELECT * FROM products ORDER BY id LIMIT 10 OFFSET (N-1)*10;
```

**Top N Per Group (Basic):**
```sql
-- Top product per category (requires subquery or window function)
SELECT * FROM products p1
WHERE price = (
    SELECT MAX(price) FROM products p2 
    WHERE p2.category_id = p1.category_id
);
```

## Summary

- **WHERE** filters rows before grouping
- **ORDER BY** sorts results (ASC default, DESC available)
- **GROUP BY** organizes rows for aggregation
- **HAVING** filters groups after aggregation
- **DISTINCT** removes duplicate rows
- **LIMIT** restricts output row count
- Clauses must appear in specific order
- Execution order differs from written order

## Additional Resources

- [MySQL SELECT Syntax](https://dev.mysql.com/doc/refman/8.0/en/select.html)
- [SQL Clauses Tutorial](https://www.w3schools.com/sql/)
- [Order of SQL Operations](https://jvns.ca/blog/2019/10/03/sql-queries-don-t-start-with-select/)
