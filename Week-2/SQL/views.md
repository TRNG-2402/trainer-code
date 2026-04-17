# Views

## Learning Objectives

- Understand what views are and their benefits
- Create and use views effectively
- Know updatable vs. read-only views
- Apply view best practices

## Why This Matters

Views simplify complex queries, provide a security layer, and create stable interfaces for applications. They are virtual tables that encapsulate logic, making databases easier to use and maintain.

## The Concept

### What is a View?

A view is a virtual table based on a SELECT query. It does not store data itself but retrieves data from underlying tables when accessed.

```sql
CREATE VIEW customer_orders AS
SELECT c.name, c.email, o.id AS order_id, o.total
FROM customers c
JOIN orders o ON c.id = o.customer_id;

-- Use it like a table
SELECT * FROM customer_orders WHERE total > 100;
```

### Benefits of Views

| Benefit | Description |
|---------|-------------|
| Simplicity | Hide complex joins and calculations |
| Security | Expose only certain columns/rows |
| Consistency | Provide a stable interface |
| Reusability | Write logic once, use many times |
| Abstraction | Hide underlying table changes |

### Creating Views

**Basic View:**
```sql
CREATE VIEW active_products AS
SELECT id, name, price
FROM products
WHERE is_active = TRUE;
```

**View with Joins:**
```sql
CREATE VIEW order_details AS
SELECT 
    o.id AS order_id,
    o.order_date,
    c.name AS customer_name,
    c.email,
    SUM(oi.quantity * oi.unit_price) AS total
FROM orders o
JOIN customers c ON o.customer_id = c.id
JOIN order_items oi ON o.id = oi.order_id
GROUP BY o.id, o.order_date, c.name, c.email;
```

**View with Calculations:**
```sql
CREATE VIEW employee_summary AS
SELECT 
    department,
    COUNT(*) AS employee_count,
    AVG(salary) AS avg_salary,
    MIN(salary) AS min_salary,
    MAX(salary) AS max_salary
FROM employees
GROUP BY department;
```

### Using Views

```sql
-- Query like a regular table
SELECT * FROM active_products;

-- Filter
SELECT * FROM order_details WHERE order_date > '2024-01-01';

-- Join with other tables
SELECT v.*, d.description
FROM employee_summary v
JOIN departments d ON v.department = d.name;
```

### Modifying Views

**Replace View:**
```sql
CREATE OR REPLACE VIEW active_products AS
SELECT id, name, price, category_id
FROM products
WHERE is_active = TRUE AND stock > 0;
```

**Alter View:**
```sql
ALTER VIEW active_products AS
SELECT id, name, price
FROM products
WHERE is_active = TRUE;
```

**Drop View:**
```sql
DROP VIEW IF EXISTS active_products;
```

### Updatable Views

Some views allow INSERT, UPDATE, and DELETE:

**Updatable View Requirements:**
- No aggregate functions (SUM, COUNT, etc.)
- No GROUP BY or HAVING
- No DISTINCT
- No subqueries in SELECT
- No UNION
- FROM clause references exactly one table

```sql
-- Updatable view
CREATE VIEW us_customers AS
SELECT id, name, email, phone
FROM customers
WHERE country = 'USA';

-- These work:
UPDATE us_customers SET phone = '555-1234' WHERE id = 1;
INSERT INTO us_customers (name, email, country) VALUES ('John', 'john@mail.com', 'USA');
DELETE FROM us_customers WHERE id = 5;
```

### WITH CHECK OPTION

Prevents modifications that would make rows invisible to the view:

```sql
CREATE VIEW us_customers AS
SELECT id, name, email, country
FROM customers
WHERE country = 'USA'
WITH CHECK OPTION;

-- This fails (would make row invisible to view)
UPDATE us_customers SET country = 'Canada' WHERE id = 1;
-- Error: CHECK OPTION failed
```

### Read-Only Views

Views with these features are read-only:

```sql
-- Read-only: has GROUP BY
CREATE VIEW department_stats AS
SELECT department, COUNT(*) as cnt
FROM employees
GROUP BY department;

-- Read-only: has DISTINCT
CREATE VIEW unique_countries AS
SELECT DISTINCT country FROM customers;

-- Read-only: has JOIN
CREATE VIEW order_details AS
SELECT o.id, c.name
FROM orders o
JOIN customers c ON o.customer_id = c.id;
```

### Practical View Examples

**Security Layer:**
```sql
-- Hide sensitive columns
CREATE VIEW public_employees AS
SELECT id, name, department, hire_date
FROM employees;
-- Note: salary and SSN are not exposed
```

**Reporting View:**
```sql
CREATE VIEW monthly_sales AS
SELECT 
    YEAR(order_date) AS year,
    MONTH(order_date) AS month,
    COUNT(*) AS order_count,
    SUM(total) AS revenue
FROM orders
WHERE status = 'completed'
GROUP BY YEAR(order_date), MONTH(order_date)
ORDER BY year, month;
```

**Application Interface:**
```sql
-- Stable interface for applications
CREATE VIEW product_catalog AS
SELECT 
    p.id,
    p.name,
    p.description,
    p.price,
    c.name AS category,
    CASE WHEN p.stock > 0 THEN 'In Stock' ELSE 'Out of Stock' END AS availability
FROM products p
JOIN categories c ON p.category_id = c.id
WHERE p.is_active = TRUE;
```

### Viewing View Definitions

```sql
-- Show view definition
SHOW CREATE VIEW active_products;

-- From information_schema
SELECT * FROM information_schema.VIEWS
WHERE TABLE_NAME = 'active_products';
```

### View Best Practices

1. **Name views descriptively** (vw_monthly_sales, customer_orders)
2. **Document view purpose** in comments
3. **Keep views focused** on specific use cases
4. **Avoid deeply nested views** (views of views)
5. **Consider performance** for complex views
6. **Use WITH CHECK OPTION** for updatable views

## Summary

- Views are virtual tables based on SELECT queries
- They simplify queries, enhance security, and provide abstraction
- Simple views (one table, no aggregates) can be updatable
- WITH CHECK OPTION prevents invisible modifications
- Views with JOINs, GROUP BY, or aggregates are read-only
- Use views to create stable interfaces for applications

## Additional Resources

- [MySQL View Reference](https://dev.mysql.com/doc/refman/8.0/en/views.html)
- [Updatable Views in MySQL](https://dev.mysql.com/doc/refman/8.0/en/view-updatability.html)
- [View Best Practices](https://www.mysqltutorial.org/mysql-views-tutorial.aspx)
