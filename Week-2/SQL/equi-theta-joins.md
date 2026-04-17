# Equi Joins and Theta Joins

## Learning Objectives

- Understand the difference between equi joins and theta joins
- Apply equi joins for equality-based relationships
- Use theta joins for non-equality comparisons
- Recognize when to use each join type

## Why This Matters

While INNER, LEFT, and RIGHT describe which rows to include, equi and theta describe the condition used to match rows. Most joins you write are equi joins, but understanding theta joins expands your ability to solve complex problems like finding overlapping date ranges or hierarchical comparisons.

## The Concept

### What is an Equi Join?

An equi join matches rows based on equality (=) between columns.

```sql
-- This is an equi join (uses = operator)
SELECT customers.name, orders.total
FROM customers
INNER JOIN orders ON customers.id = orders.customer_id;
```

**Key Point:** The join condition uses the equals (=) operator.

### Equi Join Syntax

**Explicit Condition:**
```sql
SELECT e.name, d.department_name
FROM employees e
JOIN departments d ON e.department_id = d.id;
```

**USING Clause (when column names match):**
```sql
-- When both tables have the same column name
SELECT e.name, d.department_name
FROM employees e
JOIN departments d USING (department_id);
```

**Natural Join (automatic matching):**
```sql
-- Automatically joins on columns with same names
SELECT e.name, d.department_name
FROM employees e
NATURAL JOIN departments d;
```

Note: NATURAL JOIN is risky because it may join on unintended columns.

### What is a Theta Join?

A theta join uses any comparison operator (not just equality).

**Theta operators:** `=`, `<`, `>`, `<=`, `>=`, `<>`, `!=`, `BETWEEN`

```sql
-- This is a theta join (uses < operator)
SELECT e1.name AS employee, e2.name AS higher_paid
FROM employees e1
JOIN employees e2 ON e1.salary < e2.salary;
```

### Theta Join Examples

**Finding All Higher-Salaried Employees:**
```sql
SELECT 
    e1.name AS employee,
    e1.salary AS salary,
    e2.name AS higher_paid,
    e2.salary AS higher_salary
FROM employees e1
JOIN employees e2 ON e1.salary < e2.salary
ORDER BY e1.name;
```

**Finding Overlapping Date Ranges:**
```sql
-- Find overlapping bookings
SELECT a.booking_id, b.booking_id
FROM bookings a
JOIN bookings b ON 
    a.booking_id < b.booking_id AND
    a.start_date < b.end_date AND
    a.end_date > b.start_date;
```

**Range Lookups:**
```sql
-- Match orders to discount tiers
CREATE TABLE discount_tiers (
    id INT PRIMARY KEY,
    min_amount DECIMAL(10,2),
    max_amount DECIMAL(10,2),
    discount_percent DECIMAL(5,2)
);

INSERT INTO discount_tiers VALUES
(1, 0, 100, 0),
(2, 100.01, 500, 5),
(3, 500.01, 1000, 10),
(4, 1000.01, 999999, 15);

-- Find discount for each order
SELECT 
    o.id AS order_id,
    o.total,
    d.discount_percent
FROM orders o
JOIN discount_tiers d ON 
    o.total >= d.min_amount AND o.total <= d.max_amount;
```

**Price Comparison:**
```sql
-- Find all products cheaper than each product
SELECT 
    p1.name AS product,
    p1.price,
    p2.name AS cheaper_product,
    p2.price AS cheaper_price
FROM products p1
JOIN products p2 ON p1.price > p2.price
ORDER BY p1.name, p2.price;
```

### Equi Join vs. Theta Join

| Aspect | Equi Join | Theta Join |
|--------|-----------|------------|
| Condition | Equality only (=) | Any comparison |
| Common Use | Matching related records | Range queries, comparisons |
| Frequency | Very common (90%+) | Less common |
| Example | `ON a.id = b.a_id` | `ON a.value > b.threshold` |

### Combining Join Types

You can combine theta conditions with regular joins:

```sql
SELECT 
    c.name,
    o.total,
    (o.total * d.discount_percent / 100) AS discount
FROM customers c
JOIN orders o ON c.id = o.customer_id                    -- Equi join
JOIN discount_tiers d ON 
    o.total >= d.min_amount AND o.total <= d.max_amount; -- Theta join
```

### Non-Equi Join for Hierarchy

```sql
-- Find all subordinates (not just direct reports)
CREATE TABLE org_hierarchy (
    id INT PRIMARY KEY,
    name VARCHAR(100),
    level INT
);

INSERT INTO org_hierarchy VALUES
(1, 'CEO', 1),
(2, 'VP Sales', 2),
(3, 'Manager', 3),
(4, 'Associate', 4);

-- Find all people at higher levels
SELECT 
    e.name AS employee,
    e.level AS employee_level,
    h.name AS superior,
    h.level AS superior_level
FROM org_hierarchy e
JOIN org_hierarchy h ON e.level > h.level
ORDER BY e.name, h.level;
```

### Cross Join as Theta Base

A cross join is a theta join with no condition (or always-true condition):

```sql
-- These are equivalent
SELECT * FROM table_a CROSS JOIN table_b;
SELECT * FROM table_a JOIN table_b ON 1=1;
SELECT * FROM table_a, table_b;
```

Adding a theta condition to a cross join creates a filtered theta join.

### Performance Considerations

Theta joins can be expensive because:
- They may not use indexes effectively
- They can produce large intermediate results
- Range conditions are harder to optimize

**Tips:**
- Add additional WHERE filters when possible
- Consider indexes on comparison columns
- Be aware of result set size

```sql
-- More efficient with additional filters
SELECT a.id, b.id
FROM large_table a
JOIN large_table b ON a.value < b.value
WHERE a.category = 'active'    -- Additional filter
AND b.category = 'active';     -- Reduces comparison set
```

## Summary

- **Equi joins** use equality (=) to match rows; most common type
- **Theta joins** use any comparison operator (<, >, <=, >=, <>)
- Theta joins are useful for range queries, overlapping periods, and hierarchies
- USING clause simplifies equi joins when column names match
- Theta joins can be combined with regular equi joins
- Consider performance implications of non-equality conditions

## Additional Resources

- [Database Join Types Explained](https://www.tutorialspoint.com/dbms/dbms_join_types.htm)
- [Non-Equi Joins in SQL](https://learnsql.com/blog/non-equi-joins-in-sql/)
- [MySQL JOIN Optimization](https://dev.mysql.com/doc/refman/8.0/en/join-optimization.html)
